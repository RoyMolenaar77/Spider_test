SELECT  P.ProductID
,       P.VendorItemNumber
,       P.IsConfigurable
,       pm.VendorID
,       pm.Sequence
,       pm.FileName
,       pm.MediaPath
,       pm.TypeID
FROM    ContentVendorSetting cvs
INNER JOIN ProductMedia pm ON pm.VendorID = cvs.VendorID
INNER JOIN Product p ON p.ProductID = pm.ProductID
INNER JOIN dbo.ContentProductGroup cpg ON p.ProductID = cpg.ProductID AND cpg.ConnectorID = @0
WHERE   cvs.ConnectorID = @0
        AND IsConfigurable = 1