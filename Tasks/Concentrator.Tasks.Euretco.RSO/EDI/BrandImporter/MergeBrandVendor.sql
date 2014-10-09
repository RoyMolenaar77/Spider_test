MERGE [dbo].[BrandVendor] AS T
USING 
(
  SELECT DISTINCT B.BrandID, B.Name
  FROM #Brand TempBrand
  INNER JOIN Brand B ON B.Name = TempBrand.Name
)AS S
ON    T.Name = S.Name
WHEN NOT MATCHED THEN
  INSERT (BrandID, VendorID, VendorBrandCode, Name)
  VALUES (S.BrandID, {0}, S.Name, S.Name);