if not exists (select * from vendorsetting where settingkey = 'PathForWehkampReturnOrders' and vendorid = 15)
  begin
    insert into vendorsetting values (15, 'PathForWehkampReturnOrders', 'D:\Concentrator_ftp\wehkamp_return_orders\cc')
  end

if not exists (select * from vendorsetting where settingkey = 'PathForWehkampReturnOrders' and vendorid = 25)
  begin
    insert into vendorsetting values (25, 'PathForWehkampReturnOrders', 'D:\Concentrator_ftp\wehkamp_return_orders\at')
  end

