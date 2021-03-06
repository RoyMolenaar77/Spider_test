/*
   maandag 20 juni 201115:34:02
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
ALTER TABLE dbo.VendorAssortment
	DROP CONSTRAINT FK_VendorAssortment_Product
GO
ALTER TABLE dbo.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.VendorAssortment
	DROP CONSTRAINT FK_VendorAssortment_Vendor
GO
ALTER TABLE dbo.Vendor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.VendorAssortment
	DROP CONSTRAINT DF__VendorAss__IsAct__66760F55
GO
CREATE TABLE dbo.Tmp_VendorAssortment
	(
	VendorAssortmentID int NOT NULL IDENTITY (1, 1),
	ProductID int NOT NULL,
	CustomItemNumber nvarchar(50) NOT NULL,
	VendorID int NOT NULL,
	ShortDescription nvarchar(150) NULL,
	LongDescription nvarchar(1000) NULL,
	LineType nvarchar(50) NULL,
	LedgerClass nvarchar(50) NULL,
	ExtendedCatalog bit NULL,
	ProductDesk nvarchar(150) NULL,
	ActivationKey nvarchar(255) NULL,
	IsActive bit NOT NULL,
	ZoneReferenceID nvarchar(255) NULL,
	ShipmentRateTableReferenceID nvarchar(255) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_VendorAssortment SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_VendorAssortment ADD CONSTRAINT
	DF__VendorAss__IsAct__66760F55 DEFAULT ((0)) FOR IsActive
GO
SET IDENTITY_INSERT dbo.Tmp_VendorAssortment ON
GO
IF EXISTS(SELECT * FROM dbo.VendorAssortment)
	 EXEC('INSERT INTO dbo.Tmp_VendorAssortment (VendorAssortmentID, ProductID, CustomItemNumber, VendorID, ShortDescription, LongDescription, LineType, LedgerClass, ExtendedCatalog, ProductDesk, ActivationKey, IsActive, ZoneReferenceID, ShipmentRateTableReferenceID)
		SELECT VendorAssortmentID, ProductID, CustomItemNumber, VendorID, ShortDescription, LongDescription, LineType, LedgerClass, ExtendedCatalog, ProductDesk, ActivationKey, IsActive, ZoneReferenceID, ShipmentRateTableReferenceID FROM dbo.VendorAssortment WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_VendorAssortment OFF
GO
ALTER TABLE dbo.ContentLedger
	DROP CONSTRAINT FK_ContentLedger_VendorAssortment
GO
ALTER TABLE dbo.VendorAccruel
	DROP CONSTRAINT FK_VendorAccruel_VendorAssortment
GO
ALTER TABLE dbo.VendorFreeGoods
	DROP CONSTRAINT FK_VendorFreeGoods_VendorAssortment
GO
ALTER TABLE dbo.VendorStock
	DROP CONSTRAINT FK_VendorStock_VendorAssortment
GO
ALTER TABLE dbo.VendorProductGroupAssortment
	DROP CONSTRAINT FK_VendorProductGroupAssortment_VendorAssortment
GO
ALTER TABLE dbo.VendorPrice
	DROP CONSTRAINT FK_VendorPrice_VendorAssortment
GO
DROP TABLE dbo.VendorAssortment
GO
EXECUTE sp_rename N'dbo.Tmp_VendorAssortment', N'VendorAssortment', 'OBJECT' 
GO
ALTER TABLE dbo.VendorAssortment ADD CONSTRAINT
	PK_VendorAssortment PRIMARY KEY CLUSTERED 
	(
	VendorAssortmentID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE UNIQUE NONCLUSTERED INDEX IX_VendorAssortment ON dbo.VendorAssortment
	(
	ProductID,
	VendorID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX CustomItemNumber ON dbo.VendorAssortment
	(
	CustomItemNumber,
	ProductID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_VendorID ON dbo.VendorAssortment
	(
	VendorID
	) INCLUDE (VendorAssortmentID, ProductID) 
 WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX IX_ProductID_ShortDescription ON dbo.VendorAssortment
	(
	ProductID,
	ShortDescription
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE dbo.VendorAssortment ADD CONSTRAINT
	FK_VendorAssortment_Vendor FOREIGN KEY
	(
	VendorID
	) REFERENCES dbo.Vendor
	(
	VendorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.VendorAssortment ADD CONSTRAINT
	FK_VendorAssortment_Product FOREIGN KEY
	(
	ProductID
	) REFERENCES dbo.Product
	(
	ProductID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.VendorPrice ADD CONSTRAINT
	FK_VendorPrice_VendorAssortment FOREIGN KEY
	(
	VendorAssortmentID
	) REFERENCES dbo.VendorAssortment
	(
	VendorAssortmentID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.VendorPrice SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.VendorProductGroupAssortment ADD CONSTRAINT
	FK_VendorProductGroupAssortment_VendorAssortment FOREIGN KEY
	(
	VendorAssortmentID
	) REFERENCES dbo.VendorAssortment
	(
	VendorAssortmentID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.VendorProductGroupAssortment SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.VendorStock WITH NOCHECK ADD CONSTRAINT
	FK_VendorStock_VendorAssortment FOREIGN KEY
	(
	ProductID,
	VendorID
	) REFERENCES dbo.VendorAssortment
	(
	ProductID,
	VendorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.VendorStock
	NOCHECK CONSTRAINT FK_VendorStock_VendorAssortment
GO
ALTER TABLE dbo.VendorStock SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.VendorFreeGoods ADD CONSTRAINT
	FK_VendorFreeGoods_VendorAssortment FOREIGN KEY
	(
	VendorAssortmentID
	) REFERENCES dbo.VendorAssortment
	(
	VendorAssortmentID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.VendorFreeGoods SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.VendorAccruel ADD CONSTRAINT
	FK_VendorAccruel_VendorAssortment FOREIGN KEY
	(
	VendorAssortmentID
	) REFERENCES dbo.VendorAssortment
	(
	VendorAssortmentID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.VendorAccruel SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ContentLedger ADD CONSTRAINT
	FK_ContentLedger_VendorAssortment FOREIGN KEY
	(
	VendorAssortmentID
	) REFERENCES dbo.VendorAssortment
	(
	VendorAssortmentID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ContentLedger SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
