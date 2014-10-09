
ALTER VIEW [dbo].[ProductBarcodeView] as 

select pb.Barcode, c.ConnectorID, c.ProductID, pb.BarcodeType
from content c
inner join productbarcode pb on c.productid = pb.productid

GO


