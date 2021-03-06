/*
   Monday, February 28, 20113:22:05 PM
   User: Concentrator_usr
   Server: 192.168.94.186
   Database: Concentrator_staging
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
CREATE TABLE dbo.ContentLedger
	(
	LedgerID int NOT NULL IDENTITY (1, 1),
	ProductID int NOT NULL,
	LedgerDate datetime NULL,
	UnitPrice decimal(18, 4) NULL,
	CostPrice decimal(18, 4) NULL,
	TaxRate decimal(4, 2) NULL,
	ConcentratorStatusID int NULL,
	MinimumQuantity int NULL,
	VendorAssortmentID int NULL,
	Remark nchar(10) NULL,
	LedgerObject nvarchar(50) NULL,
	CreatedBy int NOT NULL,
	CreationtTime datetime NOT NULL,
	LastModifiedBy int NULL,
	LastModificationTime datetime NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.ContentLedger ADD CONSTRAINT
	DF_ContentLedger_CreationtTime DEFAULT getdate() FOR CreationtTime
GO
ALTER TABLE dbo.ContentLedger ADD CONSTRAINT
	PK_ContentLedger PRIMARY KEY CLUSTERED 
	(
	LedgerID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.ContentLedger SET (LOCK_ESCALATION = TABLE)
GO
ALTER TABLE ContentLedger
ADD CONSTRAINT FK_ContentLedger_Product
FOREIGN KEY (ProductID)
REFERENCES Product(ProductID)
GO
ALTER TABLE ContentLedger
ADD CONSTRAINT FK_ContentLedger_VendorAssortment
FOREIGN KEY (VendorAssortmentID)
REFERENCES VendorAssortment(VendorAssortmentID)

COMMIT
