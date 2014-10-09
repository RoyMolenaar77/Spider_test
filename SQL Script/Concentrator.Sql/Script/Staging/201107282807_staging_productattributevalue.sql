alter table productattributevalue alter column languageid int null

/****** Object:  View [dbo].[Concentrator_ProductAttributesView]    Script Date: 07/28/2011 13:17:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO





ALTER VIEW [dbo].[Concentrator_ProductAttributesView]   AS

select attributevalueid,attributeid,languageid,productid,max(value) as value,max(lastattributeupdate) as lastattributeupdate
from (
SELECT pav.attributevalueid, PAN.AttributeID, PAN.LanguageID, PAV.ProductID, PAV.Value ,COALESCE(pav.LastModificationTime, pav.CreationTime) AS LastAttributeUpdate
	FROM dbo.ProductAttributeName PAN 
	LEFT JOIN dbo.ProductAttributeValue PAV ON (PAV.AttributeID = PAN.AttributeID AND (PAV.LanguageID is null or PAV.LanguageID = PAN.LanguageID))
	WHERE (PAV.Value <> '' OR PAV.Value IS NULL) 
union all
SELECT pav.attributevalueid, PAN.AttributeID, PAN.LanguageID, pm2.productid, PAV.Value ,COALESCE(pav.LastModificationTime, pav.CreationTime) AS LastAttributeUpdate
	FROM dbo.ProductAttributeName PAN 
	LEFT JOIN dbo.ProductAttributeValue PAV ON (PAV.AttributeID = PAN.AttributeID AND (PAV.LanguageID is null or PAV.LanguageID = PAN.LanguageID))
	inner join ProductMatch pm  on (pm.productid = PAV.productid and pm.isMatched = 1)
	inner join ProductMatch pm2 on pm2.ProductMatchID = pm.ProductMatchID and pm.ProductID != pm2.ProductID and pm2.isMatched = 1
WHERE (PAV.Value <> '' OR PAV.Value IS NULL)
	) a 
	group by attributevalueid, attributeid,languageid,productid

GO

/****** Object:  StoredProcedure [dbo].[sp_GenerateContentAttributes]    Script Date: 07/28/2011 13:19:44 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[sp_GenerateContentAttributes]
AS
BEGIN

DECLARE @ConnectorID int
DECLARE @LanguageID int

--SET @ConnectorID =1
--SET @LanguageID = 2

DECLARE curConnector CURSOR FOR 
	SELECT ConnectorID FROM Connector
	WHERE IsActive = 1
	AND(
		(ConnectorType & 2 = 2) OR  (ConnectorType & 4 = 4)
		)

DECLARE curLanguage CURSOR FOR SELECT LanguageID FROM Language 

OPEN curConnector
FETCH NEXT FROM curConnector INTO @ConnectorID
WHILE @@FETCH_STATUS = 0
BEGIN

	OPEN curLanguage
	
	FETCH NEXT FROM curLanguage INTO @LanguageID
	WHILE @@FETCH_STATUS = 0
	BEGIN	
		PRINT @ConnectorID 
		PRINT @LanguageID
		
		
		BEGIN TRAN UpdateContent
				
				
		DELETE FROM ContentAttribute WHERE ISNULL(ConnectorID,@ConnectorID)=@ConnectorID AND LanguageID = @LanguageID
		
		IF(select COUNT(*) from connectorlanguage
		where connectorid = @ConnectorID
		and Languageid = @LanguageID) > 0
		BEGIN
		
		INSERT INTO ContentAttribute

		SELECT distinct ISNULL(AM.NewAttributeID,AV.AttributeID) as AttributeID, AV.IsSearchable, AV.IsVisible, AV.Sign, AV.GroupID, AV.NeedsUpdate, AV.AttributeCode, AV.GroupIndex,
			AV.OrderIndex, PAGN.Name AS GroupName
			, TAV.ProductID, ISNULL(TAV.Value,'') AS AttributeValue, isnull(TAV.LanguageID,@LanguageID),
			
			ISNULL(AV.ConnectorID,@ConnectorID) as ConnectorID
			,ISNULL(AM.NewAttributeName,pan.Name) AS AttributeName,

		CASE WHEN AV.LastMetaDataUpdate > TAV.LastAttributeUpdate THEN AV.LastMetaDataUpdate ELSE TAV.LastAttributeUpdate END AS LastUpdate
,AV.VendorID, AV.AttributePath, TAV.attributevalueid
	FRom Concentrator_AttributeStructureView AV
	inner join dbo.Concentrator_ProductAttributesView TAV ON (TAV.AttributeID = AV.AttributeID)
	inner join Content c on TAV.ProductID = c.ProductID and c.ConnectorID = @ConnectorID
	inner join Product p on c.ProductID = p.ProductID
	inner join ContentProductGroup cpg on c.ProductID = cpg.ProductID and c.ConnectorID = cpg.ConnectorID
	inner join dbo.ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID and cvs.vendorid = av.vendorid
	inner join ProductAttributeGroupName PAGN ON (PAGN.ProductAttributeGroupID = AV.GroupID AND PAGN.LanguageID = @LanguageID)
	inner join ProductAttributeName PAN on (PAN.attributeid = AV.AttributeID AND PAN.LanguageID = @LanguageID )
	left join ProductGroupMapping pgm on pgm.ConnectorID = c.ConnectorID and cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
	left join (select 
	pamd.AttributeID as OldAttributeID,
	pan.Name as OldAttributeName,
	min(ams2.AttributeID) as NewAttributeID,
	min(isnull(ams2.StoreName,pan2.Name)) as NewAttributeName,
	pan.LanguageID
	from ProductAttributeMetaData pamd
	inner join ProductAttributeName pan on pamd.AttributeID = pan.AttributeID
	inner join AttributeMatchStore ams on pamd.AttributeID = ams.AttributeID
	inner join AttributeMatchStore ams2 on ams2.AttributeStoreID = ams.AttributeStoreID
	inner join ProductAttributeName pan2 on pan2.LanguageID = pan.LanguageID
	group by pamd.AttributeID,pan.Name, pan2.AttributeID, pan.LanguageID
	having pan2.AttributeID = MIN(ams2.AttributeID) ) AM on AM.OldAttributeID = av.AttributeID and am.LanguageID = tav.languageid
	WHERE ISNULL(AV.ConnectorID,@ConnectorID) = @ConnectorID
	AND (TAV.languageid is null or TAV.LanguageID = @LanguageID)
	and (cvs.BrandID is null or (p.BrandID = cvs.BrandID))
	and (cvs.ProductGroupID is null or (cvs.ProductGroupID = pgm.ProductGroupID))
	and (cvs.ProductID is null or (cvs.ProductID = c.ProductID))

	--AND TAV.Value is not null
		END
		
		COMMIT TRAN UpdateContent
		
		
		FETCH NEXT FROM curLanguage INTO @LanguageID
	END

	CLOSE curLanguage
	
	FETCH NEXT FROM curConnector INTO @ConnectorID
END
DEALLOCATE curLanguage

CLOSE curConnector
DEALLOCATE curConnector





END
