declare @PortalID int
declare @GroupID int


--add portal
if not exists (select * from Portal where Name = 'Wehkamp')
begin
	
	insert into Portal(Name)
	values('Wehkamp')

end


--add ManagementGroup
if not exists (select * from managementGroup where [group] = 'Wehkamp')
begin
	
	set @PortalID = (select PortalID from Portal where Name = 'Wehkamp') 

	insert into ManagementGroup([Group], PortalID)
	values('Wehkamp', @PortalID)

end



--add ManagementPage
if not exists (select * from ManagementPage where JSAction = 'ExcludeProducts')
begin
	
	set @GroupID = (select GroupID from managementGroup where [group] = 'Wehkamp') 

	insert into ManagementPage(Name, [Description], RoleID, JSAction, Icon, GroupID, ID, IsVisible, FunctionalityName)
	values('Exclude Products', 'Add begin of VendorItemNumber to Exclude', 1, 'ExcludeProducts', 'folder-gear', @GroupID, 'exclude-products', 1, 'GetExcludeProducts' )

end


	
