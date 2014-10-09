DELETE FROM [DBO].[Refundqueue]
WHERE OrderID=@1 and ConnectorID=@2;