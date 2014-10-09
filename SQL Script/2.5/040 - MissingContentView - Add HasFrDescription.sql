
ALTER view [dbo].[MissingContentView] as

WITH Images AS
(
      Select COUNT(MediaPath) as ImageCount,pm.ProductID from ProductMedia pm
			inner join MediaType mt on pm.TypeID = mt.TypeID
			where mt.Type = 'image' and MediaPath is not null
			group by pm.ProductID
),Media AS
(
      Select COUNT(MediaUrl) as MediaCount,pm.ProductID from ProductMedia pm
			inner join MediaType mt on pm.TypeID = mt.TypeID
			where mt.Type = 'youtube' and MediaPath is not null
			group by pm.ProductID
), Specifications as 
(
Select COUNT(pav.AttributeValueID) as AttributeCount, pav.ProductID, cvs.ConnectorID from ProductAttributeMetaData pamd
			inner join ProductAttributeValue pav on pamd.AttributeID = pav.AttributeID
			inner join ContentVendorSetting cvs on pamd.VendorID = cvs.VendorID
			group by pav.ProductID,cvs.ConnectorID
),  
ProductBarcodeList as 
(
Select top 1 Barcode, productid from ProductBarcode
),
ConfigurationStock as (

select 
			sum(vs.quantityonhand) as productQuantity, sum(vrs.quantityonhand) as configuratbleQuantity, p.ProductID, vs.vendorid, p.IsConfigurable 
			from product p
			left join vendorstock vs on p.ProductID = vs.productid and vs.VendorStockTypeID = 1
			left join relatedproduct rp on p.ProductID = rp.ProductID and vs.vendorid = rp.vendorid
			left join vendorstock vrs on rp.relatedproductid = vrs.productid  and vrs.VendorStockTypeID = 1 and vs.vendorid = vrs.vendorid
			
			group by p.ProductID, vs.vendorid, p.IsConfigurable			
		)

			
select
distinct   
c.ConnectorID,
va.IsActive as Active,
p.ProductID as ConcentratorProductID,
p.VendorItemNumber,
va.CustomItemNumber,
b.Name as brandName,
b.BrandID,
c.ShortDescription,
cast(isnull(i.ImageCount,0) as bit) as [Image],    -- + veel secs
cast(isnull(m.MediaCount, 0) as bit) as youtube,
cast(isnull(s.AttributeCount,0) as bit) as Specifications,	-- +10 secs		
pb.Barcode AS barcode,
c.CreationTime,
c.LastModificationTime
,p.IsConfigurable,
case when pd.ProductID is null then 0 else 1 end as HasDescription,
case when pd.ProductID is null and pd.LanguageID = 3 then 0 else 1 end as HasFrDescription,
case when p.isConfigurable = 1
		then cs.configuratbleQuantity else cs.productQuantity
			end as 
			QuantityOnHand
,va.IsActive

from Content c
left join ContentProduct cp on cp.ProductContentID = c.ProductContentID
left join VendorAssortment va on cp.VendorID = va.VendorID and va.ProductID = c.ProductID
inner join Product p on c.ProductID = p.ProductID
inner join Brand b on p.BrandID = b.BrandID
left join Images i on i.productid = c.ProductID
left join Media m on m.productid = c.ProductID
left join Specifications s on s.ConnectorID = c.ConnectorID and s.ProductID = c.ProductID
left join ProductBarcodeList pb on pb.productid = c.ProductID
left join productdescription pd on pd.productid = p.productid and pd.vendorid = 48
left join ConfigurationStock cs on cs.VendorID = cp.VendorID and cs.ProductID = p.ProductID

GO


