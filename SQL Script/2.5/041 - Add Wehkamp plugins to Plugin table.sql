
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.ProductPriceUpdateExport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Product Price Update Export', 'Concentrator.Plugins.Wehkamp.ProductPriceUpdateExport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Product Price Update Export added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Product Price Update Export already added'
END           
		 
--IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.ProductAttributesExport')
--BEGIN
-- INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
-- VALUES ('Wehkamp Product Attributes Export', 'Concentrator.Plugins.Wehkamp.ProductAttributesExport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
-- PRINT 'Wehkamp Product Attributes Export added to the Plugin table'
--END          
--ELSE
--BEGIN
--PRINT 'Wehkamp Product Attributes Export already added'
--END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.ProductInformationExport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Product Information Export', 'Concentrator.Plugins.Wehkamp.ProductInformationExport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Product Information Export added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Product Information Export already added'
END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.ProductMediaExport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Product Media Export', 'Concentrator.Plugins.Wehkamp.ProductMediaExport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Product Media Export added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Product Media Export already added'
END   

IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.ProductRelationImport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Product Relation Import', 'Concentrator.Plugins.Wehkamp.ProductRelationImport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Product Relation Import added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Product Relation Import already added'
END   

IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.SalesOrderImport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Sales Order Import', 'Concentrator.Plugins.Wehkamp.SalesOrderImport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Sales Order Import added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Sales Order Import already added'
END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.ShipmentConfirmationImport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Shipment Confirmation Import', 'Concentrator.Plugins.Wehkamp.ShipmentConfirmationImport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Shipment Confirmation Import added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Shipment Confirmation Import already added'
END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.ShipmentNotificationExport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Shipment Notification Export', 'Concentrator.Plugins.Wehkamp.ShipmentNotificationExport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Shipment Notification Export added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Shipment Notification Export already added'
END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.StockMutationImport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Stock Mutation Import', 'Concentrator.Plugins.Wehkamp.StockMutationImport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Stock Mutation Import added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Stock Mutation Import already added'
END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.StockMutationImport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Stock Mutation Import', 'Concentrator.Plugins.Wehkamp.StockMutationImport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Stock Mutation Import added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Stock Mutation Import already added'
END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.StockPhotoImport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Stock Photo Import', 'Concentrator.Plugins.Wehkamp.StockPhotoImport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Stock Photo Import added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Stock Photo Import already added'
END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.StockReturnConfirmationExport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Return Confirmation Export', 'Concentrator.Plugins.Wehkamp.StockReturnConfirmationExport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Return Confirmation Export added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Return Confirmation Export already added'
END   
	 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.StockReturnRequestConfirmationImport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Stock Return Request Confirmation Import', 'Concentrator.Plugins.Wehkamp.StockReturnRequestConfirmationImport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Stock Return Request Confirmation Import added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Stock Return Request Confirmation Import already added'
END   
		 
IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.StockReturnRequestExport')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Stock Return Request Export', 'Concentrator.Plugins.Wehkamp.StockReturnRequestExport', 'Wehkamp', '0 0 0/1 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Stock Return Request Export added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Stock Return Request Export already added'
END   


IF NOT EXISTS (SELECT * from Plugin where PluginType = 'Concentrator.Plugins.Wehkamp.Communicator')
BEGIN
 INSERT INTO [dbo].[Plugin] ([PluginName],[PluginType],[PluginGroup],[CronExpression],[ExecuteOnStartup],[IsActive],[JobServer])
 VALUES ('Wehkamp Communicator', 'Concentrator.Plugins.Wehkamp.Communicator', 'Wehkamp', '0 0/15 * * ?', 0, 0, 1)
 PRINT 'Wehkamp Communicator added to the Plugin table'
END          
ELSE
BEGIN
PRINT 'Wehkamp Stock Return Request Export already added'
END   
