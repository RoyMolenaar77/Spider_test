DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingDescription' )
		
		BEGIN
		
	CREATE TABLE [dbo].[MasterGroupMappingDescription](
		[MasterGroupMappingID] [int] NOT NULL,
		[LanguageID] [int] NOT NULL,
		[Description] [nvarchar](max) NULL,
	CONSTRAINT [PK_MasterGroupMappingDescription] PRIMARY KEY CLUSTERED 
	(
		[MasterGroupMappingID] ASC,
		[LanguageID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
	
	
	ALTER TABLE [dbo].[MasterGroupMappingDescription]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingDescription_Language] FOREIGN KEY([LanguageID])
	REFERENCES [dbo].[Language] ([LanguageID])
		
	ALTER TABLE [dbo].[MasterGroupMappingDescription] CHECK CONSTRAINT [FK_MasterGroupMappingDescription_Language]
		
	ALTER TABLE [dbo].[MasterGroupMappingDescription]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingDescription_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
	ON DELETE CASCADE
		
	ALTER TABLE [dbo].[MasterGroupMappingDescription] CHECK CONSTRAINT [FK_MasterGroupMappingDescription_MasterGroupMapping]
		
      PRINT 'Added table MasterGroupMappingDescription'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingDescription already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingDescription'

  ROLLBACK TRAN
END