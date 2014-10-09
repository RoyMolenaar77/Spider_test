if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'SFTPSetting')
begin
	insert into vendorsetting values (15, 'SFTPSetting', 'sftp://sftp_coolcat@sftp1.wehkamp.nl:22')
end


if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'SFTPSetting')
begin
	insert into vendorsetting values (25, 'SFTPSetting', 'sftp://sftp_americatoday@sftp1.wehkamp.nl:22')
end




if not exists(select * from vendorsetting where vendorid = 15 and settingkey = 'PrivateKeyFileName')
begin
	insert into vendorsetting values (15, 'PrivateKeyFileName', 'Coolcat.ppk')
end


if not exists(select * from vendorsetting where vendorid = 25 and settingkey = 'PrivateKeyFileName')
begin
	insert into vendorsetting values (25, 'PrivateKeyFileName', 'AmericaToday.ppk')
end
