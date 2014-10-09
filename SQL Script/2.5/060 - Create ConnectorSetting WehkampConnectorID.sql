declare @ConnectorIDCC int
declare @ConnectorIDAT int
declare @ConnectorIDWehkampCC int
declare @ConnectorIDWehkampAT int

set @ConnectorIDCC = (select connectorid from Connector where Name = 'CC Web')
set @ConnectorIDAT = (select connectorid from Connector where Name = 'AT Web')
set @ConnectorIDWehkampCC = (select connectorid from Connector where Name = 'Wehkamp CC')
set @ConnectorIDWehkampAT = (select connectorid from Connector where Name = 'Wehkamp AT')


--add setting CC
if not exists(select * from ConnectorSetting where SettingKey = 'WehkampConnectorID' and ConnectorID = @ConnectorIDCC)
begin
	insert into ConnectorSetting
	values(@ConnectorIDCC, 'WehkampConnectorID', @ConnectorIDWehkampCC)
end

--add setting AT
if not exists(select * from ConnectorSetting where SettingKey = 'WehkampConnectorID' and ConnectorID = @ConnectorIDAT)
begin
	insert into ConnectorSetting
	values(@ConnectorIDAT, 'WehkampConnectorID', @ConnectorIDWehkampAT)
end




