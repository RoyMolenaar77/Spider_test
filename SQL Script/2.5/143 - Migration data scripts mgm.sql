DECLARE @intErrorCode INT

BEGIN TRAN

print('Start delete duplicate products in content product group')

delete from Contentproductgroup where contentproductgroupid in (

select cpg.ContentProductGroupID from Contentproductgroup cpg
inner join 
(
select productid, productgroupmappingid, connectorid  from contentproductgroup 
group by productid, productgroupmappingid, connectorid 
having count(*) > 1) cpg1 on cpg.productid = cpg1.ProductID and cpg.ProductGroupMappingID = cpg1.ProductGroupMappingID and cpg.connectorid = cpg1.connectorid
where cpg.iscustom = 1 and MasterGroupMappingID is null

)

print('Finished delete duplicate products in content product group')

print('Start migrating products for source mapping')

--MIGRATE Products main
Merge MasterGroupMappingProduct AS TARGET
USING (select distinct ProductID, connMapp.SourceMasterGroupMappingID as MasterGroupMappingID, IsCustom, IsExported from ContentProductGroup cp
inner join ProductGroupMapping pMap on pMap.ProductGroupMappingID = cp.ProductGroupMappingID
INNER JOIN MasterGroupMapping connMapp on connMapp.SourceMasterGroupMappingID = pMap.MasterGroupMappingID and cp.ConnectorID = connMapp.ConnectorID
WHERE cp.MasterGroupMappingID is null) as SOURCE
ON TARGET.MasterGroupMappingID = SOURCE.MasterGroupMappingID and TARGET.ProductID = SOURCE.ProductID
WHEN NOT MATCHED
THEN INSERT (MasterGroupMappingID, ProductID, IsApproved, IsCustom, IsProductMapped)
VALUES (SOURCE.MasterGroupMappingID, SOURCE.ProductID, SOURCE.IsExported, SOURCE.IsCustom, 1);

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

print('Finished migrating products for source mapping')

print('Start migrating products for connector mapping')

--rollback tran


--MIGRATE Products connector
Merge MasterGroupMappingProduct AS TARGET
USING (select distinct ProductID, connMapp.MasterGroupMappingID, IsCustom, IsExported, cp.ConnectorID, cp.ProductGroupMappingID from ContentProductGroup cp
inner join ProductGroupMapping pMap on pMap.ProductGroupMappingID = cp.ProductGroupMappingID
INNER JOIN MasterGroupMapping connMapp on connMapp.SourceMasterGroupMappingID = pMap.MasterGroupMappingID and cp.ConnectorID = connMapp.ConnectorID
WHERE cp.MasterGroupMappingID is null) as SOURCE
ON TARGET.MasterGroupMappingID = SOURCE.MasterGroupMappingID and TARGET.ProductID = SOURCE.ProductID
WHEN NOT MATCHED
THEN INSERT (MasterGroupMappingID, ProductID, IsApproved, IsCustom, IsProductMapped)
VALUES (SOURCE.MasterGroupMappingID, SOURCE.ProductID, SOURCE.IsExported, SOURCE.IsCustom, 1);

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

print('Finished migrating products for connector mapping')

print('Start migrating custom labels')

--MIGRATE Custom labels
Merge MasterGroupMappingCustomLabel AS TARGET
USING (select LanguageID, CustomLabel, connMapp.MasterGroupMappingID, pGroupLab.ConnectorID from ProductGroupMappingCustomLabel pGroupLab
INNER JOIN ProductGroupMapping prodMap on prodMap.ProductGroupMappingID = pGroupLab.ProductGroupMappingID
INNER JOIN MasterGroupMapping connMapp on connMapp.SourceMasterGroupMappingID = prodMap.MasterGroupMappingID
) as SOURCE
on SOURCE.MasterGroupMappingID = TARGET.MasterGroupMappingID and SOURCE.ConnectorID = TARGET.ConnectorID
WHEN NOT MATCHED
THEN INSERT (MasterGroupMappingID, LanguageID, ConnectorID, CustomLabel)
VALUES(SOURCE.MasterGroupMappingID, SOURCE.LanguageID, SOURCE.ConnectorID, SOURCE.CustomLabel);

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

print('Finished migrating custom labels')

print('Start migrating descriptions')

--MIGRATE Descriptions
Merge MasterGroupMappingDescription AS TARGET
USING (select connMapp.MasterGroupMappingID, LanguageID, Description FROM ProductGroupMappingDescription prodDesc
INNER JOIN ProductGroupMapping prodMap on prodMap.ProductGroupMappingID = prodDesc.ProductGroupMappingID
INNER JOIN MasterGroupMapping connMapp on connMapp.SourceMasterGroupMappingID = prodMap.MasterGroupMappingID
) as SOURCE
on SOURCE.MasterGroupMappingID = TARGET.MasterGroupMappingID and SOURCE.LanguageID = TARGET.LanguageID
WHEN NOT MATCHED
THEN INSERT (MasterGroupMappingID, LanguageID, Description)
VALUES(SOURCE.MasterGroupMappingID, SOURCE.LanguageID, SOURCE.Description);

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

print('Finished migrating descriptions')

print('Start migrating magento settings')

--MIGRATE MAGENTO SETTINGS
Merge MagentoProductGroupSetting AS TARGET
USING (select connMapp.MasterGroupMappingID, ShowInMenu, DisabledMenu, IsAnchor, CreatedBy from MagentoProductGroupSetting magProdSet
INNER JOIN ProductGroupMapping prodMap on prodMap.ProductGroupMappingID = magProdSet.ProductGroupMappingID 
INNER JOIN MasterGroupMapping connMapp on connMapp.SourceMasterGroupMappingID = prodMap.MasterGroupMappingID
WHERE magProdSet.MasterGroupMappingID is null
) as SOURCE
on TARGET.MasterGroupMappingID = SOURCE.MasterGroupMappingID
WHEN NOT MATCHED
THEN INSERT(MasterGroupMappingID, ShowInMenu, DisabledMenu, IsAnchor, CreatedBy, CreationTime)
VALUES (SOURCE.MasterGroupMappingID, SOURCE.ShowInMenu, SOURCE.DisabledMenu, SOURCE.IsAnchor, SOURCE.CreatedBy, GETDATE());

	SELECT @intErrorCode = @@ERROR
	IF (@intErrorCode <> 0)
		GOTO PROBLEM

print('Finished migrating magento settings')

print('Start migrating ContentProductGroup for MasterGroupMapping')

MERGE ContentProductGroup as TARGET
USING (select connMapp.MasterGroupMappingID as MappingID, cp.* from ContentProductGroup cp
inner join ProductGroupMapping pMap on pMap.ProductGroupMappingID = cp.ProductGroupMappingID
INNER JOIN MasterGroupMapping connMapp on connMapp.SourceMasterGroupMappingID = pMap.MasterGroupMappingID and cp.ConnectorID = connMapp.ConnectorID) AS SOURCE
ON TARGET.ProductID = SOURCE.ProductID and TARGET.MasterGroupMappingID = SOURCE.MappingID and TARGET.ProductGroupMappingID = SOURCE.ProductGroupMappingID 
and TARGET.IsCustom = SOURCE.IsCustom and TARGET.[Exists] = SOURCE.[Exists] and TARGET.ConnectorID = SOURCE.ConnectorID and TARGET.IsExported = SOURCE.IsExported
WHEN NOT MATCHED
THEN INSERT(ConnectorID, ProductID, ProductGroupMappingID, CreatedBy, CreationTime, IsCustom, [Exists], IsExported, MasterGroupMappingID)
values( SOURCE.ConnectorID, SOURCE.ProductID, SOURCE.ProductGroupMappingID, 1, GETDATE(), SOURCE.IsCustom, SOURCE.[Exists], SOURCE.IsExported, SOURCE.MappingID);

print('Finish migrating ContentProductGroup for MasterGroupMapping')

print('Start migrating ContentProductGroup for ChildConnector MasterGroupMapping')

MERGE ContentProductGroup as TARGET
USING (select connMapp.MasterGroupMappingID as MappingID, cp.* from ContentProductGroup cp
inner join ProductGroupMapping pMap on pMap.ProductGroupMappingID = cp.ProductGroupMappingID
INNER join Connector c on cp.ConnectorID = c.ConnectorID and c.ParentConnectorID is not null
INNER JOIN MasterGroupMapping connMapp on connMapp.SourceMasterGroupMappingID = pMap.MasterGroupMappingID and c.ParentConnectorID = connMapp.ConnectorID

) AS SOURCE
ON TARGET.ProductID = SOURCE.ProductID and TARGET.MasterGroupMappingID = SOURCE.MappingID and TARGET.ProductGroupMappingID = SOURCE.ProductGroupMappingID 
and TARGET.IsCustom = SOURCE.IsCustom and TARGET.[Exists] = SOURCE.[Exists] and TARGET.ConnectorID = SOURCE.ConnectorID and TARGET.IsExported = SOURCE.IsExported
WHEN NOT MATCHED
THEN INSERT(ConnectorID, ProductID, ProductGroupMappingID, CreatedBy, CreationTime, IsCustom, [Exists], IsExported, MasterGroupMappingID)
values( SOURCE.ConnectorID, SOURCE.ProductID, SOURCE.ProductGroupMappingID, 1, GETDATE(), SOURCE.IsCustom, SOURCE.[Exists], SOURCE.IsExported, SOURCE.MappingID);

print('Finish migrating ContentProductGroup for ChildConnector MasterGroupMapping')

COMMIT TRAN
print('NO ERRORS, EVERYTHING COMMITED')
PROBLEM:

IF (@intErrorCode <> 0)
BEGIN
	PRINT 'Unexpected error occurred while migrating productgroupmapping data'

	ROLLBACK TRAN
	END