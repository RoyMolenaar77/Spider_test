Concentrator.ProductMedia = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Product Media',
      pluralObjectName: 'Product Media',
      primaryKey: 'MediaID',
      url: Concentrator.route("GetList", "ProductMedia"),
      newUrl: Concentrator.route("CreateMedia", "ProductMedia"),
      updateUrl: Concentrator.route("UpdateMedia", "ProductMedia"),
      deleteUrl: Concentrator.route("DeleteMedia", "ProductMedia"),
      permissions: {
        list: 'GetProductMedia',
        create: 'CreateProductMedia',
        remove: 'DeleteProductMedia',
        update: 'UpdateProductMedia'
      },
      structure: [
        { dataIndex: 'MediaID', type: 'int' },
        { dataIndex: 'ProductID', type: 'int',
          renderer: function (val, m, rec) { return rec.get('Description') },
          header: 'Product', editable: false,
          editor: { xtype: 'product', hiddenName: 'ProductID', allowBlank: 'false' }
        },
        { dataIndex: 'Description', type: 'string' },
        { dataIndex: 'Sequence', type: 'int', header: 'Sequence', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor', editor: { xtype: 'vendor', allowBlank: false },
          renderer: function (val, meta, record) {
            return record.get('Vendor');
          },
          filter: {
            type: 'list',
            store: Concentrator.stores.vendors,
            labelField: 'VendorName'
          }
        },
        { dataIndex: 'Vendor', type: 'string' },
        { dataIndex: 'TypeID', type: 'int', header: 'Type', editor: { xtype: 'int', allowBlank: false },
          renderer: function(val, meta, record) {
            return record.get('MediaType')
          }
        },
        { dataIndex: 'MediaType', type: 'string' },
        { dataIndex: 'MediaUrl', type: 'string', header: 'Media Url', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'MediaPath', type: 'string', header: 'Media Path', editor: { xtype: 'textfield', allowBlank: true} }
      ]
    });

    return grid;
  }
});