﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.Common;

namespace StackHashSqlControl
{
    public static class SqlSchema
    {
        private static String s_ControlTable =
            "CREATE TABLE Control (" +
            " ControlItemId int not null, " +
            " DatabaseVersion int not null," +
            " SyncCount int not null," +
            " SyncProductId int not null," +
            " SyncFileId int not null," +
            " SyncEventId int not null," +
            " SyncCabId int not null," +
            " SyncEventTypeName nvarchar(MAX), " + // Can be null if no sync has occurred yet.
            " SyncPhase int not null, " +
            " CONSTRAINT pk_Control PRIMARY KEY (ControlItemId)" +
            ");";
        
        private static String s_ProductsTable =
            "CREATE TABLE Products (" +
            " ProductId bigint not null," +
            " ProductName nvarchar(100) not null, " +
            " Version nvarchar(100) not null, " +
            " DateCreatedLocal smalldatetime, " +
            " DateModifiedLocal smalldatetime, " +
            " TotalEvents int not null," +
            " TotalResponses int not null," +
            " TotalStoredEvents int not null," +
            " CONSTRAINT pk_Products PRIMARY KEY CLUSTERED (ProductId ASC) " +
            ");";

        private static String s_ProductControlTable =
            "CREATE TABLE ProductControl (" +
            " ProductId bigint not null, " +
            " LastSyncTime datetime not null, " +
            " LastSyncCompletedTime datetime not null, " +
            " LastSyncStartedTime datetime not null, " +
            " LastHitTime datetime not null, " +
            " SyncEnabled tinyint not null, " +
            " CONSTRAINT pk_ProductControl PRIMARY KEY CLUSTERED (ProductId ASC) " +
            ");";

        private static String s_TaskControlTable =
            "CREATE TABLE TaskControl (" +
            " TaskType int not null, " +
            " TaskState int not null, " +
            " LastSuccessfulRunTimeUtc datetime not null, " +
            " LastFailedRunTimeUtc datetime not null, " +
            " LastStartedTimeUtc datetime not null, " +
            " LastDurationInSeconds int not null, " +
            " RunCount int not null, " +
            " SuccessCount int not null, " +
            " FailedCount int not null, " +
            " LastTaskException nvarchar(MAX) null, " +  // Can be null.
            " ServiceErrorCode int not null, " +
            " CONSTRAINT pk_TaskType PRIMARY KEY (TaskType) " +
            ");";

        private static String s_FilesTable =
            "CREATE TABLE Files (" +
            " FileId bigint not null, " +
            " FileName nvarchar(400) not null, " +
            " Version nvarchar(24) not null, " +
            " DateCreatedLocal smalldatetime not null, " +
            " DateModifiedLocal smalldatetime not null, " +
            " LinkDateLocal smalldatetime not null, " +
            " CONSTRAINT pk_Files PRIMARY KEY CLUSTERED (FileId ASC)" +
            ");";


        /// <summary>
        ///  BucketID is an internal term.  On the WinQual portal, you'll see it as "EventID."  It's generated by WATSON when a new report comes in.  
        ///  Identity-seeded Primary Key on each of the tables (Crash32, Crash64, Generic)
        ///  The Bucket itself is based on the following: 
        ///  1) Application Name
        ///  2) Application Version
        ///  3) Module Name
        ///  4) Module Version
        ///  5) Offset
        ///  6) Exception Code
        ///  
        /// Therefore the EventId on its own is not unique. The EventId + EventType form a unique pairing.
        /// </summary>    
        private static String s_EventsTable =
            "CREATE TABLE Events (" +
            " EventId bigint not null, " +
            " EventTypeId smallint not null, " +
            " DateCreatedLocal smalldatetime not null, " +
            " DateModifiedLocal smalldatetime not null, " +
            " TotalHits bigint  not null, " +
            " ApplicationName nvarchar(400) null, " + // Shouldn't be null but allow anyway.
            " ApplicationVersion nvarchar(50) null, " + // Shouldn't be null but allow anyway.
            " ApplicationTimeStamp datetime null, " + // Can be null.
            " ModuleName nvarchar(400) null, " + // Shouldn't be null but allow anyway.
            " ModuleVersion nvarchar(50) null, " + // Shouldn't be null but allow anyway.
            " ModuleTimeStamp datetime null, " + // Can be null.
            " OffsetOriginal nvarchar(50) null, " + // Shouldn't be null but allow anyway.
            " ExceptionCodeOriginal nvarchar(15) null, " + // Can be null.
            " Offset bigint not null, " +
            " ExceptionCode bigint not null, " +
            " BugId nvarchar(MAX) null, " + // Can be null.
            " PlugInBugId nvarchar(MAX) null, " + // Can be null.
            " WorkFlowStatus int null, " +

            " CONSTRAINT pk_Events PRIMARY KEY CLUSTERED (EventId ASC, EventTypeId ASC), " +
            " CONSTRAINT fk_EventsToEventTypes FOREIGN KEY (EventTypeId) REFERENCES EventTypes(EventTypeId)" +
             ");";


        /// <summary>
        /// Events can be associated with more than one file. 
        /// Because of this you can't associate an event with a single fileId.
        /// This table associates files and events.
        /// </summary>
        private static String s_FileEventsTable =
            "CREATE TABLE FileEvents (" +
            " FileId bigint not null, " +
            " EventId bigint not null, " +
            " EventTypeId smallint not null, " +
            " CONSTRAINT pk_FileEvents PRIMARY KEY CLUSTERED (FileId ASC, EventId ASC, EventTypeId ASC), " +
            " CONSTRAINT fk_FileEventsToEvents FOREIGN KEY (EventId, EventTypeId) REFERENCES Events(EventId,EventTypeId), " +
            " CONSTRAINT fk_FileEventsToFiles FOREIGN KEY (FileId) REFERENCES Files(FileId) " +
            ");";

        private static String s_ProductFilesTable =
            "CREATE TABLE ProductFiles (" +
            " ProductId bigint not null, " +
            " FileId bigint not null, " +
            " CONSTRAINT pk_ProductFiles PRIMARY KEY CLUSTERED (ProductId ASC, FileId ASC), " +
            " CONSTRAINT fk_ProductFilesToProducts FOREIGN KEY (ProductId) REFERENCES Products(ProductId), " +
            " CONSTRAINT fk_ProductFilesToFiles FOREIGN KEY (FileId) REFERENCES Files(FileId)" +
            ");";

        private static String s_CabDiagnosticsFields =
            " SystemUpTime nvarchar(100) null, " +
            " ProcessUpTime nvarchar(100) null, " +
            " DotNetVersion nvarchar(100) null, " +
            " OSVersion nvarchar(200) null, " +
            " MachineArchitecture nvarchar(20) null, ";

        private static String s_CabsTable =
            "CREATE TABLE Cabs (" +
            " CabId bigint, " +
            " SizeInBytes bigint not null, " +
            " CabFileName nvarchar(550) not null, " +
            " DateCreatedLocal smalldatetime not null, " +
            " DateModifiedLocal smalldatetime not null, " +
            " EventId bigint not null, " +
            " EventTypeId smallint not null, " +
            " CabDownloaded tinyint not null, " +
            " Purged tinyint not null, " +
            s_CabDiagnosticsFields +
            " CONSTRAINT pk_Cabs PRIMARY KEY CLUSTERED (CabId ASC), " +
            " CONSTRAINT fk_CabsToEvents FOREIGN KEY (EventId, EventTypeId) REFERENCES Events(EventId, EventTypeId), " +
            " CONSTRAINT fk_CabsToEventTypes FOREIGN KEY (EventTypeId) REFERENCES EventTypes(EventTypeId)" +
            ");";

        private static String s_CabNotesTable =
            "CREATE TABLE CabNotes (" +
            " NoteId int identity(1,1), " +
            " CabId bigint not null, " +
            " TimeOfEntry datetime not null, " +
            " SourceId smallint not null, " +
            " UserId smallint not null, " +
            " Note nvarchar(MAX) not null, " +
            " CONSTRAINT pk_CabNotes PRIMARY KEY CLUSTERED (NoteId ASC), " +
            " CONSTRAINT fk_CabNotesToCabs FOREIGN KEY (CabId) REFERENCES Cabs(CabId)" +
            ");";

        private static String s_EventNotesTable =
            "CREATE TABLE EventNotes (" +
            " NoteId int identity(1,1), " +
            " EventId bigint not null, " +
            " EventTypeId smallint not null, " +
            " TimeOfEntry datetime not null, " +
            " SourceId smallint not null, " +
            " UserId smallint not null, " +
            " Note nvarchar(MAX) not null, " +
            " CONSTRAINT pk_EventNotes PRIMARY KEY CLUSTERED (NoteId ASC), " +
            " CONSTRAINT fk_EventNotesToEvents FOREIGN KEY (EventId, EventTypeId) REFERENCES Events(EventId, EventTypeId)" +
            ");";

        private static String s_UsersTable =
            "CREATE TABLE Users (" +
            " UserId smallint identity(1,1) not null, " +
            " StackHashUser nvarchar(100) not null, " +
            " CONSTRAINT pk_Users PRIMARY KEY CLUSTERED (UserId ASC) " +
            ");";

        private static String s_SourceTable =
            "CREATE TABLE Sources (" +
            " SourceId smallint identity(1,1) not null, " +
            " Source nvarchar(20) not null, " +
            " CONSTRAINT pk_Sources PRIMARY KEY CLUSTERED (SourceId ASC) " +
            ");";

        
        private static String s_EventInfoTable =
            "CREATE TABLE EventInfos (" +
            " EventId bigint not null, " +
            " EventTypeId smallint not null, " +
            " DateCreatedLocal smalldatetime not null, " +
            " DateModifiedLocal smalldatetime not null, " +
            " HitDateLocal datetime not null, " +
            " LocaleId smallint not null, " +
            " TotalHits int not null, " +
            " OperatingSystemId smallint not null, " +
            " CONSTRAINT pk_EventInfos PRIMARY KEY (EventId, EventTypeId, HitDateLocal, OperatingSystemId, LocaleId), " +
            " CONSTRAINT fk_EventInfosToEventId FOREIGN KEY (EventId, EventTypeId) REFERENCES Events(EventId, EventTypeId)," +
            " CONSTRAINT fk_EventInfosToOperatingSystems FOREIGN KEY (OperatingSystemId) REFERENCES OperatingSystems(OperatingSystemId)," +
            " CONSTRAINT fk_EventInfosToLocales FOREIGN KEY (LocaleId) REFERENCES Locales(LocaleId)" +
            ");";


        /// <summary>
        /// Lists the locales that occur in EventInfos.
        /// </summary>
        private static String s_LocalesTable =
            "CREATE TABLE Locales (" +
            " LocaleId smallint not null, " +
            " LocaleCode varchar(15), " +  // Shouldn't be null but allow anyhow.
            " LocaleName nvarchar(400), " +  // Shouldn't be null but allow anyhow.
            " CONSTRAINT pk_Locales PRIMARY KEY CLUSTERED (LocaleId ASC) " +
            ");";


        /// <summary>
        /// A table containing the different operating systems. This cuts down on space for storing all of the 
        /// different OS strings in the EventInfos table.
        /// Note that the identify keyword for autoincrement is different in each DMBS. See http://www.w3schools.com/sql/sql_autoincrement.asp
        /// </summary>
        private static String s_OperatingSystemTable =
            "CREATE TABLE OperatingSystems (" +
            " OperatingSystemId smallint identity(1,1) not null, " +
            " OperatingSystemName nvarchar(100), " +  // Shouldn't be null but allow anyway.
            " OperatingSystemVersion nvarchar(100), " +  // Shouldn't be null but allow anyway.
            " CONSTRAINT pk_OperatingSystems PRIMARY KEY CLUSTERED (OperatingSystemId ASC) " +
            ");";

        /// <summary>
        /// Contains the event types that can occur.
        /// </summary>
        private static String s_EventTypesTable =
            "CREATE TABLE EventTypes (" +
            " EventTypeId smallint identity(1,1) not null, " +
            " EventTypeName nvarchar(50) not null, " +
            " CONSTRAINT pk_EventTypes PRIMARY KEY CLUSTERED (EventTypeId ASC) " +
            ");";


        /// <summary>
        /// Locale rollup table per product.
        /// Each product will have its own entries in this table.
        /// Each row for a product represents the number of hits across all events where
        /// the hit data was for a particular locale id.
        /// </summary>
        private static String s_LocaleSummaryTable =
            "IF OBJECT_ID('dbo.LocaleSummary', 'U') IS NULL " +
            " CREATE TABLE LocaleSummary (" +
            "  ProductId bigint not null, " +
            "  LocaleId smallint not null, " +
            "  TotalHits bigint not null, " +
            "  CONSTRAINT pk_LocaleSummary PRIMARY KEY CLUSTERED (LocaleId ASC, ProductId ASC), " +
            "  CONSTRAINT fk_LocaleSummaryToLocales FOREIGN KEY (LocaleId) REFERENCES Locales(LocaleId) " +
            ");";

        /// <summary>
        /// Operating System rollup table per product.
        /// Each product will have its own entries in this table.
        /// Each row for a product represents the number of hits across all events where
        /// the hit data was for a particular OS.
        /// </summary>
        private static String s_OperatingSystemSummaryTable =
            "IF OBJECT_ID('dbo.OperatingSystemSummary', 'U') IS NULL " +
            " CREATE TABLE OperatingSystemSummary (" +
            "  ProductId bigint not null, " +
            "  OperatingSystemId smallint not null, " +
            "  TotalHits bigint not null, " +
            "  CONSTRAINT pk_OperatingSystemSummary PRIMARY KEY CLUSTERED (OperatingSystemId ASC, ProductId ASC), " +
            "  CONSTRAINT fk_OperatingSystemSummaryToOperatingSystems FOREIGN KEY (OperatingSystemId) REFERENCES OperatingSystems(OperatingSystemId) " +
            ");";

        /// <summary>
        /// Hit date rollup table per product.
        /// Each product will have its own entries in this table.
        /// Each row for a product represents the number of hits across all events where
        /// the hit data was for a particular hit date.
        /// </summary>
        private static String s_HitDateSummaryTable =
            "IF OBJECT_ID('dbo.HitDateSummary', 'U') IS NULL " +
            " CREATE TABLE HitDateSummary (" +
            "  ProductId bigint not null, " +
            "  HitDateUtc datetime not null, " +
            "  TotalHits bigint not null, " +
            "  CONSTRAINT pk_HitDateSummary PRIMARY KEY CLUSTERED (HitDateUtc ASC, ProductId ASC) " +
            ");";


        /// <summary>
        /// Stores updates a record of all updates that have occurred in the database.
        /// These events are processed by the BugTrackerTask and sent to the bug tracking plug-ins.
        /// Note that the identify keyword for autoincrement is different in each DMBS. See http://www.w3schools.com/sql/sql_autoincrement.asp
        /// </summary>
        private static String s_UpdateTable =
            "IF OBJECT_ID('dbo.UpdateTable', 'U') IS NULL " +
            "  CREATE TABLE UpdateTable (" +
            "    EntryId int identity(1,1) not null, " +
            "    DateChanged datetime not null, " +
            "    ProductId bigint not null," +
            "    FileId bigint not null, " +
            "    EventId bigint not null, " +
            "    EventTypeId smallint not null, " +
            "    CabId bigint not null, " +
            "    ChangedObjectId bigint not null, " +
            "    DataThatChanged int not null, " +
            "    TypeOfChange int not null, " + 
            "    CONSTRAINT pk_Updates PRIMARY KEY CLUSTERED (EntryId ASC) " +
            ");";


        /// <summary>
        /// Adds a PlugInBugId to the EventTable if necessary.
        /// </summary>
        private static String s_AddPlugInBugReferenceToEventTable =
            "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'PlugInBugId' " +   
            "  AND Object_ID = Object_ID(N'Events')) " + 
            "BEGIN " + 
            "IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL " +
            "  ALTER TABLE dbo.Events " +
            "    ADD PlugInBugId nvarchar(MAX) null " + 
            "END ";


        /// <summary>
        /// Updates for 1.20.
        /// </summary>
        private static String s_WorkFlowMappingsTable =
            "IF OBJECT_ID('dbo.WorkFlowMappings', 'U') IS NULL " +
            " CREATE TABLE WorkFlowMappings (" +
            "  WorkFlowStatusId int not null, " +
            "  WorkFlowStatusName nvarchar(MAX) not null, " +
            "  CONSTRAINT pk_WorkFlowMappings PRIMARY KEY CLUSTERED (WorkFlowStatusId ASC), " +
            ");";

        /// <summary>
        /// Updates for 1.20.
        /// </summary>
        private static String s_GroupMappingsTable =
            "IF OBJECT_ID('dbo.GroupMappings', 'U') IS NULL " +
            " CREATE TABLE GroupMappings (" +
            "  GroupId int not null, " +
            "  GroupName nvarchar(MAX) not null, " +
            "  CONSTRAINT pk_GroupMappings PRIMARY KEY CLUSTERED (GroupId ASC), " +
            ");";

        
        /// <summary>
        /// Adds a WorkFlowStatus to the EventTable if necessary.
        /// </summary>
        private static String s_AddWorkFlowStatusToEventTable =
            "IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'WorkFlowStatus' " +
            "  AND Object_ID = Object_ID(N'Events')) " +
            "BEGIN " +
            "IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL " +
            "  ALTER TABLE dbo.Events " +
            "    ADD WorkFlowStatus int " +
            "    DEFAULT 0 NOT NULL " + 
            "END ";

        
        //private const String s_ProductEventsView =
        //    "CREATE VIEW [ProductEvents_Vw] AS " +
        //    "SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal,E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, " +
        //    "       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName, " +
        //    "       E.BugId, FE.FileId " +
        //    "FROM dbo.Events E " +
        //    "INNER JOIN dbo.EventTypes ET " +
        //    "   ON ET.EventTypeId=E.EventTypeId " +
        //    "INNER JOIN dbo.FileEvents FE " +
        //    "   ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId " +
        //    "INNER JOIN dbo.ProductFiles PF " +
        //    "   ON PF.FileId=FE.EventId AND PF.ProductId=@ProductId ";



        /// <summary>
        /// Beta 95 sees the introduction of the UpdateTable.
        /// </summary>
        /// <returns>True - update applied. False - not necessary.</returns>
        public static void UpdateBeta95(SqlUtils sqlUtils, String databaseName, DbConnection connection)
        {
            // Add the update table.
            sqlUtils.ExecuteNonQuery(s_UpdateTable, connection);

            // Adds the new bug reference field to the Event.
            sqlUtils.ExecuteNonQuery(s_AddPlugInBugReferenceToEventTable, connection);
        }

        /// <summary>
        /// 1.20 added the Mappings table for workflow values.
        /// </summary>
        /// <returns>True - update applied. False - not necessary.</returns>
        public static void Update1_20(SqlUtils sqlUtils, String databaseName, DbConnection connection)
        {
            // Add the work flow mappings table.
            sqlUtils.ExecuteNonQuery(s_WorkFlowMappingsTable, connection);

            // Add the group mappings table.
            sqlUtils.ExecuteNonQuery(s_GroupMappingsTable, connection);

            // Adds the new workflow status to the Event.
            sqlUtils.ExecuteNonQuery(s_AddWorkFlowStatusToEventTable, connection);
        }

        
        /// <summary>
        /// Creates a database for the stackhash data with the specified name.
        /// Selects the database.
        /// Creates the tables in the database.
        /// </summary>
        /// <param name="sqlUtils"></param>
        /// <param name="databaseFolder">Folder where the database is to be stored.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>True - database was created. False - already exists.</returns>
        public static bool CreateStackHashDatabase(SqlUtils sqlUtils, String databaseFolder, String databaseName, bool createIndexInDefaultLocation)
        {
            DbConnection connection = sqlUtils.CreateConnection(true);

            try
            {
                // Check if the database exists first.
                if (sqlUtils.DatabaseExists(databaseName, connection))
                {
                    // Select the new database as the active one.
                    sqlUtils.SelectDatabase(databaseName, connection);

                    // Do upgrades here.
                    UpdateBeta95(sqlUtils, databaseName, connection);
                    Update1_20(sqlUtils, databaseName, connection);

                    // Deselect the database.
                    sqlUtils.SelectDatabase("MASTER", connection);

                    return false;
                }

                // Create the database itself.
                sqlUtils.CreateDatabase(databaseFolder, databaseName, connection, createIndexInDefaultLocation);

                // Select the new database as the active one.
                sqlUtils.SelectDatabase(databaseName, connection);

                // Create the control table. This table contains general global information.
                sqlUtils.ExecuteNonQuery(s_ControlTable, connection);

                // Create the product control table. This table contains general control information for each product.
                sqlUtils.ExecuteNonQuery(s_ProductControlTable, connection);

                // Create the task status table. This table contains general global information about tasks being run.
                sqlUtils.ExecuteNonQuery(s_TaskControlTable, connection);

                // Create the operating system table.
                sqlUtils.ExecuteNonQuery(s_OperatingSystemTable, connection);

                // Create the locales table.
                sqlUtils.ExecuteNonQuery(s_LocalesTable, connection);

                // Create the event types table.
                sqlUtils.ExecuteNonQuery(s_EventTypesTable, connection);

                // Create the products table.
                sqlUtils.ExecuteNonQuery(s_ProductsTable, connection);

                // Create the files table.
                sqlUtils.ExecuteNonQuery(s_FilesTable, connection);

                // Create the product files table.
                sqlUtils.ExecuteNonQuery(s_ProductFilesTable, connection);

                // Create the events table.
                sqlUtils.ExecuteNonQuery(s_EventsTable, connection);

                // Create the file events table.
                sqlUtils.ExecuteNonQuery(s_FileEventsTable, connection);

                // Create the event infos table.
                sqlUtils.ExecuteNonQuery(s_EventInfoTable, connection);

                // Create the cabs table.
                sqlUtils.ExecuteNonQuery(s_CabsTable, connection);

                // Create the cab notes table.
                sqlUtils.ExecuteNonQuery(s_CabNotesTable, connection);

                // Create the users table.
                sqlUtils.ExecuteNonQuery(s_UsersTable, connection);

                // Create the source table.
                sqlUtils.ExecuteNonQuery(s_SourceTable, connection);

                // Create the event notes table.
                sqlUtils.ExecuteNonQuery(s_EventNotesTable, connection);

                // Add the Locale summary table.
                sqlUtils.ExecuteNonQuery(s_LocaleSummaryTable, connection);

                // Add the operating system summary table.
                sqlUtils.ExecuteNonQuery(s_OperatingSystemSummaryTable, connection);

                // Add the hit date summary table.
                sqlUtils.ExecuteNonQuery(s_HitDateSummaryTable, connection);
                
                // Additions for Beta95.
                UpdateBeta95(sqlUtils, databaseName, connection);

                // Additions for 1.20.
                Update1_20(sqlUtils, databaseName, connection);

                // Deselect the database.
                sqlUtils.SelectDatabase("MASTER", connection);
                
                return true;
            }
            finally
            {
                if (connection != null)
                    sqlUtils.ReleaseConnection(connection);
            }
        }
    }
}
