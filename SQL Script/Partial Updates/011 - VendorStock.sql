-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after insert and update
-- =============================================
CREATE TRIGGER PartUpdate_VendorStock_Insert
   ON  VendorStock
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN inserted i on p.ProductID = i.ProductID

END
GO



-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after update
-- =============================================
CREATE TRIGGER PartUpdate_VendorStock_Update
   ON  VendorStock
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN inserted i ON p.ProductID= i.ProductID
	INNER JOIN deleted d on i.ProductID = d.ProductID AND i.VendorID = d.VendorID AND i.VendorStockTypeID = d.VendorStockTypeID
	WHERE	(i.QuantityOnHand != d.QuantityOnHand)

			OR ((i.StockStatus != d.StockStatus)							-- StockStatus is changed
			OR (NOT i.StockStatus IS NULL AND d.StockStatus IS NULL)		-- StockStatus is set
			OR (i.StockStatus IS NULL AND NOT d.StockStatus IS NULL))		-- StockStatus is deleted
END
GO


-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after delete
-- =============================================
CREATE TRIGGER PartUpdate_VendorStock_Delete
   ON  VendorStock
   AFTER DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p 
	INNER JOIN deleted d on p.ProductID = d.ProductID

END
GO



