

IF EXISTS(SELECT * FROM sys.objects WHERE type = 'P' AND name = 'GenerateSlipNumber')
         DROP PROCEDURE GenerateVendorSequenceNumber
GO

CREATE PROCEDURE [dbo].GenerateSlipNumber
    @VendorID int,
    @SettingKey nvarchar(max)
AS 

    SET NOCOUNT ON;
         
         DECLARE @sequence int;         

         BEGIN TRANSACTION
         SELECT TOP 1 @sequence = Value FROM VendorSetting WHERE VendorID = @VendorID AND SettingKey = @Settingkey;

         IF (@sequence IS NULL)
                 BEGIN 
                 SET @sequence = 1;
                  INSERT INTO VendorSetting (VendorID, SettingKey, Value) VALUES (@VendorID, @Settingkey, @sequence);
                 END
         ELSE
                 BEGIN
                 SET @sequence = @sequence + 1;
                  UPDATE VendorSetting SET Value = @sequence WHERE VendorID = @VendorID AND SettingKey = @Settingkey;
                 END
         COMMIT; 

         SELECT @sequence;
GO

