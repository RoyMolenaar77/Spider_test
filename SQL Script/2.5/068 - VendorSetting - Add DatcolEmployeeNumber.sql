if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'DatcolEmployeeNumber')
begin
	insert into vendorsetting values (15, 'DatcolEmployeeNumber', '0004')
end


if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'DatcolEmployeeNumber')
begin
	insert into vendorsetting values (25, 'DatcolEmployeeNumber', '8011')
end
