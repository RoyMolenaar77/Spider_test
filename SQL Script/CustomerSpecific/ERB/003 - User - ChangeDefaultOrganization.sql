update [User]
set OrganizationID = 
	case 
		when ConnectorID in (5,7,8,13) then (select id from organization where name = 'Coolcat')
		when ConnectorID in (6,9,10, 14) then (select id from organization where name = 'America Today')
    when ConnectorID is null then (select id from organization where name = 'Default')
		when ConnectorID in (11,15) then (select id from organization where name = 'Sapph')
	end

