declare @ConnectorIDWehkampCC int
declare @ConnectorIDWehkampAT int

set @ConnectorIDWehkampCC = (select connectorid from Connector where Name = 'Wehkamp CC')
set @ConnectorIDWehkampAT = (select connectorid from Connector where Name = 'Wehkamp AT')


--add setting CC
if not exists(select * from ConnectorSetting where SettingKey = 'ExcludeProducts' and ConnectorID = @ConnectorIDWehkampCC)
begin
	insert into ConnectorSetting
	values(@ConnectorIDWehkampCC, 'ExcludeProducts', 'True')
end

--add setting AT
if not exists(select * from ConnectorSetting where SettingKey = 'ExcludeProducts' and ConnectorID = @ConnectorIDWehkampAT)
begin
	insert into ConnectorSetting
	values(@ConnectorIDWehkampAT, 'ExcludeProducts', 'True')
end




