
/****** Object:  View [dbo].[ImageView]    Script Date: 06/30/2011 09:01:31 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





ALTER VIEW [dbo].[ImageView] AS

select  
p.BrandID,
b.name,
c.ProductID,
p.VendorItemNumber as ManufacturerID,
im.MediaPath as ImagePath,
im.MediaUrl as ImageUrl,
cvs.ContentVendorSettingID,
--im.Sequence as Sequence,
c.ConnectorID,
CAST((row_number() over (PARTITION BY c.productid,c.connectorid
ORDER BY c.productid,c.connectorid,cvs.ContentVendorIndex,im.sequence) - 1) as INT) as Sequence,
ImageType = 'Product'
from Content c
inner join Product p on c.ProductID = p.ProductID
inner join Brand b on p.brandid = b.brandid
inner join ContentProductGroup cpg on c.ProductID = cpg.ProductID and c.ConnectorID = cpg.ConnectorID
inner join dbo.ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID 
--inner join PreferredConnectorVendor pcs on pcs.ConnectorID = c.ConnectorID and cvs.VendorID = pcs.VendorID
inner join ProductMedia im on c.ProductID = im.ProductID and cvs.VendorID = im.VendorID
left join ProductGroupMapping pgm on pgm.ConnectorID = c.ConnectorID and cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
where MediaPath is not null
--and pcs.isContentVisible = 1
and (cvs.BrandID is null or (p.BrandID = cvs.BrandID))
and (cvs.ProductGroupID is null or (cvs.ProductGroupID = pgm.ProductGroupID))
and (cvs.ProductID is null or (cvs.ProductID = c.ProductID))
group by 
c.ConnectorID,
p.BrandID,
c.ProductID,
p.VendorItemNumber,
im.MediaPath,
im.MediaUrl,
cvs.ContentVendorSettingID,
im.Sequence,
cvs.ContentVendorIndex,
b.name
union all
select  
p.BrandID,
b.name,
c.ProductID,
p.VendorItemNumber as ManufacturerID,
im.MediaPath as ImagePath,
im.MediaUrl as ImageUrl,
cvs.ContentVendorSettingID,
--im.Sequence as Sequence,
c.ConnectorID,
CAST((row_number() over (PARTITION BY c.productid,c.connectorid
ORDER BY c.productid,c.connectorid,cvs.ContentVendorIndex,im.sequence) - 1) as INT) as Sequence,
ImageType = 'Product'
from Content c
inner join ProductMatch pm  on (pm.productid = c.productid and pm.isMatched = 1)
inner join ProductMatch pm2 on pm2.ProductMatchID = pm.ProductMatchID and pm.ProductID != pm2.ProductID and pm2.isMatched = 1
inner join Product p on pm2.ProductID = p.ProductID
inner join brand b on p.brandid = b.brandid
inner join ContentProductGroup cpg on c.ProductID = cpg.ProductID and c.ConnectorID = cpg.ConnectorID
inner join dbo.ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID 
--inner join PreferredConnectorVendor pcs on pcs.ConnectorID = c.ConnectorID and cvs.VendorID = pcs.VendorID
inner join ProductMedia im on pm2.ProductID = im.ProductID and cvs.VendorID = im.VendorID
left join ProductGroupMapping pgm on pgm.ConnectorID = c.ConnectorID and cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
where MediaPath is not null
--and pcs.isContentVisible = 1
and (cvs.BrandID is null or (p.BrandID = cvs.BrandID))
and (cvs.ProductGroupID is null or (cvs.ProductGroupID = pgm.ProductGroupID))
and (cvs.ProductID is null or (cvs.ProductID = c.ProductID))
group by 
c.ConnectorID,
p.BrandID,
c.ProductID,
p.VendorItemNumber,
im.MediaPath,
im.MediaUrl,
cvs.ContentVendorSettingID,
im.Sequence,
cvs.ContentVendorIndex,
b.name
union all
select distinct 
p.BrandID,
b.name,
null as ProductID,
null as ManufacturerID,
b.ImagePath,
bv.Logo,
0 as ContentVendorSettingID,
--im.Sequence as Sequence,
c.ConnectorID,
0 as Sequence,
ImageType = 'Brand'
from Content c
inner join Product p on c.ProductID = p.ProductID
inner join Brand b on b.BrandID = p.BrandID
inner join BrandVendor bv on bv.BrandID = b.BrandID
where b.ImagePath is not null
and bv.Logo is not null



GO


