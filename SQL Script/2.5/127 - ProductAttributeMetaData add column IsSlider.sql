DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'IsSlider' 
    AND object_id = OBJECT_ID('ProductAttributeMetaData')) 
		BEGIN

	ALTER TABLE dbo.ProductAttributeMetaData ADD
	IsSlider bit not null Default 0
	
			PRINT 'Added column IsSlider to Table ProductAttributeMetaData'
	END
	ELSE
		BEGIN
			PRINT 'Column IsSlider already added to table ProductAttributeMetaData'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column IsSlider to table ProductAttributeMetaData'

	ROLLBACK TRAN
	END