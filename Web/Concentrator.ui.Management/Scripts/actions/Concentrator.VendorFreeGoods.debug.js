Concentrator.VendorFreeGoods = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Vendor Free Good',
      pluralObjectName: 'Vendor Free Goods',
      permissions: {
        list: 'GetVendorFreeGood',
        create: 'CreateVendorFreeGood',
        update: 'UpdateVendorFreeGood',
        remove: 'DeleteVendorFreeGood'
      },
      url: Concentrator.route('GetList', 'VendorFreeGood'),
      newUrl: Concentrator.route("Create", "VendorFreeGood"),
      deleteUrl: Concentrator.route("Delete", "VendorFreeGood"),
      updateUrl: Concentrator.route('Update', 'VendorFreeGood'),
      primaryKey: 'VendorAssortmentID',
      sortBy: 'VendorAssortmentID',
      structure: [
        { dataIndex: 'VendorAssortmentID', type: 'int', editor: { xtype: 'assortmentBox' } },
        { dataIndex: 'ProductID', type: 'int', renderer: function (val, m, rec) { return rec.get('ProductDescription') }, header: 'Product', editable: false, filter: { type: 'string'} },
        { dataIndex: 'ProductDescription', type: 'string' },
        { dataIndex: 'VendorID', type: 'int', renderer: function (val, m, rec) { return rec.get('VendorName') }, header: 'Vendor', editable: false, filter: { type: 'string'} },
        { dataIndex: 'VendorName', type: 'string' },
        { dataIndex: 'MinimumQuantity', type: 'int', header: 'Minimum Quantity', editor: { xtype: 'int', allowBlank: false} },
        { dataIndex: 'OverOrderedQuantity', type: 'int', header: 'Over Ordered Quantity', editor: { xtype: 'int', allowBlank: false} },
        { dataIndex: 'FreeGoodQuantity', type: 'int', header: 'Free Good Quantity', editor: { xtype: 'int', allowBlank: false} },
        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'UnitPrice', type: 'float', header: 'Unit Price', editor: { xtype: 'numberfield', allowBlank: false} },
        { dataIndex: 'CustomItemNumber', type: 'string', header: 'Custom Item Number', editor: { xtype: 'textfield', allowBlank: false } }
      ],
      windowConfig: {
        height: 320,
        width: 450
      }
    });

    return grid;
  }
});