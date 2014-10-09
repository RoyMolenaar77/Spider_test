insert into config (name,value,description)
values ('DatabseVersion',1.0,'Concentrator Database version')


alter table contentpricecalculation drop column roundonbeforedecimal
alter table contentpricecalculation drop column roundafterdecimal
alter table contentpricecalculation drop column digitsbeforedecimal
alter table contentpricecalculation drop column digitsafterdecimal

alter table contentpricecalculation add calcuation nvarchar(255) not null
alter table connectorpublication alter column statusid int null