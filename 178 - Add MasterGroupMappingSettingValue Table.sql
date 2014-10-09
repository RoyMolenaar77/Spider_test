DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSettingValue' )
		
		BEGIN
		
	CREATE TABLE [dbo].[MasterGroupMappingSettingValue](
	[MasterGroupMappingID] [int] NOT NULL,
	[MasterGroupMappingSettingID] [int] NOT NULL,
	[Value] [nvarchar](250) NULL,
	[LanguageID] [int] NULL,
 CONSTRAINT [PK_MasterGroupMappingSettingValue] PRIMARY KEY CLUSTERED 
(
	[MasterGroupMappingID] ASC,
	[MasterGroupMappingSettingID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[MasterGroupMappingSettingValue]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingSettingValue_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])

ALTER TABLE [dbo].[MasterGroupMappingSettingValue] CHECK CONSTRAINT [FK_MasterGroupMappingSettingValue_MasterGroupMapping]

ALTER TABLE [dbo].[MasterGroupMappingSettingValue]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingSettingValue_MasterGroupMappingSetting] FOREIGN KEY([MasterGroupMappingSettingID])
REFERENCES [dbo].[MasterGroupMappingSetting] ([MasterGroupMappingSettingID])

ALTER TABLE [dbo].[MasterGroupMappingSettingValue] CHECK CONSTRAINT [FK_MasterGroupMappingSettingValue_MasterGroupMappingSetting]
		
      PRINT 'Added table MasterGroupMappingSettingValue'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingSettingValue already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingSettingValue'

  ROLLBACK TRAN
END