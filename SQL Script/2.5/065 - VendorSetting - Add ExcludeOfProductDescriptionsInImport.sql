if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'VendorDoesNotImportDescriptions')
begin
	insert into vendorsetting values (15, 'VendorDoesNotImportDescriptions', 'true')
end


if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'VendorDoesNotImportDescriptions')
begin
	insert into vendorsetting values (25, 'VendorDoesNotImportDescriptions', 'true')
end
