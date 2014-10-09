DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'ConnectorID' 
    AND object_id = OBJECT_ID('SeoTexts')) 
		BEGIN

		  alter table SeoTexts add ConnectorID int null

			PRINT 'Added column ConnectorID to Table SeoTexts'
	END
	ELSE
		BEGIN
			PRINT 'Column ConnectorID already added to table SeoTexts'
		END

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column ConnectorID to table SeoTexts'

	ROLLBACK TRAN
	END