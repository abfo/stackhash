using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Data.SqlClient;


using StackHashBusinessObjects;
using StackHashSqlControl;
using StackHashUtilities;

namespace StackHashErrorIndex
{
    [SuppressMessage("Microsoft.Performance", "CA1812")]
    public class SqlCommands : IDisposable
    {
        #region Fields

        private DbProviderFactory m_ProviderFactory;
        private SqlUtils m_SqlUtils;
        private String m_ConnectionString;      // Connection string for database.
        private String m_MasterConnectionString; // Connection string for master database.
        private int m_ConnectionRetryLimit;
        private SqlEventTypeCache m_EventTypeCache;
        private SqlOperatingSystemCache m_OperatingSystemCache;
        private SqlLocaleCache m_LocaleCache;

        #endregion Fields

        #region AllSql

        #region ControlSql

        // ******************************************************************************************
        // CONTROL SQL.
        // ******************************************************************************************
        // The control data is set when the index is created or upgraded.

        private const String s_AddControlSql =
            "INSERT INTO dbo.Control " +
            "(ControlItemId, DatabaseVersion, SyncCount, SyncProductId, SyncFileId, SyncEventId, SyncCabId, SyncEventTypeName, SyncPhase) VALUES " +
            "(@ControlItemId, @DatabaseVersion, @SyncCount, @SyncProductId, @SyncFileId, @SyncEventId, @SyncCabId, @SyncEventTypeName, @SyncPhase);";

        private const String s_UpdateControlSql =
            "UPDATE dbo.Control " +
            "SET DatabaseVersion=@DatabaseVersion, SyncCount=@SyncCount, SyncProductId=@SyncProductId,  SyncFileId=@SyncFileId, " +
            "    SyncEventId=@SyncEventId, SyncCabId=@SyncCabId, SyncEventTypeName=@SyncEventTypeName, SyncPhase=@SyncPhase " +
            "WHERE ControlItemId=@ControlItemId;";

        private const String s_GetControlSql =
            @"SELECT * FROM dbo.Control WHERE ControlItemId=@ControlItemId;";

        private const String s_ControlExistsSql =
            @"IF EXISTS(SELECT * FROM dbo.Control WHERE ControlItemId=@ControlItemId) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        #endregion ControlSql

        #region ProductControlSql

        // ******************************************************************************************
        // PRODUCT CONTROL SQL.
        // ******************************************************************************************

        private const String s_AddProductControlSql =
            "INSERT INTO dbo.ProductControl " +
            "(ProductId, LastSyncTime, LastSyncCompletedTime, LastSyncStartedTime, LastHitTime, SyncEnabled) VALUES " +
            "(@ProductId, @LastSyncTime, @LastSyncCompletedTime, @LastSyncStartedTime, @LastHitTime, @SyncEnabled);";

        private const String s_UpdateProductControlSql =
            "UPDATE dbo.ProductControl " +
            "SET LastSyncTime=@LastSyncTime, LastSyncCompletedTime=@LastSyncCompletedTime, LastSyncStartedTime=@LastSyncStartedTime, " +
            "    LastHitTime=@LastHitTime, SyncEnabled=@SyncEnabled " +
            "WHERE ProductId=@ProductId;";

        private const String s_GetProductControlSql =
            @"SELECT * FROM dbo.ProductControl WHERE ProductId=@ProductId;";

        private const String s_ProductControlExistsSql =
            @"IF EXISTS(SELECT * FROM dbo.ProductControl WHERE ProductId=@ProductId) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        #endregion ProductControlSql

        #region TaskControlSql

        // ******************************************************************************************
        // TASK CONTROL SQL.
        // ******************************************************************************************

        private const String s_AddTaskControlSql =
            "INSERT INTO dbo.TaskControl " +
            "(TaskType, TaskState, LastSuccessfulRunTimeUtc, LastFailedRunTimeUtc, LastDurationInSeconds, LastStartedTimeUtc, RunCount, SuccessCount, FailedCount, LastTaskException, ServiceErrorCode) " +
            "VALUES " +
            "(@TaskType, @TaskState, @LastSuccessfulRunTimeUtc, @LastFailedRunTimeUtc, @LastDurationInSeconds, @LastStartedTimeUtc, @RunCount, @SuccessCount, @FailedCount, @LastTaskException, @ServiceErrorCode);";

        private const String s_UpdateTaskControlSql =
            "UPDATE dbo.TaskControl " +
            "SET TaskType=@TaskType, TaskState=@TaskState, LastSuccessfulRunTimeUtc=@LastSuccessfulRunTimeUtc, LastFailedRunTimeUtc=@LastFailedRunTimeUtc, " +
            "    LastDurationInSeconds=@LastDurationInSeconds, LastStartedTimeUtc=@LastStartedTimeUtc,  RunCount=@RunCount, SuccessCount=@SuccessCount, FailedCount=@FailedCount, LastTaskException=@LastTaskException, ServiceErrorCode=@ServiceErrorCode " +
            "WHERE TaskType=@TaskType;";

        private const String s_GetTaskControlSql =
            @"SELECT * FROM dbo.TaskControl WHERE TaskType=@TaskType;";

        private const String s_TaskControlExistsSql =
            @"IF EXISTS(SELECT * FROM dbo.TaskControl WHERE TaskType=@TaskType) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        #endregion TaskControlSql

        #region ProductSql

        // ******************************************************************************************
        // PRODUCT SQL.
        // ******************************************************************************************

        private const String s_AddProductSql = 
		    "INSERT INTO dbo.Products " +
            "    (ProductId, DateCreatedLocal, DateModifiedLocal, ProductName, TotalEvents, " +
			"     TotalResponses, Version, TotalStoredEvents) " +
            "VALUES " +
            "    (@ProductId, @DateCreatedLocal, @DateModifiedLocal, @ProductName, @TotalEvents, " + 
            "     @TotalResponses, @Version, @TotalStoredEvents); ";

        private const String s_UpdateProductSqlAll =
            "UPDATE dbo.Products " +
            "SET ProductId=@ProductId, DateCreatedLocal=@DateCreatedLocal, DateModifiedLocal=@DateModifiedLocal, " +
            "    ProductName=@ProductName, TotalEvents=@TotalEvents, TotalResponses=@TotalResponses, Version= @Version, TotalStoredEvents=@TotalStoredEvents " +
            "WHERE ProductId=@ProductId;";

        private const String s_UpdateProductSqlJustWinQual =
            "UPDATE dbo.Products " +
            "SET ProductId=@ProductId, DateCreatedLocal=@DateCreatedLocal, DateModifiedLocal=@DateModifiedLocal, " +
            "    ProductName=@ProductName, TotalEvents=@TotalEvents, TotalResponses=@TotalResponses, Version= @Version " +
            "WHERE ProductId=@ProductId;";

        private const String s_GetProductsSql =
            @"SELECT * FROM dbo.Products;";

        private const String s_GetProductSql =
            @"SELECT * FROM dbo.Products WHERE ProductId=@ProductId;";

        private const String s_ProductExistsSql =
            @"IF EXISTS (SELECT * FROM dbo.Products AS P WHERE P.ProductId=@ProductId) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_ProductExistsByCountSql =
            "SELECT COUNT(*) AS ProductCount " + 
            "FROM dbo.Products AS P " + 
            "WHERE P.ProductId=@ProductId;";


        private const String s_ProductEventCountSql =
            "WITH ALLEVENTIDS AS " +
            " (SELECT DISTINCT FE.EventId, FE.EventTypeId " +
            "  FROM dbo.ProductFiles AS PF " +
            "  INNER JOIN dbo.FileEvents AS FE " +
            "  ON PF.FileId=FE.FileId AND PF.ProductId=@ProductId) " +
            " SELECT COUNT(*) AS NumberOfEvents " +
            " FROM ALLEVENTIDS ";

        private const String s_ProductsEventCountSql =
            "WITH ALLEVENTIDS AS " +
            " (SELECT DISTINCT FE.EventId, FE.EventTypeId " +
            "  FROM dbo.ProductFiles AS PF " +
            "  INNER JOIN dbo.FileEvents AS FE " +
            "  ON PF.FileId=FE.FileId AND PF.ProductId in ({0})) " +
            " SELECT COUNT(*) AS NumberOfEvents " +
            " FROM ALLEVENTIDS ";
        
        
        private const String s_GetProductCountSql =
            "SELECT Count(*) AS ProductCount " +
            "FROM dbo.Products;";


        private const String s_GetProductMatchSql =
            "SELECT P.ProductId " +
            "FROM dbo.Products AS P " +
            "WHERE {0} ";  // Add the condition.

        private const String s_GetProductMatchNoConditionSql =
            "SELECT P.ProductId " +
            "FROM dbo.Products AS P; "; 

        #endregion ProductControlSql

        #region FileSql

        // ******************************************************************************************
        // FILE SQL.
        // ******************************************************************************************

        private const String s_AddFileSql =
            "INSERT INTO dbo.Files " +
            "    (FileId, DateCreatedLocal, DateModifiedLocal, FileName, Version, LinkDateLocal) " +
            "VALUES " +
            "    (@FileId, @DateCreatedLocal, @DateModifiedLocal, @FileName, @Version, @LinkDateLocal);";

        private const String s_UpdateFileSqlAll =
            "UPDATE dbo.Files " +
            "SET DateCreatedLocal=@DateCreatedLocal, DateModifiedLocal=@DateModifiedLocal, " +
            "    FileName=@FileName, Version=@Version, LinkDateLocal=@LinkDateLocal " +
            "WHERE FileId=@FileId;";

        private static String s_GetFilesSql =
            "SELECT * " +
            "FROM dbo.Files AS F  " +
            "INNER JOIN dbo.ProductFiles AS PF " +
            "ON F.FileID=PF.FileID AND PF.ProductId=@ProductId;";

        private const String s_GetFileSql =
            "SELECT * FROM dbo.Files WHERE FileId=@FileId;";

        private const String s_FileExistsSql =
            "IF EXISTS(SELECT * FROM dbo.Files WHERE FileId=@FileId) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_ProductFileExistsSql =
            "IF EXISTS(SELECT * FROM dbo.ProductFiles WHERE ProductId=@ProductId AND FileId=@FileId) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_AddProductFileSql =
            "INSERT INTO dbo.ProductFiles " +
            "    (ProductId, FileId) " +
            "VALUES " +
            "    (@ProductId, @FileId);";

        private const String s_GetFileCountSql =
            "SELECT Count(*) AS FileCount " +
            "FROM dbo.Files;";

        private const String s_GetFilesMatchSql =
            "SELECT PF.ProductId, PF.FileId " +
            "FROM dbo.ProductFiles AS PF " +
            "INNER JOIN dbo.Files AS F " +
            "ON PF.FileId=F.FileId "; // This command is extended.

        private const String s_GetFilesMatchWithProductSetSql =
            "SELECT PF.ProductId, PF.FileId " +
            "FROM dbo.ProductFiles AS PF " +
            "INNER JOIN dbo.Files AS F " +
            "ON PF.FileId=F.FileId AND PF.ProductId IN {0} "; // This command is extended.

        #endregion FileSql

        #region EventsSql

        // ******************************************************************************************
        // EVENTS SQL.
        // ******************************************************************************************

        private const String s_AddEventSql =
            "INSERT INTO dbo.Events " +
            "    (EventId, EventTypeId, DateCreatedLocal, DateModifiedLocal, TotalHits, BugId, PlugInBugId, WorkFlowStatus, " +
            "     ApplicationName, ApplicationVersion, ApplicationTimeStamp, ModuleName, ModuleVersion, ModuleTimeStamp, Offset, ExceptionCode, " + 
            "     OffsetOriginal, ExceptionCodeOriginal) " +
            "VALUES " +
            "    (@EventId, @EventTypeId, @DateCreatedLocal, @DateModifiedLocal, @TotalHits, @BugId, @PlugInBugId, @WorkFlowStatus, " +
            "     @ApplicationName, @ApplicationVersion, @ApplicationTimeStamp, @ModuleName, @ModuleVersion, @ModuleTimeStamp, @Offset, @ExceptionCode, " +
            "     @OffsetOriginal, @ExceptionCodeOriginal); ";

        private const String s_UpdateEventSqlAll =
            "UPDATE dbo.Events " +
            "SET EventId=@EventId, EventTypeId=@EventTypeId, DateCreatedLocal=@DateCreatedLocal, DateModifiedLocal=@DateModifiedLocal, " +
            "    TotalHits=@TotalHits, BugId=@BugId, PlugInBugId=@PlugInBugId, WorkFlowStatus=@WorkFlowStatus, ApplicationName=@ApplicationName, ApplicationVersion=@ApplicationVersion, " +
            "    ApplicationTimeStamp=@ApplicationTimeStamp, ModuleName=@ModuleName, ModuleVersion=@ModuleVersion, " +
            "    ModuleTimeStamp=@ModuleTimeStamp, Offset=@Offset, ExceptionCode=@ExceptionCode, OffsetOriginal=@OffsetOriginal, " +
            "    ExceptionCodeOriginal=@ExceptionCodeOriginal " +
            "WHERE EventId=@EventId AND EventTypeId=@EventTypeId;";

        private const String s_UpdateEventSqlJustWinQual =
            "UPDATE dbo.Events " +
            "SET EventId=@EventId, EventTypeId=@EventTypeId, DateCreatedLocal=@DateCreatedLocal, DateModifiedLocal=@DateModifiedLocal, " +
            "    ApplicationName=@ApplicationName, ApplicationVersion=@ApplicationVersion, ApplicationTimeStamp=@ApplicationTimeStamp, ModuleName=@ModuleName, ModuleVersion=@ModuleVersion, " +
            "    ModuleTimeStamp=@ModuleTimeStamp, Offset=@Offset, ExceptionCode=@ExceptionCode, OffsetOriginal=@OffsetOriginal, ExceptionCodeOriginal=@ExceptionCodeOriginal, TotalHits=@TotalHits " +
            "WHERE EventId=@EventId AND EventTypeId=@EventTypeId;";

        private const String s_EventExistsSql =
            @"if exists(select * from dbo.Events where EventId=@EventId AND EventTypeId=@EventTypeId) select 1 as extant else select 0 as extant;";

        private const String s_GetEventSql =
            "SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, " +
            "       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName, " +
            "       E.BugId, E.PlugInBugId, E.WorkFlowStatus, WF.WorkFlowStatusName, CABCOUNTER.CabCount " +
            "FROM dbo.Events AS E " +
            "INNER JOIN dbo.EventTypes AS ET " +
            "ON E.EventTypeId=ET.EventTypeId AND E.EventId=@EventId AND E.EventTypeId=@EventTypeId " +
            "INNER JOIN dbo.WorkFlowMappings AS WF " +
            "ON WF.WorkFlowStatusId=E.WorkFlowStatus " +
            "LEFT JOIN (" +
            "    SELECT EventId, EventTypeId, Count(*) AS CabCount " +
            "    FROM Cabs AS CC " +
            "    GROUP BY CC.EventId, CC.EventTypeId " +
            ") AS CABCOUNTER ON E.EventId=CABCOUNTER.EventId AND E.EventTypeId=CABCOUNTER.EventTypeId; ";

        private const String s_LoadEventsSql =
            "SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, " +
            "       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName, " +
            "       E.BugId, E.PlugInBugId, E.WorkFlowStatus, WF.WorkFlowStatusName, CABCOUNTER.CabCount " +
            "FROM dbo.Events E " +
            "INNER JOIN dbo.EventTypes ET " +
            "   ON ET.EventTypeId=E.EventTypeId " +
            "INNER JOIN dbo.WorkFlowMappings WF " +
            "   ON WF.WorkFlowStatusId=E.WorkFlowStatus " +
            "INNER JOIN dbo.FileEvents FE " +
            "   ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId " +
            "LEFT JOIN (" +
            "    SELECT EventId, EventTypeId, Count(*) AS CabCount " +
            "    FROM Cabs AS CC " +
            "    GROUP BY CC.EventId, CC.EventTypeId " +
            ") AS CABCOUNTER ON E.EventId=CABCOUNTER.EventId AND E.EventTypeId=CABCOUNTER.EventTypeId " +
            "WHERE FE.FileId=@FileId;"; 

        //private const String s_GetProductEventsSql =
        //    "SELECT EV.EventId, EV.DateCreatedLocal, EV.DateModifiedLocal, EV.ApplicationName, EV.ApplicationVersion, EV.ApplicationTimeStamp, EV.TotalHits, " +
        //    "       EV.ModuleName, EV.ModuleVersion, EV.ModuleTimeStamp, EV.Offset, EV.ExceptionCode, EV.OffsetOriginal, EV.ExceptionCodeOriginal, EV.EventTypeName, " +
        //    "       EV.BugId " +
        //    "FROM dbo.FileEvents_Vw EV INNER JOIN ProductFiles PF " +
        //    "ON PF.ProductId=@ProductId ";

        private const String s_GetProductEventsSql =
            "SELECT DISTINCT E.EventId, E.EventTypeId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, " +
            "       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName, " +
            "       E.BugId, E.PlugInBugId, PF.FileId, E.WorkFlowStatus, WF.WorkFlowStatusName, CABCOUNTER.CabCount " +
            "FROM dbo.ProductFiles AS PF " +
            "INNER JOIN dbo.FileEvents AS FE " +
            "ON PF.ProductId=@ProductId AND PF.FileId=FE.FileId " +
            "INNER JOIN dbo.Events AS E " +
            "ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId " +
            "INNER JOIN dbo.EventTypes AS ET " +
            "ON FE.EventTypeId=ET.EventTypeId " +
            "INNER JOIN dbo.WorkFlowMappings WF " +
            "ON WF.WorkFlowStatusId=E.WorkFlowStatus " +
            "LEFT JOIN (" +
            "    SELECT EventId, EventTypeId, Count(*) AS CabCount " +
            "    FROM Cabs AS CC " +
            "    GROUP BY CC.EventId, CC.EventTypeId " +
            ") AS CABCOUNTER ON E.EventId=CABCOUNTER.EventId AND E.EventTypeId=CABCOUNTER.EventTypeId " +
            "ORDER BY E.EventId, E.EventTypeId; ";

        
        private const String s_GetEventCountSql =
            "SELECT Count(*) AS EventCount " +
            "FROM dbo.Events;";

        #endregion EventsSql

        #region EventNotesSql

        // ******************************************************************************************
        // EVENT NOTES SQL.
        // ******************************************************************************************

        private const String s_AddEventNoteSql =
            "DECLARE @newKey AS INT; " +
            "INSERT INTO dbo.EventNotes " +
            "    (EventId, EventTypeId, TimeOfEntry, SourceId, UserId, Note) " +
            "VALUES " +
            "    (@EventId, @EventTypeId, @TimeOfEntry, @SourceId, @UserId, @Note);" +
            "SET @newKey = SCOPE_IDENTITY(); " +
            "SELECT @newKey AS NewKey;";

        private const String s_UpdateEventNoteSql =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.EventNotes AS EN WHERE EN.NoteId=@NoteId) " +
            "  UPDATE dbo.EventNotes " +
            "  SET EventId=@EventId, EventTypeId=@EventTypeId, TimeOfEntry=@TimeOfEntry, SourceId=@SourceId, UserId=@UserId, Note=@Note " +
            "  WHERE NoteId=@NoteId; ";

        
        private const String s_GetEventNotesSql =
            "SELECT EN.TimeOfEntry, S.Source, U.StackHashUser, EN.Note, EN.NoteId " +
            "FROM dbo.EventNotes AS EN " +
            "INNER JOIN dbo.Users AS U ON EN.UserId=U.UserId " +
            "INNER JOIN dbo.Sources AS S ON EN.SourceId=S.SourceId " +
            " AND EN.EventId=@EventId AND EN.EventTypeId=@EventTypeId " +
            "ORDER BY EN.NoteId;";

        private const String s_GetEventNoteSql =
            "SELECT EN.TimeOfEntry, S.Source, U.StackHashUser, EN.Note " +
            "FROM dbo.EventNotes AS EN " +
            "INNER JOIN dbo.Users AS U ON EN.UserId=U.UserId " +
            "INNER JOIN dbo.Sources AS S ON EN.SourceId=S.SourceId " +
            "WHERE EN.NoteId=@NoteId ";

        private const String s_DeleteEventNoteSql =
            "DELETE " +
            "  FROM dbo.EventNotes " +
            "  WHERE EventId=@EventId AND EventTypeId=@EventTypeId AND NoteId=@NoteID; ";

        #endregion EventNotesSql

        #region EventTypesSql

        // ******************************************************************************************
        // EVENT TYPES SQL.
        // ******************************************************************************************
        private const String s_GetEventTypeIdSql =
            "SELECT * FROM dbo.EventTypes WHERE EventTypeName=@EventTypeName;";

        private const String s_GetEventTypeSql =
            "SELECT * FROM dbo.EventTypes WHERE EventTypeId=@EventTypeId;";

        private const String s_EventTypeExistsSql =
            "IF EXISTS(SELECT * FROM dbo.EventTypes WHERE EventTypeName=@EventTypeName) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_AddEventTypeSql =
            "INSERT INTO dbo.EventTypes " +
            "    (EventTypeName) " +
            "VALUES " +
            "    (@EventTypeName); ";

        #endregion EventTypesSql

        #region EventInfoSql

        // ******************************************************************************************
        // EVENT INFO (HITS) SQL.
        // ******************************************************************************************

        private const String s_AddEventInfoSql =
            "INSERT into dbo.EventInfos " + 
            "    (DateCreatedLocal, DateModifiedLocal, HitDateLocal, LocaleId, TotalHits, EventId, EventTypeId, OperatingSystemId) " +
            "VALUES " +
            "    (@DateCreatedLocal, @DateModifiedLocal, @HitDateLocal, @LocaleId, @TotalHits, @EventId, @EventTypeId, @OperatingSystemId); ";

        private const String s_AddEventInfosSql =
            "INSERT into dbo.EventInfos " +
            "    (DateCreatedLocal, DateModifiedLocal, HitDateLocal, LocaleId, TotalHits, EventId, EventTypeId, OperatingSystemId) " +
            "VALUES " +
            "    (@DateCreatedLocal{0}, @DateModifiedLocal{0}, @HitDateLocal{0}, @LocaleId{0}, @TotalHits{0}, @EventId{0}, @EventTypeId{0}, @OperatingSystemId{0}); ";

        private const String s_GetEventInfosSql =
            "SELECT EI.DateCreatedLocal, EI.DateModifiedLocal, EI.HitDateLocal, EI.LocaleId, L.LocaleCode, L.LocaleName, EI.TotalHits, ET.EventTypeName, OS.OperatingSystemName, OS.OperatingSystemVersion " +
            "FROM dbo.EventInfos AS EI " +
            "INNER JOIN dbo.EventTypes AS ET ON EI.EventTypeId=ET.EventTypeId AND EI.EventId=@EventId AND EI.EventTypeId=@EventTypeId " +
            "INNER JOIN dbo.OperatingSystems AS OS ON EI.OperatingSystemId=OS.OperatingSystemId " +
            "INNER JOIN dbo.Locales AS L ON EI.LocaleId=L.LocaleId " +
            "ORDER BY EI.HitDateLocal, EI.LocaleId, OS.OperatingSystemId; ";

        private const String s_GetHitCountSql =
            "SELECT Sum(EI.TotalHits) AS TotalHits " +
            "FROM dbo.EventInfos AS EI " +
            "WHERE EI.EventId=@EventId AND EI.EventTypeId=@EventTypeId;";

        private const String s_GetMostRecentHitDateSql =
            "SELECT Max(EI.HitDateLocal) AS MostRecentHitDate " +
            "FROM dbo.EventInfos AS EI " +
            "WHERE EI.EventId=@EventId AND EI.EventTypeId=@EventTypeId;";

        #endregion EventInfoSql

        #region OsTypesSql

        // ******************************************************************************************
        // OS TYPES SQL.
        // ******************************************************************************************
        private const String s_GetOperatingSystemIdSql =
            "SELECT * FROM dbo.OperatingSystems " +
            " WHERE ((OperatingSystemName=@OperatingSystemName OR (OperatingSystemName IS NULL AND @OperatingSystemName IS NULL)) AND " +
            "  (OperatingSystemVersion=@OperatingSystemVersion OR (OperatingSystemVersion IS NULL AND @OperatingSystemVersion IS NULL)));";

        private const String s_GetOperatingSystemTypeSql =
            "SELECT * FROM dbo.OperatingSystems WHERE OperatingSystemId=@OperatingSystemId;";

        private const String s_OperatingSystemExistsSql =
            "IF EXISTS(SELECT * FROM dbo.OperatingSystems " +
            " WHERE ((OperatingSystemName=@OperatingSystemName OR (OperatingSystemName IS NULL AND @OperatingSystemName IS NULL)) AND " +
            "  (OperatingSystemVersion=@OperatingSystemVersion OR (OperatingSystemVersion IS NULL AND @OperatingSystemVersion IS NULL)))) select 1 as extant else select 0 as extant;";

        private const String s_AddOperatingSystemSql =
            "INSERT INTO dbo.OperatingSystems " +
            "    (OperatingSystemName, OperatingSystemVersion) " +
            "VALUES " +
            "    (@OperatingSystemName, @OperatingSystemVersion); ";

        #endregion OsTypesSql

        #region LocalesSql

        // ******************************************************************************************
        // LOCALE SQL.
        // ******************************************************************************************
        private const String s_GetLocaleSql =
            "SELECT * FROM dbo.Locales WHERE LocaleId=@LocaleId;";

        private const String s_GetLocalesSql =
            "SELECT * FROM dbo.Locales;";

        private const String s_LocaleExistsSql =
            "IF EXISTS(SELECT * FROM dbo.Locales WHERE LocaleId=@LocaleId) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_AddLocaleSql =
            "INSERT INTO dbo.Locales " +
            "    (LocaleId, LocaleCode, LocaleName) " +
            "VALUES " +
            "    (@LocaleId, @LocaleCode, @LocaleName); ";

        #endregion LocalesSql

        #region LocaleSummarySql

        // ******************************************************************************************
        // LOCALE SUMMARY SQL.
        // ******************************************************************************************
        private const String s_GetLocaleSummarySql =
            "SELECT L.LocaleId, L.LocaleName, L.LocaleCode, LS.ProductId, LS.TotalHits " + 
            "FROM dbo.LocaleSummary AS LS " + 
            "INNER JOIN dbo.Locales AS L ON LS.LocaleId=L.LocaleId " +
            "WHERE LS.LocaleId=@LocaleId AND LS.ProductId=@ProductId;";

        private const String s_GetAllLocaleSummariesSql =
            "SELECT L.LocaleId, L.LocaleName, L.LocaleCode, LS.ProductId, LS.TotalHits " +
            "FROM dbo.LocaleSummary AS LS " +
            "INNER JOIN dbo.Locales AS L ON LS.LocaleId=L.LocaleId " +
            "WHERE LS.ProductId=@ProductId;";

        private const String s_LocaleSummaryExistsSql =
            "IF EXISTS(SELECT * FROM dbo.LocaleSummary WHERE LocaleId=@LocaleId AND ProductId=@ProductId) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_AddLocaleSummarySql =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.LocaleSummary AS LS WHERE LS.LocaleId=@LocaleId AND LS.ProductId=@ProductId) " +
            "  UPDATE dbo.LocaleSummary " +
            "  SET LocaleId=@LocaleId, ProductId=@ProductId, TotalHits=TotalHits + @TotalHits " +
            "  WHERE ProductId=@ProductId AND LocaleId=@LocaleId " +
            "ELSE " +
            "  INSERT INTO dbo.LocaleSummary " +
            "    (LocaleId, ProductId, TotalHits) " +
            "  VALUES " +
            "    (@LocaleId, @ProductId, @TotalHits); ";

        private const String s_AddLocaleSummaryOverwriteSql =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.LocaleSummary AS LS WHERE LS.LocaleId=@LocaleId AND LS.ProductId=@ProductId) " +
            "  UPDATE dbo.LocaleSummary " +
            "  SET LocaleId=@LocaleId, ProductId=@ProductId, TotalHits=@TotalHits " +
            "  WHERE ProductId=@ProductId AND LocaleId=@LocaleId " +
            "ELSE " +
            "  INSERT INTO dbo.LocaleSummary " +
            "    (LocaleId, ProductId, TotalHits) " +
            "  VALUES " +
            "    (@LocaleId, @ProductId, @TotalHits); ";

        #endregion LocalesSql

        #region OperatingSystemSummarySql

        // ******************************************************************************************
        // OPERATING SYSTEM SUMMARY SQL.
        // ******************************************************************************************
        private const String s_GetOperatingSystemSummarySql =
            "SELECT O.OperatingSystemName, O.OperatingSystemVersion, OS.ProductId, OS.TotalHits " + 
            "FROM dbo.OperatingSystemSummary AS OS " +
            "INNER JOIN dbo.OperatingSystems AS O ON OS.OperatingSystemId=O.OperatingSystemId " +
            "WHERE (O.OperatingSystemName=@OperatingSystemName OR (O.OperatingSystemName IS NULL AND @OperatingSystemName IS NULL)) AND " + 
            "      (O.OperatingSystemVersion=@OperatingSystemVersion OR (O.OperatingSystemVersion IS NULL AND @OperatingSystemVersion IS NULL)) " + 
            "      AND OS.ProductId=@ProductId;";

        private const String s_GetAllOperatingSystemSummariesSql =
            "SELECT O.OperatingSystemName, O.OperatingSystemVersion, OS.ProductId, OS.TotalHits " + 
            "FROM dbo.OperatingSystemSummary AS OS " +
            "INNER JOIN dbo.OperatingSystems AS O ON OS.OperatingSystemId=O.OperatingSystemId " +
            "WHERE OS.ProductId=@ProductId;";

        private const String s_OperatingSystemSummaryExistsSql =
            "IF EXISTS( " + 
            "  SELECT * " + 
            "  FROM dbo.OperatingSystemSummary OS " + 
            "  INNER JOIN dbo.OperatingSystems O ON OS.OperatingSystemId=O.OperatingSystemId " +
            "WHERE (O.OperatingSystemName=@OperatingSystemName OR (O.OperatingSystemName IS NULL AND @OperatingSystemName IS NULL)) AND " + 
            "      (O.OperatingSystemVersion=@OperatingSystemVersion OR (O.OperatingSystemVersion IS NULL AND @OperatingSystemVersion IS NULL)) " + 
            "      AND OS.ProductId=@ProductId " +
            " ) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_AddOperatingSystemSummarySql =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.OperatingSystemSummary AS OSS WHERE OSS.OperatingSystemId=@OperatingSystemId AND OSS.ProductId=@ProductId) " +
            "  UPDATE dbo.OperatingSystemSummary " +
            "  SET OperatingSystemId=@OperatingSystemId, ProductId=@ProductId, TotalHits=TotalHits + @TotalHits " +
            "  WHERE ProductId=@ProductId AND OperatingSystemId=@OperatingSystemId " +
            "ELSE " + 
            "  INSERT INTO dbo.OperatingSystemSummary " +
            "    (OperatingSystemId, ProductId, TotalHits) " +
            "  VALUES " +
            "    (@OperatingSystemId, @ProductId, @TotalHits); ";

        private const String s_AddOperatingSystemSummarySqlOverwrite =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.OperatingSystemSummary AS OSS WHERE OSS.OperatingSystemId=@OperatingSystemId AND OSS.ProductId=@ProductId) " +
            "  UPDATE dbo.OperatingSystemSummary " +
            "  SET OperatingSystemId=@OperatingSystemId, ProductId=@ProductId, TotalHits=@TotalHits " +
            "  WHERE ProductId=@ProductId AND OperatingSystemId=@OperatingSystemId " +
            "ELSE " +
            "  INSERT INTO dbo.OperatingSystemSummary " +
            "    (OperatingSystemId, ProductId, TotalHits) " +
            "  VALUES " +
            "    (@OperatingSystemId, @ProductId, @TotalHits); ";

        #endregion OperatingSystemSummarySql


        #region HitDateSummarySql

        // ******************************************************************************************
        // HIT DATE SUMMARY SQL.
        // ******************************************************************************************
        private const String s_GetHitDateSummarySql =
            "SELECT HS.HitDateUtc, HS.ProductId, HS.TotalHits " +
            "FROM dbo.HitDateSummary AS HS " +
            "WHERE HS.HitDateUtc=@HitDateUtc AND HS.ProductId=@ProductId;";

        private const String s_GetAllHitDateSummariesSql =
            "SELECT HS.HitDateUtc, HS.ProductId, HS.TotalHits " +
            "FROM dbo.HitDateSummary AS HS " +
            "WHERE HS.ProductId=@ProductId;";

        private const String s_HitDateSummaryExistsSql =
            "IF EXISTS( " +
            "  SELECT * " +
            "  FROM dbo.HitDateSummary HS " +
            "  WHERE HS.HitDateUtc=@HitDateUtc AND HS.ProductId=@ProductId " + 
            " ) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_AddHitDateSummarySql =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.HitDateSummary AS HS WHERE HS.HitDateUtc=@HitDateUtc AND HS.ProductId=@ProductId) " +
            "  UPDATE dbo.HitDateSummary " +
            "  SET HitDateUtc=@HitDateUtc, ProductId=@ProductId, TotalHits=TotalHits + @TotalHits " +
            "  WHERE ProductId=@ProductId AND HitDateUtc=@HitDateUtc " +
            "ELSE " +
            "  INSERT INTO dbo.HitDateSummary " +
            "    (HitDateUtc, ProductId, TotalHits) " +
            "  VALUES " +
            "    (@HitDateUtc, @ProductId, @TotalHits); ";

        private const String s_AddHitDateSummarySqlOverwrite =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.HitDateSummary AS HS WHERE HS.HitDateUtc=@HitDateUtc AND HS.ProductId=@ProductId) " +
            "  UPDATE dbo.HitDateSummary " +
            "  SET HitDateUtc=@HitDateUtc, ProductId=@ProductId, TotalHits=@TotalHits " +
            "  WHERE ProductId=@ProductId AND HitDateUtc=@HitDateUtc " +
            "ELSE " +
            "  INSERT INTO dbo.HitDateSummary " +
            "    (HitDateUtc, ProductId, TotalHits) " +
            "  VALUES " +
            "    (@HitDateUtc, @ProductId, @TotalHits); ";

        
        #endregion HitDateSummarySql


        #region CabsSql

        // ******************************************************************************************
        // CABS SQL.
        // ******************************************************************************************

        private const String s_AddCabAllSql =
            "INSERT INTO dbo.Cabs " +
            "    (CabId, DateCreatedLocal, DateModifiedLocal, CabFileName, EventId, EventTypeId, SizeInBytes, CabDownloaded, Purged, " +  
            "     SystemUpTime, ProcessUpTime, DotNetVersion, OSVersion, MachineArchitecture) " +
            "VALUES " +
            "    (@CabId, @DateCreatedLocal, @DateModifiedLocal, @CabFileName, @EventId, @EventTypeId, @SizeInBytes, @CabDownloaded, @Purged, " + 
            "     @SystemUpTime, @ProcessUpTime, @DotNetVersion, @OSVersion, @MachineArchitecture); ";

        private const String s_UpdateCabSqlAll =
            "UPDATE dbo.Cabs " +
            "SET CabId=@CabId, DateCreatedLocal=@DateCreatedLocal, DateModifiedLocal=@DateModifiedLocal, CabFileName=@CabFileName, EventId=@EventId, " +
            "    EventTypeId=@EventTypeId, SizeInBytes=@SizeInBytes, CabDownloaded=@CabDownloaded, Purged=@Purged , " +
            "    SystemUpTime=@SystemUpTime, ProcessUpTime=@ProcessUpTime, DotNetVersion=@DotNetVersion, OSVersion=@OSVersion, MachineArchitecture=@MachineArchitecture " +
            "WHERE CabId=@CabId;";

        private const String s_UpdateCabSqlJustWinQual =
            "UPDATE dbo.Cabs " +
            "SET CabId=@CabId, DateCreatedLocal=@DateCreatedLocal, DateModifiedLocal=@DateModifiedLocal, CabFileName=@CabFileName, EventId=@EventId, " +
            "    EventTypeId=@EventTypeId, SizeInBytes=@SizeInBytes, CabDownloaded=@CabDownloaded, Purged=@Purged " +
            "WHERE CabId=@CabId;";

        private static String s_GetCabSql =
            " SELECT C.CabId, RTRIM(C.CabFileName) AS CabFileName, C.DateCreatedLocal, C.DateModifiedLocal, C.EventId, RTRIM(ET.EventTypeName) AS EventTypeName, C.SizeInBytes, C.CabDownloaded, C.Purged, " +
            "        C.SystemUpTime, C.ProcessUpTime, C.DotNetVersion, C.OSVersion, C.MachineArchitecture " +
            " FROM dbo.Cabs AS C " +
            " INNER JOIN dbo.EventTypes AS ET " +
            " ON C.EventTypeId=ET.EventTypeId AND C.CabId=@CabId;";

        private static String s_GetCabsSql =
            " SELECT C.CabId, RTRIM(C.CabFileName) AS CabFileName, C.DateCreatedLocal, C.DateModifiedLocal, C.EventId, RTRIM(ET.EventTypeName) AS EventTypeName, C.SizeInBytes, C.CabDownloaded, C.Purged, " +
            "        C.SystemUpTime, C.ProcessUpTime, C.DotNetVersion, C.OSVersion, C.MachineArchitecture " +
            " FROM dbo.Cabs AS C " +
            " INNER JOIN dbo.EventTypes AS ET " +
            " ON C.EventTypeId=ET.EventTypeId AND C.EventId=@EventId AND C.EventTypeId=@EventTypeId;";

        private const String s_CabExistsSql =
            @"IF EXISTS(SELECT * FROM dbo.Cabs WHERE CabId=@CabId) SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";


        private const String s_GetCabCountSql =
            "SELECT Count(*) AS CabCount " +
            "FROM dbo.Cabs AS C " +
            "WHERE C.EventId=@EventId AND C.EventTypeId=@EventTypeId;";

        private const String s_GetCabFileCountSql =
            "SELECT Count(*) AS CabFileCount " +
            "FROM dbo.Cabs AS C " +
            "WHERE C.EventId=@EventId AND C.EventTypeId=@EventTypeId AND C.CabDownloaded<>0;";


        #endregion CabsSql

        #region CabNotesSql

        // ******************************************************************************************
        // CAB NOTES SQL.
        // ******************************************************************************************

        private const String s_AddCabNoteSql =
            "DECLARE @newKey AS INT; " +
            "INSERT INTO dbo.CabNotes " +
            " (CabId, TimeOfEntry, SourceId, UserId, Note) " +
            "VALUES " +
            " (@CabId, @TimeOfEntry, @SourceId, @UserId, @Note); " + 
            "SET @newKey = SCOPE_IDENTITY(); " +
            "SELECT @newKey AS NewKey;";

        private const String s_UpdateCabNoteSql =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.CabNotes AS CN WHERE CN.NoteId=@NoteId) " +
            "  UPDATE dbo.CabNotes " +
            "  SET CabId=@CabId, TimeOfEntry=@TimeOfEntry, SourceId=@SourceId, UserId=@UserId, Note=@Note " +
            "  WHERE NoteId=@NoteId; ";

        private const String s_GetCabNotesSql =
            "SELECT CN.TimeOfEntry, S.Source, U.StackHashUser, CN.Note, CN.NoteId " +
            "FROM dbo.CabNotes AS CN " +
            "INNER JOIN dbo.Users AS U ON CN.UserId=U.UserId " +
            "INNER JOIN dbo.Sources AS S ON CN.SourceId=S.SourceId " +
            "  AND CN.CabId=@CabId " +
            "ORDER BY CN.NoteId;";

        private const String s_GetCabNoteSql =
            "SELECT CN.TimeOfEntry, S.Source, U.StackHashUser, CN.Note, CN.NoteId " +
            "FROM dbo.CabNotes AS CN " +
            "INNER JOIN dbo.Users AS U ON CN.UserId=U.UserId " +
            "INNER JOIN dbo.Sources AS S ON CN.SourceId=S.SourceId " +
            "WHERE CN.NoteId=@NoteId; ";

        private const String s_DeleteCabNoteSql =
            "  DELETE FROM dbo.CabNotes " +
            "  WHERE CabId=@CabId AND NoteId=@NoteId ";
        
        #endregion CabNotesSql

        #region FileEventsSql

        // ******************************************************************************************
        // FILE EVENTS SQL.
        // ******************************************************************************************

        private const String s_FileEventExistsSql =
             "IF EXISTS(SELECT * FROM dbo.FileEvents WHERE FileId=@FileId AND EventId=@EventId AND EventTypeId=@EventTypeId) " + 
             "SELECT 1 AS EXTANT ELSE SELECT 0 AS EXTANT;";

        private const String s_AddFileEventSql =
            "INSERT INTO dbo.FileEvents " +
            "    (FileId, EventId, EventTypeId) " +
            "VALUES " +
            "    (@FileId, @EventId, @EventTypeId);";


        #endregion FileEventsSql

        #region UsersSql

        // ******************************************************************************************
        // USERS SQL.
        // ******************************************************************************************
        private const String s_GetUserIdSql =
            "SELECT * FROM dbo.Users WHERE StackHashUser=@UserName;";

        private const String s_UserExistsSql =
            "IF EXISTS(SELECT * FROM dbo.Users WHERE StackHashUser=@UserName) select 1 as extant else select 0 as extant;";

        private const String s_AddUserSql =
            "INSERT INTO dbo.Users " +
            "    (StackHashUser) " +
            "VALUES " +
            "    (@UserName); ";

        #endregion UsersSql

        #region SourceSql

        // ******************************************************************************************
        // Source SQL.
        // ******************************************************************************************
        private const String s_GetSourceIdSql =
            "SELECT * FROM dbo.Sources WHERE Source=@SourceName;";

        private const String s_SourceExistsSql =
            "IF EXISTS(SELECT * FROM dbo.Sources WHERE Source=@SourceName) select 1 as extant else select 0 as extant;";

        private const String s_AddSourceSql =
            "INSERT INTO dbo.Sources " +
            "    (Source) " +
            "VALUES " +
            "    (@SourceName); ";

        #endregion SourceSql

        #region StatisticsSql

        private const String s_GetLocaleSummary =
            "WITH ROLLUP AS (" +

            " SELECT EI.LocaleId, SUM(EI.TotalHits) AS TotalHits " +
            " FROM dbo.ProductFiles AS PF " +
            " INNER JOIN dbo.FileEvents FE ON PF.FileId=FE.FileId AND PF.ProductId=@ProductId " +
            " INNER JOIN dbo.EventInfos AS EI ON FE.EventId=EI.EventId AND FE.EventTypeId=EI.EventTypeId" +
            " GROUP BY EI.LocaleId) " +

            " SELECT L.LocaleId, L.LocaleCode, L.LocaleName, ROLLUP.TotalHits " +
            " FROM ROLLUP " +
            " INNER JOIN dbo.Locales L ON L.LocaleId=ROLLUP.LocaleId " +
            " ORDER BY TotalHits ";

        private const String s_GetOperatingSystemSummary =
            "WITH ROLLUP AS (" +

            " SELECT EI.OperatingSystemId, SUM(EI.TotalHits) AS TotalHits " +
            " FROM dbo.ProductFiles AS PF " +
            " INNER JOIN dbo.FileEvents FE ON PF.FileId=FE.FileId AND PF.ProductId=@ProductId " +
            " INNER JOIN dbo.EventInfos AS EI ON FE.EventId=EI.EventId AND FE.EventTypeId=EI.EventTypeId " +
            " GROUP BY EI.OperatingSystemId) " +

            " SELECT O.OperatingSystemName, O.OperatingSystemVersion, ROLLUP.TotalHits " + 
            " FROM ROLLUP " +
            " INNER JOIN dbo.OperatingSystems O ON O.OperatingSystemId=ROLLUP.OperatingSystemId " +
            " ORDER BY TotalHits ";

        private const String s_GetHitDateSummary =
            " SELECT EI.HitDateLocal, SUM(EI.TotalHits) AS TotalHits " +
            " FROM dbo.ProductFiles AS PF " +
            " INNER JOIN dbo.FileEvents FE ON PF.FileId=FE.FileId AND PF.ProductId=@ProductId " +
            " INNER JOIN dbo.EventInfos AS EI ON FE.EventId=EI.EventId AND FE.EventTypeId=EI.EventTypeId " +
            " GROUP BY EI.HitDateLocal " +
            " ORDER BY HitDateLocal ";
        
        #endregion

        #region UpdateSql

        private const String s_AddUpdateSql =
            "  INSERT INTO dbo.UpdateTable " +
            "    (DateChanged, ProductId, FileId, EventId, EventTypeId, CabId, ChangedObjectId, DataThatChanged, TypeOfChange) " +
            "  VALUES " +
            "    (@DateChanged, @ProductId, @FileId, @EventId, @EventTypeId, @CabId, @ChangedObjectId, @DataThatChanged, @TypeOfChange); ";

        private const String s_GetUpdateSql =
            "SELECT TOP 1 U.EntryId, U.DateChanged, U.ProductId, U.FileId, U.EventId, ET.EventTypeName, U.CabId, U.ChangedObjectId, U.DataThatChanged, U.TypeOfChange " +
            "FROM dbo.UpdateTable AS U " +
            "INNER JOIN dbo.EventTypes AS ET " +
            "  ON ET.EventTypeId=U.EventTypeId " + 
            "ORDER BY U.EntryId;";

        private const String s_RemoveUpdateSql =
            "  DELETE FROM dbo.UpdateTable " +
            "  WHERE EntryId=@EntryId; ";

        private const String s_ClearAllUpdateSql =
            "  DELETE FROM dbo.UpdateTable " +
            "  WHERE EntryId > 0; ";

        
        #endregion UpdateSql

        #region MappingsSql

        private const String s_AddWorkFlowMappingSql =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.WorkFlowMappings AS M WHERE M.WorkFlowStatusId=@Id) " +
            "  UPDATE dbo.WorkFlowMappings " +
            "  SET WorkFlowStatusId=@Id, WorkFlowStatusName=@Name " +
            "  WHERE WorkFlowStatusId=@Id " +
            "ELSE " +
            " INSERT INTO dbo.WorkFlowMappings " +
            "  (WorkFlowStatusId, WorkFlowStatusName) " +
            " VALUES " +
            "  (@Id, @Name); ";

        private const String s_GetWorkFlowMappingsSql =
            @"SELECT * FROM dbo.WorkFlowMappings;";

        private const String s_AddGroupMappingSql =
            "IF EXISTS (SELECT TOP 1 1 FROM dbo.GroupMappings AS M WHERE M.GroupId=@Id) " +
            "  UPDATE dbo.GroupMappings " +
            "  SET GroupId=@Id, GroupName=@Name " +
            "  WHERE GroupId=@Id " +
            "ELSE " +
            " INSERT INTO dbo.GroupMappings " +
            "  (GroupId, GroupName) " +
            " VALUES " +
            "  (@Id, @Name); ";

        private const String s_GetGroupMappingsSql =
            @"SELECT * FROM dbo.GroupMappings;";

        #endregion

        #endregion AllSql

        #region Properties

        public SqlUtils SqlUtilities
        {
            get { return m_SqlUtils; }
        }

        #endregion Properties

        #region Constructors

        public SqlCommands(DbProviderFactory provider, String connectionString, String masterConnectionString, int connectionRetryLimit)
        {
            m_ProviderFactory = provider;
            m_ConnectionString = connectionString;
            m_ConnectionRetryLimit = connectionRetryLimit;
            m_MasterConnectionString = masterConnectionString;
            m_SqlUtils = new SqlUtils(provider, m_ConnectionString, m_MasterConnectionString, m_ConnectionRetryLimit);
            m_EventTypeCache = new SqlEventTypeCache();
            m_OperatingSystemCache = new SqlOperatingSystemCache();
            m_LocaleCache = new SqlLocaleCache();
        }

        #endregion Constructors

        #region PrivateMethods

        private static String convertListToSet(Collection<int> targetList)
        {
            StringBuilder targetString = new StringBuilder("(");

            for (int i = 0; i < targetList.Count; i++)
            {
                targetString.Append(targetList[i].ToString(CultureInfo.InvariantCulture));
                if (i < targetList.Count - 1)
                    targetString.Append(",");
            }

            targetString.Append(")");
            return targetString.ToString();
        }
        
        /// <summary>
        /// Converts the file to product mapping to a single set of file ids.
        /// The product id is not used.
        /// </summary>
        /// <param name="targetList">File/Product mappings.</param>
        /// <returns>Set of file ids on text form.</returns>
        private static String convertListToSet(StackHashFileProductMappingCollection targetList)
        {
            StringBuilder targetString = new StringBuilder("(");

            for (int i = 0; i < targetList.Count; i++)
            {
                targetString.Append(targetList[i].FileId.ToString(CultureInfo.InvariantCulture));
                if (i < targetList.Count - 1)
                    targetString.Append(",");
            }

            targetString.Append(")");
            return targetString.ToString();
        }

        
        #endregion PrivateMethods

        #region ConnectionMethods

        public DbConnection CreateConnection(bool master)
        {
            try
            {
                return m_SqlUtils.CreateConnection(master);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.SqlConnectionError);
            }
            catch (System.Exception ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.SqlConnectionError);
            }
        }

        public void ReleaseConnection(DbConnection connection)
        {
            m_SqlUtils.ReleaseConnection(connection);
        }

        #endregion ConnectionMethods

        #region DatabaseMethods

        /// <summary>
        /// Creates a database of the specified name.
        /// </summary>
        /// <param name="databaseName">Name of the database to create.</param>
        /// <returns>True - if success, false otherwise.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool DatabaseExists(String databaseName)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;
            
            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);
                return m_SqlUtils.DatabaseExists(databaseName, connection);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }

        /// <summary>
        /// Gets the status of the specified database.
        /// Checks to see if a connection can be made to the database.
        /// </summary>
        /// <param name="databaseName">Name of the database to create.</param>
        /// <returns>True - if success, false otherwise.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        [SuppressMessage("Microsoft.Design", "CA1031")]
        public ErrorIndexConnectionTestResults GetDatabaseStatus(String databaseName, bool testDatabaseExistence)
        {
            if (String.IsNullOrEmpty(databaseName))
                return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.InvalidDatabaseName, null);

            if (!InstallerInterface.IsValidSqlDatabaseName(databaseName))
                return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.InvalidDatabaseName, null);

            DbConnection connection = null;

            try
            {
                try
                {
                    // Connect to master.
                    connection = CreateConnection(true);

                    try
                    {
                        if (!testDatabaseExistence)
                            return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.Success, null);

                        bool databaseExists = m_SqlUtils.DatabaseExists(databaseName, connection);

                        if (!databaseExists)
                            return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.ConnectedToMasterButDatabaseDoesNotExist, null);
                        
                        // Database exists so try and connect to it - later.
                    }
                    catch (System.Exception ex)
                    {
                        return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.ConnectedToMasterButFailedToSeeIfDatabaseExists, ex);
                    }
                }
                catch (System.Exception ex)
                {
                    return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.FailedToConnectToMaster, ex);
                }
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }


            // Shouldn't get here unless the database exists.
            try
            {
                // Connect to the actual database.
                connection = CreateConnection(false);
                return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.Success, null);
            }
            catch (System.Exception ex)
            {
                return new ErrorIndexConnectionTestResults(StackHashErrorIndexDatabaseStatus.DatabaseExistsButFailedToConnect, ex);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }


        /// <summary>
        /// Creates a database of the specified name in the specified folder.
        /// </summary>
        /// <param name="databaseFolder">Folder to store the database.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>True - if database created, false - otherwise.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool CreateDatabase(String databaseFolder, String databaseName, bool createIndexInDefaultLocation)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);
                return m_SqlUtils.CreateDatabase(databaseFolder, databaseName, connection, createIndexInDefaultLocation);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }


        /// <summary>
        /// Creates the StackHash tables in a database in the specified folder.
        /// </summary>
        /// <param name="databaseFolder">Folder to store the database.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>True - if database created, false - otherwise.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool CreateStackHashDatabase(String databaseFolder, String databaseName, bool createIndexInDefaultLocation)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            return StackHashSqlControl.SqlSchema.CreateStackHashDatabase(m_SqlUtils, databaseFolder, databaseName, createIndexInDefaultLocation);
        }



        /// <summary>
        /// Renames the database in the specified location. The database must exist.
        /// </summary>
        /// <param name="oldDatabaseName">Old database name.</param>
        /// <param name="newDatabaseName">New database name.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool RenameDatabase(String oldDatabaseName, String newDatabaseName)
        {
            if (String.IsNullOrEmpty(oldDatabaseName))
                return false;
            if (String.IsNullOrEmpty(newDatabaseName))
                return false;

            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);

                if (!m_SqlUtils.DatabaseExists(oldDatabaseName, connection))
                    throw new StackHashException("Database not found: " + oldDatabaseName, StackHashServiceErrorCode.DatabaseNotFound);

                return m_SqlUtils.RenameDatabase(oldDatabaseName, newDatabaseName, connection);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }


        /// <summary>
        /// Sets the database to online (true) or offline (false) state.
        /// This is normally done when moving a database.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="setOnline">True - set online, False - set offline.</param>
        /// <returns>true - success, false - failed to set offline.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool SetDatabaseOnlineState(String databaseName, bool setOnline)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);

                return m_SqlUtils.SetDatabaseOnlineState(databaseName, connection, setOnline);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSetDatabaseOnlineState);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }

        }


        /// <summary>
        /// Get a list of logical names associated with a database.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>true - success, false - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public Collection<String> GetLogicalFileNames(String databaseName)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);

                return m_SqlUtils.GetLogicalFileNames(databaseName, connection);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetDatabaseLogicalFileNames);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }

        
        /// <summary>
        /// Changes the logical name for the primary files in the database.
        /// The database files are stored with a logical name. 
        /// When moving a database, the logical name should also be changed.
        /// This call gets all the logical names and changes those that currently have 
        /// the old index name into the new index name.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="newDatabaseName">New database name.</param>
        /// <returns>true - success, false - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ChangeDatabaseLogicalNames(String databaseName, String newDatabaseName, bool currentNameIsNewName)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            if (String.IsNullOrEmpty(newDatabaseName))
                throw new ArgumentNullException("newDatabaseName");

            Collection<String> logicalNames = null;

            if (currentNameIsNewName)
                logicalNames = GetLogicalFileNames(newDatabaseName);
            else
                logicalNames = GetLogicalFileNames(databaseName);

            Collection<Tuple<String, String>> changes = new Collection<Tuple<String,String>>();

            foreach (String logicalName in logicalNames)
            {
                int foundStringIndex = logicalName.IndexOf(databaseName, StringComparison.OrdinalIgnoreCase);
                int nextCharIndex = foundStringIndex + databaseName.Length;

                String newLogicalName = newDatabaseName;
                
                if (nextCharIndex < logicalName.Length)
                    newLogicalName += logicalName.Substring(nextCharIndex);

                changes.Add(new Tuple<String,String>(logicalName, newLogicalName));
            }

            if (currentNameIsNewName)
                ChangeDatabaseLogicalNameList(newDatabaseName, changes);
            else
                ChangeDatabaseLogicalNameList(databaseName, changes);
            return true;
        }

            
            
        /// <summary>
        /// Changes the logical name for the primary files in the database.
        /// The database files are stored with a logical name. 
        /// When moving a database, the logical name should also be changed.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="newDatabaseName">New database name.</param>
        /// <returns>true - success, false - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ChangeDatabaseLogicalName(String databaseName, String newDatabaseName)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            if (String.IsNullOrEmpty(newDatabaseName))
                throw new ArgumentNullException("newDatabaseName");

            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);

                return m_SqlUtils.ChangeDatabaseLogicalName(databaseName, newDatabaseName, connection);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToChangeDatabaseLogicalName);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }

        /// <summary>
        /// Changes the logical name for the primary files in the database.
        /// The database files are stored with a logical name. 
        /// When moving a database, the logical name should also be changed.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="newDatabaseName">New database name.</param>
        /// <returns>true - success, false - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        [SuppressMessage("Microsoft.Design", "CA1006")]
        public bool ChangeDatabaseLogicalNameList(String databaseName, Collection<Tuple<String,String>> changes)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            if (changes == null)
                throw new ArgumentNullException("changes");

            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);

                return m_SqlUtils.ChangeDatabaseLogicalNameList(databaseName, changes, connection);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToChangeDatabaseLogicalName);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }


        /// <summary>
        /// Changes the location of the database.
        /// This changes the expected directory and filename of the database primary files.
        /// </summary>
        /// <param name="databaseName">Current database name.</param>
        /// <param name="newDatabaseName">New database name.</param>
        /// <param name="newErrorIndexPath">New folder location of the database files.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ChangeDatabaseLocation(String databaseName, String newDatabaseName, String newErrorIndexPath)
        {
            if (String.IsNullOrEmpty(databaseName))
                throw new ArgumentNullException("databaseName");

            if (String.IsNullOrEmpty(newDatabaseName))
                throw new ArgumentNullException("newDatabaseName");

            if (String.IsNullOrEmpty(newErrorIndexPath))
                throw new ArgumentNullException("newErrorIndexPath");


            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);

                return m_SqlUtils.ChangeDatabaseLocation(databaseName, newDatabaseName, newErrorIndexPath, connection);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToChangeDatabaseLocation);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }
        
        
        /// <summary>
        /// Deletes the database of the specified name.
        /// </summary>
        /// <param name="databaseName">Name of the database to delete.</param>
        /// <returns>True - success, False - otherwise.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool DeleteDatabase(String databaseName)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            DbConnection connection = null;

            try
            {
                connection = CreateConnection(true);

                m_SqlUtils.SelectDatabase("MASTER", connection);

                return m_SqlUtils.DeleteDatabase(databaseName, connection);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToDeleteDatabase);
            }
            finally
            {
                if (connection != null)
                {
                    m_SqlUtils.ReleaseConnection(connection);
                    connection = null;
                }
            }
        }


        /// <summary>
        /// Selects the specified database in SQL server for this session.
        /// This is normally done to prevent released connections which are still present in the 
        /// connection pool from pointing to a database that we are currently trying to delete.
        /// Mainly used for unit testing where the database is created and deleted for each test.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="connection">Connection to use.</param>
        /// <returns></returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool SelectDatabase(String databaseName, DbConnection connection)
        {
            if (String.IsNullOrEmpty(databaseName))
                return false;

            if (!DatabaseExists(databaseName))
                return false;

            try
            {
                return m_SqlUtils.SelectDatabase(databaseName, connection);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSelectDatabase);
            }
        }

        #endregion DatabaseMethods


        #region ControlDataMethods

        /// <summary>
        /// There is only one control data entry in the table.
        /// Gets the control data. Should only be 1 entry with ID 1.
        /// </summary>
        /// <returns>Control data related to the index.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashControlData GetControlData()
        {
            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
            sqlCommand.CommandText = s_GetControlSql;
            m_SqlUtils.AddParameter(sqlCommand, "@ControlItemId", DbType.Int32, 1);

            DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

            try
            {
                if (reader.Read())
                {
                    int databaseVersion = (int)reader["DatabaseVersion"];
                    int syncCount = (int)reader["SyncCount"];
                    int lastSyncProductId = (int)reader["SyncProductId"];
                    int lastSyncFileId = (int)reader["SyncFileId"];
                    int lastSyncEventId = (int)reader["SyncEventId"];
                    int lastSyncCabId = (int)reader["SyncCabId"];
                    String lastSyncEventTypeName = SqlUtils.GetNullableString(reader["SyncEventTypeName"]);
                    int lastSyncPhase = (int)reader["SyncPhase"];

                    StackHashSyncProgress lastSyncProgress = new StackHashSyncProgress(lastSyncProductId, lastSyncFileId, 
                        lastSyncEventId, lastSyncEventTypeName, lastSyncCabId, (StackHashSyncPhase)lastSyncPhase);

                    StackHashControlData controlData = new StackHashControlData(databaseVersion, syncCount, lastSyncProgress);
                    return controlData;
                }
                else
                {
                    // Not found so return defaults.
                    return new StackHashControlData(0, 0, new StackHashSyncProgress());
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetControlData);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }


        /// <summary>
        /// Determines if the control entry exists.
        /// </summary>
        /// <param name="controlItemId">Item to check for (should be 1).</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ControlExists(long controlItemId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_ControlExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ControlItemId", DbType.Int32, controlItemId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToDetermineIfControlEntryExists);
            }
        }


        /// <summary>
        /// Adds a control item to the database. There should only be 1.
        /// This call assumes that the product is not currently present.
        /// </summary>
        /// <param name="controlData">ControlData to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddControl(StackHashControlData controlData)
        {
            if (controlData == null)
                throw new ArgumentNullException("controlData");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddControlSql;

                m_SqlUtils.AddParameter(sqlCommand, "@ControlItemId", DbType.Int32, 1);
                m_SqlUtils.AddParameter(sqlCommand, "@DatabaseVersion", DbType.Int32, controlData.DatabaseVersion);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncCount", DbType.Int32, controlData.SyncCount);

                m_SqlUtils.AddParameter(sqlCommand, "@SyncProductId", DbType.Int32, controlData.LastSyncProgress.ProductId);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncFileId", DbType.Int32, controlData.LastSyncProgress.FileId);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncEventId", DbType.Int32, controlData.LastSyncProgress.EventId);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncCabId", DbType.Int32, controlData.LastSyncProgress.CabId);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncEventTypeName", DbType.String, SqlUtils.MakeSqlCompliantString(controlData.LastSyncProgress.EventTypeName));
                m_SqlUtils.AddParameter(sqlCommand, "@SyncPhase", DbType.Int32, controlData.LastSyncProgress.SyncPhase);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddControlData);
            }
        }


        /// <summary>
        /// Updates the control data.
        /// </summary>
        /// <param name="controlData">ControlData to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void UpdateControl(StackHashControlData controlData)
        {
            if (controlData == null)
                throw new ArgumentNullException("controlData");

            if (!ControlExists(1))
            {
                AddControl(controlData);
                return;
            }

            if (controlData.LastSyncProgress == null)
                controlData.LastSyncProgress = new StackHashSyncProgress();

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_UpdateControlSql;

                m_SqlUtils.AddParameter(sqlCommand, "@ControlItemId", DbType.Int32, 1);
                m_SqlUtils.AddParameter(sqlCommand, "@DatabaseVersion", DbType.Int32, controlData.DatabaseVersion);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncCount", DbType.Int32, controlData.SyncCount);

                m_SqlUtils.AddParameter(sqlCommand, "@SyncProductId", DbType.Int32, controlData.LastSyncProgress.ProductId);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncFileId", DbType.Int32, controlData.LastSyncProgress.FileId);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncEventId", DbType.Int32, controlData.LastSyncProgress.EventId);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncCabId", DbType.Int32, controlData.LastSyncProgress.CabId);
                m_SqlUtils.AddParameter(sqlCommand, "@SyncEventTypeName", DbType.String, SqlUtils.MakeSqlCompliantString(controlData.LastSyncProgress.EventTypeName));
                m_SqlUtils.AddParameter(sqlCommand, "@SyncPhase", DbType.Int32, controlData.LastSyncProgress.SyncPhase);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateControlData);
            }
        }


        /// <summary>
        /// Gets control data for the specified product.
        /// Control data for the product includes the time the product was last synced etc...
        /// </summary>
        /// <returns>Control data related to the specified product.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashProductControlData GetProductControlData(long productId)
        {
            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
                
            sqlCommand.CommandText = s_GetProductControlSql;
            m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);

            DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

            try
            {
                if (reader.Read())
                {
                    DateTime lastSyncTime = (DateTime)reader["LastSyncTime"];
                    lastSyncTime = SqlUtils.ConvertToLocal(lastSyncTime);

                    DateTime lastSyncCompletedTime = (DateTime)reader["LastSyncCompletedTime"];
                    lastSyncCompletedTime = SqlUtils.ConvertToLocal(lastSyncCompletedTime);

                    DateTime lastSyncStartedTime = (DateTime)reader["LastSyncStartedTime"];
                    lastSyncStartedTime = SqlUtils.ConvertToLocal(lastSyncStartedTime);

                    DateTime lastHitTime = (DateTime)reader["LastHitTime"];
                    lastHitTime = SqlUtils.ConvertToLocal(lastHitTime);

                    byte syncEnabledValue = (byte)reader["SyncEnabled"];
                    bool syncEnabled = (syncEnabledValue == 0) ? false : true;

                    StackHashProductControlData controlData = new StackHashProductControlData((int)productId, lastSyncTime, lastSyncCompletedTime, 
                        lastSyncStartedTime, lastHitTime, syncEnabled);

                    return controlData;
                }
                else
                {
                    return new StackHashProductControlData((int)productId, new DateTime(0), new DateTime(0), new DateTime(0), new DateTime(0), false);
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToReadProductControlData);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }


        /// <summary>
        /// Determines if the product control entry exists.
        /// </summary>
        /// <param name="productId">Id of product to check for.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ProductControlExists(long productId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_ProductControlExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfProductControlDataExists);
            }
        }


        /// <summary>
        /// Adds a product control item to the database for the specified product.
        /// This call assumes that the product is not currently present.
        /// </summary>
        /// <param name="productId">ID of the product to add.</param>
        /// <param name="controlData">ControlData to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddProductControl(long productId, StackHashProductControlData controlData)
        {
            if (controlData == null)
                throw new ArgumentNullException("controlData");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddProductControlSql;

                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);
                m_SqlUtils.AddParameter(sqlCommand, "@LastSyncTime", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastSyncTime));
                m_SqlUtils.AddParameter(sqlCommand, "@LastSyncCompletedTime", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastSyncCompletedTime));
                m_SqlUtils.AddParameter(sqlCommand, "@LastSyncStartedTime", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastSyncStartedTime));
                m_SqlUtils.AddParameter(sqlCommand, "@LastHitTime", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastHitTime));
                m_SqlUtils.AddParameter(sqlCommand, "@SyncEnabled", DbType.Byte, controlData.SyncActive ? 1 : 0);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddProductControlData);
            }
        }


        /// <summary>
        /// Updates the control data.
        /// </summary>
        /// <param name="productId">ID of the product to update.</param>
        /// <param name="controlData">ControlData to update.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void UpdateProductControl(long productId, StackHashProductControlData controlData)
        {
            if (controlData == null)
                throw new ArgumentNullException("controlData");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_UpdateProductControlSql;

                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);
                m_SqlUtils.AddParameter(sqlCommand, "@LastSyncTime", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastSyncTime));
                m_SqlUtils.AddParameter(sqlCommand, "@LastSyncCompletedTime", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastSyncCompletedTime));
                m_SqlUtils.AddParameter(sqlCommand, "@LastSyncStartedTime", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastSyncStartedTime));
                m_SqlUtils.AddParameter(sqlCommand, "@LastHitTime", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastHitTime));
                m_SqlUtils.AddParameter(sqlCommand, "@SyncEnabled", DbType.Byte, controlData.SyncActive ? 1 : 0);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateProductControlData);
            }
        }



        /// <summary>
        /// Gets task control data for the specified task.
        /// </summary>
        /// <param name="taskType">Task type for which data is required.</param>
        /// <returns>Control data related to the specified task.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashTaskStatus GetTaskControlData(StackHashTaskType taskType)
        {
            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
                
            sqlCommand.CommandText = s_GetTaskControlSql;
            m_SqlUtils.AddParameter(sqlCommand, "@TaskType", DbType.Int32, (int)taskType);

            DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

            try
            {
                if (reader.Read())
                {
                    StackHashTaskState taskState = (StackHashTaskState)reader["TaskState"];

                    DateTime lastSuccessfulRunTimeUtc = (DateTime)reader["LastSuccessfulRunTimeUtc"];
                    lastSuccessfulRunTimeUtc = SqlUtils.ConvertToUtc(lastSuccessfulRunTimeUtc);

                    DateTime lastFailedRunTimeUtc = (DateTime)reader["LastFailedRunTimeUtc"];
                    lastFailedRunTimeUtc = SqlUtils.ConvertToUtc(lastFailedRunTimeUtc);

                    DateTime lastStartedTimeUtc = (DateTime)reader["LastStartedTimeUtc"];
                    lastStartedTimeUtc = SqlUtils.ConvertToUtc(lastStartedTimeUtc);

                    int lastDurationInSeconds = (int)reader["LastDurationInSeconds"];
                    int runCount = (int)reader["RunCount"];
                    int successCount = (int)reader["SuccessCount"];
                    int failedCount = (int)reader["FailedCount"];

                    String lastException = SqlUtils.GetNullableString(reader["LastTaskException"]);

                    StackHashServiceErrorCode serviceErrorCode = (StackHashServiceErrorCode)reader["ServiceErrorCode"];

                    StackHashTaskStatus controlData = new StackHashTaskStatus();
                    controlData.FailedCount = failedCount;
                    controlData.LastDurationInSeconds = lastDurationInSeconds;
                    controlData.LastException = lastException;
                    controlData.LastFailedRunTimeUtc = lastFailedRunTimeUtc;
                    controlData.LastStartedTimeUtc = lastStartedTimeUtc;
                    controlData.LastSuccessfulRunTimeUtc = lastSuccessfulRunTimeUtc;
                    controlData.RunCount = runCount;
                    controlData.ServiceErrorCode = serviceErrorCode;
                    controlData.SuccessCount = successCount;
                    controlData.TaskState = taskState;
                    controlData.TaskType = taskType;
                    return controlData;
                }
                else
                {
                    StackHashTaskStatus controlData = new StackHashTaskStatus();
                    controlData.FailedCount = 0;
                    controlData.LastDurationInSeconds = 0;
                    controlData.LastException = null;
                    controlData.LastFailedRunTimeUtc = new DateTime(0, DateTimeKind.Utc);
                    controlData.LastStartedTimeUtc = new DateTime(0, DateTimeKind.Utc);
                    controlData.LastSuccessfulRunTimeUtc = new DateTime(0, DateTimeKind.Utc);
                    controlData.RunCount = 0;
                    controlData.ServiceErrorCode = StackHashServiceErrorCode.NoError;
                    controlData.SuccessCount = 0;
                    controlData.TaskState = StackHashTaskState.NotRunning;
                    controlData.TaskType = taskType;
                    return controlData;
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToReadTaskControlData);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
        }


        /// <summary>
        /// Determines if the task control entry exists.
        /// </summary>
        /// <param name="taskType">Task type for which control data is to be tested.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool TaskControlExists(StackHashTaskType taskType)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_TaskControlExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@TaskType", DbType.Int32, (int)taskType);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfTaskControlDataExists);
            }
        }


        /// <summary>
        /// Adds a task control item to the database for the specified.
        /// This call assumes that the task data is not currently present.
        /// </summary>
        /// <param name="taskType">Task type to add.</param>
        /// <param name="controlData">Task control data to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddTaskControl(StackHashTaskType taskType, StackHashTaskStatus controlData)
        {
            if (controlData == null)
                throw new ArgumentNullException("controlData");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddTaskControlSql;

                m_SqlUtils.AddParameter(sqlCommand, "@TaskType", DbType.Int32, (int)taskType);
                m_SqlUtils.AddParameter(sqlCommand, "@LastFailedRunTimeUtc", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastFailedRunTimeUtc));
                m_SqlUtils.AddParameter(sqlCommand, "@LastSuccessfulRunTimeUtc", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastSuccessfulRunTimeUtc));
                m_SqlUtils.AddParameter(sqlCommand, "@LastStartedTimeUtc", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastStartedTimeUtc));
                m_SqlUtils.AddParameter(sqlCommand, "@FailedCount", DbType.Int32, controlData.FailedCount);
                m_SqlUtils.AddParameter(sqlCommand, "@LastDurationInSeconds", DbType.Int32, controlData.LastDurationInSeconds);
                m_SqlUtils.AddParameter(sqlCommand, "@LastTaskException", DbType.String, SqlUtils.MakeSqlCompliantString(controlData.LastException));

                m_SqlUtils.AddParameter(sqlCommand, "@RunCount", DbType.Int32, controlData.RunCount);
                m_SqlUtils.AddParameter(sqlCommand, "@ServiceErrorCode", DbType.Int32, (int)controlData.ServiceErrorCode);
                m_SqlUtils.AddParameter(sqlCommand, "@SuccessCount", DbType.Int32, controlData.SuccessCount);
                m_SqlUtils.AddParameter(sqlCommand, "@TaskState", DbType.Int32, (int)controlData.TaskState);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddTaskControlData);
            }
        }


        /// <summary>
        /// Updates a task control item to the database for the specified.
        /// This call assumes that the task control data is already present.
        /// </summary>
        /// <param name="taskType">Task type to update.</param>
        /// <param name="controlData">Task control data to update.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void UpdateTaskControl(StackHashTaskType taskType, StackHashTaskStatus controlData)
        {
            if (controlData == null)
                throw new ArgumentNullException("controlData");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_UpdateTaskControlSql;

                m_SqlUtils.AddParameter(sqlCommand, "@TaskType", DbType.Int32, (int)taskType);
                m_SqlUtils.AddParameter(sqlCommand, "@LastFailedRunTimeUtc", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastFailedRunTimeUtc));
                m_SqlUtils.AddParameter(sqlCommand, "@LastSuccessfulRunTimeUtc", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastSuccessfulRunTimeUtc));
                m_SqlUtils.AddParameter(sqlCommand, "@LastStartedTimeUtc", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(controlData.LastStartedTimeUtc));
                m_SqlUtils.AddParameter(sqlCommand, "@FailedCount", DbType.Int32, controlData.FailedCount);
                m_SqlUtils.AddParameter(sqlCommand, "@LastDurationInSeconds", DbType.Int32, controlData.LastDurationInSeconds);
                m_SqlUtils.AddParameter(sqlCommand, "@LastTaskException", DbType.String, SqlUtils.MakeSqlCompliantString(controlData.LastException));
                m_SqlUtils.AddParameter(sqlCommand, "@RunCount", DbType.Int32, controlData.RunCount);
                m_SqlUtils.AddParameter(sqlCommand, "@ServiceErrorCode", DbType.Int32, (int)controlData.ServiceErrorCode);
                m_SqlUtils.AddParameter(sqlCommand, "@SuccessCount", DbType.Int32, controlData.SuccessCount);
                m_SqlUtils.AddParameter(sqlCommand, "@TaskState", DbType.Int32, (int)controlData.TaskState);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateTaskControlData);
            }
        }

        #endregion ControlDataMethods

        #region ProductMethods

        /// <summary>
        /// Adds a StackHashProduct to the database.
        /// This call assumes that the product is not currently present.
        /// </summary>
        /// <param name="product">Product to add.</param>
        /// <returns>Product added.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool AddProduct(StackHashProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");


            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddProductSql;

                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, product.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@ProductName", DbType.String, product.Name);
                m_SqlUtils.AddParameter(sqlCommand, "@Version", DbType.String, product.Version);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, product.DateCreatedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, product.DateModifiedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalEvents", DbType.Int32, product.TotalEvents);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalResponses", DbType.Int32, product.TotalResponses);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalStoredEvents", DbType.Int32, product.TotalStoredEvents);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return true;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddProduct);
            }
        }


        /// <summary>
        /// Updates a StackHashProduct in the database.
        /// This call assumes that the product is currently present.
        /// </summary>
        /// <param name="product">Product to update.</param>
        /// <param name="updateNonWinQualFields">True - updates all field. False - just updates the WinQual fields.</param>
        /// <returns>Product updated.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool UpdateProduct(StackHashProduct product, bool updateNonWinQualFields)
        {
            if (product == null)
                throw new ArgumentNullException("product");


            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();


                if (updateNonWinQualFields)
                    sqlCommand.CommandText = s_UpdateProductSqlAll;
                else
                    sqlCommand.CommandText = s_UpdateProductSqlJustWinQual;

                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, product.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@ProductName", DbType.String, product.Name);
                m_SqlUtils.AddParameter(sqlCommand, "@Version", DbType.String, product.Version);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, product.DateCreatedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, product.DateModifiedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalEvents", DbType.Int32, product.TotalEvents);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalResponses", DbType.Int32, product.TotalResponses);

                if (updateNonWinQualFields)
                    m_SqlUtils.AddParameter(sqlCommand, "@TotalStoredEvents", DbType.Int32, product.TotalStoredEvents);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return true;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateProduct);
            }
        }

        
        /// <summary>
        /// Gets all products from the product table.
        /// </summary>
        /// <returns>Product list.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashProductCollection GetProducts()
        {
            StackHashProductCollection products = new StackHashProductCollection();

            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
                
            sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_GetProductsSql);

            DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

            try
            {
                while (reader.Read())
                {
                    DateTime dateCreatedLocal = (DateTime)reader["DateCreatedLocal"];
                    dateCreatedLocal = new DateTime(dateCreatedLocal.Year, dateCreatedLocal.Month, dateCreatedLocal.Day, dateCreatedLocal.Hour, dateCreatedLocal.Minute, dateCreatedLocal.Second, DateTimeKind.Utc);
                    DateTime dateModifiedLocal = (DateTime)reader["DateModifiedLocal"];
                    dateModifiedLocal = new DateTime(dateModifiedLocal.Year, dateModifiedLocal.Month, dateModifiedLocal.Day, dateModifiedLocal.Hour, dateModifiedLocal.Minute, dateModifiedLocal.Second, DateTimeKind.Utc);

                    long id = (long)reader["ProductId"];
                    String productName = (String)reader["ProductName"];
                    int totalEvents = (int)reader["TotalEvents"];
                    int totalResponses = (int)reader["TotalResponses"];
                    String version = (String)reader["Version"];
                    int totalStoredEvents = (int)reader["totalStoredEvents"];

                    StackHashProduct product = new StackHashProduct(dateCreatedLocal, dateModifiedLocal, null, (int)id, productName, totalEvents, totalResponses, version, totalStoredEvents);

                    products.Add(product);
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToReadProducts);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return products;
        }


        /// <summary>
        /// Gets the product with the specified ID from the database.
        /// </summary>
        /// <param name="productId">Product ID.</param>
        /// <returns>The full row of the Products table.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public StackHashProduct GetProduct(int productId)
        {
            DbCommand sqlCommand = m_ProviderFactory.CreateCommand();
                
            sqlCommand.CommandText = s_GetProductSql;
            m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);

            DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

            StackHashProduct product = null;

            try
            {
                reader.Read();

                DateTime dateCreatedLocal = (DateTime)reader["DateCreatedLocal"];
                DateTime dateModifiedLocal = (DateTime)reader["DateModifiedLocal"];
                dateCreatedLocal = new DateTime(dateCreatedLocal.Year, dateCreatedLocal.Month, dateCreatedLocal.Day, dateCreatedLocal.Hour, dateCreatedLocal.Minute, dateCreatedLocal.Second, DateTimeKind.Utc);
                dateModifiedLocal = new DateTime(dateModifiedLocal.Year, dateModifiedLocal.Month, dateModifiedLocal.Day, dateModifiedLocal.Hour, dateModifiedLocal.Minute, dateModifiedLocal.Second, DateTimeKind.Utc);

                long id = (long)reader["ProductId"];
                String productName = (String)reader["ProductName"];
                int totalEvents = (int)reader["TotalEvents"];
                int totalResponses = (int)reader["TotalResponses"];
                String version = (String)reader["Version"];
                int totalStoredEvents = (int)reader["totalStoredEvents"];

                product = new StackHashProduct(dateCreatedLocal, dateModifiedLocal, null, (int)id, productName, totalEvents, totalResponses, version, totalStoredEvents);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToReadProduct);
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }

            return product;
        }


        
        /// <summary>
        /// Determines if a product with the specified ID exists in the database.
        /// </summary>
        /// <param name="productId">Product to check for.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ProductExists(long productId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_ProductExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);

                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfProductExists);
            }
        }

        
        /// <summary>
        /// Gets the number of events for a product.
        /// </summary>
        /// <param name="productId">Product to check.</param>
        /// <returns>Total number of events.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public int GetProductEventCount(long productId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_ProductEventCountSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                int numEvents = 0;
                try
                {
                    reader.Read();

                    numEvents = (int)reader["NumberOfEvents"];
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }

                return numEvents;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetProductEventCount);
            }
        }


        /// <summary>
        /// Gets the number of events for the specified products.
        /// Excludes duplicates that might be shared across products.
        /// </summary>
        /// <param name="productIds">Products to check.</param>
        /// <returns>Total number of events.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public long GetProductsEventCount(Collection<int> productIds)
        {
            if (productIds == null)
                throw new ArgumentNullException("productIds");

            if (productIds.Count == 0)
                return 0;

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                StringBuilder productList = new StringBuilder();
                bool firstProduct = true;
                foreach (int productId in productIds)
                {
                    if (!firstProduct)
                        productList.Append(",");
                    else
                        firstProduct = false;

                    productList.Append(productId.ToString(CultureInfo.InvariantCulture));
                }

                sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_ProductsEventCountSql, productList.ToString());

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                int numEvents = 0;
                try
                {
                    reader.Read();

                    numEvents = (int)reader["NumberOfEvents"];
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }

                return numEvents;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetProductEventCount);
            }
        }

        /// <summary>
        /// Gets product count across the whole database.
        /// </summary>
        /// <returns>Number of products in the entire index.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public long GetProductCount()
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetProductCountSql;

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        int productCount = (int)reader["ProductCount"];
                        return productCount;
                    }
                    else
                    {
                        return 0;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetProductCount);
            }
        }

        /// <summary>
        /// Gets product matching the specified criteria.
        /// </summary>
        /// <returns>List of products matching the specified criteria.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public Collection<int> GetProductMatch(String condition)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                if (String.IsNullOrEmpty(condition))
                    sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_GetProductMatchNoConditionSql);
                else
                    sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_GetProductMatchSql, condition);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                Collection<int> productIds = new Collection<int>();

                try
                {
                    while (reader.Read())
                    {
                        long productId = (long)reader["ProductId"];
                        productIds.Add((int)productId);
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }

                return productIds;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetProductMatch);
            }
        }
        
        
        #endregion ProductMethods

        #region FileMethods

        /// <summary>
        /// Adds a StackHashFile to the database.
        /// This call assumes that the file is not currently present.
        /// </summary>
        /// <param name="file">File to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddFile(StackHashFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddFileSql;

                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, file.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@FileName", DbType.String, file.Name);
                m_SqlUtils.AddParameter(sqlCommand, "@Version", DbType.String, file.Version);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, file.DateCreatedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, file.DateModifiedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@LinkDateLocal", DbType.DateTime, file.LinkDateLocal);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddFile);
            }
        }


        /// <summary>
        /// Updates a StackHashFile in the database.
        /// This call assumes that the file is currently present.
        /// </summary>
        /// <param name="file">File to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void UpdateFile(StackHashFile file)
        {
            if (file == null)
                throw new ArgumentNullException("file");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_UpdateFileSqlAll;

                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, file.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, file.DateCreatedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, file.DateModifiedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@FileName", DbType.String, file.Name);
                m_SqlUtils.AddParameter(sqlCommand, "@Version", DbType.String, file.Version);
                m_SqlUtils.AddParameter(sqlCommand, "@LinkDateLocal", DbType.DateTime, file.LinkDateLocal);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateFile);
            }
        }

        /// <summary>
        /// Determines if a file with the specified ID exists in the database.
        /// Note the file could be associated with any number of products.
        /// </summary>
        /// <param name="fileId">File to check for.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool FileExists(long fileId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_FileExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, fileId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfFileExists);
            }
        }


        /// <summary>
        /// Gets file count across the whole database.
        /// </summary>
        /// <returns>Number of files in the entire index.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public long GetFileCount()
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetFileCountSql;

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        int fileCount = (int)reader["FileCount"];
                        return fileCount;
                    }
                    else
                    {
                        return 0;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetFileCount);
            }
        }


        /// <summary>
        /// Determines if a file with the specified ID exists in the database associated with 
        /// the specified product.
        /// </summary>
        /// <param name="productId">Product owning the file.</param>
        /// <param name="fileId">File to check for.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool ProductFileExists(long productId, long fileId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_ProductFileExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);
                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, fileId);

                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfProductFileExists);
            }
        }





        /// <summary>
        /// Adds an association between a file and a product to the database.
        /// </summary>
        /// <param name="product">Product that owns the file.</param>
        /// <param name="file">File to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddProductFile(StackHashProduct product, StackHashFile file)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            if (file == null)
                throw new ArgumentNullException("file");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddProductFileSql;

                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, product.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, file.Id);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddProductFile);
            }
        }


        /// <summary>
        /// Gets all files associated with a particular product.
        /// </summary>
        /// <param name="product">Product for which the files are required.</param>
        /// <returns>List of product files.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashFileCollection GetFiles(StackHashProduct product)
        {
            if (product == null)
                throw new ArgumentNullException("product");


            try
            {
                StackHashFileCollection files = new StackHashFileCollection();

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetFilesSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, product.Id);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        DateTime dateCreatedLocal = (DateTime)reader["DateCreatedLocal"];
                        dateCreatedLocal = new DateTime(dateCreatedLocal.Year, dateCreatedLocal.Month, dateCreatedLocal.Day, dateCreatedLocal.Hour, dateCreatedLocal.Minute, dateCreatedLocal.Second, DateTimeKind.Utc);
                        DateTime dateModifiedLocal = (DateTime)reader["DateModifiedLocal"];
                        dateModifiedLocal = new DateTime(dateModifiedLocal.Year, dateModifiedLocal.Month, dateModifiedLocal.Day, dateModifiedLocal.Hour, dateModifiedLocal.Minute, dateModifiedLocal.Second, DateTimeKind.Utc);
                        DateTime linkDateLocal = (DateTime)reader["LinkDateLocal"];
                        linkDateLocal = new DateTime(linkDateLocal.Year, linkDateLocal.Month, linkDateLocal.Day, linkDateLocal.Hour, linkDateLocal.Minute, linkDateLocal.Second, DateTimeKind.Utc);

                        long id = (long)reader["FileId"];
                        String fileName = (String)reader["FileName"];
                        String version = (String)reader["Version"];

                        StackHashFile file = new StackHashFile(dateCreatedLocal, dateModifiedLocal, (int)id, linkDateLocal, fileName, version);

                        files.Add(file);
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
                return files;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetFiles);
            }
        }


        /// <summary>
        /// Gets files for the specified list of products matching the specified product criteria.
        /// </summary>
        /// <returns>List of products matching the specified criteria.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashFileProductMappingCollection GetFilesMatch(Collection<int> products, String fileCondition)
        {
            if (products == null)
                throw new ArgumentNullException("products");
            if (fileCondition == null)
                throw new ArgumentNullException("fileCondition");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                if (products.Count > 0)
                    sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_GetFilesMatchWithProductSetSql, convertListToSet(products));
                else
                    sqlCommand.CommandText = String.Format(CultureInfo.InvariantCulture, s_GetFilesMatchSql);

                if (!String.IsNullOrEmpty(fileCondition))
                    sqlCommand.CommandText += " AND " + fileCondition;

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashFileProductMappingCollection fileIds = new StackHashFileProductMappingCollection();

                try
                {
                    while (reader.Read())
                    {
                        long fileId = (long)reader["FileId"];
                        long productId = (long)reader["ProductId"];
                        fileIds.Add(new StackHashFileProductMapping((int)fileId, (int)productId));
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }

                return fileIds;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetFilesMatch);
            }
        }


        /// <summary>
        /// Gets the file with the specified ID.
        /// </summary>
        /// <param name="fileId">File to get.</param>
        /// <returns>File data.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        [SuppressMessage("Microsoft.Globalization", "CA1303")]
        public StackHashFile GetFile(long fileId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetFileSql;
                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, fileId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        DateTime dateCreatedLocal = (DateTime)reader["DateCreatedLocal"];
                        dateCreatedLocal = new DateTime(dateCreatedLocal.Year, dateCreatedLocal.Month, dateCreatedLocal.Day, dateCreatedLocal.Hour, dateCreatedLocal.Minute, dateCreatedLocal.Second, DateTimeKind.Utc);
                        DateTime dateModifiedLocal = (DateTime)reader["DateModifiedLocal"];
                        dateModifiedLocal = new DateTime(dateModifiedLocal.Year, dateModifiedLocal.Month, dateModifiedLocal.Day, dateModifiedLocal.Hour, dateModifiedLocal.Minute, dateModifiedLocal.Second, DateTimeKind.Utc);
                        DateTime linkDateLocal = (DateTime)reader["LinkDateLocal"];
                        linkDateLocal = new DateTime(linkDateLocal.Year, linkDateLocal.Month, linkDateLocal.Day, linkDateLocal.Hour, linkDateLocal.Minute, linkDateLocal.Second, DateTimeKind.Utc);

                        long id = (long)reader["FileId"];
                        String fileName = (String)reader["FileName"];
                        String version = (String)reader["Version"];

                        return new StackHashFile(dateCreatedLocal, dateModifiedLocal, (int)id, linkDateLocal, fileName, version);
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetFile);
            }
        }

        #endregion FileMethods

        #region EventMethods

        /// <summary>
        /// Adds a StackHashEvent to the database.
        /// This call assumes that the event is not currently present.
        /// </summary>
        /// <param name="theEvent">Event to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddEvent(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");


            try
            {
                // First make sure the event type name exists in the database.
                short eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                if (eventTypeId == -1)
                {
                    AddEventType(theEvent.EventTypeName);
                    eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                }

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddEventSql;

                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(theEvent.DateCreatedLocal));
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(theEvent.DateModifiedLocal));
                m_SqlUtils.AddParameter(sqlCommand, "@TotalHits", DbType.Int64, theEvent.TotalHits);
                m_SqlUtils.AddParameter(sqlCommand, "@BugId", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.BugId));
                m_SqlUtils.AddParameter(sqlCommand, "@PlugInBugId", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.PlugInBugId));
                m_SqlUtils.AddParameter(sqlCommand, "@WorkFlowStatus", DbType.Int32, theEvent.WorkFlowStatus);

                m_SqlUtils.AddParameter(sqlCommand, "@ApplicationName", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.EventSignature.ApplicationName));
                m_SqlUtils.AddParameter(sqlCommand, "@ApplicationVersion", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.EventSignature.ApplicationVersion));
                m_SqlUtils.AddParameter(sqlCommand, "@ApplicationTimeStamp", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(theEvent.EventSignature.ApplicationTimeStamp));
                m_SqlUtils.AddParameter(sqlCommand, "@ModuleName", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.EventSignature.ModuleName));
                m_SqlUtils.AddParameter(sqlCommand, "@ModuleVersion", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.EventSignature.ModuleVersion));
                m_SqlUtils.AddParameter(sqlCommand, "@ModuleTimeStamp", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(theEvent.EventSignature.ModuleTimeStamp));

                // These are converted versions of the original string.
                m_SqlUtils.AddParameter(sqlCommand, "@Offset", DbType.Int64, theEvent.EventSignature.Offset);
                m_SqlUtils.AddParameter(sqlCommand, "@ExceptionCode", DbType.Int64, theEvent.EventSignature.ExceptionCode);

                if (theEvent.EventSignature.Parameters != null)
                {
                    StackHashParameter param = theEvent.EventSignature.Parameters.FindParameter(StackHashEventSignature.ParamOffset);
                    if (param != null)
                        m_SqlUtils.AddParameter(sqlCommand, "@OffsetOriginal", DbType.String, param.Value);
                    else
                        m_SqlUtils.AddParameter(sqlCommand, "@OffsetOriginal", DbType.String, DBNull.Value);

                    param = theEvent.EventSignature.Parameters.FindParameter(StackHashEventSignature.ParamExceptionCode);
                    if (param != null)
                        m_SqlUtils.AddParameter(sqlCommand, "@ExceptionCodeOriginal", DbType.String, param.Value);
                    else
                        m_SqlUtils.AddParameter(sqlCommand, "@ExceptionCodeOriginal", DbType.String, DBNull.Value);
                }
                else
                {
                    m_SqlUtils.AddParameter(sqlCommand, "@OffsetOriginal", DbType.String, "0");
                    m_SqlUtils.AddParameter(sqlCommand, "@ExceptionCodeOriginal", DbType.String, "0");
                }

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddEvent);
            }
        }


        /// <summary>
        /// Updates the specified entry in the database.
        /// This call assumes that the event is not currently present.
        /// </summary>
        /// <param name="theEvent">Event to add.</param>
        /// <param name="updateNonWinQualFields">True - updates all field. False - just updates the WinQual fields.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void UpdateEvent(StackHashEvent theEvent, bool updateNonWinQualFields)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                // Get the associated event type ID. Must already exist if we are doing an update.
                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                if (updateNonWinQualFields)
                    sqlCommand.CommandText = s_UpdateEventSqlAll;
                else
                    sqlCommand.CommandText = s_UpdateEventSqlJustWinQual;

                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(theEvent.DateCreatedLocal));
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(theEvent.DateModifiedLocal));
                m_SqlUtils.AddParameter(sqlCommand, "@TotalHits", DbType.Int64, theEvent.TotalHits);
                m_SqlUtils.AddParameter(sqlCommand, "@ApplicationName", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.EventSignature.ApplicationName));
                m_SqlUtils.AddParameter(sqlCommand, "@ApplicationVersion", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.EventSignature.ApplicationVersion));
                m_SqlUtils.AddParameter(sqlCommand, "@ApplicationTimeStamp", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(theEvent.EventSignature.ApplicationTimeStamp));
                m_SqlUtils.AddParameter(sqlCommand, "@ModuleName", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.EventSignature.ModuleName));
                m_SqlUtils.AddParameter(sqlCommand, "@ModuleVersion", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.EventSignature.ModuleVersion));
                m_SqlUtils.AddParameter(sqlCommand, "@ModuleTimeStamp", DbType.DateTime, SqlUtils.MakeDateSqlCompliant(theEvent.EventSignature.ModuleTimeStamp));
                m_SqlUtils.AddParameter(sqlCommand, "@Offset", DbType.Int64, theEvent.EventSignature.Offset);
                m_SqlUtils.AddParameter(sqlCommand, "@ExceptionCode", DbType.Int64, theEvent.EventSignature.ExceptionCode);

                if (theEvent.EventSignature.Parameters != null)
                {
                    StackHashParameter param = theEvent.EventSignature.Parameters.FindParameter(StackHashEventSignature.ParamOffset);
                    if ((param != null) && (param.Value != null))
                        m_SqlUtils.AddParameter(sqlCommand, "@OffsetOriginal", DbType.String, param.Value);
                    else
                        m_SqlUtils.AddParameter(sqlCommand, "@OffsetOriginal", DbType.String, DBNull.Value);

                    param = theEvent.EventSignature.Parameters.FindParameter(StackHashEventSignature.ParamExceptionCode);
                    if ((param != null) && (param.Value != null))
                        m_SqlUtils.AddParameter(sqlCommand, "@ExceptionCodeOriginal", DbType.String, param.Value);
                    else
                        m_SqlUtils.AddParameter(sqlCommand, "@ExceptionCodeOriginal", DbType.String, DBNull.Value);
                }
                else
                {
                    m_SqlUtils.AddParameter(sqlCommand, "@OffsetOriginal", DbType.String, "0");
                    m_SqlUtils.AddParameter(sqlCommand, "@ExceptionCodeOriginal", DbType.String, "0");
                }

                if (updateNonWinQualFields)
                {
                    m_SqlUtils.AddParameter(sqlCommand, "@BugId", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.BugId));
                    m_SqlUtils.AddParameter(sqlCommand, "@PlugInBugId", DbType.String, SqlUtils.MakeSqlCompliantString(theEvent.PlugInBugId));
                    m_SqlUtils.AddParameter(sqlCommand, "@WorkFlowStatus", DbType.Int32, theEvent.WorkFlowStatus);
                }

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateEvent);
            }
        }


        /// <summary>
        /// Gets the event with the specified ID.
        /// </summary>
        /// <param name="fileId">File owning the event.</param>
        /// <param name="eventId">The event whose type id is required.</param>
        /// <returns>ID of the event entry.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashEvent GetEvent(long fileId, long eventId, String eventTypeName)
        {
            int eventTypeId = GetEventTypeId(eventTypeName);

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetEventSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, eventId);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        StackHashEvent retrievedEvent = readEvent(reader, fileId);
                        retrievedEvent.FileId = (int)fileId;
                        return retrievedEvent;
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEvent);
            }
        }


        /// <summary>
        /// Gets event count across the whole database.
        /// </summary>
        /// <returns>Number of events in the entire index.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public long GetEventCount()
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetEventCountSql;

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        int eventCount = (int)reader["EventCount"];
                        return eventCount;
                    }
                    else
                    {
                        return 0;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEventCount);
            }
        }

        
        /// <summary>
        /// Reads an event in from the specified Database Data reader.
        /// </summary>
        /// <param name="reader">The reader containing the event.</param>
        /// <param name="fileId">Id of the file.</param>
        /// <returns>The stackhashevent.</returns>
        private static StackHashEvent readEvent(DbDataReader reader, long fileId)
        {
            DateTime dateCreatedLocal = SqlUtils.ConvertToUtc((DateTime)reader["DateCreatedLocal"]);
            DateTime dateModifiedLocal = SqlUtils.ConvertToUtc((DateTime)reader["DateModifiedLocal"]);

            long id = (long)reader["EventId"];
            String eventTypeName = (String)reader["EventTypeName"];
            long totalHits = (long)reader["TotalHits"];
            String bugId = SqlUtils.GetNullableString(reader["BugId"]);
            String plugInBugId = SqlUtils.GetNullableString(reader["PlugInBugId"]);
            int workFlowStatus = (int)reader["WorkFlowStatus"];
            String workFlowStatusName = (String)reader["WorkFlowStatusName"];
            int cabCount = SqlUtils.GetNullableInteger(reader["CabCount"]);

            StackHashEventSignature eventSignature = new StackHashEventSignature();
            eventSignature.Parameters = new StackHashParameterCollection();
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationName, SqlUtils.GetNullableString(reader["ApplicationName"])));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationVersion, SqlUtils.GetNullableString(reader["ApplicationVersion"])));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamApplicationTimeStamp, ((DateTime)reader["ApplicationTimeStamp"]).ToString(CultureInfo.InvariantCulture)));

            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleName, SqlUtils.GetNullableString(reader["ModuleName"])));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleVersion, SqlUtils.GetNullableString(reader["ModuleVersion"])));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamModuleTimeStamp, ((DateTime)reader["ModuleTimeStamp"]).ToString(CultureInfo.InvariantCulture)));

            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamExceptionCode, SqlUtils.GetNullableString(reader["ExceptionCodeOriginal"])));
            eventSignature.Parameters.Add(new StackHashParameter(StackHashEventSignature.ParamOffset, SqlUtils.GetNullableString(reader["OffsetOriginal"])));

            eventSignature.ApplicationName = SqlUtils.GetNullableString(reader["ApplicationName"]);
            eventSignature.ApplicationVersion = SqlUtils.GetNullableString(reader["ApplicationVersion"]);
            eventSignature.ApplicationTimeStamp = SqlUtils.ConvertToUtc((DateTime)reader["ApplicationTimeStamp"]);

            eventSignature.ModuleName = SqlUtils.GetNullableString(reader["ModuleName"]);
            eventSignature.ModuleVersion = SqlUtils.GetNullableString(reader["ModuleVersion"]);
            eventSignature.ModuleTimeStamp = SqlUtils.ConvertToUtc((DateTime)reader["ModuleTimeStamp"]);

            eventSignature.ExceptionCode = (long)reader["ExceptionCode"];
            eventSignature.Offset = (long)reader["Offset"];

            // Just parse the numeric fields as the others should be ok as they are strings and dates.
            // This is done in case we have got the conversion wrong in the past - should now be correctable.
            eventSignature.InterpretParameters(true);

            StackHashEvent retrievedEvent = new StackHashEvent(dateCreatedLocal, dateModifiedLocal, eventTypeName, 
                (int)id, eventSignature, (int)totalHits, (int)fileId, bugId, plugInBugId, workFlowStatus, workFlowStatusName);

            retrievedEvent.CabCount = cabCount;

            return retrievedEvent;
        }


        /// <summary>
        /// Gets the events associated with the specified file.
        /// </summary>
        /// <param name="fileId">File owning the event.</param>
        /// <param name="eventId">The event whose type id is required.</param>
        /// <returns>List of events on the specific file.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashEventCollection LoadEventList(long fileId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_LoadEventsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, fileId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashEventCollection allEvents = new StackHashEventCollection();

                try
                {
                    while (reader.Read())
                    {
                        StackHashEvent retrievedEvent = readEvent(reader, fileId);
                        allEvents.Add(retrievedEvent);
                    }

                    return allEvents;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToLoadEventList);
            }
        }


        /// <summary>
        /// Gets the events associated with the specified product.
        /// </summary>
        /// <param name="productId">Identifier for the product.</param>
        /// <returns>List of events on the specific product.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashEventPackageCollection GetProductEvents(long productId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetProductEventsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

                try
                {
                    while (reader.Read())
                    {
                        StackHashEvent retrievedEvent = readEvent(reader, 0);
                        long fileId = (long)reader["FileId"];
                        retrievedEvent.FileId = (int)fileId;
                        StackHashEventPackage eventPackage = new StackHashEventPackage(null, null, retrievedEvent, (int)productId);
                        allEvents.Add(eventPackage);
                    }

                    return allEvents;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.GetProductEvents);
            }
        }


        /// <summary>
        /// Gets all events matching the specified search criteria.
        /// </summary>
        /// <param name="searchCriteriaCollection">The search criteria to match on.</param>
        /// <returns>List of matching events.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashEventPackageCollection GetEvents(StackHashFileProductMappingCollection fileIds, StackHashSearchCriteriaCollection searchCriteriaCollection)
        {
            if (fileIds == null)
                throw new ArgumentNullException("fileIds");
            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");

            try
            {
                // Example select statement.
                //  SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits,
                //         E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName,
                //         E.BugId
                //  FROM dbo.FileEvents FE
                //  INNER JOIN dbo.Events E 
                //  ON FE.EventId=E.EventId
                //  AND FE.FileId IN (1,2,3)
                //  AND <EventConditions>

                // "INNER JOIN dbo.EventInfos EI" +
                // "ON EI.EventId=FE.EventId {3} " +
                // "INNER JOIN dbo.EventTypes ET " +
                // "ON E.EventTypeId=ET.EventTypeId AND E.EventId=@EventId";



                StringBuilder cmd = new StringBuilder(
                    "SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, " +
                    "       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName, " +
                    "       E.BugId, E.PlugInBugId, FE.FileId " +
                    "FROM dbo.FileEvents FE " +
                    "INNER JOIN dbo.EventTypes ET " +
                    " ON FE.EventTypeId=ET.EventTypeId " +
                    "INNER JOIN dbo.Events E " +
                     "ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId ");

                String eventSearchString = searchCriteriaCollection.ToSqlString(StackHashObjectType.Event, "E");
                String eventSignatureSearchString = searchCriteriaCollection.ToSqlString(StackHashObjectType.EventSignature, "E");
                if (!String.IsNullOrEmpty(eventSignatureSearchString))
                {
                    if (!String.IsNullOrEmpty(eventSearchString))
                        eventSearchString += " AND " + eventSignatureSearchString;
                    else
                        eventSearchString += eventSignatureSearchString;
                }

                if (fileIds.Count > 0)
                {
                    cmd.Append(" AND FE.FileId IN ");
                    cmd.Append(convertListToSet(fileIds));
                }

                if (!String.IsNullOrEmpty(eventSearchString))
                {
                    cmd.Append(" AND ");
                    cmd.Append(eventSearchString);
                }

                cmd.AppendLine("INNER JOIN dbo.WorkFlowMappings WF " +
                               " ON WF.WorkFlowStatusId=E.WorkFlowStatus ");

                // Add a CAB criteria if one is present.
                if (searchCriteriaCollection.ObjectCount(StackHashObjectType.CabInfo) > 0)
                {
                    String cabSearchString = searchCriteriaCollection.ToSqlString(StackHashObjectType.CabInfo, "C");
                    if (!String.IsNullOrEmpty(cabSearchString))
                    {
                        cmd.AppendLine(" INNER JOIN (");
                        cmd.AppendLine("    SELECT DISTINCT C.EventId, C.EventTypeId ");
                        cmd.AppendLine("    FROM Cabs AS C ");
                        cmd.AppendLine("    INNER JOIN dbo.EventTypes CET ");
                        cmd.AppendLine("    ON C.EventTypeId=CET.EventTypeId ");
                        cmd.AppendLine("    WHERE ");
                        cmd.AppendLine(cabSearchString);
                        cmd.AppendLine(") AS MC ON E.EventId=MC.EventId AND E.EventTypeId=MC.EventTypeId ");
                    }
                }

                // Add a CAB criteria if one is present.
                if (searchCriteriaCollection.ObjectCount(StackHashObjectType.EventInfo) > 0)
                {
                    String eventInfoSearchString = searchCriteriaCollection.ToSqlString(StackHashObjectType.EventInfo, "EI");
                    if (!String.IsNullOrEmpty(eventInfoSearchString))
                    {
                        cmd.AppendLine(" INNER JOIN (");
                        cmd.AppendLine("    SELECT DISTINCT EI.EventId, EI.EventTypeId ");
                        cmd.AppendLine("    FROM EventInfos AS EI ");
                        cmd.AppendLine("    INNER JOIN OperatingSystems AS O ");
                        cmd.AppendLine("    ON O.OperatingSystemId=EI.OperatingSystemId ");
                        cmd.AppendLine("    INNER JOIN Locales AS L ");
                        cmd.AppendLine("    ON L.LocaleId=EI.LocaleId AND ");
                        cmd.AppendLine(eventInfoSearchString);
                        cmd.AppendLine(") AS MEI ON E.EventId=MEI.EventId AND E.EventTypeId=MEI.EventTypeId ");
                    }
                }


                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = cmd.ToString();

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

                try
                {
                    while (reader.Read())
                    {
                        StackHashEvent retrievedEvent = readEvent(reader, 0);

                        // Read in any additional fields.
                        long fileId = (long)reader["FileId"];
                        retrievedEvent.FileId = (int)fileId;

                        // TODO: NEED TO SET THE PRODUCT ID HERE.

                        StackHashEventPackage eventPackage = new StackHashEventPackage(null, null, retrievedEvent, (int)2);

                        if (allEvents.FindEventPackage(retrievedEvent.Id, retrievedEvent.EventTypeName) == null)
                            allEvents.Add(eventPackage);
                    }

                    return allEvents;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEvents);
            }
        }


        /// <summary>
        /// Gets a window of events matching the specified search criteria and sort options.
        /// </summary>
        /// <param name="searchCriteriaCollection">The search criteria to match on.</param>
        /// <returns>List of matching events.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashEventPackageCollection GetEvents(StackHashFileProductMappingCollection fileIds, 
            StackHashSearchCriteriaCollection searchCriteriaCollection,
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOptions)
        {
            if (fileIds == null)
                throw new ArgumentNullException("fileIds");
            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");
            if (sortOptions == null)
                throw new ArgumentNullException("sortOptions");

            try
            {
                // Example select statement.
                //  SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits,
                //         E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName,
                //         E.BugId, 
                //         ROW_NUMBER() OVER (ORDER BY E.EventId ASC, E....) AS RowNumber    // The sort options make up this line.
                //  FROM dbo.FileEvents FE
                //  INNER JOIN dbo.Events E 
                //  ON FE.EventId=E.EventId
                //  AND FE.FileId IN (1,2,3)
                //  AND <EventConditions>

                // "INNER JOIN dbo.EventInfos EI" +
                // "ON EI.EventId=FE.EventId {3} " +
                // "INNER JOIN dbo.EventTypes ET " +
                // "ON E.EventTypeId=ET.EventTypeId AND E.EventId=@EventId";



                StringBuilder cmd = new StringBuilder(
                    "WITH AE AS ( ");

                cmd.Append(
                    "SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, " +
                    "       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName, " +
                    "       E.BugId, E.PlugInBugId, FE.FileId, ");

                // Add the row number based on the specified sort order.
                cmd.Append(
                    "ROW_NUMBER() OVER (ORDER BY " +
                    sortOptions.ToSqlString("E", false) +
                    ") AS RowNumber ");

                cmd.Append(
                    "FROM dbo.FileEvents FE " +
                    "INNER JOIN dbo.EventTypes ET " +
                    " ON FE.EventTypeId=ET.EventTypeId " +
                    "INNER JOIN dbo.Events E " +
                     "ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId ");

                String eventSearchString = searchCriteriaCollection.ToSqlString(StackHashObjectType.Event, "E");
                String eventSignatureSearchString = searchCriteriaCollection.ToSqlString(StackHashObjectType.EventSignature, "E");
                if (!String.IsNullOrEmpty(eventSignatureSearchString))
                {
                    if (!String.IsNullOrEmpty(eventSearchString))
                        eventSearchString += " AND " + eventSignatureSearchString;
                    else
                        eventSearchString += eventSignatureSearchString;
                }

                if (fileIds.Count > 0)
                {
                    cmd.Append(" AND FE.FileId IN ");
                    cmd.Append(convertListToSet(fileIds));
                }

                if (!String.IsNullOrEmpty(eventSearchString))
                {
                    cmd.Append(" AND ");
                    cmd.Append(eventSearchString);
                }

                cmd.AppendLine("INNER JOIN dbo.WorkFlowMappings WF " +
                               " ON WF.WorkFlowStatusId=E.WorkFlowStatus ");

                // Add a CAB criteria if one is present.
                if (searchCriteriaCollection.ObjectCount(StackHashObjectType.CabInfo) > 0)
                {
                    String cabSearchString = searchCriteriaCollection.ToSqlString(StackHashObjectType.CabInfo, "C");
                    if (!String.IsNullOrEmpty(cabSearchString))
                    {
                        cmd.Append(" AND FE.EventId IN (");
                        cmd.Append("    SELECT C.EventId DISCTINCT ");
                        cmd.Append("    FROM Cabs AS C ");
                        cmd.Append("    WHERE ");
                        cmd.Append(cabSearchString);
                        cmd.Append(")");
                    }
                }

                // Add a CAB criteria if one is present.
                if (searchCriteriaCollection.ObjectCount(StackHashObjectType.EventInfo) > 0)
                {
                    String eventInfoSearchString = searchCriteriaCollection.ToSqlString(StackHashObjectType.EventInfo, "EI");
                    if (!String.IsNullOrEmpty(eventInfoSearchString))
                    {
                        cmd.Append(" AND FE.EventId IN (");
                        cmd.Append("    SELECT DISTINCT EI.EventId ");
                        cmd.Append("    FROM EventInfos AS EI ");
                        cmd.Append("    INNER JOIN OperatingSystems AS O ");
                        cmd.Append("    ON O.OperatingSystemId=EI.OperatingSystemId ");
                        cmd.Append("    INNER JOIN Locales AS L ");
                        cmd.Append("    ON L.LocaleId=EI.LocaleId AND ");
                        cmd.Append(eventInfoSearchString);
                        cmd.Append(")");
                    }
                }

                cmd.Append(
                    " ) " +
                    " SELECT AE.EventId, AE.DateCreatedLocal, AE.DateModifiedLocal, AE.ApplicationName, AE.ApplicationVersion, AE.ApplicationTimeStamp, AE.TotalHits, " +
                    "       AE.ModuleName, AE.ModuleVersion, AE.ModuleTimeStamp, AE.Offset, AE.ExceptionCode, AE.OffsetOriginal, AE.ExceptionCodeOriginal, AE.EventTypeName, " +
                    "       AE.BugId, AE.PlugInBugId, AE.FileId " +
                    " FROM AE " +
                    " WHERE AE.RowNumber>=@StartRow AND AE.RowNumber < (@StartRow + @NumberOfRows) " +
                    " ORDER BY " + sortOptions.ToSqlString("AE", true)); // Use virtual so the table name is not changed.

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = cmd.ToString();
                m_SqlUtils.AddParameter(sqlCommand, "@StartRow", DbType.Int64, startRow);
                m_SqlUtils.AddParameter(sqlCommand, "@NumberOfRows", DbType.Int64, numberOfRows);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

                try
                {
                    while (reader.Read())
                    {
                        StackHashEvent retrievedEvent = readEvent(reader, 0);

                        // Read in any additional fields.
                        long fileId = (long)reader["FileId"];
                        retrievedEvent.FileId = (int)fileId;

                        // TODO: NEED TO SET THE PRODUCT ID HERE.

                        StackHashEventPackage eventPackage = new StackHashEventPackage(null, null, retrievedEvent, (int)2);
                        allEvents.Add(eventPackage);
                    }

                    return allEvents;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEvents);
            }
        }



        /// <summary>
        /// Gets a window of events matching the specified search criteria and sort options.
        /// </summary>
        /// <param name="searchCriteriaCollection">The search criteria to match on.</param>
        /// <returns>List of matching events.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505")]
        public StackHashEventPackageCollection GetWindowedEvents(StackHashSearchCriteriaCollection searchCriteriaCollection,
            long startRow, long numberOfRows, StackHashSortOrderCollection sortOptions, StackHashProductSyncDataCollection enabledProducts)
        {
            if (searchCriteriaCollection == null)
                throw new ArgumentNullException("searchCriteriaCollection");
            if (sortOptions == null)
                throw new ArgumentNullException("sortOptions");

            if (startRow > Int32.MaxValue)
                startRow = Int32.MaxValue;
            if (numberOfRows > Int32.MaxValue)
                numberOfRows = Int32.MaxValue;
            if (startRow < 1)
                startRow = 1;

            // Some fields - like WorkFlowStatusName appears in the Event object but the field doesn't exist in the corresponding 
            // table in the database. In this case WorkFlowStatusName appears in the WorkFlow table. This call changes the object.
            // The client could do this itself but might be a bit confusing in the code.
            searchCriteriaCollection.CorrectObjects();
            sortOptions.Correct();

            try
            {
                StackHashFileProductMappingCollection allFileIds = new StackHashFileProductMappingCollection();

                // Example select statement.
                // WITH CR1 AS 
                // (
                //  SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits,        
                //         E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName,        
                //         E.BugId, FE.FileId 
                //  FROM dbo.FileEvents FE 
                //  INNER JOIN dbo.EventTypes ET  ON FE.EventTypeId=ET.EventTypeId 
                //  INNER JOIN dbo.Events E ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId  AND FE.FileId IN (1,2,3,4,5,6,7,8)
                // ),
                // CR2 AS 
                // (
                //  SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits,        
                //         E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName,        
                //         E.BugId, FE.FileId 
                //  FROM dbo.FileEvents FE 
                //  INNER JOIN dbo.EventTypes ET  ON FE.EventTypeId=ET.EventTypeId 
                //  INNER JOIN dbo.Events E ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId  AND FE.FileId IN (1,2,3,4,5,6,7,8)
                // ) , 
                // AE AS 
                // (
                //  SELECT CRALL.*,  
                //    ROW_NUMBER() OVER (ORDER BY CRALL.DateCreatedLocal ASC ,CRALL.DateModifiedLocal ASC ,CRALL.EventTypeName ASC ,CRALL.EventId ASC ,
                //                                CRALL.TotalHits ASC ,CRALL.BugId ASC ,CRALL.ApplicationName ASC ,CRALL.ApplicationVersion ASC ,
                //                                CRALL.ApplicationTimeStamp ASC ,CRALL.ModuleName ASC ,CRALL.ModuleVersion ASC ,CRALL.ModuleTimeStamp ASC ,
                //                                CRALL.Offset ASC ,CRALL.ExceptionCode ASC ) AS RowNumber  
                //  FROM  ( SELECT CR1.* FROM CR1 UNION SELECT CR2.* FROM CR2 )  AS CRALL 
                // )  
                // SELECT AE.EventId, AE.DateCreatedLocal, AE.DateModifiedLocal, AE.ApplicationName, AE.ApplicationVersion, AE.ApplicationTimeStamp, AE.TotalHits,        
                //        AE.ModuleName, AE.ModuleVersion, AE.ModuleTimeStamp, AE.Offset, AE.ExceptionCode, AE.OffsetOriginal, AE.ExceptionCodeOriginal, AE.EventTypeName,        
                //        AE.BugId, AE.FileId  
                // FROM AE  
                // WHERE AE.RowNumber>=@StartRow AND AE.RowNumber < (@StartRow + @NumberOfRows)  
                // ORDER BY AE.DateCreatedLocal ASC ,AE.DateModifiedLocal ASC ,AE.EventTypeName ASC ,AE.EventId ASC ,AE.TotalHits ASC ,
                //          AE.BugId ASC ,AE.ApplicationName ASC ,AE.ApplicationVersion ASC ,AE.ApplicationTimeStamp ASC ,AE.ModuleName ASC ,
                //          AE.ModuleVersion ASC ,AE.ModuleTimeStamp ASC ,AE.Offset ASC ,AE.ExceptionCode ASC 

                StringBuilder cmd = new StringBuilder();

                int criteriaNumber = 0;
                cmd.Append("WITH ");
                foreach (StackHashSearchCriteria criteria in searchCriteriaCollection)
                {
                    // Get the search string for the products. This can be null (or empty) in which case
                    // a list of ALL products will be returned.
                    String productSearchString = criteria.ToSqlString(StackHashObjectType.Product, "P");
                    Collection<int> matchedProductIds = GetProductMatch(productSearchString);

                    // Remove an products that are not enabled.
                    Collection<int> allProductIds = new Collection<int>();
                    foreach (int productId in matchedProductIds)
                    {
                        if (enabledProducts == null)
                            allProductIds.Add(productId);                        
                        else if (enabledProducts.FindProduct(productId) != null)
                            allProductIds.Add(productId);
                    }

                    if ( /*(criteria.ObjectCount(StackHashObjectType.Product) != 0) && */
                        (allProductIds.Count == 0))
                    {
                        continue;
                    }

                    String fileSearchString = criteria.ToSqlString(StackHashObjectType.File, "F");
                    StackHashFileProductMappingCollection fileIds = GetFilesMatch(allProductIds, fileSearchString);

                    if (/*(criteria.ObjectCount(StackHashObjectType.File) != 0) && */
                        (fileIds.Count == 0))
                    {
                        continue;
                    }

                    foreach (StackHashFileProductMapping fileProductMapping in fileIds)
                    {
                        allFileIds.Add(fileProductMapping);
                    }

                    if (criteriaNumber > 0)
                    {
                        cmd.Append(",");
                    }

                    int criteriaMapValue = 1 << criteriaNumber;
                    criteriaNumber++;
 
                    cmd.Append("CR");
                    cmd.Append(criteriaNumber);
                    cmd.AppendLine(" AS (");

                    cmd.AppendLine("SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal, E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, ");
                    cmd.AppendLine("       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, E.EventTypeId, ET.EventTypeName, ");
                    cmd.AppendLine("       E.BugId, E.PlugInBugId, E.WorkFlowStatus, WF.WorkFlowStatusName, FE.FileId, CABCOUNTER.CabCount, " + criteriaMapValue.ToString(CultureInfo.InvariantCulture) + " AS CriteriaNumber ");

                    cmd.AppendLine("FROM dbo.FileEvents FE ");
                    cmd.AppendLine("INNER JOIN dbo.EventTypes ET ");
                    cmd.AppendLine(" ON FE.EventTypeId=ET.EventTypeId ");
                    cmd.AppendLine("INNER JOIN dbo.Events E ");
                    cmd.AppendLine(" ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId ");


                    cmd.AppendLine("INNER JOIN dbo.WorkFlowMappings WF ");
                    cmd.AppendLine(" ON E.WorkFlowStatus=WF.WorkFlowStatusId ");

                    String workFlowSearchString = criteria.ToSqlString(StackHashObjectType.EventWorkFlow, "WF");
                    if (!String.IsNullOrEmpty(workFlowSearchString))
                    {
                        cmd.Append(" AND ");
                        cmd.Append(workFlowSearchString);
                    }
                    
                    // Add a CAB criteria if one is present.
                    if (searchCriteriaCollection.ObjectCount(StackHashObjectType.CabInfo) > 0)
                    {
                        String cabSearchString = criteria.ToSqlString(StackHashObjectType.CabInfo, "C");
                        if (!String.IsNullOrEmpty(cabSearchString))
                        {
                            cmd.AppendLine(" INNER JOIN (");
                            cmd.AppendLine("    SELECT DISTINCT C.EventId, C.EventTypeId ");
                            cmd.AppendLine("    FROM Cabs AS C ");
                            cmd.AppendLine("    INNER JOIN dbo.EventTypes CET ");
                            cmd.AppendLine("    ON C.EventTypeId=CET.EventTypeId ");
                            cmd.AppendLine("    WHERE ");
                            cmd.AppendLine(cabSearchString);
                            cmd.AppendLine(") AS MC ON E.EventId=MC.EventId AND E.EventTypeId=MC.EventTypeId ");
                        }
                    }

                    // Add a HITS criteria if one is present.
                    if (searchCriteriaCollection.ObjectCount(StackHashObjectType.EventInfo) > 0)
                    {
                        String eventInfoSearchString = criteria.ToSqlString(StackHashObjectType.EventInfo, "EI");
                        if (!String.IsNullOrEmpty(eventInfoSearchString))
                        {
                            cmd.AppendLine(" INNER JOIN (");
                            cmd.AppendLine("    SELECT DISTINCT EI.EventId, EI.EventTypeId ");
                            cmd.AppendLine("    FROM EventInfos AS EI ");
                            cmd.AppendLine("    INNER JOIN OperatingSystems AS O ");
                            cmd.AppendLine("    ON O.OperatingSystemId=EI.OperatingSystemId ");
                            cmd.AppendLine("    INNER JOIN Locales AS L ");
                            cmd.AppendLine("    ON L.LocaleId=EI.LocaleId AND ");
                            cmd.AppendLine(eventInfoSearchString);
                            cmd.AppendLine(") AS MEI ON E.EventId=MEI.EventId AND E.EventTypeId=MEI.EventTypeId ");
                        }
                    }

                    // Add Event Notes criteria if one is present.
                    if (searchCriteriaCollection.ObjectCount(StackHashObjectType.EventNotes) > 0)
                    {
                        String eventNotesSearchString = criteria.ToSqlString(StackHashObjectType.EventNotes, "EN");
                        if (!String.IsNullOrEmpty(eventNotesSearchString))
                        {
                            cmd.AppendLine(" INNER JOIN (");
                            cmd.AppendLine("    SELECT DISTINCT EN.EventId, EN.EventTypeId ");
                            cmd.AppendLine("    FROM EventNotes AS EN ");
                            cmd.AppendLine("    INNER JOIN Sources AS S ON EN.SourceId=S.SourceId ");
                            cmd.AppendLine("    INNER JOIN Users AS U ON EN.UserId=U.UserId ");
                            cmd.AppendLine("    WHERE ");
                            cmd.AppendLine(eventNotesSearchString);
                            cmd.AppendLine(") AS ENS ON E.EventId=ENS.EventId AND E.EventTypeId=ENS.EventTypeId ");
                        }
                    }

                    // Add Cab Notes criteria if one is present.
                    if (searchCriteriaCollection.ObjectCount(StackHashObjectType.CabNotes) > 0)
                    {
                        String cabNotesSearchString = criteria.ToSqlString(StackHashObjectType.CabNotes, "CN");
                        if (!String.IsNullOrEmpty(cabNotesSearchString))
                        {
                            cmd.AppendLine(" INNER JOIN (");
                            cmd.AppendLine("    SELECT DISTINCT C.EventId, C.EventTypeId ");
                            cmd.AppendLine("    FROM Cabs AS C ");
                            cmd.AppendLine("    INNER JOIN dbo.CabNotes CN ON C.CabId=CN.CabId ");
                            cmd.AppendLine("    INNER JOIN Sources AS S ON CN.SourceId=S.SourceId ");
                            cmd.AppendLine("    INNER JOIN Users AS U ON CN.UserId=U.UserId ");
                            cmd.AppendLine("    WHERE ");
                            cmd.AppendLine(cabNotesSearchString);
                            cmd.AppendLine(") AS CNS ON E.EventId=CNS.EventId AND E.EventTypeId=CNS.EventTypeId ");
                        }
                    }
                    
                    cmd.AppendLine(" LEFT JOIN (");
                    cmd.AppendLine("    SELECT EventId, EventTypeId, Count(*) AS CabCount ");
                    cmd.AppendLine("    FROM Cabs AS CC ");
                    cmd.AppendLine("    GROUP BY CC.EventId, CC.EventTypeId ");
                    cmd.AppendLine(") AS CABCOUNTER ON E.EventId=CABCOUNTER.EventId AND E.EventTypeId=CABCOUNTER.EventTypeId ");


                    String eventSearchString = criteria.ToSqlString(StackHashObjectType.Event, "E");
                    String eventSignatureSearchString = criteria.ToSqlString(StackHashObjectType.EventSignature, "E");

                    if (!String.IsNullOrEmpty(eventSearchString) || !String.IsNullOrEmpty(eventSignatureSearchString) ||
                        (fileIds.Count > 0))
                    {
                        cmd.AppendLine("WHERE ");
                    }

                    bool addAnAnd = false;
                    if (!String.IsNullOrEmpty(eventSignatureSearchString))
                    {
                        if (!String.IsNullOrEmpty(eventSearchString))
                            eventSearchString += " AND " + eventSignatureSearchString;
                        else
                            eventSearchString += eventSignatureSearchString;
                    }

                    if (fileIds.Count > 0)
                    {
                        if (addAnAnd)
                            cmd.Append(" AND ");
                        cmd.Append("FE.FileId IN ");
                        cmd.Append(convertListToSet(fileIds));
                        addAnAnd = true;
                    }

                    if (!String.IsNullOrEmpty(eventSearchString))
                    {
                        if (addAnAnd)
                            cmd.Append(" AND ");
                        cmd.Append(eventSearchString);
                        addAnAnd = true;
                    }

                    cmd.Append(") ");
                }

                cmd.AppendLine(", CRGROUPED AS (");
                cmd.AppendLine("SELECT EventId, DateCreatedLocal, DateModifiedLocal, ApplicationName, ApplicationVersion, ApplicationTimeStamp, TotalHits, ");
                cmd.AppendLine("ModuleName, ModuleVersion, ModuleTimeStamp, Offset, ExceptionCode, OffsetOriginal, ExceptionCodeOriginal, EventTypeId, EventTypeName, ");
                cmd.AppendLine("BugId, PlugInBugId, WorkFlowStatus, WorkFlowStatusName, MAX(FileId) AS FileId, CabCount, SUM(DISTINCT CriteriaNumber) AS CriteriaMap ");
                cmd.AppendLine(" FROM ");

                // Add the criteria CTEs.
                cmd.AppendLine(" ( ");
                for (int criteriaCount = 0; criteriaCount < criteriaNumber; criteriaCount++)
                {
                    if (criteriaCount > 0)
                        cmd.AppendLine(" UNION "); // DISTINCT is the default.

                    cmd.AppendLine("SELECT ");
                    cmd.Append("CR");
                    cmd.Append(criteriaCount + 1);
                    cmd.Append(".* FROM CR");
                    cmd.Append(criteriaCount + 1);
                }
                cmd.AppendLine(" ) ");
                cmd.AppendLine(" AS CRALL ");
                cmd.AppendLine(" GROUP BY EventId, DateCreatedLocal, DateModifiedLocal, ApplicationName, ApplicationVersion, ApplicationTimeStamp, TotalHits, ");
                cmd.AppendLine(" ModuleName, ModuleVersion, ModuleTimeStamp, Offset, ExceptionCode, OffsetOriginal, ExceptionCodeOriginal, EventTypeId, EventTypeName, ");
                cmd.AppendLine(" BugId, PlugInBugId, WorkFlowStatus, WorkFlowStatusName, CabCount ");
                                
                cmd.AppendLine(") ");

                
                
                cmd.AppendLine(", AE1 AS (");
                cmd.AppendLine("SELECT CRGROUPED.*, ");
                cmd.Append(
                    " ROW_NUMBER() OVER (ORDER BY " +
                    sortOptions.ToSqlString("CRGROUPED", true) +
                    ") AS RowNumber ");

                cmd.AppendLine(" FROM CRGROUPED ");
                cmd.AppendLine(") ");

                // Add in a column for the max row number and CriteriaMap.
                cmd.AppendLine(", AE AS (");
                cmd.AppendLine("SELECT AE1.*, MAXROW.MaxRowNumber ");
                cmd.AppendLine("FROM AE1 ");
                cmd.AppendLine("INNER JOIN (SELECT MAX(AE1.RowNumber) AS MaxRowNumber FROM AE1) AS MAXROW ON (0 < 1) ");
                cmd.AppendLine(" ) ");

                // Check if no product/files matched the criteria.
                if (criteriaNumber == 0)
                    return new StackHashEventPackageCollection();

                cmd.AppendLine(" SELECT AE.EventId, AE.DateCreatedLocal, AE.DateModifiedLocal, AE.ApplicationName, AE.ApplicationVersion, AE.ApplicationTimeStamp, AE.TotalHits, ");
                cmd.AppendLine("       AE.ModuleName, AE.ModuleVersion, AE.ModuleTimeStamp, AE.Offset, AE.ExceptionCode, AE.OffsetOriginal, AE.ExceptionCodeOriginal, AE.EventTypeName, ");
                cmd.AppendLine("       AE.BugId, AE.PlugInBugId, AE.FileId, AE.WorkFlowStatus, AE.WorkFlowStatusName, AE.MaxRowNumber, AE.RowNumber, AE.CabCount, AE.CriteriaMap ");
                cmd.AppendLine(" FROM AE ");
                cmd.AppendLine(" WHERE (AE.RowNumber>=@StartRow AND AE.RowNumber < (@StartRow + @NumberOfRows)) ");
                cmd.AppendLine("      OR ((@StartRow=0x7fffffff) AND (AE.RowNumber>AE.MaxRowNumber-@NumberOfRows) AND (AE.RowNumber<=AE.MaxRowNumber)) ");
                cmd.AppendLine(" ORDER BY " + sortOptions.ToSqlString("AE", true)); // Use virtual so the table name is not changed.

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = cmd.ToString();
                m_SqlUtils.AddParameter(sqlCommand, "@StartRow", DbType.Int64, startRow);
                m_SqlUtils.AddParameter(sqlCommand, "@NumberOfRows", DbType.Int64, numberOfRows);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashEventPackageCollection allEvents = new StackHashEventPackageCollection();

                try
                {
                    long totalSqlRows = 0;

                    while (reader.Read())
                    {
                        StackHashEvent retrievedEvent = readEvent(reader, 0);

                        // Read in any additional fields.
                        long fileId = (long)reader["FileId"];
                        retrievedEvent.FileId = (int)fileId;

                        long rowNumber = (long)reader["RowNumber"];

                        // All rows have the same max row number so just return one of them.
                        if (totalSqlRows == 0)
                            totalSqlRows = (long)reader["MaxRowNumber"];

                        int criteriaMap = (int)reader["CriteriaMap"]; 

                        int productId = allFileIds.FindFile(retrievedEvent.FileId).ProductId;

                        StackHashEventPackage eventPackage = new StackHashEventPackage(null, null, retrievedEvent, productId);
                        eventPackage.RowNumber = rowNumber;
                        eventPackage.CriteriaMatchMap = criteriaMap;

                        if (allEvents.FindEventPackage(retrievedEvent.Id, retrievedEvent.EventTypeName) == null)
                            allEvents.Add(eventPackage);
                    }

                    allEvents.TotalRows = totalSqlRows;
                    allEvents.MaximumSqlRows = totalSqlRows;

                    return allEvents;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetWindow);
            }
        }

        
        /// <summary>
        /// Determines if a event name exists.
        /// </summary>
        /// <param name="theEvent">The event to check.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool EventExists(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_EventExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfEventExists);
            }
        }

        #endregion EventMethods

        #region EventTypeMethods

        /// <summary>
        /// Gets the event type with the specified name.
        /// </summary>
        /// <param name="eventTypeName">Name of the event whose ID is required.</param>
        /// <returns>ID of the event entry.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public short GetEventTypeId(String eventTypeName)
        {
            if (eventTypeName == null)
                throw new ArgumentNullException("eventTypeName");

            try
            {
                // Check the cache first.
                short eventTypeId = m_EventTypeCache.GetEventTypeId(eventTypeName);

                if (eventTypeId != -1)
                    return eventTypeId;

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetEventTypeIdSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeName", DbType.String, eventTypeName);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        short id = (short)reader["EventTypeId"];
                        m_EventTypeCache.Add(new SqlEventType(id, eventTypeName));
                        return id;
                    }
                    else
                    {
                        return -1;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEventTypeId);
            }
        }

        
        /// <summary>
        /// Gets the event type with the specified id.
        /// </summary>
        /// <param name="theEvent">Then event whose type is required.</param>
        /// <returns>The event type.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public String GetEventType(int eventTypeId)
        {
            try
            {
                // Check the cache first.
                String eventTypeName = m_EventTypeCache.GetEventTypeName((short)eventTypeId);

                if (!String.IsNullOrEmpty(eventTypeName))
                    return eventTypeName;

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetEventTypeSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        return (String)reader["EventTypeName"];
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEventType);
            }
        }


        /// <summary>
        /// Determines if an event type name exists.
        /// </summary>
        /// <param name="eventTypeName">Event type name.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool EventTypeExists(String eventTypeName)
        {
            if (eventTypeName == null)
                throw new ArgumentNullException("eventTypeName");

            try
            {
                if (m_EventTypeCache.GetEventTypeId(eventTypeName) != -1)
                    return true;

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_EventTypeExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeName", DbType.String, eventTypeName);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfEventTypeExists);
            }
        }


        /// <summary>
        /// Adds an EventTypeName to the database.
        /// This call assumes that the event is not currently present.
        /// </summary>
        /// <param name="eventTypeName">Event type to add.</param>
        /// <returns>The assigned event type id.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public short AddEventType(String eventTypeName)
        {
            if (eventTypeName == null)
                throw new ArgumentNullException("eventTypeName");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddEventTypeSql;

                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeName", DbType.String, eventTypeName);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                short eventTypeId = GetEventTypeId(eventTypeName);

                return eventTypeId;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddEventType);
            }
        }


        #endregion EventTypeMethods

        #region FileEvents

        /// <summary>
        /// Adds a file event to the database.
        /// This call assumes that the file event is not currently present.
        /// The file event table contains a list of all events associated with the file.
        /// Note a file may be associated with more than one product.
        /// </summary>
        /// <param name="file">File to add.</param>
        /// <param name="theEvent">Event to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool AddFileEvent(StackHashFile file, StackHashEvent theEvent)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddFileEventSql;

                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, file.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, GetEventTypeId(theEvent.EventTypeName));

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return true;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddFileEvent);
            }
        }


        /// <summary>
        /// Determines if a file exists with the specified event ID and event type ID.
        /// </summary>
        /// <param name="eventId">Event ID to check for.</param>
        /// <param name="eventTypeId">Event type ID to check for.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool FileEventExists(StackHashFile file, StackHashEvent theEvent)
        {
            if (file == null)
                throw new ArgumentNullException("file");
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_FileEventExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, file.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, GetEventTypeId(theEvent.EventTypeName));

                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfFileEventExists);
            }
        }

        #endregion FileEvents

        #region EventNotesMethods

        /// <summary>
        /// Adds a note to the specified event.
        /// </summary>
        /// <param name="theEvent">The event to which a note is to be added.</param>
        /// <param name="note">The note to add.</param>
        /// <returns>The ID of the event note.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public int AddEventNote(StackHashEvent theEvent, StackHashNoteEntry note)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (note == null)
                throw new ArgumentNullException("note");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddEventNoteSql;

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                int userId = GetUserId(note.User);
                int sourceId = GetSourceId(note.Source);

                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@TimeOfEntry", DbType.DateTime, note.TimeOfEntry);
                m_SqlUtils.AddParameter(sqlCommand, "@SourceId", DbType.Int16, sourceId);
                m_SqlUtils.AddParameter(sqlCommand, "@UserId", DbType.Int16, userId);
                m_SqlUtils.AddParameter(sqlCommand, "@Note", DbType.String, note.Note);

                Object value = m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                if (value == DBNull.Value)
                    return -1;
                else
                    return (int)value;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddEventNote);
            }
        }

        /// <summary>
        /// Deletes an event note.
        /// </summary>
        /// <param name="theEvent">The event to which a note is to deleted from.</param>
        /// <param name="noteId">The note to delete.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void DeleteEventNote(StackHashEvent theEvent, int noteId)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_DeleteEventNoteSql;

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);

                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@NoteId", DbType.Int16, noteId);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToDeleteEventNote);
            }
        }


        /// <summary>
        /// Updates the specified event note.
        /// </summary>
        /// <param name="theEvent">The event to which a note is to be added.</param>
        /// <param name="note">The note to update.</param>
        /// <returns>The ID of the event note.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public int UpdateEventNote(StackHashEvent theEvent, StackHashNoteEntry note)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (note == null)
                throw new ArgumentNullException("note");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_UpdateEventNoteSql;

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                int userId = GetUserId(note.User);
                int sourceId = GetSourceId(note.Source);

                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@TimeOfEntry", DbType.DateTime, note.TimeOfEntry);
                m_SqlUtils.AddParameter(sqlCommand, "@SourceId", DbType.Int16, sourceId);
                m_SqlUtils.AddParameter(sqlCommand, "@UserId", DbType.Int16, userId);
                m_SqlUtils.AddParameter(sqlCommand, "@Note", DbType.String, note.Note);
                m_SqlUtils.AddParameter(sqlCommand, "@NoteId", DbType.String, note.NoteId);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return note.NoteId;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateEventNote);
            }
        }


        /// <summary>
        /// Gets all of the notes associated with a particular event.
        /// </summary>
        /// <param name="theEvent">Event for which the notes are required.</param>
        /// <returns>Notes for the specified event.</returns>
        public StackHashNotes GetEventNotes(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetEventNotesSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashNotes allNotes = new StackHashNotes();

                try
                {
                    while (reader.Read())
                    {
                        DateTime timeOfEntry = (DateTime)reader["TimeOfEntry"];
                        timeOfEntry = new DateTime(timeOfEntry.Year, timeOfEntry.Month, timeOfEntry.Day, timeOfEntry.Hour, timeOfEntry.Minute, timeOfEntry.Second, DateTimeKind.Utc);
                        String source = (String)reader["Source"];
                        String user = (String)reader["StackHashUser"];
                        String note = (String)reader["Note"];
                        int noteId = (int)reader["NoteId"];

                        StackHashNoteEntry noteEntry = new StackHashNoteEntry(timeOfEntry, source, user, note, noteId);


                        allNotes.Add(noteEntry);
                    }

                    return allNotes;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEventNotes);
            }
        }

        /// <summary>
        /// Gets the specified event note.
        /// </summary>
        /// <param name="noteId">Id of the note to retrieve.</param>
        /// <returns>The retrieved note entry or null.</returns>
        public StackHashNoteEntry GetEventNote(int noteId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetEventNoteSql;
                m_SqlUtils.AddParameter(sqlCommand, "@NoteId", DbType.Int32, noteId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        DateTime timeOfEntry = (DateTime)reader["TimeOfEntry"];
                        timeOfEntry = new DateTime(timeOfEntry.Year, timeOfEntry.Month, timeOfEntry.Day, timeOfEntry.Hour, timeOfEntry.Minute, timeOfEntry.Second, DateTimeKind.Utc);
                        String source = (String)reader["Source"];
                        String user = (String)reader["StackHashUser"];
                        String note = (String)reader["Note"];

                        StackHashNoteEntry noteEntry = new StackHashNoteEntry(timeOfEntry, source, user, note);

                        return noteEntry;
                    }

                    return null;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEventNotes);
            }
        }

        #endregion EventNotesMethods

        #region EventInfoMethods

        /// <summary>
        /// Adds an event info for the specified event.
        /// </summary>
        /// <param name="theEvent">The event to which hit data is to be added.</param>
        /// <param name="eventInfo">The hit data to add.</param>
        /// <returns>True - success, False - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool AddEventInfo(StackHashEvent theEvent, StackHashEventInfo eventInfo)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (eventInfo == null)
                throw new ArgumentNullException("eventInfo");

            try
            {
                // Add the OS to the OS table if necessary.
                short osId = GetOperatingSystemId(eventInfo.OperatingSystemName, eventInfo.OperatingSystemVersion);

                if (osId == -1)
                {
                    AddOperatingSystem(eventInfo.OperatingSystemName, eventInfo.OperatingSystemVersion);
                    osId = GetOperatingSystemId(eventInfo.OperatingSystemName, eventInfo.OperatingSystemVersion);
                }

                // Add the locale to the locales table if necessary.
                if (!LocaleExists(eventInfo.Lcid))
                    AddLocale(eventInfo.Lcid, eventInfo.Locale, eventInfo.Language);

                short eventTypeId = GetEventTypeId(theEvent.EventTypeName);

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddEventInfoSql;

                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, eventInfo.DateCreatedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, eventInfo.DateModifiedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@HitDateLocal", DbType.DateTime, eventInfo.HitDateLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@LocaleId", DbType.Int16, (short)eventInfo.Lcid);
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemId", DbType.Int16, osId);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalHits", DbType.Int32, eventInfo.TotalHits);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return true;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddEventInfo);
            }
        }


        /// <summary>
        /// Adds event infos for the specified event.
        /// The event infos cannot currently exist.
        /// </summary>
        /// <param name="theEvent">The event to which hit data is to be added.</param>
        /// <param name="eventInfos">The hit data to add.</param>
        /// <returns>True - success, False - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool AddEventInfos(StackHashEvent theEvent, StackHashEventInfoCollection eventInfos)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (eventInfos == null)
                throw new ArgumentNullException("eventInfos");
            if (eventInfos.Count == 0)
                return true;

            try
            {
                //int blockNumber = 1;

                //DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                // TODO: Turn this into a single SQL statement to add all event infos at the same time.
                foreach (StackHashEventInfo eventInfo in eventInfos)
                {
                    AddEventInfo(theEvent, eventInfo);
                    //// Add the OS to the OS table if necessary.
                    //short osId = GetOperatingSystemId(eventInfo.OperatingSystemName, eventInfo.OperatingSystemVersion);

                    //if (osId == -1)
                    //{
                    //    AddOperatingSystem(eventInfo.OperatingSystemName, eventInfo.OperatingSystemVersion);
                    //    osId = GetOperatingSystemId(eventInfo.OperatingSystemName, eventInfo.OperatingSystemVersion);
                    //}

                    //// Add the locale to the locales table if necessary.
                    //if (!LocaleExists(eventInfo.Lcid))
                    //    AddLocale(eventInfo.Lcid, eventInfo.Locale, eventInfo.Language);

                    //short eventTypeId = GetEventTypeId(theEvent.EventTypeName);

                    //sqlCommand.CommandText += String.Format(CultureInfo.InvariantCulture, s_AddEventInfosSql, blockNumber);

                    //String block = blockNumber.ToString(CultureInfo.InvariantCulture);

                    //m_SqlUtils.AddParameter(sqlCommand, "@EventId" + block, DbType.Int64, theEvent.Id);
                    //m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId" + block, DbType.Int16, eventTypeId);
                    //m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal" + block, DbType.DateTime, eventInfo.DateCreatedLocal);
                    //m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal" + block, DbType.DateTime, eventInfo.DateModifiedLocal);
                    //m_SqlUtils.AddParameter(sqlCommand, "@HitDateLocal" + block, DbType.DateTime, eventInfo.HitDateLocal);
                    //m_SqlUtils.AddParameter(sqlCommand, "@LocaleId" + block, DbType.Int16, (short)eventInfo.Lcid);
                    //m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemId" + block, DbType.Int16, osId);
                    //m_SqlUtils.AddParameter(sqlCommand, "@TotalHits" + block, DbType.Int32, eventInfo.TotalHits);

                    //blockNumber++;
                }

                //            m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);



                return true;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddEventInfos);
            }
        }


        /// <summary>
        /// Gets the event info for a particular event.
        /// </summary>
        /// <param name="theEvent">The event for which the event infos are required.</param>
        /// <returns>Event infos for the specified event.</returns>
        public StackHashEventInfoCollection GetEventInfoCollection(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetEventInfosSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashEventInfoCollection allEventInfos = new StackHashEventInfoCollection();

                try
                {
                    while (reader.Read())
                    {
                        DateTime dateCreatedLocal = (DateTime)reader["DateCreatedLocal"];
                        dateCreatedLocal = new DateTime(dateCreatedLocal.Year, dateCreatedLocal.Month, dateCreatedLocal.Day, dateCreatedLocal.Hour, dateCreatedLocal.Minute, dateCreatedLocal.Second, DateTimeKind.Utc);

                        DateTime dateModifiedLocal = (DateTime)reader["DateModifiedLocal"];
                        dateModifiedLocal = new DateTime(dateModifiedLocal.Year, dateModifiedLocal.Month, dateModifiedLocal.Day, dateModifiedLocal.Hour, dateModifiedLocal.Minute, dateModifiedLocal.Second, DateTimeKind.Utc);

                        DateTime hitDateLocal = (DateTime)reader["HitDateLocal"];
                        hitDateLocal = new DateTime(hitDateLocal.Year, hitDateLocal.Month, hitDateLocal.Day, hitDateLocal.Hour, hitDateLocal.Minute, hitDateLocal.Second, DateTimeKind.Utc);

                        int totalHits = (int)reader["TotalHits"];
                        int localeId = (short)reader["LocaleId"];
                        String locale = (String)reader["LocaleCode"];
                        String language = (String)reader["LocaleName"];
                        String osName = (String)reader["OperatingSystemName"];
                        String osVersion = (String)reader["OperatingSystemVersion"];

                        StackHashEventInfo eventInfo = new StackHashEventInfo(dateCreatedLocal, dateModifiedLocal, hitDateLocal, language, localeId, locale, osName, osVersion, totalHits);
                        allEventInfos.Add(eventInfo);
                    }

                    return allEventInfos;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEventInfos);
            }
        }


        /// <summary>
        /// Gets the event info for a set of events.
        /// </summary>
        /// <param name="allEvents">The events for which the event infos are required.</param>
        /// <returns>Event infos for all the specified events ordered by eventId/eventtypeid</returns>
        [SuppressMessage("Microsoft.Design", "CA1002")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashEventInfoPackageCollection GetEventInfoPackageCollection(List<StackHashEventPackage> theEvents)
        {
            if (theEvents == null)
                throw new ArgumentNullException("theEvents");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                String s_GetEventInfoPackagesSql =
                    "SELECT EI.DateCreatedLocal, EI.DateModifiedLocal, EI.HitDateLocal, EI.LocaleId, L.LocaleCode, L.LocaleName, EI.TotalHits, " +
                    "       ET.EventTypeName, OS.OperatingSystemName, OS.OperatingSystemVersion, EI.EventId, ET.EventTypeName " +
                    "FROM dbo.EventInfos AS EI " +
                    "INNER JOIN dbo.EventTypes AS ET ON EI.EventTypeId=ET.EventTypeId ";

                StringBuilder sqlCommandText = new StringBuilder(s_GetEventInfoPackagesSql);
                sqlCommandText.Append(" AND (");

                bool firstEvent = true;
                foreach (StackHashEventPackage currentEvent in theEvents)
                {
                    if (!firstEvent)
                        sqlCommandText.Append(" OR ");
                    firstEvent = false;

                    if (theEvents.Count != 1)
                        sqlCommandText.Append("(");

                    sqlCommandText.Append("EI.EventId=");
                    sqlCommandText.Append(currentEvent.EventData.Id.ToString(CultureInfo.InvariantCulture));
                    sqlCommandText.Append(" AND ET.EventTypeName='");
                    sqlCommandText.Append(currentEvent.EventData.EventTypeName);
                    sqlCommandText.Append("'");
                    if (theEvents.Count != 1)
                        sqlCommandText.Append(")");
                }

                sqlCommandText.Append(") ");
                sqlCommandText.Append("INNER JOIN OperatingSystems AS OS ON EI.OperatingSystemId=OS.OperatingSystemId ");
                sqlCommandText.Append("INNER JOIN Locales AS L ON EI.LocaleId=L.LocaleId ");
                sqlCommandText.Append(" ORDER BY EI.EventId, EI.EventTypeId, EI.HitDateLocal, EI.LocaleId, OS.OperatingSystemId ");

                sqlCommand.CommandText = sqlCommandText.ToString();


                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashEventInfoPackageCollection allEventInfos = new StackHashEventInfoPackageCollection();

                try
                {
                    while (reader.Read())
                    {
                        DateTime dateCreatedLocal = (DateTime)reader["DateCreatedLocal"];
                        dateCreatedLocal = new DateTime(dateCreatedLocal.Year, dateCreatedLocal.Month, dateCreatedLocal.Day, dateCreatedLocal.Hour, dateCreatedLocal.Minute, dateCreatedLocal.Second, DateTimeKind.Utc);

                        DateTime dateModifiedLocal = (DateTime)reader["DateModifiedLocal"];
                        dateModifiedLocal = new DateTime(dateModifiedLocal.Year, dateModifiedLocal.Month, dateModifiedLocal.Day, dateModifiedLocal.Hour, dateModifiedLocal.Minute, dateModifiedLocal.Second, DateTimeKind.Utc);

                        DateTime hitDateLocal = (DateTime)reader["HitDateLocal"];
                        hitDateLocal = new DateTime(hitDateLocal.Year, hitDateLocal.Month, hitDateLocal.Day, hitDateLocal.Hour, hitDateLocal.Minute, hitDateLocal.Second, DateTimeKind.Utc);

                        int totalHits = (int)reader["TotalHits"];
                        int localeId = (short)reader["LocaleId"];
                        String locale = (String)reader["LocaleCode"];
                        String language = (String)reader["LocaleName"];
                        String osName = (String)reader["OperatingSystemName"];
                        String osVersion = (String)reader["OperatingSystemVersion"];
                        String eventTypeName = (String)reader["EventTypeName"];
                        long eventId = (long)reader["EventId"];

                        StackHashEventInfo eventInfo = new StackHashEventInfo(dateCreatedLocal, dateModifiedLocal, hitDateLocal, language, localeId, locale, osName, osVersion, totalHits);
                        allEventInfos.Add(new StackHashEventInfoPackage((int)eventId, eventTypeName, eventInfo));
                    }

                    return allEventInfos;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetEventInfoPackageCollection);
            }
        }

        /// <summary>
        /// Get the most recent hit date.
        /// </summary>
        /// <param name="theEvent">The event for which the hit date is required.</param>
        /// <returns>Most recent hit date.</returns>
        public DateTime GetMostRecentHitDate(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetMostRecentHitDateSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                Object value = m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                if (value == DBNull.Value)
                {
                    return new DateTime(0, DateTimeKind.Utc);
                }
                else
                {
                    DateTime mostRecentHitDate = (DateTime)value;
                    mostRecentHitDate = new DateTime(mostRecentHitDate.Year, mostRecentHitDate.Month, mostRecentHitDate.Day, mostRecentHitDate.Hour, mostRecentHitDate.Minute, mostRecentHitDate.Second, DateTimeKind.Utc);
                    return mostRecentHitDate;
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetMostRecentHitDate);
            }
        }

        /// <summary>
        /// Get the hit count for a particular event.
        /// </summary>
        /// <param name="theEvent">The event for which the hit count is required.</param>
        /// <returns>Hit count</returns>
        public int GetHitCount(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetHitCountSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                Object value = m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                if (value == DBNull.Value)
                    return 0;
                else
                    return (int)value;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetHitCount);
            }
        }

        #endregion EventInfoMethods

        #region OperatingSystemMethods

        /// <summary>
        /// Gets the OS type ID with the specified name.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        /// <returns>ID of the OS entry.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public short GetOperatingSystemId(String operatingSystemName, String operatingSystemVersion)
        {
            try
            {
                // Check the cache first.
                short operatingSystemId = m_OperatingSystemCache.GetId(new SqlOperatingSystem(0, operatingSystemName, operatingSystemVersion));

                if (operatingSystemId != -1)
                    return operatingSystemId;

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetOperatingSystemIdSql;
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemName", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemName));
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemVersion", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemVersion));

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        short id = (short)reader["OperatingSystemId"];
                        m_OperatingSystemCache.Add(new SqlOperatingSystem(id, operatingSystemName, operatingSystemVersion));
                        return id;
                    }
                    else
                    {
                        // Not found.
                        return -1;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetOperatingSystemId);
            }
        }


        /// <summary>
        /// Gets the OS with the specified id.
        /// </summary>
        /// <param name="operatingSystemId">Operating system ID to get.</param>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        [SuppressMessage("Microsoft.Design", "CA1021")]
        public void GetOperatingSystemDetails(int operatingSystemId, out String operatingSystemName, out String operatingSystemVersion)
        {
            try
            {
                // Check the cache first.
                SqlOperatingSystem operatingSystem = m_OperatingSystemCache.GetOperatingSystem((short)operatingSystemId);
                if (operatingSystem != null)
                {
                    operatingSystemName = operatingSystem.OperatingSystemName;
                    operatingSystemVersion = operatingSystem.OperatingSystemVersion;
                }

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetOperatingSystemTypeSql;
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemId", DbType.Int16, operatingSystemId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        operatingSystemName = SqlUtils.GetNullableString(reader["OperatingSystemName"]);
                        operatingSystemVersion = SqlUtils.GetNullableString(reader["OperatingSystemVersion"]);
                    }
                    else
                    {
                        operatingSystemName = String.Empty;
                        operatingSystemVersion = String.Empty;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetOperatingSystemDetails);
            }
        }


        /// <summary>
        /// Determines if the OS exists or not.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool OperatingSystemExists(String operatingSystemName, String operatingSystemVersion)
        {
            try
            {
                // Check the cache first.
                if (m_OperatingSystemCache.GetId(new SqlOperatingSystem(0, operatingSystemName, operatingSystemVersion)) != -1)
                    return true;

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_OperatingSystemExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemName", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemName));
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemVersion", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemVersion));
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfOperatingSystemExists);
            }
        }


        /// <summary>
        /// Adds an operating system.
        /// </summary>
        /// <param name="operatingSystemName">Operating system name.</param>
        /// <param name="operatingSystemVersion">Operating system version.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddOperatingSystem(String operatingSystemName, String operatingSystemVersion)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddOperatingSystemSql;

                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemName", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemName));
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemVersion", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemVersion));

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddOperatingSystem);
            }
        }

        #endregion OperatingSystemMethods

        #region LocaleMethods


        private void primeLocaleCache()
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetLocalesSql;

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        short localeId = (short)reader["LocaleId"];
                        String localeCode = SqlUtils.GetNullableString(reader["LocaleCode"]);
                        String localeName = SqlUtils.GetNullableString(reader["LocaleName"]);

                        m_LocaleCache.Add(new SqlLocale(localeId, localeCode, localeName));
                    }

                    m_LocaleCache.Primed = true;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToPrimeLocaleCache);
            }
        }

        /// <summary>
        /// Determines if a locale ID exists.
        /// </summary>
        /// <param name="localeId">ID of the locale to check.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool LocaleExists(int localeId)
        {
            try
            {
                // Check the cache first.
                if (!m_LocaleCache.Primed)
                    primeLocaleCache();

                if (m_LocaleCache.GetLocale((short)localeId) != null)
                    return true;

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_LocaleExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@LocaleId", DbType.Int16, (short)localeId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToDetermineIfLocaleExists);
            }
        }


        /// <summary>
        /// Adds a locale to the database.
        /// </summary>
        /// <param name="localeId">ID of the locale.</param>
        /// <param name="localeCode">Locale code.</param>
        /// <param name="localeName">Locale name.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddLocale(int localeId, String localeCode, String localeName)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddLocaleSql;

                m_SqlUtils.AddParameter(sqlCommand, "@LocaleId", DbType.Int16, (short)localeId);
                m_SqlUtils.AddParameter(sqlCommand, "@LocaleCode", DbType.String, SqlUtils.MakeSqlCompliantString(localeCode));
                m_SqlUtils.AddParameter(sqlCommand, "@LocaleName", DbType.String, SqlUtils.MakeSqlCompliantString(localeName));

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                m_LocaleCache.Add(new SqlLocale((short)localeId, localeCode, localeName));

                return;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddLocale);
            }
        }

        #endregion LocaleMethods

        #region CabMethods

        /// <summary>
        /// Adds a Cab to the database.
        /// This call assumes that the file is not currently present.
        /// </summary>
        /// <param name="theEvent">Event that owns the cab.</param>
        /// <param name="cab">Cab to add.</param>
        /// <returns>The cab added.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashCab AddCab(StackHashEvent theEvent, StackHashCab cab)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddCabAllSql;

                // Get the corresponding event type id.
                short eventTypeId = GetEventTypeId(cab.EventTypeName);

                if (eventTypeId == -1)
                {
                    AddEventType(cab.EventTypeName);
                    eventTypeId = GetEventTypeId(cab.EventTypeName);
                }

                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, cab.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@CabFileName", DbType.String, cab.FileName);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, cab.DateCreatedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, cab.DateModifiedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@SizeInBytes", DbType.Int64, cab.SizeInBytes);
                m_SqlUtils.AddParameter(sqlCommand, "@CabDownloaded", DbType.Byte, cab.CabDownloaded ? 1 : 0);
                m_SqlUtils.AddParameter(sqlCommand, "@Purged", DbType.Byte, cab.Purged ? 1 : 0);

                // Dump analysis may be null.
                if (cab.DumpAnalysis != null)
                {
                    m_SqlUtils.AddParameter(sqlCommand, "@SystemUpTime", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.SystemUpTime));
                    m_SqlUtils.AddParameter(sqlCommand, "@ProcessUpTime", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.ProcessUpTime));
                    m_SqlUtils.AddParameter(sqlCommand, "@OSVersion", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.OSVersion));
                    m_SqlUtils.AddParameter(sqlCommand, "@MachineArchitecture", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.MachineArchitecture));
                    m_SqlUtils.AddParameter(sqlCommand, "@DotNetVersion", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.DotNetVersion));
                }
                else
                {
                    m_SqlUtils.AddParameter(sqlCommand, "@SystemUpTime", DbType.String, DBNull.Value);
                    m_SqlUtils.AddParameter(sqlCommand, "@ProcessUpTime", DbType.String, DBNull.Value);
                    m_SqlUtils.AddParameter(sqlCommand, "@OSVersion", DbType.String, DBNull.Value);
                    m_SqlUtils.AddParameter(sqlCommand, "@MachineArchitecture", DbType.String, DBNull.Value);
                    m_SqlUtils.AddParameter(sqlCommand, "@DotNetVersion", DbType.String, DBNull.Value);
                }
                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return cab;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddCab);
            }
        }


        /// <summary>
        /// Updates the specified cab.
        /// This call assumes that the cab IS present.
        /// </summary>
        /// <param name="theEvent">Event that owns the cab.</param>
        /// <param name="cab">Cab to add.</param>
        /// <param name="setDiagnosticInfo">True - updates the diagnostic fields.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void UpdateCab(StackHashEvent theEvent, StackHashCab cab, bool setDiagnosticInfo)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");
            if (cab == null)
                throw new ArgumentNullException("cab");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();


                if (setDiagnosticInfo)
                    sqlCommand.CommandText = s_UpdateCabSqlAll;
                else
                    sqlCommand.CommandText = s_UpdateCabSqlJustWinQual;

                // Get the corresponding event type id.
                short eventTypeId = GetEventTypeId(cab.EventTypeName);

                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, cab.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@CabFileName", DbType.String, cab.FileName);
                m_SqlUtils.AddParameter(sqlCommand, "@DateCreatedLocal", DbType.DateTime, cab.DateCreatedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@DateModifiedLocal", DbType.DateTime, cab.DateModifiedLocal);
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@SizeInBytes", DbType.Int64, cab.SizeInBytes);
                m_SqlUtils.AddParameter(sqlCommand, "@CabDownloaded", DbType.Byte, cab.CabDownloaded ? 1 : 0);
                m_SqlUtils.AddParameter(sqlCommand, "@Purged", DbType.Byte, cab.Purged ? 1 : 0);

                if (setDiagnosticInfo)
                {
                    if (cab.DumpAnalysis != null)
                    {
                        m_SqlUtils.AddParameter(sqlCommand, "@SystemUpTime", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.SystemUpTime));
                        m_SqlUtils.AddParameter(sqlCommand, "@ProcessUpTime", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.ProcessUpTime));
                        m_SqlUtils.AddParameter(sqlCommand, "@OSVersion", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.OSVersion));
                        m_SqlUtils.AddParameter(sqlCommand, "@MachineArchitecture", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.MachineArchitecture));
                        m_SqlUtils.AddParameter(sqlCommand, "@DotNetVersion", DbType.String, SqlUtils.MakeSqlCompliantString(cab.DumpAnalysis.DotNetVersion));
                    }
                    else
                    {
                        m_SqlUtils.AddParameter(sqlCommand, "@SystemUpTime", DbType.String, DBNull.Value);
                        m_SqlUtils.AddParameter(sqlCommand, "@ProcessUpTime", DbType.String, DBNull.Value);
                        m_SqlUtils.AddParameter(sqlCommand, "@OSVersion", DbType.String, DBNull.Value);
                        m_SqlUtils.AddParameter(sqlCommand, "@MachineArchitecture", DbType.String, DBNull.Value);
                        m_SqlUtils.AddParameter(sqlCommand, "@DotNetVersion", DbType.String, DBNull.Value);
                    }
                }

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateCab);
            }
        }
        

        /// <summary>
        /// Determines if the cab Id exists.
        /// </summary>
        /// <param name="cabId">ID of the cab to check.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool CabExists(long cabId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_CabExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, cabId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfCabExists);
            }
        }


        /// <summary>
        /// Reads in the fields of the StackHashCab data structure from the specified dbase reader.
        /// </summary>
        /// <param name="reader">Reader containing the data.</param>
        /// <returns>Cab object.</returns>
        [SuppressMessage("Microsoft.Performance", "CA1822")]
        private StackHashCab readCabFields(DbDataReader reader)
        {
            DateTime dateCreatedLocal = (DateTime)reader["DateCreatedLocal"];
            dateCreatedLocal = new DateTime(dateCreatedLocal.Year, dateCreatedLocal.Month, dateCreatedLocal.Day, dateCreatedLocal.Hour, dateCreatedLocal.Minute, dateCreatedLocal.Second, DateTimeKind.Utc);

            DateTime dateModifiedLocal = (DateTime)reader["DateModifiedLocal"];
            dateModifiedLocal = new DateTime(dateModifiedLocal.Year, dateModifiedLocal.Month, dateModifiedLocal.Day, dateModifiedLocal.Hour, dateModifiedLocal.Minute, dateModifiedLocal.Second, DateTimeKind.Utc);

            long cabId = (long)reader["CabId"];
            long eventId = (long)reader["EventId"];
            String eventTypeName = (String)reader["EventTypeName"];
            String fileName = (String)reader["CabFileName"];
            long sizeInBytes = (long)reader["SizeInBytes"];

            byte downloaded = (byte)reader["CabDownloaded"];
            byte purged = (byte)reader["Purged"];

            StackHashCab cab = new StackHashCab(dateCreatedLocal, dateModifiedLocal, (int)eventId, eventTypeName, fileName, (int)cabId, sizeInBytes);

            cab.CabDownloaded = (downloaded == 0) ? false : true;
            cab.Purged = (purged == 0) ? false : true;

            String systemUpTime = SqlUtils.GetNullableString(reader["SystemUpTime"]);
            String processUpTime = SqlUtils.GetNullableString(reader["ProcessUpTime"]);
            String dotNetVersion = SqlUtils.GetNullableString(reader["DotNetVersion"]);
            String osVersion = SqlUtils.GetNullableString(reader["OsVersion"]);
            String machineArchitecture = SqlUtils.GetNullableString(reader["MachineArchitecture"]);

            cab.DumpAnalysis = new StackHashDumpAnalysis(systemUpTime, processUpTime, dotNetVersion, osVersion, machineArchitecture);
            return cab;
        }


        /// <summary>
        /// Gets the cab infor associated with the specified cab id.
        /// </summary>
        /// <param name="cabId">The ID of the cab required.</param>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        [SuppressMessage("Microsoft.Design", "CA1021")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashCab GetCab(long cabId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetCabSql;
                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, cabId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        StackHashCab cab = readCabFields(reader);
                        return cab;
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetCab);
            }
        }


        /// <summary>
        /// Gets the cabs for the specified event.
        /// </summary>
        /// <param name="eventId">The ID of the event for which cabs are required.</param>
        /// <returns>Retrieved cab collection.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        [SuppressMessage("Microsoft.Design", "CA1021")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashCabCollection LoadCabCollection(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetCabsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    StackHashCabCollection cabs = new StackHashCabCollection();

                    while (reader.Read())
                    {
                        StackHashCab cab = readCabFields(reader);

                        cabs.Add(cab);
                    }

                    return cabs;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToLoadCabCollection);
            }
        }


        /// <summary>
        /// Gets the cabs for the specified list of events - ordered by the event ID and eventTypeId.
        /// </summary>
        /// <param name="theEvents">The events for which the cabs are required.</param>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        [SuppressMessage("Microsoft.Design", "CA1021")]
        [SuppressMessage("Microsoft.Security", "CA2100")]
        [SuppressMessage("Microsoft.Design", "CA1002")]
        public StackHashCabCollection GetCabs(List<StackHashEventPackage> theEvents)
        {
            if (theEvents == null)
                throw new ArgumentNullException("theEvents");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                String s_GetCabsForMultipleEventsSql =
                " SELECT C.CabId, RTRIM(C.CabFileName) AS CabFileName, C.DateCreatedLocal, C.DateModifiedLocal, C.EventId, RTRIM(ET.EventTypeName) AS EventTypeName, C.SizeInBytes, C.CabDownloaded, C.Purged, " +
                "        C.SystemUpTime, C.ProcessUpTime, C.DotNetVersion, C.OSVersion, C.MachineArchitecture " +
                " FROM dbo.Cabs AS C " +
                " INNER JOIN dbo.EventTypes AS ET " +
                " ON C.EventTypeId=ET.EventTypeId ";

                StringBuilder sqlCommandText = new StringBuilder(s_GetCabsForMultipleEventsSql);
                sqlCommandText.Append(" AND (");

                bool firstEvent = true;
                foreach (StackHashEventPackage currentEvent in theEvents)
                {
                    if (!firstEvent)
                        sqlCommandText.Append(" OR ");
                    firstEvent = false;

                    if (theEvents.Count != 1)
                        sqlCommandText.Append("(");

                    sqlCommandText.Append("C.EventId=");
                    sqlCommandText.Append(currentEvent.EventData.Id.ToString(CultureInfo.InvariantCulture));
                    sqlCommandText.Append(" AND ET.EventTypeName='");
                    sqlCommandText.Append(currentEvent.EventData.EventTypeName);
                    sqlCommandText.Append("'");

                    if (theEvents.Count != 1)
                        sqlCommandText.Append(")");
                }

                sqlCommandText.Append(")");
                sqlCommandText.Append(" ORDER BY C.EventId, C.EventTypeId ");

                sqlCommand.CommandText = sqlCommandText.ToString(); ;

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    StackHashCabCollection cabs = new StackHashCabCollection();
                    while (reader.Read())
                    {
                        StackHashCab cab = readCabFields(reader);
                        cabs.Add(cab);
                    }

                    return cabs;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetCabs);
            }
        }

        
        /// <summary>
        /// Gets a count of all cabs in the database related to a particular event.
        /// </summary>
        /// <param name="theEvent">Event owning the cabs.</param>
        /// <returns>Number of cabs</returns>
        public int GetCabCount(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetCabCountSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        int cabCount = (int)reader["CabCount"];
                        return cabCount;
                    }
                    else
                    {
                        return 0;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetCabCount);
            }
        }


        /// <summary>
        /// Gets a count of all cabs in the database related to a particular event where the cab has been downloaded.
        /// </summary>
        /// <param name="theEvent">Event owning the cabs.</param>
        /// <returns>Number of downloaded cabs</returns>
        public int GetCabFileCount(StackHashEvent theEvent)
        {
            if (theEvent == null)
                throw new ArgumentNullException("theEvent");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetCabFileCountSql;
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, theEvent.Id);

                int eventTypeId = GetEventTypeId(theEvent.EventTypeName);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        int cabCount = (int)reader["CabFileCount"];
                        return cabCount;
                    }
                    else
                    {
                        return 0;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetCabFileCount);
            }
        }

        #endregion CabMethods

        #region CabNotesMethods

        /// <summary>
        /// Adds a note to the specified cab.
        /// </summary>
        /// <param name="cab">The cab to which a note is to be added.</param>
        /// <param name="note">The note to add.</param>
        /// <returns>True - cab added, False - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public int AddCabNote(StackHashCab cab, StackHashNoteEntry note)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (note == null)
                throw new ArgumentNullException("note");

            try
            {
                short sourceId = GetSourceId(note.Source);
                short userId = GetUserId(note.User);

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddCabNoteSql;

                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, cab.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@TimeOfEntry", DbType.DateTime, note.TimeOfEntry);
                m_SqlUtils.AddParameter(sqlCommand, "@SourceId", DbType.Int16, sourceId);
                m_SqlUtils.AddParameter(sqlCommand, "@UserId", DbType.Int16, userId);
                m_SqlUtils.AddParameter(sqlCommand, "@Note", DbType.String, note.Note);

                Object value = m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                if (value == DBNull.Value)
                    return -1;
                else
                    return (int)value;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddCabNote);
            }
        }

        /// <summary>
        /// Updates the specified cab note.
        /// </summary>
        /// <param name="cab">The cab to which a note is to be updated.</param>
        /// <param name="note">The note to update.</param>
        /// <returns>The ID of the event note.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public int UpdateCabNote(StackHashCab cab, StackHashNoteEntry note)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");
            if (note == null)
                throw new ArgumentNullException("note");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_UpdateCabNoteSql;

                int userId = GetUserId(note.User);
                int sourceId = GetSourceId(note.Source);


                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, cab.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@TimeOfEntry", DbType.DateTime, note.TimeOfEntry);
                m_SqlUtils.AddParameter(sqlCommand, "@SourceId", DbType.Int16, sourceId);
                m_SqlUtils.AddParameter(sqlCommand, "@UserId", DbType.Int16, userId);
                m_SqlUtils.AddParameter(sqlCommand, "@Note", DbType.String, note.Note);
                m_SqlUtils.AddParameter(sqlCommand, "@NoteId", DbType.String, note.NoteId);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return note.NoteId;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToUpdateCabNote);
            }
        }


        /// <summary>
        /// Deletes the specified cab note.
        /// </summary>
        /// <param name="cab">The cab to which a note is to be deleted.</param>
        /// <param name="noteId">The note to delete.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void DeleteCabNote(StackHashCab cab, int noteId)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_DeleteCabNoteSql;

                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, cab.Id);
                m_SqlUtils.AddParameter(sqlCommand, "@NoteId", DbType.String, noteId);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToDeleteCabNote);
            }
        }


        /// <summary>
        /// Gets all of the notes associated with a particular cab in the order they were added.
        /// </summary>
        /// <param name="cab">Cab for which the notes are required.</param>
        /// <returns>List of all notes in date order.</returns>
        public StackHashNotes GetCabNotes(StackHashCab cab)
        {
            if (cab == null)
                throw new ArgumentNullException("cab");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetCabNotesSql;
                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, cab.Id);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashNotes allNotes = new StackHashNotes();

                try
                {
                    while (reader.Read())
                    {
                        DateTime timeOfEntry = (DateTime)reader["TimeOfEntry"];
                        timeOfEntry = new DateTime(timeOfEntry.Year, timeOfEntry.Month, timeOfEntry.Day, timeOfEntry.Hour, timeOfEntry.Minute, timeOfEntry.Second, DateTimeKind.Utc);
                        String source = (String)reader["Source"];
                        String user = (String)reader["StackHashUser"];
                        String note = (String)reader["Note"];
                        int noteId = (int)reader["NoteId"];

                        StackHashNoteEntry noteEntry = new StackHashNoteEntry(timeOfEntry, source, user, note, noteId);

                        allNotes.Add(noteEntry);
                    }

                    return allNotes;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetCabNotes);
            }
        }

        /// <summary>
        /// Gets the specified cab note.
        /// </summary>
        /// <param name="noteId">The cab entry required.</param>
        /// <returns>The requested cab note or null.</returns>
        public StackHashNoteEntry GetCabNote(int noteId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetCabNoteSql;
                m_SqlUtils.AddParameter(sqlCommand, "@NoteId", DbType.Int32, noteId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        DateTime timeOfEntry = (DateTime)reader["TimeOfEntry"];
                        timeOfEntry = new DateTime(timeOfEntry.Year, timeOfEntry.Month, timeOfEntry.Day, timeOfEntry.Hour, timeOfEntry.Minute, timeOfEntry.Second, DateTimeKind.Utc);
                        String source = (String)reader["Source"];
                        String user = (String)reader["StackHashUser"];
                        String note = (String)reader["Note"];

                        StackHashNoteEntry noteEntry = new StackHashNoteEntry(timeOfEntry, source, user, note);

                        return noteEntry;
                    }

                    return null;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetCabNote);
            }
        }


        #endregion CabNotesMethods

        #region UserMethods

        /// <summary>
        /// Gets the User ID with the specified name.
        /// </summary>
        /// <param name="userName">Name of the user to get the ID for.</param>
        /// <returns>ID of the User.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public short GetUserId(String userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetUserIdSql;
                m_SqlUtils.AddParameter(sqlCommand, "@UserName", DbType.String, userName);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        short id = (short)reader["UserId"];
                        return id;
                    }
                    else
                    {
                        return -1;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetUserId);
            }
        }


        /// <summary>
        /// Determines if the user exists or not.
        /// </summary>
        /// <param name="userName">User name.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool UserExists(String userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_UserExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@UserName", DbType.String, userName);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfUserExists);
            }
        }


        /// <summary>
        /// Adds a user. Must not exist already.
        /// </summary>
        /// <param name="userName">Name of the user to get.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddUser(String userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddUserSql;

                m_SqlUtils.AddParameter(sqlCommand, "@UserName", DbType.String, userName);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddUser);
            }
        }

        #endregion UserMethods

        #region SourceMethods

        /// <summary>
        /// Gets the Source ID with the specified name.
        /// </summary>
        /// <param name="sourceName">Name of the source to get the ID for.</param>
        /// <returns>ID of the source.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public short GetSourceId(String sourceName)
        {
            if (sourceName == null)
                throw new ArgumentNullException("sourceName");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetSourceIdSql;
                m_SqlUtils.AddParameter(sqlCommand, "@SourceName", DbType.String, sourceName);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    if (reader.Read())
                    {
                        short id = (short)reader["SourceId"];
                        return id;
                    }
                    else
                    {
                        return -1;
                    }
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetSourceId);
            }
        }


        /// <summary>
        /// Determines if the source exists or not.
        /// </summary>
        /// <param name="sourceName">Source name.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool SourceExists(String sourceName)
        {
            if (sourceName == null)
                throw new ArgumentNullException("sourceName");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_SourceExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@SourceName", DbType.String, sourceName);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfSourceExists);
            }
        }


        /// <summary>
        /// Adds a source.
        /// </summary>
        /// <param name="sourceName">Name of the source to add.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddSource(String sourceName)
        {
            if (sourceName == null)
                throw new ArgumentNullException("sourceName");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_AddSourceSql;

                m_SqlUtils.AddParameter(sqlCommand, "@SourceName", DbType.String, sourceName);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddSource);
            }
        }

        #endregion SourceMethods


        #region StatisticsMethods

        /// <summary>
        /// Gets the rollup information for the languages.
        /// Each language is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full language rollup.</returns>
        public StackHashProductLocaleSummaryCollection GetLocaleSummaryFresh(int productId)
        {
            try
            {
                StackHashProductLocaleSummaryCollection newLocaleSummaryCollection = new StackHashProductLocaleSummaryCollection();

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetLocaleSummary;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        short localeId = (short)reader["LocaleId"];
                        String localeCode = (String)reader["LocaleCode"];
                        String localeName = (String)reader["LocaleName"];
                        int totalHits = (int)reader["TotalHits"];

                        StackHashProductLocaleSummary localeSummary = new StackHashProductLocaleSummary(localeName, localeId, localeCode, totalHits);

                        newLocaleSummaryCollection.Add(localeSummary);
                    }

                    return newLocaleSummaryCollection;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetLocaleSummaryFresh);
            }
        }


        /// <summary>
        /// Gets the rollup information for the operating systems.
        /// Each OS is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full OS rollup.</returns>
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaryFresh(int productId)
        {
            try
            {
                StackHashProductOperatingSystemSummaryCollection newOSSummaryCollection = new StackHashProductOperatingSystemSummaryCollection();

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetOperatingSystemSummary;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        String operatingSystemName = (String)reader["OperatingSystemName"];
                        String operatingSystemVersion = (String)reader["OperatingSystemVersion"];
                        int totalHits = (int)reader["TotalHits"];

                        StackHashProductOperatingSystemSummary localeSummary = new StackHashProductOperatingSystemSummary(operatingSystemName, operatingSystemVersion, totalHits);

                        newOSSummaryCollection.Add(localeSummary);
                    }

                    return newOSSummaryCollection;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetOperatingSystemSummaryFresh);
            }
        }


        /// <summary>
        /// Gets the rollup information for the hit dates.
        /// Each hit date is recorded once with the total hits from all eventinfos for the product.
        /// </summary>
        /// <param name="productId">Id of the product who's stats are required.</param>
        /// <returns>Full hit date rollup.</returns>
        public StackHashProductHitDateSummaryCollection GetHitDateSummaryFresh(int productId)
        {
            try
            {
                StackHashProductHitDateSummaryCollection newHitDateSummaryCollection = new StackHashProductHitDateSummaryCollection();

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetHitDateSummary;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        DateTime hitDate = (DateTime)reader["HitDateLocal"];
                        int totalHits = (int)reader["TotalHits"];

                        StackHashProductHitDateSummary localeSummary = new StackHashProductHitDateSummary(hitDate, totalHits);

                        newHitDateSummaryCollection.Add(localeSummary);
                    }

                    return newHitDateSummaryCollection;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetHitDateSummaryFresh);
            }
        }

        #endregion

        #region LocaleSummaryMethods


        /// <summary>
        /// Determines if a locale summary exists.
        /// </summary>
        /// <param name="localeId">ID of the locale to check.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool LocaleSummaryExists(int productId, int localeId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_LocaleSummaryExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@LocaleId", DbType.Int16, (short)localeId);
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfLocaleSummaryExists);
            }
        }


        /// <summary>
        /// Gets all of the locale rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashProductLocaleSummaryCollection GetLocaleSummaries(int productId)
        {
            try
            {
                StackHashProductLocaleSummaryCollection localeSummaries = new StackHashProductLocaleSummaryCollection();

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetAllLocaleSummariesSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        short localeId = (short)reader["LocaleId"];
                        String localeCode = SqlUtils.GetNullableString(reader["LocaleCode"]);
                        String localeName = SqlUtils.GetNullableString(reader["LocaleName"]);
                        long totalHits = (long)reader["TotalHits"];

                        localeSummaries.Add(new StackHashProductLocaleSummary(localeName, localeId, localeCode, totalHits));
                    }

                    return localeSummaries;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetLocaleSummaries);
            }
        }

        /// <summary>
        /// Gets a specific locale summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="localeId">ID of the locale to get.</param>
        /// <returns>Product rollup information.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashProductLocaleSummary GetLocaleSummaryForProduct(int productId, int localeId)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetLocaleSummarySql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);
                m_SqlUtils.AddParameter(sqlCommand, "@LocaleId", DbType.Int16, (short)localeId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        String localeCode = SqlUtils.GetNullableString(reader["LocaleCode"]);
                        String localeName = SqlUtils.GetNullableString(reader["LocaleName"]);
                        long totalHits = (long)reader["TotalHits"];

                        return new StackHashProductLocaleSummary(localeName, localeId, localeCode, totalHits);
                    }

                    return null;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetLocaleSummaryForProduct);
            }
        }
        
        /// <summary>
        /// Adds a locale summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose local data is to be updated.</param>
        /// <param name="localeId">ID of the locale.</param>
        /// <param name="totalHits">Running total of all hits for this locale.</param>
        /// <param name="overwrite">True - overwrites original if exists. False - adds the totals if exists already.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddLocaleSummary(int productId, int localeId, long totalHits, bool overwrite)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                if (overwrite)
                    sqlCommand.CommandText = s_AddLocaleSummaryOverwriteSql;
                else
                    sqlCommand.CommandText = s_AddLocaleSummarySql;

                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);
                m_SqlUtils.AddParameter(sqlCommand, "@LocaleId", DbType.Int16, (short)localeId);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalHits", DbType.Int64, totalHits);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddLocaleSummary);
            }
        }

        #endregion LocaleSummaryMethods

        #region OperatingSystemSummaryMethods


        /// <summary>
        /// Determines if a OS summary exists.
        /// </summary>
        /// <param name="productId">ID of the product to which the rollup data relates.</param>
        /// <param name="operatingSystemName">Name of the OS.</param>
        /// <param name="operatingSystemVersion">OS Version.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool OperatingSystemSummaryExists(int productId, String operatingSystemName, String operatingSystemVersion)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_OperatingSystemSummaryExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemName", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemName));
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemVersion", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemVersion));
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfOperatingSystemSummaryExists);
            }
        }


        /// <summary>
        /// Gets all of the OS rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashProductOperatingSystemSummaryCollection GetOperatingSystemSummaries(int productId)
        {
            try
            {
                StackHashProductOperatingSystemSummaryCollection operatingSystemSummaries = new StackHashProductOperatingSystemSummaryCollection();

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetAllOperatingSystemSummariesSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        String operatingSystemName = SqlUtils.GetNullableString(reader["OperatingSystemName"]);
                        String operatingSystemVersion = SqlUtils.GetNullableString(reader["OperatingSystemVersion"]);
                        long totalHits = (long)reader["TotalHits"];

                        operatingSystemSummaries.Add(new StackHashProductOperatingSystemSummary(operatingSystemName, operatingSystemVersion, totalHits));
                    }

                    return operatingSystemSummaries;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetOperatingSystemSummaries);
            }
        }

        /// <summary>
        /// Gets a specific OS summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="localeId">ID of the locale to get.</param>
        /// <returns>Product rollup information.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashProductOperatingSystemSummary GetOperatingSystemSummaryForProduct(int productId, String operatingSystemName, String operatingSystemVersion)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetOperatingSystemSummarySql;

                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemName", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemName));
                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemVersion", DbType.String, SqlUtils.MakeSqlCompliantString(operatingSystemVersion));
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        long totalHits = (long)reader["TotalHits"];

                        return new StackHashProductOperatingSystemSummary(operatingSystemName, operatingSystemVersion, totalHits);
                    }

                    return null;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetOperatingSystemSummaryForProduct);
            }
        }


        /// <summary>
        /// Adds a OS summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose OS data is to be updated.</param>
        /// <param name="operatingSystemId">OS ID.</param>
        /// <param name="totalHits">Running total of all hits for this locale.</param>
        /// <param name="overwrite">True - overwrites original if exists. False - adds the totals if exists already.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddOperatingSystemSummary(int productId, short operatingSystemId, long totalHits, bool overwrite)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                if (overwrite)
                    sqlCommand.CommandText = s_AddOperatingSystemSummarySqlOverwrite;
                else
                    sqlCommand.CommandText = s_AddOperatingSystemSummarySql;

                m_SqlUtils.AddParameter(sqlCommand, "@OperatingSystemId", DbType.Int16, operatingSystemId);
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalHits", DbType.Int64, totalHits);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddOperatingSystemSummary);
            }
        }

        #endregion OperatingSystemSummaryMethods


        #region HitDateSummaryMethods

        /// <summary>
        /// Determines if a HitDate summary exists.
        /// </summary>
        /// <param name="productId">ID of the product to which the rollup data relates.</param>
        /// <param name="hitDateLocal">Hit date.</param>
        /// <returns>True - is present. False - not present.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool HitDateSummaryExists(int productId, DateTime hitDateLocal)
        {
            try
            {
                DateTime hitDateUtc = SqlUtils.ConvertToUtc(hitDateLocal);

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_HitDateSummaryExistsSql;
                m_SqlUtils.AddParameter(sqlCommand, "@HitDateUtc", DbType.DateTime, hitDateUtc);
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);
                int result = (int)m_SqlUtils.ExecuteScalarWithRetry(sqlCommand);

                return (result != 0);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToSeeIfHitDateSummaryExists);
            }
        }


        /// <summary>
        /// Gets all of the HitDate  rollup information for a particular product ID.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <returns>Product rollup information.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashProductHitDateSummaryCollection GetHitDateSummaries(int productId)
        {
            try
            {
                StackHashProductHitDateSummaryCollection hitDateSummaries = new StackHashProductHitDateSummaryCollection();

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetAllHitDateSummariesSql;
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        DateTime hitDateLocal = (DateTime)reader["HitDateUtc"];
                        hitDateLocal = SqlUtils.ConvertToLocal(hitDateLocal);
                        long totalHits = (long)reader["TotalHits"];

                        hitDateSummaries.Add(new StackHashProductHitDateSummary(hitDateLocal, totalHits));
                    }

                    return hitDateSummaries;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetHitDateSummaries);
            }
        }

        /// <summary>
        /// Gets a specific HitDate summary for a particular product.
        /// </summary>
        /// <param name="productId">ID of the product whose rollup data is required.</param>
        /// <param name="hitDateLocal">Hit date to get.</param>
        /// <returns>Product rollup information.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public StackHashProductHitDateSummary GetHitDateSummaryForProduct(int productId, DateTime hitDateLocal)
        {
            try
            {
                DateTime hitDateUtc = SqlUtils.ConvertToUtc(hitDateLocal);

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetHitDateSummarySql;

                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);
                m_SqlUtils.AddParameter(sqlCommand, "@HitDateUtc", DbType.DateTime, hitDateUtc);

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        long totalHits = (long)reader["TotalHits"];

                        return new StackHashProductHitDateSummary(hitDateLocal, totalHits);
                    }

                    return null;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetHitDateSummaryForProduct);
            }
        }


        /// <summary>
        /// Adds a HitDate summary to the database.
        /// </summary>
        /// <param name="productId">ID of the product whose OS data is to be updated.</param>
        /// <param name="hitDateLocal">Hit date.</param>
        /// <param name="totalHits">Running total of all hits for this hit date.</param>
        /// <param name="overwrite">True - overwrites original if exists. False - adds the totals if exists already.</param>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public void AddHitDateSummary(int productId, DateTime hitDateLocal, long totalHits, bool overwrite)
        {
            try
            {
                DateTime hitDateUtc = SqlUtils.ConvertToUtc(hitDateLocal);

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                if (overwrite)
                    sqlCommand.CommandText = s_AddHitDateSummarySqlOverwrite;
                else
                    sqlCommand.CommandText = s_AddHitDateSummarySql;

                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, (long)productId);
                m_SqlUtils.AddParameter(sqlCommand, "@HitDateUtc", DbType.DateTime, hitDateUtc);
                m_SqlUtils.AddParameter(sqlCommand, "@TotalHits", DbType.Int64, totalHits);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);

                return;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddHitDateSummary);
            }
        }

        #endregion HitDateSummaryMethods

        #region UpdateTableMethods;


        /// <summary>
        /// Gets the first entry in the Update Table belonging to this profile.
        /// </summary>
        /// <returns>The update located - or null if no update entry exists.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024")]
        public StackHashBugTrackerUpdate GetFirstUpdate()
        {
            try
            {
                StackHashBugTrackerUpdate update = new StackHashBugTrackerUpdate();

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_GetUpdateSql;
                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                try
                {
                    while (reader.Read())
                    {
                        update.EntryId = (int)reader["EntryId"];
                        update.DateChanged = (DateTime)reader["DateChanged"];
                        update.DataThatChanged = (StackHashDataChanged)reader["DataThatChanged"];
                        update.TypeOfChange = (StackHashChangeType)reader["TypeOfChange"];
                        update.ProductId = (long)reader["ProductId"];
                        update.FileId = (long)reader["FileId"];
                        update.EventId = (long)reader["EventId"];
                        update.EventTypeName = (String)reader["EventTypeName"];
                        update.CabId = (long)reader["CabId"];
                        update.ChangedObjectId = (long)reader["ChangedObjectId"];

                        return update;
                    }

                    return null;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetUpdateTableEntry);
            }
        }


        /// <summary>
        /// Adds a new update entry to the Update Table.
        /// Updates indicate changes that have occurred to objects in other tables.
        /// This table exists to feed the bug tracker plugins changes that have occurred
        /// to the database.
        /// Entries are normally added by the WinQualSync task and when notes are added.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void AddUpdate(StackHashBugTrackerUpdate update)
        {
            if (update == null)
                throw new ArgumentNullException("update");

            try
            {
                if (update.EventTypeName == null)
                    update.EventTypeName = "StackHashUndefined";

                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                short eventTypeId = GetEventTypeId(update.EventTypeName);
                if (eventTypeId == -1)
                    eventTypeId = AddEventType(update.EventTypeName);

                sqlCommand.CommandText = s_AddUpdateSql;

                m_SqlUtils.AddParameter(sqlCommand, "@DateChanged", DbType.DateTime, update.DateChanged);
                m_SqlUtils.AddParameter(sqlCommand, "@DataThatChanged", DbType.Int32, update.DataThatChanged);
                m_SqlUtils.AddParameter(sqlCommand, "@TypeOfChange", DbType.Int32, update.TypeOfChange);
                m_SqlUtils.AddParameter(sqlCommand, "@ProductId", DbType.Int64, update.ProductId);
                m_SqlUtils.AddParameter(sqlCommand, "@FileId", DbType.Int64, update.FileId);
                m_SqlUtils.AddParameter(sqlCommand, "@EventId", DbType.Int64, update.EventId);
                m_SqlUtils.AddParameter(sqlCommand, "@EventTypeId", DbType.Int16, eventTypeId);
                m_SqlUtils.AddParameter(sqlCommand, "@CabId", DbType.Int64, update.CabId);
                m_SqlUtils.AddParameter(sqlCommand, "@ChangedObjectId", DbType.Int64, update.ChangedObjectId);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddUpdateTableEntry);
            }
        }


        /// <summary>
        /// Ensures that the specified database has the correct tables and columns.
        /// </summary>
        /// <param name="databaseName">Name of the database to update.</param>
        /// <returns>true - if all up to date.</returns>
        public bool UpgradeDatabase(String databaseName)
        {
            DbConnection connection = m_SqlUtils.CreateConnection(true);

            try
            {
                // Select the new database as the active one.
                m_SqlUtils.SelectDatabase(databaseName, connection);

                SqlSchema.UpdateBeta95(m_SqlUtils, databaseName, connection);
                SqlSchema.Update1_20(m_SqlUtils, databaseName, connection);

                // Deselect the database.
                m_SqlUtils.SelectDatabase("MASTER", connection);

                return true;
            }
            finally
            {
                if (connection != null)
                    m_SqlUtils.ReleaseConnection(connection);
            }
        }

        /// <summary>
        /// Clear the update table.
        /// </summary>
        public void ClearAllUpdates()
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_ClearAllUpdateSql;

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToClearUpdateTableEntry);
            }
        }


        /// <summary>
        /// Removes the specified entry from the update table.
        /// </summary>
        /// <param name="update">Update to add.</param>
        public void RemoveUpdate(StackHashBugTrackerUpdate update)
        {
            if (update == null)
                throw new ArgumentNullException("update");

            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                sqlCommand.CommandText = s_RemoveUpdateSql;

                m_SqlUtils.AddParameter(sqlCommand, "@EntryId", DbType.Int32, update.EntryId);

                m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToRemoveUpdateTableEntry);
            }
        }


        #endregion


        #region MappingMethods

        /// <summary>
        /// Adds mappings to the mappings table.
        /// </summary>
        /// <param name="mappings">Collection of mappings to add.</param>
        /// <returns>True - success, False - failed.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100")]
        public bool AddMappings(StackHashMappingCollection mappings)
        {
            if (mappings == null)
                throw new ArgumentNullException("mappings");

            if (mappings.Count == 0)
                return true;

            try
            {
                foreach (StackHashMapping mapping in mappings)
                {
                    DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                    if (mapping.MappingType == StackHashMappingType.WorkFlow)
                        sqlCommand.CommandText = s_AddWorkFlowMappingSql;
                    else if (mapping.MappingType == StackHashMappingType.Group)
                        sqlCommand.CommandText = s_AddGroupMappingSql;
                    else
                        throw new ArgumentException("Unexpected mappings type", "mappings");

                    m_SqlUtils.AddParameter(sqlCommand, "@Id", DbType.Int32, mapping.Id);
                    m_SqlUtils.AddParameter(sqlCommand, "@Name", DbType.String, mapping.Name);

                    m_SqlUtils.ExecuteNonQueryWithRetry(sqlCommand);
                }

                return true;
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToAddMappings);
            }
        }


        /// <summary>
        /// Gets all mappings that match the specified mapping type.
        /// </summary>
        /// <param name="mappingType">Type of mapping required.</param>
        /// <returns>Collection of matching mappings.</returns>
        public StackHashMappingCollection GetMappings(StackHashMappingType mappingType)
        {
            try
            {
                DbCommand sqlCommand = m_ProviderFactory.CreateCommand();

                String idField;
                String nameField;

                if (mappingType == StackHashMappingType.WorkFlow)
                {
                    sqlCommand.CommandText = s_GetWorkFlowMappingsSql;
                    idField = "WorkFlowStatusId";
                    nameField = "WorkFlowStatusName";
                }
                else if (mappingType == StackHashMappingType.Group)
                {
                    sqlCommand.CommandText = s_GetGroupMappingsSql;
                    idField = "GroupId";
                    nameField = "GroupName";
                }
                else
                {
                    throw new ArgumentException("Unexpected mappings type", "mappingType");
                }

                DbDataReader reader = m_SqlUtils.ExecuteReaderWithRetry(sqlCommand);

                StackHashMappingCollection allMappings = new StackHashMappingCollection();

                try
                {
                    while (reader.Read())
                    {
                        int Id = (int)reader[idField];
                        String name = (String)reader[nameField];

                        StackHashMapping mapping = new StackHashMapping(mappingType, Id, name);

                        allMappings.Add(mapping);
                    }

                    return allMappings;
                }
                finally
                {
                    if (reader != null)
                        reader.Close();
                }
            }
            catch (SqlException ex)
            {
                throw new StackHashException(ex.Message, ex, StackHashServiceErrorCode.FailedToGetMappings);
            }
        }

        #endregion 


        #region IDisposable Members

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // These objects are owned by the SqlErrorIndex so don't close them - just get rid of the reference.
                m_ProviderFactory = null;
                m_SqlUtils.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
