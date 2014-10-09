IF NOT exists(select * from sys.indexes where object_id = object_id('MissingContent') and name = 'MissingContent_Index_AllContent')    
  begin
	

CREATE NONCLUSTERED INDEX MissingContent_Index_AllContent
ON [dbo].[MissingContent] ([ConnectorID])
INCLUDE ([ConcentratorProductID],[isActive],[VendorItemNumber],[CustomItemNumber],[BrandName],[ShortDescription],[ProductGroupID],[BrandID],[Image],[YouTube],[Specifications],[ContentVendor],[ContentVendorID],[Barcode],[CreationTime],[LastModificationTime],[IsConfigurable],[HasDescription],[QuantityOnHand])

    print 'Added Index to MissingContent'
  end
else
  begin
    print 'MissingContent index already added to the table'
  end






