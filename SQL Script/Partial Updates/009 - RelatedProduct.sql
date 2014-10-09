-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after insert and update
-- =============================================
CREATE TRIGGER PartUpdate_RelatedProduct_Insert
   ON  RelatedProduct
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

	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN inserted i on p.ProductID = i.RelatedProductID
END
GO



-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after update
-- =============================================
CREATE TRIGGER PartUpdate_RelatedProduct_Update
   ON  RelatedProduct
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN inserted i ON p.ProductID = i.ProductID
	INNER JOIN deleted d on i.ProductID = d.ProductID AND i.RelatedProductID = d.RelatedProductID AND i.VendorID = d.VendorID AND i.RelatedProductTypeID = d.RelatedProductTypeID 
	WHERE	(i.[Index] != d.[Index])

	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN inserted i ON p.ProductID = i.RelatedProductID
	INNER JOIN deleted d on i.ProductID = d.ProductID AND i.RelatedProductID = d.RelatedProductID AND i.VendorID = d.VendorID AND i.RelatedProductTypeID = d.RelatedProductTypeID 
	WHERE	(i.[Index] != d.[Index])
END
GO



-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after delete
-- =============================================
CREATE TRIGGER PartUpdate_RelatedProduct_Delete
   ON  RelatedProduct
   AFTER DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p 
	INNER JOIN deleted d ON d.ProductID = p.ProductID

	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p 
	INNER JOIN deleted d ON d.RelatedProductID = p.ProductID
END
GO



