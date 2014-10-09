
if not exists (select * from preferredconnectorvendor where connectorid = 13 and vendorid = 15)
begin
insert into PreferredConnectorVendor values (15,13,1,1,null,0)
end

if not exists (select * from preferredconnectorvendor where connectorid = 14 and vendorid = 25)
begin
insert into PreferredConnectorVendor values (25,14,1,1,null,0)
end


if not exists (select * from connectorproductstatus where connectorid = 13 and concentratorstatusid = 4)
begin
insert into connectorproductstatus values (13, 'Enabled', 4, null)
end
if not exists (select * from connectorproductstatus where connectorid = 14 and concentratorstatusid = 4)
begin
insert into connectorproductstatus values (14, 'Enabled', 4, null)
end