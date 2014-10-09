DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'ExcludeProduct' )
		BEGIN
		
			create table ExcludeProduct(
				ExcludeProductID int primary key identity(1,1),
				ConnectorID int not null,
				Value nvarchar(200) not null,
				CONSTRAINT uc_ConnectorValue UNIQUE (ConnectorID, Value)
			)

			alter table ExcludeProduct 
			add constraint FK_Connector_ExcludeProduct foreign key (ConnectorID) references Connector(ConnectorID) on delete cascade on update cascade



			PRINT 'Added table ExcludeProduct'
		END
	ELSE
		BEGIN
			PRINT 'Table ExcludeProduct already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating table ExcludeProduct'

	ROLLBACK TRAN
END