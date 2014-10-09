/****** Object:  View [dbo].[AssortmentContentView]    Script Date: 07/28/2011 10:37:00 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO










ALTER VIEW [dbo].[AssortmentContentView] AS
WITH VendorStockSummary AS
(
      SELECT VendorID, ProductID, SUM(QuantityOnHand) AS QuantityOnHand,
      MIN(PromisedDeliveryDate) AS PromisedDeliveryDate, SUM(QuantityToReceive) AS QuantityToReceive
      FROM VendorStock
      GROUP BY VendorID, ProductID
), BrandList AS
(                       SELECT BrandID, VendorID, MIN(VendorBrandCode) AS VendorBrandCode
                        FROM BrandVendor
                        GROUP BY BrandID,VendorID
),
CustomItemNumbers AS
(
SELECT ConnectorID, ProductID, VendorID, CustomItemNumber
FROM (
SELECT pcv.ConnectorID, VA.ProductID, pcv.VendorID, VA.CustomItemNumber, pcv.isPreferred, ROW_NUMBER ()over(partition by pcv.ConnectorID, VA.ProductID order by pcv.IsPreferred desc) AS RN
FROM VendorAssortment VA 
INNER JOIN PreferredConnectorVendor pcv ON (pcv.VendorID = VA.VendorID)
) a WHERE RN=1
)
--,
--ProductDescriptionList AS 
--(

--SELECT ConnectorID, ProductID, LanguageID, ShortContentDescription, LongContentDescription
--FROM (
--SELECT CVS.ConnectorID, PD.ProductID, PD.LanguageID, PD.ShortContentDescription, PD.LongContentDescription,
--                ROW_NUMBER ()over(partition by CVS.ConnectorID, PD.ProductID, PD.LanguageID order by cvs.ContentVendorIndex asc) AS RN
                  
--FROM ProductDescription PD
--INNER JOIN PreferredConnectorVendor PCV on (PD.VendorID = PCV.VendorID)
--INNER JOIN ContentVendorSetting cvs on (PD.VendorID = CVS.VendorID)
--) A WHERE RN=1
--)
--,
--ProductDescriptionList AS 
--(

--SELECT ConnectorID, ProductID, LanguageID, ProductName
--FROM (
--SELECT CVS.ConnectorID, PD.ProductID, PD.LanguageID, PD.ProductName,
--                ROW_NUMBER ()over(partition by CVS.ConnectorID, PD.ProductID, PD.LanguageID order by cvs.ContentVendorIndex asc) AS RN       
--FROM ProductDescription PD
--INNER JOIN PreferredConnectorVendor PCV on (PD.VendorID = PCV.VendorID)
--INNER JOIN ContentVendorSetting cvs on (PD.VendorID = CVS.VendorID)
--) A WHERE RN=1
--)
SELECT distinct
      C.ConnectorID,
      C.ProductID, 
      --L.LanguageID,
      p.VendorItemNumber,
      P.BrandID,
      B.Name AS BrandName,
      BV.VendorBrandCode,
      C.ShortDescription,
      C.LongDescription,
      C.LineType,
      va.LedgerClass,
      va.ProductDesk,
      va.ExtendedCatalog,
      C.ProductContentID,
      CP.VendorID,
      V.CutOffTime,
      V.DeliveryHours,
      CI.CustomItemNumber,
      cps.ConnectorStatus,
      vs.PromisedDeliveryDate,
      vs.QuantityToReceive,
      vs.QuantityOnHand,
      cp.ProductContentIndex
      --PDL.ShortContentDescription,
      --PDL.LongContentDescription
      --,pdl.ProductName
      ,null as ProductName
      FROM Content C 
            INNER JOIN ContentProduct CP ON (CP.ProductContentID = C.ProductContentID)
            INNER JOIN Product P ON (P.ProductID = C.ProductID)
            INNER JOIN Brand B ON (P.BrandID = B.BrandID)
            INNER JOIN Vendor V on (CP.VendorID = V.VendorID)
            INNER JOIN VendorAssortment va on (cp.VendorID = va.VendorID and c.ProductID = va.ProductID)
            INNER JOIN VendorPrice vp on (va.VendorAssortmentID = vp.VendorAssortmentID)
            INNER JOIN VendorStockSummary vs ON (vs.VendorID = va.VendorID and vs.ProductID = va.ProductID)
            INNER JOIN CustomItemNumbers CI ON (CI.ConnectorID =C.ConnectorID AND CI.ProductID = va.ProductID)
			--LEFT JOIN ProductDescriptionList PDL ON (PDL.ConnectorID = C.ConnectorID AND PDL.ProductID = P.ProductID)
            left JOIN BrandList BV ON (BV.BrandID = P.BrandID AND ((V.ParentVendorID is not null and BV.VendorID = v.ParentVendorID) or BV.VendorID = v.VendorID))
               inner join Connector con on con.ConnectorID = c.ConnectorID
            inner JOIN ConnectorProductStatus cps on cps.ConcentratorStatusID = vp.ConcentratorStatusID and (cps.ConnectorID = c.ConnectorID or (con.ParentConnectorID is not null and cps.ConnectorID = con.ParentConnectorID))

      --, [Language] L
      --AND L.LanguageID =2
      --AND (PDL.LanguageID = L.LanguageID )



GO


