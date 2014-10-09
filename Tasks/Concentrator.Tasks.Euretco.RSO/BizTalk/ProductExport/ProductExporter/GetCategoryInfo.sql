;
with MasterGroupMappingNames as (
select m.MasterGroupMappingID, splitName.Value, ROW_NUMBER() OVER ( PARTITION BY m.mastergroupmappingid ORDER BY splitname.value ) AS RowNumber
from MasterGroupMapping m
inner join MasterGroupMappingLanguage ml on m.MasterGroupMappingID = ml.MasterGroupMappingID and LanguageID = 3
cross apply SplitString(name, '_') splitName
where ConnectorID = {0}
)
select m.MasterGroupMappingID, ParentMasterGroupMappingID, mCode.Value as LevelID, mName.Value as LevelName
from MasterGroupMapping m 
left join MasterGroupMappingNames mCode on m.MasterGroupMappingID = mCode.MasterGroupMappingID and mCode.RowNumber = 1
left join MasterGroupMappingNames mName on m.MasterGroupMappingID = mName.MasterGroupMappingID and mName.RowNumber = 2

where m.MasterGroupMappingID = {1}
