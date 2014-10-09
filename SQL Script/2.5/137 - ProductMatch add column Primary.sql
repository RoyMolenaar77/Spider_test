DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'Primary' 
    AND object_id = OBJECT_ID('ProductMatch')) 
		BEGIN
		
	ALTER TABLE dbo.ProductMatch ADD
	[Primary] bit null

			PRINT 'Added column Primary to Table ProductMatch'
	END
	ELSE
		BEGIN
			PRINT 'Column Primary already added to table ProductMatch'
		END	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column [Primary] to table ProductMatch'

	ROLLBACK TRAN
	END