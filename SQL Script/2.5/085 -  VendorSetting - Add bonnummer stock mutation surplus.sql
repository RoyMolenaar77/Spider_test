if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'StockReceiptSalesslipNumber_Surplus') 
begin
  insert into vendorsetting (vendorid, settingkey, value)
  select vendorid, 'StockReceiptSalesslipNumber_Surplus', cast((cast(value as int) + 1) as nvarchar)
  from vendorsetting
  where vendorid = 15 and settingkey = 'StockReceiptSalesslipNumber'

end


if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'StockReceiptSalesslipNumber_Surplus') 
begin
  insert into vendorsetting (vendorid, settingkey, value)
  select vendorid, 'StockReceiptSalesslipNumber_Surplus', cast((cast(value as int) + 1) as nvarchar)
  from vendorsetting
  where vendorid = 25 and settingkey = 'StockReceiptSalesslipNumber'
end