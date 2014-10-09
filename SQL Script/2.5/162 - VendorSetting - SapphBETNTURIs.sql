if not exists (select * from vendorsetting where settingkey = 'TNTDestinationURI' and vendorid = 51)
  begin
  insert into vendorsetting values (51	,'TNTDestinationURI',	'ftp://sapph:D5277qwl@62.177.226.211:21/test')
  insert into vendorsetting values (51	,'TNTSourceURI',	'ftp://sapph:D5277qwl@62.177.226.211:21/Test/Out')
end


