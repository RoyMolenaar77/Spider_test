DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'MasterGroupMappingID' 
    AND object_id = OBJECT_ID('MagentoProductGroupSetting')) 
		BEGIN

	ALTER TABLE dbo.MagentoProductGroupSetting ADD
	[MasterGroupMappingID] int null

    ALTER TABLE [dbo].[MagentoProductGroupSetting] ADD  CONSTRAINT [DF__MagentoPr__Maste__28552256]  DEFAULT ((0)) FOR [MasterGroupMappingID]
    
    ALTER TABLE [dbo].[MagentoProductGroupSetting]  WITH CHECK ADD  CONSTRAINT [FK_MagentoProductGroupSetting_MasterGroupMapping] FOREIGN KEY([MasterGroupMappingID])
    REFERENCES [dbo].[MasterGroupMapping] ([MasterGroupMappingID])
    ON DELETE CASCADE

			PRINT 'Added column MasterGroupMappingID to Table MagentoProductGroupSetting'
	END
	ELSE
		BEGIN
			PRINT 'Column MasterGroupMappingID already added to table MagentoProductGroupSetting'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column MasterGroupMappingID to table MagentoProductGroupSetting'

	ROLLBACK TRAN
	END