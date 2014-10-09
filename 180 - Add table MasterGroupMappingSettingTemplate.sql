DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSettingTemplate' ) 
  OR 
  (
    EXISTS (SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSettingTemplate') AND NOT EXISTS ( SELECT * FROM MasterGroupMappingSettingTemplate )
  ) 
		
		BEGIN
		
    IF EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSettingTemplate' ) AND NOT EXISTS ( SELECT * FROM MasterGroupMappingSettingTemplate )
    BEGIN
      DROP TABLE MasterGroupMappingSettingTemplate
    END

	CREATE TABLE [dbo].[MasterGroupMappingSettingTemplate](
	[MasterGroupMappingSettingTemplateID] [int] IDENTITY(1,1) NOT NULL,
	[MasterGroupMappingSettingID] [int] NOT NULL,
	[LanguageID] [int] NOT NULL,
	[DefaultValue] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_MasterGroupMappingSettingTemplate] PRIMARY KEY CLUSTERED 
(
	[MasterGroupMappingSettingTemplateID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]


ALTER TABLE [dbo].[MasterGroupMappingSettingTemplate]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingSettingTemplate_MasterGroupMappingSetting] FOREIGN KEY([MasterGroupMappingSettingID])
REFERENCES [dbo].[MasterGroupMappingSetting] ([MasterGroupMappingSettingID])

ALTER TABLE [dbo].[MasterGroupMappingSettingTemplate] CHECK CONSTRAINT [FK_MasterGroupMappingSettingTemplate_MasterGroupMappingSetting]

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