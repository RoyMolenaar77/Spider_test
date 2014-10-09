if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'Stock/DifferenceShop')
begin
	insert into vendorsetting values (15, 'Stock/DifferenceShop', '982')
end


if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'Stock/DifferenceShop')
begin
	insert into vendorsetting values (25, 'Stock/DifferenceShop', '97')
end
