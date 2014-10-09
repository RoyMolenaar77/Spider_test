SELECT
  D.dtvente                                     AS OrderDate,
  D.dtcre                                       AS OrderTime,
  D.codtransac                                  AS SaleType,
  D.numticket                                   AS Ticket,
  D.numlign                                     AS Line,
  D.codmodele                                   AS ArticleCode,
  D.codcoloris                                  AS ColorCode,
  D.numtaill                                    AS SizeCode,
  D.qte*sgn                                     AS Quantity,
  ROUND(D.puachat, 2)                           AS PurchasePrice,
  ROUND(D.puvente, 2)                           AS SalePrice,
  ROUND(D.mntlgnht, 2)                          AS BrutoPrice,
  ROUND(D.mntlgntax, 2)                         AS VAT,
  ROUND(D.mntlgnttc, 2)                         AS NettoPrice,
  cast((D.mntntremp * 100) AS INTEGER) / 100.0  AS DiscountPercentage,
  ROUND(D.mntntremv, 2)                         AS DiscountValue,
  D.codbout                                     AS ShopCode,
  e.nomvend                                     AS SalesPerson,
  e.codclient                                   AS Client
FROM  dvented AS D, dventee AS E
WHERE D.FLAG <> 'S'
AND   D.numticket = E.numticket
AND   D.codbout = E.codbout
AND   D.codtransac in ('V','U','R','F')
AND   D.dtcre >= '{0}'
ORDER BY D.codbout, D.dtvente, D.numticket, D.numlign