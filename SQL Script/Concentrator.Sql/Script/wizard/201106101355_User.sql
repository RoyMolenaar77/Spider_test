/*
   vrijdag 10 juni 201113:54:28
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
ALTER TABLE dbo.[User]
	DROP CONSTRAINT FK_User_Connector
GO
ALTER TABLE dbo.Connector SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[User]
	DROP CONSTRAINT FK_User_Language
GO
ALTER TABLE dbo.Language SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[User]
	DROP CONSTRAINT DF_User_CreationTime
GO
ALTER TABLE dbo.[User]
	DROP CONSTRAINT DF_User_LanguageID
GO
ALTER TABLE dbo.[User]
	DROP CONSTRAINT DF__User__Timeout__14FBF414
GO
CREATE TABLE dbo.Tmp_User
	(
	UserID int NOT NULL IDENTITY (1, 1),
	Username varchar(50) NOT NULL,
	Password varchar(50) NOT NULL,
	Firstname nvarchar(50) NULL,
	Lastname nvarchar(50) NULL,
	IsActive bit NOT NULL,
	CreationTime datetime NOT NULL,
	CreatedBy int NOT NULL,
	LastModificationTime datetime NULL,
	LastModifiedBy int NULL,
	LanguageID int NOT NULL,
	ConnectorID int NULL,
	Logo nvarchar(50) NULL,
	Timeout int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_User SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE dbo.Tmp_User ADD CONSTRAINT
	DF_User_CreationTime DEFAULT (getdate()) FOR CreationTime
GO
ALTER TABLE dbo.Tmp_User ADD CONSTRAINT
	DF_User_LanguageID DEFAULT ((2)) FOR LanguageID
GO
ALTER TABLE dbo.Tmp_User ADD CONSTRAINT
	DF__User__Timeout__14FBF414 DEFAULT ((15)) FOR Timeout
GO
SET IDENTITY_INSERT dbo.Tmp_User ON
GO
IF EXISTS(SELECT * FROM dbo.[User])
	 EXEC('INSERT INTO dbo.Tmp_User (UserID, Username, Password, Firstname, Lastname, IsActive, CreationTime, CreatedBy, LastModificationTime, LastModifiedBy, LanguageID, ConnectorID, Logo, Timeout)
		SELECT UserID, Username, Password, Firstname, Lastname, IsActive, CreationTime, CreatedBy, LastModificationTime, LastModifiedBy, LanguageID, ConnectorID, Logo, Timeout FROM dbo.[User] WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_User OFF
GO
ALTER TABLE dbo.Event
	DROP CONSTRAINT FK_Event_User
GO
ALTER TABLE dbo.Event
	DROP CONSTRAINT FK_Event_User1
GO
ALTER TABLE dbo.UserRole
	DROP CONSTRAINT FK_UserRoles_User
GO
ALTER TABLE dbo.ProductCompetitorPrice
	DROP CONSTRAINT FK_ProductCompetitorPrice_User
GO
ALTER TABLE dbo.ProductCompetitorPrice
	DROP CONSTRAINT FK_ProductCompetitorPrice_User1
GO
ALTER TABLE dbo.ProductDescription
	DROP CONSTRAINT FK_ContentDescription_User
GO
ALTER TABLE dbo.ProductDescription
	DROP CONSTRAINT FK_ContentDescription_User1
GO
ALTER TABLE dbo.UserPortal
	DROP CONSTRAINT FK_UserPortals_User
GO
ALTER TABLE dbo.[Content]
	DROP CONSTRAINT FK_Content_User
GO
ALTER TABLE dbo.[Content]
	DROP CONSTRAINT FK_Content_User1
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT FK_RelatedProduct_User
GO
ALTER TABLE dbo.RelatedProduct
	DROP CONSTRAINT FK_RelatedProduct_User1
GO
ALTER TABLE dbo.UserState
	DROP CONSTRAINT FK_UserState_User
GO
ALTER TABLE dbo.Product
	DROP CONSTRAINT FK_Product_User
GO
ALTER TABLE dbo.Product
	DROP CONSTRAINT FK_Product_User1
GO
ALTER TABLE dbo.CrossLedgerclass
	DROP CONSTRAINT FK_CrossLedgerclass_User
GO
ALTER TABLE dbo.CrossLedgerclass
	DROP CONSTRAINT FK_CrossLedgerclass_User1
GO
DROP TABLE dbo.[User]
GO
EXECUTE sp_rename N'dbo.Tmp_User', N'User', 'OBJECT' 
GO
ALTER TABLE dbo.[User] ADD CONSTRAINT
	PK_User PRIMARY KEY CLUSTERED 
	(
	UserID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.[User] ADD CONSTRAINT
	FK_User_Language FOREIGN KEY
	(
	LanguageID
	) REFERENCES dbo.Language
	(
	LanguageID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.CrossLedgerclass ADD CONSTRAINT
	FK_CrossLedgerclass_User FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CrossLedgerclass ADD CONSTRAINT
	FK_CrossLedgerclass_User1 FOREIGN KEY
	(
	LastModifiedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.CrossLedgerclass SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Product ADD CONSTRAINT
	FK_Product_User FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Product ADD CONSTRAINT
	FK_Product_User1 FOREIGN KEY
	(
	LastModifiedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Product SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.UserState ADD CONSTRAINT
	FK_UserState_User FOREIGN KEY
	(
	UserID
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.UserState SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
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
ALTER TABLE dbo.RelatedProduct SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.[Content] ADD CONSTRAINT
	FK_Content_User FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[Content] ADD CONSTRAINT
	FK_Content_User1 FOREIGN KEY
	(
	LastModifiedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.[Content] SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.UserPortal ADD CONSTRAINT
	FK_UserPortals_User FOREIGN KEY
	(
	UserID
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.UserPortal SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductDescription ADD CONSTRAINT
	FK_ContentDescription_User FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductDescription ADD CONSTRAINT
	FK_ContentDescription_User1 FOREIGN KEY
	(
	LastModifiedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductDescription SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.ProductCompetitorPrice ADD CONSTRAINT
	FK_ProductCompetitorPrice_User FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductCompetitorPrice ADD CONSTRAINT
	FK_ProductCompetitorPrice_User1 FOREIGN KEY
	(
	LastModifiedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ProductCompetitorPrice SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.UserRole ADD CONSTRAINT
	FK_UserRoles_User FOREIGN KEY
	(
	UserID
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  CASCADE 
	 ON DELETE  CASCADE 
	
GO
ALTER TABLE dbo.UserRole SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
ALTER TABLE dbo.Event ADD CONSTRAINT
	FK_Event_User FOREIGN KEY
	(
	CreatedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Event ADD CONSTRAINT
	FK_Event_User1 FOREIGN KEY
	(
	LastModifiedBy
	) REFERENCES dbo.[User]
	(
	UserID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.Event SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
