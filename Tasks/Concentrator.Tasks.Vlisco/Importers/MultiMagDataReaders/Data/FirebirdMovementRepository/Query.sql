SELECT
  D.codbout                   AS ShopCode,
  D.nummvt                    AS MovementNumber,
  D.typmvt                    AS MovementType,
  (
    select L.designl  
    from PARLIB L 
    where L.famparam  = 'MSK'
    AND   L.codlang   = '1'
    AND   D.typmvt    = L.codparam
  )                           AS MoveDescription,
  D.codmodele                 AS ArticleCode,
  D.codcoloris                AS ColorCode,
  D.numtaill                  AS SizeCode,
  D.flsens                    AS MovementDirection,
  D.dtmvt                     AS MovementDate,
  D.dtsys                     AS SystemDate,
  D.codpaie                   AS SalesPerson,
  ROUND(D.pa, 2)              AS CostPrice,
  ROUND(D.prix, 2)            AS SalesPrice,
  D.lgndoc                    AS DocumentLine,
  D.numdoc                    AS DocumentNumber,
  D.deporig                   AS LocationFrom,
  D.depdest                   AS LocationTo,
  D.qte                       AS Quantity,
  D.NUMTRANSAC                AS TransactionNumber,
  D.codlot                    AS LotNumber
FROM  dmvtstck AS D, parlib AS L
WHERE L.famparam = 'MSK'
AND   L.codlang = '1'
AND   D.typmvt = L.codparam
AND   D.dtsys >= '{0}' 