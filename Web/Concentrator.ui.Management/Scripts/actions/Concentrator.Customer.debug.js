Concentrator.Customers = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Customers',
      singularObjectName: 'Customer',
      primaryKey: 'CustomerID',
      url: Concentrator.route("GetList", "Customer"),
      newUrl: Concentrator.route("Create", "Customer"),
      updateUrl: Concentrator.route("Update", "Customer"),
      deleteUrl: Concentrator.route("Delete", "Customer"),
      permissions: {
        list: 'ViewCustomers',
        create: 'ViewCustomers',
        remove: 'ViewCustomers',
        update: 'ViewCustomers'
      },
      structure: [
        { dataIndex: 'CustomerID', type: 'int' },
        { dataIndex: 'CustomerTelephone', type: 'string', header: 'Customer Telephone', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'CustomerEmail', type: 'string', header: 'Customer Email', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'City', type: 'string', header: 'City', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'Country', type: 'string', header: 'Country', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'PostCode', type: 'string', header: 'Post Code', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'CustomerAddressLine1', type: 'string', header: 'Customer AddressLine 1', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'CustomerAddressLine2', type: 'string', header: 'Customer AddressLine 2', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'CustomerAddressLine3', type: 'string', header: 'Customer Address Line 3', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'EANIdentifier', type: 'string', header: 'EAN Identifier', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'CustomerName', type: 'string', header: 'Customer Name', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'HouseNumber', type: 'string', header: 'House Number', editor: { xtype: 'textfield', allowBlank: true} }
      ],
      windowConfig: {
        height: 420
      }
    });

    return grid;
  }

});