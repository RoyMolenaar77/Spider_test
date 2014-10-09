/****** Object:  Table [dbo].[VendorAccruel]    Script Date: 02/07/2011 08:00:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[VendorAccruel](
	[VendorAssortmentID] [int] NOT NULL,
	[AccruelCode] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
	[UnitPrice] [decimal](18, 4) NOT NULL,
	[MinimumQuantity] [int] NOT NULL,
 CONSTRAINT [PK_VendorAccruel] PRIMARY KEY CLUSTERED 
(
	[VendorAssortmentID] ASC,
	[AccruelCode] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[VendorAccruel]  WITH CHECK ADD  CONSTRAINT [FK_VendorAccruel_VendorAssortment] FOREIGN KEY([VendorAssortmentID])
REFERENCES [dbo].[VendorAssortment] ([VendorAssortmentID])
GO

ALTER TABLE [dbo].[VendorAccruel] CHECK CONSTRAINT [FK_VendorAccruel_VendorAssortment]
GO


/****** Object:  Table [dbo].[VendorFreeGoods]    Script Date: 02/07/2011 08:00:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[VendorFreeGoods](
	[VendorAssortmentID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[MinimumQuantity] [int] NOT NULL,
	[OverOrderedQuantity] [int] NOT NULL,
	[FreeGoodQuantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 4) NOT NULL,
	[Description] [nvarchar](255) NOT NULL,
 CONSTRAINT [PK_VendorFreeGoods_1] PRIMARY KEY CLUSTERED 
(
	[VendorAssortmentID] ASC,
	[ProductID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[VendorFreeGoods]  WITH CHECK ADD  CONSTRAINT [FK_VendorFreeGoods_Product] FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO

ALTER TABLE [dbo].[VendorFreeGoods] CHECK CONSTRAINT [FK_VendorFreeGoods_Product]
GO

ALTER TABLE [dbo].[VendorFreeGoods]  WITH CHECK ADD  CONSTRAINT [FK_VendorFreeGoods_VendorAssortment] FOREIGN KEY([VendorAssortmentID])
REFERENCES [dbo].[VendorAssortment] ([VendorAssortmentID])
GO

ALTER TABLE [dbo].[VendorFreeGoods] CHECK CONSTRAINT [FK_VendorFreeGoods_VendorAssortment]
GO

alter table productmedia add Description nvarchar(150) null
alter table relatedProduct add RelationType nvarchar(50) null
