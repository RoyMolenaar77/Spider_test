/*
   woensdag 22 juni 201115:17:55
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
ALTER TABLE dbo.VendorProductStatus
	DROP CONSTRAINT FK_VendorProductStatus_AssortmentStatus
GO
ALTER TABLE dbo.AssortmentStatus SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.Tmp_VendorProductStatus
	(
	VendorProductStatusID int NOT NULL IDENTITY (1, 1),
	VendorID int NOT NULL,
	VendorStatus nvarchar(50) NOT NULL,
	ConcentratorStatusID int NOT NULL,
	VendorStatusID int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_VendorProductStatus SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_VendorProductStatus OFF
GO
IF EXISTS(SELECT * FROM dbo.VendorProductStatus)
	 EXEC('INSERT INTO dbo.Tmp_VendorProductStatus (VendorID, VendorStatus, ConcentratorStatusID, VendorStatusID)
		SELECT VendorID, VendorStatus, ConcentratorStatusID, VendorStatusID FROM dbo.VendorProductStatus WITH (HOLDLOCK TABLOCKX)')
GO
DROP TABLE dbo.VendorProductStatus
GO
EXECUTE sp_rename N'dbo.Tmp_VendorProductStatus', N'VendorProductStatus', 'OBJECT' 
GO
ALTER TABLE dbo.VendorProductStatus ADD CONSTRAINT
	PK_VendorProductStatus_1 PRIMARY KEY CLUSTERED 
	(
	VendorProductStatusID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.VendorProductStatus ADD CONSTRAINT
	FK_VendorProductStatus_AssortmentStatus FOREIGN KEY
	(
	ConcentratorStatusID
	) REFERENCES dbo.AssortmentStatus
	(
	StatusID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
-- =============================================
-- Author:		Stan Todorov
-- Create date: 24/09/2010
-- Description:	Updates the statuses in the vendorstock table to the newly mapped status
-- =============================================
CREATE TRIGGER UpdateStockStatus 
   ON  dbo.VendorProductStatus
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    
    UPDATE V
    set
    ConcentratorStatusID = i.ConcentratorStatusID
    --SELECT V.StockStatus, d.ConcentratorStatusID AS OldStatusID, V.VendorID, d.VendorStatus AS OldVendorStatus, i.ConcentratorStatusID, i.VendorStatus
    FROM dbo.VendorStock V
    LEFT JOIN deleted d ON 
    d.VendorID = V.VendorID AND 
    V.ConcentratorStatusID = d.ConcentratorStatusID AND 
    d.VendorStatus = v.vendorstatus
    LEFT JOIN INSERTED i ON i.VendorID = v.VendorID AND i.VendorStatus = v.vendorstatus
    WHERE v.VendorID = d.vendorid AND v.ConcentratorStatusID = d.ConcentratorStatusID

END
GO
COMMIT
