DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSetting' )
		
		BEGIN
		
	CREATE TABLE [dbo].[MasterGroupMappingSetting](
		[MasterGroupMappingSettingID] [int] IDENTITY(1,1) NOT NULL,
		[MasterGroupMappingID] [int] NOT NULL,
		[Code] [nvarchar](255) NOT NULL,
		[Value] [nvarchar](max) NOT NULL,
	PRIMARY KEY CLUSTERED 
	(
		[MasterGroupMappingSettingID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	CONSTRAINT [unique_MasterGroupMappingSetting] UNIQUE NONCLUSTERED 
	(
		[MasterGroupMappingID] ASC,
		[Code] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
		
	ALTER TABLE [dbo].[MasterGroupMappingSetting]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingSetting_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
	REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
		
	ALTER TABLE [dbo].[MasterGroupMappingSetting] CHECK CONSTRAINT [FK_MasterGroupMappingSetting_MasterGroupMapping]
		
      PRINT 'Added table MasterGroupMappingSetting'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingSetting already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingSetting'

  ROLLBACK TRAN
END
