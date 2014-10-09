ALTER TABLE VendorAssortment add IsActive bit not null default 0
ALTER TABLE VendorAssortment add ZoneReferenceID nvarchar(255) NULL
ALTER TABLE VendorAssortment add ShipmentRateTableReferenceID nvarchar(255) NULL


IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ProductMatch_Product]') AND parent_object_id = OBJECT_ID(N'[dbo].[ProductMatch]'))
ALTER TABLE [dbo].[ProductMatch] DROP CONSTRAINT [FK_ProductMatch_Product]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ProductMatch_Product1]') AND parent_object_id = OBJECT_ID(N'[dbo].[ProductMatch]'))
ALTER TABLE [dbo].[ProductMatch] DROP CONSTRAINT [FK_ProductMatch_Product1]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_ProductMatch_Vendor]') AND parent_object_id = OBJECT_ID(N'[dbo].[ProductMatch]'))
ALTER TABLE [dbo].[ProductMatch] DROP CONSTRAINT [FK_ProductMatch_Vendor]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_ProductMatch_isChecked]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[ProductMatch] DROP CONSTRAINT [DF_ProductMatch_isChecked]
END

GO

/****** Object:  Table [dbo].[ProductMatch]    Script Date: 01/24/2011 15:30:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ProductMatch]') AND type in (N'U'))
DROP TABLE [dbo].[ProductMatch]
GO

/****** Object:  Table [dbo].[ProductMatch]    Script Date: 01/24/2011 15:30:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProductMatch](
	[ProductMatchID] [int] IDENTITY(1,1) NOT NULL,
	[ProductID] [int] NOT NULL,
	[CorrespondingProductID] [int] NULL,
	[isMatched] [bit] NOT NULL,
	[VendorItemNumber] [nvarchar](50) NULL,
	[VendorID] [int] NOT NULL,
 CONSTRAINT [PK_ProductMatch] PRIMARY KEY CLUSTERED 
(
	[ProductMatchID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ProductMatch]  WITH CHECK ADD  CONSTRAINT [FK_ProductMatch_Product] FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO

ALTER TABLE [dbo].[ProductMatch] CHECK CONSTRAINT [FK_ProductMatch_Product]
GO

ALTER TABLE [dbo].[ProductMatch]  WITH CHECK ADD  CONSTRAINT [FK_ProductMatch_Product1] FOREIGN KEY([CorrespondingProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO

ALTER TABLE [dbo].[ProductMatch] CHECK CONSTRAINT [FK_ProductMatch_Product1]
GO

ALTER TABLE [dbo].[ProductMatch]  WITH CHECK ADD  CONSTRAINT [FK_ProductMatch_Vendor] FOREIGN KEY([VendorID])
REFERENCES [dbo].[Vendor] ([VendorID])
GO

ALTER TABLE [dbo].[ProductMatch] CHECK CONSTRAINT [FK_ProductMatch_Vendor]
GO

ALTER TABLE [dbo].[ProductMatch] ADD  CONSTRAINT [DF_ProductMatch_isChecked]  DEFAULT ((0)) FOR [isMatched]
GO


/****** Object:  Table [dbo].[ConnectorRelation]    Script Date: 12/27/2010 17:08:12 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ConnectorRelation](
      [CustomerID] [int] NOT NULL,
      [OrderConfirmation] [bit] NULL,
      [ShipmentConfirmation] [bit] NULL,
      [InvoiceConfirmation] [bit] NULL,
      [OrderConfirmationEmail] [nvarchar](100) NULL,
      [ShipmentConfirmationEmail] [nvarchar](100) NULL,
      [InvoiceConfirmationEmail] [nvarchar](100) NULL,
      [OutboundTo] [nvarchar](100) NULL,
      [OutboundPassword] [nvarchar](100) NULL,
      [OutboundUsername] [nvarchar](100) NULL,
      [ConnectorType] [nvarchar](15) NULL,
      [OutboundMessageType] [nvarchar](10) NULL,
      [AuthorisationAddresses] [nvarchar](max) NULL,
      [AccountPrivilages] [nvarchar](10) NULL,
      [UseFtp] [bit] NULL,
      [ProviderType] [nvarchar](10) NULL,
      [FtpType] [nvarchar](50) NULL,
      [FtpFrequency] [int] NULL,
      [FtpAddress] [nvarchar](100) NULL,
      [FtpPass] [nvarchar](50) NULL,
      [FtpPort] [int] NULL,
CONSTRAINT [PK_ConnectorRelation] PRIMARY KEY CLUSTERED 
(
      [CustomerID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO


/****** Object:  UserDefinedFunction [dbo].[GetProductGroupHierarchy]    Script Date: 01/27/2011 07:50:56 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetProductGroupHierarchy] 
(
	@ProductID int,
	@ConnectorID int
)

RETURNS @table table (
 productGroupMappingID int,
 ProductGroupID int,
 Name nvarchar(255),
 Score int,
 Depth int,
 LanguageID int,
 [Set] int,
 CustomProductGroupLabel nvarchar(255)
 )

as  
BEGIN
declare @lineage hierarchyid 
declare @counter int
set @counter = 1

DECLARE pg_cursor CURSOR FOR
Select distinct CAST(lineage as hierarchyid), cpg.productid, cpg.connectorid 
	from ProductGroupMapping pgm2
	inner join ContentProductGroup cpg on pgm2.ProductGroupMappingID = cpg.ProductGroupMappingID
	where cpg.ProductID = @ProductID and cpg.ConnectorID = @ConnectorID
OPEN pg_cursor;
FETCH NEXT FROM pg_cursor
INTO @lineage,@productid,@connectorID;

WHILE @@FETCH_STATUS = 0
   BEGIN

   insert into @table
   SELECT
    pgm.ProductGroupMappingID,
    pgl.productgroupid,
    pgl.Name,
	isnull(pgm.Score,pg.score) as Score,
    Depth,
    pgl.LanguageID,
    @counter as [Set],
    CustomProductGroupLabel
FROM ProductGroupMapping pgm
inner join ProductGroup pg on pgm.ProductGroupID = pg.ProductGroupID
inner join ProductGroupLanguage pgl on pgm.ProductGroupID = pgl.ProductGroupID-- and pgl.LanguageID = 2
where @lineage.IsDescendantOf(Lineage) = 1

   set @counter = @counter + 1;

FETCH NEXT FROM pg_cursor
INTO @lineage,@productid,@connectorID;

   END;

CLOSE pg_cursor;
DEALLOCATE pg_cursor;

return
END

GO

/****** Object:  Table [dbo].[PaymentProvider]    Script Date: 01/27/2011 07:51:49 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PaymentProvider](
	[PaymentProviderID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ProviderType] [nvarchar](255) NOT NULL,
	[IsActive] [bit] NOT NULL,
 CONSTRAINT [PK_PaymentProvider] PRIMARY KEY CLUSTERED 
(
	[PaymentProviderID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[PaymentProvider] ADD  CONSTRAINT [DF_PaymentProvider_IsActive]  DEFAULT ((0)) FOR [IsActive]
GO



/****** Object:  Table [dbo].[ConnectorPaymentProvider]    Script Date: 01/27/2011 07:51:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ConnectorPaymentProvider](
	[ConnectorID] [int] NOT NULL,
	[PaymentProviderID] [int] NOT NULL,
	[PaymentTermsCode] [nvarchar](50) NOT NULL,
	[PaymentInstrument] [nvarchar](50) NULL,
	[Profile] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ConnectorPaymentProvider] PRIMARY KEY CLUSTERED 
(
	[ConnectorID] ASC,
	[PaymentProviderID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ConnectorPaymentProvider]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPaymentProvider_Connector] FOREIGN KEY([ConnectorID])
REFERENCES [dbo].[Connector] ([ConnectorID])
GO

ALTER TABLE [dbo].[ConnectorPaymentProvider] CHECK CONSTRAINT [FK_ConnectorPaymentProvider_Connector]
GO

ALTER TABLE [dbo].[ConnectorPaymentProvider]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPaymentProvider_PaymentProvider] FOREIGN KEY([PaymentProviderID])
REFERENCES [dbo].[PaymentProvider] ([PaymentProviderID])
GO

ALTER TABLE [dbo].[ConnectorPaymentProvider] CHECK CONSTRAINT [FK_ConnectorPaymentProvider_PaymentProvider]
GO


IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PortalPortlets_Portal]') AND parent_object_id = OBJECT_ID(N'[dbo].[PortalPortlet]'))
ALTER TABLE [dbo].[PortalPortlet] DROP CONSTRAINT [FK_PortalPortlets_Portal]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_PortalPortlets_Portlet]') AND parent_object_id = OBJECT_ID(N'[dbo].[PortalPortlet]'))
ALTER TABLE [dbo].[PortalPortlet] DROP CONSTRAINT [FK_PortalPortlets_Portlet]
GO

/****** Object:  Table [dbo].[PortalPortlet]    Script Date: 01/27/2011 10:13:44 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PortalPortlet]') AND type in (N'U'))
DROP TABLE [dbo].[PortalPortlet]
GO



alter table connectorrelation add XtractType nvarchar(50) NULL

