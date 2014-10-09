if exists (select * from sys.tables where name = 'selectorattribute')
begin
  drop table selectorattribute
end

if exists (select * from sys.tables where name = 'productgroupselector')
begin
  drop table productgroupselector
end

if exists (select * from sys.tables where name = 'selectorstep')
begin
  drop table selectorstep
end

if exists (select * from sys.tables where name = 'selector')
begin
  drop table selector
end

if exists (select * from sys.views where name = 'SelectorImportProductView')
begin
  drop table SelectorImportProductView
end

if exists (select * from sys.views where name = 'SelectorProductAttributeView')
begin
  drop table SelectorProductAttributeView
end

if exists (select * from sys.views where name = 'SelectorProductView')
begin
  drop table SelectorProductView
end