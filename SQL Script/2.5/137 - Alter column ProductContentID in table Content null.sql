DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'ProductContentID' 
    AND object_id = OBJECT_ID('Content')) 
		BEGIN

     alter table Content alter column ProductContentID int NULL

			PRINT 'Altered column ProductContentID on Table Content set NULL'
	END
	ELSE
		BEGIN
			PRINT 'Column ProductContentID does not exist on table Content'
		END
	
	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while setting column ProductContentID on table Content to NULL'

	ROLLBACK TRAN
	END

