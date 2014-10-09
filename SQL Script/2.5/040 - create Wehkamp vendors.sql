declare @VendorIDCC int
declare @VendorIDAT int

set @VendorIDCC = 15
set @VendorIDAT = 25

if not exists (select * from Vendor where Name = 'Wehkamp CC')

begin
	insert into Vendor (VendorID, VendorType, Name, [Description], OrderDispatcherType, PurchaseOrderType, IsActive, ParentVendorID)
	values (@VendorIDCC, 135, 'Wehkamp CC', 'Wehkamp CC', NULL, NULL, 1, 1)
end


if not exists (select * from Vendor where Name = 'Wehkamp AT')

begin

	insert into Vendor (VendorID, VendorType, Name, [Description], OrderDispatcherType, PurchaseOrderType, IsActive, ParentVendorID)
	values (@VendorIDAT, 135, 'Wehkamp AT', 'Wehkamp AT', NULL, NULL, 1, 2)
end
 

 
