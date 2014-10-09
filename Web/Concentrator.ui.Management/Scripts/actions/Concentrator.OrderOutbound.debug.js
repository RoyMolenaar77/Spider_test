Concentrator.OrderOutbound = Ext.extend(Concentrator.BaseAction,  {

  getPanel: function() {
    var grid = new Diract.ui.Grid({
      pluralObjectName: 'Outbound orders',
      singularObjectName: 'Outbound order',
      primaryKey: 'OutboundID',
      url: Concentrator.route("GetList", "OrderOutbound"),
      updateUrl: Concentrator.route("Update", "OrderOutbound"),
      forceFit: false,
      deleteUrl: Concentrator.route("Delete", "Language"),
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
          handler: (function(record) {
            this.getOrder(record.get('OrderID'));
          }).createDelegate(this)
        },
        {
          text: 'View XML',
          iconCls: 'merge', //TODO: change icon
          handler: function(record) {

            //form textarea
            var message = new Ext.form.TextArea({
              anchor: '100%',
              value: record.get('OutboundMessage')           
            });

            var xmlForm = new Ext.Panel({
              layout: "fit",                 
              items: [
                message
              ]
            });

            var window = new Ext.Window({
              title: 'View XML',
              width: 600,
              height: 250,
              modal: true,
              layout: 'fit',
              items: [
                xmlForm
              ]
            });

            window.show();
          }
        }
      ],
      structure: [
        { dataIndex: 'OutboundID', type: 'int' },
        { dataIndex: 'OutboundMessage', type: 'string' },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector', renderer: Concentrator.renderers.field('connectors', 'Name'), filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' } },
        { dataIndex: 'Processed', type: 'boolean', header: 'Processed', editable: true },
        { dataIndex: 'CreationTime', type: 'date', header: 'Creation Time' },
        { dataIndex: 'Type', type: 'string', header: 'Type' },
        { dataIndex: 'OutboundUrl', type: 'string', header: 'Url', editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'ResponseRemark', type: 'string', header: 'Response Remark' },
        { dataIndex: 'ResponseTime', type: 'int', header: 'Response Time' },
        { dataIndex: 'ProcessedCount', type: 'int', header: 'Processed Count' },
        { dataIndex: 'ErrorMessage', type: 'string', header: 'Error Message' },
        { dataIndex: 'ProcessDate', type: 'date', header: 'Process Date' },
        { dataIndex: 'OrderID', type: 'int', header: 'Order ID' }
      ]
    });

    return grid;
  },

  getOrder: function(OrderID) {
    
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
        { dataIndex: 'OrderID', type: 'int', header: 'Concentrator Order ID', renderer: function(value, metadata, record) { return "#" + value } },
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
        OrderID: OrderID
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