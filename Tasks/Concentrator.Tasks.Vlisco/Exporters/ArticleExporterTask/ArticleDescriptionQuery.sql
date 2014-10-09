SELECT DISTINCT
  [PT].[Value]                    AS [ProductID],
  [PD].[LongContentDescription]   AS [DescriptionLong],
  [PD].[ShortContentDescription]  AS [DescriptionShort]
FROM    @0                          AS [PT]
JOIN    [dbo].[RelatedProduct]      AS [RP]   ON  [PT].[Value] = [RP].[RelatedProductID] or [PT].[Value] = [RP].[RelatedProductID]
JOIN    [dbo].[RelatedProductType]  AS [RPT]  ON  [RP].[RelatedProductTypeID] = [RPT].[RelatedProductTypeID]
JOIN    [dbo].[ProductDescription]  AS [PD]   ON  [RP].[ProductID] = [PD].[ProductID]
WHERE   [PD].[VendorID] = @1
  AND   [RP].[VendorID] = @1