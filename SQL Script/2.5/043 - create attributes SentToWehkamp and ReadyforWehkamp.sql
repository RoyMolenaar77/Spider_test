
--declare @ProductAttributeGroupId int

--set @ProductAttributeGroupId = (
--	select pagmd.ProductAttributeGroupID from ProductAttributeGroupMetaData pagmd
--	inner join ProductAttributeGroupName pagn
--	on pagn.LanguageID = 1 and pagn.ProductAttributeGroupID = pagmd.ProductAttributeGroupID
--	where VendorID = 48 and pagn.LanguageID = 1 and pagn.Name = 'General'
--)

----insert ReadyForWehkamp attribute
--if not exists
--(
--	select * from ProductAttributeMetaData
--	where AttributeCode = 'ReadyForWehkamp' 
--	and VendorID = 48
--)
--begin

--	insert into ProductAttributeMetaData (attributeCode, ProductAttributeGroupId, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, CreatedBy, CreationTime, Mandatory, IsConfigurable)
--	values ('ReadyForWehkamp', @ProductAttributeGroupId, 0, 0,0, 48, 0, 1, GETDATE(), 0, 0)

--	insert into ProductAttributeName (AttributeID,LanguageID, Name)
--	values(SCOPE_IDENTITY(), 1, 'ReadyForWehkamp')

--end


----insert SentToWehkamp attribute
--if not exists
--(
--	select * from ProductAttributeMetaData
--	where AttributeCode = 'SentToWehkamp' 
--	and VendorID = 48
--)
--begin

--	insert into ProductAttributeMetaData (attributeCode, ProductAttributeGroupId, [Index], IsVisible, NeedsUpdate, VendorID, IsSearchable, CreatedBy, CreationTime, Mandatory, IsConfigurable)
--	values ('SentToWehkamp', @ProductAttributeGroupId, 0, 0,0, 48, 0, 1, GETDATE(), 0, 0)

--	insert into ProductAttributeName (AttributeID,LanguageID, Name)
--	values(SCOPE_IDENTITY(), 1, 'SentToWehkamp')

--end

