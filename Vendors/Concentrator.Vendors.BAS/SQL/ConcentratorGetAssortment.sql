USE [Laorta]
GO
/****** Object:  StoredProcedure [dbo].[ConcentratorGetAssortment_DEV]    Script Date: 05/04/2011 14:31:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Tim Zeeman
-- Create date: 10-9-2009
-- Description:	Published assortment for contentrator
-- CHANGE: T.Zeeman, JDEVandaag(). 08-10-2009
-- CHANGE: T.Zeeman, no details. 08-10-2009
-- CHANGE: T.Zeeman, no details. 09-10-2009
-- CHANGE: T.Zeeman, no details. 09-10-2009
-- CHANGE: T.Zeeman, Change AllowDC10Obystatus, show U items with stock. 19-10-2009
-- CHANGE: T.Zeeman, Sum reserved stock with quantityonhand  
-- CHANGE: JJA Bakker, reference to PRODDTA changed to dbo. 24-10-2009
-- CHANGE: JJA Bakker, changed #ass OR <> 'U' to OR <> 'O'. 24-10-2009 REMOVED
-- CHANGE: T.Zeeman, #ass removed wher A.stockstatus <> 'O', fixed AllowDC10bystatus 26-10-2009
-- CHANGE: T.Zeeman, change productgroup IMSRP4 to IMSRP5 
-- CHANGE: T.Zeeman, remove "AND A.BAPRP9 <> 'O'" in internetass for internetass = 0 24-12-2009
-- CHANGE: T.Zeeman, ADD LIQOWO for quantityonhand
-- CHANGE: T.Zeeman, ADD BSC stock in result as parameter 8-2-2010
-- CHANGE: T.Zeeman, ADD ShowShopInfo in results as parameter 26-2-2010
-- CHANGE: T.Zeeman, Single item Xtract, Multi Companie 11-3-2010
-- CHANGE: T.Zeeman, Get Ledgerclass JDE 17-3-2010
-- CHANGE: T.Zeeman, Set IMLITM TO IMITM for basitemnumber 29-3-2010
-- CHANGE: T.Zeeman, Include Retail stock parameter 6-4-2010
-- CHANGE: T.Zeeman, Show BSC stock <> DEV 7-4-2010
-- CHANGE: T.Zeeman, BSC Stock OEM and Veiling consilidated view, Costprices param, Show stock items only param 13-4-2010
-- CHANGE: T.Zeeman, Add litm to assortment
-- CHANGE: T.Zeeman, Show barcodes whitout 
-- CHANGE: T.Zeeman, BSC stock fix 19-5-2010
-- CHANGE: T.Zeeman, Generate assortment list without prices
-- CHANGE: T.Zeeman, FIX U items in O item result 28-5-2010
-- =============================================
ALTER PROCEDURE [dbo].[ConcentratorGetAssortment_DEV]
(
	@CustomerID int = null,
	@InternetAss int = 1,
	@AllowDC10O int = 0,
	@RetailBU NvarChar(12) = null,
	@AllowNonStock int = 0,
	@AllowDC10ObyStatus int = 0,
	@AssortmentBU Nvarchar(12) = null,
	@ShowBSCStock Nvarchar(12) = null,
	@ShowShopInfo Nvarchar(12) = null,
	@ManufacturerID NVarchar(50) = null, 
	@EAN NVarchar(50) = null, 
	@BasItemNumber int  = null,
	@BasLItemNumber int  = null,
	@DC nvarchar(12) = '        DC10',
	@ShowRetailStock Nvarchar(12) = null,
	@ShowCostPriceDC10 Nvarchar(12) = null,
	@ShowCostPriceBSC Nvarchar(12) = null,
	@ShowStockItemsOnly bit = null
)
AS
BEGIN
	
-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
Declare @BusinessUnit NvarChar(12)
DECLARE @JDEVandaag numeric(18,0)

set @JDEVandaag = cast(datepart(year, getdate()) - 1900 AS varchar(3) ) + 
 	right('000' + cast(datepart(y, getdate()) AS varchar(3) ), 3 )

IF(@AssortmentBU IS NOT NULL)
	BEGIN
		SET @BusinessUnit = RIGHT('            ' + @AssortmentBU,12)
	END
ELSE
	BEGIN
		Select @BusinessUnit = ABMCU from F0101 where ABAN8 = @CustomerID
	END

print @BusinessUnit;

WITH PromisedDeliveries AS (
SELECT PDITM, 
case when MIN(PDPDDJ) < 1 then @JDEVandaag + 21 else MIN(PDPDDJ) end AS PromisedDeliveryDate, SUM(PDUOPN) AS QuantityToReceive
FROM dbo.F4311
where   
--pdpddj >@JDEVandaag
pduopn > 0
and pdnxtr >= 280 and pdnxtr < 999
and (pddcto = 'OP' or pddcto = 'OT')
and pdmcu = @DC  
GROUP BY PDITM
)

, ReservationOrders AS (
	
SELECT SDITM, ISNULL(SUM(SDSOQS),0) AS QuantityReserved
FROM F4211
	LEFT JOIN F0150 ON (SDAN8 = MAPA8)
	WHERE SDDCTO = 'SH' AND SDNXTR < 999
	AND MAAN8 = @CustomerID
GROUP BY  SDITM

)
, StockInformation AS (
	SELECT LIITM, SUM(LIPQOH)-SUM(LIPCOM)-SUM(LIHCOM)-SUM(LIFCOM)-SUM(LIQOWO)  AS QuantityOnHand
	FROM F41021 
	WHERE LIMCU = @DC AND LTRIM(RTRIM(LILOCN)) <> 'ASSEMBLY' AND LILOTS = ''
	GROUP BY LIITM 
	HAVING SUM(LIPQOH)-SUM(LIPCOM)-SUM(LIHCOM)-SUM(LIFCOM)-SUM(LIQOWO) > 0 
	
)
, BarCodes AS (
	SELECT IVITM, MAX( IVCITM) AS IVCITM
	FROM F4104
	WHERE ivxrt IN ('EA', 'UP')
	GROUP BY IVITM
)


    select *
INTO #Ass
    from(
    
   select
P.IMAITM as VendorItemNumber,
Barcodes.IVCITM as Barcode,
P.IMDSC1 as Description1,
P.IMDSC2 as Description2,
P.IMSRP4 as ProductSubSubGroup,
p.IMSRP2 as ProductSubGroup,
P.IMSRP3 as Brand,
P.IMSRP5 as ProductGroup,
P.IMITM as ShortItemNumber,
LTRIM(RTRIM(P.IMLITM)) as LongItemNumber,
P.IMSRTX as ModelName,
ISNULL(AI$PDL/100,1) AS Length,
ISNULL(AI$PDW/100,1) AS Width,
ISNULL(AI$PDH/100,1) AS Height,
LTRIM(RTRIM(A.BAMCU)) as BusinessUnit,
LTRIM(RTRIM(StockBU.BASTKT)) as StockStatus,
LTRIM(RTRIM(A.BAPRP9)) as CommercialStatus,


ISNULL(StockInformation.QuantityOnHand,0)
+ ISNULL(ReservationOrders.QuantityReserved,0) AS QuantityOnHand,
 
 dbo.JDEToDate(PromisedDeliveries.PromisedDeliveryDate) AS PromisedDeliveryDate,
 PromisedDeliveries.QuantityToReceive,

	LTRIM(RTRIM(ISNULL(DRDL02,''))) AS IntraStatCode,
P.IMLNTY as LineType




FROM F574102A A
INNER JOIN F574102A StockBU ON (A.BAITM = StockBU.BAITM and StockBU.BAMCU = @DC  and 
(@AllowDC10ObyStatus = 1 or (((@AllowDC10O = 1) OR (@AllowDC10O = 0 AND StockBU.BASTKT <> 'O')))))



INNER JOIN F4101 P ON (A.BAITM = P.IMITM)
LEFT JOIN F0005 ON (LTRIM(RTRIM(IMSHCM)) = LTRIM(RTRIM(DRKY)) AND DRSY = '41' and ltrim(rtrim(DRRT))='E')
LEFT JOIN F554102 ON (AIITM = A.BAITM)
LEFT JOIN Barcodes ON (Barcodes.IVITM = IMITM)
LEFT JOIN StockInformation ON (StockInformation.LIITM = IMITM)
LEFT JOIN PromisedDeliveries ON (IMITM = PromisedDeliveries.PDITM)
LEFT JOIN ReservationOrders ON (IMITM = ReservationOrders.SDITM)
where 
A.BAMCU = @BusinessUnit
AND ((@AllowDC10O = 1) OR (@AllowDC10O = 0 AND A.BASTKT <> 'O'))
AND ((@InternetAss = 1 AND A.BA$ISTS = 1) OR (@AllowDC10O = 1 or (@InternetAss = 0 AND (A.BAPRP9 <> 'O'))))
AND ((@AllowNonStock = 0 AND P.IMLNTY = 'S') OR (@AllowNonStock = 1))
AND ((@AllowNonStock = 0 AND P.IMGLPT = 'STCK') OR (@AllowNonStock = 1))
AND (@AllowDC10ObyStatus = 0 OR (@AllowDC10ObyStatus = 1 AND A.BAPRP9 <> 'O'))
AND (@BasItemNumber is null or (@BasItemNumber is not null and A.BAITM = @BasItemNumber))
AND (@ManufacturerID is null or (@ManufacturerID is not null and P.IMAITM = @ManufacturerID))
AND (@EAN is null or (@EAN is not null and Barcodes.IVCITM = @EAN))
) A																											  
--where (@AllowDC10ObyStatus = 1 OR (@AllowDC10ObyStatus = 0 AND ((A.quantityonhand > 0 and A.StockStatus = 'U') OR A.StockStatus <> 'U'))) 

CREATE INDEX [prod] ON #Ass (ShortItemNumber ASC)
CREATE INDEX [prod2] ON #Ass (LongItemNumber ASC)

--------------------------------------------------
---------------------------------------------------
-- ShowShopInfo for shop assortment				 --
---------------------------------------------------
---------------------------------------------------

IF @ShowShopInfo is not null
BEGIN 
ALTER TABLE #Ass add LedgerClass nvarchar(50) null
ALTER TABLE #Ass add ExtendedCatalog bit null
ALTER TABLE #Ass add ProductDesk nvarchar(50) null

Update #Ass set 
	LedgerClass = IBGLPT,
	ExtendedCatalog = 1,
	ProductDesk = LTRIM(RTRIM(DRDL01))
from F4102 
inner join f0005 on (DRSY = '41' and DRRT = 'P3' and DRKY <> '' AND LTRIM(RTRIM(DRKY)) = IBPRP3)
inner join f4201 on (IBMCU = @BusinessUnit)
where iblitm = LongItemNumber
END

--------------------------------------------------
---------------------------------------------------
-- ShowBSCStock									 --
---------------------------------------------------
---------------------------------------------------

IF @ShowBSCStock is not null
BEGIN

select 
distinct
(CASE WHEN 
	CONVERT(int,
	SUM(S.LIPQOH)-SUM(S.LIPCOM)-SUM(S.LIHCOM)-SUM(S.LIFCOM)-SUM(S.LIQOWO)) 
   > 0 
THEN 
CONVERT(int,
	SUM(S.LIPQOH)-SUM(S.LIPCOM)-SUM(S.LIHCOM)-SUM(S.LIFCOM)-SUM(S.LIQOWO)) 
ELSE 0 END) as stockOEM, 
(CASE WHEN 
	CONVERT(int,
	SUM(S1.LIPQOH)-SUM(S1.LIPCOM)-SUM(S1.LIHCOM)-SUM(S1.LIFCOM)-SUM(S1.LIQOWO)) 
   > 0 
THEN 
CONVERT(int,
	SUM(S1.LIPQOH)-SUM(S1.LIPCOM)-SUM(S1.LIHCOM)-SUM(S1.LIFCOM)-SUM(S1.LIQOWO)) 
ELSE 0 END) as stockMYVEIL,
(CASE WHEN 
	CONVERT(int,
	SUM(DEMO.LIPQOH)-SUM(DEMO.LIPCOM)-SUM(DEMO.LIHCOM)-SUM(DEMO.LIFCOM)-SUM(DEMO.LIQOWO)) 
   > 0 
THEN 
CONVERT(int,
	SUM(DEMO.LIPQOH)-SUM(DEMO.LIPCOM)-SUM(DEMO.LIHCOM)-SUM(DEMO.LIFCOM)-SUM(DEMO.LIQOWO)) 
ELSE 0 END) as DEMO,
(CASE WHEN 
	CONVERT(int,
	SUM(DMGBOX.LIPQOH)-SUM(DMGBOX.LIPCOM)-SUM(DMGBOX.LIHCOM)-SUM(DMGBOX.LIFCOM)-SUM(DMGBOX.LIQOWO)) 
   > 0 
THEN 
CONVERT(int,
	SUM(DMGBOX.LIPQOH)-SUM(DMGBOX.LIPCOM)-SUM(DMGBOX.LIHCOM)-SUM(DMGBOX.LIFCOM)-SUM(DMGBOX.LIQOWO)) 
ELSE 0 END) as DMGBOX,
(CASE WHEN 
	CONVERT(int,
	SUM(DMGITEM.LIPQOH)-SUM(DMGITEM.LIPCOM)-SUM(DMGITEM.LIHCOM)-SUM(DMGITEM.LIFCOM)-SUM(DMGITEM.LIQOWO)) 
   > 0 
THEN 
CONVERT(int,
	SUM(DMGITEM.LIPQOH)-SUM(DMGITEM.LIPCOM)-SUM(DMGITEM.LIHCOM)-SUM(DMGITEM.LIFCOM)-SUM(DMGITEM.LIQOWO)) 
ELSE 0 END) as DMGITEM,
(CASE WHEN 
	CONVERT(int,
	SUM(INCOMPL.LIPQOH)-SUM(INCOMPL.LIPCOM)-SUM(INCOMPL.LIHCOM)-SUM(INCOMPL.LIFCOM)-SUM(INCOMPL.LIQOWO)) 
   > 0 
THEN 
CONVERT(int,
	SUM(INCOMPL.LIPQOH)-SUM(INCOMPL.LIPCOM)-SUM(INCOMPL.LIHCOM)-SUM(INCOMPL.LIFCOM)-SUM(INCOMPL.LIQOWO)) 
ELSE 0 END) as INCOMPL,
(CASE WHEN 
	CONVERT(int,
	SUM(SRETURN.LIPQOH)-SUM(SRETURN.LIPCOM)-SUM(SRETURN.LIHCOM)-SUM(SRETURN.LIFCOM)-SUM(SRETURN.LIQOWO)) 
   > 0 
THEN 
CONVERT(int,
	SUM(SRETURN.LIPQOH)-SUM(SRETURN.LIPCOM)-SUM(SRETURN.LIHCOM)-SUM(SRETURN.LIFCOM)-SUM(SRETURN.LIQOWO)) 
ELSE 0 END) as SRETURN,
(CASE WHEN 
	CONVERT(int,
	SUM(USED.LIPQOH)-SUM(USED.LIPCOM)-SUM(USED.LIHCOM)-SUM(USED.LIFCOM)-SUM(USED.LIQOWO)) 
   > 0 
THEN 
CONVERT(int,
	SUM(USED.LIPQOH)-SUM(USED.LIPCOM)-SUM(USED.LIHCOM)-SUM(USED.LIFCOM)-SUM(USED.LIQOWO)) 
ELSE 0 END) as USED,
A.ShortItemNumber as product
INTO #bscstock
FROM #Ass A
LEFT JOIN F41021 S ON (A.LongItemNumber = CONVERT(nvarchar(50),S.LIITM) AND LTRIM(S.LIMCU) = 'BSC' 
	and LTRIM(RTRIM(S.LILOCN)) = 'OEM' 
	and s.LILOTS = '')
LEFT JOIN F41021 S1 ON (A.LongItemNumber = CONVERT(nvarchar(50),S1.LIITM)AND LTRIM(S1.LIMCU) = 'BSC' 
	and LTRIM(RTRIM(S1.LILOCN)) = 'MYVEIL' 
	and S1.LILOTS = '')	
LEFT JOIN F41021 DEMO ON (A.LongItemNumber = CONVERT(nvarchar(50),DEMO.LIITM)AND LTRIM(DEMO.LIMCU) = 'BSC' 
	and LTRIM(RTRIM(DEMO.LILOCN)) = 'DEMO' 
	and DEMO.LILOTS = '')	
LEFT JOIN F41021 DMGBOX ON (A.LongItemNumber = CONVERT(nvarchar(50),DMGBOX.LIITM)AND LTRIM(DMGBOX.LIMCU) = 'BSC' 
	and LTRIM(RTRIM(DMGBOX.LILOCN)) = 'DMGBOX'
	and DMGBOX.LILOTS = '')	
LEFT JOIN F41021 DMGITEM ON (A.LongItemNumber = CONVERT(nvarchar(50),DMGITEM.LIITM)AND LTRIM(DMGITEM.LIMCU) = 'BSC' 
	and LTRIM(RTRIM(DMGITEM.LILOCN)) = 'DMGITEM'
	and DMGITEM.LILOTS = '')	
LEFT JOIN F41021 INCOMPL ON (A.LongItemNumber = CONVERT(nvarchar(50),INCOMPL.LIITM)AND LTRIM(INCOMPL.LIMCU) = 'BSC' 
	and LTRIM(RTRIM(INCOMPL.LILOCN)) = 'INCOMPL'
	and INCOMPL.LILOTS = '')	
LEFT JOIN F41021 SRETURN ON (A.LongItemNumber = CONVERT(nvarchar(50),SRETURN.LIITM)AND LTRIM(SRETURN.LIMCU) = 'BSC' 
	and LTRIM(RTRIM(SRETURN.LILOCN)) = 'RETURN'
	and SRETURN.LILOTS = '')	
LEFT JOIN F41021 USED ON (A.LongItemNumber = CONVERT(nvarchar(50),USED.LIITM)AND LTRIM(USED.LIMCU) = 'BSC' 
	and LTRIM(RTRIM(USED.LILOCN)) = 'USED'
	and USED.LILOTS = '')	
Group By A.ShortItemNumber 

CREATE INDEX [prod] ON #bscstock (product ASC)

IF @ShowBSCStock = 'ALL'
BEGIN
ALTER TABLE #Ass add QuantityBSC int null

update #Ass 
set QuantityBSC = stockMYVEIL + stockOEM + DEMO + DMGBOX + DMGITEM + INCOMPL + SRETURN + USED

from #bscstock
where product = ShortItemNumber
AND ((@ShowStockItemsOnly is null) OR (@ShowStockItemsOnly is not null and (stockMYVEIL + stockOEM + DEMO + DMGBOX + DMGITEM + INCOMPL + SRETURN + USED) > 0))

IF @ShowStockItemsOnly is not null
BEGIN 
DELETE FROM #Ass where QuantityOnHand < 1 and QuantityBSC is null
END

END
ELSE
BEGIN
ALTER TABLE #Ass add QuantityBSCOEM int null
ALTER TABLE #Ass add QuantityBSC int null
ALTER TABLE #Ass add QuantityBSCDEMO int null
ALTER TABLE #Ass add QuantityBSCDMGBOX int null
ALTER TABLE #Ass add QuantityBSCDMGITEM int null
ALTER TABLE #Ass add QuantityBSCINCOMPL int null
ALTER TABLE #Ass add QuantityBSCRETURN int null
ALTER TABLE #Ass add QuantityBSCUSED int null

update #Ass 
set QuantityBSC = stockMYVEIL, 
QuantityBSCOEM = stockOEM,
QuantityBSCDEMO = DEMO,
QuantityBSCDMGBOX = DMGBOX,
QuantityBSCDMGITEM = DMGITEM,
QuantityBSCINCOMPL = INCOMPL,
QuantityBSCRETURN =  SRETURN,
QuantityBSCUSED =  USED

from #bscstock
where product = ShortItemNumber
AND ((@ShowStockItemsOnly is null) OR (@ShowStockItemsOnly is not null and (stockMYVEIL + stockOEM + DEMO + DMGBOX + DMGITEM + INCOMPL + SRETURN + USED) > 0))

IF @ShowStockItemsOnly is not null
BEGIN 
DELETE FROM #Ass where QuantityOnHand < 1 
and QuantityBSC is null
and QuantityBSCOEM is null
and QuantityBSCDEMO is null
and QuantityBSCDMGBOX is null
and QuantityBSCDMGITEM is null
and QuantityBSCINCOMPL is null
and QuantityBSCRETURN is null
and QuantityBSCUSED is null
END

END

DROP TABLE #bscstock

END

--------------------------------------------------
---------------------------------------------------
-- ShowCostPrice DC10							 --
---------------------------------------------------
---------------------------------------------------

IF @ShowCostPriceDC10 is not null
BEGIN
ALTER TABLE #Ass add CostPriceDC10 Decimal(18,4) null

select 
distinct
cast(councs / 10000 as decimal(18,4)) as CostPrice, a1.ShortItemNumber as product
INTO #dc10costprice
FROM #Ass a1
INNER JOIN F4105 a on (a1.LongItemNumber = a.COLITM AND ltrim(A.COMCU) = 'DC10')
where 
A.COLEDG = @ShowCostPriceDC10
Group By a1.ShortItemNumber,councs

CREATE INDEX [prod] ON #dc10costprice (product ASC)

update #Ass set CostPriceDC10 = 
CostPrice from #dc10costprice
where product = ShortItemNumber

DROP TABLE #dc10costprice

END

--------------------------------------------------
---------------------------------------------------
-- ShowCostPrice BSC							 --
---------------------------------------------------
---------------------------------------------------

IF @ShowCostPriceBSC is not null
BEGIN
ALTER TABLE #Ass add CostPriceBSC Decimal(18,4) null

select 
distinct
cast(councs / 10000 as decimal(18,4)) as CostPrice, a1.ShortItemNumber as product
INTO #bsccostprice
FROM #Ass a1
INNER JOIN F4105 a on (a1.LongItemNumber = a.COLITM AND ltrim(A.COMCU) = 'BSC')
where 
A.COLEDG = @ShowCostPriceBSC
Group By a1.ShortItemNumber,councs 

CREATE INDEX [prod] ON #bsccostprice (product ASC)

update #Ass set CostPriceBSC = 
CostPrice from #bsccostprice
where product = ShortItemNumber

DROP TABLE #bsccostprice

END

--------------------------------------------------
---------------------------------------------------
-- Generate Prices								--
---------------------------------------------------
---------------------------------------------------

DECLARE @wBusinessUnit varchar(12) 
IF @RetailBU IS NOT NULL
BEGIN
	SET @RetailBU = RIGHT('            ' + @RetailBU,12)
	SET @wBusinessUnit = @RetailBU
END
ELSE 
	SET @wBusinessUnit = @BusinessUnit

SET NOCOUNT OFF
SET FMTONLY OFF

DECLARE @Company varchar(5) 
DECLARE @OrderDetailBusinessUnit varchar(12) 


DECLARE @ShipToCustomerID float
DECLARE @SoldToCustomerID float
DECLARE @ParentCustomerID float

DECLARE @Schedule char(8)
DECLARE @SoldToCustomerKey varchar(280)
DECLARE @ShipToCustomerKey varchar(280)
DECLARE @TaxArea varchar(10)

-- ------------------------------------------------------------------------------------------- 
-- Variable Initialization
-- ------------------------------------------------------------------------------------------- 
IF @CustomerID is not null
BEGIN

SET @SoldToCustomerID = @CustomerID

SELECT @ParentCustomerID = MAPA8 FROM F0150 WHERE MAAN8 = @CustomerID AND MAOSTP=''
IF @ParentCustomerID IS NOT NULL AND @ParentCustomerID > 0 
		SET @SoldToCustomerID = @ParentCustomerID

PRINT @SoldToCustomerID
PRINT @ShipToCustomerID

SET @wBusinessUnit = RIGHT('            ' + @wBusinessUnit, 12)

IF @Company IS NULL OR @Company = ''
	SELECT @Company = LTRIM(RTRIM(MCCO)) FROM dbo.F0006 WHERE MCMCU = @wBusinessUnit
PRINT @wBusinessUnit
PRINT @Company

SELECT TOP 1  @Schedule = AIASN, @TaxArea = LTRIM(RTRIM(AITXA1)), @SoldToCustomerKey = 'AC01|' + AIAC01 + '#'
					 + 'AC02|' + AIAC02 + '#'
					 + 'AC03|' + AIAC03 + '#'
					 + 'AC04|' + AIAC04 + '#'
					 + 'AC05|' + AIAC05 + '#'
					 + 'AC06|' + AIAC06 + '#'
					 + 'AC07|' + AIAC07 + '#'
					 + 'AC08|' + AIAC08 + '#'
					 + 'AC09|' + AIAC09 + '#'
					 + 'AC10|' + AIAC10 + '#'
					 + 'AC11|' + AIAC11 + '#'
					 + 'AC12|' + AIAC12 + '#'
					 + 'AC13|' + AIAC13 + '#'
					 + 'AC14|' + AIAC14 + '#'
					 + 'AC15|' + AIAC15 + '#'
					 + 'AC16|' + AIAC16 + '#'
					 + 'AC17|' + AIAC17 + '#'
					 + 'AC18|' + AIAC18 + '#'
					 + 'AC19|' + AIAC19 + '#'
					 + 'AC20|' + AIAC20 + '#'
					 + 'AC21|' + AIAC21 + '#'
					 + 'AC22|' + AIAC22 + '#'
					 + 'AC23|' + AIAC23 + '#'
					 + 'AC24|' + AIAC24 + '#'
					 + 'AC25|' + AIAC25 + '#'
					 + 'AC26|' + AIAC26 + '#'
					 + 'AC27|' + AIAC27 + '#'
					 + 'AC28|' + AIAC28 + '#'
					 + 'AC29|' + AIAC29 + '#'
					 + 'AC30|' + AIAC30 + '#'		
	FROM F03012 WHERE AIAN8 = @SoldToCustomerID AND AICO IN ('00000', @Company) ORDER BY AICO DESC



SELECT TOP 1  @Schedule = AIASN, @ShipToCustomerKey = 'AC01|' + AIAC01 + '#'
					 + 'AC02|' + AIAC02 + '#' 
					 + 'AC03|' + AIAC03 + '#'
					 + 'AC04|' + AIAC04 + '#'
					 + 'AC05|' + AIAC05 + '#'
					 + 'AC06|' + AIAC06 + '#'
					 + 'AC07|' + AIAC07 + '#'
					 + 'AC08|' + AIAC08 + '#'
					 + 'AC09|' + AIAC09 + '#'
					 + 'AC10|' + AIAC10 + '#'
					 + 'AC11|' + AIAC11 + '#'
					 + 'AC12|' + AIAC12 + '#'
					 + 'AC13|' + AIAC13 + '#'
					 + 'AC14|' + AIAC14 + '#'
					 + 'AC15|' + AIAC15 + '#'
					 + 'AC16|' + AIAC16 + '#'
					 + 'AC17|' + AIAC17 + '#'
					 + 'AC18|' + AIAC18 + '#'
					 + 'AC19|' + AIAC19 + '#'
					 + 'AC20|' + AIAC20 + '#'
					 + 'AC21|' + AIAC21 + '#'
					 + 'AC22|' + AIAC22 + '#'
					 + 'AC23|' + AIAC23 + '#'
					 + 'AC24|' + AIAC24 + '#'
					 + 'AC25|' + AIAC25 + '#'
					 + 'AC26|' + AIAC26 + '#'
					 + 'AC27|' + AIAC27 + '#'
					 + 'AC28|' + AIAC28 + '#'
					 + 'AC29|' + AIAC29 + '#'
					 + 'AC30|' + AIAC30 + '#'		
	FROM F03012 WHERE AIAN8 = @ShipToCustomerID AND AICO IN ('00000', @Company) ORDER BY AICO DESC


PRINT @Schedule
PRINT @SoldToCustomerKey
PRINT @ShipToCustomerKey

DECLARE @SNOSEQ float
DECLARE @ProductID float
DECLARE @ADBSCD int
DECLARE @ATABAS char(1)
DECLARE @FactorValue float
DECLARE @BasePrice float
DECLARE @AdjustmentPrice float
DECLARE @ADMNQ float

--
---- --------------------------------------------------------------------------------------------------------
---- Retrieve Prices
---- --------------------------------------------------------------------------------------------------------
--	
--CREATE TABLE #PriceList
--(
--	[ProductID] [float] NOT NULL,
--	[SNOSEQ] [float] NOT NULL,
--	[ADAST] [char](8) NOT NULL,
--	[PrefColumn] [nvarchar](10) NOT NULL,
--	[PrefValue] [float] NULL,
--	[ADBSCD] [char](1) NULL,
--	[ATABAS] [char](1) NULL,
--	[FactorValue] [float] NULL,
--	[BP] [float] NULL,
--	[ADJ] [float] NULL,
--	ADFGY [char](1) NULL,
--	ADATID float NULL,
--	ADMNQ float NOT NULL,
--	[DRDL01] [char](30) NULL,
--	[LinePrice] [float] NULL,
--	[RN] int IDENTITY(1,1)
--)

DECLARE @TableName varchar(44)
SET @TableName = QUOTENAME('PL-'+ CONVERT(varchar(40),NEWID()))
PRINT @TableName

DECLARE @Query varchar(MAX)
SET @Query = ''



--INSERT INTO #PriceList
SELECT *,  RN = IDENTITY(int, 1, 1)
INTO #PriceList
FROM (
	SELECT ProductID, SNOSEQ,ADAST, PrefColumn,PrefValue, ADBSCD,ATABAS, ADFVTR AS FactorValue ,
	BasePRice AS BP, AdjustmentPrice AS ADJ, ADFGY,ADATID,ADMNQ , DRDL01 , CONVERT(float ,0) AS LinePrice
	, MIN(PrefValue) OVER (PARTITION BY ProductID, ADAST) AS MinPrefValue, ATACNT
	--, MIN(PrefValue) OVER (PARTITION BY ProductID, ADAST) AS MinPrefValue

	FROM xtract_IntermediateResults X
	inner join #ass assor on (productid = ShortItemNumber)
	WHERE SNASN = @Schedule AND X.BusinessUnit = @wBusinessUnit
	AND ADLEDG <> '07'
	AND
	CASE 

	WHEN PrefColumn = 'HYHY01' AND @ShipToCustomerID = ADAN8 AND ADITM = ProductID THEN 1 
	WHEN PrefColumn = 'HYHY02' AND @ShipToCustomerID = ADAN8 AND ProductGroupFilterCount > 0 THEN 1
	WHEN PrefColumn = 'HYHY03' AND @ShipToCustomerID = ADAN8 THEN 1

	WHEN PrefColumn = 'HYHY04' AND CustomerGroupFilterCount > 0 AND ISNULL(ADAN8,0) = 0 AND ADITM = ProductID THEN 1
	WHEN PrefColumn = 'HYHY05' AND CustomerGroupFilterCount > 0 AND ISNULL(ADAN8,0) = 0 AND ProductGroupFilterCount > 0 THEN 1
	WHEN PrefColumn = 'HYHY06' AND CustomerGroupFilterCount > 0 AND ISNULL(ADAN8,0) = 0 THEN 1

	WHEN PrefColumn = 'HYHY07' AND @SoldToCustomerID = ADAN8 AND ADITM = ProductID THEN 1 
	WHEN PrefColumn = 'HYHY08' AND @SoldToCustomerID = ADAN8 AND ProductGroupFilterCount > 0 THEN 1
	WHEN PrefColumn = 'HYHY09' AND @SoldToCustomerID = ADAN8  AND (ADITM IS NULL OR ADITM = 0 )  THEN 1

	WHEN PrefColumn = 'HYHY10' AND CustomerGroupFilterCount > 0 AND ADITM = ProductID THEN 1
	WHEN PrefColumn = 'HYHY11' AND CustomerGroupFilterCount > 0 AND ProductGroupFilterCount > 0 THEN 1
	WHEN PrefColumn = 'HYHY12' AND CustomerGroupFilterCount > 0 AND ProductGroupFilterCount = 0 AND (ADITM IS NULL OR ADITM = 0 )  THEN 1

	WHEN PrefColumn = 'HYHY13' THEN 0
	WHEN PrefColumn = 'HYHY14' THEN 0
	WHEN PrefColumn = 'HYHY15' THEN 0

	WHEN PrefColumn = 'HYHY16' THEN 0
	WHEN PrefColumn = 'HYHY17' THEN 0
	WHEN PrefColumn = 'HYHY18' THEN 0

	WHEN PrefColumn = 'HYHY19' AND CustomerGroupFilterCount = 0 AND ISNULL(ADAN8,0) = 0  AND ADITM = ProductID THEN 1
	WHEN PrefColumn = 'HYHY20' AND CustomerGroupFilterCount = 0 AND ISNULL(ADAN8,0) = 0  AND ProductGroupFilterCount > 0 THEN 1
	WHEN PrefColumn = 'HYHY21' AND CustomerGroupFilterCount = 0 AND ISNULL(ADAN8,0) = 0  AND ProductGroupFilterCount = 0 AND (ADITM IS NULL OR ADITM = 0 )THEN 1

	ELSE 0 

	END = 1

	AND
	(
		CustomerGroupFilterCount = 0
		OR
		(  CustomerGroupFilterCount =
			CASE WHEN CHARINDEX(CustomerGroupHash1, @SoldToCustomerKey) > 0 THEN 1 ELSE 0 END
			+ CASE WHEN CHARINDEX(CustomerGroupHash2, @SoldToCustomerKey) > 0 THEN 1 ELSE 0 END
			+ CASE WHEN CHARINDEX(CustomerGroupHash3, @SoldToCustomerKey) > 0 THEN 1 ELSE 0 END
			+ CASE WHEN CHARINDEX(CustomerGroupHash4, @SoldToCustomerKey) > 0 THEN 1 ELSE 0 END
		)
	)

	
) A 
WHERE A.PrefValue = MinPrefValue
ORDER BY ProductID, ADMNQ, SNOSEQ


CREATE INDEX [prod] ON #PriceList (ProductID ASC)

END

--------------------------------------------------
---------------------------------------------------
-- First resultset assortment with price		 --
---------------------------------------------------
---------------------------------------------------
IF @CustomerID is not null
BEGIN
SELECT distinct ProductID,ass.*
, MinimumQuantity, CONVERT(decimal(18,4),ISNULL(Price,0)) AS UnitPrice, ISNULL(tatxr1,0)/1000 AS TaxRate
FROM (
	SELECT A.RN, CONVERT(int,A.ProductID) AS ProductID, CONVERT(int,A.ADMNQ) AS MinimumQuantity, MAX(A.RN) OVER (PARTITION BY A.ProductID, A.ADMNQ) AS MaxRow,
			     dbo.xtract_CalculatePrice(A.ADBSCD, A.FactorValue, A.BP, A.ADJ, 
					 dbo.xtract_CalculatePrice(B.ADBSCD, B.FactorValue, B.BP, B.ADJ, 
						dbo.xtract_CalculatePrice(C.ADBSCD, C.FactorValue, C.BP, C.ADJ, 
						    dbo.xtract_CalculatePrice(D.ADBSCD, D.FactorValue, D.BP, D.ADJ, null,
								 BPUPRC/10000 , D.ATABAS)
								 , BPUPRC/10000, C.ATABAS),
									BPUPRC/10000, B.ATABAS),
									 BPUPRC/10000, A.ATABAS) AS Price
				   FROM #PriceList A
					 LEFT JOIN F4106 ON (A.ProductID = BPITM AND BPMCU ='            ' )--default BU)
					   LEFT JOIN #PriceList B ON (A.ProductID = B.ProductID AND A.ADMNQ >= B.ADMNQ AND A.RN = B.RN + 1)
						   LEFT JOIN #PriceList C ON (B.ProductID = C.ProductID AND B.ADMNQ >= C.ADMNQ AND B.RN = C.RN + 1)
							   LEFT JOIN #PriceList D ON (C.ProductID = D.ProductID AND C.ADMNQ >= D.ADMNQ AND C.RN = D.RN + 1)

		WHERE A.FactorValue <> 0
		AND A.ATACNT NOT IN (4,5,6) -- exclude accruels
	) A
	INNER JOIN #ass ass on(ShortItemNumber = ProductID)
	INNER JOIN F4102 ON (IBITM = ProductID AND IBMCU = @wBusinessUnit)
	
	LEFT JOIN F4008 ON (LTRIM(RTRIM(tatxa1))=ISNULL(@TaxArea,'NL') and taitm=
	case when isnumeric(ibtax1)=1 then ibtax1 when ibtax1='Y' then 0 else null end)
	WHERE RN = MaxRow



END
ELSE
BEGIN
SELECT ShortItemNumber,ass.*
,0 as MinimumQuantity, CONVERT(decimal(18,4),0) AS UnitPrice, CONVERT(numeric(13,2),0) AS TaxRate
FROM #ass ass 
	INNER JOIN F4102 ON (IBITM = ShortItemNumber AND IBMCU = @wBusinessUnit)
	LEFT JOIN F4008 ON (LTRIM(RTRIM(tatxa1))=ISNULL(@TaxArea,'NL') and taitm=
	case when isnumeric(ibtax1)=1 then ibtax1 when ibtax1='Y' then 0 else null end)
END

--------------------------------------------------
---------------------------------------------------
-- Second resultset freegoods					 --
---------------------------------------------------
---------------------------------------------------
IF @CustomerID is not null
BEGIN
select CONVERT(int,ProductID) AS ProductID 
	, CONVERT(int, ADMNQ) AS MinimumQuantity, 
  CONVERT(int,FGFQTY)/100 AS OverOrderedQuantity, 
	CONVERT(int,FGITMR) AS FreeGoodProductID, 
									CONVERT(int,FGUORG) AS FreeGoodQuantity, 
									CONVERT(decimal(18,4), FGRPRI/100) AS FreeGoodUnitPrice
, ISNULL(tatxr1,0)/1000 AS TaxRate
					,LTRIM(RTRIM(IMDSC1)) AS FreeGoodDescription

	FROM #PriceList 
		INNER JOIN F4073 ON (FGATID = ADATID AND FGAST = ADAST)
		INNER JOIN F4101 ON (FGITMR = IMITM)
		INNER JOIN F4102 ON (IBITM = IMITM AND IBMCU = @wBusinessUnit)
		LEFT JOIN F4008 ON (LTRIM(RTRIM(tatxa1))=ISNULL(@TaxArea,'NL') and taitm=
		case when isnumeric(ibtax1)=1 then ibtax1 when ibtax1='Y' then 0 else null end)
	WHERE ADFGY ='Y'
END


--------------------------------------------------
---------------------------------------------------
-- Accruels --
---------------------------------------------------
---------------------------------------------------
IF @CustomerID is not null
BEGIN
--select CONVERT(int,ProductID) AS ProductID 
--	, CONVERT(int, ADMNQ) AS MinimumQuantity, 
--  CONVERT(int,FGFQTY)/100 AS OverOrderedQuantity, 
--	CONVERT(int,FGITMR) AS FreeGoodProductID, 
--									CONVERT(int,FGUORG) AS FreeGoodQuantity, 
--									CONVERT(decimal(18,4), FGRPRI/100) AS FreeGoodUnitPrice
--, ISNULL(tatxr1,0)/1000 AS TaxRate
--					,LTRIM(RTRIM(IMDSC1)) AS FreeGoodDescription

	SELECT  CONVERT(int,ProductID) AS ProductID,
	CONVERT(int, ADMNQ) AS MinimumQuantity, 
	LTRIM(RTRIM(ADAST)) AS AccruelCode,
	LTRIM(RTRIM(DRDL01)) AS AccruelDescription,
	CONVERT(decimal(18,4), FactorValue/10000) AS UnitPrice
	FROM #PriceList 
		--INNER JOIN F4073 ON (FGATID = ADATID AND FGAST = ADAST)
		--INNER JOIN F4101 ON (FGITMR = IMITM)
		INNER JOIN F4102 ON (IBITM = ProductID AND IBMCU = @wBusinessUnit)
		LEFT JOIN F4008 ON (LTRIM(RTRIM(tatxa1))=ISNULL(@TaxArea,'NL') and taitm=
		case when isnumeric(ibtax1)=1 then ibtax1 when ibtax1='Y' then 0 else null end)
	WHERE ATACNT IN (4,5,6)
END
--------------------------------------------------
---------------------------------------------------
-- Third resultset RetailStock					 --
---------------------------------------------------
---------------------------------------------------

IF @ShowRetailStock is not null
BEGIN 
select 
	s.productID As ProductID,
	s.QuantityOnHand as InStock,
	s.relationid,
	s.lastModificationTime as LastUpdate,
	VendorItemNumber,
	Brand
FROM #ass
INNER JOIN VMW3KH06.MyComWEBV2_Prod.dbo.shopstock s on ShortItemNumber = s.ProductID
END

--------------------------------------------------
---------------------------------------------------
-- END Drop temp tables							 --
---------------------------------------------------
---------------------------------------------------
IF @CustomerID is not null
BEGIN
DROP TABLE #PriceList
END

DROP TABLE #Ass

END