/*
   donderdag 23 juni 201110:27:43
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
ALTER TABLE dbo.EdiOrderPost
	DROP CONSTRAINT FK_EdiOrderPost_EdiOrderListener
GO
ALTER TABLE dbo.EdiOrderListener SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.EdiOrderPost
	DROP CONSTRAINT FK_EdiOrderPost_Customer
GO
ALTER TABLE dbo.Customer SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.EdiOrderPost
	DROP CONSTRAINT FK_EdiOrderPost_Connector
GO
ALTER TABLE dbo.Connector SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.EdiOrderPost
	DROP CONSTRAINT FK_EdiOrderPost_EdiOrder
GO
ALTER TABLE dbo.EdiOrder SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.EdiOrderPost ADD CONSTRAINT
	FK_EdiOrderPost_EdiOrder FOREIGN KEY
	(
	EdiOrderID
	) REFERENCES dbo.EdiOrder
	(
	EdiOrderID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.EdiOrderPost ADD CONSTRAINT
	FK_EdiOrderPost_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.EdiOrderPost ADD CONSTRAINT
	FK_EdiOrderPost_Customer FOREIGN KEY
	(
	CustomerID
	) REFERENCES dbo.Customer
	(
	CustomerID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.EdiOrderPost ADD CONSTRAINT
	FK_EdiOrderPost_EdiOrderListener FOREIGN KEY
	(
	EdiRequestID
	) REFERENCES dbo.EdiOrderListener
	(
	EdiRequestID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.EdiOrderPost SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
