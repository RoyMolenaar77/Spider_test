DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'MasterGroupMappingID' 
    AND object_id = OBJECT_ID('ProductGroupMapping')) 
		BEGIN

		ALTER TABLE dbo.ProductGroupMapping
		add MasterGroupMappingID int not null default 0

			PRINT 'Added column MasterGroupMappingID to Table ProductGroupMapping'
	END
	ELSE
		BEGIN
			PRINT 'Column MasterGroupMappingID already added to table ProductGroupMapping'
		END	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column MasterGroupMappingID to table ProductGroupMapping'

	ROLLBACK TRAN
	END