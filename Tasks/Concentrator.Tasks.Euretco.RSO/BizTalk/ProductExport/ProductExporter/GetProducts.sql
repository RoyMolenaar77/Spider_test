;
WITH  ProductNames
        AS ( SELECT p.ProductID
             ,      ProductName
             ,      ROW_NUMBER() OVER ( PARTITION BY p.ProductID ORDER BY ContentVendorIndex ) AS RowNumber
             FROM   dbo.Product p
             INNER JOIN dbo.ProductDescription pd ON p.ProductID = pd.ProductID
             INNER JOIN dbo.ContentVendorSetting cvs ON pd.VendorID = cvs.VendorID
             WHERE  ConnectorID = {0}
                    AND LanguageID = {1}
                    AND IsConfigurable = 1
                    AND ProductName IS NOT NULL
                    AND ProductName != ''
           ) ,
      ShortDescriptions
        AS ( SELECT p.ProductID
             ,      ShortcontentDescription
             ,      ROW_NUMBER() OVER ( PARTITION BY p.ProductID ORDER BY ContentVendorIndex ) AS RowNumber
             FROM   dbo.Product p
             INNER JOIN dbo.ProductDescription pd ON p.ProductID = pd.ProductID
             INNER JOIN dbo.ContentVendorSetting cvs ON pd.VendorID = cvs.VendorID
             WHERE  ConnectorID = {0}
                    AND LanguageID = {1}
                    AND IsConfigurable = 1
                    AND ShortcontentDescription IS NOT NULL
                    AND ShortcontentDescription != ''
           ) ,
      LongDescriptions
        AS ( SELECT p.ProductID
             ,      LongContentDescription
             ,      ROW_NUMBER() OVER ( PARTITION BY p.ProductID ORDER BY ContentVendorIndex ) AS RowNumber
             FROM   dbo.Product p
             INNER JOIN dbo.ProductDescription pd ON p.ProductID = pd.ProductID
             INNER JOIN dbo.ContentVendorSetting cvs ON pd.VendorID = cvs.VendorID
             WHERE  ConnectorID = {0}
                    AND LanguageID = {1}
                    AND IsConfigurable = 1
                    AND LongContentDescription IS NOT NULL
                    AND LongContentDescription != ''
           ) ,
      ShortSummaryDescriptions
        AS ( SELECT p.ProductID
             ,      ShortSummaryDescription
             ,      ROW_NUMBER() OVER ( PARTITION BY p.ProductID ORDER BY ContentVendorIndex ) AS RowNumber
             FROM   dbo.Product p
             INNER JOIN dbo.ProductDescription pd ON p.ProductID = pd.ProductID
             INNER JOIN dbo.ContentVendorSetting cvs ON pd.VendorID = cvs.VendorID
             WHERE  ConnectorID = {0}
                    AND LanguageID = {1}
                    AND IsConfigurable = 1
                    AND ShortSummaryDescription IS NOT NULL
                    AND ShortSummaryDescription != ''
           ) ,
      LongSummaryDescriptions
        AS ( SELECT p.ProductID
             ,      LongSummaryDescription
             ,      ROW_NUMBER() OVER ( PARTITION BY p.ProductID ORDER BY ContentVendorIndex ) AS RowNumber
             FROM   dbo.Product p
             INNER JOIN dbo.ProductDescription pd ON p.ProductID = pd.ProductID
             INNER JOIN dbo.ContentVendorSetting cvs ON pd.VendorID = cvs.VendorID
             WHERE  ConnectorID = {0}
                    AND LanguageID = {1}
                    AND IsConfigurable = 1
                    AND LongSummaryDescription IS NOT NULL
                    AND LongSummaryDescription != ''
           )
  SELECT  p.ProductID
	, 		  p.VendorItemnumber
	,       p.IsConfigurable
  ,       Name as BrandName
  ,       pn.ProductName
  ,       sd.ShortContentDescription
  ,       ld.LongContentDescription
  ,       ssd.ShortSummaryDescription
  ,       lsd.LongSummaryDescription
  ,       IsActive
  ,       Price
  FROM    dbo.Product p
  LEFT JOIN ProductNames pn ON p.ProductID = pn.ProductID
                               AND pn.RowNumber = 1
  LEFT JOIN ShortDescriptions sd ON p.ProductID = sd.ProductID
                                    AND sd.RowNumber = 1
  LEFT JOIN LongDescriptions ld ON p.ProductID = ld.ProductID
                                   AND ld.RowNumber = 1
  LEFT JOIN ShortSummaryDescriptions ssd ON p.ProductID = ssd.ProductID
                                            AND ssd.RowNumber = 1
  LEFT JOIN LongSummaryDescriptions lsd ON p.ProductID = lsd.ProductID
                                           AND lsd.RowNumber = 1
  INNER JOIN dbo.VendorAssortment va ON p.ProductID = va.ProductID
  INNER JOIN dbo.VendorPrice vp ON va.VendorAssortmentID = vp.VendorAssortmentID
                                   AND VendorID = {2}
  INNER JOIN dbo.Brand b ON p.BrandID = b.BrandID
  WHERE   IsConfigurable = 1
          AND p.ProductID IN ( {3} )