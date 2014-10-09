Concentrator.ContentStock = Ext.extend(Concentrator.GridAction, {

  getPanel: function () {

    var grid = new Concentrator.ui.ExcelGrid({
      pluralObjectName: 'Content Stocks',
      singularObjectName: 'Content Stock',
      primaryKey: ['VendorID', 'ConnectorID', 'VendorStockTypeID'],
      url: Concentrator.route("GetList", "ContentStock"),
      newUrl: Concentrator.route("Create", "ContentStock"),
      updateUrl: Concentrator.route("Update", "ContentStock"),
      deleteUrl: Concentrator.route("Delete", "ContentStock"),
      windowConfig: { height: 300 },
      permissions: {
        list: 'GetContentStock',
        create: 'CreateContentStock',
        remove: 'DeleteContentStock',
        update: 'UpdateContentStock'
      },
      structure: [
        {
          dataIndex: 'VendorID', type: 'int', header: 'Vendor',
          renderer: function (val, m, rec) {
            return rec.get('VendorName');
          },
          //renderer: Concentrator.renderers.field('allVendors', 'VendorName'),
          filter: { type: 'string', store: Concentrator.stores.vendors, filterField: 'VendorName' },
          editor: { xtype: 'vendor', allowBlank: false },
          editable: false
        },
        { dataIndex: 'VendorName', type: 'string' },
        {
          dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          editor: { xtype: 'connector', allowBlank: false },
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' },
          editable: false
        },
        {
          dataIndex: 'VendorStockTypeID', type: 'int',
          editor: { fieldLabel: 'Vendor Stock Type', xtype: 'vendorStockType', allowBlank: false },
          editable: false,
          renderer: function (val, m, rec) {
            return rec.get('StockType');
          }, header: 'Stock type'
        },
        { dataIndex: 'StockType', type: 'string', width: 200 }

      ]
    });
    return grid;
  }
});
