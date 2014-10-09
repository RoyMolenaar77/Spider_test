Concentrator.Vendors = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Vendor',
      pluralObjectName: 'Vendors',
      permissions: {
        update: 'UpdateVendor',
        list: 'GetVendor'
      },
      url: Concentrator.route('GetList', 'Vendor'),
      updateUrl: Concentrator.route('Update', 'Vendor'),
      primaryKey: 'VendorID',
      sortBy: 'VendorID',
      structure: [
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor', renderer: Concentrator.renderers.field('vendors', 'VendorName'), filter: { type: 'string', store: Concentrator.stores.vendors, filterField: 'VendorName'} },
        { dataIndex: 'ParentVendorID', type: 'int', header: 'Parent Vendor', renderer: Concentrator.renderers.field('vendors', 'VendorName'),
          filter: {
            type: 'string',
            store: Concentrator.stores.vendors,
            filterField: 'VendorName'
          },
          editor: {
            xtype: 'vendor'
          }
        },
        { dataIndex: 'BackendVendorCode', type: 'string', header: 'Backend Vendor Code' },
        { dataIndex: 'IsActive', type: 'boolean', header: 'Is Active', editable : true },
        { dataIndex: 'CutOffTime', type: 'date', header: 'Cut Off Time' },
        { dataIndex: 'DeliveryHours', type: 'int', header: 'Delivery Hours' },
        { dataIndex: 'isAssortment', type: 'boolean', header: 'Assortment', editable: true, editor: { xtype: 'checkbox' } },
        { dataIndex: 'isContent', type: 'boolean', header: 'Content', editable: true, editor: { xtype: 'checkbox'} },
        { dataIndex: 'CDPrice', type: 'float', header: 'Central Delivery Price', editor: { xtype: 'textfield', allowBlank: true },
          renderer: Concentrator.renderers.euro()
        },
        { dataIndex: 'DSPrice', type: 'float', header: 'Drop Shipment Price', editor: { xtype: 'textfield', allowBlank: true },
          renderer: Concentrator.renderers.euro()
        }
      ]
    });

    return grid;
  }
});