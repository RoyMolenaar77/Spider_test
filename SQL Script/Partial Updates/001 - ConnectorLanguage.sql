-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after insert
-- =============================================
CREATE TRIGGER PartUpdate_ConnectorLanguage_Insert
   ON  ConnectorLanguage
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p 
	INNER JOIN Content c ON p.ProductID = c.ProductID
	INNER JOIN inserted i ON i.ConnectorID = c.ConnectorID
END
GO


-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after delete
-- =============================================
CREATE TRIGGER PartUpdate_ConnectorLanguage_Delete
   ON  ConnectorLanguage
   AFTER DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p 
	INNER JOIN Content c ON p.ProductID = c.ProductID
	INNER JOIN deleted d ON d.ConnectorID = c.ConnectorID
END
GO



