DECLARE @intErrorCode INT

BEGIN TRAN

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'CommunicatorMessage' )
		BEGIN
		
			create table CommunicatorMessage(
				ID int not null identity(1,1) primary key,
				[Type] int not null,
				LocalSubPath nvarchar(200) not null,
        RemoteSubPath nvarchar(200) not null,
        Incoming bit not null default 1
			)


		
			PRINT 'Added table CommunicatorMessage'
		END
	ELSE
		BEGIN
			PRINT 'Table CommunicatorMessage already added to the database'
		END
	--End Action 

	--Begin Action
	IF NOT EXISTS ( SELECT * FROM sys.tables WHERE NAME = 'VendorCommunicatorMessage' )
		BEGIN
		
			create table VendorCommunicatorMessage(
				MessageID int not null,
				VendorID int not null, 
        Primary Key (VendorID, MessageID)
			)

      alter table VendorCommunicatorMessage add constraint FK_VendorCommunicatorMessage_Vendor foreign key (VendorID) references Vendor(VendorID) ON DELETE Cascade

      alter table VendorCommunicatorMessage add constraint FK_VendorCommunicatorMessage_CommunicatorMessage foreign key (MessageID) references CommunicatorMessage(ID) ON DELETE Cascade
		
			PRINT 'Added table VendorCommunicatorMessage'
		END
	ELSE
		BEGIN
			PRINT 'Table VendorCommunicatorMessage already added to the database'
		END
	--End Action 

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

COMMIT TRAN
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while creating table VendorCommunicatorMessage'

	ROLLBACK TRAN
END