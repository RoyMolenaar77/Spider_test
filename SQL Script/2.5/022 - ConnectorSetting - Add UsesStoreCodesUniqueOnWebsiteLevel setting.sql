IF NOT exists (select * from connectorsetting where connectorid = 5 and settingkey = 'StoreCodesUniqueOnWebsiteLevel')
  begin 
    insert into connectorsetting values (5, 'StoreCodesUniqueOnWebsiteLevel', 'True')
    insert into connectorsetting values (7, 'StoreCodesUniqueOnWebsiteLevel', 'True')
    insert into connectorsetting values (8, 'StoreCodesUniqueOnWebsiteLevel', 'True')
    print 'Inserted connector settings'
  end
else
  begin
    print 'Connector settings already inserted'
  end