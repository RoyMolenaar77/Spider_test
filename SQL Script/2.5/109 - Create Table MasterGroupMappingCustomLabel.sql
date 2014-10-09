DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingCustomLabel' )
		
		BEGIN
		
	CREATE TABLE [dbo].[MasterGroupMappingCustomLabel](
		[MasterGroupMappingCustomLabelID] [int] IDENTITY(1,1) NOT NULL,
		[MasterGroupMappingID] [int] NOT NULL,
		[LanguageID] [int] NOT NULL,
		[ConnectorID] [int] NULL,
		[CustomLabel] [nvarchar](250) NOT NULL,
	CONSTRAINT [PK_MasterGroupMappingCustomLabel] PRIMARY KEY CLUSTERED 
	(
		[MasterGroupMappingCustomLabelID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	ALTER TABLE [dbo].[MasterGroupMappingCustomLabel]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingCustomLabel_Connector] FOREIGN KEY([ConnectorID])
	REFERENCES [dbo].[Connector] ([ConnectorID])
	
	ALTER TABLE [dbo].[MasterGroupMappingCustomLabel] CHECK CONSTRAINT [FK_MasterGroupMappingCustomLabel_Connector]
	
	ALTER TABLE [dbo].[MasterGroupMappingCustomLabel]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingCustomLabel_Language] FOREIGN KEY([LanguageID])
	REFERENCES [dbo].[Language] ([LanguageID])
	
	ALTER TABLE [dbo].[MasterGroupMappingCustomLabel] CHECK CONSTRAINT [FK_MasterGroupMappingCustomLabel_Language]
	
	ALTER TABLE [dbo].[MasterGroupMappingCustomLabel]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingCustomLabel_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
	
	ALTER TABLE [dbo].[MasterGroupMappingCustomLabel] CHECK CONSTRAINT [FK_MasterGroupMappingCustomLabel_MasterGroupMapping]
	
      PRINT 'Added table MasterGroupMappingCustomLabel'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingCustomLabel already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingCustomLabel'

  ROLLBACK TRAN
END