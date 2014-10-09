alter view AssortmentContentView as
WITH VendorStockSummary AS
(
			
      SELECT vs.VendorID, ProductID, MAX(QuantityOnHand) AS QuantityOnHand, -- SUM(..) --> MAX(..) i.v.m. ContentStock LEFT JOIN records
      MIN(PromisedDeliveryDate) AS PromisedDeliveryDate, MAX(QuantityToReceive) AS QuantityToReceive -- SUM(..) --> MAX(..) i.v.m. ContentStock LEFT JOIN records
      FROM VendorStock vs
	  left join ContentStock cs on vs.VendorID = cs.VendorID
	  and cs.VendorStockTypeID = vs.VendorStockTypeID
	  WHERE vs.VendorStockTypeID = 1
      GROUP BY vs.VendorID, ProductID      
), 

WebshopVendorStockSummary as 
(
	select vs.vendorid, productid, quantityonhand 
	from vendorstock vs
	left join ContentStock cs on vs.VendorID = cs.VendorID and cs.VendorStockTypeID = vs.VendorStockTypeID
	where vs.vendorstocktypeid = 1
),
BrandList AS
(                       SELECT BrandID, VendorID, MIN(VendorBrandCode) AS VendorBrandCode
												FROM BrandVendor
												GROUP BY BrandID,VendorID
),
CustomItemNumbers AS
(
SELECT ConnectorID, ProductID, VendorID, CustomItemNumber
FROM (
SELECT pcv.ConnectorID, VA.ProductID, pcv.VendorID, VA.CustomItemNumber, pcv.isPreferred, ROW_NUMBER ()over(partition by pcv.ConnectorID, VA.ProductID order by pcv.IsPreferred desc) AS RN
FROM VendorAssortment VA 
INNER JOIN PreferredConnectorVendor pcv ON (pcv.VendorID = VA.VendorID)
) a WHERE RN=1
),

ConcentratorDescriptionCount as (
	select distinct productid  from productdescription pd
	inner join vendor v on pd.vendorid = v.vendorid
	where v.name = 'Concentrator' and pd.languageid = 2
),

ImageCount as (
select productid, vendorid, count(productid) as imageCount
from productmedia
group by productid, vendorid
)

SELECT distinct
			C.ConnectorID,
			C.ProductID, 
		P.ParentProductID,
			--L.LanguageID,
			p.VendorItemNumber,
			P.BrandID,
			B.Name AS BrandName,
			BV.VendorBrandCode,
			C.ShortDescription,
			C.LongDescription,
			C.LineType,
			va.LedgerClass,
			va.ProductDesk,
			va.ExtendedCatalog,
			C.ProductContentID,
			CP.VendorID,
			V.CutOffTime,
			V.DeliveryHours,
			CI.CustomItemNumber,
			cps.ConnectorStatus,
			vs.PromisedDeliveryDate,
			vs.QuantityToReceive,
		 ISNULL(vs.QuantityOnHand,0) AS QuantityOnHand,
			cp.PublicationIndex as ProductContentIndex
			--PDL.ShortContentDescription,
			--PDL.LongContentDescription
			--,pdl.ProductName
			,null as ProductName,
      p.Visible,
			p.IsConfigurable,     
			P.IsNonAssortmentItem      
			FROM Content C 
						INNER JOIN connectorpublicationrule CP ON (CP.connectorpublicationruleid = C.connectorpublicationruleid)
						INNER JOIN Product P ON (P.ProductID = C.ProductID)
						INNER JOIN Brand B ON (P.BrandID = B.BrandID)
						INNER JOIN Vendor V on (CP.VendorID = V.VendorID)
						INNER JOIN VendorAssortment va on (cp.VendorID = va.VendorID and c.ProductID = va.ProductID)
						INNER JOIN VendorPrice vp on (va.VendorAssortmentID = vp.VendorAssortmentID)
						left join ContentStock cs on cs.ConnectorID = c.connectorid
						LEFT JOIN VendorStockSummary vs ON (vs.VendorID = isnull(cs.vendorid, va.VendorID) and vs.ProductID = va.ProductID)
						LEFT JOIN WebshopVendorStockSummary vsW on (vsW.VendorID = isnull(cs.vendorid, va.VendorID) and vsW.ProductID = va.ProductID)
						left join VendorStock vstock on vstock.vendorid = v.vendorid and vstock.productid = p.productid and vstock.vendorstocktypeid = 3 
						left join VendorStock vStockWebShop on vStockWebShop.vendorid = v.vendorid and vStockWebShop.productid = p.productid and vStockWebShop.vendorstocktypeid = 1 
			INNER JOIN CustomItemNumbers CI ON (CI.ConnectorID =C.ConnectorID AND CI.ProductID = va.ProductID)
						Left JOIN Connector CT ON (CT.ConnectorID = C.ConnectorID)
						
						left join ConnectorPublication cpc on CPc.ConnectorID = c.ConnectorID
			left join ConcentratorDescriptionCount pd on pd.productid = c.productid
						 left join RelatedProduct rp on rp.RelatedProductID = p.ProductID and rp.RelatedProductTypeID = 8
						left join Content cConfi on cConfi.ConnectorID = c.ConnectorID and cConfi.ProductID = rp.ProductID
						left join ConcentratorDescriptionCount pdConfi on rp.ProductID = pdConfi.ProductID
						left join ImageCount pm on pm.productid = c.productid and (cp.vendorid = pm.vendorid or pm.VendorID = v.ParentVendorID)
						left join ImageCount cpm on cpm.productid = rp.productid and (cpm.vendorid= cp.vendorid or cpm.VendorID = v.ParentVendorID)
						left join vendorsetting vsettings on vsettings.vendorid = v.vendorid and vsettings.settingkey = 'UsesWMSStockAsDefault'
						left join vendorsetting vsettingsWebshop on vsettingsWebshop.vendorid = v.vendorid and vsettingsWebshop.settingkey = 'UsesWebShopStockAsDefault'
						left JOIN BrandList BV ON (BV.BrandID = P.BrandID AND ((V.ParentVendorID is not null and BV.VendorID = v.ParentVendorID) or BV.VendorID = v.VendorID))
							 inner join Connector con on con.ConnectorID = c.ConnectorID
						inner JOIN ConnectorProductStatus cps on cps.ConcentratorStatusID = vp.ConcentratorStatusID and (cps.ConnectorID = c.ConnectorID or (con.ParentConnectorID is not null and cps.ConnectorID = con.ParentConnectorID))
		where 
		(p.IsNonAssortmentItem = 1 or 
		(
		(p.isconfigurable = 1 or (p.IsConfigurable = 0 and rp.ProductID is null) or(p.IsConfigurable = 0 and rp.ProductID is not null and cConfi.ProductID is not null))
		 and (ct.IgnoreMissingImage is null or (ct.IgnoreMissingImage = 1 and pm.imageCount > 0 and (rp.productid is null or (rp.productid is not null and cpm.imageCount > 0)))) and
		 ((ct.IgnoreMissingConcentratorDescription = 0 or
		 ((p.IsConfigurable = 1 and pd.ProductID is not null) or (p.IsConfigurable = 0 and pd.productid is not null and rp.RelatedProductID is null) or (p.IsConfigurable = 0 and  rp.RelatedProductID is not null and pdConfi.ProductID is not null))
		 )	 
		 ) and 
		 (vsettings.Value is null or ( vsettings.Value = 'TRUE' and ( vstock.QuantityOnHand > 0 or p.IsConfigurable = 1)))
		 and 
		 (vsettingsWebshop.Value is null or ( vsettingsWebshop.Value = 'TRUE' and ( vsW.QuantityOnHand > 0 or p.IsConfigurable = 1)))
		 ) )
		
		
		