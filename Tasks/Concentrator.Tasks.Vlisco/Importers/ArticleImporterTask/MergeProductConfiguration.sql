MERGE [dbo].[ProductAttributeConfiguration] AS [Target]
USING [dbo].[ProductAttributeValue]         AS [Source]
ON  [Target].[ProductID]    = [Source].[ProductID]
AND [Target].[AttributeID]  = [Source].[AttributeID]
WHEN NOT MATCHED BY TARGET AND [Source].[AttributeID] = @0 THEN
  INSERT ([ProductID], [AttributeID])
  VALUES ([ProductID], [AttributeID])
WHEN NOT MATCHED BY SOURCE AND [Target].[AttributeID] = @0 THEN
  DELETE;