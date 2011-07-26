---------------------------------------------------------------------
-- StackHash database update script.  Copyright 2011 - Cucku, Inc. 
--
-- This script updates the StackHash database to the latest structure.
--
-- Supported versions of SQL Server: 2005, 2008
--
-- Last updated: 06-April-2012
---------------------------------------------------------------------

---------------------------------------------------------------------
-- Update StackHash database.
---------------------------------------------------------------------

-- Comment back in this USE statement with the database name to be updated.
-- USE StackHash;

-- Perform some preliminary checks to make sure the user is trying to update
-- a StackHash database.
IF OBJECT_ID('dbo.LocaleSummary', 'U') IS NULL 
   RAISERROR('You must have an existing StackHash database to perform an update. If you do not have a StackHash database the create on using CreateStackHashDatabase.sql', 127, 127) WITH NOWAIT, LOG;
GO


---------------------------------------------------------------------
-- Updates from 0.90 to 0.95.
---------------------------------------------------------------------

IF NOT EXISTS(SELECT * FROM sys.columns 
			  WHERE Name = N'PlugInBugId'    
			  AND Object_ID = Object_ID(N'Events'))  
BEGIN 
	IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL 
		ALTER TABLE dbo.Events ADD PlugInBugId nvarchar(MAX) null 
END


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


---------------------------------------------------------------------
-- Updates from 0.95 to 1.20.
---------------------------------------------------------------------

IF NOT EXISTS(SELECT * FROM sys.columns 
			  WHERE Name = N'WorkFlowStatus'    
			  AND Object_ID = Object_ID(N'Events'))  
BEGIN 
	IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL 
		ALTER TABLE dbo.Events ADD WorkFlowStatus int DEFAULT 0 NOT NULL
 END


-- Create table WorkFlowMappings table
IF OBJECT_ID('dbo.WorkFlowMappings', 'U') IS NULL 
CREATE TABLE WorkFlowMappings
(
    WorkFlowStatusId int not null, 
    WorkFlowStatusName nvarchar(MAX) not null, 
    
    CONSTRAINT pk_WorkFlowMappings PRIMARY KEY CLUSTERED (WorkFlowStatusId ASC), 
);

-- Create table GroupMappings table
IF OBJECT_ID('dbo.GroupMappings', 'U') IS NULL 
CREATE TABLE GroupMappings
(
    GroupId int not null, 
    GroupName nvarchar(MAX) not null, 
    
    CONSTRAINT pk_GroupMappings PRIMARY KEY CLUSTERED (GroupId ASC), 
);

UPDATE dbo.Control 
SET DatabaseVersion=3
WHERE ControlItemId=1

GO

