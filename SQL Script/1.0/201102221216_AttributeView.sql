
/****** Object:  View [dbo].[Concentrator_ProductAttributesView]    Script Date: 02/22/2011 12:04:55 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO




ALTER VIEW [dbo].[Concentrator_ProductAttributesView]   AS

select attributeid,languageid,productid,max(value) as value,max(lastattributeupdate) as lastattributeupdate
from (
SELECT PAN.AttributeID, PAN.LanguageID, PAV.ProductID, PAV.Value ,COALESCE(pav.LastModificationTime, pav.CreationTime) AS LastAttributeUpdate
	FROM dbo.ProductAttributeName PAN 
	LEFT JOIN dbo.ProductAttributeValue PAV ON (PAV.AttributeID = PAN.AttributeID AND PAV.LanguageID = PAN.LanguageID)
	WHERE (PAV.Value <> '' OR PAV.Value IS NULL) 
union all
SELECT PAN.AttributeID, PAN.LanguageID, pm.CorrespondingProductid, PAV.Value ,COALESCE(pav.LastModificationTime, pav.CreationTime) AS LastAttributeUpdate
	FROM dbo.ProductAttributeName PAN 
	LEFT JOIN dbo.ProductAttributeValue PAV ON (PAV.AttributeID = PAN.AttributeID AND PAV.LanguageID = PAN.LanguageID)
	inner join productmatch pm on (pm.productid = PAV.productid and pm.isMatched = 1)
	WHERE (PAV.Value <> '' OR PAV.Value IS NULL)
union all
SELECT PAN.AttributeID, PAN.LanguageID, pm.Productid, PAV.Value ,COALESCE(pav.LastModificationTime, pav.CreationTime) AS LastAttributeUpdate
	FROM dbo.ProductAttributeName PAN 
	LEFT JOIN dbo.ProductAttributeValue PAV ON (PAV.AttributeID = PAN.AttributeID AND PAV.LanguageID = PAN.LanguageID)
	inner join productmatch pm on (pm.CorrespondingProductid = PAV.productid and pm.isMatched = 1)
	WHERE (PAV.Value <> '' OR PAV.Value IS NULL)
	) a 
	group by attributeid,languageid,productid

GO

/****** Object:  StoredProcedure [dbo].[sp_GenerateContentAttributes]    Script Date: 02/22/2011 11:51:49 ******/
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
		SELECT distinct AV.AttributeID, AV.IsSearchable, AV.IsVisible, AV.Sign, AV.GroupID, AV.NeedsUpdate, AV.AttributeCode, AV.GroupIndex,
			AV.OrderIndex, PAGN.Name AS GroupName
			, TAV.ProductID, ISNULL(TAV.Value,'') AS AttributeValue, TAV.LanguageID,
			
			ISNULL(AV.ConnectorID,@ConnectorID) as ConnectorID
			,PAN.Name AS AttributeName,

		CASE WHEN AV.LastMetaDataUpdate > TAV.LastAttributeUpdate THEN AV.LastMetaDataUpdate ELSE TAV.LastAttributeUpdate END AS LastUpdate
,AV.VendorID
	FRom Concentrator_AttributeStructureView AV
	inner join dbo.Concentrator_ProductAttributesView TAV ON (TAV.AttributeID = AV.AttributeID)
	inner join Content c on TAV.ProductID = c.ProductID and c.ConnectorID = @ConnectorID
	inner join Product p on c.ProductID = p.ProductID
	inner join ContentProductGroup cpg on c.ProductID = cpg.ProductID and c.ConnectorID = cpg.ConnectorID
	inner join dbo.ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID and cvs.vendorid = av.vendorid
	inner join ProductAttributeGroupName PAGN ON (PAGN.ProductAttributeGroupID = AV.GroupID AND PAGN.LanguageID = TAV.LanguageID)
	inner join ProductAttributeName PAN on (PAN.attributeid = AV.AttributeID AND PAN.LanguageID = TAV.LanguageID )
	left join ProductGroupMapping pgm on pgm.ConnectorID = c.ConnectorID and cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
	WHERE ISNULL(AV.ConnectorID,@ConnectorID) = @ConnectorID
	AND TAV.LanguageID = @LanguageID
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
 end