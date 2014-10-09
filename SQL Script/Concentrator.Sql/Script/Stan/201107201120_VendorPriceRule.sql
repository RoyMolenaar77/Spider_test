
/****** Object:  Table [dbo].[VendorPriceRule]    Script Date: 07/20/2011 11:20:13 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[VendorPriceRule](
	[VendorPriceRuleID] [int] IDENTITY(1,1) NOT NULL,
	[VendorID] [int] NOT NULL,
	[ProductID] [int] NULL,
	[BrandID] [int] NULL,
	[ProductGroupID] [int] NULL,
	[VendorPriceCalculationID] [int] NULL,
	[Margin] [nvarchar](10) NOT NULL,
	[UnitPriceIncrease] [decimal](18, 4) NULL,
	[CostPriceIncrease] [decimal](18, 4) NULL,
	[MinimumQuantity] [int] NULL,
	[VendorPriceRuleIndex] [int] NOT NULL,
	[PriceRuleType] [int] NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModificationTime] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[VendorPriceRuleID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[VendorPriceRule]  WITH CHECK ADD  CONSTRAINT [FK_VendorPriceCalculation_VendorPriceRule] FOREIGN KEY([VendorPriceCalculationID])
REFERENCES [dbo].[VendorPriceCalculation] ([VendorPriceCalculationID])
GO

ALTER TABLE [dbo].[VendorPriceRule] CHECK CONSTRAINT [FK_VendorPriceCalculation_VendorPriceRule]
GO

ALTER TABLE [dbo].[VendorPriceRule]  WITH CHECK ADD  CONSTRAINT [FK_VendorPriceRule_Brand] FOREIGN KEY([BrandID])
REFERENCES [dbo].[Brand] ([BrandID])
GO

ALTER TABLE [dbo].[VendorPriceRule] CHECK CONSTRAINT [FK_VendorPriceRule_Brand]
GO

ALTER TABLE [dbo].[VendorPriceRule]  WITH CHECK ADD  CONSTRAINT [FK_VendorPriceRule_Product] FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO

ALTER TABLE [dbo].[VendorPriceRule] CHECK CONSTRAINT [FK_VendorPriceRule_Product]
GO

ALTER TABLE [dbo].[VendorPriceRule]  WITH CHECK ADD  CONSTRAINT [FK_VendorPriceRule_ProductGroup] FOREIGN KEY([ProductGroupID])
REFERENCES [dbo].[ProductGroup] ([ProductGroupID])
GO

ALTER TABLE [dbo].[VendorPriceRule] CHECK CONSTRAINT [FK_VendorPriceRule_ProductGroup]
GO

ALTER TABLE [dbo].[VendorPriceRule]  WITH CHECK ADD  CONSTRAINT [FK_VendorPriceRule_Vendor] FOREIGN KEY([VendorID])
REFERENCES [dbo].[Vendor] ([VendorID])
GO

ALTER TABLE [dbo].[VendorPriceRule] CHECK CONSTRAINT [FK_VendorPriceRule_Vendor]
GO

ALTER TABLE [dbo].[VendorPriceRule] ADD  DEFAULT (getdate()) FOR [CreationTime]
GO


