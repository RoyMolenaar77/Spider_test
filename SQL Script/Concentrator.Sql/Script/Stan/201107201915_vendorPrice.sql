
/****** Object:  StoredProcedure [dbo].[CalculateVendorPrices]    Script Date: 07/15/2011 10:07:32 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Stan Todorov
-- Create date: 14/07/2011
-- Description:	Updates the vendor prices by applying certain rules to them
-- =============================================
create PROCEDURE [dbo].[sp_CalculateVendorPrices] 
	-- Add the parameters for the stored procedure here

	@VendorID int = null

AS
BEGIN

	update vendorprice
	set		
		vendorprice.price = VendorPriceCalculationView.price,
		vendorprice.costprice = VendorPriceCalculationView.costprice,
			vendorprice.VendorPriceRuleID = VendorPriceCalculationView.VendorPriceRuleID
	from vendorprice, VendorPriceCalculationView
	
	where vendorprice.VendorAssortmentID = VendorPriceCalculationView.VendorAssortmentID and	
	 (@VendorID is null or VendorPriceCalculationView.VendorID =  @VendorID)

END

GO