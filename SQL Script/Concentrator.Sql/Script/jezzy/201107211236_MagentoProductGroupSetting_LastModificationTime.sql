/*
   donderdag 21 juli 201112:35:06
   User: spider
   Server: .
   Database: Concentrator_dev_entity
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
EXECUTE sp_rename N'dbo.MagentoProductGroupSetting.LastModifiactionTime', N'Tmp_LastModificationTime', 'COLUMN' 
GO
EXECUTE sp_rename N'dbo.MagentoProductGroupSetting.Tmp_LastModificationTime', N'LastModificationTime', 'COLUMN' 
GO
ALTER TABLE dbo.MagentoProductGroupSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
