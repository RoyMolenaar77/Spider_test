/*
   maandag 27 juni 201113:07:57
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
ALTER TABLE dbo.Connector SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
EXECUTE sp_rename N'dbo.ProductCompare.TotalStoc', N'Tmp_TotalStock_2', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.ProductCompare.Tmp_TotalStock_2', N'TotalStock', 'COLUMN' 
GO
ALTER TABLE dbo.ProductCompare ADD CONSTRAINT
	FK_ProductCompare_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductCompare SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
