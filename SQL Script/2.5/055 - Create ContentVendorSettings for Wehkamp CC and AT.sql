declare @WehkampATConnectorID int
declare @WehkampCCConnectorID int
declare @WehkampCCVendorID int
declare @WehkampATVendorID int
declare @ConcentratorVendorID int
declare @ImageCCVendorID int
declare @ImageATVendorID int

set @WehkampCCConnectorID = (select Connectorid from Connector where Name = 'Wehkamp CC')
set @WehkampATConnectorID = (select Connectorid from Connector where Name = 'Wehkamp AT')
set @WehkampCCVendorID = (select vendorid from Vendor where Name = 'Wehkamp CC')
set @WehkampATVendorID = (select vendorid from Vendor where Name = 'Wehkamp AT')
set @ConcentratorVendorID = (select vendorid from Vendor where Name = 'Concentrator')
set @ImageCCVendorID = (select vendorid from Vendor where Name = 'PFA CC')
set @ImageATVendorID = (select vendorid from Vendor where Name = 'PFA AT')



--add content vendorsettings for Connector wehkamp CC
--add vendor 48 (Concentrator)
if not exists(select * from ContentVendorSetting where ConnectorID = @WehkampCCConnectorID and VendorID = @ConcentratorVendorID )
begin
	insert into ContentVendorSetting (ConnectorID, VendorID, CreatedBy, CreationTime, ContentVendorIndex)
	values (@WehkampCCConnectorID, @ConcentratorVendorID, 1, getdate(), 1)
end

--add vendor 15 (Wehkamp CC)
if not exists(select * from ContentVendorSetting where ConnectorID = @WehkampCCConnectorID and VendorID = @WehkampCCVendorID )
begin
	insert into ContentVendorSetting (ConnectorID, VendorID, CreatedBy, CreationTime, ContentVendorIndex)
	values (@WehkampCCConnectorID, @WehkampCCVendorID, 1, getdate(), 0)
end

--images add vendor 1 (PFA CC)
if not exists(select * from ContentVendorSetting where ConnectorID = @WehkampCCConnectorID and VendorID = @ImageCCVendorID )
begin
	insert into ContentVendorSetting (ConnectorID, VendorID, CreatedBy, CreationTime, ContentVendorIndex)
	values (@WehkampCCConnectorID, @ImageCCVendorID, 1, getdate(), 2)
end



--add content vendorsettings for Connector wehkamp AT
--add vendor 48 (Concentrator)
if not exists(select * from ContentVendorSetting where ConnectorID = @WehkampATConnectorID and VendorID = @ConcentratorVendorID )
begin
	insert into ContentVendorSetting (ConnectorID, VendorID, CreatedBy, CreationTime, ContentVendorIndex)
	values (@WehkampATConnectorID, @ConcentratorVendorID, 1, getdate(), 1)
end

--add vendor 25 (Wehkamp AT)
if not exists(select * from ContentVendorSetting where ConnectorID = @WehkampATConnectorID and VendorID = @WehkampATVendorID )
begin
	insert into ContentVendorSetting (ConnectorID, VendorID, CreatedBy, CreationTime, ContentVendorIndex)
	values (@WehkampATConnectorID, @WehkampATVendorID, 1, getdate(), 0)
end

--images add vendor 2 (PFA AT)
if not exists(select * from ContentVendorSetting where ConnectorID = @WehkampATConnectorID and VendorID = @ImageATVendorID )
begin
	insert into ContentVendorSetting (ConnectorID, VendorID, CreatedBy, CreationTime, ContentVendorIndex)
	values (@WehkampATConnectorID, @ImageATVendorID, 1, getdate(), 2)
end


