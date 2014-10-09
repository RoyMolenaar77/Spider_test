/****** Object:  View [dbo].[MissingContentView]    Script Date: 02/16/2011 12:55:27 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO






ALTER VIEW [dbo].[MissingContentView] AS

select  
distinct
case when cpg.ProductID is null then 0 else 1 end as Active,
p.ProductID as ConcentratorProductID,
p.VendorItemNumber,
va.CustomItemNumber,
b.Name as brandName,
c.ShortDescription,
case when (Select COUNT(*) from ProductMedia pm
			inner join MediaType mt on pm.TypeID = mt.TypeID
			where ProductID = c.ProductID and mt.Type = 'image') > 0 then 1 else 0 end as [Image],
case when (Select COUNT(*) from ProductMedia pm
			inner join MediaType mt on pm.TypeID = mt.TypeID
			where ProductID = c.ProductID and mt.Type = 'youtube' and Sequence = 0) > 0 then 1 else 0 end as youtube,
case when (Select COUNT(*) from ProductAttributeMetaData pamd
			inner join ProductAttributeValue pav on pamd.AttributeID = pav.AttributeID
			left join ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID
			where pamd.VendorID = cvs.VendorID and pav.ProductID = c.ProductID) > 0  then 1 else 0 end as Specifications,
(Select top 1 v.name from ProductGroupMapping pgm
			left join ContentVendorSetting cvs on c.ConnectorID = c.ConnectorID
			inner join vendor v on cvs.vendorid = v.vendorid
			where cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
			order by cvs.contentvendorindex ) as ContentVendor,			
(Select top 1 pb.Barcode from ProductBarcode pb
			where pb.ProductID = c.ProductID) AS barcode,
c.CreationTime,
c.LastModificationTime		
from Content c
left join ContentProductGroup cpg on cpg.ConnectorID = c.ConnectorID and cpg.ProductID = c.ProductID
left join ContentProduct cp on cp.ProductContentID = c.ProductContentID
left join VendorAssortment va on cp.VendorID = va.VendorID and va.ProductID = c.ProductID
inner join Product p on c.ProductID = p.ProductID
inner join Brand b on p.BrandID = b.BrandID
where c.ConnectorID = 1



   







GO



      
      











GO

