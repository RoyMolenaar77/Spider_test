Concentrator.PriceSets = Ext.extend(Concentrator.BaseAction, {

  editProductPriceSet: function (PriceSetID) {

    var ProductPriceSetGrid = new Diract.ui.Grid({
      pluralObjectName: 'Price Set Products',
      width: 150,
      singularObjectName: 'Price Set Product',
      primaryKey: ['PriceSetID', 'ProductID'],
      sortBy: 'ProductID',
      permissions: {
        create: 'CreatePriceSet',
        remove: 'DeletePriceSet',
        update: 'UpdatePriceSet',
        list: 'GetPriceSet'
      },
      params: {
        PriceSetID: PriceSetID
      },
      newParams: {
        PriceSetID: PriceSetID
      },
      url: Concentrator.route("GetList", "ProductPriceSet"),
      newUrl: Concentrator.route("Create", "ProductPriceSet"),
      deleteUrl: Concentrator.route("Delete", "ProductPriceSet"),
      updateUrl: Concentrator.route("Update", "ProductPriceSet"),
      structure:
            [
                { dataIndex: 'ProductID', header: 'Product',
                  renderer: function (v, m, record) {
                    return record.get('Product');
                  }, editable: false,
                  editor: {
                    xtype: 'product',
                    label: 'Product',
                    allowBlank: false,
                    name: 'ProductID'
                  }
                },
                { dataIndex: 'Product', type: 'string' },
                { dataIndex: 'Quantity', type: 'int', editor: { xtype: 'numberfield' }, header: 'Quantity' }
            ]
    });
    var priceSetWindow = new Ext.Window({
      modal: true,
      items: ProductPriceSetGrid,
      layout: 'fit',
      width: 600,
      height: 300
    });
    priceSetWindow.show();
  },

  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Price Sets',
      singularObjectName: 'Price Set',
      primaryKey: 'PriceSetID',
      url: Concentrator.route("GetList", "PriceSet"),
      newUrl: Concentrator.route("Create", "PriceSet"),
      updateUrl: Concentrator.route("Update", "PriceSet"),
      deleteUrl: Concentrator.route("Delete", "PriceSet"),
      permissions: {
        all: 'Default'
      },
      structure: [
        { dataIndex: 'PriceSetID', type: 'int' },
        { dataIndex: 'Name', type: 'string', header: 'Set Name', editor: { xtype: 'textfield'} },
        { dataIndex: 'Price', type: 'string', header: 'Set Price', editor: { xtype: 'numberfield' }, allowblank: true },
        { dataIndex: 'DiscountPercentage', type: 'string', header: 'Discount Percentage', editor: { xtype: 'numberfield' }, allowblank: true },
        { dataIndex: 'Description', type: 'string', header: 'Set description', editor: { xtype: 'textarea'} },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          editor: { xtype: 'connector', allowBlank: false },
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        }
      ],
      rowActions: [
        {
          iconCls: 'package',
          text: 'View Price set Products',
          handler: (function (record) {
            this.editProductPriceSet(record.get('PriceSetID'));
          }).createDelegate(this),
          roles: ["ViewPriceSet"]
        }
      ]
    });

    return grid;
  }
});