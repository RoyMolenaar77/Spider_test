DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingLanguage' )
		
		BEGIN
		
	CREATE TABLE [dbo].[MasterGroupMappingLanguage](
		[MasterGroupMappingID] [int] NOT NULL,
		[LanguageID] [int] NOT NULL,
		[Name] [nvarchar](255) NOT NULL,
	CONSTRAINT [PK_MasterGroupMappingLanguage] PRIMARY KEY CLUSTERED 
	(
		[MasterGroupMappingID] ASC,
		[LanguageID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
	
	
	ALTER TABLE [dbo].[MasterGroupMappingLanguage]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingLanguage_Language] FOREIGN KEY([LanguageID])
	REFERENCES [dbo].[Language] ([LanguageID])
		
	ALTER TABLE [dbo].[MasterGroupMappingLanguage] CHECK CONSTRAINT [FK_MasterGroupMappingLanguage_Language]
		
	ALTER TABLE [dbo].[MasterGroupMappingLanguage]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingLanguage_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
	ON DELETE CASCADE
	
	ALTER TABLE [dbo].[MasterGroupMappingLanguage] CHECK CONSTRAINT [FK_MasterGroupMappingLanguage_MasterGroupMapping]
		
      PRINT 'Added table MasterGroupMappingLanguage'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingLanguage already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingLanguage'

  ROLLBACK TRAN
END
