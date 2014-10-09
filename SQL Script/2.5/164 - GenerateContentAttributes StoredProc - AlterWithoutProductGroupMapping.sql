
ALTER PROCEDURE [dbo].[sp_GenerateContentAttributes] 
      (@ConnectorID int = null)
AS
BEGIN

DECLARE @LanguageID int


      
DECLARE curConnector CURSOR FOR 
      SELECT ConnectorID FROM Connector
      WHERE IsActive = 1 or ConnectorID = 9
      AND(
            (ConnectorType & 2 = 2) OR  (ConnectorType & 4 = 4)
            )
      AND (ConnectorID = @ConnectorID OR @ConnectorID IS NULL)


DECLARE curLanguage CURSOR FOR SELECT LanguageID FROM Language 

OPEN curConnector
FETCH NEXT FROM curConnector INTO @ConnectorID



WHILE @@FETCH_STATUS = 0
BEGIN 
      --DELETE FROM ContentAttribute where connectorid = @ConnectorID

      OPEN curLanguage
      
      FETCH NEXT FROM curLanguage INTO @LanguageID
      WHILE @@FETCH_STATUS = 0
      BEGIN 
            PRINT @ConnectorID 
            PRINT @LanguageID;
            
      
                  
                  WITH




                  AttributesWithMatches as (
                        SELECT                       
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
                  ),


                  ProductAttributes as (
                        select 
                        LAV.AttributeCode,
                        LAV.AttributeID,
                        LAV.AttributePath,
                        LAV.AttributeValueID,
                        LAV.AttributeValueLanguageID,
                        LAV.GroupID,
                        LAV.GroupIndex,
						LAV.ConfigurablePosition,
                        LAV.IsSearchable,
                        LAV.IsVisible,
                        LAV.IsConfigurable,
                        LAV.LanguageID,
                        LAV.LastAttributeUpdate,
                        LAV.LastMetaDataUpdate,
                        LAV.Name,
                        LAV.NeedsUpdate,
                        LAV.OrderIndex,
                        LAV.ProductID,
                        LAV.[Sign],
                        isnull(LAV.ValueLabel, LAV.Value) as Value,
                        LAV.Value as AttributeOriginalValue,
                        LAV.VendorID,
                        lav.ConnectorID as AttributeConnectorID
                        FROM  
                        (
                                   SELECT 
                                   ProductID
                                   FROM CONTENT
                                   WHERE ConnectorID = @ConnectorID
                                   
                                   UNION 
                                   
                                   SELECT 
                                   PM2.ProductID
                                   FROM CONTENT CP 
                                   INNER JOIN ProductMatch PM on (CP.ProductID = PM.ProductID and PM.isMatched = 1)
                                   INNER JOIN ProductMatch PM2 on (PM2.ProductMatchID = PM.ProductMatchID AND PM.ProductID != PM2.ProductID and PM2.isMatched = 1)
                                    WHERE CP.ConnectorID = @ConnectorID
                        ) MCP
                        INNER JOIN AttributesWithMatches LAV  on (LAV.ProductID = MCP.ProductID )
                        WHERE (lav.connectorid is null or lav.ConnectorID = @connectorid)
                  )



                  SELECT distinct c.* ,p.brandid, @ConnectorID AS ConnectorID
                  into #attTemp
                  FROM ProductAttributes c 
                  inner join connectorlanguage cl on (c.languageid = cl.languageid and cl.ConnectorID = @ConnectorID)
                  inner join product p on c.productid = p.productid
                  where c.LanguageID = @LanguageID


                  --CREATE NONCLUSTERED INDEX [IDX_ConnectorVendor] ON #attTemp
                  --(
                  --    [VendorID] ASC
                  --)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                  --CREATE NONCLUSTERED INDEX [IDX_Filter] ON #attTemp
                  --(
                  --    productid ASC,
                  --    brandid ASC
                  --)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
                  --;

                  --CREATE NONCLUSTERED INDEX [IDX_AttGroup] ON #attTemp
                  --(
                  --    LanguageID ASC,
                  --    GroupID ASC
                  --)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

      
                  
                  MERGE ContentAttribute [target]
                  USING (
                  
                        
                  
                        select distinct c.*, PAGN.Name as GroupName from #attTemp c
                        inner join connector con on con.connectorid = @connectorid
                  inner join contentvendorsetting cvs on cvs.ConnectorID = con.ConnectorID and cvs.VendorID = c.vendorid
                  --inner join ContentProductGroup cpg on (c.ProductID = cpg.ProductID and cpg.ConnectorID = @Connectorid)
                  --inner join ProductGroupMapping pgm on (pgm.ConnectorID = @ConnectorID or(con.parentconnectorid is not null and pgm.connectorid = con.parentconnectorid))and cpg.ProductGroupMappingID = pgm.ProductGroupMappingID
                  --and (cvs.BrandID is null or (c.BrandID = cvs.BrandID))
                  --      and (cvs.ProductGroupID is null or (cvs.ProductGroupID = pgm.ProductGroupID))
                  --      and (cvs.ProductID is null or (cvs.ProductID = c.ProductID))
                  inner join ProductAttributeGroupName PAGN on PAGN.ProductAttributeGroupID = c.GroupID AND  PAGN.LanguageID = c.LanguageID
                  
                  ) [source]
                  ON 
                  (
                        [target].attributevalueid = [source].attributevalueid and [source].connectorid = [target].connectorid and @languageID = [target].languageID
                  ) 
                  WHEN NOT MATCHED BY TARGET 
                  THEN INSERT (AttributeID, IsSearchable, IsVisible, [Sign], GroupID , IsConfigurable, NeedsUpdate, AttributeCode, GroupIndex, OrderIndex, GroupName, ProductID, AttributeValue, LanguageID, ConnectorID, AttributeName, LastUpdate, VendorID, AttributePath, AttributeValueID, AttributeOriginalValue, ConfigurablePosition)
                        VALUES (   [source].AttributeID, 
                                         [source].IsSearchable, 
                                         [source].IsVisible, 
                                         [source].Sign, 
                                         [source].GroupID, 
                                         [source].IsConfigurable,
                                         isnull([source].NeedsUpdate, 0), 
                                         [source].AttributeCode, 
                                         [source].GroupIndex, 
                                         [source].OrderIndex, 
                                         
                                         [source].GroupName, 
                                         [source].ProductID, 
                                         isnull([source].Value, ''), 
                                         isnull([source].LanguageID, @LanguageID), 
                                         @ConnectorID,
                                         [source].Name,
                                         CASE WHEN [source].LastMetaDataUpdate > [source].LastAttributeUpdate THEN [source].LastMetaDataUpdate ELSE [source].LastAttributeUpdate END,
                                         [source].VendorID, 
                                         [source].AttributePath, 
                                         [source].AttributeValueID,
                                         [source].AttributeOriginalValue,
										 [source].ConfigurablePosition
                                   )
      
                  WHEN MATCHED 
                  THEN UPDATE SET
                                         [target].AttributeValue = [source].Value,
                                         [target].AttributeOriginalValue = [source].AttributeOriginalValue,
                                         [target].IsConfigurable = [source].IsConfigurable,
                                         [target].IsVisible = [source].IsVisible,
										 [target].OrderIndex = [source].OrderIndex,
										 [target].ConfigurablePosition = [source].ConfigurablePosition;
                                         
                                         
                  drop table #attTemp          
                        
            
            
            FETCH NEXT FROM curLanguage INTO @LanguageID
      END

      CLOSE curLanguage
      
      FETCH NEXT FROM curConnector INTO @ConnectorID
END
DEALLOCATE curLanguage

CLOSE curConnector
DEALLOCATE curConnector

END










GO


