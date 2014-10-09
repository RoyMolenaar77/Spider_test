
ALTER PROCEDURE [dbo].[sp_GenerateMissingContent] AS
BEGIN

	truncate table MissingContent

	insert into MissingContent (ConcentratorProductID, ConnectorID, isActive, VendorItemNumber, CustomItemNumber, BrandName, ShortDescription, ProductGroupID, BrandID, [Image], YouTube, Specifications, ContentVendor, ContentVendorID, Barcode, CreationTime, LastModificationTime,
		   HasDescription,
		   IsConfigurable,
		   QuantityOnHand
		   )

	select 
	   distinct
			 ConcentratorProductID,
		   ConnectorID, 
		   Active, 
		   VendorItemNumber, 
		   CustomItemNumber, 
		   brandName, 
		   ShortDescription, 
		   isnull(ProductGroupID, -1), 
		   BrandID, [Image], youtube, Specifications, ContentVendor, ContentVendorID, barcode, CreationTime, LastModificationTime,		   
		   HasDescription,
		   IsConfigurable,
		   isnull(QuantityOnHand, 0)
		   
	from MissingContentView

END



GO


