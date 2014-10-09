SELECT  p.ProductID
,       AttributeCode
,       Value
,       Name
,       pamd.IsConfigurable
FROM    dbo.Product p
INNER JOIN dbo.ProductAttributeValue pav ON p.ProductID = pav.ProductID
INNER JOIN dbo.ProductAttributeMetaData pamd ON pav.AttributeID = pamd.AttributeID
INNER JOIN dbo.ProductAttributeName pan ON pamd.AttributeID = pan.AttributeID
WHERE   VendorID = {0}
        AND IsVisible = 1
        AND pan.LanguageID = {1}
        AND p.ProductID IN ({2} )