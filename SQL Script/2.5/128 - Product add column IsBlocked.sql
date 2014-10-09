DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'IsBlocked' 
    AND object_id = OBJECT_ID('Product')) 
		BEGIN

	ALTER TABLE dbo.Product ADD
	IsBlocked bit not null Default 0	

			PRINT 'Added column IsBlocked to Table Product'
	END
	ELSE
		BEGIN
			PRINT 'Column IsBlocked already added to table Product'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column IsBlocked to table Product'

	ROLLBACK TRAN
	END