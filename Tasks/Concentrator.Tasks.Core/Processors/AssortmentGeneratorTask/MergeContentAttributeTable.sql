WITH [ProductAttributeSelection] AS
( SELECT                       
    isnull(AM.newAttributeID, A.attributeid) as AttributeID,
    A.AttributeID as OriginalAttributeID,
    A.IsSearchable,
    A.IsVisible,
    A.[Sign],
    A.ConfigurablePosition,
    A.GroupID,
    A.NeedsUpdate,
    A.AttributeCode,
    A.ConnectorID,
    A.GroupIndex,
    A.IsConfigurable,
    A.OrderIndex,
    A.LastMetaDataUpdate,
    A.VendorID,
    A.AttributePath,
    PAN.LanguageID,
    isnull(AM.StoreName, PAN.Name) as Name,
    PAV.AttributeValueID,
    PAV.LanguageID as AttributeValueLanguageID,
    PAV.Value,
    PAVT.Label as ValueLabel,
    PAV.ProductID,
    COALESCE(PAV.LastModificationTime, pav.CreationTime) as LastAttributeUpdate
  FROM
  (           select  
                  pamd.AttributeID,
                  pamd.issearchable as IsSearchable,
	pamd.ConfigurablePosition,
                  pamd.isvisible as IsVisible,
                  pamd.[sign] as [Sign],
                  pamd.productattributegroupid as GroupID,
                  pamd.needsupdate as NeedsUpdate,
                  pamd.attributecode as AttributeCode,
                  pamd.IsConfigurable,    
                  pagmd.[index] as GroupIndex,
                  COALESCE(pamd.[index],0) as OrderIndex,
                  pagmd.ConnectorID
                  ,COALESCE(pamd.LastModificationTime, pamd.CreationTime) AS LastMetaDataUpdate
                  ,pamd.VendorID
                  ,pamd.attributepath     
                  from ProductAttributeMetaData pamd 
                  inner join ProductAttributeGroupMetaData pagmd on pamd.ProductAttributeGroupID = pagmd.ProductAttributeGroupID
      )  A
      INNER JOIN ProductAttributeName PAN ON (a.AttributeID = PAN.AttributeID)
      INNER JOIN ProductAttributeValue PAV on( a.AttributeID = PAV.AttributeID and (pav.languageid is null or pav.languageid = @LanguageID))
      LEFT JOIN  ProductAttributeValueLabel PAVT on PAVT.LanguageID = @LanguageID and PAVT.AttributeID = pav.AttributeID and pavt.Value = pav.Value and pavt.ConnectorID = @ConnectorID
      LEFT JOIN (
                              select 
                        ams.attributeid, 
                        min(ams2.attributeid) as newAttributeID, 
                        ams.connectorid,
                        ams.StoreName
                  from attributematchstore ams 
                  left join attributematchstore ams2 on ams2.attributestoreid = ams.attributestoreid
                  group by ams.attributeid,ams.connectorid, ams.storename      
                  having ams.connectorid = @ConnectorID
      ) AM on A.AttributeID = AM.AttributeID
)
MERGE [dbo].[ContentAttribute] AS [Target]
USING
(
  SELECT DISTINCT c.*, PAGN.Name AS GroupName
  FROM #attTemp c
  INNER JOIN [dbo].[Connector] con on con.[ConnectorID] = @ConnectorID
  INNER JOIN Contentvendorsetting cvs on cvs.[ConnectorID] = con.[ConnectorID] and cvs.VendorID = c.vendorid
  INNER JOIN ContentProductGroup cpg on (c.ProductID = cpg.ProductID and cpg.[ConnectorID] = @ConnectorID)
  INNER JOIN ProductGroupMapping pgm on (pgm.[ConnectorID] = @ConnectorID or(con.parentconnectorid is not null and pgm.[ConnectorID] = con.parentconnectorid))and cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
  and (cvs.BrandID is null or (c.BrandID = cvs.BrandID))
  and (cvs.ProductGroupID is null or (cvs.ProductGroupID = pgm.ProductGroupID))
  and (cvs.ProductID is null or (cvs.ProductID = c.ProductID))
  inner join ProductAttributeGroupName PAGN on PAGN.ProductAttributeGroupID = c.GroupID AND  PAGN.[LanguageID] = c.[LanguageID]                 
) AS [Source]
ON  [Target].[AttributeValueID] = [Source].[AttributeValueID] 
AND [target].[ConnectorID] = [Source].[ConnectorID]
AND [Target].[LanguageID] = @LanguageID
WHEN NOT MATCHED BY TARGET THEN
  INSERT
  ( [AttributeID], 
    [IsSearchable], 
    [IsVisible], 
    [Sign], 
    [GroupID], 
    [IsConfigurable], 
    [NeedsUpdate], 
    [AttributeCode], 
    [GroupIndex],
    [OrderIndex],
    [GroupName],
    [ProductID],
    [AttributeValue],
    [LanguageID],
    [ConnectorID],
    [AttributeName],
    [LastUpdate],
    [VendorI]D,
    [AttributePath],
    [AttributeValueID],
    [AttributeOriginalValue],
    [ConfigurablePosition]
  )
  VALUES
  ( [Source].[AttributeID],
    [Source].[IsSearchable],
    [Source].[IsVisible],
    [Source].[Sign],
    [Source].[GroupID],
    [Source].[IsConfigurable],
    ISNULL([Source].NeedsUpdate, 0),
    [Source].[AttributeCode],
    [Source].[GroupIndex],
    [Source].[OrderIndex],
    [Source].[GroupName],
    [Source].[ProductID],
    isnull([Source].[Value], ''), 
    isnull([Source].[LanguageID], @LanguageID), 
    @ConnectorID,
    [Source].[Name],
    CASE WHEN [Source].[LastMetaDataUpdate] > [Source].[LastAttributeUpdate]
      THEN [Source].[LastMetaDataUpdate]
      ELSE [Source].[LastAttributeUpdate]
    END,
    [Source].[VendorID], 
    [Source].[AttributePath], 
    [Source].[AttributeValueID],
    [Source].[AttributeOriginalValue],
		[Source].[ConfigurablePosition]
  )      
WHEN MATCHED THEN 
  UPDATE SET
    [Target].AttributeValue = [Source].Value,
    [Target].AttributeOriginalValue = [Source].AttributeOriginalValue,
    [Target].IsConfigurable = [Source].IsConfigurable,
    [Target].IsVisible = [Source].IsVisible,
		[Target].OrderIndex = [Source].OrderIndex,
		[Target].ConfigurablePosition = [Source].ConfigurablePosition
;