SELECT 
  [O].[ConnectorID],
  [O].[ReceivedDate]  AS [OrderDate],
  [O].[ReceivedDate]  AS [OrderTime],
  CASE [O].[OrderType]
    WHEN 1 THEN 'V' 
    WHEN 5 THEN 'R'
  END AS [SaleType],
  [O].[WebSiteOrderNumber]  AS [Ticket],
  ROW_NUMBER() OVER (PARTITION BY [O].[OrderID] ORDER BY [OL].[OrderLineID])  AS [Line],
  [P].[VendorItemNumber]    AS [ArticleCode],
  (
    SELECT  [Value]
    FROM    [dbo].[ProductAttributeMetaData]  AS [PA]
    JOIN    [dbo].[ProductAttributeValue]     AS [PAV]  ON [PA].[AttributeID] = [PAV].[AttributeID] AND [PAV].[ProductID] = [OL].[ProductID]
    WHERE   [PA].[AttributeCode] = @ColorAttributeCode
      AND   [PA].[VendorID] = [OL].[DispatchedToVendorID]      
  ) AS [ColorCode],
  (
    SELECT  [Value]
    FROM    [dbo].[ProductAttributeMetaData]  AS [PA]
    JOIN    [dbo].[ProductAttributeValue]     AS [PAV]  ON [PA].[AttributeID] = [PAV].[AttributeID] AND [PAV].[ProductID] = [OL].[ProductID]
    WHERE   [PA].[AttributeCode] = @SizeAttributeCode
      AND   [PA].[VendorID] = [OL].[DispatchedToVendorID]
  ) AS [SizeCode],
  [OL].[Quantity]                             AS [Quantity],
  [OL].[BasePrice]                            AS [PurchasePrice],
  [OL].[UnitPrice]                            AS [SalePrice],
  [OL].[Price]                                AS [NettoPrice],
  (
    [OL].[Price] - [OL].[TaxRate]
  )                                           AS [BrutoPrice],
  [OL].[TaxRate]                              AS [VAT],
  [OL].[LineDiscount]                         AS [DiscountPercentage],
  (
    [OL].[Price] * [OL].[LineDiscount] / 100
  )                                           AS [DiscountValue],
  [OL].[WareHouseCode]                        AS [ShopCode],
  [O].[BSKIdentifier]                         AS [SalesPerson],
  [C].[ServicePointID]                        AS [Client]
FROM      [dbo].[Order]                     AS [O]
JOIN      [dbo].[OrderLine]                 AS [OL]     ON [O].[OrderID] = [OL].[OrderID]
LEFT JOIN [dbo].[Customer]                  AS [C]      ON [O].[SoldToCustomerID] = [C].[CustomerID]
JOIN      [dbo].[RelatedProduct]            AS [RP]     ON [OL].[ProductID] = [RP].[RelatedProductID] AND [RP].[VendorID] = [OL].[DispatchedToVendorID]
JOIN      [dbo].[Product]                   AS [P]      ON [RP].[ProductID] = [P].[ProductID]
WHERE     [RP].[RelatedProductTypeID] = @RelatedProductTypeID