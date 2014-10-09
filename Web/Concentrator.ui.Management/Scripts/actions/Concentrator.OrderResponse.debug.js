Concentrator.OrderResponses = Ext.extend(Concentrator.BaseAction, {
  hasParams: false,

  resetParams: function () {

  },
  setParams: function (action, params) {
    action.hasParams = true;

    Ext.apply(action.grid.store.baseParams, params);

    action.grid.store.load();
  },

  getPanel: function () {
    this.grid = new Diract.ui.Grid({
      pluralObjectName: 'Rules',
      singularObjectName: 'Rule',
      primaryKey: 'OrderResponseID',
      url: Concentrator.route("GetList", "OrderResponse"),
      forceFit: false,
      permissions: {
        list: 'ViewOrders',
        create: 'ViewOrders',
        remove: 'ViewOrders',
        update: 'ViewOrders'
      },
      rowActions: [
        {
          text: 'Order',
          iconCls: 'merge',
          roles: ['ViewOrders'],
          handler: (function (record) {
            this.viewOrder(record.get('OrderID'));
          }).createDelegate(this)
        }
      ],
      listeners: {
        'rowdblclick': function (grid, rowIndex) {

          var grid = new Diract.ui.Grid({
            pluralObjectName: 'Response lines',
            singularObjectName: 'Response line',
            forceFit: false,
            primaryKey: 'OrderResponseLineID',
            params: {
              OrderResponseID: grid.getStore().getAt(rowIndex).get('OrderResponseID')
            },
            permissions: {
              list: 'ViewOrders',
              create: 'ViewOrders',
              remove: 'ViewOrders',
              update: 'ViewOrders'
            },
            url: Concentrator.route("GetLines", "OrderResponse"),
            structure: [
              { dataIndex: 'OrderID', type: 'int' },
              { dataIndex: 'OrderResponseLineID', type: 'int' },
              { dataIndex: 'OrderResponseID', type: 'int', header: 'Order Response ID' },
              { dataIndex: 'OrderLineID', type: 'int', header: 'Order Line ID' },
              { dataIndex: 'Ordered', type: 'int', header: 'Ordered' },
              { dataIndex: 'Backordered', type: 'int', header: 'Backordered' },
              { dataIndex: 'Cancelled', type: 'int', header: 'Cancelled' },
              { dataIndex: 'Shipped', type: 'int', header: 'Shipped' },
              { dataIndex: 'Invoiced', type: 'int', header: 'Invoiced' },
							{dataIndex : 'Description', type: 'string', header : 'Description'},
              { dataIndex: 'Unit', type: 'string', header: 'Unit' },
              { dataIndex: 'Price', type: 'float', header: 'Price' },
              { dataIndex: 'DeliveryDate', type: 'date', header: 'DeliveryDate' },
              { dataIndex: 'VendorLineNumber', type: 'string', header: 'Vendor Line Number' },
              { dataIndex: 'VendorItemNumber', type: 'string', header: 'VendorItemNumber' },
              { dataIndex: 'OEMNumber', type: 'string', header: 'OEMNumber' },
              { dataIndex: 'Barcode', type: 'string', header: 'Barcode' },
              { dataIndex: 'Remark', type: 'string', header: 'Remark' }
            ]
          });

          var window = new Ext.Window({
            title: 'Response Lines',
            width: 800,
            height: 300,
            modal: true,
            layout: 'fit',
            autoScroll: true,
            items: [
              grid
            ]
          });

          window.show();
        }
      },
      structure: [
        { dataIndex: 'OrderResponseID', type: 'int' },
        { dataIndex: 'OrderID', type: 'int', header: 'OrderID' },
        { dataIndex: 'WebSiteOrderNumber', type: 'string', header: 'Order number' },
        { dataIndex: 'ResponseType', type: 'string', header: 'Response Type', width: 110 },
        { dataIndex: 'VendorDocument', type: 'string', header: 'Vendor Document', width: 110 },
        { dataIndex: 'AdministrationCost', type: 'float', header: 'Administration Cost', width: 110 },
        { dataIndex: 'DropShipmentCost', type: 'float', header: ' Drop Shipment Cost', width: 110 },
        { dataIndex: 'ShipmentCost', type: 'float', header: 'Shipment Cost', width: 110 },
        { dataIndex: 'OrderDate', type: 'date', header: 'Order Date', width: 110 },
        { dataIndex: 'PartialDelivery', type: 'boolean', header: 'Partial Delivery', width: 110 },
        { dataIndex: 'VendorDocumentNumber', type: 'string', header: 'Vendor Document Number', width: 110 },
        { dataIndex: 'VendorDocumentDate', type: 'date', header: 'Vendor Document Date', width: 110 },
        { dataIndex: 'VatPercentage', type: 'float', header: 'Vat Percentrage' },
        { dataIndex: 'VatAmount', type: 'float', header: 'Vat Amount', width: 110 },
        { dataIndex: 'TotalGoods', type: 'float', header: 'Total Goods', width: 110 },
        { dataIndex: 'TotalExVat', type: 'float', header: 'Total Ex Vat', width: 110 },
        { dataIndex: 'TotalAmount', type: 'float', header: 'Total Amount', width: 110 },
        { dataIndex: 'PaymentConditionDays', type: 'int', header: 'Payment Condition Days', width: 110 },
        { dataIndex: 'PaymentConditionCode', type: 'string', header: 'Payment Condition Code', width: 110 },
        { dataIndex: 'PaymentConditionDiscount', type: 'string', header: 'Payment Condition Discount', width: 110 },
        { dataIndex: 'PaymentConditionDiscountDescription', type: 'string', header: 'Payment Condition Discount Description', width: 110 },
        { dataIndex: 'TrackAndTrace', type: 'string', header: 'Track And Trace', width: 110 }
      ]
    });

    return this.grid;
  },
  viewOrder: function (orderID) {
    var grid = new Diract.ui.Grid({
      singularObjectName: 'Pending Order',
      pluralObjectName: 'Pending Orders',
      primaryKey: 'OrderID',
      sortBy: 'OrderID',
      url: Concentrator.route('GetList', 'Order'),
      permissions: {
        list: 'ViewOrders',
        create: 'ViewOrders',
        remove: 'ViewOrders',
        update: 'ViewOrders'
      },
      structure: [
        { dataIndex: 'OrderID', type: 'int', header: 'Concentrator Order ID', renderer: function (value, metadata, record) { return "#" + value } },

        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' },
          editable: false
        },

       { dataIndex: 'ReceivedDate', type: 'date', header: 'Received Date', renderer: Ext.util.Format.dateRenderer('d-m-Y') },
       { dataIndex: 'isDispatched', type: 'boolean', header: 'Dispatched' },
       { dataIndex: 'isDropShipment', type: 'boolean', header: 'Dropshipment' },
       { dataIndex: 'BackOrdersAllowed', type: 'boolean', header: 'Allow Backorders' },
       { dataIndex: 'BSKIdentifier', type: 'string', header: 'BSK ID' },
       { dataIndex: 'CustomerID', type: 'string' },
       { dataIndex: 'CustomerOrderReference', type: 'string', header: 'Customer Order Reference' },
       { dataIndex: 'EdiVersion', type: 'string', header: 'EDI Version' },
       { dataIndex: 'PaymentInstrument', type: 'string', header: 'Payment Instrument' },
       { dataIndex: 'PaymentTermsCode', type: 'string', header: 'Payment Terms Code' },
       { dataIndex: 'RouteCode', type: 'string', header: 'Route Code' },
       { dataIndex: 'WebSiteOrderNumber', type: 'string', header: 'Web Site Order Number' }

       ],
      params: {
        OrderID: orderID
      }
    });
    var window = new Ext.Window({
      title: 'Order',
      width: 900,
      height: 400,
      modal: true,
      layout: 'fit',
      items: [
                  grid
                ]
    });

    window.show();
  }
});