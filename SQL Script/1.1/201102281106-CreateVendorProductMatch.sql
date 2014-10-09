
CREATE TABLE [dbo].[VendorProductMatch](
	[VendorProductMatchID] [int] IDENTITY(1,1) NOT NULL,
	[VendorID] [int] NOT NULL,
	[ProductID] [int] NOT NULL,
	[VendorProductID] [int] NULL,
	[VendorItemNumber] [nvarchar](50) NULL,
	[MatchPercentage] [int] NULL,
 CONSTRAINT [PK_VendorProductMatch] PRIMARY KEY CLUSTERED 
(
	[VendorProductMatchID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [dbo].[VendorProductMatch]  WITH CHECK ADD  CONSTRAINT [FK_VendorProductMatch_Product] FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[VendorProductMatch] CHECK CONSTRAINT [FK_VendorProductMatch_Product]
GO

ALTER TABLE [dbo].[VendorProductMatch]  WITH CHECK ADD  CONSTRAINT [FK_VendorProductMatch_Vendor] FOREIGN KEY([VendorID])
REFERENCES [dbo].[Vendor] ([VendorID])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[VendorProductMatch] CHECK CONSTRAINT [FK_VendorProductMatch_Vendor]
GO


