declare @kraagvormAttributeID int

set @kraagvormAttributeID = (select AttributeID from ProductAttributeMetaData
							where AttributeCode = 'Kraagvorm' 
							and VendorID = 48) 

if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Blousekraag') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Blousekraag ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Bontkraag') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Bontkraag ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Boothals') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Boothals ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Button down') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Button down ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Capuchon') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Capuchon ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Col') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Col ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Halter') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Halter ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'NVT') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'NVT ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Offschoulder') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Offschoulder ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Opstaande kraag') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Opstaande kraag ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Overslagkraag') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Overslagkraag ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Polo') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Polo ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Racerback') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Racerback ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Reverskraag') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Reverskraag ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Ronde hals') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Ronde hals ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Sjaalkraag') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Sjaalkraag ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Smokingkraag') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Smokingkraag ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Strapless') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Strapless ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Strikkraag') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Strikkraag ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'V-hals') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'V-hals ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Vierkante hals') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Vierkante hals ')
if not exists (select * from ProductAttributeOption where attributeID = @kraagvormAttributeID and AttributeOption = 'Watervalhals') insert into ProductAttributeOption  values (@kraagvormAttributeID, 'Watervalhals ')
