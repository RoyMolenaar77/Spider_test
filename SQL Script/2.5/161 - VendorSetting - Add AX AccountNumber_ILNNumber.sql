if not exists (select * from vendorsetting where settingkey = 'AxaptaAccountNumber' and vendorid = 50)
  begin
    insert into vendorsetting values (50, 	'AxaptaAccountNumber',	'310000')
    insert into vendorsetting values (50, 	'AxaptaILNNumber',	'8717890000158')
  end

if not exists (select * from vendorsetting where settingkey = 'AxaptaAccountNumber' and vendorid = 51)
  begin
    insert into vendorsetting values (51, 	'AxaptaAccountNumber',	'320000')
insert into vendorsetting values (51, 	'AxaptaILNNumber',	'8717890000170')
  end

