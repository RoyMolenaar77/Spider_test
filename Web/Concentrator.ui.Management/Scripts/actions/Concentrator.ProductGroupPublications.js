Concentrator.ProductGroupPublications = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Product Group Publications',
      singularObjectName: 'Product Group Publication',
      primaryKey: ['ProductGroupID', 'Published', 'ConnectorID'],
      sortBy: 'ProductGroupID',
      url: Concentrator.route("GetList", "ProductGroupPublish"),
      permissions: {
        list: 'GetProductGroup',
        create: 'CreateProductGroup',
        remove: 'DeleteProductGroup',
        update: 'UpdateProductGroup'
      },
      updateUrl: Concentrator.route("Update", "ProductGroupPublish"),
      newUrl: Concentrator.route("Create", "ProductGroupPublish"),
      deleteUrl: Concentrator.route("Delete", "ProductGroupPublish"),
      groupField: "PowerUser",
      structure: [
                { dataIndex: 'ProductGroupID', type: 'int', header: "Product Group ID", width: 25,
                  editable: false,

                  editor: {
                    xtype: 'productgroup',
                    allowBlank: false
                  }
                },
                { dataIndex: 'ProductGroupName', type: 'string', header: "English Name", width: 100 },
                { dataIndex: 'Published', type: 'boolean', header: "Gepubliceerd", width: 100, editable: true },
                { dataIndex: 'ConnectorID', type: 'int', width: 100, header: 'Connector',
                  renderer: Concentrator.renderers.field('connectors', 'Name'),
                  editor: {
                    xtype: 'connector'
                  },
                  filter: {
                    type: 'string',
                    filterField: 'ConnectorName'
                  }
                },
                { dataIndex: 'ConnectorName' }

        ]
    });

    return grid;
  }
});