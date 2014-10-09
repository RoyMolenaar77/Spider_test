Concentrator.PreferredConnectorVendors = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Preferred Connector Vendor',
      pluralObjectName: 'Preferred Connector Vendors',
      permissions: {
        list: 'GetPreferredConnectorVendor',
        create: 'CreatePreferredConnectorVendor',
        update: 'UpdatePreferredConnectorVendor',
        remove: 'DeletePreferredConnectorVendor'
      },
      url: Concentrator.route('GetList', 'PreferredConnectorVendor'),
      newUrl: Concentrator.route("Create", "PreferredConnectorVendor"),
      deleteUrl: Concentrator.route("Delete", "PreferredConnectorVendor"),
      updateUrl: Concentrator.route('Update', 'PreferredConnectorVendor'),
      primaryKey: ['VendorID', 'ConnectorID'],
      sortBy: 'ConnectorID',
      params: {
        vendorID: 'VendorID',
        connectorID: 'ConnectorID'
      },
      structure: [
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor',          
          editor: { xtype: 'vendor', allowBlank: false },
          renderer: Concentrator.renderers.field('vendors', 'VendorName'), filter: {
            type: 'list',
            store: Concentrator.stores.vendors,
            labelField: 'VendorName'
          },
          editable: false          
        },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' },
          editor: {
            xtype: 'connector',
            allowBlank: false
          },
          editable: false
        },
        { dataIndex: 'isPreferred', type: 'boolean', header: 'is Preferred', editable: true },
        { dataIndex: 'isContentVisible', type: 'boolean', header: 'is Content Visible', editable: true },
        { dataIndex: 'VendorIdentifier', type: 'string', header: 'Vendor Identifier', editable: false },
        { dataIndex: 'CentralDelivery', type: 'boolean', header: 'Central Delivery', editable: false }
      ]
    });

    return grid;
  }
});