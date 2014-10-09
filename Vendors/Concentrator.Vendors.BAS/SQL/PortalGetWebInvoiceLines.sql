/****** Object:  StoredProcedure [dbo].[PortalGetWebInvoiceLines]    Script Date: 03/07/2011 12:09:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[PortalGetWebInvoiceLines] 
	(
@InvoiceID int,
@CustomerID int,
@InvoiceType char(2)		
	)
AS
	SET NOCOUNT ON

BEGIN
DECLARE @CompanyCode varchar(12)
SELECT @CompanyCode = LTRIM(RTRIM(MCCO))
	FROM F0101 INNER JOIN F0006 ON (ABMCU = MCMCU)
	WHERE ABAN8 = @customerID

SELECT     OrderHeader.SHKCOO AS OrderCompany, OrderHeader.SHDOCO AS OrderNumber, OrderHeader.SHDCTO AS OrderType, 
                      OrderDetail.SDLNID AS LineNumber, OrderDetail.SDMCU AS CostCenter, OrderDetail.SDITM AS ShortItemNumber, 
                      OrderDetail.SDAITM AS SupplierNumber, OrderDetail.SDDSC1 AS ItemDescription1, OrderDetail.SDDSC2 AS ItemDescription2, 
                      OrderDetail.SDLNTY AS LineType, OrderDetail.SDNXTR AS StatusNext, OrderDetail.SDLTTR AS StatusLast, 
                      OrderDetail.SDEMCU AS BusinessUnitHeader, OrderDetail.SDUOM AS UnitOfMeasure, OrderDetail.SDUORG AS QtyOrdered, 
                      OrderDetail.SDSOQS AS QtyShipped, OrderDetail.SDSOBK AS QtyBackordered, OrderDetail.SDSOCN AS QtyCancelled, 
                      (OrderDetail.SDUPRC / 10000) AS PricePerUnit, (OrderDetail.SDAEXP / 100) AS ExtendedPrice, OrderDetail.SDKCO AS InvoiceCompany, 
                      OrderDetail.SDDOC AS InvoiceNumber, OrderDetail.SDDCT AS InvoiceType, OrderDetail.SDPSN AS PickslipNumber, OrderDetail.SDTAX1 AS Tax, 
                      OrderDetail.SDANI AS CutomerItemNumber, OrderDetail.SDCRCD AS CurrencyCodeFrom, OrderDetail.SDCRR AS CurrencyConverRate, 
                      OrderDetail.SDFUP AS ForeignPricePerUnit, OrderDetail.SDFEA AS ForeignExtendedPrice, OrderDetail.SDTORG AS TransactionOriginator, 
                      ItemGroup2.DRDL01 AS WebGroup, 
                      ItemGroup.DRDL01 AS [Group], ItemBrand.DRDL01 AS Brand, OrderDetail.SDVR01 AS CustomerReference, OrderDetail.SDVR02 AS WebOrderNumber, 
                      OrderHeader.SHEXR1 AS TaxExplanationCode, OrderHeader.SHTXA1 AS TaxArea, OrderHeader.SHRYIN as PaymentInstrument, OrderHeader.SHPTC AS PaymentTermsCode,
                      (SELECT TATXR1 / 1000
FROM         F4008
WHERE     (TATXA1 = OrderDetail.sdtxa1) AND (TAITM = OrderDetail.sdtax1)) as tax
FROM         (SELECT   SHKCOO, SHDOCO, SHDCTO, SHMCU, SHAN8, SHSHAN, SHPA8, SHVR01, SHVR02, SHPTC, SHRYIN, SHTXA1, SHEXR1, SHCARS, SHROUT, 
                                              SHCRRM, SHCRCD, SHCRR, SHFAP, SHFCST, SHORBY, SHTKBY
                       FROM         F42019) AS OrderHeader INNER JOIN
                          (SELECT SDKCOO, SDDOCO, SDDCTO, SDLNID, SDMCU, SDTRDJ, SDIVD, SDCNDJ, SDVR01, SDVR02, SDITM, SDLITM, SDAITM, SDDSC1, 
                                                   SDDSC2, SDLNTY, SDNXTR, SDLTTR, SDEMCU, SDSRP2, SDSRP3, SDSRP5, SDUOM, SDUORG, SDSOQS, SDSOBK, SDSOCN, SDUPRC, 
                                                   SDAEXP, SDKCO, SDDOC, SDDCT, SDPSN, SDTAX1, SDANI, SDCRCD, SDCRR, SDFUP, SDFEA, SDTORG,sdtxa1
                            FROM          F42119) AS OrderDetail ON OrderHeader.SHKCOO = OrderDetail.SDKCOO AND 
                      OrderHeader.SHDOCO = OrderDetail.SDDOCO AND OrderHeader.SHDCTO = OrderDetail.SDDCTO 
                      --INNER JOIN
                      --    (SELECT     F4008.TATXA1, F4008.TATXR1 / 1000 AS TATXR1, PRODDTA.F4008.TAGL01, PRODDTA.F4008.TAITM, 
                      --                             EffectiveDate.FullDate AS EffectiveDate, ExpireDate.FullDate AS ExpireDate, GETDATE() AS Now
                      --      FROM          F4008 LEFT OUTER JOIN
                      --                                 (SELECT     JDEDate, FullDate
                      --                                   FROM          QV_DateDimension AS QV_DateDimension_1) AS ExpireDate ON 
                      --                             F4008.TAEFDJ = ExpireDate.JDEDate LEFT OUTER JOIN
                      --                                 (SELECT     JDEDate, FullDate
                      --                                   FROM          QV_DateDimension AS QV_DateDimension_1) AS EffectiveDate ON 
                      --                             F4008.TAEFTJ = EffectiveDate.JDEDate
                      --      WHERE      (ExpireDate.FullDate > GETDATE()) OR
                      --                             (ExpireDate.FullDate IS NULL)) AS Tax ON OrderDetail.SDTAX1 = Tax.TAITM AND OrderHeader.SHTXA1 = Tax.TATXA1 
                                                   LEFT OUTER JOIN
                          (SELECT     LTRIM(DRKY) AS DRKY, DRDL01
                            FROM          F0005 AS F0005_2
                            WHERE      (DRSY = '41') AND (DRRT = 'S5')) AS ItemGroup2 ON OrderDetail.SDSRP5 = ItemGroup2.DRKY LEFT OUTER JOIN
                          (SELECT     LTRIM(DRKY) AS DRKY, DRDL01
                            FROM         F0005 AS F0005_1
                            WHERE      (DRSY = '41') AND (DRRT = 'S3')) AS ItemBrand ON OrderDetail.SDSRP3 = ItemBrand.DRKY LEFT OUTER JOIN
                          (SELECT     LTRIM(DRKY) AS DRKY, DRDL01
                            FROM          F0005 AS F0005_2
                            WHERE      (DRSY = '41') AND (DRRT = 'S2')) AS ItemGroup ON OrderDetail.SDSRP2 = ItemGroup.DRKY
where (OrderHeader.SHAN8 = @CustomerID) 
	AND (OrderDetail.SDDOC = @InvoiceID) 
	AND(OrderDetail.SDDCT = @InvoiceType) 
	AND (OrderDetail.SDKCO = @companyCode)




--Select	t2.SDITM, 
--		t2.SDDSC1, 
--		t2.SDDSC2, 
--		t2.SDSOQS, 
--		CASE SHCRRM WHEN 'F' THEN t2.FUP ELSE t2.UPRC END AS UnitPrice, 
--		CASE SHCRRM WHEN 'F' THEN t2.FEA ELSE t2.AEXP END AS TotalExVat, 
--		t2.SDTAX1, 
--		t2.SDTXA1, 
--		t2.SDCRCD, 
--		t2.SDCRR, 
--		t2.FUP, 
--		t2.FEA, 
--		t2.Vat, 
--		SHCRRM, 
--		t2.SDAITM
--From F42019
--inner join
--(
--SELECT F42119.SDLNID,
--	   F42119.SDITM, 
--	   F42119.SDDSC1, 
--	   F42119.SDDSC2, 
--	   F42119.SDSOQS, 
--	   F42119.SDUPRC / 10000 AS UPRC, 
--	   F42119.SDAEXP / 100 AS AEXP,
--       F42119.SDTAX1, 
--       F42119.SDTXA1, 
--       F42119.SDCRCD, 
--       F42119.SDCRR, 
--       F42119.SDFUP / 10000 AS FUP, 
--       F42119.SDFEA / 100 AS FEA, 
--       (F4008.TATXR1+F4008.TATXR2) / 1000 AS Vat, 
--       F42119.SDKCOO, 
--       F42119.SDDOCO, 
--       F42119.SDDCTO, 
--       F42119.SDAITM
--FROM F42119 INNER JOIN
--     F4008 ON (F42119.SDTXA1 = F4008.TATXA1 AND CASE F42119.SDTAX1 WHEN 'y' THEN 5 WHEN 'n' THEN 3 ELSE F42119.SDTAX1 END = F4008.TAITM)
--WHERE (F42119.SDAN8 = @CustomerID) 
--	AND (F42119.SDDOC = @InvoiceID) 
--	AND(F42119.SDDCT = @InvoiceType) 
--	AND (F42119.SDKCO = @companyCode)
--)as t2 on (SHKCOO= t2.SDKCOO and SHDOCO = t2.SDDOCO and SHDCTO = t2.SDDCTO)

--ORDER BY SDLNID

END
GO


