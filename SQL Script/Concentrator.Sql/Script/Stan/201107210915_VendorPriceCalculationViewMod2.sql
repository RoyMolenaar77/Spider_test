
ALTER view [dbo].[VendorPriceCalculationView] as
 with 
	
	--select existing rules
	Rules as (
		select * from VendorPriceRule
	
	),	
	
	--- select product, product group and brand information related to vendor
	Catalogue as (
		select va.ProductID, pgv.ProductGroupID, p.BrandID, va.VendorID, va.vendorassortmentid
		from VendorAssortment va 
		inner join VendorProductGroupAssortment vpga on va.vendorassortmentid = vpga.vendorassortmentid
		inner join ProductGroupVendor pgv on pgv.ProductGroupVendorID = vpga.ProductGroupVendorID
		inner join ProductGroup pg on pg.ProductGroupID = pgv.ProductGroupID
		inner join product p on p.ProductID = va.ProductID
		where pgv.ProductGroupID != -1 --and va.vendorassortmentid = 2318240
	),	
	
	--rule assign weights to rules
	RuleHierarchy as (
		SELECT  VP.VendorAssortmentID,
		
		--assign weight
		CASE
			WHEN R.ProductID = C.ProductID THEN 3
			WHEN R.BrandID = C.BrandID THEN 2
			WHEN R.ProductGroupID= C.ProductGroupID THEN 1
			ELSE 0
		END AS MatchNumber
		,
		Rank() OVER (PARTITION BY VP.VendorAssortmentID 
		
		ORDER BY CASE 
			WHEN R.ProductID = C.ProductID THEN 3			
			WHEN R.ProductGroupID= C.ProductGroupID THEN 1
			WHEN R.BrandID = C.BrandID THEN 2
			ELSE NULL 
		END DESC ) AS RankNumber,
		R.*
		
		FROM VendorPrice VP 
		inner join Catalogue C on C.VendorAssortmentID = VP.VendorAssortmentID
		left join Rules R on (R.ProductID = C.ProductID OR R.ProductGroupID = C.ProductGroupID OR R.BrandID = C.BrandID)
		where R.ProductID is not null or R.BrandID is not null or R.ProductGroupID is not null
		
	),
	
	-- get only the most concrete rule
	RulesFiltered as (
		SELECT distinct * FROM RuleHierarchy
		where RankNumber = 1 
		
	),
	
	-- rank over indexes. Second level of filtering
	RankOrderRuleIndex as (
		SELECT 
		RF.* ,
		RANK() OVER (PARTITION BY RF.VendorAssortmentID
		ORDER BY RF.VendorPriceRuleIndex, RF.VendorPriceRuleID) as VendorPriceRuleIndexRank
		FROM RulesFiltered RF	
	),
	
	--select existing prices	
	Prices as (
		select  
		
		price.VendorAssortmentID,
		va.VendorID,
		
		--calculate price
		case when r.Margin is null then price.BasePrice else
			case when r.margin='%' and r.unitpriceincrease is not null and price.baseprice is null and r.priceruletype = 2 then price.basecostprice * r.unitpriceincrease else
				case when r.margin = '%' and r.unitpriceincrease is not null and price.baseprice is not null then price.baseprice * r.unitpriceincrease else
					case when r.margin='+' and r.unitpriceincrease is not null and price.baseprice is null and r.priceruletype = 2 then price.basecostprice + r.unitpriceincrease else
						case when r.margin = '+' and r.unitpriceincrease is not null and price.baseprice is not null then price.baseprice + r.unitpriceincrease
						end
					end
				end
			end			
		end as Price,
		
		--calculate cost price
		case when r.Margin is null then price.BaseCostPrice else
			case when r.margin = '%' and r.costpriceincrease is not null and price.basecostprice is null and r.priceruletype = 1 then price.BasePrice * r.CostPriceIncrease else
				case when r.margin = '%' and r.costpriceincrease is not null and price.basecostprice is not null then price.BaseCostPrice * r.CostPriceIncrease else
					case when r.Margin = '+' and r.CostPriceIncrease is not null and price.BaseCostPrice is null and r.priceruletype = 1 then price.baseprice + r.CostPriceIncrease else
						case when r.Margin = '+' and r.CostPriceIncrease is not null and price.BaseCostPrice is not null then price.basecostprice + r.CostPriceIncrease 
						end
					end
				end	
			end
		end as CostPrice,
		
		price.taxrate,
		price.minimumquantity,
		price.commercialstatus,
		price.concentratorstatusid,
		price.baseprice,
		price.basecostprice,
		r.VendorPriceRuleID,
		r.unitpriceincrease,
		r.costpriceincrease,
		r.ProductID as pid,
		r.productgroupid as pgid,
		r.brandid as bid,
		r.margin
		
		
		from VendorPrice price
		
		inner join vendorassortment va on va.vendorassortmentid = price.vendorassortmentid
		left join RankOrderRuleIndex R on R.VendorAssortmentID = price.VendorAssortmentID 
		where R.VendorPriceRuleIndexRank = 1
		)
		
		
		select * from Prices  --where vendorassortmentid = 2318240  
GO


