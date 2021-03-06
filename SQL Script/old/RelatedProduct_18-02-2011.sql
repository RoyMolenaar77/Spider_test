/*
   vrijdag 18 februari 201112:11:21
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
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT FK_RelatedProduct_RelatedProductType
GO
ALTER TABLE dbo.RelatedProductType SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT FK_RelatedProduct_Vendor
GO
ALTER TABLE dbo.Vendor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT FK_RelatedProduct_User
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT FK_RelatedProduct_User1
GO
ALTER TABLE dbo.[User] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT FK_RelatedProduct_Product
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT FK_RelatedProduct_Product1
GO
ALTER TABLE dbo.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT DF_RelatedProduct_CreationTime
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT DF_RelatedProduct_VendorID
GO
CREATE TABLE dbo.Tmp_RelatedProduct
	(
	ProductID int NOT NULL,
	RelatedProductID int NOT NULL,
	Preferred bit NULL,
	Reversed bit NULL,
	CreationTime datetime NOT NULL,
	CreatedBy int NOT NULL,
	LastModificationTime datetime NULL,
	LastModifiedBy int NULL,
	VendorID int NOT NULL,
	RelatedProductTypeID int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_RelatedProduct SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_RelatedProduct ADD CONSTRAINT
	DF_RelatedProduct_CreationTime DEFAULT (getdate()) FOR CreationTime
GO
ALTER TABLE dbo.Tmp_RelatedProduct ADD CONSTRAINT
	DF_RelatedProduct_VendorID DEFAULT ((36)) FOR VendorID
GO
IF EXISTS(SELECT * FROM dbo.RelatedProduct)
	 EXEC('INSERT INTO dbo.Tmp_RelatedProduct (ProductID, RelatedProductID, Preferred, Reversed, CreationTime, CreatedBy, LastModificationTime, LastModifiedBy, VendorID, RelatedProductTypeID)
		SELECT ProductID, RelatedProductID, Preferred, Reversed, CreationTime, CreatedBy, LastModificationTime, LastModifiedBy, VendorID, RelatedProductTypeID FROM dbo.RelatedProduct WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.RelatedProduct
GO
EXECUTE sp_rename N'dbo.Tmp_RelatedProduct', N'RelatedProduct', 'OBJECT' 
GO
ALTER TABLE dbo.RelatedProduct ADD CONSTRAINT
	PK_RelatedProduct PRIMARY KEY CLUSTERED 
	(
	ProductID,
	RelatedProductID,
	VendorID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.RelatedProduct ADD CONSTRAINT
	FK_RelatedProduct_Product FOREIGN KEY
	(
	ProductID
	) REFERENCES dbo.Product
	(
	ProductID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RelatedProduct ADD CONSTRAINT
	FK_RelatedProduct_Product1 FOREIGN KEY
	(
	RelatedProductID
	) REFERENCES dbo.Product
	(
	ProductID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RelatedProduct ADD CONSTRAINT
	FK_RelatedProduct_User FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RelatedProduct ADD CONSTRAINT
	FK_RelatedProduct_User1 FOREIGN KEY
	(
	LastModifiedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RelatedProduct ADD CONSTRAINT
	FK_RelatedProduct_Vendor FOREIGN KEY
	(
	VendorID
	) REFERENCES dbo.Vendor
	(
	VendorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.RelatedProduct ADD CONSTRAINT
	FK_RelatedProduct_RelatedProductType FOREIGN KEY
	(
	RelatedProductTypeID
	) REFERENCES dbo.RelatedProductType
	(
	RelatedProductTypeID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
