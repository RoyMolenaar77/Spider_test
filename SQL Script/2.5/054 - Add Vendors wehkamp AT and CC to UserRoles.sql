declare @WehkampATVendorID int
declare @WehkampCCVendorID int

declare @AdminRoleID int
declare @CustomerAdmin int
declare @ContentOnderhouden int

set @WehkampATVendorID = (select vendorid from Vendor where Name = 'Wehkamp AT')
set @WehkampCCVendorID = (select vendorid from Vendor where Name = 'Wehkamp CC')

set @AdminRoleID = (select RoleID from [Role] where RoleName = 'Administrator')
set @CustomerAdmin = (select RoleID from [Role] where RoleName = 'Customer admin')
set @ContentOnderhouden = (select RoleID from [Role] where RoleName = 'Content onderhouden')


--insert Administrator and Wehkamp AT
insert into UserRole
select distinct UserID, @AdminRoleID, @WehkampATVendorID from UserRole
where RoleID = @AdminRoleID and UserID not in (
	select distinct UserID from UserRole
	where VendorID = @WehkampATVendorID and RoleID = @AdminRoleID
)

--insert Administrator and Wehkamp CC
insert into UserRole
select distinct UserID, @AdminRoleID, @WehkampCCVendorID from UserRole
where RoleID = @AdminRoleID and UserID not in (
	select distinct UserID from UserRole
	where VendorID = @WehkampCCVendorID and RoleID = @AdminRoleID
)




--insert Customer admin and Wehkamp AT
insert into UserRole
select distinct UserID, @CustomerAdmin, @WehkampATVendorID from UserRole
where RoleID = @CustomerAdmin and UserID not in (
	select distinct UserID from UserRole
	where VendorID = @WehkampATVendorID and RoleID = @CustomerAdmin
)

--insert Customer admin and Wehkamp CC
insert into UserRole
select distinct UserID, @CustomerAdmin, @WehkampCCVendorID from UserRole
where RoleID = @CustomerAdmin and UserID not in (
	select distinct UserID from UserRole
	where VendorID = @WehkampCCVendorID and RoleID = @CustomerAdmin
)




--insert Content onderhouden and Wehkamp AT
insert into UserRole
select distinct UserID, @ContentOnderhouden, @WehkampATVendorID from UserRole
where RoleID = @ContentOnderhouden and UserID not in (
	select distinct UserID from UserRole
	where VendorID = @WehkampATVendorID and RoleID = @ContentOnderhouden
)

--insert Content onderhouden and Wehkamp CC
insert into UserRole
select distinct UserID, @ContentOnderhouden, @WehkampCCVendorID from UserRole
where RoleID = @ContentOnderhouden and UserID not in (
	select distinct UserID from UserRole
	where VendorID = @WehkampCCVendorID and RoleID = @ContentOnderhouden
)



