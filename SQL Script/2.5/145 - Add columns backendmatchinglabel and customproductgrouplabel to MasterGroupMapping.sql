DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'CustomProductGroupLabel' 
    AND object_id = OBJECT_ID('MasterGroupMapping')) 
		BEGIN

		alter table MasterGroupMapping add CustomProductGroupLabel nvarchar(255) null

			PRINT 'Added column CustomProductGroupLabel to Table MasterGroupMapping'
	END
	ELSE
		BEGIN
			PRINT 'Column CustomProductGroupLabel already added to table MasterGroupMapping'
		END

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'BackendMatchingLabel' 
    AND object_id = OBJECT_ID('MasterGroupMapping')) 
		BEGIN

		alter table MasterGroupMapping add BackendMatchingLabel nvarchar(200) null

			PRINT 'Added column BackendMatchingLabel to Table MasterGroupMapping'
	END
	ELSE
		BEGIN
			PRINT 'Column BackendMatchingLabel already added to table MasterGroupMapping'
		END

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding columns BackendMatchingLabel and CustomProductGroupLabel to table MasterGroupMapping'

	ROLLBACK TRAN
	END