IF NOT exists (select * from vendorsetting where vendorid = 15 and settingkey = 'NonSoldenVendor')
  begin 
    insert into VendorSetting values (15, 'NonSoldenVendor', 'true')
    
    print 'Inserted Non solden vendor setting'
  end
else
  begin
    print 'Vendor setting already added'
  end


  IF NOT exists (select * from vendorsetting where vendorid = 25 and settingkey = 'NonSoldenVendor')
  begin 
    insert into VendorSetting values (25, 'NonSoldenVendor', 'true')
    
    print 'Inserted Non solden vendor setting'
  end
else
  begin
    print 'Vendor setting already added'
  end