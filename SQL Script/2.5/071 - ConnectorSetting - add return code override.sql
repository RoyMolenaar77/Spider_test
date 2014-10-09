if not exists(select * from connectorsetting where connectorid = 14 and settingkey = 'ReturnCodeOverride')
  begin 
    insert into connectorsetting values (14, 'ReturnCodeOverride', '6')
    end
