if not exists(select * from connectorsetting where connectorid= 13 and settingkey = 'ReturnCodeOverride')
 begin
  insert into connectorsetting values (13, 'ReturnCodeOverride', '27')
end