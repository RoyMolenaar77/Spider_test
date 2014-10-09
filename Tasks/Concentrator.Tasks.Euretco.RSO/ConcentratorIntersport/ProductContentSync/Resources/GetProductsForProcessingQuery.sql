;WITH IntersportProductsWithMedia AS 
(
  SELECT P.ProductID, VendorItemNumber
  FROM {0}.[dbo].[Product] P
  INNER JOIN {0}.[dbo].[ProductMedia] PM ON P.ProductID = PM.ProductID
  GROUP BY P.ProductID, P.VendorItemNumber
),

RSOProductsWithoutMedia AS 
(
  SELECT ProductID, VendorItemNumber 
  FROM {1}.[dbo].[Product] 
  WHERE ProductID NOT IN (
                            SELECT ProductID 
                            FROM ProductMedia
                         )
)

SELECT P.ProductID
      , P.VendorItemNumber
      , P2.ProductID AS RSOProductID
      , P2.VendorItemNumber AS RSOVendorItemNumber
FROM IntersportProductsWithMedia P
INNER JOIN RSOProductsWithoutMedia P2 ON P.VendorItemNumber = P2.VendorItemNumber