

if not exists(select * from vendorsetting where vendorid=  51 and settingkey ='ReturnCostsProduct')
begin
  insert into vendorsetting values (51, 'ReturnCostsProduct',	'WebRetCostBE NVT NVT')
  insert into vendorsetting values (51, 'ShipmentCostsProduct', 'WebOrdCostBE NVT NVT')
end

update product set isnonassortmentitem = 1, brandid = 57061 where productid in (
	select productid from product where vendoritemnumber like '%CostBE%'
)

if not exists (select * from additionalorderproduct where connectorid = 15)
begin 
  insert into additionalorderproduct (connectorid, ConnectorProductID, VendorID, VendorProductID, CreatedBy)
  select 15, VendorItemnumber, 51, productid, 1
  from product 
  where vendoritemnumber like '%CostBE NVT%' 
end

if not exists(select * from userrole where vendorid =51 and userid = 1)
begin 
  insert into userrole values (1,1,51)
  insert into userrole values (15,10,51)
end

if not exists(select * from connectorpublicationrule where vendorid =51 )
begin
  insert into connectorpublicationrule (connectorid, vendorid, publicationtype, publicationindex, createdby, isactive, enabledbydefault, onlyapprovedproducts)
  values (15,51,1,0,1,1,1,0)
end


if not exists(select * from connectorsetting where connectorid = 15 and settingkey = 'FtpAddress')
begin
  insert into connectorsetting values (15,	'FtpAddress',	'172.16.250.51')
  insert into connectorsetting values (15,	'FtpPassword',	'f8=2A"j?C9g14!*')
  insert into connectorsetting values (15,	'FtpPath',	'/content_staging/sapph/media/catalog/product/')
  insert into connectorsetting values (15,	'FtpUserName',	'concentrator')
end

if not exists(select * from contentvendorsetting where vendorid = 51 and connectorid= 15)
begin
  insert into contentvendorsetting (connectorid, vendorid, createdby, contentvendorindex)
  values (15,51,1,0)

  insert into contentvendorsetting (connectorid, vendorid, createdby, contentvendorindex)
values (15,48,1,0)

  insert into preferredconnectorvendor (vendorid, connectorid, ispreferred, iscontentvisible)
  values (51,15,1,1)
  insert into contentvendorsetting (connectorid, vendorid, createdby, contentvendorindex)
  values (15,50,1,2)

end

if not exists(select * from language where languageid = 5)
begin 
  insert into language values (5, 'Vlaams', 'NL')
  insert into connectorlanguage (connectorid, languageid, country)
  values (15,3, 'BE')

  insert into connectorlanguage (connectorid, languageid, country)
  values (15,5, 'BE') 

  insert into productattributegroupname values ('General', 81,3)
  insert into productattributegroupname values ('General', 81,5)
  insert into productattributegroupname values ('General', 4,5)
 end


 if not exists (select * from connectorproductstatus where connectorid = 15)
 begin 
  insert into connectorproductstatus values (15, 'ENA', 9,9)
  insert into connectorproductstatus values (15, 'Active', 4,4)

  insert into vendorproductstatus values (51,	'ENA',	9,	NULL	)
  insert into vendorproductstatus values (51,	'Non-Web',	-1,	NULL)	
  insert into vendorproductstatus values (51,	'NonStock',	-1,	NULl)
  insert into vendorproductstatus values (51,	'Active',4	,NULL)
end

if not exists(select * from connectorsetting where connectorid = 15 and settingkey = 'OverwriteStockUpdateParentConnectorBehavior')
begin
  insert into connectorsetting values (15, 'OverwriteStockUpdateParentConnectorBehavior', 'True')
end