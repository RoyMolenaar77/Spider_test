if not exists (select * from connector where name = 'shop.coolcat.nl') 
begin
	insert into connector values ('shop.coolcat.nl', 4251, 2,0 ,0,0,0,0, 1235, null, 0, 'server=172.16.250.15;User Id=stan;password=23df908jA!@$#;database=magento_coolcat;Connect Timeout=30000;Default Command Timeout=30000;port=3306', 0,1,1,'http://10.172.26.1:1331', 5, null, 949,1,1,2)
end

if not exists (select * from connector where name = 'shop.coolcat.be') 
begin
	insert into connector values ('shop.coolcat.be', 4251, 2,0 ,0,0,0,0, 1236, null, 0, 'server=172.16.250.15;User Id=stan;password=23df908jA!@$#;database=magento_coolcat;Connect Timeout=30000;Default Command Timeout=30000;port=3306', 0,1,1,'http://10.172.26.1:1331', 5, null, 949,1,1,2)
end

if not exists( select * from connectorpublicationrule where connectorid = (select connectorid from connector where name = 'shop.coolcat.nl'))
begin
	insert into connectorpublicationrule (connectorid, vendorid, publicationtype, publicationindex, createdby, creationtime, isactive, enabledbydefault, onlyapprovedproducts) 
	select (select connectorid from connector where name = 'shop.coolcat.nl') as Connectorid, vendorid, publicationtype, publicationindex, createdby, creationtime, isactive, enabledbydefault, onlyapprovedproducts
	from connectorpublicationrule where connectorid = 5
end

if not exists( select * from connectorpublicationrule where connectorid = (select connectorid from connector where name = 'shop.coolcat.be'))
begin
	insert into connectorpublicationrule (connectorid, vendorid, publicationtype, publicationindex, createdby, creationtime, isactive, enabledbydefault, onlyapprovedproducts) 
	select (select connectorid from connector where name = 'shop.coolcat.be') as Connectorid, vendorid, publicationtype, publicationindex, createdby, creationtime, isactive, enabledbydefault, onlyapprovedproducts
	from connectorpublicationrule where connectorid = 7
end

if not exists (select * from preferredconnectorvendor where connectorid = (select connectorid from connector where name = 'shop.coolcat.nl'))
begin 
	insert into PreferredConnectorVendor (vendorid, connectorid , ispreferred, iscontentvisible, CentralDelivery)
	select  vendorid, (select connectorid from connector where name = 'shop.coolcat.nl') as Connectorid, ispreferred, iscontentvisible, CentralDelivery
	from preferredconnectorvendor 
	where connectorid = 5
end

if not exists (select * from preferredconnectorvendor where connectorid = (select connectorid from connector where name = 'shop.coolcat.be'))
begin 
	insert into PreferredConnectorVendor (vendorid, connectorid , ispreferred, iscontentvisible, CentralDelivery)
	select  vendorid, (select connectorid from connector where name = 'shop.coolcat.be') as Connectorid, ispreferred, iscontentvisible, CentralDelivery
	from preferredconnectorvendor 
	where connectorid = 7
end

if not exists (select * from additionalorderproduct where connectorid = (select connectorid from connector where name = 'shop.coolcat.nl'))
begin 
	insert into additionalorderproduct (connectorid, connectorproductid, vendorid, vendorproductid, createdby)
	select (select connectorid from connector where name = 'shop.coolcat.nl') as Connectorid, connectorproductid, vendorid, vendorproductid, createdby
	from additionalorderproduct
	where connectorid = 5
end 

if not exists (select * from additionalorderproduct where connectorid = (select connectorid from connector where name = 'shop.coolcat.be'))
begin 
	insert into additionalorderproduct (connectorid, connectorproductid, vendorid, vendorproductid, createdby)
	select (select connectorid from connector where name = 'shop.coolcat.be') as Connectorid, connectorproductid, vendorid, vendorproductid, createdby
	from additionalorderproduct
	where connectorid = 7
end 

