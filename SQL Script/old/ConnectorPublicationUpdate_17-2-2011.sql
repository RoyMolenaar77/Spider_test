--drop table connectorpublication
CREATE TABLE dbo.ConnectorPublication
	(
	ConnectorPublicationID int NOT NULL IDENTITY (1, 1) primary key,
	ConnectorID int NOT NULL,
	VendorID int NOT NULL,
	ProductGroupID int NULL,
	BrandID int NULL,
	ProductID int NULL,
	Publish bit NULL,
	PublishOnlyStock bit NULL,
	CreatedBy int NOT NULL,
	CreationTime datetime NOT NULL,
	LastModifiedBy int NULL,
	LastModificationTime datetime NULL,
	ProductContentIndex int NOT NULL,
	StatusID int NOT NULL
	)  ON [PRIMARY]
GO

ALTER TABLE dbo.ConnectorPublication ADD CONSTRAINT
	FK_ConnectorPublication_Brand FOREIGN KEY
	(
	BrandID
	) REFERENCES dbo.Brand
	(
	BrandID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
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
ALTER TABLE dbo.ConnectorPublication ADD CONSTRAINT
	FK_ConnectorPublication_Product FOREIGN KEY
	(
	ProductID
	) REFERENCES dbo.Product
	(
	ProductID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorPublication ADD CONSTRAINT
	FK_ConnectorPublication_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorPublication ADD CONSTRAINT
	FK_ConnectorPublication_Vendor FOREIGN KEY
	(
	VendorID
	) REFERENCES dbo.Vendor
	(
	VendorID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO
ALTER TABLE dbo.ConnectorPublication ADD CONSTRAINT
	FK_ConnectorPublication_AssortmentStatus FOREIGN KEY
	(
	StatusID
	) REFERENCES dbo.AssortmentStatus
	(
	StatusID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 
	
GO


/****** Object:  Table [dbo].[ContentPriceCalculation]    Script Date: 02/17/2011 08:41:46 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ContentPriceCalculation](
	[ContentPriceCalculationID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[RoundOnBeforeDecimal] [int] NOT NULL,
	[RoundAfterDecimal] [int] NOT NULL,
	[DigitsBeforeDecimal] [int] NOT NULL,
	[DigitsAfterDecimal] [int] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModificationTime] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
 CONSTRAINT [PK_ContentPriceCalculation] PRIMARY KEY CLUSTERED 
(
	[ContentPriceCalculationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


alter table contentprice add [ContentPriceCalculationID] int null

ALTER TABLE contentprice
ADD CONSTRAINT FK_ContentPriceCalculation_ContentPrice
FOREIGN KEY ([ContentPriceCalculationID])
REFERENCES [ContentPriceCalculation]([ContentPriceCalculationID])