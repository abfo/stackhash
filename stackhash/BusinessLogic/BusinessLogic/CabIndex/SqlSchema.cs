

// Moved to StackHashControl.dll


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Data;
//using System.Data.Common;

//using StackHashSqlControl;

//namespace StackHashErrorIndex
//{
//    internal static class SqlSchema
//    {
//        private static String s_ControlTable =
//            "CREATE TABLE Control (" +
//            " ControlItemId int, " +
//            " DatabaseVersion int," +
//            " SyncCount int," +
//            " CONSTRAINT pk_Control PRIMARY KEY (ControlItemId)" +
//            ")";

//        private static String s_ProductsTable =
//            "CREATE TABLE Products (" +
//            " ProductId bigint," +
//            " ProductName nvarchar(MAX), " +
//            " Version nvarchar(MAX), " +
//            " DateCreatedLocal smalldatetime, " +
//            " DateModifiedLocal smalldatetime, " +
//            " TotalEvents int, " +
//            " TotalResponses int, " +
//            " TotalStoredEvents int, " +
//            " CONSTRAINT pk_Products PRIMARY KEY (ProductId)" +
//            ")";

//        private static String s_ProductControlTable =
//            "CREATE TABLE ProductControl (" +
//            " ProductId bigint, " +
//            " LastSyncTime datetime, " +
//            " LastSyncCompletedTime datetime, " +
//            " LastSyncStartedTime datetime, " +
//            " LastHitTime datetime, " +
//            " SyncEnabled smallint, " +
//            " CONSTRAINT pk_ProductControl PRIMARY KEY (ProductId) " +
//            ")";

//        private static String s_TaskControlTable =
//            "CREATE TABLE TaskControl (" +
//            " TaskType int, " +
//            " TaskState int, " +
//            " LastSuccessfulRunTimeUtc datetime, " +
//            " LastFailedRunTimeUtc datetime, " +
//            " LastDurationInSeconds int, " +
//            " LastStartedTimeUtc datetime, " +
//            " RunCount int, " +
//            " SuccessCount int, " +
//            " FailedCount int, " +
//            " LastTaskException nvarchar(MAX), " +
//            " ServiceErrorCode int, " +
//            " CONSTRAINT pk_TaskType PRIMARY KEY (TaskType) " +
//            ")";
            
//        private static String s_FilesTable =
//            "CREATE TABLE Files (" +
//            " DateCreatedLocal smalldatetime, " +
//            " DateModifiedLocal smalldatetime, " +
//            " FileId bigint not null, " +
//            " LinkDateLocal datetime, " +
//            " FileName nvarchar(MAX), " +
//            " Version nvarchar(MAX), " +
//            " CONSTRAINT pk_Files PRIMARY KEY (FileId)" +
//            ")";

//        private static String s_EventsTable =
//            "CREATE TABLE Events (" +
//            " DateCreatedLocal smalldatetime, " +
//            " DateModifiedLocal smalldatetime, " +
//            " EventTypeId smallint, " +
//            " EventId bigint, " +
//            " ApplicationName nvarchar(MAX), " +
//            " ApplicationVersion nvarchar(MAX), " +
//            " ApplicationTimeStamp datetime, " +
//            " ModuleName nvarchar(MAX), " +
//            " ModuleVersion nvarchar(MAX), " +
//            " ModuleTimeStamp datetime, " +
//            " Offset bigint, " +
//            " ExceptionCode bigint, " +
//            " OffsetOriginal nvarchar(MAX), " +
//            " ExceptionCodeOriginal nvarchar(MAX), " +
//            " TotalHits int, " +
//            " BugId nvarchar(MAX), " +
//            " CONSTRAINT pk_Events PRIMARY KEY (EventId), " +
//            " CONSTRAINT fk_Events_EventTypeId FOREIGN KEY (EventTypeId) REFERENCES EventTypes(EventTypeId)" +
//             ")";


//        /// <summary>
//        /// Events can be associated with more than one file. 
//        /// Because of this you can't associate an event with a single fileId.
//        /// This table associates files and events.
//        /// </summary>
//        private static String s_FileEventsTable =
//            "CREATE TABLE FileEvents (" +
//            " FileId bigint not null, " +
//            " EventId bigint not null, " +
//            " EventTypeId smallint not null, " +
//            " CONSTRAINT pk_FileEvents PRIMARY KEY (FileId, EventId, EventTypeId), " +
//            ")";

//        private static String s_ProductFilesTable =
//            "CREATE TABLE ProductFiles (" +
//            " ProductId bigint not null, " +
//            " FileId bigint not null, " +
//            " CONSTRAINT pk_ProductFiles PRIMARY KEY (ProductId, FileId), " +
//            " CONSTRAINT fk_ProductFiles_ProductId FOREIGN KEY (ProductId) REFERENCES Products(ProductId), " +
//            " CONSTRAINT fk_ProductFiles_FileId FOREIGN KEY (FileId) REFERENCES Files(FileId)" +
//            ")";

//        private static String s_CabDiagnosticsFields =
//            " SystemUpTime nvarchar(100), " +
//            " ProcessUpTime nvarchar(100), " +
//            " DotNetVersion nvarchar(100), " +
//            " OSVersion nvarchar(200), " +
//            " MachineArchitecture nvarchar(20), ";

//        private static String s_CabsTable =
//            "CREATE TABLE Cabs (" +
//            " DateCreatedLocal smalldatetime, " +
//            " DateModifiedLocal smalldatetime, " +
//            " CabId bigint, " +
//            " CabFileName nvarchar(550), " +
//            " EventId bigint not null, " +
//            " EventTypeId smallint not null, " +
//            " SizeInBytes bigint not null, " +
//            " CabDownloaded tinyint, " +
//            " Purged tinyint, " +
//            s_CabDiagnosticsFields + 
//            " CONSTRAINT pk_Cabs PRIMARY KEY (CabId), " +
//            " CONSTRAINT fk_Cabs_EventId FOREIGN KEY (EventId) REFERENCES Events(EventId), " +
//            " CONSTRAINT fk_Cabs_EventTypeId FOREIGN KEY (EventTypeId) REFERENCES EventTypes(EventTypeId)" +
//            ")";

//        private static String s_CabNotesTable =
//            "CREATE TABLE CabNotes (" +
//            " NoteId int identity(1,1), " +
//            " CabId bigint not null, " +
//            " TimeOfEntry datetime not null, " +
//            " Source nchar(20) not null, " +
//            " StackHashUser nchar(100) not null, " +
//            " Note nvarchar(MAX) not null, " +
//            " CONSTRAINT pk_CabNotes PRIMARY KEY (NoteId), " +
//            " CONSTRAINT fk_CabNotes_CabId FOREIGN KEY (CabId) REFERENCES Cabs(CabId)" +
//            ")";

//        private static String s_EventNotesTable =
//            "CREATE TABLE EventNotes (" +
//            " NoteId int identity(1,1), " +
//            " EventId bigint not null, " +
//            " TimeOfEntry datetime not null, " +
//            " Source nchar(20) not null, " +
//            " StackHashUser nchar(100) not null, " +
//            " Note nvarchar(MAX) not null, " +
//            " CONSTRAINT pk_EventNotes PRIMARY KEY (NoteId), " +
//            " CONSTRAINT fk_EventNotes_EventId FOREIGN KEY (EventId) REFERENCES Events(EventId)" +
//            ")";

//        private static String s_EventInfoTable =
//            "CREATE TABLE EventInfos (" +
//            " DateCreatedLocal smalldatetime, " +
//            " DateModifiedLocal smalldatetime, " +
//            " HitDateLocal datetime, " +
//            " LocaleId smallint, " +
//            " TotalHits int, " +
//            " EventId bigint, " +
//            " EventTypeId smallint, " +
//            " OperatingSystemId smallint, " +
//            " CONSTRAINT pk_EventInfos PRIMARY KEY (EventId, HitDateLocal, OperatingSystemId), " +
//            " CONSTRAINT fk_EventInfos_EventId FOREIGN KEY (EventId) REFERENCES Events(EventId)," +
//            " CONSTRAINT fk_EventInfos_OperatingSystemId FOREIGN KEY (OperatingSystemId) REFERENCES OperatingSystems(OperatingSystemId)," +
//            " CONSTRAINT fk_EventInfos_LocaleId FOREIGN KEY (LocaleId) REFERENCES Locales(LocaleId)" +
//            ")";


//        /// <summary>
//        /// Lists the locales that occur in EventInfos.
//        /// </summary>
//        private static String s_LocalesTable =
//            "CREATE TABLE Locales (" +
//            " LocaleId smallint not null, " +
//            " LocaleCode varchar(15), " +
//            " LocaleName nvarchar(MAX), " +
//            " CONSTRAINT pk_Locales PRIMARY KEY (LocaleId ASC), " +
//            ")";

        
//        /// <summary>
//        /// A table containing the different operating systems. This cuts down on space for storing all of the 
//        /// different OS strings in the EventInfos table.
//        /// Note that the identify keyword for autoincrement is different in each DMBS. See http://www.w3schools.com/sql/sql_autoincrement.asp
//        /// </summary>
//        private static String s_OperatingSystemTable =
//            "CREATE TABLE OperatingSystems (" +
//            " OperatingSystemId smallint identity(1,1), " +
//            " OperatingSystemName nvarchar(MAX), " +
//            " OperatingSystemVersion nvarchar(MAX), " +
//            " CONSTRAINT pk_OperatingSystems PRIMARY KEY (OperatingSystemId), " +
//            ")";

//        /// <summary>
//        /// Contains the event types that can occur.
//        /// </summary>
//        private static String s_EventTypesTable =
//            "CREATE TABLE EventTypes (" +
//            " EventTypeId smallint identity(1,1), " +
//            " EventTypeName nvarchar(50), " +
//            " CONSTRAINT pk_EventTypes PRIMARY KEY (EventTypeId), " +
//            ")";
        

//        // ****************************************************************************************
//        // Views.
//        // ****************************************************************************************

//        private const String s_FileEventsView =
//            "CREATE VIEW [FileEvents_Vw] AS " +
//            "SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal,E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, " +
//            "       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName, " +
//            "       E.BugId, FE.FileId " +
//            "FROM dbo.Events E " +
//            "INNER JOIN dbo.EventTypes ET " +
//            "   ON ET.EventTypeId=E.EventTypeId " +
//            "INNER JOIN dbo.FileEvents FE " +
//            "   ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId";

//        private const String s_ProductEventsView =
//            "CREATE VIEW [ProductEvents_Vw] AS " +
//            "SELECT E.EventId, E.DateCreatedLocal, E.DateModifiedLocal,E.ApplicationName, E.ApplicationVersion, E.ApplicationTimeStamp, E.TotalHits, " +
//            "       E.ModuleName, E.ModuleVersion, E.ModuleTimeStamp, E.Offset, E.ExceptionCode, E.OffsetOriginal, E.ExceptionCodeOriginal, ET.EventTypeName, " +
//            "       E.BugId, FE.FileId " +
//            "FROM dbo.Events E " +
//            "INNER JOIN dbo.EventTypes ET " +
//            "   ON ET.EventTypeId=E.EventTypeId " +
//            "INNER JOIN dbo.FileEvents FE " +
//            "   ON FE.EventId=E.EventId AND FE.EventTypeId=E.EventTypeId " +
//            "INNER JOIN dbo.ProductFiles PF " +
//            "   ON PF.FileId=FE.EventId AND PF.ProductId=@ProductId ";

//        /// <summary>
//        /// Creates a database for the stackhash data with the specified name.
//        /// Selects the database.
//        /// Creates the tables in the database.
//        /// </summary>
//        /// <param name="sqlCommands"></param>
//        /// <param name="databaseName"></param>
//        /// <returns></returns>
//        public static bool CreateStackHashDatabase(SqlUtils sqlCommands, String databaseName)
//        {
//            // Check if the database exists first.
//            if (sqlCommands.DatabaseExists(databaseName))
//                return true;

//            // Create the database itself.
//            sqlCommands.CreateDatabase(databaseName);

//            // Select the new database as the active one.
//            sqlCommands.SelectDatabase(databaseName);

//            // Create the control table. This table contains general global information.
//            sqlCommands.ExecuteNonQuery(s_ControlTable);

//            // Create the product control table. This table contains general control information for each product.
//            sqlCommands.ExecuteNonQuery(s_ProductControlTable);

//            // Create the task status table. This table contains general global information about tasks being run.
//            sqlCommands.ExecuteNonQuery(s_TaskControlTable);
            
//            // Create the operating system table.
//            sqlCommands.ExecuteNonQuery(s_OperatingSystemTable);

//            // Create the locales table.
//            sqlCommands.ExecuteNonQuery(s_LocalesTable);

//            // Create the event types table.
//            sqlCommands.ExecuteNonQuery(s_EventTypesTable);

//            // Create the products table.
//            sqlCommands.ExecuteNonQuery(s_ProductsTable);

//            // Create the files table.
//            sqlCommands.ExecuteNonQuery(s_FilesTable);

//            // Create the product files table.
//            sqlCommands.ExecuteNonQuery(s_ProductFilesTable);

//            // Create the events table.
//            sqlCommands.ExecuteNonQuery(s_EventsTable);

//            // Create the file events table.
//            sqlCommands.ExecuteNonQuery(s_FileEventsTable);

//            // Create the event infos table.
//            sqlCommands.ExecuteNonQuery(s_EventInfoTable);

//            // Create the cabs table.
//            sqlCommands.ExecuteNonQuery(s_CabsTable);

//            // Create the cab notes table.
//            sqlCommands.ExecuteNonQuery(s_CabNotesTable);

//            // Create the event notes table.
//            sqlCommands.ExecuteNonQuery(s_EventNotesTable);

//            // Create File Events view.
//            sqlCommands.ExecuteNonQuery(s_FileEventsView);

//            // Create Product Events view.
// //           sqlCommands.ExecuteNonQuery(s_ProductEventsView);

//            // Set the initial control data.
//            sqlCommands.AddControl(new StackHashBusinessObjects.StackHashControlData(SqlErrorIndex.CurrentDatabaseVersion, 0));

//            return true;
//        }
//    }
//}
