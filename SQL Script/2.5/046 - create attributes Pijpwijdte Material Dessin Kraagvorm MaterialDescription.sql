
declare @ProductAttributeGroupId int
declare @currentAttributeID int

set @ProductAttributeGroupId = (
	select pagmd.ProductAttributeGroupID from ProductAttributeGroupMetaData pagmd
	inner join ProductAttributeGroupName pagn
	on pagn.LanguageID = 1 and pagn.ProductAttributeGroupID = pagmd.ProductAttributeGroupID
	where VendorID = 48 and pagn.LanguageID = 1 and pagn.Name = 'General'
)



--insert Pijpwijdte attribute
if not exists
(
	select * from ProductAttributeMetaData
	where AttributeCode = 'Pijpwijdte' 
	and VendorID = 48
)
begin

	insert into ProductAttributeMetaData (attributeCode, ProductAttributeGroupId, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, CreatedBy, CreationTime, Mandatory, IsConfigurable)
	values ('Pijpwijdte', @ProductAttributeGroupId, 0, 0,0, 48, 0, 1, GETDATE(), 0, 0)

	set @currentAttributeID = SCOPE_IDENTITY()

	insert into ProductAttributeName (AttributeID,LanguageID, Name)
	values(@currentAttributeID, 1, 'Pijpwijdte')

end



--insert Kraagvorm attribute
if not exists
(
	select * from ProductAttributeMetaData
	where AttributeCode = 'Kraagvorm' 
	and VendorID = 48
)
begin

	insert into ProductAttributeMetaData (attributeCode, ProductAttributeGroupId, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, CreatedBy, CreationTime, Mandatory, IsConfigurable)
	values ('Kraagvorm', @ProductAttributeGroupId, 0, 0,0, 48, 0, 1, GETDATE(), 0, 0)

	insert into ProductAttributeName (AttributeID,LanguageID, Name)
	values(SCOPE_IDENTITY(), 1, 'Kraagvorm')

end



--insert Dessin attribute
if not exists
(
	select * from ProductAttributeMetaData
	where AttributeCode = 'Dessin' 
	and VendorID = 48
)
begin

	insert into ProductAttributeMetaData (attributeCode, ProductAttributeGroupId, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, CreatedBy, CreationTime, Mandatory, IsConfigurable)
	values ('Dessin', @ProductAttributeGroupId, 0, 0,0, 48, 0, 1, GETDATE(), 0, 0)

	insert into ProductAttributeName (AttributeID,LanguageID, Name)
	values(SCOPE_IDENTITY(), 1, 'Dessin')

end



--insert Material attribute
if not exists
(
	select * from ProductAttributeMetaData
	where AttributeCode = 'Material' 
	and VendorID = 48
)
begin

	insert into ProductAttributeMetaData (attributeCode, ProductAttributeGroupId, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, CreatedBy, CreationTime, Mandatory, IsConfigurable)
	values ('Material', @ProductAttributeGroupId, 0, 0,0, 48, 0, 1, GETDATE(), 0, 0)

	insert into ProductAttributeName (AttributeID,LanguageID, Name)
	values(SCOPE_IDENTITY(), 1, 'Material')

end



--insert MaterialDescription attribute
if not exists
(
	select * from ProductAttributeMetaData
	where AttributeCode = 'MaterialDescription' 
	and VendorID = 48
)
begin

	insert into ProductAttributeMetaData (attributeCode, ProductAttributeGroupId, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, CreatedBy, CreationTime, Mandatory, IsConfigurable)
	values ('MaterialDescription', @ProductAttributeGroupId, 0, 0,0, 48, 0, 1, GETDATE(), 0, 0)

	insert into ProductAttributeName (AttributeID,LanguageID, Name)
	values(SCOPE_IDENTITY(), 1, 'MaterialDescription')

end


