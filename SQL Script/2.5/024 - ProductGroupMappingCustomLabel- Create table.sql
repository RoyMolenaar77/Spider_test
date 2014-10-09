DECLARE @intErrorCode INT

BEGIN TRAN
	--Begin Action
    IF NOT exists(select * from sys.tables where name = N'ProductGroupMappingCustomLabel')    
	  begin
		
		CREATE TABLE [dbo].ProductGroupMappingCustomLabel(
			ProductGroupMappingID int not null constraint FK_ProductGroupMappingCustomLabel_ProductGroupMapping foreign key (ProductGroupMappingID) references ProductGroupMapping,
			LanguageID int not null constraint FK_ProductGroupMappingCustomLabel_Language foreign key (LanguageID) references [Language],
			CustomLabel nvarchar(250) not null,
			primary key (ProductGroupMappingID, LanguageID)
	  )
	
		print 'Added table ProductGroupMappingCustomLabel'
	  end
	else
	  begin
		print 'Table ProductGroupMappingCustomLabel already added to the database'
	  end
	--End Action 

	SELECT @intErrorCode = @@ERROR
    IF (@intErrorCode <> 0) GOTO PROBLEM
COMMIT TRAN

PROBLEM:
  IF (@intErrorCode <> 0) 
  BEGIN
    PRINT 'Unexpected error occurred while creating table ProductGroupMappingCustomLabel'
    ROLLBACK TRAN
END


