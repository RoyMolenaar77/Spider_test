if not exists (select * from vendorsetting where settingkey = 'BaseRemoteMessagePath' and vendorid = 15)
begin 
  insert into vendorsetting values (15, 'BaseRemoteMessagePath', '\\10.97.1.51\export\web2\wehkamp')
	insert into vendorsetting values (15, 'RemotePathUsername', 'cat-dom\sa_concentrator')
	insert into vendorsetting values (15, 'RemotePathPassword', 'lk23n32vvf')
	insert into vendorsetting values (15, 'BaseLocalMessagePath', 'D:\PfaMessages')
	insert into vendorsetting values (15, 'ArchiveDirectoryPath', 'D:\PfaMessages\Archive')
end

if not exists (select * from vendorsetting where settingkey = 'BaseRemoteMessagePath' and vendorid = 25)
begin 
  insert into vendorsetting values (25, 'BaseRemoteMessagePath', '\\10.97.1.51\export\web2\wehkamp\at')
	insert into vendorsetting values (25, 'RemotePathUsername', 'cat-dom\sa_concentrator')
	insert into vendorsetting values (25, 'RemotePathPassword', 'lk23n32vvf')
	insert into vendorsetting values (25, 'BaseLocalMessagePath', 'D:\AT\PfaMessages')
	insert into vendorsetting values (25, 'ArchiveDirectoryPath', 'D:\AT\PfaMessages\Archive')
end