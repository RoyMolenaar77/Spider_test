SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Pieter de Wit
-- Create date: 2013-10-31
-- Description:	Update the LastUpdated column after a price change
-- =============================================

IF NOT EXISTS (select * from sys.triggers where name = 'VendorPricesChanged') 
BEGIN

	EXEC('CREATE TRIGGER dbo.VendorPricesChanged 
	ON  dbo.VendorPrice
	AFTER UPDATE
	AS 
	
	
	BEGIN
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		
		IF NOT EXISTS (SELECT * FROM inserted) RETURN

		begin
			UPDATE VendorPrice SET LastUpdated = getdate() 
			WHERE VendorAssortmentID IN
			(SELECT i.VendorAssortmentID FROM Inserted i
			INNER JOIN Deleted d on i.VendorAssortmentID = d.VendorAssortmentID
			AND ((i.Price != d.Price) OR (i.SpecialPrice != d.SpecialPrice) 
			OR (NOT i.Price IS NULL AND d.Price IS NULL) 
			OR (NOT i.SpecialPrice IS NULL AND d.SpecialPrice IS NULL)
			OR (i.SpecialPrice IS NULL AND NOT d.SpecialPrice IS NULL)
			))
		end
	END
	
	
	')
END
