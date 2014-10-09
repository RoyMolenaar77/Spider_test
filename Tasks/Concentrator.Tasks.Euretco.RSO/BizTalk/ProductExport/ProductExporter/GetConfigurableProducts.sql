SELECT m.MasterGroupMappingID, p.ProductID, IsConfigurable
FROM dbo.MasterGroupMapping m
INNER JOIN dbo.MasterGroupMappingProduct mp ON m.MasterGroupMappingID = mp.MasterGroupMappingID
INNER JOIN dbo.Product p ON mp.ProductID = p.ProductID
WHERE ConnectorID = {0} AND IsConfigurable = 1