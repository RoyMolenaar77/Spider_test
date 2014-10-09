/****** Object:  StoredProcedure [dbo].[PortalGetWebCustomers]    Script Date: 03/07/2011 12:09:08 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PortalGetWebCustomers] --'40001'
(
            @BusinessUnit nvarchar(50)
)
AS

BEGIN 

select AIAN8 as addressBookNumber, AIBADT,AICRCD,AIACL,AITRAR,AIRYIN,AIROUT,ABALPH,ABTAX,ABTX2,ABAC02
into #AddressBookNumbers
FROM F03012
	inner join F0006 on LTRIM(RTRIM(MCMCU)) = @BusinessUnit and mcco = AICO
	INNER JOIN F0101 ON (AIAN8 = ABAN8 and ABMCU = MCMCU)
WHERE AIEDF2 = ''
union all 
select AIAN8 as addressBookNumber, AIBADT,AICRCD,AIACL,AITRAR,AIRYIN,AIROUT,ABALPH,ABTAX,ABTX2,ABAC02
FROM F03012
	INNER JOIN F0101 ON (AIAN8 = ABAN8)
WHERE AIAN8 = 300  


BEGIN 
WITH AccountManger (CustomerNumber, Name, EmailAddress, Tel) AS
(	
	SELECT distinct customerNumber, ABALPH AS Name, EAEMAL AS EmailAddress, Cast(ISNULL(WPAR1,'')+WPPH1 as nvarchar) AS Tel
	FROM F01151 
	INNER JOIN F0111 ON (EAAN8 = WWAN8 AND EAIDLN = WWIDLN AND EAETP='E')
	INNER JOIN #AddressBookNumbers ON (WWAN8 = addressBookNumber)
	inner join (select addressBookNumber as customerNumber, rtrim(ltrim(DRSPHD)) as AccountManagerCode
				from dbo.f0005 
				inner join #AddressBookNumbers on rtrim(ltrim(DRKY)) = rtrim(ltrim(ABAC02))
				where drsy = '01' and drrt = '02') code on addressBookNumber = code.AccountManagerCode
	Inner join dbo.f0115 on WPAN8 = addressBookNumber
	where WPPHTP <> 'PAS' and EARCK7 + WPRCK7 = (
		select MAX(earck7) +  MAX(WPRCK7)
	FROM F01151
	inner join f0115 on (WPAN8 = EAAN8)
INNER JOIN F0111 ON (WWAN8 = EAAN8)
where eaan8 = addressBookNumber AND EAETP='E' AND EAIDLN = WWIDLN and WPPHTP <> 'PAS'
	group by EAAN8
	) 
	)
	,Carrier (CustomerNumber, Name, Carrier) 	as(
	SELECT    F03012.AIAN8, F0101.ABALPH,F03012.AICARS  AS DefaultTransportMethod
	FROM         F0101 
	INNER JOIN F03012 ON F0101.ABAN8 = F03012.AICARS
	)
	,InvoiceInformation (CustomerNumber, InvoiceAmount, OpenInvoiceAmount, InvoiceCurrency ) as (
	SELECT  RPAN8, SUM(SUMRPAG)/100 AS SUMRPAG, SUM(SUMRPAAP)/100 AS SUMRPAAP, RPCRCD
FROM         (SELECT    RPAN8, RPCRCD, SUM(RPAG) AS SUMRPAG, SUM(RPAAP) AS SUMRPAAP
                       FROM          F03B11
                       WHERE      (RPDCT = 'RI' OR
                                              RPDCT = 'RL')
                       GROUP BY RPAN8,RPDOC, RPCRRM, RPCRCD
                       HAVING      (SUM(RPAAP) <> 0)) drvTbl 
                       GROUP BY RPAN8, RPCRCD                       
	)

SELECT DISTINCT
	CAST(a.addressBookNumber as decimal) AS BackendRelationID,
	CAST(MAPA8 as decimal) AS ParentBackendRelationID,
	LTRIM(RTRIM(ABALPH)) AS Name,
	WPPH1 as Password,
	RTRIM(ABTAX) AS TaxNumber,
	RTRIM(ABTX2) AS KvkNr,
	RTRIM(ALADD1) AS AddressLine1,
	RTRIM(ALADD2) AS AddressLine2,
	RTRIM(ALADDZ) AS ZipCode,
	RTRIM(ALCTY1) AS City,
	RTRIM(ALCTR) AS Country,
	AIBADT as AddressType,
	AccountManger.Name as AccountManagerName,
	AccountManger.EmailAddress as AccountManagerEmailAddress,
	AccountManger.Tel as AccountManagerPhoneNumber,
	cast(Carrier.Carrier as decimal) as DefaultCarrier,
	Carrier.Name as DefaultCarrierName,
	AICRCD as Currency, 
	cast(AIACL as decimal) as CreditLimit,
	AITRAR as PaymentDays,
	AIRYIN as PaymentInstrument,
	AIROUT as RouteCode,
	InvoiceInformation.InvoiceAmount,
	InvoiceInformation.OpenInvoiceAmount,
	InvoiceInformation.InvoiceCurrency
From #AddressBookNumbers a
	INNER JOIN F0111 ON (a.addressBookNumber = WWAN8 AND WWIDLN = 0)
	inner JOIN F0116 on (ALAN8 = a.addressBookNumber)
	LEFT JOIN F0115 on (WPAN8 = a.addressBookNumber AND WPPHTP='PAS' AND WPIDLN = 0)
	left JOIN F0150 on (MAAN8 = a.addressBookNumber AND MAOSTP = '' AND MADSS7 = 0)	
	left join AccountManger on CustomerNumber = a.addressBookNumber
	left join Carrier on Carrier.CustomerNumber = a.addressBookNumber
	left join InvoiceInformation on InvoiceInformation.CustomerNumber = a.addressBookNumber
END



SELECT
	addressBookNumber AS BackendRelationID,
	WPPHTP AS TelephoneType,
	LTRIM(RTRIM(WPAR1)) AS AreaCode,
	LTRIM(RTRIM(WPPH1)) AS PhoneNumber
FROM #AddressBookNumbers
	INNER JOIN F0115 ON (addressBookNumber = WPAN8 AND WPIDLN = 0)
where WPPHTP <> 'PAS' AND WPAR1 <> ''

SELECT DISTINCT  addressBookNumber AS BackendRelationID,
	LTRIM(RTRIM(EAEMAL)) AS Address,
	EAETP AS ElectronicAddressType
FROM #AddressBookNumbers
	INNER JOIN F01151 ON (addressBookNumber = EAAN8 AND EAIDLN = 0)

drop table #AddressBookNumbers


END

GO


