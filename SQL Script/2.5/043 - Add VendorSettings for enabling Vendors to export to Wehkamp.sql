
IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'ExportToWehkamp' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'ExportToWehkamp', 1)
 PRINT 'ExportToWehkamp set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'ExportToWehkamp setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'ExportToWehkamp' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'ExportToWehkamp', 1)
 PRINT 'ExportToWehkamp set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'ExportToWehkamp setting for Vendor Wehkamp AT already added'
END 