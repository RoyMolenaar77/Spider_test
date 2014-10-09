DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingMedia' )
		
		BEGIN	

	    CREATE TABLE [dbo].[MasterGroupMappingMedia](
	[MasterGroupMappingMediaID] [int] IDENTITY(1,1) NOT NULL,
	[MasterGroupMappingID] [int] NOT NULL,
	[ImageTypeID] [int] NOT NULL,
	[ImagePath] [varchar](250) NOT NULL,
	[ConnectorID] [int] NULL,
	[LanguageID] [int] NULL,
	[CreationTime] [datetime] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[LastModificationTime] [datetime] NULL,
	[LastModifiedBy] [int] NULL,
	CONSTRAINT [PK_MasterGroupMappingMedia] PRIMARY KEY CLUSTERED 
	(
		[MasterGroupMappingMediaID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	ALTER TABLE [dbo].[MasterGroupMappingMedia]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingMedia_Connector] FOREIGN KEY([ConnectorID])
	REFERENCES [dbo].[Connector] ([ConnectorID])
	
	ALTER TABLE [dbo].[MasterGroupMappingMedia] CHECK CONSTRAINT [FK_MasterGroupMappingMedia_Connector]
	
	ALTER TABLE [dbo].[MasterGroupMappingMedia]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingMedia_Language] FOREIGN KEY([LanguageID])
	REFERENCES [dbo].[Language] ([LanguageID])
	
	ALTER TABLE [dbo].[MasterGroupMappingMedia] CHECK CONSTRAINT [FK_MasterGroupMappingMedia_Language]
	
	ALTER TABLE [dbo].[MasterGroupMappingMedia]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingMedia_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
	
	ALTER TABLE [dbo].[MasterGroupMappingMedia] CHECK CONSTRAINT [FK_MasterGroupMappingMedia_MasterGroupMapping]

  INSERT INTO MasterGroupMappingMedia(MasterGroupMappingID, ImageTypeID, ImagePath, ConnectorID, LanguageID, CreationTime, CreatedBy, LastModificationTime, LastModifiedBy)
  SELECT MasterGroupMappingID, 1, ImagePath, NULL, NULL, GETDATE(), 1, NULL, NULL
  FROM MasterGroupMapping
  WHERE ImagePath IS NOT NULL

  INSERT INTO MasterGroupMappingMedia(MasterGroupMappingID, ImageTypeID, ImagePath, ConnectorID, LanguageID, CreationTime, CreatedBy, LastModificationTime, LastModifiedBy)
  SELECT MasterGroupMappingID, 2, ThumbnailPath, NULL, NULL, GETDATE(), 1, NULL, NULL
  FROM MasterGroupMapping
  WHERE ThumbnailPath IS NOT NULL

  ALTER TABLE MasterGroupMapping
  DROP COLUMN ImagePath, ThumbnailPath

    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingMedia already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingMedia'

  ROLLBACK TRAN
END