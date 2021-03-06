/*
   vrijdag 10 juni 201110:07:58
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
ALTER TABLE dbo.Connector
	DROP CONSTRAINT FK_Connector_Vendor
GO
ALTER TABLE dbo.Vendor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT FK_Connector_ConnectorSystem
GO
ALTER TABLE dbo.ConnectorSystem SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT DF_Connector_ConcatenateBrandName
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT DF_Connector_ObsoleteProducts
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT DF_Connector_ZipCodes
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT DF_Connector_Selectors
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT DF_Connector_OverrideDescriptions
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT DF_Connector_UseConcentratorProductID
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT DF_Connector_ImportCommercialText
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT DF_Connector_IsActive
GO
CREATE TABLE dbo.Tmp_Connector
	(
	ConnectorID int NOT NULL IDENTITY (1, 1),
	Name nvarchar(50) NOT NULL,
	ConnectorType int NOT NULL,
	ConnectorSystemID int NULL,
	ConcatenateBrandName bit NOT NULL,
	ObsoleteProducts bit NOT NULL,
	ZipCodes bit NOT NULL,
	Selectors bit NOT NULL,
	OverrideDescriptions bit NOT NULL,
	BSKIdentifier int NULL,
	BackendEanIdentifier nvarchar(100) NULL,
	UseConcentratorProductID bit NOT NULL,
	Connection nvarchar(255) NULL,
	ImportCommercialText bit NOT NULL,
	IsActive bit NOT NULL,
	AdministrativeVendorID int NULL,
	OutboundUrl nvarchar(255) NULL,
	ParentConnectorID int NULL,
	DefaultImage nvarchar(255) NULL,
	ConnectorSystemType int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_Connector SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_Connector ADD CONSTRAINT
	DF_Connector_ConcatenateBrandName DEFAULT ((0)) FOR ConcatenateBrandName
GO
ALTER TABLE dbo.Tmp_Connector ADD CONSTRAINT
	DF_Connector_ObsoleteProducts DEFAULT ((0)) FOR ObsoleteProducts
GO
ALTER TABLE dbo.Tmp_Connector ADD CONSTRAINT
	DF_Connector_ZipCodes DEFAULT ((0)) FOR ZipCodes
GO
ALTER TABLE dbo.Tmp_Connector ADD CONSTRAINT
	DF_Connector_Selectors DEFAULT ((0)) FOR Selectors
GO
ALTER TABLE dbo.Tmp_Connector ADD CONSTRAINT
	DF_Connector_OverrideDescriptions DEFAULT ((0)) FOR OverrideDescriptions
GO
ALTER TABLE dbo.Tmp_Connector ADD CONSTRAINT
	DF_Connector_UseConcentratorProductID DEFAULT ((0)) FOR UseConcentratorProductID
GO
ALTER TABLE dbo.Tmp_Connector ADD CONSTRAINT
	DF_Connector_ImportCommercialText DEFAULT ((0)) FOR ImportCommercialText
GO
ALTER TABLE dbo.Tmp_Connector ADD CONSTRAINT
	DF_Connector_IsActive DEFAULT ((0)) FOR IsActive
GO
SET IDENTITY_INSERT dbo.Tmp_Connector ON
GO
IF EXISTS(SELECT * FROM dbo.Connector)
	 EXEC('INSERT INTO dbo.Tmp_Connector (ConnectorID, Name, ConnectorType, ConnectorSystemID, ConcatenateBrandName, ObsoleteProducts, ZipCodes, Selectors, OverrideDescriptions, BSKIdentifier, BackendEanIdentifier, UseConcentratorProductID, Connection, ImportCommercialText, IsActive, AdministrativeVendorID, OutboundUrl, ParentConnectorID, DefaultImage, ConnectorSystemType)
		SELECT ConnectorID, Name, ConnectorType, ConnectorSystemID, ConcatenateBrandName, ObsoleteProducts, ZipCodes, Selectors, OverrideDescriptions, BSKIdentifier, BackendEanIdentifier, UseConcentratorProductID, Connection, ImportCommercialText, IsActive, AdministrativeVendorID, OutboundUrl, ParentConnectorID, DefaultImage, ConnectorSystemType FROM dbo.Connector WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_Connector OFF
GO
ALTER TABLE dbo.Connector
	DROP CONSTRAINT Connector_ParentConnector
GO
ALTER TABLE dbo.AdditionalOrderProduct
	DROP CONSTRAINT FK_AdditionalOrderProduct_Connector
GO
ALTER TABLE dbo.AttributeMatchStore
	DROP CONSTRAINT FK_AttributeMatchStore_Connector
GO
ALTER TABLE dbo.ConnectorLanguage
	DROP CONSTRAINT FK_ConnectorLanguage_Connector
GO
ALTER TABLE dbo.ConnectorPaymentProvider
	DROP CONSTRAINT FK_ConnectorPaymentProvider_Connector
GO
ALTER TABLE dbo.ConnectorPublication
	DROP CONSTRAINT FK_ConnectorPublication_Connector
GO
ALTER TABLE dbo.ConnectorRuleValue
	DROP CONSTRAINT FK_ConnectorRuleValue_Connector
GO
ALTER TABLE dbo.ConnectorSchedule
	DROP CONSTRAINT FK_ConnectorSchedule_Connector
GO
ALTER TABLE dbo.ConnectorSetting
	DROP CONSTRAINT FK_ConnectorSetting_Connector
GO
ALTER TABLE dbo.[Content]
	DROP CONSTRAINT FK_Content_Connector
GO
ALTER TABLE dbo.ContentPrice
	DROP CONSTRAINT FK_ContentPrice_Connector
GO
ALTER TABLE dbo.ContentProduct
	DROP CONSTRAINT FK_ContentProduct_Connector
GO
ALTER TABLE dbo.ContentProductGroup
	DROP CONSTRAINT FK_ContentProductGroup_Connector
GO
ALTER TABLE dbo.ContentVendorSetting
	DROP CONSTRAINT FK_ContentVendorSetting_Connector
GO
ALTER TABLE dbo.CrossLedgerclass
	DROP CONSTRAINT FK_CrossLedgerclass_Connector
GO
ALTER TABLE dbo.EdiOrder
	DROP CONSTRAINT FK_EdiOrder_Connector
GO
ALTER TABLE dbo.[Order]
	DROP CONSTRAINT FK_Order_Connector
GO
ALTER TABLE dbo.Outbound
	DROP CONSTRAINT FK_Outbound_Connector
GO
ALTER TABLE dbo.PreferredConnectorVendor
	DROP CONSTRAINT FK_PreferredConnectorVendor_Connector
GO
ALTER TABLE dbo.ProductAttributeGroupMetaData
	DROP CONSTRAINT FK_ProductAttributeGroup_ProductAttributeGroup
GO
ALTER TABLE dbo.ProductGroupMapping
	DROP CONSTRAINT FK_ProductGroupMapping_Connector
GO
ALTER TABLE dbo.ProductGroupPublish
	DROP CONSTRAINT FK_ProductGroupPublish_Connector
GO
ALTER TABLE dbo.[User]
	DROP CONSTRAINT FK_User_Connector
GO
ALTER TABLE dbo.ConnectorRelation
	DROP CONSTRAINT FK_ConnectorRelation_Connector
GO
DROP TABLE dbo.Connector
GO
EXECUTE sp_rename N'dbo.Tmp_Connector', N'Connector', 'OBJECT' 
GO
ALTER TABLE dbo.Connector ADD CONSTRAINT
	PK_Connector PRIMARY KEY CLUSTERED 
	(
	ConnectorID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Connector ADD CONSTRAINT
	Connector_ParentConnector FOREIGN KEY
	(
	ParentConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Connector ADD CONSTRAINT
	FK_Connector_ConnectorSystem FOREIGN KEY
	(
	ConnectorSystemID
	) REFERENCES dbo.ConnectorSystem
	(
	ConnectorSystemID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Connector ADD CONSTRAINT
	FK_Connector_Vendor FOREIGN KEY
	(
	AdministrativeVendorID
	) REFERENCES dbo.Vendor
	(
	VendorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorRelation ADD CONSTRAINT
	FK_ConnectorRelation_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorRelation SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[User] ADD CONSTRAINT
	FK_User_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[User] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupPublish ADD CONSTRAINT
	FK_ProductGroupPublish_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductGroupPublish SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductGroupMapping ADD CONSTRAINT
	FK_ProductGroupMapping_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductGroupMapping SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductAttributeGroupMetaData ADD CONSTRAINT
	FK_ProductAttributeGroup_ProductAttributeGroup FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductAttributeGroupMetaData SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.PreferredConnectorVendor ADD CONSTRAINT
	FK_PreferredConnectorVendor_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.PreferredConnectorVendor SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Outbound ADD CONSTRAINT
	FK_Outbound_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Outbound SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Order] ADD CONSTRAINT
	FK_Order_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[Order] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.EdiOrder ADD CONSTRAINT
	FK_EdiOrder_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.EdiOrder SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CrossLedgerclass ADD CONSTRAINT
	FK_CrossLedgerclass_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CrossLedgerclass SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ContentVendorSetting ADD CONSTRAINT
	FK_ContentVendorSetting_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ContentVendorSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ContentProductGroup ADD CONSTRAINT
	FK_ContentProductGroup_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ContentProductGroup SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ContentProduct ADD CONSTRAINT
	FK_ContentProduct_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ContentProduct SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ContentPrice ADD CONSTRAINT
	FK_ContentPrice_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ContentPrice SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Content] ADD CONSTRAINT
	FK_Content_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[Content] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorSetting ADD CONSTRAINT
	FK_ConnectorSetting_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorSetting SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorSchedule ADD CONSTRAINT
	FK_ConnectorSchedule_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorSchedule SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorRuleValue ADD CONSTRAINT
	FK_ConnectorRuleValue_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.ConnectorRuleValue SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorPublication ADD CONSTRAINT
	FK_ConnectorPublication_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorPublication SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorPaymentProvider ADD CONSTRAINT
	FK_ConnectorPaymentProvider_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorPaymentProvider SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ConnectorLanguage ADD CONSTRAINT
	FK_ConnectorLanguage_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorLanguage SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.AttributeMatchStore ADD CONSTRAINT
	FK_AttributeMatchStore_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.AttributeMatchStore SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.AdditionalOrderProduct ADD CONSTRAINT
	FK_AdditionalOrderProduct_Connector FOREIGN KEY
	(
	ConnectorID
	) REFERENCES dbo.Connector
	(
	ConnectorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.AdditionalOrderProduct SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
