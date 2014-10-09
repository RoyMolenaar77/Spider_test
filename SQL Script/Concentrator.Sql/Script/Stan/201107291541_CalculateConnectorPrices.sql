SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Stan Todorov
-- Create date: 29-07-2011
-- Description:	Fetches and updates connector product prices
-- =============================================
CREATE PROCEDURE sp_CalculateConnectorPrices 
	-- Add the parameters for the stored procedure here
	@ConnectorID int = null
AS
BEGIN
	
	MERGE ConnectorProductPrice AS target
	USING (SELECT ProductID,ConnectorID, Price, ContentPriceRuleID, PriceRuleType
		   FROM ConnectorPriceCalculationView
		   WHERE @ConnectorID is null OR @ConnectorID = ConnectorID) 
			AS source (
					ProductID,
					ConnectorID,
					Price,
					ContentPriceRuleID,
					PriceRuleType					
					)
			ON		(
					target.ProductID = source.ProductID AND
					target.ConnectorID = source.ConnectorID AND
					target.PriceRuleType = source.PriceRuleType
					)
	WHEN MATCHED THEN 
		UPDATE SET	Price = source.Price,
					ContentPriceRuleID = source.ContentPriceRuleID
	WHEN NOT MATCHED THEN
		INSERT (ProductID, ConnectorID, Price,  ContentPriceRuleID,PriceRuleType)
		VALUES (source.ProductID, source.ConnectorID, source.Price,  source.ContentPriceRuleID, source.PriceRuleType);
END
GO
