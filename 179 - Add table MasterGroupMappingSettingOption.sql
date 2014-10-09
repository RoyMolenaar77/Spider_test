DECLARE @intErrorCode INT

BEGIN TRAN

  --Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'MasterGroupMappingSettingOption' )
		
		BEGIN
		
    CREATE TABLE [dbo].[MasterGroupMappingSettingOption](
	[MasterGroupMappingSettingID] [int] NOT NULL,
	[OptionID] [int] IDENTITY(1,1) NOT NULL,
	[Value] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_MasterGroupMappingSettingOption] PRIMARY KEY CLUSTERED 
(
	[MasterGroupMappingSettingID] ASC,
	[OptionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [dbo].[MasterGroupMappingSettingOption]  WITH CHECK ADD  CONSTRAINT [FK_MasterGroupMappingSettingOption_MasterGroupMappingSetting] FOREIGN KEY([MasterGroupMappingSettingID])
REFERENCES [dbo].[MasterGroupMappingSetting] ([MasterGroupMappingSettingID])

ALTER TABLE [dbo].[MasterGroupMappingSettingOption] CHECK CONSTRAINT [FK_MasterGroupMappingSettingOption_MasterGroupMappingSetting]
		
      PRINT 'Added table MasterGroupMappingSettingOption'
    END
  ELSE
    BEGIN
      PRINT 'Table MasterGroupMappingSettingOption already added to the database'
    END
  --End Action 

  SELECT @intErrorCode = @@ERROR
  IF (@intErrorCode <> 0)
    GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while creating table MasterGroupMappingSettingOption'

  ROLLBACK TRAN
END