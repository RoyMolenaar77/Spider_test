create nonclustered index IDX_Content_ProductID 
ON Content (ConnectorID, ProductContentID) 
INCLUDE (ProductID)