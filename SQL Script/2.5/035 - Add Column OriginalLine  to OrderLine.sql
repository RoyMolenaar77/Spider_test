
-- =============================================
-- Author     : Dashti 
-- Create date: 2013-07-22
-- Description: Add OriginalLine To OrderLine
-- =============================================

DECLARE @intErrorCode INT

BEGIN TRAN
	--Begin Action
    IF NOT exists(select * from sys.columns where Name = N'OriginalLine' and Object_ID = Object_ID(N'OrderLine'))    
	  begin
		ALTER TABLE OrderLine
		ADD OriginalLine nvarchar(max) null
		print 'Added OriginalLine column to table <OrderLine>'
	  end
	else
	  begin
		print 'Column <OriginalLine> already added to the table <OrderLine>'
	  end
	--End Action 

	SELECT @intErrorCode = @@ERROR
    IF (@intErrorCode <> 0) GOTO PROBLEM
COMMIT TRAN

PROBLEM:
  IF (@intErrorCode <> 0) 
  BEGIN
    PRINT 'Unexpected error occurred while <add OriginalLine to OrderLine>!'
    ROLLBACK TRAN
END




