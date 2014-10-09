IF NOT exists (select * from connectorsetting where connectorid = 5 and settingkey = 'SupportContentProductGroupExplosion')
  begin 
    insert into connectorsetting values (5, 'SupportContentProductGroupExplosion', 'True')
    
    print 'Inserted connector settings'
  end
else
  begin
    print 'Connector settings already added'
  end 