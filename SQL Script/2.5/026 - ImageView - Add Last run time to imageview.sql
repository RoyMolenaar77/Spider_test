
ALTER view [dbo].[ImageView] as
select  
	im.mediaid,
	p.BrandID,
	b.name,
	c.ProductID,
	p.VendorItemNumber as ManufacturerID,
	im.MediaPath as ImagePath,
	im.Description as [Description],
	isnull(im.MediaUrl,im.MediaPath) as ImageUrl,
	cvs.ContentVendorSettingID,
	c.ConnectorID,
	im.IsThumbNailImage,
	CAST((row_number() over (PARTITION BY c.productid,c.connectorid
	ORDER BY c.productid,c.connectorid,cvs.ContentVendorIndex,im.sequence) - 1) as INT) as Sequence,
	ImageType = mt.Type,
	im.LastChanged
from Content c
inner join Product p on c.ProductID = p.ProductID
inner join Brand b on p.brandid = b.brandid
left join ContentProductGroup cpg on c.ProductID = cpg.ProductID and c.ConnectorID = cpg.ConnectorID
inner join dbo.ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID 
--inner join PreferredConnectorVendor pcs on pcs.ConnectorID = c.ConnectorID and cvs.VendorID = pcs.VendorID
inner join ProductMedia im on c.ProductID = im.ProductID and cvs.VendorID = im.VendorID
inner join MediaType mt ON im.TypeID = mt.TypeID
left join ProductGroupMapping pgm on pgm.ConnectorID = c.ConnectorID and cpg.ProductGroupMappingID = pgm.ProductGroupMappingID

where MediaPath is not null
--and pcs.isContentVisible = 1
and (cvs.BrandID is null or (p.BrandID = cvs.BrandID))
and (cvs.ProductGroupID is null or (cvs.ProductGroupID = pgm.ProductGroupID))
and (cvs.ProductID is null or (cvs.ProductID = c.ProductID))
group by 
im.mediaID,
c.ConnectorID,
p.BrandID,
im.Description,
c.ProductID,
p.VendorItemNumber,
im.MediaPath,
im.MediaUrl,
cvs.ContentVendorSettingID,
im.Sequence,
cvs.ContentVendorIndex,
b.name,
im.IsThumbNailImage,
mt.type,
im.LastChanged

union all

select  
im.MediaID,
p.BrandID,
b.name,
c.ProductID,
p.VendorItemNumber as ManufacturerID,
im.MediaPath as ImagePath,
im.Description as [Description],
im.MediaUrl as ImageUrl,
cvs.ContentVendorSettingID,
--im.Sequence as Sequence,
c.ConnectorID,
im.IsThumbNailImage,
CAST((row_number() over (PARTITION BY c.productid,c.connectorid
ORDER BY c.productid,c.connectorid,cvs.ContentVendorIndex,im.sequence) - 1) as INT) as Sequence,
ImageType = mt.Type,
im.LastChanged
from Content c
inner join ProductMatch pm  on (pm.productid = c.productid and pm.isMatched = 1)
inner join ProductMatch pm2 on pm2.ProductMatchID = pm.ProductMatchID and pm.ProductID != pm2.ProductID and pm2.isMatched = 1
inner join Product p on pm2.ProductID = p.ProductID
inner join brand b on p.brandid = b.brandid
left join ContentProductGroup cpg on c.ProductID = cpg.ProductID and c.ConnectorID = cpg.ConnectorID
inner join dbo.ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID 
--inner join PreferredConnectorVendor pcs on pcs.ConnectorID = c.ConnectorID and cvs.VendorID = pcs.VendorID
inner join ProductMedia im on pm2.ProductID = im.ProductID and cvs.VendorID = im.VendorID
inner join MediaType mt ON im.TypeID = mt.TypeID
left join ProductGroupMapping pgm on pgm.ConnectorID = c.ConnectorID and cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
where MediaPath is not null
--and pcs.isContentVisible = 1
and (cvs.BrandID is null or (p.BrandID = cvs.BrandID))
and (cvs.ProductGroupID is null or (cvs.ProductGroupID = pgm.ProductGroupID))
and (cvs.ProductID is null or (cvs.ProductID = c.ProductID))
group by 
im.mediaID,
c.ConnectorID,
p.BrandID,
im.Description,
c.ProductID,
p.VendorItemNumber,
im.MediaPath,
im.MediaUrl,
cvs.ContentVendorSettingID,
im.Sequence,
cvs.ContentVendorIndex,
b.name,
im.IsThumbNailImage,
mt.type,
im.LastChanged


GO


