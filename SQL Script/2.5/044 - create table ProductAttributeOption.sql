DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'ProductAttributeOption' )
		BEGIN
		
			create table ProductAttributeOption(
				OptionID int primary key identity(1,1),
				AttributeID int not null,
				AttributeOption nvarchar(500) not null
			)


			alter table ProductAttributeOption 
			add constraint FK_ProductAttributeOption_ProductAttributeMetaData foreign key (AttributeID) 
			references ProductAttributeMetaData (AttributeID) on delete cascade on update cascade


			PRINT 'Added table ProductAttributeOption'
		END
	ELSE
		BEGIN
			PRINT 'Table ProductAttributeOption already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating table ProductAttributeOption'

	ROLLBACK TRAN
END