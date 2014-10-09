CREATE View ConnectorPriceCalculationView 
AS

WITH
	Rules AS (
		select * from contentprice	
	),	
	
	--- select product, product group and brand information related to vendor & connector
	Catalogue AS (
		SELECT distinct CP.ProductID, CP.ConnectorID, PGM.ProductGroupID, P.BrandID
		FROM Content CP 
		INNER JOIN ContentProductGroup CPG on CP.ProductID = CPG.ProductID and CP.ConnectorID = CPG.ConnectorID
		INNER JOIN Product P on P.ProductID = CP.ProductID
		INNER JOIN ProductGroupMapping PGM on PGM.ProductGroupMappingID = CPG.ProductGroupMappingID				
	),
	
	-- get all vendor assortments for the catalogue
	VendorCatalogue as (
		SELECT C.ProductID, VA.VendorID, C.ConnectorID, C.ProductGroupID, C.BrandID, VA.VendorAssortmentID
		FROM Catalogue C
		INNER JOIN VendorAssortment VA on VA.ProductID = C.ProductID
	),

	-- filter on content product and rank the options
	VendorCatalogueRanked as (
		SELECT 
		VC.*,
		RANK() OVER (PARTITION BY CP.ProductContentID
					 ORDER BY CASE
						WHEN CP.ProductID = VC.ProductID THEN 4
						WHEN CP.ProductGroupID= VC.ProductGroupID THEN 2
						WHEN CP.BrandID = VC.BrandID THEN 3
						WHEN CP.ConnectorID = VC.ConnectorID THEN 1
						ELSE NULL 
					END DESC ) 
		AS RankNumber
		FROM VendorCatalogue VC 
		INNER JOIN ContentProduct CP on VC.VendorID = CP.VendorID AND (CP.ConnectorID = VC.ConnectorID OR CP.ProductID = VC.ProductID OR VC.BrandID = CP.BrandID OR VC.ProductGroupID = CP.ProductGroupID)
	),
	
	-- get the top ranked vendor for this product 
	VendorCatalogueFiltered AS (
		SELECT distinct * 
		FROM VendorCatalogueRanked 
		WHERE RankNumber = 1
	),
	
	-- construct the rules
	RuleHierarchy as (
	  	SELECT
	  	C.ProductID AS CatalogueProductID,
	  	C.ProductGroupID AS CatalogueProductGroupID,
	  	c.BrandID AS CatalogueBrandID,
	  	c.ConnectorID as CatalogueConnectorID,
	  	C.VendorAssortmentID,
	  	CASE 
	  		WHEN R.ProductID = C.ProductID THEN 4
			WHEN R.ProductGroupID= C.ProductGroupID THEN 2
			WHEN R.BrandID = C.BrandID THEN 3
			WHEN R.ConnectorID = C.ConnectorID THEN 1			
			ELSE 0
		END AS MatchNumber,
		
		Rank() OVER (PARTITION BY VP.ContentPriceRuleID
		ORDER BY CASE
			WHEN R.ProductID = C.ProductID THEN 4
			WHEN R.ProductGroupID= C.ProductGroupID THEN 2
			WHEN R.BrandID = C.BrandID THEN 3
			WHEN R.ConnectorID = C.ConnectorID THEN 1
			ELSE NULL 
		END DESC ) AS RankNumber,
	  	R.*
	  	FROM ContentPrice VP 
	  	INNER JOIN VendorCatalogueFiltered C ON (VP.ProductID = C.ProductID OR VP.ProductGroupID = C.ProductGroupID OR VP.BrandID = C.BrandID OR VP.ConnectorID = C.ConnectorID) 
	  	LEFT JOIN Rules R ON (VP.ProductID = R.ProductID OR VP.ProductGroupID = R.ProductGroupID OR VP.BrandID = R.BrandID OR VP.ConnectorID = R.ConnectorID)
	  	WHERE R.ProductID is not null or R.BrandID is not null or R.ProductGroupID is not null OR R.ConnectorID is not null
	),
	
	-- get only the most concrete rule
	RulesFiltered as (
		SELECT distinct * 
		FROM RuleHierarchy
		where RankNumber = 1 
		
	),
	
	-- rank over indexes. Another level of filtering
	RankOrderRuleIndex as (
		SELECT 
		RF.* ,
		RANK() OVER (PARTITION BY RF.ContentPriceRuleID
		ORDER BY RF.ContentPriceRuleIndex,
		RF.ContentPriceRuleID) as ContentPriceRuleIndexRank
		FROM  RulesFiltered RF	
	),
	
	-- get top ranked rules 
	RankedOrderRules as (
		SELECT 
			CatalogueProductID, 
			CatalogueConnectorID, 
			UnitPriceIncrease, 
			CostPriceIncrease,
			ContentPriceRuleID,
			VendorAssortmentID, 
			Margin,
			FixedPrice,
			PriceRuleType,
			VendorID
		FROM RankOrderRuleIndex
		WHERE ContentPriceRuleIndexRank = 1 
	),
	
	 --get the prices
	Prices as (
		SELECT 
		DISTINCT		
		ROR.CatalogueProductID as ProductID,
		ROR.CatalogueConnectorID as ConnectorID,
		ROR.VendorAssortmentID,
		(
			CASE WHEN ROR.Margin is null AND ROR.PriceRuleType = 1 THEN VP.Price ELSE
				CASE WHEN ROR.Margin is null AND ROR.PriceRuleType = 2 THEN VP.CostPrice ELSE
					CASE WHEN ROR.FixedPrice is not null then ROR.FixedPrice ELSE
						CASE WHEN ROR.Margin = '%' AND ROR.PriceRuleType = 1 THEN ROR.UnitPriceIncrease * VP.Price else
							CASE WHEN ROR.Margin = '%' AND ROR.PriceRuleType = 2 THEN ROR.UnitPriceIncrease * VP.CostPrice else
								CASE WHEN ROR.Margin = '+' AND ROR.PriceRuleType = 1 THEN ROR.UnitPriceIncrease + VP.Price else
									CASE WHEN ROR.Margin = '+' AND ROR.PriceRuleType = 2 THEN ROR.UnitPriceIncrease + VP.CostPrice
									END
								END
							END					
						END
					END
				END
			END
		) AS Price,
		ROR.ContentPriceRuleID,
		ROR.PriceRuleType
		
		FROM VendorPrice VP
		Inner JOIN RankedOrderRules ROR on VP.VendorAssortmentID = ROR.VendorAssortmentID								
		WHERE VP.MinimumQuantity <= 1
	)
	
	select * from prices
	
GO