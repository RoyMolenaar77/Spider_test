MERGE FunctionalityRole AS TARGET
USING (
	SELECT roleID
		,functionalityname
	FROM (
		VALUES ('AssignUserToMasterGroupMapping')
			,('CopyMasterGroupMapping')
			,('CreateMasterGroupMapping')
			,('DefaultMasterGroupMapping')
			,('DeleteMasterGroupMapping')
			,('GetMasterGroupMapping')
			,('GetMasterGroupMappingDescription')
      ,('ManageSeoTexts')
			,('MasterGroupMappingAddConnectorMapping')
			,('MasterGroupMappingAddProductGroupMapping')
			,('MasterGroupMappingAttributeManagement')
			,('MasterGroupMappingAttributeSelectorManagement')
			,('MasterGroupMappingChooseSource')
			,('MasterGroupMappingControlAllProductsWizard')
			,('MasterGroupMappingCopyProductGroupMapping')
			,('MasterGroupMappingCrossReferenceManagement')
			,('MasterGroupMappingDeleteConnectorMapping')
			,('MasterGroupMappingDeleteProductGroupMapping')
			,('MasterGroupMappingFindSource')
			,('MasterGroupMappingLanguageWizard')
			,('MasterGroupMappingMoveProductGroupMapping')
			,('MasterGroupMappingProductControl')
			,('MasterGroupMappingProductControleManagement')
			,('MasterGroupMappingProductGroupSettings')
			,('MasterGroupMappingRelatedManagement')
			,('MasterGroupMappingRenameConnectorMapping')
			,('MasterGroupMappingVendorProducsManagement')
			,('MasterGroupMappingVendorProductGroupsManagement')
			,('MasterGroupMappingViewConnectorMapping')
			,('MasterGroupMappingViewConnectorPublicationRule')
			,('MasterGroupMappingViewGroupAttributeMapping')
			,('MasterGroupMappingViewPriceRule')
			,('MasterGroupMappingViewPriceTagMapping')
			,('MasterGroupMappingViewProducts')
			,('MasterGroupMappingViewVendorProductGroups')
			,('MasterGroupMappingViewVendorProducts')
			,('SetMasterGroupMappingDescription')
			,('UpdateMasterGroupMapping')
			,('View_MasterGroupMappings')
		) X(FunctionalityName)
	LEFT JOIN [Role] r ON r.RoleID > 0
	) AS SOURCE
	ON TARGET.RoleID = SOURCE.RoleID
		AND TARGET.FunctionalityName = SOURCE.FunctionalityName
WHEN NOT MATCHED
	THEN
		INSERT (
			RoleID
			,FunctionalityName
			)
		VALUES (
			SOURCE.RoleID
			,SOURCE.FunctionalityName
			);