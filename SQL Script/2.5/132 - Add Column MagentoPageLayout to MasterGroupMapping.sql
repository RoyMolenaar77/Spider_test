DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'MagentoPageLayoutID' 
    AND object_id = OBJECT_ID('MasterGroupMapping')) 
		BEGIN
		
			ALTER TABLE dbo.MasterGroupMapping ADD
	MagentoPageLayoutID int NULL
ALTER TABLE dbo.MasterGroupMapping ADD CONSTRAINT
	FK_MasterGroupMapping_MagentoPageLayout FOREIGN KEY
	(
	MagentoPageLayoutID
	) REFERENCES dbo.MagentoPageLayout
	(
	LayoutID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

			PRINT 'Added column MagentoPageLayoutID to Table MasterGroupMapping'
	END
	ELSE
		BEGIN
			PRINT 'Column MagentoPageLayoutID already added to table MasterGroupMapping'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column MagentoPageLayoutID to table MasterGroupMapping'

	ROLLBACK TRAN
	END