/****** Object:  StoredProcedure [dbo].[PortalGetWebOrders]    Script Date: 03/07/2011 12:10:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PortalGetWebOrders]
	(	
@CustomerID int,
@MaxRows int = null,
@ItemNumber nvarchar(50) = null,
@SearchNumber int = null,
@StartDate datetime = null,
@EndDate datetime = null
)
AS

BEGIN
DECLARE @FromDate int
DECLARE @ToDate int

IF @StartDate IS NULL 
	SET @StartDate = GETDATE() - 30
IF @EndDate IS NULL
	SET @EndDate = GETDATE()

SET @FromDate = dbo.DateToJDE(@StartDate)
SET @ToDate = dbo.DateToJDE(@EndDate)


DECLARE @SoldToNumber int
DECLARE @ShipToNumber int

SET @ShipToNumber = @CustomerID

SET @SoldToNumber = (SELECT MAPA8 FROM F0150 WHERE MAAN8 = @CustomerID AND LTRIM(RTRIM(MAOSTP)) ='' AND MADSS7 = 0)
IF @SoldToNumber IS NULL
	SET @SoldToNumber = @CustomerID

IF @ShiptoNumber = @SoldToNumber
	SET @ShiptoNumber = NULL

PRINT 'Ship - ' +  CONVERT(varchar, @ShipToNumber)
PRINT 'SoldTo - ' + CONVERT(varchar,@SoldToNumber)


DECLARE @CompanyCode varchar(12)
SELECT @CompanyCode = LTRIM(RTRIM(MCCO))
	FROM F0101 INNER JOIN F0006 ON (ABMCU = MCMCU)
	WHERE ABAN8 = COALESCE( @SoldToNumber,@CustomerID)

PRINT @CompanyCode
IF @CompanyCode IS NULL
	RETURN

if (@MaxRows > 0) 
	BEGIN
	SET ROWCOUNT @MaxRows
	END
else
BEGIN
	SET ROWCOUNT 0
	END


BEGIN
SELECT	
                     SDKCOO AS OrderCompany, SDDOCO AS OrderNumber, SDDCTO AS OrderType, 
                      SDMCU AS BusinessUnit, SDAN8 AS SoldToNumber, SDSHAN AS ShipToNumber, 
                      SDPA8 AS ParentNumber, dbo.JDEToDate(SDTRDJ) AS OrderDate, SDVR01 AS CustomerReference, 
                      SDVR02 AS WebOrderNumber, SDPTC AS PaymentTermsCode, 
                       SDTXA1 AS TaxArea, SDEXR1 AS TaxExplanationCode, SDCARS AS Carrier, 
                      SDROUT AS RouteCode, SDCRCD AS CurrencyCodeFrom, 
                      SDCRR AS CurrencyConverRate, --SDORBY AS OrderedBy, SDTKBY AS OrderTakenBy, 
                      SDDCT AS InvoiceType, SDDOC AS InvoiceNumber, SDKCO AS InvoiceCompany, 
                      SoldTo.ALCTR AS SoldToCountry, SoldTo.ALCTY1 AS SoldToCity, SoldTo.ALADDZ AS SoldToPostalcode, SoldTo.ALADD3 AS SoldToTAV, 
                      SoldTo.ALADD2 AS SoldToExtra, SoldTo.ALADD1 AS SoldToStreet, ShipTo.ALCTR AS ShipToCountry, ShipTo.ALCTY1 AS ShipToCity, 
                      ShipTo.ALADDZ AS ShipToPostalcode, ShipTo.ALADD3 AS ShipToTAV, ShipTo.ALADD2 AS ShipToExtra, ShipTo.ALADD1 AS ShipToStreet, 
                      ParentNumber.ALCTR AS ParentNumberCountry, ParentNumber.ALCTY1 AS ParentNumberCity, ParentNumber.ALADDZ AS ParentNumberPostalcode, 
                      ParentNumber.ALADD3 AS ParentNumberTAV, ParentNumber.ALADD2 AS ParentNumberExtra, ParentNumber.ALADD1 AS ParentNumberStreet, 
                      DropAddress.OAMLNM AS DropAddressName, DropAddress.OAADD1 AS DropAddressStreet, DropAddress.OAADD2 AS DropAddressExtra, 
                      DropAddress.OAADD3 AS DropAddressTAV, DropAddress.OAADDZ AS DropAddressPostalcode, DropAddress.OACTY1 AS DropAddressCity, 
                      DropAddress.OACTR AS DropAddressCountry,
		soldtoname.abalph as soldtoname ,shiptoname.abalph as shiptoname 
		--RPCRRM,RPACR,RPAG,RPFAP,RPAAP,
		--CASE RPCRRM WHEN 'F' THEN (SUM(RPACR)/100) ELSE (SUM(RPAG)/100) END AS AmountForeignTotalC,
		--CASE RPCRRM WHEN 'F' THEN (SUM(RPFAP)/100) ELSE (SUM(RPAAP)/100) END AS AmountForeignOpen,
		--RPDCT
--INTO #invoiceHeaders
FROM 
F4211	
INNER JOIN
                          (SELECT     ALAN8, ALADD1, ALADD2, ALADD3, ALADD4, ALADDZ, ALCTY1, ALCTR
                            FROM          F0116) AS SoldTo ON SDAN8 = SoldTo.ALAN8 INNER JOIN
                          (SELECT     ALAN8, ALADD1, ALADD2, ALADD3, ALADD4, ALADDZ, ALCTY1, ALCTR
                            FROM          F0116 AS F0116_1) AS ShipTo ON SDSHAN = ShipTo.ALAN8 INNER JOIN
                          (SELECT     ALAN8, ALADD1, ALADD2, ALADD3, ALADD4, ALADDZ, ALCTY1, ALCTR
                            FROM          F0116 AS F0116_2) AS ParentNumber ON SDPA8 = ParentNumber.ALAN8  LEFT OUTER JOIN
                          (SELECT     OADOCO, OADCTO, OAKCOO, OAMLNM, OAADD1, OAADD2, OAADD3, OAADD4, OAADDZ, OACTY1, OACTR
                            FROM          F4006) AS DropAddress ON SDDOCO = DropAddress.OADOCO AND 
                      SDDCTO = DropAddress.OADCTO AND SDKCOO = DropAddress.OAKCOO 
                        inner join f0101 soldtoname on soldtoname.aban8 = sdan8
                      inner join f0101 shiptoname on shiptoname.aban8 = sdshan                        
where SDAN8 = @SoldToNumber
		and (@ShipToNumber is null or (@ShipToNumber is not null and SDSHAN = @ShipToNumber))
		and (@itemNumber is null or (@ItemNumber is not null and SDLITM = @itemNumber))
		and (@SearchNumber is null or (@SearchNumber is not null and (sddoco = @SearchNumber)))
GROUP BY SDDOCO,
 SDKCOO, SDDOCO, SDDCTO, SDMCU , SDAN8, SDSHAN, 
                      SDPA8 , SDTRDJ , SDVR01 , 
                      SDVR02 , SDPTC ,
                       SDTXA1 , SDEXR1, SDCARS , 
                      SDROUT , SDCRCD, 
                      SDCRR, SDDCT, SDDOC, SDKCO, 
                      SoldTo.ALCTR , SoldTo.ALCTY1 , SoldTo.ALADDZ , SoldTo.ALADD3 , 
                      SoldTo.ALADD2, SoldTo.ALADD1 , ShipTo.ALCTR , ShipTo.ALCTY1 , 
                      ShipTo.ALADDZ , ShipTo.ALADD3, ShipTo.ALADD2 , ShipTo.ALADD1, 
                      ParentNumber.ALCTR , ParentNumber.ALCTY1 , ParentNumber.ALADDZ , 
                      ParentNumber.ALADD3 , ParentNumber.ALADD2 , ParentNumber.ALADD1 , 
                      DropAddress.OAMLNM , DropAddress.OAADD1 , DropAddress.OAADD2 , 
                      DropAddress.OAADD3 , DropAddress.OAADDZ , DropAddress.OACTY1 , 
                      DropAddress.OACTR , soldtoname.abalph,shiptoname.abalph
end 
END
GO


