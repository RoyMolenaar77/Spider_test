/*
   Friday, February 25, 20113:02:25 PM
   User: 
   Server: localhost\sqlexpress
   Database: Concentrator_staging_new
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
ALTER TABLE dbo.ProductMedia ADD
	Resolution nvarchar(50) NULL,
	Size int NULL
GO
ALTER TABLE dbo.ProductMedia SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
