ALTER TRIGGER [dbo].[VendorPricesChanged] 
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

			--Step 3: SentToWehkamp op 'false' zetten

			--Step3: Set ResendProductInformationToWehkamp attribute

			DECLARE @ResendProductInformationToWehkampAttributeID INT   -- Attribute
			DECLARE @Prod_Id INT																				-- ProductID
			DECLARE @Val BIT																						-- Value
			DECLARE @CreatedBy INT																			-- Created by
			DECLARE @CreationDate DATE																	-- Creation date
			DECLARE @LastModificationDate DATE													-- Last modification date

			SELECT DISTINCT @ResendProductInformationToWehkampAttributeID = (SELECT AttributeID FROM ProductAttributeMetaData pamd WHERE pamd.AttributeCode = 'ResendProductInformationToWehkamp'), 
							@Prod_Id = P.ParentProductID, @Val = 'true', @CreatedBy = 1, @CreationDate = GETDATE(), @LastModificationDate = GETDATE()
			FROM Product p
			INNER JOIN VendorAssortment va ON va.ProductID = p.ProductID
			LEFT OUTER JOIN ProductAttributeValue pav on pav.ProductID = p.ProductID AND pav.AttributeID = @ResendProductInformationToWehkampAttributeID
			WHERE va.VendorAssortmentID IN
			(
				SELECT DISTINCT i.VendorAssortmentID FROM Inserted i INNER JOIN Deleted d ON i.VendorAssortmentID = d.VendorAssortmentID AND (i.Price < d.Price) -- Price is higher than old price
			)
			AND (pav.Value IS NULL or LOWER(pav.Value) = 'false')

			-- Check if there;s already an AttributeID=97 (ResendProductInformationToWehkamp) in the ProductattributeValue for the updated (price) product
			IF NOT EXISTS (SELECT pav.AttributeID 
						   FROM ProductattributeValue pav 
						   where pav.AttributeID = @ResendProductInformationToWehkampAttributeID AND pav.ProductID = @Prod_Id)

			-- If it's not there, insertion has to be done
			BEGIN
				INSERT INTO ProductAttributeValue (AttributeID, ProductID, Value, CreatedBy, CreationTime) 
				VALUES(@ResendProductInformationToWehkampAttributeID, @Prod_Id, @Val, @CreatedBy, @CreationDate)
			END
			-- If it's there, update has to be done
			ELSE
				UPDATE ProductattributeValue SET Value=@Val ,LastModificationTime=@LastModificationDate WHERE ProductID = @Prod_Id;




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
	
GO