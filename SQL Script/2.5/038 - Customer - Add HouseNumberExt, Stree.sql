DECLARE @intErrorCode INT

BEGIN TRAN
	--Begin Action
    IF NOT exists(select * from sys.columns where Name = N'Street' and Object_ID = Object_ID(N'Customer'))    
	  begin
		ALTER TABLE Customer
		ADD 
			Street nvarchar(300) null,
			HouseNumberExt nvarchar(15) null
		print 'Added Street, HouseNumberExt columns to table Customer'
	  end
	else
	  begin
		print 'Columns <Street, HouseNumberExt> already added to the table <Customer>'
	  end
	--End Action 

	SELECT @intErrorCode = @@ERROR
    IF (@intErrorCode <> 0) GOTO PROBLEM
COMMIT TRAN

PROBLEM:
  IF (@intErrorCode <> 0) 
  BEGIN
    PRINT 'Unexpected error occurred while <Street, HouseNumberExt> to table <Customer>!'
    ROLLBACK TRAN
END
