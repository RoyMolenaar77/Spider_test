
IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('[dbo].[Connector]') AND [name] = 'OrganizationID')
BEGIN
	alter table Connector add OrganizationID int not null default((1));
end

alter table Connector add constraint FK_Connector_Organization foreign key (OrganizationID) references Organization(Id)