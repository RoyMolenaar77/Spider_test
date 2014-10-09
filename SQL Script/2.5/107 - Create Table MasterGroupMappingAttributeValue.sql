DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingAttributeValue' )
		
		BEGIN
		
		CREATE TABLE [dbo].[MasterGroupMappingAttributeValue](
			[MasterGroupMappingID] [int] NOT NULL,
			[AttributeID] [int] NOT NULL,
		CONSTRAINT [PK_MasterGroupMappingAttributeValue] PRIMARY KEY CLUSTERED 
		(
			[MasterGroupMappingID] ASC,
			[AttributeID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
		
				
		ALTER TABLE [dbo].[MasterGroupMappingAttributeValue]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingAttributeValue_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
		REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
		ON DELETE CASCADE
				
		ALTER TABLE [dbo].[MasterGroupMappingAttributeValue] CHECK CONSTRAINT [FK_MasterGroupMappingAttributeValue_MasterGroupMapping]
				
		ALTER TABLE [dbo].[MasterGroupMappingAttributeValue]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingAttributeValue_ProductAttributeMetaData] FOREIGN KEY([AttributeID])
		REFERENCES [dbo].[ProductAttributeMetaData] ([AttributeID])
				
		ALTER TABLE [dbo].[MasterGroupMappingAttributeValue] CHECK CONSTRAINT [FK_MasterGroupMappingAttributeValue_ProductAttributeMetaData]
			
      PRINT 'Added table MasterGroupMappingAttributeValue'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingAttributeValue already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingAttributeValue'

  ROLLBACK TRAN
END