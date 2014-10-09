Concentrator.MasterGroupMappingsv2 = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    return new Diract.ui.GroupMappings({

      treeConfig: {
        loadDataUrl: Concentrator.route('GetTreeView', 'MasterGroupMapping'),
        rootText: 'Master group mapping',
        deleteUrl: Concentrator.route('Delete', 'MasterGroupMapping'),
        singularObjectName: 'Master group mapping',
        hierarchicalIndexID: 'MasterGroupMappingID',
        productGroupID: 'ProductGroupID',
        hierarchicalParentIndexID: 'ParentMasterGroupMappingID',
        baseAttributes: { MasterGroupMappingID: -1, ProductGroupID: -1 }
      },

      newEntityFormConfig: {
        height: 320,
        fileUpload: true,
        ref: this,
        url: Concentrator.route('Create', 'MasterGroupMapping'),
        items: [
          {
            xtype: 'productgroup',
            fieldLabel: 'Product group',
            hiddenName: 'ProductGroupID'
          }
        ]
      },

      entityFormConfig: {
        url: Concentrator.route('Update', 'MasterGroupMapping'),
        fileUpload: true,
        ref: this,
        loadUrl: Concentrator.route('Get', 'MasterGroupMapping'),
        items: [
        {
          xtype: 'hidden',
          name: 'MasterGroupMappingID',
          fieldLabel: 'Master group mapping ID',
          readOnly: true
        },
        {
          xtype: 'hidden',
          name: 'MasterGroupID'
        },
        {
          xtype: 'textfield',
          name: 'MasterGroupName',
          readOnly: true,
          fieldLabel: 'Master group'
        },
        {
          xtype: 'hidden',
          name: 'ConnectorName',
          readOnly: true,
          fieldLabel: 'Connector'
        },
        {
          xtype: 'textfield',
          name: 'MasterGroupMappingLabel',
          fieldLabel: 'Master group mapping label'
        },
        {
          xtype: 'textfield',
          name: 'CustomMasterGroupLabel',
          fieldLabel: 'Custom Master Group Label'
        }]
      }
    });
  }
});