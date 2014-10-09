if not exists (select * from organization where name = 'Coolcat') begin
  insert into Organization values ('Coolcat')
end

if not exists (select * from organization where name = 'America Today') begin
  insert into Organization values ('America Today')
end

if not exists (select * from organization where name = 'Sapph') begin
  insert into Organization values ('Sapph')
end

