IF NOT EXISTS(SELECT * FROM [sys].[columns] WHERE [object_id] = OBJECT_ID('[dbo].[User]') AND [name] = 'OrganizationID')
  BEGIN
	alter table [User] add OrganizationID int not null default((1));
  end

  alter table [User] add constraint FK_User_Organization foreign key (OrganizationID) references Organization(Id)