DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingProduct' )
		
		BEGIN
		CREATE TABLE [dbo].[MasterGroupMappingProduct](
			[MasterGroupMappingID] [int] NOT NULL,
			[ProductID] [int] NOT NULL,
			[IsApproved] [bit] NOT NULL,
			[IsCustom] [bit] NOT NULL,
			[IsProductMapped] [bit] NOT NULL,
			[ConnectorPublicationRuleID] [int] NULL,
		CONSTRAINT [PK_MasterGroupMappingProduct] PRIMARY KEY CLUSTERED 
		(
			[MasterGroupMappingID] ASC,
			[ProductID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
		
			
		ALTER TABLE [dbo].[MasterGroupMappingProduct] ADD  CONSTRAINT [DF_MasterGroupMappingProduct_IsApproved]  DEFAULT ((0)) FOR [IsApproved]
				
		ALTER TABLE [dbo].[MasterGroupMappingProduct] ADD  CONSTRAINT [DF__MasterGro__IsCus__3BB20F8D]  DEFAULT ((0)) FOR [IsCustom]
				
		ALTER TABLE [dbo].[MasterGroupMappingProduct] ADD  CONSTRAINT [DF__MasterGro__IsPro__24B99B9C]  DEFAULT ((0)) FOR [IsProductMapped]
				
		ALTER TABLE [dbo].[MasterGroupMappingProduct]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingProduct_ConnectorPublicationRule] FOREIGN KEY([ConnectorPublicationRuleID])
		REFERENCES [dbo].[ConnectorPublicationRule] ([ConnectorPublicationRuleID])
		ON DELETE CASCADE
				
		ALTER TABLE [dbo].[MasterGroupMappingProduct] CHECK CONSTRAINT [FK_MasterGroupMappingProduct_ConnectorPublicationRule]
				
		ALTER TABLE [dbo].[MasterGroupMappingProduct]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingProduct_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
		REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
		ON DELETE CASCADE
				
		ALTER TABLE [dbo].[MasterGroupMappingProduct] CHECK CONSTRAINT [FK_MasterGroupMappingProduct_MasterGroupMapping]
				
		ALTER TABLE [dbo].[MasterGroupMappingProduct]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingProduct_Product] FOREIGN KEY([ProductID])
		REFERENCES [dbo].[Product] ([ProductID])
				
		ALTER TABLE [dbo].[MasterGroupMappingProduct] CHECK CONSTRAINT [FK_MasterGroupMappingProduct_Product]

    	ALTER TABLE [dbo].[ConnectorPublicationRule]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorPublicationRule_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
	ON DELETE SET NULL
		
	ALTER TABLE [dbo].[ConnectorPublicationRule] CHECK CONSTRAINT [FK_ConnectorPublicationRule_MasterGroupMapping]
			
      PRINT 'Added table MasterGroupMappingProduct'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingProduct already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingProduct'

  ROLLBACK TRAN
END

