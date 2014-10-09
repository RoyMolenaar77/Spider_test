SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Stan Todorov
-- Create date: 18/07/2011
-- Description:	Ledgering for vendor prices
-- =============================================
Create TRIGGER dbo.VendorPriceLedger 
   ON  dbo.VendorPrice 
   AFTER Insert, UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

     insert into contentledger (ProductID, LedgerDate, UnitPrice, CostPrice, TaxRate, ConcentratorStatusID, MinimumQuantity, VendorAssortmentID, Remark, LedgerObject, CreatedBy, CreationTime, BasePrice, BaseCostPrice)
     
     select va.ProductID, getdate(), i.price, i.costprice, i.taxrate, i.concentratorstatusid, i.minimumquantity, i.vendorassortmentid, 'Inserted vendor price', 'VendorPrice', 0, getdate(), i.baseprice, i.basecostprice
     from inserted i 
     inner join vendorassortment va on i.vendorassortmentid = va.vendorassortmentid    
     

     
END
GO
