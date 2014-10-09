Concentrator.ProductGroupConnectorVendors = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Diract.ui.Grid({
      singularObjectName: 'Preferred product group vendor',
      singularObjectName: 'Preferred product group vendors',
      primaryKey: ['ProductGroupID', 'ConnectorID', 'VendorID'],
      sortBy: 'ConnectorID',
      groupField: 'ConnectorID',
      permissions: {
        list: 'GetProductGroupConnectorVendor',
        create: 'CreateProductGroupConnectorVendor',
        remove: 'DeleteProductGroupConnectorVendor',
        update: 'UpdateProductGroupConnectorVendor'
      },
      url: Concentrator.route('GetList', 'ProductGroupConnectorVendor'),
      //url: Diract.user.hasFunctionality("Default") ? Concentrator.route('GetList', 'ProductGroupConnectorVendor') : null,
      newUrl: Concentrator.route('Create', 'ProductGroupConnectorVendor'),
      //newUrl: Diract.user.hasFunctionality("CreateProductGroupConnectorVendor") ? Concentrator.route('Create', 'ProductGroupConnectorVendor') : null,
      updateUrl: Concentrator.route('Update', 'ProductGroupConnectorVendor'),
      //updateUrl: Diract.user.hasFunctionality("UpdateProductGroupConnectorVendor") ? Concentrator.route('Update', 'ProductGroupConnectorVendor') : null,
      deleteUrl: Concentrator.route('Delete', 'ProductGroupConnectorVendor'),
      //deleteUrl: Diract.user.hasFunctionality("DeleteProductGroupConnectorVendor") ? Concentrator.route('Delete', 'ProductGroupConnectorVendor') : null,
      structure: [
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' },
          editable: false,
          editor:
          {
            xtype: 'connector'
          }
        },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor',
          renderer: Concentrator.renderers.field('vendors', 'VendorName'),
          editable: false,
          editor: { xtype: 'vendor', allowBlank: false },
          filter: {
            type: 'string',
            store: Concentrator.stores.vendors,
            filterField: 'VendorName'
          }
        },
        { dataIndex: 'ProductGroupID', type: 'int', header: "Product Group",
          renderer: function (val, m, rec) { return rec.get('ProductGroupName') },
          filter: {
            type: 'string',
            filterField: 'ProductGroup'
          },
          editor: {
            xtype: 'productgroup'

          }
        },
        { dataIndex: 'ProductGroupName', type: 'string' },
        { dataIndex: 'isPreferredAssortmentVendor', type: 'boolean', header: "Preferred Assortment Vendor", editable: true },
        { dataIndex: 'isPreferredContentVendor', type: 'boolean', header: "Preferred Content Vendor", editable: true }
      ]
    });

    return grid;
  }
});