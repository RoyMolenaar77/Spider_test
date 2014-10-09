-- =============================================
-- Author:		Dashti
-- Create date: 17-09-2013
-- Description:	Get Vendor Products For Master Group Mapping tab "Vendor Products"
-- =============================================

-- =============================================
-- Author:		Dashti
-- Modify date: 20-09-2013
-- Description:	Product Match logic
-- =============================================

-- =============================================
-- Author:		Andor
-- Modify date: 25-11-2013
-- Description:	Product Match logic changed, choose primary at ProductMatch
-- =============================================

create PROCEDURE [dbo].[sp_MasterGroupMapping_FetchProducts]
  @MasterGroupMappingID INT = NULL
, @IsBlocked BIT = NULL
, @ProductID NVARCHAR(100) = NULL
, @SearchTerm NVARCHAR(100) = NULL
, @IsProductMapped BIT = NULL
, @HasProductAnImage BIT = NULL
AS 
  BEGIN
    SET NOCOUNT ON;

    IF @MasterGroupMappingID IS NULL 
      BEGIN
        WITH  ActiveAssortment
                AS ( SELECT MIN(VendorAssortmentID) AS VendorAssortmentID
                     ,      COUNT(1) AS CountVendorItemNumbers
                     FROM   dbo.VendorAssortment va
                     WHERE  IsActive = 1
                     GROUP BY ProductID
                   ),
              ActiveProducts
                AS ( SELECT p.ProductID
                     ,      VendorItemNumber
                     ,      ShortDescription
                     ,      CountVendorItemNumbers
                     ,      p.IsBlocked
                     ,      NAME AS BrandName
                     FROM   dbo.Product p
                     INNER JOIN dbo.VendorAssortment va ON p.ProductID = va.ProductID
                     INNER JOIN dbo.Brand b ON p.BrandID = b.BrandID
                     INNER JOIN ActiveAssortment aa ON va.VendorAssortmentID = aa.VendorAssortmentID
                   ),
              CountProductImage
                AS ( SELECT COUNT(1) AS CountImages
                     ,      ProductID
                     FROM   dbo.ProductMedia AS p
                     WHERE  ( TypeID = 1 )
                     GROUP BY ProductID
                   ),
              CountMatches
                AS ( SELECT ProductMatchID
                     ,      COUNT(1) AS CountMatches
                     FROM   dbo.ProductMatch pm
                     GROUP BY ProductMatchID
                   ),
              PossibleMatches
                AS ( SELECT ProductID
                     ,      MatchPercentage
                     ,      CountMatches
                     FROM   dbo.ProductMatch pm2
                     INNER JOIN CountMatches cm ON pm2.ProductMatchID = cm.ProductMatchID
                   ),
              MatchedProducts
                AS ( SELECT ProductID
                     ,      ROW_NUMBER() OVER ( PARTITION BY ProductMatchID ORDER BY [Primary] DESC ) AS RowNumber
                     FROM   dbo.ProductMatch pm
                     WHERE  MatchStatus IN ( 1, 2 ) AND isMatched = 1
                   ),
              MasterGroupMappingProducts
                AS ( SELECT DISTINCT
                            ProductID
                     FROM   dbo.MasterGroupMapping m
                     INNER JOIN dbo.MasterGroupMappingProduct mp ON m.MasterGroupMappingID = mp.MasterGroupMappingID
                     WHERE  ConnectorID IS NULL
                            AND IsProductMapped = 1
                   )
          SELECT  ap.ProductID
          ,       VendorItemNumber
          ,       ShortDescription
          ,       CountVendorItemNumbers
          ,       IsBlocked
          ,       BrandName
          ,       CASE WHEN CountImages IS NULL THEN 0
                       ELSE 1
                  END AS HasProductAnImage
          ,       ISNULL(pm.MatchPercentage, 0) AS MatchPercentage
          ,       ISNULL(pm.CountMatches, 0) AS CountMatches
          ,       CASE WHEN mps.ProductID IS NULL THEN 0
                       ELSE 1
                  END AS IsProductMapped
          ,       0 AS IsApproved
          FROM    ActiveProducts ap
          LEFT JOIN CountProductImage cpi ON ap.ProductID = cpi.ProductID
          LEFT JOIN PossibleMatches pm ON ap.ProductID = pm.ProductID
          LEFT JOIN MasterGroupMappingProducts mps ON pm.ProductID = mps.ProductID
          WHERE   ap.ProductID NOT IN ( SELECT  ProductID
                                        FROM    MatchedProducts mp
                                        WHERE   RowNumber != 1 )
                  AND ( @IsBlocked IS NULL
                        OR IsBlocked = @IsBlocked
                      )
                  AND ( @ProductID IS NULL
                        OR ap.ProductID LIKE '%' + @ProductID + '%'
                      )
                  AND ( @SearchTerm IS NULL
                        OR ( ShortDescription LIKE '%' + @SearchTerm + '%'
                             OR BrandName LIKE '%' + @SearchTerm + '%'
                             OR VendorItemNumber LIKE '%' + @SearchTerm + '%'
                           )
                      )
                  AND ( @IsProductMapped IS NULL
                        OR @IsProductMapped = CASE WHEN mps.ProductID IS NULL
                                                   THEN 0
                                                   ELSE 1
                                              END
                      )
                  AND ( @HasProductAnImage IS NULL
                        OR @HasProductAnImage = CASE WHEN CountImages IS NULL
                                                     THEN 0
                                                     ELSE 1
                                                END
                      )
      END
    ELSE 
      BEGIN
        WITH  ActiveAssortment
                AS ( SELECT MIN(VendorAssortmentID) AS VendorAssortmentID
                     ,      COUNT(1) AS CountVendorItemNumbers
                     FROM   dbo.VendorAssortment va
                     WHERE  IsActive = 1
                     GROUP BY ProductID
                   ),
              ActiveProducts
                AS ( SELECT p.ProductID
                     ,      VendorItemNumber
                     ,      ShortDescription
                     ,      CountVendorItemNumbers
                     ,      p.IsBlocked
                     ,      NAME AS BrandName
					 ,      p.IsConfigurable
                     FROM   dbo.Product p
                     INNER JOIN dbo.VendorAssortment va ON p.ProductID = va.ProductID
                     INNER JOIN dbo.Brand b ON p.BrandID = b.BrandID
                     INNER JOIN ActiveAssortment aa ON va.VendorAssortmentID = aa.VendorAssortmentID
                   ),
              CountProductImage
                AS ( SELECT COUNT(1) AS CountImages
                     ,      ProductID
                     FROM   dbo.ProductMedia AS p
                     WHERE  ( TypeID = 1 )
                     GROUP BY ProductID
                   ),
              CountMatches
                AS ( SELECT ProductMatchID
                     ,      COUNT(1) AS CountMatches
                     FROM   dbo.ProductMatch pm
                     GROUP BY ProductMatchID
                   ),
              PossibleMatches
                AS ( SELECT ProductID
                     ,      MatchPercentage
                     ,      CountMatches
                     FROM   dbo.ProductMatch pm2
                     INNER JOIN CountMatches cm ON pm2.ProductMatchID = cm.ProductMatchID
                   ),
              MatchedProducts
                AS ( SELECT ProductID
                     ,      ROW_NUMBER() OVER ( PARTITION BY ProductMatchID ORDER BY [Primary] DESC ) AS RowNumber
                     FROM   dbo.ProductMatch pm
                     WHERE  MatchStatus IN ( 1, 2 ) AND isMatched = 1
                   ),
              MappedProductsToMasterGroupMapping
                AS ( SELECT ProductID
                     ,      IsApproved
                     FROM   dbo.MasterGroupMappingProduct mp
                     WHERE  MasterGroupMappingID = @MasterGroupMappingID
                            AND IsProductMapped = 1
                   )
          SELECT  ap.ProductID
          ,       VendorItemNumber
          ,       ShortDescription
          ,       CountVendorItemNumbers
          ,       IsBlocked
          ,       BrandName
          ,       CASE WHEN CountImages IS NULL THEN 0
                       ELSE 1
                  END AS HasProductAnImage
          ,       ISNULL(pm.MatchPercentage, 0) AS MatchPercentage
          ,       ISNULL(pm.CountMatches, 0) AS CountMatches
          ,       1 AS IsProductMapped
          ,       IsApproved
		  ,		  IsConfigurable
          FROM    ActiveProducts ap
          LEFT JOIN CountProductImage cpi ON ap.ProductID = cpi.ProductID
          LEFT JOIN PossibleMatches pm ON ap.ProductID = pm.ProductID
          INNER JOIN MappedProductsToMasterGroupMapping mpmgm ON ap.ProductID = mpmgm.ProductID
          WHERE   ap.ProductID NOT IN ( SELECT  ProductID
                                        FROM    MatchedProducts mp
                                        WHERE   RowNumber != 1 )
                  AND ( @IsBlocked IS NULL
                        OR IsBlocked = @IsBlocked
                      )
                  AND ( @ProductID IS NULL
                        OR ap.ProductID LIKE '%' + @ProductID + '%'
                      )
                  AND ( @SearchTerm IS NULL
                        OR ( ShortDescription LIKE '%' + @SearchTerm + '%'
                             OR BrandName LIKE '%' + @SearchTerm + '%'
                             OR VendorItemNumber LIKE '%' + @SearchTerm + '%'
                           )
                      )
                  AND ( @IsProductMapped IS NULL
                        OR @IsProductMapped = 1
                      )
                  AND ( @HasProductAnImage IS NULL
                        OR @HasProductAnImage = CASE WHEN CountImages IS NULL
                                                     THEN 0
                                                     ELSE 1
                                                END
                      )
      END
  END
		


GO


