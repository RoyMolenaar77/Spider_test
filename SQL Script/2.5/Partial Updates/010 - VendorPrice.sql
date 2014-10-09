-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after insert and update
-- =============================================
CREATE TRIGGER PartUpdate_VendorPrice_Insert
   ON  VendorPrice
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN VendorAssortment va ON p.ProductID = va.ProductID
	INNER JOIN inserted i on va.VendorAssortmentID = i.VendorAssortmentID

END
GO




-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after update
-- =============================================
CREATE TRIGGER PartUpdate_VendorPrice_Update
   ON  VendorPrice
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN VendorAssortment va ON p.ProductID = va.ProductID
	INNER JOIN inserted i on va.VendorAssortmentID = i.VendorAssortmentID
	INNER JOIN deleted d on i.VendorAssortmentID = d.VendorAssortmentID AND i.MinimumQuantity = d.MinimumQuantity
	WHERE	((i.Price != d.Price)												-- Price is changed
			OR (NOT i.Price IS NULL AND d.Price IS NULL)						-- Price is set
			OR (i.Price IS NULL AND NOT d.Price IS NULL))						-- Price is deleted

			OR ((i.CostPrice != d.CostPrice)									-- CostPrice is changed
			OR (NOT i.CostPrice IS NULL AND d.CostPrice IS NULL)				-- CostPrice is set
			OR (i.CostPrice IS NULL AND NOT d.CostPrice IS NULL))				-- CostPrice is deleted

			OR ((i.SpecialPrice != d.SpecialPrice)								-- SpecialPrice is changed
			OR (NOT i.SpecialPrice IS NULL AND d.SpecialPrice IS NULL)			-- SpecialPrice is set
			OR (i.SpecialPrice IS NULL AND NOT d.SpecialPrice IS NULL))			-- SpecialPrice is deleted
			
			OR ((i.BasePrice != d.BasePrice)									-- BasePrice is changed
			OR (NOT i.BasePrice IS NULL AND d.BasePrice IS NULL)				-- BasePrice is set
			OR (i.BasePrice IS NULL AND NOT d.BasePrice IS NULL))				-- BasePrice is deleted

			OR ((i.BaseCostPrice != d.BaseCostPrice)							-- BaseCostPrice is changed
			OR (NOT i.BaseCostPrice IS NULL AND d.BaseCostPrice IS NULL)		-- BaseCostPrice is set
			OR (i.BaseCostPrice IS NULL AND NOT d.BaseCostPrice IS NULL))		-- BaseCostPrice is deleted
END
GO


-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after delete
-- =============================================
CREATE TRIGGER PartUpdate_VendorPrice_Delete
   ON  VendorPrice
   AFTER DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p 
	INNER JOIN VendorAssortment va ON p.ProductID = va.ProductID
	INNER JOIN deleted d on va.VendorAssortmentID = d.VendorAssortmentID

END
GO



