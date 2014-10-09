if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'Return/DifferenceShop') 
begin
  insert into vendorsetting (vendorid, settingkey, value)
  values (15, 'Return/DifferenceShop', '982')

end


if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'Return/DifferenceShop') 
begin
  insert into vendorsetting (vendorid, settingkey, value)
  values (25, 'Return/DifferenceShop', '1')

end
