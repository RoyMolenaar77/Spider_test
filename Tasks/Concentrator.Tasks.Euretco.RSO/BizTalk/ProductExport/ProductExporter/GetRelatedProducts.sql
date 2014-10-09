SELECT  rp.ProductID AS ConfigurableProductID
,       rp.RelatedProductID AS SimpleProductID
,       RelatedProductTypeID
,       Barcode
,       Price
,       va.IsActive
FROM    dbo.Product p 
INNER JOIN dbo.RelatedProduct rp ON p.ProductID = rp.ProductID
INNER JOIN dbo.ProductBarcode pb ON rp.RelatedProductID = pb.ProductID
INNER JOIN dbo.VendorAssortment va ON rp.RelatedProductID = va.ProductID
INNER JOIN dbo.VendorPrice vp ON va.VendorAssortmentID = vp.VendorAssortmentID
WHERE    va.VendorID = {0}
         AND p.ProductID IN ({1} )