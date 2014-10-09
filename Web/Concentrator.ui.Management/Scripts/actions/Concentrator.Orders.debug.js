Concentrator.Orders = Ext.extend(Concentrator.BaseAction, {


	getPanel: function (id) {
		var self = this;
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
        { dataIndex: 'ReceivedDate', type: 'date', header: 'Received Date', renderer: Ext.util.Format.dateRenderer('d-m-Y H:i:s') },
        { dataIndex: 'IsDispatched', type: 'boolean', header: 'Dispatched' },
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
        	iconCls: 'box-view',
        	roles: ['ViewOrders'],
        	handler: function (record) {
        		self.getOrderDetails(record);
        	}
        },
        {
        	text: 'Order Response',
        	iconCls: 'merge',
        	roles: ['ViewOrders'],
        	handler: function (record) {
        		self.getOrderResponse(record.get('OrderID'));
        	}
        },
        {
        	text: 'Order Outbound',
        	iconCls: 'default',
        	roles: ['ViewOrders'],
        	handler: function (record) {
        		self.getOrderOutbound(record.get('OrderID'));
        	}
        }
       ],
			params: {
				orderID: id
			},
			customButtons: [
        new Ext.Toolbar.Button({
        	text: 'Dispatch pending order lines',
        	iconCls: 'box-out',
        	roles: ['Dispatch'],
        	roles: ['Default'],
        	handler: function () {
        		self.dispatchJob()
        	}
        })
      ]
		}
    );
		return grid;
	},

	dispatchJob: function () {
		Diract.request({
			url: Concentrator.route('StartJob', 'MiddleWare'),
			params: {
				jobName: 'Concentrator Order Process',
				groupName: 'Order Processing'
			}
		});
	},

	getOrderDetails: function (record) {
		var self = this;

		Diract.silent_request({
			url: Concentrator.route('GetByOrder', 'Customer'),
			permissions: {
				list: 'ViewOrders',
				create: 'ViewOrders',
				remove: 'ViewOrders',
				update: 'ViewOrders'
			},
			params: {
				orderID: record.get('OrderID')
			},
			method: 'GET',
			success: function (data) {

				var olGrid = new Diract.ui.Grid({
					singularObjectName: 'Order Line',
					pluralObjectName: 'Order Lines',
					primarykey: 'OrderLineID',
					url: Concentrator.route('GetList', 'OrderLine'),
					permissions: {
						list: 'GetOrderLine'
					},
					sortBy: 'OrderLineID',
					params: {
						orderID: record.get('OrderID')
					},
					structure: [
         { dataIndex: 'OrderID', type: 'int' },
        { dataIndex: 'OrderLineID', type: 'int', header: 'Order Line ID' },
        { dataIndex: 'ProductName', header: 'Product', type: 'string' },
        { dataIndex: 'ProductID', type: 'int' },
        { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor item number' },
        { dataIndex: 'isDispatched', type: 'boolean', header: 'Dispatched' },
        { dataIndex: 'CustomerOrderNr', type: 'string', header: 'Customer Order Number' },
        { dataIndex: 'CustomerOrderLineNr', type: 'string', header: 'Customer Orderline Number' },
        { dataIndex: 'CustomerItemNumber', type: 'string', header: 'Customer Item Number' },
        { dataIndex: 'Quantity', header: 'Quantity', type: 'int' },
        { dataIndex: 'DispatchedToVendorID', header: 'Dispatched To', type: 'int',
        	renderer: Concentrator.renderers.field('vendors', 'VendorName'),
        	filter: { type: 'string', filterField: 'VendorName', store: Concentrator.stores.vendors }
        },
        { dataIndex: 'VendorOrderNumber', type: 'int', header: 'Vendor Order ID' },
        { dataIndex: 'CurrentState', type: 'string', header: 'Current State', renderer: Concentrator.renderers.orderLineStatus }
      ],
					rowActions: [
        {
        	text: 'Dispatch order line',
        	iconCls: 'box-out',
        	predicate: function (record) { return (record.get('isDispatched') == false); },
        	roles: ['ViewOrders'],
        	handler: function (record) {
        		self.dispatchOrderLine(record);
        	}
        },
        {
        	text: 'Vendors',
        	iconCls: 'default',
        	predicate: function (record) { return (record.get('isDispatched') == false); },
        	handler: function (record) {
        		self.vendorList(record);
        	}
        },
          {
          	text: 'Status',
          	iconCls: 'merge',
          	roles: ['ViewOrders'],
          	handler: function (record) {

          		var grid = new Diract.ui.Grid({
          			pluralObjectName: 'Order Ledgers',
          			singularObjectName: 'Order Ledger',
          			primaryKey: 'OrderLedgerID',
          			url: Concentrator.route("GetList", "OrderLedger"),
          			permissions: {
          				list: 'GetOrderLedger'
          			},
          			params: {
          				orderLineID: record.get('OrderLineID')
          			},
          			structure: [
                  { dataIndex: 'OrderLedgerID', type: 'int' },
                  { dataIndex: 'OrderLineID', type: 'int', header: 'Order Line' },
                  { dataIndex: 'Status', type: 'int', header: 'Status', renderer: Concentrator.renderers.orderLineStatus },
                  { dataIndex: 'LedgerDate', type: 'date', header: 'Ledger Date' }
                ]

          		});

          		var window = new Ext.Window({
          			title: 'Order ledges',
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

				var custTpl =
  "<div><h3 style='margin-bottom:5px;'>Contact Details:</h3>" +
  "<p><span class='caption'>Name: </span>{CustomerName}</p>" +
    "<p><span class='caption'>Email: </span>{CustomerEmail}</p>" +
  "<p><span class='caption'>Telephone: </span>{CustomerTelephone}</p>" +
  "</div>" +

  "<div style='margin-top: 5px'><h3 style='margin-bottom:5px;'>Contact Address:</h3>" +
  "<p><span class='caption'>Address Line 1: </span>{CustomerAddressLine1}</p>" +
  "<p><span class='caption'>Address Line 2: </span>{CustomerAddressLine2}</p>" +
  "<p><span class='caption'>Address Line 3: </span>{CustomerAddressLine3}</p>" +
  "<p><span class='caption'>PostCode: </span>{PostCode}</p>" +
  "<p><span class='caption'>City: </span>{City}</p>" +
  "<p><span class='caption'>Country: </span>{Country}</p>" +
  "<p><span class='caption'>EAN : </span>{EANIdentifier}</p>" +
  "</div>";

				var custShipTpl =
  "<div><h3 style='margin-bottom:5px; margin-top: 5px;'>Contact Ship Details:</h3>" +
  "<p><span class='caption'>Name: </span>{CustomerName}</p>" +
    "<p><span class='caption'>Email: </span>{CustomerEmail}</p>" +
  "<p><span class='caption'>Telephone: </span>{CustomerTelephone}</p>" +
  "</div>" +

  "<div style='margin-top: 5px'><h3 style='margin-bottom:5px;'>Contact Address:</h3>" +
  "<p><span class='caption'>Address Line 1: </span>{ShipCustomerAddressLine1}</p>" +
  "<p><span class='caption'>Address Line 2: </span>{ShipCustomerAddressLine2}</p>" +
  "<p><span class='caption'>Address Line 3: </span>{ShipCustomerAddressLine3}</p>" +
  "<p><span class='caption'>PostCode: </span>{ShipPostCode}</p>" +
  "<p><span class='caption'>City: </span>{ShipCity}</p>" +
  "<p><span class='caption'>Country: </span>{ShipCountry}</p>" +
  "<p><span class='caption'>EAN : </span>{ShipEANIdentifier}</p>" +
  "</div>";

				var tpl = new Ext.Template(custTpl, custShipTpl);

				// tpl.compile();


				var customerPanel = new Ext.Panel({
					tpl: tpl,
					data: data.customer,
					padding: 10
				});
				var tabPanel = new Ext.TabPanel({
					id: 'order-tab',
					enableTabScroll: true,
					activeTab: 0,
					border: true,
					deferredRender: false,
					layoutOnTabChange: true,
					bodyStyle: 'padding:25px',
					autoDestroy: true,
					plugins: [new Ext.ux.TabCloseMenu()],

					items: [
      {
      	id: 'tab-order-lines',
      	title: 'Order Lines',
      	iconCls: 'package-view',
      	items: olGrid,
      	closable: false,
      	layout: 'fit'
      },
      {
      	id: 'customer-tab',
      	iconCls: 'customer',
      	title: 'Customer',
      	items: customerPanel
      }
      ]
				});

				var oLWindow = new Ext.Window({
					modal: true,
					items: tabPanel,
					layout: 'fit',
					width: 1100,
					height: 500
				});
				oLWindow.show();

			}
		});
	},

	dispatchOrderLine: function (record) {
		var isDispatched = eval(record.get('isDispatched'));
		if (!isDispatched) {
			Diract.request({
				url: Concentrator.route('Dispatch', 'OrderLine'),
				params: {
					orderLineID: record.get('OrderLineID')
				}
			})
		}
	},

	vendorList: function (record) {
		var r = record;
		var grid = new Diract.ui.Grid({
			pluralObjectName: 'Vendors',
			singularObjectName: 'Vendor',
			primaryKey: 'VendorID',
			url: Concentrator.route("GetVendorList", "OrderLine"),
			permissions: {
				all: 'Default'
			},
			structure: [
        { dataIndex: 'VendorID', type: 'int' },
        { dataIndex: 'VendorType', type: 'int', header: 'Vendor Type' },
        { dataIndex: 'Name', type: 'string', header: 'Name' },
        { dataIndex: 'Description', type: 'string', header: 'Description' },
        { dataIndex: 'BackendVendorCode', type: 'string', header: 'Backend Vendor Code' },
        { dataIndex: 'ParentVendorID', type: 'int', header: 'Parent Vendor ID' },
        { dataIndex: 'OrderDispatcherType', type: 'string', header: 'Order Dispatch Type' },
        { dataIndex: 'CDPrice', type: 'float', header: 'CD Price' },
        { dataIndex: 'DSPrice', type: 'float', header: 'DS Price' },
        { dataIndex: 'PurchaseOrderType', type: 'string', header: 'Purchase Order Type' },
        { dataIndex: 'IsActive', type: 'boolean', header: 'Is Active' },
        { dataIndex: 'CutOffTime', type: 'date', header: 'Cut Off Time' },
        { dataIndex: 'DeliveryHours', type: 'int', header: 'Delivery Hours' }
      ],
			rowActions: [
        {
        	text: 'Dispatch',
        	iconCls: 'merge',
        	alwaysEnabled: true,
        	handler: function (record) {

        		var vendorID = record.get('VendorID');

        		Diract.request({
        			url: Concentrator.route('DispatchEverything', 'OrderLine'),
        			params: {
        				orderID: r.get('OrderID'),
        				vendorID: vendorID
        			}
        		});

        	}
        }
      ]
		});

		var window = new Ext.Window({
			title: 'Vendors',
			modal: true,
			layout: 'fit',
			width: 900,
			height: 350,
			items: [
        grid
      ]
		});

		window.show();
	},

	getCustomer: function (orderID, callback) {
		var c = callback;
	},

	getOrderResponse: function (orderID) {
		var grid = new Diract.ui.Grid({
			pluralObjectName: 'Rules',
			singularObjectName: 'Rule',
			primaryKey: 'OrderResponseID',
			url: Concentrator.route("GetList", "OrderResponse"),
			forceFit: false,
			permissions: {
				list: 'GetOrderResponse'
			},
			params: { OrderID: orderID },

			listeners: {
				'rowdblclick': function (g) {
					
					var grid = new Diract.ui.Grid({
						pluralObjectName: 'Response lines',
						singularObjectName: 'Response line',
						forceFit: false,
						primaryKey: 'OrderResponseLineID',
						permissions: {
							list: 'GetOrderLine'
						},
						params : {
							OrderResponseID : g.getSelectionModel().getSelected().get('OrderResponseID')
						},
						url: Concentrator.route("GetLines", "OrderResponse"),
						structure: [
              { dataIndex: 'OrderResponseLineID', type: 'int' },
              { dataIndex: 'OrderResponseID', type: 'int', header: 'Order Response ID' },
              { dataIndex: 'OrderLineID', type: 'int', header: 'Order Line ID' },
              { dataIndex: 'Ordered', type: 'int', header: 'Ordered' },
						//{ dataIndex: 'Backorded', type: 'int', header: 'Backordered' },
              {dataIndex: 'Backordered', type: 'int', header: 'Back Ordered' },
              { dataIndex: 'Cancelled', type: 'int', header: 'Cancelled' },
              { dataIndex: 'Shipped', type: 'int', header: 'Shipped' },
              { dataIndex: 'Invoiced', type: 'int', header: 'Invoiced' },
							{ dataIndex: 'Description', type: 'string', header: 'Description' },
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
						width: 1000,
						height: 300,
						modal: true,
						layout: 'fit',
						//            autoScroll: true,
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
        { dataIndex: 'ResponseType', type: 'string', header: 'Response Type', width: 110 },
        { dataIndex: 'VendorDocument', type: 'string', header: 'Vendor Document', width: 110 },
        { dataIndex: 'AdministrationCost', type: 'float', header: 'Administration Cost', width: 110 },
        { dataIndex: 'DropShipmentCost', type: 'float', header: 'Drop Shipment Cost', width: 110 },
        { dataIndex: 'ShipmentCost', type: 'float', header: 'Shipment Cost', width: 110 },
        { dataIndex: 'OrderDate', type: 'date', header: 'Order Date', width: 110 },
        { dataIndex: 'PartialDelivery', type: 'boolean', header: 'Partial Delivery', width: 110 },
        { dataIndex: 'VendorDocumentNumber', type: 'string', header: 'Vendor Document Number', width: 110 },
        { dataIndex: 'VendorDocumentDate', type: 'date', header: 'Vendor Document Date', width: 110 },
			//{ dataIndex: 'VatPercentage', type: 'float', header: 'Vat Percentrage' },
        {dataIndex: 'VatAmount', type: 'float', header: 'Vat Amount', width: 110 },
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

		var window = new Ext.Window({
			modal: true,
			width: 1000,
			layout: 'fit',
			height: 500,
			items: grid,
			title: 'Order responses'

		});
		window.show();
	},
	getOrderOutbound: function (orderID) {
		var grid = new Diract.ui.Grid({
			pluralObjectName: 'Outbound orders',
			singularObjectName: 'Outbound order',
			primaryKey: 'OutboundID',
			url: Concentrator.route("GetList", "OrderOutbound"),
			updateUrl: Concentrator.route("Update", "OrderOutbound"),
			params: { OrderID: orderID },
			forceFit: false,
			permissions: {
				list: 'GetOrderOutbound',
				update: 'UpdateOrderOutbound'
			},
			rowActions: [
        {
        	text: 'View XML',
        	iconCls: 'merge', //TODO: change icon
        	handler: function (record) {

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
        { dataIndex: 'OutboundMessage', header: 'Message' },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector', filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name'} },
        { dataIndex: 'Processed', type: 'boolean', header: 'Processed', editable: true },
        { dataIndex: 'CreationTime', type: 'date', header: 'Creation Time' },
        { dataIndex: 'Type', type: 'string', header: 'Type' },
        { dataIndex: 'OutboundUrl', type: 'string', header: 'Url', editable: true },
        { dataIndex: 'ResponseRemark', type: 'string', header: 'Response Remark' },
        { dataIndex: 'ResponseTime', type: 'int', header: 'Response Time' },
        { dataIndex: 'ProcessedCount', type: 'int', header: 'Processed Count' },
        { dataIndex: 'ErrorMessage', type: 'string', header: 'Error Message' },
        { dataIndex: 'ProcessDate', type: 'date', header: 'Process Date' },
        { dataIndex: 'OrderID', type: 'int', header: 'Order ID' }
      ]
		});

		var window = new Ext.Window({
			modal: true,
			width: 1000,
			layout: 'fit',
			height: 400,
			items: grid,
			title: 'Order outbounds'
		});

		window.show();
	}
});