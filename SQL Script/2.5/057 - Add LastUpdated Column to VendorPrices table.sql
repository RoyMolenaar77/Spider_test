IF NOT exists(select * from sys.columns where Name = N'LastUpdated' and Object_ID = Object_ID(N'VendorPrice'))    
  begin
    ALTER TABLE [dbo].[VendorPrice]
    ADD LastUpdated datetime NOT NULL DEFAULT(getdate())
    print 'Added LastUpdated column to table VendorPrice'
  end
else
  begin
    print 'Column LastUpdated already added to the table VendorPrice'
  end
 
 