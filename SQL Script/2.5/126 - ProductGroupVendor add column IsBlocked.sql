DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'IsBlocked' 
    AND object_id = OBJECT_ID('ProductGroupVendor')) 
		BEGIN
		
    ALTER TABLE dbo.ProductGroupVendor ADD
	IsBlocked bit NOT NULL Default 0

			PRINT 'Added column IsBlocked to Table ProductGroupVendor'
	END
	ELSE
		BEGIN
			PRINT 'Column IsBlocked already added to table ProductGroupVendor'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column IsBlocked to table ProductGroupVendor'

	ROLLBACK TRAN
	END