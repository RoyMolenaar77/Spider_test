DECLARE	@return_value int

IF((select count(*) from MasterGroupMapping) = 0)
BEGIN

EXEC	@return_value = [dbo].[MigrationProductGroupMapping]
		@CopyProductGroupMappingToRoot = 0,
		@ReverseScore = 0,
		@RootMasterGroupMappingName = N'Coolcat',
		@CopyFromConnectorID = 5,
		@OnlyMigrateForVendor = 1


IF @return_value = 0 BEGIN print('Migrated Data for Coolcat') END ELSE BEGIN print('Something went wrong when migrating mapping for Coolcat: ' + @return_value)END


EXEC	@return_value = [dbo].[MigrationProductGroupMapping]
		@CopyProductGroupMappingToRoot = 1,
		@ReverseScore = 0,
		@CopyFromConnectorID = 5,
		@CopyToConnectorID = 5,
		@OnlyMigrateForVendor = 1


IF @return_value = 0 BEGIN print('Migrated Data for Coolcat Connector') END ELSE BEGIN print('Something went wrong when migrating mapping for Coolcat Connector: ' + @return_value)END


EXEC	@return_value = [dbo].[MigrationProductGroupMapping]
		@CopyProductGroupMappingToRoot = 0,
		@ReverseScore = 0,
		@RootMasterGroupMappingName = N'AT',
		@CopyFromConnectorID = 6,
		@OnlyMigrateForVendor = 2

IF @return_value = 0 BEGIN print('Migrated Data for AT') END ELSE BEGIN print('Something went wrong when migrating mapping for AT: ' + @return_value)END


EXEC	@return_value = [dbo].[MigrationProductGroupMapping]
		@CopyProductGroupMappingToRoot = 1,
		@ReverseScore = 0,
		@CopyFromConnectorID = 6,
		@CopyToConnectorID = 6,
		@OnlyMigrateForVendor = 2


IF @return_value = 0 BEGIN print('Migrated Data for AT Connector') END ELSE BEGIN print('Something went wrong when migrating mapping for AT Connector: ' + @return_value)END


EXEC	@return_value = [dbo].[MigrationProductGroupMapping]
		@CopyProductGroupMappingToRoot = 0,
		@ReverseScore = 0,
		@RootMasterGroupMappingName = N'Sapph',
		@CopyFromConnectorID = 11,
		@OnlyMigrateForVendor = 50


IF @return_value = 0 BEGIN print('Migrated Data for Sapph') END ELSE BEGIN print('Something went wrong when migrating mapping for Sapph: ' + @return_value)END


EXEC	@return_value = [dbo].[MigrationProductGroupMapping]
		@CopyProductGroupMappingToRoot = 1,
		@ReverseScore = 0,
		@CopyFromConnectorID = 11,
		@CopyToConnectorID = 11,
		@OnlyMigrateForVendor = 50


IF @return_value = 0 BEGIN print('Migrated Data for Sapph Connector') END ELSE BEGIN print('Something went wrong when migrating mapping for Sapph Connector: ' + @return_value)END

END
ELSE
BEGIN
print('MasterGroupMapping table not empty, Product group mapping migration probably already happend')
END
