DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSettingTemplate' )
		
		BEGIN
		
	CREATE TABLE [dbo].[MasterGroupMappingSettingTemplate](
		[MasterGroupMappingSettingTemplateID] [int] IDENTITY(1,1) NOT NULL,
		[Type] [nvarchar](50) NOT NULL,
		[Code] [nvarchar](255) NOT NULL,
		[DefaultValue] [nvarchar](max) NULL,
	PRIMARY KEY CLUSTERED 
	(
		[MasterGroupMappingSettingTemplateID] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY],
	CONSTRAINT [unique_MasterGroupMappingSettingTemplate] UNIQUE NONCLUSTERED 
	(
		[Type] ASC,
		[Code] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
		
      PRINT 'Added table MasterGroupMappingSettingTemplate'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingSettingTemplate already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingSettingTemplate'

  ROLLBACK TRAN
END
