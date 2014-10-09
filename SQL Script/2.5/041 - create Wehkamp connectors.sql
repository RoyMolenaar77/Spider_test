

if not exists (select * from ConnectorSystem where Name = 'Wehkamp')
begin
	insert into ConnectorSystem
	values(4, 'Wehkamp')
end


if not exists (select * from connector where Name = 'Wehkamp CC')

begin

  set identity_insert connector on

	insert into Connector (ConnectorID, Name, ConnectorType, ConnectorSystemID, ConcatenateBrandName, ObsoleteProducts, ZipCodes, Selectors,
	 OverrideDescriptions, 	BSKIdentifier, 	UseConcentratorProductID, Connection, ImportCommercialText, IsActive, AdministrativeVendorID, OutboundUrl, 
	 ParentConnectorID, ConnectorSystemType, IgnoreMissingImage, IgnoreMissingConcentratorDescription )
	 values(13, 'Wehkamp CC', 0, 4, 0, 0, 0, 0, 0, 9998, 0, NULL, 0, 1, 1, NULL, NULL, 0, 1, 1)

   set identity_insert connector off
end


if not exists (select * from connector where Name = 'Wehkamp AT')

begin
    set identity_insert connector on
	insert into Connector (ConnectorID, Name, ConnectorType, ConnectorSystemID, ConcatenateBrandName, ObsoleteProducts, ZipCodes, Selectors,
	 OverrideDescriptions, 	BSKIdentifier, 	UseConcentratorProductID, Connection, ImportCommercialText, IsActive, AdministrativeVendorID, OutboundUrl, 
	 ParentConnectorID, ConnectorSystemType, IgnoreMissingImage, IgnoreMissingConcentratorDescription )
	  values(14, 'Wehkamp AT', 0, 4, 0, 0, 0, 0, 0, 9999, 0, NULL, 0, 1, 1, NULL, NULL, 0, 1, 1)
    set identity_insert connector off
end
 

