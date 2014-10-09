-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after insert
-- =============================================
CREATE TRIGGER PartUpdate_ContentFlat_Insert
   ON  ContentFlat
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
CREATE TRIGGER PartUpdate_ContentFlat_Update
   ON  ContentFlat
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
	INNER JOIN deleted d on i.ConnectorID = d.ConnectorID AND i.ProductID = d.ProductID AND i.LanguageID = d.LanguageID
	WHERE	((i.ShortContentDescription != d.ShortContentDescription)							-- ShortContentDescription is changed
			OR (NOT i.ShortContentDescription IS NULL AND d.ShortContentDescription IS NULL)	-- ShortContentDescription is set
			OR (i.ShortContentDescription IS NULL AND NOT d.ShortContentDescription IS NULL))	-- ShortContentDescription is deleted

			OR ((i.LongContentDescription != d.LongContentDescription)							-- LongContentDescription is changed
			OR (NOT i.LongContentDescription IS NULL AND d.LongContentDescription IS NULL)		-- LongContentDescription is set
			OR (i.LongContentDescription IS NULL AND NOT d.LongContentDescription IS NULL))		-- LongContentDescription is deleted

			OR ((i.ShortDescription != d.ShortDescription)										-- ShortDescription is changed
			OR (NOT i.ShortDescription IS NULL AND d.ShortDescription IS NULL)					-- ShortDescription is set
			OR (i.ShortDescription IS NULL AND NOT d.ShortDescription IS NULL))					-- ShortDescription is deleted

			OR ((i.LongDescription != d.LongDescription)										-- LongDescription is changed
			OR (NOT i.LongDescription IS NULL AND d.LongDescription IS NULL)					-- LongDescription is set
			OR (i.LongDescription IS NULL AND NOT d.LongDescription IS NULL))					-- LongDescription is deleted
END
GO



-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after delete
-- =============================================
CREATE TRIGGER PartUpdate_ContentFlat_Delete
   ON  ContentFlat
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



