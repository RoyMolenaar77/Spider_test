SELECT
  CURRENT_TIMESTAMP AS Datum,
  codbout           AS ShopCode,
  codmodele         AS ArticleCode,
  codcoloris        AS ColorCode,
  numtaill          AS SizeCode,
  qtestock          AS InStock,
  qtestkmax         AS Maximum,
  qtestkmin         AS Minimum,
  qtereserv         AS Reserved,
  reacmd            AS Ordered,
  realivr           AS Delivered,
  qtedispo          AS Available,
  totin             AS TotalIn,
  totout            AS TotalOut,
  ROUND(cmup, 2)  AS WAC
FROM      dstock
WHERE     qtestock <> 0
ORDER BY  codbout, codmodele