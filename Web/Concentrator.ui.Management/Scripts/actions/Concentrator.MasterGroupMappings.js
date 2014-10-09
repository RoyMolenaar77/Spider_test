Concentrator.MasterGroupMappings = Ext.extend(Concentrator.BaseAction, {
  needsConnector: true,
  listeners: {
    'beforeclose': function () {
      Concentrator.navigationPanel.expand(true);
    }
  },
  getPanel: function () {
    Concentrator.navigationPanel.collapse(true);

    return new Diract.ui.MTG({
      
      treeConfig: {
        loadDataUrl: Concentrator.route('GetTreeView', 'MasterGroupMapping'),
        rootText: 'Master group mapping',
        deleteUrl: Concentrator.route('Delete', 'MasterGroupMapping'),
        singularObjectName: 'Master group mapping',
        hierarchicalIndexID: 'MasterGroupMappingID',
        masterGroupID: 'MasterGroupID',
        productGroupID: 'ProductGroupID',
        hierarchicalParentIndexID: 'ParentMasterGroupMappingID',
        baseAttributes: { MasterGroupMappingID: -1, ConnectorID: this.connectorID, ProductGroupID: -1, MasterGroupID: 1 }
      },
      
      searchConfig: {
        fieldsToSearchIn: ['product', 'product group', 'product group mapping'],
        searchUrl: Concentrator.route('SearchTree', 'MasterGroupMapping')
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
          },
          {
            xtype: 'textfield',
            name: 'MasterGroupMappingLabel',
            width: 200,
            fieldLabel: 'Master group mapping label'
          },
          {
            xtype: 'textfield',
            name: 'CustomMasterGroupLabel',
            width: 200,
            fieldLabel: 'Custom Master Group Label'
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
            }       
          ]
      }
    });
  }
});