SELECT  p.ProductID
,       VendorItemNumber
,       ProductName
,       ShortContentDescription
,       Name AS BrandName
,       CostPrice
,       Price
FROM    dbo.Product p
INNER JOIN dbo.Brand b ON p.BrandID = b.BrandID
INNER JOIN dbo.VendorAssortment va ON p.ProductID = va.ProductID
INNER JOIN dbo.VendorPrice vp ON va.VendorAssortmentID = vp.VendorAssortmentID
LEFT JOIN dbo.ProductDescription pd ON p.ProductID = pd.ProductID
WHERE   va.VendorID = {0}
				AND pd.VendorID = 18
				AND LanguageID = {1}
        AND IsConfigurable = 1






