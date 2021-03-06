/*
   Thursday, February 03, 20115:46:21 PM
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
ALTER TABLE dbo.BrandVendor
	DROP CONSTRAINT FK_BrandVendor_Brand1
GO
ALTER TABLE dbo.Brand SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_BrandVendor
	(
	BrandID int NOT NULL,
	VendorID int NOT NULL,
	VendorBrandCode nvarchar(150) NOT NULL,
	Name nvarchar(150) NULL,
	Logo nvarchar(255) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_BrandVendor SET (LOCK_ESCALATION = TABLE)
GO
IF EXISTS(SELECT * FROM dbo.BrandVendor)
	 EXEC('INSERT INTO dbo.Tmp_BrandVendor (BrandID, VendorID, VendorBrandCode, Name, Logo)
		SELECT BrandID, VendorID, VendorBrandCode, Name, Logo FROM dbo.BrandVendor WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.BrandVendor
GO
EXECUTE sp_rename N'dbo.Tmp_BrandVendor', N'BrandVendor', 'OBJECT' 
GO
ALTER TABLE dbo.BrandVendor ADD CONSTRAINT
	PK_BrandVendor_1 PRIMARY KEY CLUSTERED 
	(
	BrandID,
	VendorID,
	VendorBrandCode
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.BrandVendor ADD CONSTRAINT
	FK_BrandVendor_Brand1 FOREIGN KEY
	(
	BrandID
	) REFERENCES dbo.Brand
	(
	BrandID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
