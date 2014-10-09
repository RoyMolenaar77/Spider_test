DECLARE @intErrorCode INT


  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingProductVendor' )
		
		BEGIN
		
CREATE TABLE [dbo].[MasterGroupMappingProductVendor](
	[VendorID] [int] NOT NULL,
	[ProductID] [int] NOT NULL, 
	[MasterGroupMappingID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[VendorID] ASC,
	[ProductID] ASC,
	[MasterGroupMappingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[MasterGroupMappingProductVendor]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingProductVendor_MGM] FOREIGN KEY([MasterGroupMappingID])
REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])


ALTER TABLE [dbo].[MasterGroupMappingProductVendor] CHECK CONSTRAINT [FK_MasterGroupMappingProductVendor_MGM]


ALTER TABLE [dbo].[MasterGroupMappingProductVendor]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingProductVendor_Product] FOREIGN KEY([ProductID])
REFERENCES [dbo].[Product] ([ProductID])


ALTER TABLE [dbo].[MasterGroupMappingProductVendor] CHECK CONSTRAINT [FK_MasterGroupMappingProductVendor_Product]


ALTER TABLE [dbo].[MasterGroupMappingProductVendor]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingProductVendor_Vendor] FOREIGN KEY([VendorID])
REFERENCES [dbo].[Vendor] ([VendorID])

ALTER TABLE [dbo].[MasterGroupMappingProductVendor] CHECK CONSTRAINT [FK_MasterGroupMappingProductVendor_Vendor]


end