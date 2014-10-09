Concentrator.AdditionalOrderProduct = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Additional Order Products',
      singularObjectName: 'Additional Order Product',
      primaryKey: 'AdditionalOrderProductID',
      permissions: {
        list: 'GetAdditionalOrderProducts',
        create: 'CreateAdditionalOrderProducts',
        remove: 'DeleteAdditionalOrderProducts',
        update: 'UpdateAdditionalOrderProducts'
      },
      url: Concentrator.route("GetList", "AdditionalOrderProduct"),
      newUrl: Concentrator.route("Create", "AdditionalOrderProduct"),
      updateUrl: Concentrator.route("Update", "AdditionalOrderProduct"),
      deleteUrl: Concentrator.route("Delete", "AdditionalOrderProduct"),
      structure: [
        { dataIndex: 'AdditionalOrderProductID', type: 'int' },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector', editor: { xtype: 'connector', allowBlank: false },
          renderer: function(val, meta, record) {
            return record.get("Connector");
          },
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'Connector', type: 'string' },
        { dataIndex: 'ConnectorProductID', type: 'string', header: 'Connector Product ID', editor: { xtype: 'int', allowBlank: 'false'} },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor', editor: { xtype: 'vendor', allowBlank: false },
          renderer: function(val, meta, record) {
            return record.get('Vendor');
          }
        },
        { dataIndex: 'Vendor', type: 'string' },
        { dataIndex: 'VendorProductID', type: 'string', header: 'Vendor Product ID', editor: { xtype: 'int', allowBlank: false} },
        { dataIndex: 'UnitPrice', type: 'float', header: 'Unit Price', editor: { xtype: 'numberfield', allowBlank: false} },
        { dataIndex: 'CreatedBy', type: 'int', header: 'Created By' },
        { dataIndex: 'CreationTime', type: 'date', header: 'Creation Time' },
        { dataIndex: 'LastModifiedBy', type: 'int', header: 'Last Modified By' },
        { dataIndex: 'LastModificationTime', type: 'date', header: 'Last Modification Time' }
      ]
    });

    return grid;
  },
  //overrides
  refresh: function () {
    
  }


});