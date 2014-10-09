DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingRelatedAttribute' )
		
		BEGIN
		
	CREATE TABLE [dbo].[MasterGroupMappingRelatedAttribute](
		[RelatedAttributeID] [int] IDENTITY(1,1) NOT NULL,
		[MasterGroupMappingID] [int] NOT NULL,
		[ParentID] [int] NULL,
		[AttributeID] [int] NOT NULL,
		[AttributeValue] [varchar](50) NULL,
	CONSTRAINT [PK_MasterGroupMappingRelatedAttribute] PRIMARY KEY CLUSTERED 
	(
		[RelatedAttributeID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	SET ANSI_PADDING OFF
	
	ALTER TABLE [dbo].[MasterGroupMappingRelatedAttribute]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingRelatedAttribute_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[MasterGroupMappingRelatedAttribute] CHECK CONSTRAINT [FK_MasterGroupMappingRelatedAttribute_MasterGroupMapping]
	
	ALTER TABLE [dbo].[MasterGroupMappingRelatedAttribute]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingRelatedAttribute_MasterGroupMappingRelatedAttribute] FOREIGN KEY([ParentID])
	REFERENCES [dbo].[MasterGroupMappingRelatedAttribute] ([RelatedAttributeID])
	
	ALTER TABLE [dbo].[MasterGroupMappingRelatedAttribute] CHECK CONSTRAINT [FK_MasterGroupMappingRelatedAttribute_MasterGroupMappingRelatedAttribute]
	
	ALTER TABLE [dbo].[MasterGroupMappingRelatedAttribute]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingRelatedAttribute_ProductAttributeMetaData] FOREIGN KEY([AttributeID])
	REFERENCES [dbo].[ProductAttributeMetaData] ([AttributeID])
	
	ALTER TABLE [dbo].[MasterGroupMappingRelatedAttribute] CHECK CONSTRAINT [FK_MasterGroupMappingRelatedAttribute_ProductAttributeMetaData]

      PRINT 'Added table MasterGroupMappingRelatedAttribute'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingRelatedAttribute already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingRelatedAttribute'

  ROLLBACK TRAN
END

