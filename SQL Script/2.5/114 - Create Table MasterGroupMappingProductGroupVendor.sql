DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingProductGroupVendor' )
		
		BEGIN
		
	CREATE TABLE [dbo].[MasterGroupMappingProductGroupVendor](
		[MasterGroupMappingID] [int] NOT NULL,
		[ProductGroupVendorID] [int] NOT NULL,
	CONSTRAINT [PK_MasterGroupMappingProductGroupVendor] PRIMARY KEY CLUSTERED 
	(
		[MasterGroupMappingID] ASC,
		[ProductGroupVendorID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	
	ALTER TABLE [dbo].[MasterGroupMappingProductGroupVendor]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingProductGroupVendor_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
	ON DELETE CASCADE
		
	ALTER TABLE [dbo].[MasterGroupMappingProductGroupVendor] CHECK CONSTRAINT [FK_MasterGroupMappingProductGroupVendor_MasterGroupMapping]
		
	ALTER TABLE [dbo].[MasterGroupMappingProductGroupVendor]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingProductGroupVendor_ProductGroupVendor] FOREIGN KEY([ProductGroupVendorID])
	REFERENCES [dbo].[ProductGroupVendor] ([ProductGroupVendorID])
		
	ALTER TABLE [dbo].[MasterGroupMappingProductGroupVendor] CHECK CONSTRAINT [FK_MasterGroupMappingProductGroupVendor_ProductGroupVendor]
		
      PRINT 'Added table MasterGroupMappingProductGroupVendor'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingProductGroupVendor already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingProductGroupVendor'

  ROLLBACK TRAN
END