-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after insert
-- =============================================
CREATE TRIGGER PartUpdate_ContentAttribute_Insert
   ON  ContentAttribute
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
CREATE TRIGGER PartUpdate_ContentAttribute_Update
   ON  ContentAttribute
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
	INNER JOIN deleted d on i.ContentAttributeID = d.ContentAttributeID
	WHERE	(i.IsSearchable != d.IsSearchable)							
		OR	(i.IsVisible != d.IsVisible)
		OR	(i.GroupName != d.GroupName)
		OR	(i.AttributeValue != d.AttributeValue)
		OR	(i.AttributeName != d.AttributeName)
		OR	((i.AttributeOriginalValue != d.AttributeOriginalValue)							-- AttributeOriginalValue is changed
			OR (NOT i.AttributeOriginalValue IS NULL AND d.AttributeOriginalValue IS NULL)  -- AttributeOriginalValue is set
			OR (i.AttributeOriginalValue IS NULL AND NOT d.AttributeOriginalValue IS NULL)) -- AttributeOriginalValue is deleted
END
GO



-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after delete
-- =============================================
CREATE TRIGGER PartUpdate_ContentAttribute_Delete
   ON  ContentAttribute
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



