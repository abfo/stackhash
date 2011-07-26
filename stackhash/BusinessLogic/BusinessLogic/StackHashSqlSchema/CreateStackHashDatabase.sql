---------------------------------------------------------------------
-- StackHash database creation script.  Copyright 2011 - Cucku, Inc. 
--
-- This script creates a database for use by StackHash Version 0.95.
-- To update an existing database use the UpdateStackHashDatabase.sql
-- script located in the installation folder for the StackHash 
-- product.
--
-- The database name will be "StackHash". To create a database of a 
-- different name, change the database name and USE statement at the 
-- start of this script.
--
-- Supported versions of SQL Server: 2005, 2008
--
-- Last updated: 15-Jan-2011
---------------------------------------------------------------------

---------------------------------------------------------------------
-- Create Database
---------------------------------------------------------------------

USE master;

-- If the database already exists then raise an error.
IF DB_ID('StackHash') IS NOT NULL 
   RAISERROR('Database cannot be created because it already exists. Either upgrade the old database or edit this script with a new database name.', 127, 127) WITH NOWAIT, LOG;

-- Create database
CREATE DATABASE StackHash;
GO

USE StackHash;
GO


---------------------------------------------------------------------
-- Create Tables
---------------------------------------------------------------------

-- Create table Control
CREATE TABLE Control
(
	ControlItemId		int			not null,
    DatabaseVersion		int			not null,
    SyncCount			int			not null,
    SyncProductId		int			not null,
	SyncFileId			int			not null,
    SyncEventId			int			not null,
    SyncCabId			int			not null,
    SyncEventTypeName	nvarchar(MAX),
    SyncPhase			int			not null,
    
	CONSTRAINT pk_Control PRIMARY KEY (ControlItemId)
);


-- Create table Locales
CREATE TABLE Locales
(
    LocaleId	smallint		not null, 
    LocaleCode	varchar(15), 
    LocaleName	nvarchar(400), 

    CONSTRAINT pk_Locales PRIMARY KEY CLUSTERED (LocaleId ASC)
);


-- Create table OperatingSystems
CREATE TABLE OperatingSystems
(
    OperatingSystemId		smallint		identity(1,1) not null, 
    OperatingSystemName		nvarchar(100), 
    OperatingSystemVersion	nvarchar(100), 

    CONSTRAINT pk_OperatingSystems PRIMARY KEY CLUSTERED (OperatingSystemId ASC)
);


-- Create table EventTypes
CREATE TABLE EventTypes
(
    EventTypeId smallint identity(1,1) not null, 
    EventTypeName nvarchar(50) not null, 

    CONSTRAINT pk_EventTypes PRIMARY KEY CLUSTERED (EventTypeId ASC)
);


-- Create table Products
CREATE TABLE Products
(
	ProductId			bigint			not null,
	ProductName			nvarchar(100)	not null, 
	Version				nvarchar(100)	not null,
	DateCreatedLocal	smalldatetime,
	DateModifiedLocal	smalldatetime,
	TotalEvents			int				not null,
	TotalResponses		int				not null,
	TotalStoredEvents	int				not null,

	CONSTRAINT pk_Products PRIMARY KEY CLUSTERED (ProductId ASC)
);


-- Create table ProductControl
CREATE TABLE ProductControl
(
    ProductId				bigint		not null,
    LastSyncTime			datetime	not null,
    LastSyncCompletedTime	datetime	not null,
    LastSyncStartedTime		datetime	not null,
    LastHitTime				datetime	not null,
    SyncEnabled				tinyint		not null,
    
	CONSTRAINT pk_ProductControl PRIMARY KEY CLUSTERED (ProductId ASC)
);


-- Create table TaskControl
CREATE TABLE TaskControl
(
    TaskType					int				not null,
    TaskState					int				not null, 
    LastSuccessfulRunTimeUtc	datetime		not null, 
    LastFailedRunTimeUtc		datetime		not null, 
    LastStartedTimeUtc			datetime		not null, 
    LastDurationInSeconds		int				not null, 
    RunCount					int				not null, 
    SuccessCount				int				not null, 
    FailedCount					int				not null, 
    LastTaskException			nvarchar(MAX)	null, 
    ServiceErrorCode			int				not null,

    CONSTRAINT pk_TaskType PRIMARY KEY (TaskType)
);


-- Create table Files
CREATE TABLE Files
(
    FileId				bigint			not null,
    FileName			nvarchar(400)	not null,
    Version				nvarchar(24)	not null,
    DateCreatedLocal	smalldatetime	not null,
    DateModifiedLocal	smalldatetime	not null,
    LinkDateLocal		smalldatetime	not null,

    CONSTRAINT pk_Files PRIMARY KEY CLUSTERED (FileId ASC)
);


-- Create table Events
CREATE TABLE Events
(
    EventId					bigint			not null,
    EventTypeId				smallint		not null,
    DateCreatedLocal		smalldatetime	not null,
    DateModifiedLocal		smalldatetime	not null,
    TotalHits				bigint			not null,
    ApplicationName			nvarchar(400)	null, 
    ApplicationVersion		nvarchar(50)	null, 
    ApplicationTimeStamp	datetime		null, 
    ModuleName				nvarchar(400)	null, 
    ModuleVersion			nvarchar(50)	null, 
    ModuleTimeStamp			datetime		null, 
    OffsetOriginal			nvarchar(50)	null, 
    ExceptionCodeOriginal	nvarchar(15)	null, 
    Offset					bigint			not null,
    ExceptionCode			bigint			not null, 
    BugId					nvarchar(MAX)	null, 
    PlugInBugId				nvarchar(MAX)	null, 
	WorkFlowStatus			int				not null,

    CONSTRAINT pk_Events PRIMARY KEY CLUSTERED (EventId ASC, EventTypeId ASC), 
    CONSTRAINT fk_EventsToEventTypes FOREIGN KEY (EventTypeId) REFERENCES EventTypes(EventTypeId)
);


-- Create table FileEvents
CREATE TABLE FileEvents
(
    FileId			bigint		not null,
    EventId			bigint		not null, 
    EventTypeId		smallint	not null, 

    CONSTRAINT pk_FileEvents PRIMARY KEY CLUSTERED (FileId ASC, EventId ASC, EventTypeId ASC), 
    CONSTRAINT fk_FileEventsToEvents FOREIGN KEY (EventId, EventTypeId) REFERENCES Events(EventId,EventTypeId), 
    CONSTRAINT fk_FileEventsToFiles FOREIGN KEY (FileId) REFERENCES Files(FileId) 
);


-- Create table ProductFiles
CREATE TABLE ProductFiles
(
    ProductId bigint not null, 
    FileId bigint not null, 
    
	CONSTRAINT pk_ProductFiles PRIMARY KEY CLUSTERED (ProductId ASC, FileId ASC), 
    CONSTRAINT fk_ProductFilesToProducts FOREIGN KEY (ProductId) REFERENCES Products(ProductId), 
    CONSTRAINT fk_ProductFilesToFiles FOREIGN KEY (FileId) REFERENCES Files(FileId)
);



-- Create table Cabs
CREATE TABLE Cabs
(
    CabId					bigint, 
    SizeInBytes				bigint			not null, 
    CabFileName				nvarchar(550)	not null, 
    DateCreatedLocal		smalldatetime	not null, 
    DateModifiedLocal		smalldatetime	not null, 
    EventId					bigint			not null, 
    EventTypeId				smallint		not null, 
    CabDownloaded			tinyint			not null, 
    Purged					tinyint			not null, 
    SystemUpTime			nvarchar(100)	null, 
    ProcessUpTime			nvarchar(100)	null, 
    DotNetVersion			nvarchar(100)	null, 
    OSVersion				nvarchar(200)	null, 
    MachineArchitecture		nvarchar(20)	null,

    CONSTRAINT pk_Cabs PRIMARY KEY CLUSTERED (CabId ASC), 
    CONSTRAINT fk_CabsToEvents FOREIGN KEY (EventId, EventTypeId) REFERENCES Events(EventId, EventTypeId),
    CONSTRAINT fk_CabsToEventTypes FOREIGN KEY (EventTypeId) REFERENCES EventTypes(EventTypeId)
);


-- Create table CabNotes
CREATE TABLE CabNotes
(
    NoteId			int				identity(1,1), 
    CabId			bigint			not null,
    TimeOfEntry		datetime		not null, 
    SourceId		smallint		not null, 
    UserId			smallint		not null, 
    Note			nvarchar(MAX)	not null, 

    CONSTRAINT pk_CabNotes PRIMARY KEY CLUSTERED (NoteId ASC), 
    CONSTRAINT fk_CabNotesToCabs FOREIGN KEY (CabId) REFERENCES Cabs(CabId)
);


-- Create table EventNotes
CREATE TABLE EventNotes
(
    NoteId			int				identity(1,1), 
    EventId			bigint			not null, 
    EventTypeId		smallint		not null, 
    TimeOfEntry		datetime		not null, 
    SourceId		smallint		not null, 
    UserId			smallint		not null, 
    Note			nvarchar(MAX)	not null, 

    CONSTRAINT pk_EventNotes PRIMARY KEY CLUSTERED (NoteId ASC), 
    CONSTRAINT fk_EventNotesToEvents FOREIGN KEY (EventId, EventTypeId) REFERENCES Events(EventId, EventTypeId)
);


-- Create table Users
CREATE TABLE Users
(
    UserId			smallint		identity(1,1) not null, 
    StackHashUser	nvarchar(100)	not null,

    CONSTRAINT pk_Users PRIMARY KEY CLUSTERED (UserId ASC) 
);


-- Create table Sources
CREATE TABLE Sources
(
    SourceId	smallint		identity(1,1) not null, 
    Source		nvarchar(20)	not null, 

    CONSTRAINT pk_Sources PRIMARY KEY CLUSTERED (SourceId ASC) 
)


-- Create table EventInfos
CREATE TABLE EventInfos
(
    EventId				bigint			not null, 
    EventTypeId			smallint		not null, 
    DateCreatedLocal	smalldatetime	not null, 
    DateModifiedLocal	smalldatetime	not null, 
    HitDateLocal		datetime		not null, 
    LocaleId			smallint		not null, 
    TotalHits			int				not null, 
    OperatingSystemId	smallint		not null, 

    CONSTRAINT pk_EventInfos PRIMARY KEY (EventId, EventTypeId, HitDateLocal, OperatingSystemId, LocaleId), 
    CONSTRAINT fk_EventInfosToEventId FOREIGN KEY (EventId, EventTypeId) REFERENCES Events(EventId, EventTypeId),
    CONSTRAINT fk_EventInfosToOperatingSystems FOREIGN KEY (OperatingSystemId) REFERENCES OperatingSystems(OperatingSystemId),
    CONSTRAINT fk_EventInfosToLocales FOREIGN KEY (LocaleId) REFERENCES Locales(LocaleId)
);



-- Create table LocalSummary
IF OBJECT_ID('dbo.LocaleSummary', 'U') IS NULL 
CREATE TABLE LocaleSummary
(
    ProductId	bigint		not null, 
    LocaleId	smallint	not null, 
    TotalHits	bigint		not null, 
    
	CONSTRAINT pk_LocaleSummary PRIMARY KEY CLUSTERED (LocaleId ASC, ProductId ASC), 
    CONSTRAINT fk_LocaleSummaryToLocales FOREIGN KEY (LocaleId) REFERENCES Locales(LocaleId) 
);


-- Create table OperatingSystemSummary
IF OBJECT_ID('dbo.OperatingSystemSummary', 'U') IS NULL 
CREATE TABLE OperatingSystemSummary
(
    ProductId			bigint		not null, 
    OperatingSystemId	smallint	not null, 
    TotalHits			bigint		not null, 

    CONSTRAINT pk_OperatingSystemSummary PRIMARY KEY CLUSTERED (OperatingSystemId ASC, ProductId ASC), 
    CONSTRAINT fk_OperatingSystemSummaryToOperatingSystems FOREIGN KEY (OperatingSystemId) REFERENCES OperatingSystems(OperatingSystemId) 
);


-- Create table HitDateSummary
IF OBJECT_ID('dbo.HitDateSummary', 'U') IS NULL 
CREATE TABLE HitDateSummary
(
    ProductId	bigint		not null, 
    HitDateUtc	datetime	not null, 
    TotalHits	bigint		not null, 

    CONSTRAINT pk_HitDateSummary PRIMARY KEY CLUSTERED (HitDateUtc ASC, ProductId ASC) 
);


-- Create table UpdateTable
IF OBJECT_ID('dbo.UpdateTable', 'U') IS NULL 
CREATE TABLE UpdateTable
(
    EntryId int identity(1,1) not null, 
    DateChanged datetime not null, 
    ProductId bigint not null,
    FileId bigint not null, 
    EventId bigint not null, 
    EventTypeId smallint not null, 
    CabId bigint not null, 
    ChangedObjectId bigint not null, 
    DataThatChanged int not null, 
    TypeOfChange int not null, 
    
	CONSTRAINT pk_Updates PRIMARY KEY CLUSTERED (EntryId ASC)
);

-- Create table WorkFlow mappings table.
IF OBJECT_ID('dbo.WorkFlowMappings', 'U') IS NULL 
CREATE TABLE WorkFlowMappings
(
    WorkFlowStatusId int not null, 
    WorkFlowStatusName nvarchar(MAX) not null, 
    
    CONSTRAINT pk_WorkFlowMappings PRIMARY KEY CLUSTERED (WorkFlowStatusId ASC), 
);

-- Create table Group mappings table
IF OBJECT_ID('dbo.GroupMappings', 'U') IS NULL 
CREATE TABLE GroupMappings
(
    GroupId int not null, 
    GroupName nvarchar(MAX) not null, 
    
    CONSTRAINT pk_GroupMappings PRIMARY KEY CLUSTERED (GroupId ASC), 
);

GO
