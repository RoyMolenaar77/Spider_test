SELECT DISTINCT
  [VA].[ProductID]    AS [ProductID],
  [VA].[VendorID]     AS [VendorID],
  [VP].[CostPrice]    AS [CostPrice],
  [VP].[Price]        AS [UnitPrice],
  [VP].[TaxRate]      AS [TaxRate],
  [Country].[Value]   AS [CountryCode],
  [Currency].[Value]  AS [CurrencyCode]
FROM  @0                        AS [VAT]
JOIN  [dbo].[VendorAssortment]  AS [VA]       ON  [VAT].[Value] = [VA].[VendorAssortmentID]
JOIN  [dbo].[VendorPrice]       AS [VP]       ON  [VA].[VendorAssortmentID] = [VP].[VendorAssortmentID] AND [VP].[MinimumQuantity] = 0
JOIN  [dbo].[VendorSetting]     AS [Country]  ON  [VA].[VendorID] = [Country].[VendorID] AND [Country].[SettingKey]  = @1
JOIN  [dbo].[VendorSetting]     AS [Currency] ON  [VA].[VendorID] = [Currency].[VendorID] AND [Currency].[SettingKey] = @2
JOIN  [dbo].[Product]           AS [P]        ON  [P].ProductID = [Va].ProductID 
WHERE P.IsConfigurable = 0