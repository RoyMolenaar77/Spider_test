/*
   donderdag 23 juni 201110:05:56
   User: 
   Server: DIRACT-025\SQL2008
   Database: Concentrator_storage
   Application: 
*/

/* To prevent any potential data loss issues, you should review this script in detail before running it outside the context of the database designer.*/
BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorProductStatus
	DROP CONSTRAINT FK_ConnectorProductStatus_AssortmentStatus
GO
ALTER TABLE dbo.AssortmentStatus SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_ConnectorProductStatus
	(
	ProductStatusID int NOT NULL IDENTITY (1, 1),
	ConnectorID int NOT NULL,
	ConnectorStatus nvarchar(50) NOT NULL,
	ConcentratorStatusID int NOT NULL,
	ConnectorStatusID int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_ConnectorProductStatus SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_ConnectorProductStatus ON
GO
IF EXISTS(SELECT * FROM dbo.ConnectorProductStatus)
	 EXEC('INSERT INTO dbo.Tmp_ConnectorProductStatus (ProductStatusID, ConnectorID, ConnectorStatus, ConcentratorStatusID, ConnectorStatusID)
		SELECT ProductStatusID, ConnectorID, ConnectorStatus, ConcentratorStatusID, ConnectorStatusID FROM dbo.ConnectorProductStatus WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_ConnectorProductStatus OFF
GO
DROP TABLE dbo.ConnectorProductStatus
GO
EXECUTE sp_rename N'dbo.Tmp_ConnectorProductStatus', N'ConnectorProductStatus', 'OBJECT' 
GO
ALTER TABLE dbo.ConnectorProductStatus ADD CONSTRAINT
	PK_ConnectorProductStatus_1 PRIMARY KEY CLUSTERED 
	(
	ProductStatusID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ConnectorProductStatus ADD CONSTRAINT
	FK_ConnectorProductStatus_AssortmentStatus FOREIGN KEY
	(
	ConcentratorStatusID
	) REFERENCES dbo.AssortmentStatus
	(
	StatusID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
