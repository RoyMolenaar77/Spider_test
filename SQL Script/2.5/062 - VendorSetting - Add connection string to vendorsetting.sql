if not exists(select * from vendorsetting where vendorid = 1 and settingkey = 'PfaConnectionString')
begin
	insert into vendorsetting values (1, 'PfaConnectionString', 'DSN=PROD_PFA_CC;PWD=progress')
end


if not exists(select * from vendorsetting where vendorid = 2 and settingkey = 'PfaConnectionString')
begin
	insert into vendorsetting values (2, 'PfaConnectionString', 'DSN=PROD_PFA_AT;PWD=progress')
end

if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'PfaConnectionString')
begin
	insert into vendorsetting values (15, 'PfaConnectionString', 'DSN=PROD_PFA_CC;PWD=progress')
end


if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'PfaConnectionString')
begin
	insert into vendorsetting values (25, 'PfaConnectionString', 'DSN=PROD_PFA_AT;PWD=progress')
end 