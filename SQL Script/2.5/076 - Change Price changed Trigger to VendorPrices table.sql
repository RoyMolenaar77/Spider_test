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

	EXEC('ALTER TRIGGER [dbo].[VendorPricesChanged] 
	ON  [dbo].[VendorPrice]
	AFTER UPDATE
	AS 
	BEGIN
		PRINT 'Total rows ' +  cast(@@ROWCOUNT as varchar)
		-- SET NOCOUNT ON added to prevent extra result sets from
		-- interfering with SELECT statements.
		SET NOCOUNT ON;
		
		IF NOT EXISTS (SELECT * FROM inserted) RETURN


		begin
			--Update when special price has been changed (afprijzing)
			UPDATE VendorPrice SET LastUpdated = GETDATE() 
			WHERE VendorAssortmentID IN
			(
				SELECT i.VendorAssortmentID FROM Inserted i
				INNER JOIN Deleted d on i.VendorAssortmentID = d.VendorAssortmentID
				AND ((i.SpecialPrice != d.SpecialPrice)						-- SpecialPrice is changed
				OR (NOT i.SpecialPrice IS NULL AND d.SpecialPrice IS NULL)  -- SpecialPrice is set
				OR (i.SpecialPrice IS NULL AND NOT d.SpecialPrice IS NULL)) -- SpecialPrice is deleted
			)





			--Update when new price is lower (omprijzing)
			--Step 1: LastUpdated van Price zetten
			UPDATE VendorPrice SET LastUpdated = GETDATE() 
			WHERE VendorAssortmentID IN
			(
				SELECT i.VendorAssortmentID FROM Inserted i
				INNER JOIN Deleted d on i.VendorAssortmentID = d.VendorAssortmentID
				AND (i.Price < d.Price) -- Price is lower than old price
			)

			--Step 2: LastUpdated van Product zetten
			UPDATE Product SET LastModificationTime = GETDATE()
			WHERE ProductID IN 
			(
				SELECT ParentProductID 
				FROM product 
				WHERE productid IN 
				(
					SELECT ProductID
					FROM VendorAssortment 
					WHERE VendorAssortmentID IN
					(
						SELECT i.VendorAssortmentID FROM Inserted i
								INNER JOIN Deleted d on i.VendorAssortmentID = d.VendorAssortmentID
								AND (i.Price < d.Price) -- Price is lower than old price
					)
					
				)
			)

			--Step3: Set ResendProductInformationToWehkamp attribute
			declare @ResendProductInformationToWehkampAttributeID int
			set @ResendProductInformationToWehkampAttributeID = (SELECT AttributeID FROM ProductAttributeMetaData pamd WHERE pamd.AttributeCode = 'ResendProductInformationToWehkamp')

			INSERT INTO ProductAttributeValue (AttributeID, ProductID, Value, CreatedBy, CreationTime) 
			SELECT DISTINCT @ResendProductInformationToWehkampAttributeID, P.ParentProductID, 'true', 1, GETDATE()
			FROM Product p
			INNER JOIN VendorAssortment va ON va.ProductID = p.ProductID
			LEFT OUTER JOIN ProductAttributeValue pav on pav.ProductID = p.ProductID AND pav.AttributeID = @ResendProductInformationToWehkampAttributeID
			WHERE va.VendorAssortmentID IN
			(
				SELECT DISTINCT i.VendorAssortmentID FROM Inserted i
						INNER JOIN Deleted d on i.VendorAssortmentID = d.VendorAssortmentID
						AND (i.Price < d.Price) -- Price is higher than old price
			)
			AND (pav.Value IS NULL or LOWER(pav.Value) = 'false')




			


			--Update when new price is higher (opprijzing)
			--Step 1: SentToWehkamp op 'false' zetten
			UPDATE ProductAttributeValue SET Value = 'false' 
			FROM ProductAttributeValue pav 
			INNER JOIN Product p ON p.ParentProductID = pav.ProductID 
			INNER JOIN VendorAssortment va ON p.ProductID = va.ProductID
			WHERE pav.AttributeID = (SELECT AttributeID FROM ProductAttributeMetaData pamd WHERE pamd.AttributeCode = 'SentToWehkamp')
			AND va.VendorAssortmentID IN
			(
				SELECT i.VendorAssortmentID FROM Inserted i
						INNER JOIN Deleted d on i.VendorAssortmentID = d.VendorAssortmentID
						AND (i.Price > d.Price) -- Price is lower than old price
			)

			--Step2: Set ResendPriceUpdateToWehkamp attribute
			declare @ResendPriceUpdateToWehkampAttributeID int
			set @ResendPriceUpdateToWehkampAttributeID = (SELECT AttributeID FROM ProductAttributeMetaData pamd WHERE pamd.AttributeCode = 'ResendPriceUpdateToWehkamp')

			INSERT INTO ProductAttributeValue (AttributeID, ProductID, Value, CreatedBy, CreationTime) 
			SELECT DISTINCT @ResendPriceUpdateToWehkampAttributeID, P.ParentProductID, 'true', 1, GETDATE()
			FROM Product p
			INNER JOIN VendorAssortment va ON va.ProductID = p.ProductID
			LEFT OUTER JOIN ProductAttributeValue pav on pav.ProductID = p.ParentProductID AND pav.AttributeID = @ResendPriceUpdateToWehkampAttributeID
			WHERE va.VendorAssortmentID IN
			(
				SELECT DISTINCT i.VendorAssortmentID FROM Inserted i
						INNER JOIN Deleted d on i.VendorAssortmentID = d.VendorAssortmentID
						AND (i.Price > d.Price) -- Price is higher than old price
			)
			AND (pav.Value IS NULL or LOWER(pav.Value) = 'false')
		end
	END
			')
END
