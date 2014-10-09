ALTER PROCEDURE [dbo].[GetCalculatedPriceView] (@ConnectorID int)   
AS  
WITH OwnPrices  AS  (        
      SELECT PCP.ProductID, AVG(PCP.Price) AS OwnPrice        
      FROM ProductCompetitorMapping PCM        
      INNER JOIN ProductCompetitorPrice PCP ON (PCP.ProductCompetitorMappingID = PCM.ProductCompetitorMappingID)        
      WHERE PCM.Competitor = 'MyCom'        
      GROUP BY PCP.ProductID  ),  
PriceSource AS  (        
      SELECT PCM.Competitor, PCP.ProductID, PCP.Price        
      FROM ProductCompetitorMapping PCM        
      INNER JOIN ProductCompetitorPrice PCP ON (PCP.ProductCompetitorMappingID = PCM.ProductCompetitorMappingID)  ),  
preferedVendorPrices as(   
      select va.ProductID,va.vendorid,vp.*, pcv.isPreferred
      from VendorPrice vp   
      inner join VendorAssortment va on vp.VendorAssortmentID = va.VendorAssortmentID   
      left join PreferredConnectorVendor pcv on pcv.VendorID = va.VendorID   
      where vp.Price is not null AND pcv.ConnectorId = @ConnectorID  ),   
ConnectorProductGroups as (  
      select productID,productGroupID  
      from ContentProductGroup cpg   
      inner join ProductGroupMapping pgm on cpg.ProductGroupMappingID = pgm.ProductGroupMappingID  
      where cpg.connectorID  = @ConnectorID  
      group by ProductID,ProductGroupID  ),   
ConnectorPriceRule as (    
      select VendorID,
            ProductGroupID,
            BrandID,
            ProductID,
            Margin,
            UnitPriceIncrease,
            CostPriceIncrease,
            MinimumQuantity, 
            contentPriceRuleIndex as contentPriceRuleIndex, 
            fixedPrice, 
            ContentPriceLabel, 
            FromDate, 
            ToDate, 
            BottomMargin, 
            ComparePricePosition, 
            MinComparePricePosition, 
            MaxComparePricePosition, 
            CompareSourceID, 
            SellMargin     
      from ContentPrice    
      WHERE ConnectorID = @ConnectorID   ),  
CompetitorInfo as(   
      select    pcm.ProductCompareSourceID,   
            ROW_NUMBER() OVER (PARTITION BY pcp.ProductID ORDER BY price) AS CompetitorRank,   
            pcp.Price,   
            pcp.LastImport,   
            pcp.Stock,   
            pcp.ProductID,   
            pcm.Competitor,   
            pcs.source,   
            (     SELECT COUNT(*)+1 
                  FROM PriceSource PCP2               
                  WHERE PCP2.ProductID = PCP.ProductID 
                        AND PCP2.Price <  AVG(OP.OwnPrice)
                        AND PCP2.Competitor <> 'MyCom'        ) AS CurrentRank   
      from ProductCompetitorPrice pcp   
            inner join ProductCompetitorMapping pcm on pcp.ProductCompetitorMappingID = pcm.ProductCompetitorMappingID   
            inner join ProductCompareSource pcs on pcs.ProductCompareSourceID = pcm.ProductCompareSourceID   
            left JOIN OwnPrices OP ON (OP.ProductID = PCP.ProductID)   
      group by pcm.ProductCompareSourceID, pcp.ProductID,pcp.price,pcm.Competitor,pcp.LastImport,pcp.Stock, pcs.Source  ),  
prices as (   
      select distinct 
            c.productid as ProductID,   
            --case when cpr.FixedPrice is not null then cpr.FixedPrice else      
            --      case when cpr.Margin = '%' and cpr.UnitPriceIncrease is not null and vp.Price is not null then vp.Price * cpr.UnitPriceIncrease else      
            --            case when cpr.Margin = '%' and cpr.CostPriceIncrease is not null and vp.CostPrice is not null then vp.CostPrice * cpr.CostPriceIncrease else       
            --                 case when cpr.Margin = '+' and cpr.UnitPriceIncrease is not null and vp.Price is not null then vp.Price + cpr.UnitPriceIncrease else        
            --                       case when cpr.Margin = '+' and cpr.CostPriceIncrease is not null and vp.CostPrice is not null then vp.CostPrice + cpr.CostPriceIncrease else
            --                             case when cpr.CompareSourceID is not null and cpr.ComparePricePosition is not null          
            --                                         and cpr.BottomMargin is not null and vp.CostPrice is not null and ci.Price is not null            
            --                                         and ((((ci.Price - 0.01) / ((isnull(vp.taxrate,19) / 100) + 1)) - vp.CostPrice) / (vp.CostPrice/100)) > cpr.BottomMargin          
            --                                         then (ci.Price - 0.01) / ((isnull(vp.taxrate,19) / 100) + 1) else                  
            --                                   case when cpr.CompareSourceID is not null and cpr.MinComparePricePosition is not null and cpr.MaxComparePricePosition is not null           
            --                                               and cr.Price is not null and ((((cr.Price - 0.01) / ((isnull(vp.taxrate,19) / 100) + 1)) - vp.CostPrice) /(vp.CostPrice /100)) > cpr.BottomMargin  
            --                                               then (cr.Price - 0.01) / ((isnull(vp.taxrate,19) / 100) + 1) else 
            --                                         (Select top 1 pvp.price              
            --                                         from preferedVendorPrices pvp              
            --                                         where pvp.ProductID = c.ProductID            
            --                                         order by pvp.isPreferred desc)          
            --                                   end         
            --                             end        
            --                       end
            --                 end
            --            end
            --      end
            --end as PriceEx,  
			vp.CostPrice as PriceEx , -- TODO:  Quick and dirty
            vp.CostPrice,    
            vp.SpecialPrice,
            vp.TaxRate,    
            cpr.ContentPriceRuleIndex,    
            va.CustomItemNumber,    
            c.ConnectorID,    
            vp.ConcentratorStatusID,    
            vp.CommercialStatus,    
            vp.MinimumQuantity,    
            cpr.ContentPriceLabel,     
            cpr.FromDate,     
            cpr.ToDate,     
            cpr.BottomMargin,     
            cpr.ComparePricePosition,     
            cpr.MinComparePricePosition,     
            cpr.MaxComparePricePosition,    
            cpr.CompareSourceID,    
            va.ShortDescription,    
            va.VendorID,    
            p.BrandID,    
            p.VendorItemNumber  
      from content c  
      inner join Product p on c.ProductID = p.ProductID  
      inner join ConnectorPublicationRule cp on cp.ConnectorPublicationRuleID = c.ConnectorPublicationRuleID  
      inner join VendorAssortment va on c.ProductID = va.ProductID and va.VendorID = cp.VendorID  
      inner join VendorPrice vp on va.VendorAssortmentID = vp.VendorAssortmentID  
      left join ConnectorProductGroups pgm on pgm.ProductID = c.ProductID  
      left join ConnectorPriceRule cpr on  va.VendorID = cpr.VendorID    
            and ((cpr.BrandID is null or cpr.BrandID = p.BrandID) 
            and cpr.ProductID is null and (cpr.ProductGroupID is null or cpr.ProductGroupID = pgm.ProductGroupID))    
            or (cpr.ProductID = c.ProductID and cpr.BrandID is null and cpr.ProductGroupID is null)  
      left join CompetitorInfo ci on ci.ProductID = c.ProductID and ci.ProductCompareSourceID = cpr.CompareSourceID and ci.CompetitorRank = cpr.ComparePricePosition  
      left join CompetitorInfo cr on cr.ProductID = c.ProductID and cr.ProductCompareSourceID = cpr.CompareSourceID and cr.CurrentRank > cpr.MaxComparePricePosition   
      where vp.MinimumQuantity <=1    
            and (cpr.FromDate is null or cpr.FromDate > GETDATE())   
            and (cpr.ToDate is null or cpr.ToDate < GETDATE())  )  
select * from (  
      select    
            c.ProductID,  
            pr.PriceEx,  
            cast(pr.PriceEx * ((isnull(pr.taxrate,19) / 100) + 1) as decimal(18,4)) as priceInc,  
            pr.CostPrice,  
            pr.PriceEx - pr.CostPrice as Margin,  
            MIN(pcp.Price) as minPriceInc,   
            MAX(pcp.price) as maxPriceInc,   
            count(pcp.Competitor) as competitorcount,  
            ROW_NUMBER() OVER (PARTITION BY c.ProductID ORDER BY ContentPriceRuleIndex) AS RankNumber,  
            AVG(OP.OwnPrice) AS OwnPriceInc,    
            AVG(PCP.Price) AS AverageMarketPriceInc,  
            (   SELECT COUNT(*)+1 
                  FROM PriceSource PCP2              
                  WHERE PCP2.ProductID = PCP.ProductID AND PCP2.Price <  AVG(OP.OwnPrice)              
                  AND PCP2.Competitor <> 'MyCom'        ) AS CurrentRank,  
            pr.ConcentratorStatusID,
            pr.CommercialStatus,  
            pr.TaxRate,  
            pr.MinimumQuantity,  
            pr.ContentPriceLabel,     
            pr.FromDate,     
            pr.SpecialPrice,
            pr.ToDate,     
            pr.BottomMargin,     
            pr.ComparePricePosition,     
            pr.MinComparePricePosition,     
            pr.MaxComparePricePosition,  
            pr.CustomItemNumber,    
            pr.ShortDescription,    
            pr.VendorID,    
            MAX(cti.lastimport) as lastImport,    
            min(cti.stock) as CompetitorStock,    
            pr.BrandID,    pr.VendorItemNumber,    
            c.ConnectorID,    
            cti.Source as CompetitorSource, 
            pr.CompareSourceID as ProductCompareSourceID  
      from content c  
            LEFT join prices pr ON  (pr.productid = c.ProductID AND c.ConnectorID = pr.connectorID)  
            left join  PriceSource PCP on (pcp.ProductID = c.ProductID)  
            left JOIN OwnPrices OP ON (OP.ProductID = PCP.ProductID)  
            left join CompetitorInfo cti on c.ProductID = cti.ProductID  
      where cast(pr.PriceEx * ((isnull(pr.taxrate,19) / 100) + 1)as decimal(18,4)) > 0  
            and c.connectorid = @ConnectorID    
      group by pr.ProductID,
      pr.ContentPriceRuleIndex,
      pr.CostPrice, 
      pr.SpecialPrice,
      pcp.ProductID, 
      pr.PriceEx, 
      pr.TaxRate, 
      c.productid  ,
      pr.ConcentratorStatusID,
      pr.commercialstatus, 
      pr.TaxRate, 
      pr.MinimumQuantity,
      pr.ContentPriceLabel,     
      pr.FromDate,     
      pr.ToDate,     
      pr.BottomMargin,     
      pr.ComparePricePosition,     
      pr.MinComparePricePosition,     
      pr.MaxComparePricePosition,    
      pr.CustomItemNumber,    
      pr.ShortDescription,   
       pr.VendorID,    
       pr.BrandID,    
       pr.VendorItemNumber,    
       c.ConnectorID,    
       cti.Source, 
       pr.CompareSourceID  ) a where a.ranknumber = 1 