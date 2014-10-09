  MERGE MasterGroupMapping AS TARGET
  USING (SELECT MasterGroupMappingID, MagentoPageLayoutID from ProductGroupMapping WHERE MagentoPageLayoutID is not null) AS SOURCE
  ON TARGET.SourceMasterGroupMappingID = SOURCE.MasterGroupMappingID
  WHEN MATCHED AND TARGET.MagentoPageLayoutID is NULL
  THEN UPDATE
  SET MagentoPageLayoutID = SOURCE.MagentoPagelayoutID;
