/****** Object:  View [dbo].[CalculatedPriceView]    Script Date: 07/26/2011 11:07:17 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO









ALTER VIEW [dbo].[CalculatedPriceView] AS

WITH OwnPrices
AS
(
      SELECT PCP.ProductID, AVG(PCP.Price) AS OwnPrice
      FROM
      ProductCompetitorMapping PCM
      INNER JOIN ProductCompetitorPrice PCP ON (PCP.ProductCompetitorMappingID = PCM.ProductCompetitorMappingID)
      WHERE PCM.Competitor = 'MyCom'
      GROUP BY PCP.ProductID
),
PriceSource AS
(
      SELECT PCM.Competitor, PCP.ProductID, PCP.Price
      FROM
      ProductCompetitorMapping PCM
      INNER JOIN ProductCompetitorPrice PCP ON (PCP.ProductCompetitorMappingID = PCM.ProductCompetitorMappingID)
),
preferedVendorPrices as(
	select pcv.ConnectorID,va.ProductID,va.vendorid,vp.*, pcv.isPreferred from VendorPrice vp
	inner join VendorAssortment va on vp.VendorAssortmentID = va.VendorAssortmentID
	left join PreferredConnectorVendor pcv on pcv.VendorID = va.VendorID
	where vp.Price is not null
), 
ConnectorProductGroups as (
select productID,productGroupID, cpg.connectorID 
from ContentProductGroup cpg 
inner join ProductGroupMapping pgm on cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
group by ProductID,ProductGroupID,cpg.ConnectorID
), 
ConnectorPriceRule as (
	 select VendorID,ConnectorID,ProductGroupID,BrandID,ProductID,Margin,UnitPriceIncrease,CostPriceIncrease,MinimumQuantity
	 , MIN(contentPriceRuleIndex) as contentPriceRuleIndex 
	 , fixedPrice from ContentPrice
	 group by VendorID,ConnectorID,ProductGroupID,BrandID,ProductID,Margin,UnitPriceIncrease,CostPriceIncrease,MinimumQuantity,fixedPrice
	),
prices as (
 select distinct
 c.productid as ProductID,
 case when cpr.FixedPrice is not null then cpr.FixedPrice else 
			case when cpr.Margin = '%' and cpr.UnitPriceIncrease is not null and vp.Price is not null then vp.Price * cpr.UnitPriceIncrease else
				case when cpr.Margin = '%' and cpr.CostPriceIncrease is not null and vp.CostPrice is not null then vp.CostPrice * cpr.CostPriceIncrease else
					case when cpr.Margin = '+' and cpr.UnitPriceIncrease is not null and vp.Price is not null then vp.Price + cpr.UnitPriceIncrease else
						case when cpr.Margin = '+' and cpr.CostPriceIncrease is not null and vp.CostPrice is not null then vp.CostPrice + cpr.CostPriceIncrease 
							else (Select top 1 pvp.price from preferedVendorPrices pvp 
								where pvp.ProductID = c.ProductID and pvp.ConnectorID = c.ConnectorID
								order by pvp.isPreferred desc
								)
						end 
					end
				end
			end
		end as PriceEx,
		vp.CostPrice,
		vp.TaxRate,
		cpr.ContentPriceRuleIndex,
		va.CustomItemNumber,
		c.ConnectorID,
		vp.ConcentratorStatusID,
		vp.CommercialStatus,
		vp.MinimumQuantity
  from content c
inner join Product p on c.ProductID = p.ProductID
inner join ContentProduct cp on cp.ProductContentID = c.ProductContentID
inner join VendorAssortment va on c.ProductID = va.ProductID and va.VendorID = cp.VendorID
inner join VendorPrice vp on va.VendorAssortmentID = vp.VendorAssortmentID
left join ConnectorProductGroups pgm on pgm.ConnectorID = c.ConnectorID and pgm.ProductID = c.ProductID
left join ConnectorPriceRule cpr on  va.VendorID = cpr.VendorID and cpr.ConnectorID = c.ConnectorID 
	and ((cpr.BrandID is null or cpr.BrandID = p.BrandID) and cpr.ProductID is null and (cpr.ProductGroupID is null or cpr.ProductGroupID = pgm.ProductGroupID)) 
	or (cpr.ProductID = c.ProductID and cpr.BrandID is null and cpr.ProductGroupID is null)
	where vp.MinimumQuantity <=1
)
select  
c.ProductID,
pr.PriceEx,
cast(pr.PriceEx * ((pr.taxrate / 100) + 1) as decimal(18,4)) as priceInc,
pr.CostPrice,
MIN(pcp.Price) as minPriceInc, 
MAX(pcp.price) as maxPriceInc, 
count(pcp.Competitor) as competitorcount,
ROW_NUMBER() OVER (PARTITION BY c.ConnectorID ,c.ProductID ORDER BY ContentPriceRuleIndex) AS RankNumber,
AVG(OP.OwnPrice) AS OwnPriceInc,  
AVG(PCP.Price) AS AverageMarketPriceInc,
(     SELECT COUNT(*)+1 FROM PriceSource PCP2 
            WHERE PCP2.ProductID = PCP.ProductID AND PCP2.Price <  AVG(OP.OwnPrice)
            AND PCP2.Competitor <> 'MyCom'
      ) AS CurrentRank,
c.connectorID,
pr.ConcentratorStatusID,
pr.CommercialStatus,
pr.TaxRate,
pr.MinimumQuantity
from content c
LEFT join prices pr ON  pr.productid = c.ProductID AND c.ConnectorID = pr.connectorID
left join  PriceSource PCP on (pcp.ProductID = c.ProductID)
      left JOIN OwnPrices OP ON (OP.ProductID = PCP.ProductID)
group by pr.ProductID,pr.ContentPriceRuleIndex,c.ConnectorID,pr.CostPrice, pcp.ProductID, pr.PriceEx, pr.TaxRate, c.productid
,pr.ConcentratorStatusID,pr.commercialstatus, pr.TaxRate, pr.MinimumQuantity









GO


