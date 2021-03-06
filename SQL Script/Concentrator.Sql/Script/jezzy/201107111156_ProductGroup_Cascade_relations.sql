/*
   maandag 11 juli 201111:56:17
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
ALTER TABLE dbo.ProductGroup
	DROP CONSTRAINT DF_ProductGroup_Score
GO
CREATE TABLE dbo.Tmp_ProductGroup
	(
	ProductGroupID int NOT NULL,
	Score int NOT NULL,
	Searchable bit NULL,
	ImagePath nvarchar(255) NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_ProductGroup SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_ProductGroup ADD CONSTRAINT
	DF_ProductGroup_Score DEFAULT ((0)) FOR Score
GO
IF EXISTS(SELECT * FROM dbo.ProductGroup)
	 EXEC('INSERT INTO dbo.Tmp_ProductGroup (ProductGroupID, Score, Searchable, ImagePath)
		SELECT ProductGroupID, Score, Searchable, ImagePath FROM dbo.ProductGroup WITH (HOLDLOCK TABLOCKX)')
GO
ALTER TABLE dbo.ConnectorPublication
	DROP CONSTRAINT FK_ConnectorPublication_ProductGroup
GO
ALTER TABLE dbo.ProductGroupPublish
	DROP CONSTRAINT FK_ProductGroupPublish_ProductGroup
GO
ALTER TABLE dbo.ContentProduct
	DROP CONSTRAINT FK_ContentProduct_ProductGroup
GO
ALTER TABLE dbo.ProductGroupLanguage
	DROP CONSTRAINT FK_ProductGroupLanguage_ProductGroup
GO
ALTER TABLE dbo.ContentVendorSetting
	DROP CONSTRAINT FK_ContentVendorSetting_ProductGroup
GO
ALTER TABLE dbo.ProductGroupSelector
	DROP CONSTRAINT FK_ProductGroupSelector_ProductGroup
GO
ALTER TABLE dbo.ProductGroupContentVendor
	DROP CONSTRAINT FK_ProductGroupContentVendor_ProductGroup
GO
ALTER TABLE dbo.ProductGroupVendor
	DROP CONSTRAINT FK_ProductGroupVendor_ProductGroup
GO
ALTER TABLE dbo.ProductGroupConnectorVendor
	DROP CONSTRAINT FK_PreferredVendorProductGroup_ProductGroup
GO
ALTER TABLE dbo.ProductGroupMapping
	DROP CONSTRAINT FK_ProductGroupMapping_ProductGroup
GO
ALTER TABLE dbo.ContentPrice
	DROP CONSTRAINT FK_ContentPrice_ProductGroup
GO
ALTER TABLE dbo.MissingContent
	DROP CONSTRAINT FK_MissingContent_ProductGroup
GO
DROP TABLE dbo.ProductGroup
GO
EXECUTE sp_rename N'dbo.Tmp_ProductGroup', N'ProductGroup', 'OBJECT' 
GO
ALTER TABLE dbo.ProductGroup ADD CONSTRAINT
	PK_ProductGroup PRIMARY KEY CLUSTERED 
	(
	ProductGroupID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.MissingContent ADD CONSTRAINT
	FK_MissingContent_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.MissingContent SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ContentPrice ADD CONSTRAINT
	FK_ContentPrice_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ContentPrice SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupMapping ADD CONSTRAINT
	FK_ProductGroupMapping_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ProductGroupMapping SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupConnectorVendor WITH NOCHECK ADD CONSTRAINT
	FK_PreferredVendorProductGroup_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ProductGroupConnectorVendor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupVendor ADD CONSTRAINT
	FK_ProductGroupVendor_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ProductGroupVendor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupContentVendor ADD CONSTRAINT
	FK_ProductGroupContentVendor_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ProductGroupContentVendor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupSelector ADD CONSTRAINT
	FK_ProductGroupSelector_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ProductGroupSelector SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ContentVendorSetting ADD CONSTRAINT
	FK_ContentVendorSetting_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ContentVendorSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupLanguage ADD CONSTRAINT
	FK_ProductGroupLanguage_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ProductGroupLanguage SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ContentProduct ADD CONSTRAINT
	FK_ContentProduct_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ContentProduct SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupPublish ADD CONSTRAINT
	FK_ProductGroupPublish_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ProductGroupPublish SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorPublication ADD CONSTRAINT
	FK_ConnectorPublication_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ConnectorPublication SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
