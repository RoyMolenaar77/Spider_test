/*
   dinsdag 8 maart 201111:40:46
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
EXECUTE sp_rename N'dbo.EdiOrderPost.OrderID', N'Tmp_EdiOrderID', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.EdiOrderPost.BackendOrderID', N'Tmp_EdiBackendOrderID_1', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.EdiOrderPost.Tmp_EdiOrderID', N'EdiOrderID', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.EdiOrderPost.Tmp_EdiBackendOrderID_1', N'EdiBackendOrderID', 'COLUMN' 
GO
ALTER TABLE dbo.EdiOrderPost SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
