DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'MasterGroupMappingID' 
    AND object_id = OBJECT_ID('ContentProductGroup')) 
		BEGIN

		ALTER TABLE dbo.ContentProductGroup ADD
	    MasterGroupMappingID int NULL
	
		ALTER TABLE dbo.ContentProductGroup ADD CONSTRAINT
			FK_ContentProductGroup_MasterGroupMapping FOREIGN KEY
			(
			MasterGroupMappingID
			) REFERENCES dbo.MasterGroupMapping
			(
			MasterGroupMappingID
			) ON UPDATE  NO ACTION 
			ON DELETE  CASCADE 

			PRINT 'Added column MasterGroupMappingID to Table ContentProductGroup'
	END
	ELSE
		BEGIN
			PRINT 'Column MasterGroupMappingID already added to table ContentProductGroup'
		END
	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column MasterGroupMappingID to table ContentProductGroup'

	ROLLBACK TRAN
	END