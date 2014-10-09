SELECT
  (
    SELECT MAX(D.DTTARIF)
    FROM dtarartd AS D  
    WHERE D.numtaill = g.numtaill 
      AND D.codcoloris = A.codcoloris 
      AND D.codmodele = A.codmodele 
    GROUP BY D.DTTARIF
  )               AS DateTime,
  a.codmodele     AS ArticleCode,
  a.codcoloris    AS ColorCode,
  (
    SELECT  c.designc 
    from    pcoloris AS C  
    where   C.codcoloris = a.codcoloris
  )               AS ColorName,
  g.numtaill      AS SizeCode,
  G.gencod        AS Barcode,
  m.designl       AS DescriptionLong,
  m.designc       AS DescriptionShort,
  (
    SELECT    MAX(d.prixunit)
    FROM      dtarartd D  
    WHERE     D.numtaill = g.numtaill and D.codcoloris = A.codcoloris and D.codmodele  =A.codmodele 
    GROUP BY  D.DTTARIF
  )               AS Price,
  a.Codcll        AS OriginCode,
  (
    SELECT l.designl
    from parlib L  
    WHERE L.famparam = 'CLL' and L.codparam = A.codcll
  )               AS OriginName,
  a.codray        AS LabelCode,
  (
    SELECT l.designl
    FROM parlib L  
    WHERE L.famparam = 'RAY' and L.codparam = A.codray
  )               AS LabelName,
  a.codfam        AS ProductSegmentCode,
  (
    SELECT l.designl
    from parlib L  
    where L.famparam = 'FAM' and L.codparam = COALESCE(A.codray, A.codfam)
  )               AS ProductSegmentName,
  a.codsfm        AS ProductGroup,
  (
    SELECT  l.designl
    from    parlib L  
    where   L.famparam = 'SFM' and L.codparam = COALESCE(A.codray, A.codfam, A.codsfm)
  )               AS ProductGroupName
FROM dgencod G, pmodele M, darticle A, dtarartd Z
WHERE M.codmodele   = G.codmodele
  AND M.codmodele   = A.codmodele
  AND M.flag        <> 'S'
  AND M.dtmaj       >= '{0}'
  AND G.codcoloris  = A.codcoloris
  AND Z.codmodele   = A.codmodele
  AND Z.codcoloris  = A.codcoloris
  AND Z.numtaill    = g.numtaill