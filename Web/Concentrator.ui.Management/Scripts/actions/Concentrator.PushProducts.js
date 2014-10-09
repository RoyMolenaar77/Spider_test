Concentrator.PushProducts = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {
    var grid = new Diract.ui.Grid({
      singularObjectName: 'Product',
      pluralObjectName: 'Products',
      permissions: { all: 'PushProducts' },
      url: Concentrator.route('GetList', 'PushProduct'),
      newUrl: Concentrator.route('Create', 'PushProduct'),
      customButtons: [
                      { xtype: 'tMButton', grid: function() { return grid; }, mappingField: 'Processed', unmappedText: 'Show all unprocessed products' },
                      { xtype: 'button', iconCls: 'outbox-out', text: 'Push all unprocessed products',
                        handler: function() {
                          Diract.request({
                            url: Concentrator.route('PushProducts', 'PushProduct')
                          });
                        }
                      }
                     ],
      deleteUrl: Concentrator.route('Delete', 'PushProduct'),
      primaryKey: 'PushProductID',
      structure: [
        { dataIndex: 'PushProductID', type: 'int' },
        { dataIndex: 'ProductID', type: 'int', renderer: function(val, m, rec) { return rec.get('ProductName') }, header: 'Product', editor: { xtype: 'product', allowBlank: true }, editable: false, filter: { type: 'string'} },
        { dataIndex: 'ProductName', type: 'string' },
        { dataIndex: 'ConnectorID', type: 'int', renderer: Concentrator.renderers.field('connectors', 'Name'), header: 'Connector', 
          editor: { xtype: 'connector' },
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' } 
        },
        { dataIndex: 'VendorID', type: 'int', renderer: Concentrator.renderers.field('vendors', 'Name'), header: 'Vendor', editor: { xtype: 'vendor'} },
        { dataIndex: 'Processed', type: 'boolean', header: 'Processed' },
        { dataIndex: 'CustomItemNumber', type: 'string', header: 'Item number', editor: { xtype: 'textfield'} },
        { dataIndex: 'LastPushDate', header: 'Last push date', type: 'date', renderer: Ext.util.Format.dateRenderer('d-m-Y  g:i a') }
      ]
    });

    return grid;
  }
});