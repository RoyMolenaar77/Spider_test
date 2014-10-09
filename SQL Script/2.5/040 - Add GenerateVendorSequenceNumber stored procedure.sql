IF EXISTS(SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GenerateVendorSequenceNumber')
	DROP PROCEDURE GenerateVendorSequenceNumber
GO

CREATE PROCEDURE [dbo].GenerateVendorSequenceNumber
    @VendorID int
AS 

    SET NOCOUNT ON;
	
	DECLARE @sequence int;
	DECLARE @settingkey varchar(16) = 'WehkampSequence';

	BEGIN TRANSACTION
	SELECT TOP 1 @sequence = Value FROM VendorSetting WHERE VendorID = @VendorID AND SettingKey = @settingkey;

	IF (@sequence IS NULL)
		BEGIN 
		SET @sequence = 1;
		INSERT INTO VendorSetting (VendorID, SettingKey, Value) VALUES (@VendorID, @settingkey, @sequence);
		END
	ELSE
		BEGIN
		SET @sequence = @sequence + 1;
		UPDATE VendorSetting SET Value = @sequence WHERE VendorID = @VendorID AND SettingKey = @settingkey;
		END
	COMMIT; 

	SELECT @sequence;
GO