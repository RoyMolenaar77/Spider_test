-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after insert and update
-- =============================================
CREATE TRIGGER PartUpdate_ProductMedia_Insert
   ON  ProductMedia
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
CREATE TRIGGER PartUpdate_ProductMedia_Update
   ON  ProductMedia
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN inserted i on p.ProductID = i.ProductID
	INNER JOIN deleted d on i.MediaID = d.MediaID 
	WHERE	(i.LastChanged != d.LastChanged)									
			OR (i.Sequence != d.Sequence)	
			
			OR ((i.MediaPath != d.MediaPath)							-- MediaPath is changed
			OR (NOT i.MediaPath IS NULL AND d.MediaPath IS NULL)		-- MediaPath is set
			OR (i.MediaPath IS NULL AND NOT d.MediaPath IS NULL))		-- MediaPath is deleted
END
GO


-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after delete
-- =============================================
CREATE TRIGGER PartUpdate_ProductMedia_Delete
   ON  ProductMedia
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
END
GO



