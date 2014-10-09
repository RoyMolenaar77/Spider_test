/****** Object:  View [dbo].[AssortmentContentView]    Script Date: 02/10/2011 11:53:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





CREATE VIEW [dbo].[MissingContentView] AS
select 
distinct 
c.connectorid,
case when cpg.ProductID is null then 0 else 1 end as Active,
pgl.Name,
p.VendorItemNumber,
va.CustomItemNumber,
b.Name as brandName,
c.ShortDescription,
pb.Barcode,
case when (Select COUNT(*) from ProductMedia pm
			inner join MediaType mt on pm.TypeID = mt.TypeID
			where ProductID = c.ProductID and mt.Type = 'image') > 0 then 1 else 0 end as [Image],
case when (Select COUNT(*) from ProductMedia pm
			inner join MediaType mt on pm.TypeID = mt.TypeID
			where ProductID = c.ProductID and mt.Type = 'youtube' and Sequence = 0) > 0 then 1 else 0 end as youtube,
case when (Select COUNT(*) from ProductAttributeMetaData pamd
			inner join ProductAttributeValue pav on pamd.AttributeID = pav.AttributeID
			where pamd.VendorID = cvs.VendorID and pav.ProductID = c.ProductID) > 0  then 1 else 0 end as Specifications,
c.CreationTime,
c.LastModificationTime,
p.ProductID				
from Content c
left join ContentProductGroup cpg on cpg.ProductID = c.ProductID and cpg.ConnectorID = c.ConnectorID
left join ProductGroupMapping pgm on pgm.ProductGroupMappingID = cpg.ProductGroupMappingID
left join ProductGroupLanguage pgl on pgl.ProductGroupID = pgm.ProductGroupID
left join ContentProduct cp on cp.ProductContentID = c.ProductContentID
left join VendorAssortment va on cp.VendorID = va.VendorID and va.ProductID = c.ProductID
left join ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID
inner join Product p on c.ProductID = p.ProductID
inner join Brand b on p.BrandID = b.BrandID
left join ProductBarcode pb on pb.ProductID = p.ProductID
      
      
      










GO
