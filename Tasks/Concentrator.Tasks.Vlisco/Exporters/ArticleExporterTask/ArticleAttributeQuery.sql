SELECT DISTINCT
  [PT].[Value]          AS [ProductID],
  [PA].[AttributeCode]  AS [AttributeCode],
  [PAV].[AttributeID]   AS [AttributeID],
  [PAV].[Value]         AS [AttributeValue]
FROM    @0                                AS [PT]
JOIN    [dbo].[RelatedProduct]            AS [RP]     ON  [PT].[Value] = [RP].[RelatedProductID] or [PT].[Value] = [RP].[ProductID]
JOIN    [dbo].[RelatedProductType]        AS [RPT]    ON  [RP].[RelatedProductTypeID] = [RPT].[RelatedProductTypeID]
JOIN    [dbo].[Product]                   AS [P]      ON  [RP].[ProductID] = [P].[ProductID] AND [RP].[IsConfigured] = [P].[IsConfigurable] AND [RPT].[IsConfigured] = [P].[IsConfigurable]
JOIN    [dbo].[ProductAttributeValue]     AS [PAV]    ON  [PT].[Value] = [PAV].[ProductID] OR [P].[ProductID] = [PAV].[ProductID]
JOIN    [dbo].[ProductAttributeMetaData]  AS [PA]     ON  [PAV].[AttributeID] = [PA].[AttributeID]
WHERE   [PA].[VendorID] = @1
  AND   [RP].[VendorID] = @1