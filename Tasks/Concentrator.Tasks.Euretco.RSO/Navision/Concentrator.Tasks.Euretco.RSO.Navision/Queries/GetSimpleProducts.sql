SELECT  rp.ProductID AS ConfigurableProductID
,       RelatedProductID AS SimpleProductID
,       VendorItemNumber AS SimpleVendorItemNumber
,       Barcode
FROM    dbo.VendorAssortment va
INNER JOIN dbo.Product p ON va.ProductID = p.ProductID
INNER JOIN dbo.RelatedProduct rp ON p.ProductID = rp.RelatedProductID
INNER JOIN dbo.ProductBarcode pb ON rp.RelatedProductID = pb.ProductID
WHERE   va.VendorID = {0}
				AND RelatedProductTypeID = {1}