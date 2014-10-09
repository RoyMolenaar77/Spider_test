
declare @WehkampATConnectorID int
declare @WehkampCCConnectorID int

set @WehkampCCConnectorID = (select Connectorid from Connector where Name = 'Wehkamp CC')
set @WehkampATConnectorID = (select Connectorid from Connector where Name = 'Wehkamp AT')

if not exists( 
	select * from ConnectorLanguage
	where ConnectorID = @WehkampCCConnectorID and languageid = 2 and Country = 'NL'
	)
begin
	insert into ConnectorLanguage(ConnectorID, LanguageID, Country)
	values(@WehkampCCConnectorID, 2, 'NL')
end

if not exists( 
	select * from ConnectorLanguage
	where ConnectorID = @WehkampATConnectorID and languageid = 2 and Country = 'NL'
	)
begin
	insert into ConnectorLanguage(ConnectorID, LanguageID, Country)
	values(@WehkampATConnectorID, 2, 'NL')
end