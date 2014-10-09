IF NOT exists (select * from vendorsetting where vendorid = 15 and settingkey = 'PFACurrencyCode')
  begin 
    insert into VendorSetting values (15, 'PFACurrencyCode', 'EURN')
    
    print 'Inserted PFACurrencyCode setting'
  end
else
  begin
    print 'Vendor setting already added'
  end


  IF NOT exists (select * from vendorsetting where vendorid = 25 and settingkey = 'PFACurrencyCode')
  begin 
    insert into VendorSetting values (25, 'PFACurrencyCode', 'EURN')
    
    print 'Inserted PFACurrencyCode setting'
  end
else
  begin
    print 'Vendor setting already added'
  end

