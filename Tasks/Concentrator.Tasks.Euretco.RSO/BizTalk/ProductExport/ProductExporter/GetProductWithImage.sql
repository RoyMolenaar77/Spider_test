SELECT DISTINCT
        p.ProductID
FROM    dbo.Product p
INNER JOIN dbo.ProductMedia pm ON p.ProductID = pm.ProductID
INNER JOIN dbo.ContentVendorSetting cvs ON pm.VendorID = cvs.VendorID
WHERE   ConnectorID = {0}
        AND IsConfigurable = 1