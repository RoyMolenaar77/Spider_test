
/****** Object:  Table [dbo].[MissingContent]    Script Date: 06/16/2011 10:05:51 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[MissingContent](
	[ConcentratorProductID] [int] NOT NULL,
	[ConnectorID] [int] NOT NULL,
	[isActive] [bit] NOT NULL,
	[VendorItemNumber] [nvarchar](50) NOT NULL,
	[CustomItemNumber] [nvarchar](50) NULL,
	[BrandName] [nvarchar](50) NOT NULL,
	[ShortDescription] [nvarchar](255) NULL,
	[ProductGroupID] [int] NOT NULL,
	[BrandID] [int] NOT NULL,
	[Image] [bit] NOT NULL,
	[YouTube] [bit] NOT NULL,
	[Specifications] [bit] NOT NULL,
	[ContentVendor] [nvarchar](50) NULL,
	[ContentVendorID] [int] NULL,
	[Barcode] [nvarchar](50) NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModificationTime] [date] NULL,
 CONSTRAINT [PK_MissingContent] PRIMARY KEY CLUSTERED 
(
	[ConcentratorProductID] ASC,
	[ConnectorID] ASC,
	[ProductGroupID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[MissingContent]  WITH CHECK ADD  CONSTRAINT [FK_MissingContent_Brand] FOREIGN KEY([BrandID])
REFERENCES [dbo].[Brand] ([BrandID])
GO

ALTER TABLE [dbo].[MissingContent] CHECK CONSTRAINT [FK_MissingContent_Brand]
GO

ALTER TABLE [dbo].[MissingContent]  WITH CHECK ADD  CONSTRAINT [FK_MissingContent_Connector] FOREIGN KEY([ConnectorID])
REFERENCES [dbo].[Connector] ([ConnectorID])
GO

ALTER TABLE [dbo].[MissingContent] CHECK CONSTRAINT [FK_MissingContent_Connector]
GO

ALTER TABLE [dbo].[MissingContent]  WITH CHECK ADD  CONSTRAINT [FK_MissingContent_Product] FOREIGN KEY([ConcentratorProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO

ALTER TABLE [dbo].[MissingContent] CHECK CONSTRAINT [FK_MissingContent_Product]
GO

ALTER TABLE [dbo].[MissingContent]  WITH CHECK ADD  CONSTRAINT [FK_MissingContent_ProductGroup] FOREIGN KEY([ProductGroupID])
REFERENCES [dbo].[ProductGroup] ([ProductGroupID])
GO

ALTER TABLE [dbo].[MissingContent] CHECK CONSTRAINT [FK_MissingContent_ProductGroup]
GO

ALTER TABLE [dbo].[MissingContent]  WITH CHECK ADD  CONSTRAINT [FK_MissingContent_Vendor] FOREIGN KEY([ContentVendorID])
REFERENCES [dbo].[Vendor] ([VendorID])
GO

ALTER TABLE [dbo].[MissingContent] CHECK CONSTRAINT [FK_MissingContent_Vendor]
GO


/****** Object:  StoredProcedure [dbo].[sp_GenerateMissingContent]    Script Date: 06/16/2011 10:06:36 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Stan Todorov
-- Create date: 15/06/2011
-- Description:	Prepares the missing content and inserts it into a table
-- =============================================
CREATE PROCEDURE [dbo].[sp_GenerateMissingContent] AS
BEGIN
	
	delete from MissingContent
	
	insert into MissingContent (ConcentratorProductID, ConnectorID, isActive, VendorItemNumber, CustomItemNumber, BrandName, ShortDescription, ProductGroupID, BrandID, [Image], YouTube, Specifications, ContentVendor, ContentVendorID, Barcode, CreationTime, LastModificationTime)
	select ConcentratorProductID,
		   ConnectorID, 
		   Active, 
		   VendorItemNumber, 
		   CustomItemNumber, 
		   brandName, 
		   ShortDescription, 
		   isnull(ProductGroupID, -1), 
		   BrandID, [Image], youtube, Specifications, ContentVendor, ContentVendorID, barcode, CreationTime, LastModificationTime
	from MissingContentView
	
END


GO




