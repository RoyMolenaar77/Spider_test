DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'ConnectorPublicationRuleID' 
    AND object_id = OBJECT_ID('Content')) 
		BEGIN
		
	alter table Content add ConnectorPublicationRuleID int null

			PRINT 'Added column ConnectorPublicationRuleID to Table Content'
	END
	ELSE
		BEGIN
			PRINT 'Column ConnectorPublicationRuleID already added to table Content'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column ConnectorPublicationRuleID to table Content'

	ROLLBACK TRAN
	END