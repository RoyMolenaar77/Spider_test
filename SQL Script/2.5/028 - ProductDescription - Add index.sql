IF NOT exists(select * from sys.indexes where object_id = object_id('ProductDescription') and name = 'ProductDescription_VendorID_Index')    
  begin
	

CREATE NONCLUSTERED INDEX ProductDescription_VendorID_Index
ON [dbo].[ProductDescription] ([VendorID])
INCLUDE ([ProductID])

    print 'Added Index to ProductDescription'
  end
else
  begin
    print 'ProductDescription index already added to the table'
  end



