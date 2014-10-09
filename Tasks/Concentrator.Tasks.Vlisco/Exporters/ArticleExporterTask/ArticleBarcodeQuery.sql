SELECT DISTINCT
  [PT].[Value]    AS [ProductID],
  [PB].[Barcode]  AS [Barcode]
FROM    @0                      AS [PT]
JOIN    [dbo].[ProductBarcode]  AS [PB] ON  [PT].[Value] = [PB].[ProductID]
WHERE   [PB].[VendorID] = @1 AND [PB].[Barcode] <> ''