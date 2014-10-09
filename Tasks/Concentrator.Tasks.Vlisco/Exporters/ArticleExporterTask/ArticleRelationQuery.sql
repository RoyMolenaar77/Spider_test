SELECT DISTINCT
  [PT].[Value]            AS [ProductID],
  [P].[ProductID]         AS [ParentProductID],
  [P].[VendorItemNumber]  AS [ArticleCode]
FROM    @0                          AS [PT]
JOIN    [dbo].[RelatedProduct]      AS [RP]     ON  [PT].[Value] = [RP].[RelatedProductID]
JOIN    [dbo].[RelatedProductType]  AS [RPT]    ON  [RP].[RelatedProductTypeID] = [RPT].[RelatedProductTypeID]
JOIN    [dbo].[Product]             AS [P]      ON  [RP].[ProductID] = [P].[ProductID] AND [RP].[IsConfigured] = [P].[IsConfigurable] AND [RPT].[IsConfigured] = [P].[IsConfigurable]
WHERE   [RP].[VendorID] = @1
  AND   [RPT].[Type] = @2