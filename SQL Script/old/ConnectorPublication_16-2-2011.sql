
/****** Object:  Table [dbo].[ConnectorPublication]    Script Date: 02/16/2011 10:11:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ConnectorPublication](
	[ConnectorPublicationID] [int] IDENTITY(1,1) NOT NULL,
	[ConnectorID] [int] NOT NULL,
	[VendorID] [int] NOT NULL,
	[ProductGroupID] [int] NULL,
	[BrandID] [int] NULL,
	[ProductID] [int] NULL,
	[Publish] [bit] NULL,
	[PublishOnlyStock] [bit] Null,
	[CreatedBy] [int] NOT NULL,
	[CreationTime] [datetime] NOT NULL,
	[LastModifiedBy] [int] NULL,
	[LastModificationTime] [datetime] NULL,
	[ProductContentIndex] [int] NOT NULL,
 CONSTRAINT [PK_ConnectorPublication] PRIMARY KEY CLUSTERED 
(
	[ConnectorPublicationID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[ConnectorPublication]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublication_Brand] FOREIGN KEY([BrandID])
REFERENCES [dbo].[Brand] ([BrandID])
GO

ALTER TABLE [dbo].[ConnectorPublication] CHECK CONSTRAINT [FK_ConnectorPublication_Brand]
GO

ALTER TABLE [dbo].[ConnectorPublication]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublication_Connector] FOREIGN KEY([ConnectorID])
REFERENCES [dbo].[Connector] ([ConnectorID])
GO

ALTER TABLE [dbo].[ConnectorPublication] CHECK CONSTRAINT [FK_ConnectorPublication_Connector]
GO

ALTER TABLE [dbo].[ConnectorPublication]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublication_Product] FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
GO

ALTER TABLE [dbo].[ConnectorPublication] CHECK CONSTRAINT [FK_ConnectorPublication_Product]
GO

ALTER TABLE [dbo].[ConnectorPublication]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublication_ProductGroup] FOREIGN KEY([ProductGroupID])
REFERENCES [dbo].[ProductGroup] ([ProductGroupID])
GO

ALTER TABLE [dbo].[ConnectorPublication] CHECK CONSTRAINT [FK_ConnectorPublication_ProductGroup]
GO

ALTER TABLE [dbo].[ConnectorPublication]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublication_Vendor] FOREIGN KEY([VendorID])
REFERENCES [dbo].[Vendor] ([VendorID])
GO

ALTER TABLE [dbo].[ConnectorPublication] CHECK CONSTRAINT [FK_ConnectorPublication_Vendor]
GO

ALTER TABLE [dbo].[ConnectorPublication] ADD  CONSTRAINT [DF_ConnectorPublication_CreationTime]  DEFAULT (getdate()) FOR [CreationTime]
GO


