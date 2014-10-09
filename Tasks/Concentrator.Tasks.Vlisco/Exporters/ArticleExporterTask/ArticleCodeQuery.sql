SELECT DISTINCT
  [P].[VendorItemNumber]  AS [ArticleCode],
  [P].[BrandID]           AS [BrandID],
  [RP].[ProductID]        AS [ParentProductID],
  [RP].[RelatedProductID] AS [ProductID]
FROM    @0                          AS [PT]
JOIN    [dbo].[RelatedProduct]      AS [RP]     ON  [PT].[Value] = [RP].[RelatedProductID] or [PT].[Value] = [RP].[ProductID]
JOIN    [dbo].[RelatedProductType]  AS [RPT]    ON  [RP].[RelatedProductTypeID] = [RPT].[RelatedProductTypeID]
JOIN    [dbo].[Product]             AS [P]      ON  [RP].[ProductID] = [P].[ProductID] AND [RP].[IsConfigured] = [P].[IsConfigurable] AND [RPT].[IsConfigured] = [P].[IsConfigurable]
WHERE   [RP].[VendorID] = @1