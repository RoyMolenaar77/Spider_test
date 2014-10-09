DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT exists(select * from sys.columns where Name = N'ConnectorID' and Object_ID = Object_ID(N'ProductGroupMappingCustomLabel')) 
		BEGIN

			--first remove old pk
			alter table ProductGroupMappingCustomLabel
			drop constraint  PK__ProductG__A6FA448F20837C81

			--create new pk
			alter table ProductGroupMappingCustomLabel
			add ProductGroupMappingCustomLabelID int not null identity(1,1)

			--create new pk constraint
			alter table ProductGroupMappingCustomLabel
			add constraint PK_ProductGroupMappingCustomLabel_ProductGroupMappingCustomLabelID
			primary key ( ProductGroupMappingCustomLabelID)
		
			--create ConnectorID
			alter table ProductGroupMappingCustomLabel
			add ConnectorID int null

			alter table ProductGroupMappingCustomLabel
			add constraint FK_ProductGroupMappingCustomLabel_Connector foreign key(ConnectorID) references Connector (ConnectorID) on delete cascade on update cascade

			--create unique constraint 
			alter table ProductGroupMappingCustomLabel
			add constraint uc_ProductGroupMappingCustomLabel unique(ConnectorID, ProductGroupMappingID, LanguageID)
			
			PRINT 'Added ConnectorID to table ProductGroupMappingCustomLabel'
		END
	ELSE
		BEGIN
			PRINT 'Column ConnectorID already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating column ConnectorID to table ProductGroupMappingCustomLabel'

	ROLLBACK TRAN
END




