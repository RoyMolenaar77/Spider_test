if not exists (select * from sys.tables where name = 'Organization')
begin
create table Organization (
	Id int not null identity (1,1) primary key,
	Name nvarchar(100) not null
)

insert into Organization values ('Default')

end

