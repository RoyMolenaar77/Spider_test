/*
   donderdag 3 maart 201117:01:17
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
EXECUTE sp_rename N'dbo.EdiOrderResponseLine.EdOrderLineID', N'Tmp_EdiOrderLineID_5', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.EdiOrderResponseLine.Tmp_EdiOrderLineID_5', N'EdiOrderLineID', 'COLUMN' 
GO
ALTER TABLE dbo.EdiOrderResponseLine SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
