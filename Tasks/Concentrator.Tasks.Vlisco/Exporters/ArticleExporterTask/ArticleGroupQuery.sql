SELECT DISTINCT
  [VA].[ProductID], 
  [CategoryID], 
  [CategoryCode], 
  [CategoryName], 
  [FamilyID],
  [FamilyCode], 
  [FamilyName], 
  [SubfamilyID],
  [SubfamilyCode], 
  [SubfamilyName]
FROM @0                       AS [PT]
JOIN [dbo].[VendorAssortment] AS [VA] ON [PT].[Value] = [VA].[ProductID]
LEFT JOIN
(
  SELECT DISTINCT
    [VA].[ProductID]                AS [ProductID],
    [PGV].[ProductGroupID]          AS [CategoryID],
    [PGV].[VendorProductGroupCode1] AS [CategoryCode],
    [PGV].[VendorName]              AS [CategoryName]
  FROM  [dbo].[VendorAssortment]             AS [VA]
  JOIN  [dbo].[VendorProductGroupAssortment] AS [VPGA] ON [VA].[VendorAssortmentID] = [VPGA].[VendorAssortmentID]
  JOIN  [dbo].[ProductGroupVendor]           AS [PGV]  ON [VPGA].[ProductGroupVendorID] = [PGV].[ProductGroupVendorID] AND [VA].[VendorID] = [PGV].[VendorID]
  WHERE [PGV].[VendorProductGroupCode1] IS NOT NULL
) AS [Category] ON [VA].[ProductID] = [Category].[ProductID]
LEFT JOIN
(
  SELECT DISTINCT
    [VA].[ProductID]                AS [ProductID],
    [PGV].[ProductGroupID]          AS [FamilyID],
    [PGV].[VendorProductGroupCode2] AS [FamilyCode],
    [PGV].[VendorName]              AS [FamilyName]
  FROM  [dbo].[VendorAssortment]             AS [VA]
  JOIN  [dbo].[VendorProductGroupAssortment] AS [VPGA] ON [VA].[VendorAssortmentID] = [VPGA].[VendorAssortmentID]
  JOIN  [dbo].[ProductGroupVendor]           AS [PGV]  ON [VPGA].[ProductGroupVendorID] = [PGV].[ProductGroupVendorID] AND [VA].[VendorID] = [PGV].[VendorID]
  WHERE [PGV].[VendorProductGroupCode2] IS NOT NULL
) AS [Family] ON [VA].[ProductID] = [Family].[ProductID]
LEFT JOIN
(
  SELECT DISTINCT
    [VA].[ProductID]                AS [ProductID],
    [PGV].[ProductGroupID]          AS [SubfamilyID],
    [PGV].[VendorProductGroupCode3] AS [SubfamilyCode],
    [PGV].[VendorName]              AS [SubfamilyName]
  FROM  [dbo].[VendorAssortment]             AS [VA]
  JOIN  [dbo].[VendorProductGroupAssortment] AS [VPGA] ON [VA].[VendorAssortmentID] = [VPGA].[VendorAssortmentID]
  JOIN  [dbo].[ProductGroupVendor]           AS [PGV]  ON [VPGA].[ProductGroupVendorID] = [PGV].[ProductGroupVendorID] AND [VA].[VendorID] = [PGV].[VendorID]
  WHERE [PGV].[VendorProductGroupCode3] IS NOT NULL
) AS [Subfamily] ON [VA].[ProductID] = [Subfamily].[ProductID]
WHERE [VA].[VendorID] = @1