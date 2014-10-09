
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
), ContentVendor as 
(
	select max(v.name) as contentVendorName, MAX(c.vendorid) as contentVendorID ,c.productid, cvs.ConnectorID from productdescription c
	inner join contentvendorsetting cvs on c.VendorID = cvs.VendorID
	inner join vendor v on v.vendorid = cvs.vendorid
	group by cvs.ConnectorID, c.ProductID 
), ProductBarcodeList as 
(
Select top 1 Barcode, productid from ProductBarcode
)
select
distinct   
c.ConnectorID,
cast(case when cpg.ProductID is null then 0 else 1 end as bit )as Active,
p.ProductID as ConcentratorProductID,
p.VendorItemNumber,
va.CustomItemNumber,
b.Name as brandName,
b.BrandID,
c.ShortDescription,
cast(isnull(i.ImageCount,0) as bit) as [Image],
cast(isnull(m.MediaCount, 0) as bit) as youtube,
cast(isnull(s.AttributeCount,0) as bit) as Specifications,
ca.contentVendorName as ContentVendor,		
ca.contentVendorID as ContentVendorID,			
pb.Barcode AS barcode,
pm.ProductGroupID,
c.CreationTime,
c.LastModificationTime,
p.IsConfigurable,
case when pd.ProductID is null then 0 else 1 end as HasDescription,
case when p.isConfigurable = 1
		then 
			(select sum(vs.quantityonhand) from relatedproduct rp
			inner join vendorstock vs on rp.relatedproductid = vs.productid and vs.vendorid = rp.vendorid and vs.VendorStockTypeID = 1
			where vs.vendorid = 1 and rp.productid = p.productid) 
	
		else  
			(select quantityonhand from vendorstock where productid = p.productid and vendorid = cp.vendorid and VendorStockTypeID = 1)
			end as 
			QuantityOnHand,
			va.IsActive

from Content c
left join ContentProductGroup cpg on cpg.ConnectorID = c.ConnectorID and cpg.ProductID = c.ProductID
left join ContentProduct cp on cp.ProductContentID = c.ProductContentID
left join ProductGroupMapping pm on cpg.ProductGroupMappingID = pm.ProductGroupMappingID
left join VendorAssortment va on cp.VendorID = va.VendorID and va.ProductID = c.ProductID
inner join Product p on c.ProductID = p.ProductID
inner join Brand b on p.BrandID = b.BrandID
left join Images i on i.productid = c.ProductID
left join Media m on m.productid = c.ProductID
left join Specifications s on s.ConnectorID = c.ConnectorID and s.ProductID = c.ProductID
left join ContentVendor ca on ca.ConnectorID = c.ConnectorID and ca.ProductID = c.ProductID
left join ProductBarcodeList pb on pb.productid = c.ProductID
left join productdescription pd on pd.productid = p.productid and pd.vendorid = 48

