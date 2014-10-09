DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'ThumbnailPath' 
    AND object_id = OBJECT_ID('MasterGroupMapping')) 
		BEGIN
		
		Alter Table MasterGroupMapping ADD ThumbnailPath nvarchar(255) null

			PRINT 'Added column ThumbnailPath to Table MasterGroupMapping'
	END
	ELSE
		BEGIN
			PRINT 'Column ThumbnailPath already added to table MasterGroupMapping'
		END	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column ThumbnailPath to table MasterGroupMapping'

	ROLLBACK TRAN
	END