DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'MasterGroupMappingID' 
    AND object_id = OBJECT_ID('SeoTexts')) 
		BEGIN
		
	ALTER TABLE dbo.SeoTexts ADD MasterGroupMappingID INT NULL

	ALTER TABLE dbo.SeoTexts

    ALTER COLUMN ProductGroupMappingID INT NULL

			PRINT 'Added column MasterGroupMappingID to Table SeoTexts'
	END
	ELSE
		BEGIN
			PRINT 'Column MasterGroupMappingID already added to table SeoTexts'
		END	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column MasterGroupMappingID to table SeoTexts'

	ROLLBACK TRAN
	END
