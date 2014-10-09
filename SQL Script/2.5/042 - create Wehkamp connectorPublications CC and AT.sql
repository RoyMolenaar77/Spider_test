
declare @ConnectorIDCC int
declare @ConnectorIDAT int
declare @VendorIDCC int
declare @VendorIDAT int

set @ConnectorIDCC = (select connectorid from Connector where name	= 'Wehkamp CC')
set @ConnectorIDAT = (select connectorid from Connector where name	= 'Wehkamp AT')
set @VendorIDCC = 15
set @VendorIDAT = 25


--rule for CC
--include all products
if not exists ( select * from ContentProduct 
				where 
				ConnectorID = @ConnectorIDCC 
				and VendorID = @VendorIDCC
				and ProductGroupID is null
				and BrandID is null 
				and ProductID is null
				and IsAssortment = 1 
				and createdBy = 1 
				and ProductContentIndex = 0
)
begin

	insert into ContentProduct (ConnectorID, VendorID, createdby, creationTime, ProductContentIndex, IsAssortment)
	values(@ConnectorIDCC, @VendorIDCC, 1, GETDATE(), 0, 1)
end


--rule for AT
--include all products
if not exists ( select * from ContentProduct 
				where 
				ConnectorID = @ConnectorIDAT 
				and VendorID = @VendorIDAT 
				and ProductGroupID is null
				and BrandID is null 
				and ProductID is null
				and IsAssortment = 1 
				and createdBy = 1 
				and ProductContentIndex = 0
)
begin

	insert into ContentProduct (ConnectorID, VendorID, createdby, creationTime, ProductContentIndex, IsAssortment)
	values(@ConnectorIDAT, @VendorIDAT, 1, GETDATE(), 0, 1)
end

if not exists (select * from connectorpublication where connectorid = 14 and vendorid = 25 and attributevalue = 'False')
  begin 
    insert into connectorpublication (ConnectorID, VendorID, createdby, creationtime, productcontentindex, attributeid, attributevalue)
    values (14,25,1, getdate(),1,93,'False')
  end


update connector set parentconnectorid = 5 where connectorid = 13
update connector set parentconnectorid = 6 where connectorid = 14