DECLARE @intErrorCode INT

BEGIN TRAN


IF NOT EXISTS ( SELECT * FROM ProductAttributeMetaData WHERE AttributeCode = 'Gender' )

  BEGIN

  INSERT INTO ProductAttributeMetaData
	VALUES('Gender', 4, null, null, 0, 1, 0, 48, 1, null, 1, getdate(), null, null, null, 0, null, 0, null, null, 0)

  PRINT 'Added gender attribute.'

  END

ELSE
  BEGIN
    PRINT 'Gender attribute already added.'
  END

  SELECT @intErrorCode = @@ERROR
    IF (@intErrorCode <> 0)
      GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
  PRINT 'Unexpected error occurred while adding the Gender attribute.'

  ROLLBACK TRAN
END