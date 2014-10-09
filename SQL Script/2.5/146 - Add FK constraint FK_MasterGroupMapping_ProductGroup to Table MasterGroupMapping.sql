DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (SELECT * 
  FROM sys.foreign_keys 
   WHERE object_id = OBJECT_ID(N'dbo.FK_MasterGroupMapping_ProductGroup')
   AND parent_object_id = OBJECT_ID(N'dbo.MasterGroupMapping')
)
  BEGIN
ALTER TABLE dbo.MasterGroupMapping ADD CONSTRAINT
	FK_MasterGroupMapping_ProductGroup FOREIGN KEY
	(
	ProductGroupID
	) REFERENCES dbo.ProductGroup
	(
	ProductGroupID
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 

			PRINT 'Added FK constraint FK_MasterGroupMapping_ProductGroup to Table MasterGroupMapping'
	END
	ELSE
		BEGIN
			PRINT 'FK constraint FK_MasterGroupMapping_ProductGroup already added to Table MasterGroupMapping'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding FK constraint FK_MasterGroupMapping_ProductGroup to Table MasterGroupMapping'

	ROLLBACK TRAN
	END
