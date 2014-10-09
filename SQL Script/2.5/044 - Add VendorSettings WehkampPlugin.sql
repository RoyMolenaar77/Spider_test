
IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'WehkampAlliantieName' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'WehkampAlliantieName', 'Coolcat')
 PRINT 'WehkampAlliantieName set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'WehkampAlliantieName setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'WehkampAlliantieName' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'WehkampAlliantieName', 'Americatoday')
 PRINT 'WehkampAlliantieName set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'WehkampAlliantieName setting for Vendor Wehkamp AT already added'
END 





IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'WehkampRetailPartnerCode' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'WehkampRetailPartnerCode', '00892')
 PRINT 'WehkampRetailPartnerCode set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'WehkampRetailPartnerCode setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'WehkampRetailPartnerCode' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'WehkampRetailPartnerCode', '00803')
 PRINT 'WehkampRetailPartnerCode set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'WehkampRetailPartnerCode setting for Vendor Wehkamp AT already added'
END 






IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'Merknaam' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'Merknaam', 'CoolCat')
 PRINT 'Merknaam set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'Merknaam setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'Merknaam' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'Merknaam', 'America Today')
 PRINT 'Merknaam set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'Merknaam setting for Vendor Wehkamp AT already added'
END 



IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'PricesExportedDatetime' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'PricesExportedDatetime', '')
 PRINT 'PricesExportedDatetime set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'PricesExportedDatetime setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'PricesExportedDatetime' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'PricesExportedDatetime', '')
 PRINT 'PricesExportedDatetime set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'PricesExportedDatetime setting for Vendor Wehkamp AT already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'ShipmentNotificationAddDaysFromToday' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'ShipmentNotificationAddDaysFromToday', '1')
 PRINT 'ShipmentNotificationAddDaysFromToday set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'ShipmentNotificationAddDaysFromToday setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'ShipmentNotificationAddDaysFromToday' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'ShipmentNotificationAddDaysFromToday', '0')
 PRINT 'ShipmentNotificationAddDaysFromToday set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'ShipmentNotificationAddDaysFromToday setting for Vendor Wehkamp AT already added'
END 




IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'ArtikelGroepCharacters' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'ArtikelGroepCharacters', '2')
 PRINT 'ArtikelGroepCharacters set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'ArtikelGroepCharacters setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'ArtikelGroepCharacters' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'ArtikelGroepCharacters', '3')
 PRINT 'ArtikelGroepCharacters set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'ArtikelGroepCharacters setting for Vendor Wehkamp AT already added'
END 







IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'ExportOnlyProductsOlderThanXXXMinutes' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'ExportOnlyProductsOlderThanXXXMinutes', '60')
 PRINT 'ExportOnlyProductsOlderThanXXXMinutes set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'ExportOnlyProductsOlderThanXXXMinutes setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'ExportOnlyProductsOlderThanXXXMinutes' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'ExportOnlyProductsOlderThanXXXMinutes', '60')
 PRINT 'ExportOnlyProductsOlderThanXXXMinutes set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'ExportOnlyProductsOlderThanXXXMinutes setting for Vendor Wehkamp AT already added'
END 










IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'KleurConnectorID' and VendorID = 15)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (15, 'KleurConnectorID', '5')
 PRINT 'KleurConnectorID set for Vendor Wehkamp CC'
END          
ELSE
BEGIN
PRINT 'KleurConnectorID setting for Vendor Wehkamp CC already added'
END 


IF NOT EXISTS (SELECT * from VendorSetting where SettingKey = 'KleurConnectorID' and VendorID = 25)
BEGIN
 INSERT INTO [dbo].[VendorSetting] ([VendorID],[SettingKey],[Value])
 VALUES (25, 'KleurConnectorID', '6')
 PRINT 'KleurConnectorID set for Vendor Wehkamp AT'
END          
ELSE
BEGIN
PRINT 'KleurConnectorID setting for Vendor Wehkamp AT already added'
END 





