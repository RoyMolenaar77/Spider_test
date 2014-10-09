SELECT
  btqcrea                                                         AS ShopCode,
  codclient                                                       AS Client,
  NOM                                                             AS Name,
  PRENOM                                                          AS FirstName,
  email                                                           as Email,
  adr1                                                            AS Address1,
  adr2                                                            AS Address2,
  adr3                                                            AS Address3,
  codpost                                                         AS PostCode,
  VILLE                                                           AS City,
  telprive                                                        AS TelephoneHome,
  telprof                                                         AS TelephoneWork,
  CAST
  (
    COALESCE(journaiss, 1900)
    || '-' ||
    COALESCE(moisnaiss, 1)
    || '-' ||
    COALESCE(annenaiss, 1)
    AS TIMESTAMP
  ) AS Birthday,
  numcarte                                                        AS CardNumber,
  caclient                                                        AS TotalAmountSpent,
  dt1achat                                                        AS FirstBuy,
  dtderachat                                                      AS Lastbuy,
  dtcre                                                           AS Created,
  dtmaj                                                           AS LastModified
FROM dclient
WHERE caclient <> 0 AND dtcre >= '{0}'
ORDER BY codclient ASC