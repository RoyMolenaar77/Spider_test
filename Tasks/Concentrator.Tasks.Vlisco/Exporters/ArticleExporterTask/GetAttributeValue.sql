SELECT  *
FROM    [dbo].[ProductAttributeValue]
WHERE   [AttributeID] = @0
  AND   [ProductID] IN (@1, @2)