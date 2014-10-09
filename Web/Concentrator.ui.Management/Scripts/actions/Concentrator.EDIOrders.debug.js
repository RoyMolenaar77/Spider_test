Concentrator.EDIOrders = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {

    var self = this;

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'EDI Order',
      pluralObjectName: 'EDI Orders',
      permissions: {
        list: 'GetEdiOrder',
        create: 'CreateEdiOrder',
        update: 'UpdateEdiOrder',
        remove: 'DeleteEdiOrder'
      },
      url: Concentrator.route("GetList", "EdiOrder"),
      primaryKey: 'EdiOrderID',
      sortBy: 'EdiOrderID',
      structure: [
        { dataIndex: 'EdiOrderID', type: 'int', header: 'EDI Order ID', renderer: function (value, metadata, record) { return "#" + value } },

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
      rowActions: [
        {
          text: 'View Order Details',
          iconCls: 'merge',
          roles: ['Default'],
          handler: function (record) {
            self.getOrderDetails(record);
          }
        }
      ]
    });

    return grid;
  },

  getOrderDetails: function (record) {
    var self = this;

    var grid = new Diract.ui.Grid({
      pluralObjectName: 'EDI Order Lines',
      singularObjectName: 'EDI Order Line',
      primaryKey: 'EdiOrderLineID',
      url: Concentrator.route("GetList", "EDIOrderLine"),
      params: {
        ediOrderID: record.get('EdiOrderID')
      },
      permissions: {
        list: 'GetEdiOrderLine',
        create: 'CreateEdiOrderLine',
        update: 'UpdateEdiOrderLine',
        remove: 'DeleteEdiOrderLine'
      },
      sortBy: 'EdiOrderLineID',
      structure: [
        { dataIndex: 'EdiOrderLineID', type: 'int', header: 'Order Line ID' },
        { dataIndex: 'Remarks', type: 'string', header: 'Remark' },
        { dataIndex: 'EdiOrderID', type: 'int', header: 'EDI Order ID' },
        { dataIndex: 'CustomerEdiOrderLineNr', type: 'string', header: 'Customer Edi Order Line Nr' },
        { dataIndex: 'CustomerOrderNr', type: 'string', header: 'Customer Order Nr' },
        { dataIndex: 'ProductID', type: 'int', header: 'Product ID' },
        { dataIndex: 'Price', type: 'float', header: 'Price' },
        { dataIndex: 'Quantity', type: 'int', header: 'Quantity' },
        { dataIndex: 'isDispatched', type: 'boolean', header: 'Is Dispatched' },
        { dataIndex: 'DispatchedToVendorID', type: 'int', header: 'Dispatched To Vendor ID' },
        { dataIndex: 'VendorOrderNumber', type: 'int', header: 'Vendor Order Number' },
        { dataIndex: 'Response', type: 'string', header: 'Response' },
        { dataIndex: 'CentralDelivery', type: 'boolean', header: 'Central Delivery' },
        { dataIndex: 'CustomerItemNumber', type: 'string', header: 'Customer Item Number' },
        { dataIndex: 'WareHouseCode', type: 'string', header: 'Warehouse Code' },
        { dataIndex: 'PriceOverride', type: 'boolean', header: 'Price Override' }
      ],
      rowActions: [
        {
          text: 'Status',
          iconCls: 'default',
          handler: function (record) {
            
            var grid = new Diract.ui.Grid({
              pluralObjectName: 'Order Ledgers',
              singularObjectName: 'Order Ledger',
              primaryKey: 'EdiOrderLedgerID',
              url: Concentrator.route("GetList", "EdiOrderLedger"),
              permissions: {
                list: 'GetEdiOrderLedger',
                create: 'CreateEdiOrderLedger',
                remove: 'DeleteEdiOrderLedger',
                update: 'UpdateEdiOrderLedger'
              },
              params: {
                ediOrderLineID: record.get('EdiOrderLineID')
              },
              structure: [
                  { dataIndex: 'EdiOrderLedgerID', type: 'int' },
                  { dataIndex: 'EdiOrderLineID', type: 'int', header: 'Order Line' },
                  { dataIndex: 'Status', type: 'int', header: 'Status', renderer: Concentrator.renderers.orderLineStatus },
                  { dataIndex: 'LedgerDate', type: 'date', header: 'Ledger Date' }
                ]

            });

            var window = new Ext.Window({
              title: 'Order ledgers',
              width: 1100,
              height: 400,
              modal: true,
              layout: 'fit',
              items: [
                grid
              ]
            });

            window.show();

          }
        }
      ]
    });

    var window = new Ext.Window({
      width: 1100,
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