SELECT  p.ProductID
,       AttributeID
,       Value AS AttributeValue
FROM    dbo.Product p
INNER JOIN dbo.VendorAssortment va ON p.ProductID = va.ProductID
INNER JOIN dbo.ProductAttributeValue pav ON va.ProductID = pav.ProductID
WHERE   VendorID = {0}
        AND LanguageID = {1}