MERGE dbo.ProductMedia AS T
USING 
(
  SELECT  Value AS ProductID, @1 AS VendorID, @2 AS TypeID, @3 AS MediaPath, @4 AS Sequence, @5 AS MediaFileName
  FROM    @0 -- Integer-table
) AS S
ON T.ProductID = S.ProductID AND T.VendorID = S.VendorID AND T.TypeID = S.TypeID AND T.Sequence = S.Sequence
WHEN MATCHED AND T.MediaPath <> S.MediaPath THEN
  UPDATE SET MediaPath = S.MediaPath, FileName = S.MediaFileName
WHEN NOT MATCHED THEN
  INSERT (ProductID, VendorID, TypeID, MediaPath, Sequence, FileName)
  VALUES (ProductID, VendorID, TypeID, MediaPath, Sequence, MediaFileName);