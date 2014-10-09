Concentrator.VendorStocks = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {
    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Vendor stock',
      pluralObjectName: 'Vendor stocks',
      permissions: {
        list: 'GetVendorStock'
      },
      url: Concentrator.route('GetList', 'VendorStock'),
      primaryKey: ['ProductID', 'VendorID'],
      sortBy: 'ProductID',
      structure: [
        { dataIndex: 'ProductID', type: 'int' },
        { dataIndex: 'ProductName', type: 'string', header: 'Product' },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor',
          renderer: Concentrator.renderers.field('vendors', 'VendorName'),
          filter: { type: 'list', store: Concentrator.stores.vendors, labelField: 'VendorName' }
        },
        { dataIndex: 'QuantityOnHand', type: 'string', header: 'Quantity on hand' },
        { dataIndex: 'PromisedDeliveryDate', type: 'date', header: 'Delivery Date' },
        { dataIndex: 'QuantityToReceive', type: 'int', header: 'Quantity To Receive' },
        { dataIndex: 'VendorStatus', type: 'string', header: 'Vendor Status' },
        { dataIndex: 'ConcentratorStatus', type: 'string', header: 'Concentrator Status' },
        { dataIndex: 'UnitCost', type: 'float', header: 'Unit Cost' }
      ]

    });
    return grid;

  }


});