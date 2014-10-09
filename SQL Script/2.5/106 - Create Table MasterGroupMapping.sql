DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMapping' )
		
		BEGIN
		
		CREATE TABLE [dbo].[MasterGroupMapping](
			[MasterGroupMappingID] [int] IDENTITY(1,1) NOT NULL,
			[ParentMasterGroupMappingID] [int] NULL,
			[ProductGroupID] [int] NOT NULL,
			[Score] [int] NULL,
			[ConnectorID] [int] NULL,
			[SourceMasterGroupMappingID] [int] NULL,
			[FlattenHierarchy] [bit] NOT NULL,
			[FilterByParentGroup] [bit] NOT NULL,
			[ExportID] [int] NULL,
			[SourceProductGroupMappingID] [int] NULL,
			[ImagePath] [nvarchar](255) NULL,
		CONSTRAINT [PK__MasterGr__B19BCE820EFCFAAE] PRIMARY KEY CLUSTERED 
		(
			[MasterGroupMappingID] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
		) ON [PRIMARY]
				
		ALTER TABLE [dbo].[MasterGroupMapping] ADD  CONSTRAINT [DF__MasterGro__Flatt__5A80AF70]  DEFAULT ((0)) FOR [FlattenHierarchy]
				
		ALTER TABLE [dbo].[MasterGroupMapping] ADD  CONSTRAINT [DF__MasterGro__Filte__5B74D3A9]  DEFAULT ((0)) FOR [FilterByParentGroup]
				
		ALTER TABLE [dbo].[MasterGroupMapping]  WITH CHECK ADD  CONSTRAINT [FK__MasterGro__Paren__10E54320] FOREIGN KEY([ParentMasterGroupMappingID])
		REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
				
		ALTER TABLE [dbo].[MasterGroupMapping] CHECK CONSTRAINT [FK__MasterGro__Paren__10E54320]
				
		ALTER TABLE [dbo].[MasterGroupMapping]  WITH CHECK ADD  CONSTRAINT [FK_ConnectorMapping_ParentConnectorMapping] FOREIGN KEY([SourceProductGroupMappingID])
		REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
				
		ALTER TABLE [dbo].[MasterGroupMapping] CHECK CONSTRAINT [FK_ConnectorMapping_ParentConnectorMapping]
				
		ALTER TABLE [dbo].[MasterGroupMapping]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMapping_Connector] FOREIGN KEY([ConnectorID])
		REFERENCES [dbo].[Connector] ([ConnectorID])
				
		ALTER TABLE [dbo].[MasterGroupMapping] CHECK CONSTRAINT [FK_MasterGroupMapping_Connector]
				
		ALTER TABLE [dbo].[MasterGroupMapping]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMapping_ConnectorMapping] FOREIGN KEY([SourceMasterGroupMappingID])
		REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
				
		ALTER TABLE [dbo].[MasterGroupMapping] CHECK CONSTRAINT [FK_MasterGroupMapping_ConnectorMapping]
			
      PRINT 'Added table MasterGroupMapping'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMapping already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMapping'

  ROLLBACK TRAN
END