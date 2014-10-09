IF NOT exists (select * from connectorsetting where connectorid = 5 and settingkey = 'CompleteTheLookConnector')
  begin 
    insert into connectorsetting values (5, 'CompleteTheLookConnector', 'True')
    insert into connectorsetting values (7, 'CompleteTheLookConnector', 'True')
    insert into connectorsetting values (8, 'CompleteTheLookConnector', 'True')
    print 'Inserted connector settings'
  end
else
  begin
    print 'Connector settings already added'
  end