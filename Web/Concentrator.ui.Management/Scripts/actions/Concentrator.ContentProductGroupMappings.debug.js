Concentrator.ContentProductGroupMappings = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {
    var self = this;
    var grid = new Diract.ui.TreeGrid({
      hierarchicalColumn: 'ProductGroupID',
      hierarchicalIDParentDataIndex: 'ParentProductGroupMappingID',
      pluralObjectName: 'Content Product Group Mappings',
      singularObjectName: 'Content Product Group Mapping',
      primaryKey: ['ProductGroupMappingID', 'ConnectorID', 'ProductID'],
      eagerLoad: true,
      url: Concentrator.route("GetList", "ContentProductGroupMapping"),
      permissions: {
        list: 'GetProductGroupMapping',
        create: 'CreateProductGroupMapping',
        remove: 'DeleteProductGroupMapping',
        update: 'UpdateProductGroupMapping'
      },

      structure: [
                    { dataIndex: 'ProductGroupMappingID', type: 'int' },
                    { dataIndex: 'ProductGroupID', type: 'int', header: 'Content Product Group',
                      editable: false,
                      editor: { xtype: 'productgroup' },
                      renderer: function(val, m, rec) {
                        return rec.get('ProductGroupName');
                      },

                      filter: {
                        type: 'string',
                        filterField: 'ProductGroupName'

                      }
                    },
                    { dataIndex: 'ProductGroupName', type: 'string' },
      //                    { dataIndex: 'FlattenHierarchy', type: 'boolean', header: 'Flatten hierarchy', filter: { type: 'string' }, editor: { xtype: 'checkbox'}, 
      //                    renderer : function(val, m, rec){
      //                            return ''; }
      //                     },
      //                  //  { dataIndex: 'FilterByParentGroup', type: 'boolean', header: 'Filter By Parent', filter: { type: 'string' }, editor: { xtype: 'checkbox'} },
                    {dataIndex: 'ParentProductGroupMappingID', type: 'int' },
      //                    { dataIndex: 'Score', type: 'int', header: 'Score', renderer : function(val, m, rec){return ''}},
                    {dataIndex: 'ConnectorID', header: 'Connector', editable: false,
                    renderer: Concentrator.renderers.field('connectors', 'Name')/*function(val, m, r) { return r.get('Connector') }*/, editor: { xtype: 'connector' }
                  },
                    { dataIndex: 'ProductID', type: 'int', header: 'Product',
                      renderer: function(val, m, rec) {
                        return rec.get('ProductName');
                      }
                    },
                    { dataIndex: 'ProductName', type: 'string' }

            ]
      //            customButtons: [
      ////      {
      ////          text: 'Copy mappings',
      ////          iconCls: 'add',
      ////          handler: function() {
      ////              self.getCopyMappingsWindow();
      ////          }
      ////      }
      //    ]

    });

    return grid;
  }

  //    getCopyMappingsWindow: function() {
  //        var wi = new Diract.ui.FormWindow({
  //            url: Concentrator.route('Copy', 'ContentProductGroupMapping'),
  //            width: 400,
  //            height: 200,
  //            items: [
  //      {
  //          xtype: 'connector',
  //          fieldLabel: 'From Connector',
  //          hiddenName: 'sourceConnectorID'
  //      },
  //      {
  //          xtype: 'connector',
  //          fieldLabel: 'To Connector',
  //          hiddenName: 'destinationConnectorID'
  //      }
  //],
  //            success: function() {
  //                wi.destroy();
  //            }
  //        });

  //        wi.show();
  //    }
});