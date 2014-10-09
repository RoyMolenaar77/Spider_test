DECLARE @intErrorCode INT

BEGIN TRAN


  ALTER TABLE ContentAttribute 
  ALTER COLUMN AttributeOriginalValue nvarchar(2000) null
  
  PRINT 'Altered ContentAttribute.'


  SELECT @intErrorCode = @@ERROR
    IF (@intErrorCode <> 0)
      GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while altering ContentAttribute.'

  ROLLBACK TRAN
END