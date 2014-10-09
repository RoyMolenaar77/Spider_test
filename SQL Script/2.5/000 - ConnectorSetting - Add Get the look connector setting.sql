IF NOT exists (select * from connectorsetting where connectorid = 5 and settingkey = 'EnableGetTheLook')
  begin 
    insert into connectorsetting values (5, 'EnableGetTheLook', 'True')
    insert into connectorsetting values (7, 'EnableGetTheLook', 'True')
    insert into connectorsetting values (8, 'EnableGetTheLook', 'True')
    print 'Inserted connector settings'
  end
else
  begin
    print 'Connector settings already added'
  end