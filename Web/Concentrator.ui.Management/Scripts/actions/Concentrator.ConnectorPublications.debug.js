Concentrator.ConnectorPublications = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Connector Publication',
      pluralObjectName: 'Connector Publications',
      permissions: {
        list: 'GetConnectorPublication',
        create: 'CreateConnectorPublication',
        update: 'UpdateConnectorPublication',
        remove: 'DeleteConnectorPublication'
      },
      url: Concentrator.route('GetList', 'ConnectorPublication'),
      newUrl: Concentrator.route("Create", "ConnectorPublication"),
      deleteUrl: Concentrator.route("Delete", "ConnectorPublication"),
      updateUrl: Concentrator.route('Update', 'ConnectorPublication'),
      primaryKey: 'ConnectorPublicationID',
      sortBy: 'ConnectorPublicationID',
      structure: [
        { dataIndex: 'ConnectorPublicationID', type: 'int' },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' },
          editor: {
            xtype: 'connector',
            allowBlank: false
          },
          editable: true
        },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor',
          editable: false,
          editor: { xtype: 'vendor', allowBlank: false },
          renderer: Concentrator.renderers.field('vendors', 'VendorName'), filter: {
            type: 'list',
            store: Concentrator.stores.vendors,
            labelField: 'VendorName'
          },
          editable: true
        },
        { dataIndex: 'ProductGroupID', type: 'int', header: "Product Group",
          renderer: function (val, m, rec) { return rec.get('ProductGroupName') },
          filter: {
            type: 'string',
            filterField: 'ProductGroup'
          },
          editor: {
            xtype: 'productgroup',
            allowBlank: true
          }
        },
        { dataIndex: 'ProductGroupName', type: 'string' },
        { dataIndex: 'BrandID', type: 'int', header: 'Brand',
          filter: {
            type: 'string',
            filterField: 'BrandName'
          },
          editor: { xtype: 'brand', hiddenName: 'BrandID', allowBlank: true },
          renderer: function (val, metadata, record) {
            return record.get('BrandName');
          }
        },
        { dataIndex: 'BrandName', type: 'string' },
        { dataIndex: 'AttributeID', type: 'int', header: 'Attribute',
          filter: {
            type: 'string',
            filterField: 'AttributeName'
          },
          editor: { xtype: 'attribute', hiddenName: 'AttributeID', allowBlank: true },
          renderer: function (val, metadata, record) {
            return record.get('AttributeName');
          }
        },
        { dataIndex: 'AttributeName', type: 'string' },
        { dataIndex: 'AttributeValue', type: 'string', header: 'Attribute Value', editor: { xtype:'textfield', fieldLabel: 'Attribute Value', allowBlank: true} },
        { dataIndex: 'ProductID', type: 'int', editor: { xtype: 'product', fieldLabel: 'Product', allowBlank: true} },
        { dataIndex: 'ProductDescription', type: 'string', header: 'Product' },
        { dataIndex: 'Publish', type: 'boolean', header: 'Publish', editor: { xtype: 'boolean'} },
        { dataIndex: 'PublishOnlyStock', type: 'boolean', header: 'Publish Only Stock', editor: { xtype: 'boolean'} },
        { dataIndex: 'ProductContentIndex', type: 'int', header: 'Product Content Index', editor: { xtype: 'int', allowBlank: false} },
        { dataIndex: 'StatusID', type: 'int', header: 'Concentrator Status',
          renderer: function (val, m, re) {
            return re.get('ConcentratorStatus');
          },
          editor: {
            xtype: 'concentratorstatus',
            roles: ['CreateProductStatus'],
            valueField: 'StatusID'
          },
          filter: {
            type: 'string',
            fieldLabel: 'Status',
            filterField: 'ConcentratorStatus'
          }
        },
        { dataIndex: 'ConcentratorStatus', type: 'string' }
      ],
      windowConfig: {
        height: 350,
        width: 450
      }
    });

    return grid;
  }
});