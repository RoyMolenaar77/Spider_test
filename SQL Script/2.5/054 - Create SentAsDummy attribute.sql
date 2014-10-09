declare @ProductAttributeGroupId int

set @ProductAttributeGroupId = (
	select pagmd.ProductAttributeGroupID from ProductAttributeGroupMetaData pagmd
	inner join ProductAttributeGroupName pagn
	on pagn.LanguageID = 1 and pagn.ProductAttributeGroupID = pagmd.ProductAttributeGroupID
	where VendorID = 48 and pagn.LanguageID = 1 and pagn.Name = 'General'
)


--insert SentToWehkampAsDummy attribute
if not exists
(
	select * from ProductAttributeMetaData
	where AttributeCode = 'SentToWehkampAsDummy' 
	and VendorID = 48
)
begin

	insert into ProductAttributeMetaData (attributeCode, ProductAttributeGroupId, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, CreatedBy, CreationTime, Mandatory, IsConfigurable)
	values ('SentToWehkampAsDummy', @ProductAttributeGroupId, 0, 0,0, 48, 0, 1, GETDATE(), 0, 0)

	insert into ProductAttributeName (AttributeID,LanguageID, Name)
	values(SCOPE_IDENTITY(), 1, 'SentToWehkampAsDummy')

end

