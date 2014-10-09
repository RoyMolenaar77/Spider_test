-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-30
-- Description:	Update Product LastModificationTime after insert and update
-- =============================================
CREATE TRIGGER PartUpdate_MagentoProductGroupSetting_Insert
   ON  MagentoProductGroupSetting
   AFTER INSERT
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p 
	INNER JOIN ContentProductGroup cpg ON p.ProductID = cpg.ProductID
	INNER JOIN inserted i ON i.ProductGroupmappingID = cpg.ProductGroupMappingID
END
GO




-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-31
-- Description:	Update Product LastModificationTime after update
-- =============================================
CREATE TRIGGER PartUpdate_MagentoProductGroupSetting_Update
   ON  MagentoProductGroupSetting
   AFTER UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p
	INNER JOIN ContentProductGroup cpg ON p.ProductID = cpg.ProductID
	INNER JOIN inserted i ON i.ProductGroupmappingID = cpg.ProductGroupMappingID
	INNER JOIN deleted d on i.MagentoProductGroupSettingID = d.MagentoProductGroupSettingID 
	WHERE	((i.ShowInMenu != d.ShowInMenu)									-- ShowInMenu is changed
			OR (NOT i.ShowInMenu IS NULL AND d.ShowInMenu IS NULL)			-- ShowInMenu is set
			OR (i.ShowInMenu IS NULL AND NOT d.ShowInMenu IS NULL))			-- ShowInMenu is deleted

			OR ((i.DisabledMenu != d.DisabledMenu)							-- LongContentDescription is changed
			OR (NOT i.DisabledMenu IS NULL AND d.DisabledMenu IS NULL)		-- LongContentDescription is set
			OR (i.DisabledMenu IS NULL AND NOT d.DisabledMenu IS NULL))		-- LongContentDescription is deleted

			OR ((i.IsAnchor != d.IsAnchor)									-- IsAnchor is changed
			OR (NOT i.IsAnchor IS NULL AND d.IsAnchor IS NULL)				-- IsAnchor is set
			OR (i.IsAnchor IS NULL AND NOT d.IsAnchor IS NULL))				-- IsAnchor is deleted
END
GO



-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2014-01-30
-- Description:	Update Product LastModificationTime after delete
-- =============================================
CREATE TRIGGER PartUpdate_MagentoProductGroupSetting_Delete
   ON  MagentoProductGroupSetting
   AFTER DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
	UPDATE p SET p.LastModificationTime = GETDATE()
	FROM Product p 
	INNER JOIN ContentProductGroup cpg ON p.ProductID = cpg.ProductID
	INNER JOIN deleted d ON d.ProductGroupmappingID = cpg.ProductGroupMappingID
END
GO



