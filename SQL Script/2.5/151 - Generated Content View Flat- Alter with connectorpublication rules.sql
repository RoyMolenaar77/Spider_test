

ALTER VIEW [dbo].[GenerateContentViewFlat] AS

SELECT distinct C.ConnectorID,
	C.ProductID, 
	--coalesce(pd.languageid, l.LanguageID, 0) as LanguageID,
	cL.LanguageID,
	p.VendorItemNumber,
	P.BrandID,
	B.Name AS BrandName,
	BV.VendorBrandCode,
	C.ShortDescription,
	C.LongDescription,
	C.LineType,
	C.LedgerClass,
	C.ProductDesk,
	C.ExtendedCatalog,
	C.ConnectorPublicationRuleID as  ProductContentID,
	CP.VendorID,
	(select top 1 PD1.LongContentDescription
	from ProductDescription PD1 
	INNER JOIN ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID
	where pd1.productid = c.productid and PD1.LanguageID = cl.LanguageID and PD1.VendorID = cvs.VendorID and PD1.LongContentDescription is not null
	order by cvs.ContentVendorIndex) as LongContentDescription,
	
	(select top 1 PD1.ShortContentDescription
	from ProductDescription PD1 
	INNER JOIN ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID
where pd1.productid = c.productid and PD1.LanguageID = cl.languageID and PD1.VendorID = cvs.VendorID and PD1.ShortContentDescription is not null
	order by cvs.ContentVendorIndex) as ShortContentDescription,
	
	(select top 1 PD1.ProductName
	from ProductDescription PD1 
	INNER JOIN ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID
	where pd1.productid = c.productid and PD1.LanguageID = cl.LanguageID
	order by PD1.ProductName desc,cvs.ContentVendorIndex) as ProductName,
		
	(select top 1 PD1.ModelName
	from ProductDescription PD1 
	INNER JOIN ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID
	where pd1.productid = c.productid and PD1.LanguageID = cl.LanguageID
	order by PD1.ModelName desc,cvs.ContentVendorIndex) as ModelName,
			
	(select top 1 PD1.WarrantyInfo
	from ProductDescription PD1 
	INNER JOIN ContentVendorSetting cvs on cvs.ConnectorID = c.ConnectorID
	where pd1.productid = c.productid and PD1.LanguageID = cl.LanguageID
	order by PD1.WarrantyInfo desc,cvs.ContentVendorIndex) as WarrantyInfo,
		V.CutOffTime,
	V.DeliveryHours
	 FROM Content C 
	 INNER JOIN connectorpublicationrule CP ON (CP.connectorpublicationruleid = C.connectorpublicationruleid)
	INNER JOIN Product P ON (P.ProductID = C.ProductID)
	INNER JOIN Brand B ON (P.BrandID = B.BrandID)
	INNER JOIN Vendor V on (CP.VendorID = V.VendorID)
	INNER JOIN ConnectorLanguage cl on (c.ConnectorID = cl.ConnectorID)
	--LEFT JOIN (select distinct LanguageID,productid from ProductDescription) PD ON (C.ProductID = PD.ProductID)
	LEFT JOIN (
				SELECT BrandID, VendorID, MIN(VendorBrandCode) AS VendorBrandCode
				FROM BrandVendor
				GROUP BY BrandID,VendorID
				) BV ON (BV.BrandID = P.BrandID AND ((V.ParentVendorID is not null and BV.VendorID = v.ParentVendorID) or BV.VendorID = v.VendorID))

	--, [Language] L
	
	




GO


