MERGE [dbo].[Content] AS [Target]
USING [tmp].[{0}] AS [Source]
ON    [Target].[ProductID] = [Source].[ProductID]
AND   [Target].[ConnectorID] = [Source].[ConnectorID]
WHEN NOT MATCHED BY TARGET THEN 
	INSERT ([ConnectorID], [ConnectorPublicationRuleID], [ProductID], [CreatedBy], [CreationTime])
	VALUES ([Source].[ConnectorID], [Source].[ConnectorPublicationRuleID], [Source].[ProductID], 1, GetDate())
WHEN MATCHED THEN 
UPDATE SET
  [Target].[ConnectorPublicationRuleID] = [Source].[ConnectorPublicationRuleID],
	[Target].[ShortDescription] = [Source].[ShortDescription],
	[Target].[LongDescription] = [Source].[LongDescription]
WHEN NOT MATCHED BY SOURCE AND [Target].[ConnectorID] = @ConnectorID THEN 
  DELETE;