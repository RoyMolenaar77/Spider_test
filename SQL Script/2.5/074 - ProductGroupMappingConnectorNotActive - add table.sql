DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'ProductGroupMappingConnectorNotActive' )
		BEGIN
		
			create table ProductGroupMappingConnectorNotActive(
				ConnectorID int not null,
				ProductGroupMappingID int not null,
				primary key(ConnectorID,ProductGroupMappingID)
			)

			alter table ProductGroupMappingConnectorNotActive 
			add constraint FK_Connector_ProductGroupMappingConnectorNotActive foreign key (ConnectorID) references Connector(ConnectorID) on delete cascade on update cascade
			
			alter table ProductGroupMappingConnectorNotActive 
			add constraint FK_ProductGroupMapping_ProductGroupMappingConnectorNotActive foreign key (ProductGroupMappingID) references ProductGroupMapping(ProductGroupMappingID) on delete cascade on update cascade



			PRINT 'Added table ProductGroupMappingConnectorNotActive'
		END
	ELSE
		BEGIN
			PRINT 'Table ProductGroupMappingConnectorNotActive already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating table ProductGroupMappingConnectorNotActive'

	ROLLBACK TRAN
END