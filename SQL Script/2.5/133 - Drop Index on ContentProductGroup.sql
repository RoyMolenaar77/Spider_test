DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF EXISTS (
	SELECT * 
FROM sys.indexes 
WHERE name='Index' AND object_id = OBJECT_ID('ContentProductGroup')) 
		BEGIN

		drop INDEX [Index] on dbo.ContentProductGroup

			PRINT 'Dropped index [Index] on Table ContentProductGroup'
	END
	ELSE
		BEGIN
			PRINT 'Index [Index] already dropped on table ContentProductGroup'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while dropping index [Index] on table ContentProductGroup'

	ROLLBACK TRAN
	END