IF NOT exists(select * from sys.indexes where object_id = object_id('MissingContent') and name = 'MissingContent_ConnectorIndex')    
  begin


CREATE NONCLUSTERED INDEX MissingContent_ConnectorIndex
ON [dbo].[MissingContent] ([ConnectorID])
INCLUDE ([ConcentratorProductID],[VendorItemNumber],[CustomItemNumber],[BrandName],[ShortDescription],[Image],[YouTube],[Specifications],[CreationTime],[LastModificationTime])

    print 'Added Index to missing content'
  end
else
  begin
    print 'Missing content index already added to the table'
  end



