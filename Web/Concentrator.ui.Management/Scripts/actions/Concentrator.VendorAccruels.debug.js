Concentrator.VendorAccruels = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Vendor Accruel',
      pluralObjectName: 'Vendor Accruels',
      permissions: {
        list: 'GetVendorAccruel',
        create: 'CreateVendorAccruel',
        update: 'UpdateVendorAccruel',
        remove: 'DeleteVendorAccruel'
      },
      url: Concentrator.route('GetList', 'VendorAccruel'),
      newUrl: Concentrator.route("Create", "VendorAccruel"),
      deleteUrl: Concentrator.route("Delete", "VendorAccruel"),
      updateUrl: Concentrator.route('Update', 'VendorAccruel'),
      primaryKey: 'VendorAssortmentID',
      sortBy: 'VendorAssortmentID',
      structure: [
        { dataIndex: 'VendorAssortmentID', type: 'int', editor: { xtype: 'assortmentBox' } },
        { dataIndex: 'AccruelCode', type: 'string', header: 'Accruel Code', editor: { xtype: 'textfield', allowBlank: false }, editable: true },
        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textfield', allowBlank: false }, editable: true },
        { dataIndex: 'UnitPrice', type: 'float', header: 'Unit Price', editor: { xtype: 'numberfield', allowBlank: false }, editable: true },
        { dataIndex: 'MinimumQuantity', type: 'int', header: 'Minimum Quantity', editor: { xtype: 'int', allowBlank: false }, editable: true }
      ],
      windowConfig: {
        width: 450
      }
    });

    return grid;
  }
});