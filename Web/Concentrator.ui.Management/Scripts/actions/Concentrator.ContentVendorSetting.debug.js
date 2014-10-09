Concentrator.ContentVendorSetting = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Content Vendor Settings',
      singularObjectName: 'Content Vendor Setting',
      primaryKey: 'ContentVendorSettingID',
      url: Concentrator.route("GetList", "ContentVendorSetting"),
      updateUrl: Concentrator.route("Update", "ContentVendorSetting"),
      newUrl: Concentrator.route("Create", "ContentVendorSetting"),
      deleteUrl: Concentrator.route("Delete", "ContentVendorSetting"),
      permissions: {
        list: 'GetProductGroupConnectorVendor',
        create: 'CreateProductGroupConnectorVendor',
        remove: 'DeleteProductGroupConnectorVendor',
        update: 'UpdateProductGroupConnectorVendor'
      },
      structure: [
        { dataIndex: 'ContentVendorSettingID', type: 'int' },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          editor: {
            xtype: 'connector',
            allowBlank: false
          },
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor',
          editor: {
            xtype: 'vendor',
            allowBlank: false
          },
          renderer: Concentrator.renderers.field('vendors', 'VendorName'),
          filter: { type: 'list', store: Concentrator.stores.vendors, labelField: 'VendorName' }
        },
        { dataIndex: 'ProductGroupID', type: 'int', header: 'Product group',
          editor: {
            xtype: 'productgroup',
            allowBlank: true
          },
          renderer: function (val, m, rec) {
            return rec.get('ProductGroupName')
          },
          filter: {
            type: 'string',
            store: Concentrator.stores.productGroups,
            filterField: 'ProductGroupName'
          }
        },
        { dataIndex: 'ProductGroupName', type: 'string' },
        { dataIndex: 'ProductID', type: 'int', header: 'Product',
          renderer: function (val, m, record) {
            return record.get('ProductDescription');
          }
        },
        { dataIndex: 'ProductDescription', type: 'string' },
        { dataIndex: 'BrandID', type: 'int', header: 'Brand ID',
          renderer: function (val, m, record) {
            return record.get('Brandname');
          }
        },
        { dataIndex: 'Brandname', type: 'string' },
        { dataIndex: 'CreationTime', type: 'date', header: 'Creation Time' },
        { dataIndex: 'LastModificationTime', type: 'date', header: 'Last Modification Time' },
        { dataIndex: 'ContentVendorIndex', type: 'int', header: 'Content Vendor Index', editor: { xtype: 'int', allowBlank: false} }
      ]
    });

    return grid;
  }


});