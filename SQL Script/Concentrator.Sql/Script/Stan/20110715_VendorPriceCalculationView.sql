create view VendorPriceCalculationView as
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
		where pgv.ProductGroupID != -1 
	),	
	
	--select existing prices
	
	Prices as (
		select  
		c.VendorAssortmentID,
		c.ProductID,
		c.ProductGroupID,
		case when r.margin='%' and r.unitpriceincrease is not null and price.baseprice is null and r.priceruletype = 2 then price.basecostprice * r.unitpriceincrease else
			case when r.margin = '%' and r.unitpriceincrease is not null and price.baseprice is not null then price.baseprice * r.unitpriceincrease else
				case when r.margin='+' and r.unitpriceincrease is not null and price.baseprice is null and r.priceruletype = 2 then price.basecostprice + r.unitpriceincrease else
					case when r.margin = '+' and r.unitpriceincrease is not null and price.baseprice is not null then price.baseprice + r.unitpriceincrease
					end
				end
			end
			
		end as Price,
		case when r.margin = '%' and r.costpriceincrease is not null and price.basecostprice is null and r.priceruletype = 1 then price.BasePrice * r.CostPriceIncrease else
			case when r.margin = '%' and r.costpriceincrease is not null and price.basecostprice is not null then price.BaseCostPrice * r.CostPriceIncrease else
				case when r.Margin = '+' and r.CostPriceIncrease is not null and price.CostPrice is null and r.priceruletype = 1 then price.baseprice + r.CostPriceIncrease else
					case when r.Margin = '+' and r.CostPriceIncrease is not null and price.CostPrice is not null then price.basecostprice + r.CostPriceIncrease 
					end
				end
			end	
		end as CostPrice,
		
		price.taxrate,
		price.minimumquantity,
		price.commercialstatus,
		price.concentratorstatusid,
		price.baseprice,
		price.basecostprice
		from vendorprice price
		inner join catalogue c on price.vendorassortmentid = c.vendorassortmentid
		left join rules r on r.vendorid = c.VendorID and (
			(r.brandid is null or c.brandid = r.brandid) and
			r.ProductID is null and (r.ProductGroupID is null or r.ProductGroupID = c.ProductGroupID)) or
			(r.ProductID = c.ProductID and r.BrandID is null and r.ProductGroupID is null)
		) 
		
		select * from Prices p