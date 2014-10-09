if not exists(select * from vendorsetting where vendorid = 1 and settingkey = 'PfaCountryCode')
begin
	insert into vendorsetting values (1, 'PfaCountryCode', 'NL')
end

if not exists(select * from vendorsetting where vendorid = 2 and settingkey = 'PfaCountryCode')
begin
	insert into vendorsetting values (2, 'PfaCountryCode', 'NL')
end

if not exists(select * from vendorsetting where vendorid = 13 and settingkey = 'PfaCountryCode')
begin
	insert into vendorsetting values (13, 'PfaCountryCode', 'BEL')
end

if not exists(select * from vendorsetting where vendorid = 14 and settingkey = 'PfaCountryCode')
begin
	insert into vendorsetting values (14, 'PfaCountryCode', 'FRA')
end

if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'PfaCountryCode')
begin
	insert into vendorsetting values (15, 'PfaCountryCode', 'NL')
end

if not exists(select * from vendorsetting where vendorid = 23 and settingkey = 'PfaCountryCode')
begin
	insert into vendorsetting values (23, 'PfaCountryCode', 'BEL')
end

if not exists(select * from vendorsetting where vendorid = 24 and settingkey = 'PfaCountryCode')
begin
	insert into vendorsetting values (24, 'PfaCountryCode', 'FRA')
end

if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'PfaCountryCode')
begin
	insert into vendorsetting values (25, 'PfaCountryCode', 'NL')
end
