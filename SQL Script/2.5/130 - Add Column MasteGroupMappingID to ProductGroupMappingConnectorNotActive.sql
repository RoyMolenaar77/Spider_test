DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action		
IF EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'ProductGroupMappingID' 
    AND object_id = OBJECT_ID('ProductGroupMappingConnectorNotActive')) 
		BEGIN
		
   delete from ProductGroupMappingConnectorNotActive

   DECLARE @sql NVARCHAR(max);
   
   SELECT @sql = 'ALTER TABLE ProductGroupMappingConnectorNotActive'  
    + ' DROP CONSTRAINT ' + name + ';'
    FROM sys.key_constraints
    WHERE [type] = 'PK'
    AND [parent_object_id] = OBJECT_ID('ProductGroupMappingConnectorNotActive');

	EXEC sp_executeSQL @sql;

	ALTER TABLE dbo.ProductGroupMappingConnectorNotActive
	DROP CONSTRAINT FK_ProductGroupMapping_ProductGroupMappingConnectorNotActive
	
	ALTER TABLE dbo.ProductGroupMappingConnectorNotActive
	DROP COLUMN ProductGroupMappingID
	
			PRINT 'Dropped column ProductGroupMappingID to Table ProductGroupMappingConnectorNotActive'
	END
	ELSE
		BEGIN
			PRINT 'Column ProductGroupMappingID already dropped on table ProductGroupMappingConnectorNotActive'
		END
		
IF NOT EXISTS (
	SELECT * 
    FROM   sys.columns 
    WHERE  name = 'MasterGroupMappingID' 
    AND object_id = OBJECT_ID('ProductGroupMappingConnectorNotActive')) 
		BEGIN
		
	ALTER TABLE dbo.ProductGroupMappingConnectorNotActive ADD
	MasterGroupMappingID int NOT NULL
	
ALTER TABLE dbo.ProductGroupMappingConnectorNotActive ADD CONSTRAINT
	FK_ProductGroupMappingConnectorNotActive_MasterGroupMapping FOREIGN KEY
	(
	MasterGroupMappingID
	) REFERENCES dbo.MasterGroupMapping
	(
	MasterGroupMappingID
	) ON UPDATE  NO ACTION 
	 ON DELETE  CASCADE
	 
	 ALTER TABLE dbo.ProductGroupMappingConnectorNotActive ADD CONSTRAINT
	PK_ProductGroupMappingConnectorNotActive_1 PRIMARY KEY CLUSTERED 
	(
	ConnectorID,
	MasterGroupMappingID
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	

			PRINT 'Added column MasterGroupMappingID to Table ProductGroupMappingConnectorNotActive'
	END
	ELSE
		BEGIN
			PRINT 'Column MasterGroupMappingID already added to table ProductGroupMappingConnectorNotActive'
		END	

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while adding column MasterGroupMappingID to table ProductGroupMappingConnectorNotActive'

	ROLLBACK TRAN
	END

