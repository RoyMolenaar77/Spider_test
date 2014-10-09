Concentrator.ContentProductGroups = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Content Product Groups',
      singularObjectName: 'Content Product Group',
      primaryKey: ['ProductID', 'ProductGroupID'],
      sortBy: 'ProductID',
      // THIS IS NOT BEING USED
      url: Concentrator.route("GetList", "ContentProductGroup"),
      updateUrl: Concentrator.route("Update", "ContentProductGroup"),
      newUrl: Concentrator.route("Create", "ContentProductGroup"),
      deleteUrl: Concentrator.route("Delete", "ContentProductGroup"),
      permissions: {
        list: 'GetProductGroup',
        create: 'CreateProductGroup',
        remove: 'DeleteProductGroup',
        update: 'UpdateProductGroup'
      },
      structure: [
                { dataIndex: 'ProductID', type: 'int', header: 'Product ID',
                  editable: false,
                  editor: {
                    xtype: 'product',
                    allowBlank: false
                  }
                },
                { dataIndex: 'ProductName', header: 'Product Description' },
                { dataIndex: 'ProductGroupID', type: 'int', header: "Product Group", width: 45,
                  renderer: Concentrator.renderers.field('productGroups', 'Name'),
                  editor: {
                    allowBlank: false,
                    xtype: 'productgroup'
                  },
                  filter: {
                    type: 'string',
                    filterField: 'ProductGroupName'
                  }
                },
                { dataIndex: 'ProductGroupName' }
        ]
    });

    return grid;
  }
});