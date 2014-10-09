
DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'DatcolLink' )
		BEGIN
		
			CREATE TABLE DatcolLink
			(
				 Id int not null identity (1,1) primary key,
         OrderID int not null,
         ShopNumber nvarchar(100) not null,
         DateCreated datetime not null default((getdate())),
         Amount decimal(18,4) not null,
         DatcolNumber nvarchar(10) not null,
         SourceMessage nvarchar(200) null         
			)

      alter table DatcolLink add constraint FK_DatcolLink_Order foreign key (OrderID) references [Order] (OrderID)

			PRINT 'Added table DatcolLink'
		END
	ELSE
		BEGIN
			PRINT 'Table DatcolLink already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating table DatcolLink'

	ROLLBACK TRAN
END