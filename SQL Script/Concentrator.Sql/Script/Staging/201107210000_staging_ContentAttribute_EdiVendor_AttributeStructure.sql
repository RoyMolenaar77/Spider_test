alter table contentAttribute add AttributePath nvarchar(255) null 
alter table contentAttribute add AttributeValueID int null
alter table edivendor add CompanyCode nvarchar(50) null
alter table edivendor add DefaultDocumentType nvarchar(50) null
alter table edivendor add OrderBy nvarchar(50) null
/****** Object:  View [dbo].[Concentrator_AttributeStructureView]    Script Date: 07/20/2011 18:13:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


ALTER VIEW [dbo].[Concentrator_AttributeStructureView]   AS 

		select 
			pamd.attributeid as AttributeID,
			--pav.value as AttributeValue
			pamd.issearchable as IsSearchable,
			pamd.isvisible as IsVisible,
			pamd.[sign] as [Sign],
			pamd.productattributegroupid as GroupID,
			pamd.needsupdate as NeedsUpdate,
			pamd.attributecode as AttributeCode,
			--pagn.Name as GroupName,
			--pan.name as AttributeName,
			pagmd.[index] as GroupIndex,
			COALESCE(pamd.[index],0) as OrderIndex,
			pagmd.ConnectorID
			,COALESCE(pamd.LastModificationTime, pamd.CreationTime) AS LastMetaDataUpdate
			,pamd.VendorID
			,pamd.attributepath
			--pan.languageid 
			--pav.productid as ProductID
			
		from dbo.productattributemetadata pamd
			--inner join productattributename pan on (pan.attributeid = pamd.attributeid )
			--inner join productattributevalue pav on (pan.attributeid = pav.attributeid and pav.languageid = pan.languageid)
			--inner join productattributegroupname pagn on pagn.productattributegroupid = pamd.productattributegroupid 
			--and pan.languageid = pagn.languageid
			inner join dbo.productattributegroupmetadata pagmd on pagmd.productattributegroupid = pamd.productattributegroupid
			--inner join Product p on p.productid = pav.productID
			--left join Content c on p.ProductID = c.ProductID 
			--WHERE ISNULL(pagmd.connectorID,@ConnectorID) = @ConnectorID 

GO




