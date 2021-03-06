/*
   dinsdag 1 maart 201115:22:42
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
CREATE TABLE dbo.Tmp_EdiOrder
	(
	EdiOrderID int NOT NULL IDENTITY (1, 1),
	[Document] nvarchar(MAX) NULL,
	ConnectorID int NOT NULL,
	IsDispatched bit NOT NULL,
	DispatchToVendorDate datetime NULL,
	ReceivedDate datetime NOT NULL,
	isDropShipment bit NULL,
	Remarks nvarchar(MAX) NULL,
	ShipToCustomerID int NULL,
	SoldToCustomerID int NULL,
	CustomerOrderReference nvarchar(MAX) NULL,
	EdiVersion nvarchar(50) NULL,
	BSKIdentifier int NULL,
	WebSiteOrderNumber nvarchar(100) NULL,
	PaymentTermsCode nvarchar(50) NULL,
	PaymentInstrument nvarchar(50) NULL,
	BackOrdersAllowed bit NULL,
	RouteCode nvarchar(50) NULL,
	HoldCode nvarchar(50) NULL,
	HoldOrder bit NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_EdiOrder SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_EdiOrder ON
GO
IF EXISTS(SELECT * FROM dbo.EdiOrder)
	 EXEC('INSERT INTO dbo.Tmp_EdiOrder (EdiOrderID, [Document], ConnectorID, IsDispatched, DispatchToVendorDate, ReceivedDate, isDropShipment, Remarks, ShipToCustomerID, SoldToCustomerID, CustomerOrderReference, EdiVersion, BSKIdentifier, WebSiteOrderNumber, PaymentTermsCode, PaymentInstrument, BackOrdersAllowed, RouteCode, HoldCode, HoldOrder)
		SELECT EdiOrderID, [Document], ConnectorID, IsDispatched, DispatchToVendorDate, ReceivedDate, isDropShipment, Remarks, ShipToCustomerID, SoldToCustomerID, CustomerOrderReference, EdiVersion, CONVERT(int, BSKIdentifier), WebSiteOrderNumber, PaymentTermsCode, PaymentInstrument, BackOrdersAllowed, RouteCode, HoldCode, HoldOrder FROM dbo.EdiOrder WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_EdiOrder OFF
GO
DROP TABLE dbo.EdiOrder
GO
EXECUTE sp_rename N'dbo.Tmp_EdiOrder', N'EdiOrder', 'OBJECT' 
GO
ALTER TABLE dbo.EdiOrder ADD CONSTRAINT
	PK_EdiOrder PRIMARY KEY CLUSTERED 
	(
	EdiOrderID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
COMMIT
