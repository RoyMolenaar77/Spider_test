declare @pijpwijdteAttributeID int

set @pijpwijdteAttributeID = (select AttributeID from ProductAttributeMetaData
							where AttributeCode = 'Pijpwijdte' 
							and VendorID = 48) 

if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = '7/8e Broek') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, '7/8e Broek ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Bermuda') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Bermuda ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Bootcut') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Bootcut ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Boyfriend') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Boyfriend ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Flare') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Flare ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Hotpants') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Hotpants ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Legging') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Legging ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'N.v.t.') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'N.v.t. ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Pofbroek') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Pofbroek ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Short') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Short ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Skinny fit') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Skinny fit ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Straight fit') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Straight fit ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Tapered') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Tapered ')
if not exists (select * from ProductAttributeOption where attributeID = @pijpwijdteAttributeID and AttributeOption = 'Wijde pijp') insert into ProductAttributeOption  values (@pijpwijdteAttributeID, 'Wijde pijp ')
