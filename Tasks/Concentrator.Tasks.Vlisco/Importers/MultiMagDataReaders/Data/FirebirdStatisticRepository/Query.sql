SELECT
  S.codamb                            as Atmosphere,
  S.codctxt                           as Context,
  S.dtjour                            AS Datum,
  S.codbout                           AS Shop,
  S.nbvt                              AS TotalSales,
  S.nbvtacli                          AS ClientSales,
  S.nbvtncli                          AS GeneralSales,
  S.nbpieces                          AS UnitsSold,
  ROUND(S.panqte, 2)                  AS UnitsPerClient,
  S.ca                                AS TotalAmount,
  ROUND(S.panca, 2)                   AS SalesPerClient,
  S.nbvissa                           AS VisitorCount,
  CAST(S.nbvissa - S.nbvt AS NUMERIC) AS VisitorsNoSale
FROM  dstatmag AS S
WHERE fljma = 'J'
AND   (codpaie = '' OR codpaie IS NULL)
AND   S.dtcre >= '{0}' 
ORDER BY codbout, dtjour