Concentrator.EDIOrderResponses = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'EDI Order Response',
      pluralObjectName: 'EDI Order Responses',
      permissions: {
        list: 'GetEdiOrderResponse',
        create: 'CreateEdiOrderResponse',
        update: 'UpdateEdiOrderResponse',
        remove: 'DeleteEdiOrderResponse'
      },
      url: Concentrator.route('GetList', 'EDIOrderResponse'),
      primaryKey: 'EdiOrderResponseID',
      sortBy: 'EdiOrderResponseID',
      structure: [
        { dataIndex: 'EdiOrderResponseID', type: 'int', header: 'Edi Order Response ID' },
        { dataIndex: 'ResponseType', type: 'string', header: 'Response Type' },
        { dataIndex: 'VendorDocument', type: '', header: 'Vendor Document' },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor ID' },
        { dataIndex: 'AdministrationCost', type: 'float', header: 'Administration Cost' },
        { dataIndex: 'DropShipmentCost', type: 'float', header: 'Drop Shipment Cost' },
        { dataIndex: 'ShipmentCost', type: 'float', header: 'Shipment Cost' },
        { dataIndex: 'OrderDate', type: 'date', header: 'OrderDate' },
        { dataIndex: 'PartialDelivery', type: 'boolean', header: 'Partial Delivery' },
        { dataIndex: 'VendorDocumentNumber', type: 'string', header: 'Vendor Document Number' },
        { dataIndex: 'VendorDocumentDate', type: 'date', header: 'Vendor Document Date' },
        { dataIndex: 'VatPercentage', type: 'float', header: 'Vat Percentage' },
        { dataIndex: 'VatAmount', type: 'float', header: 'Vat Amount' },
        { dataIndex: 'TotalGoods', type: 'float', header: 'Total Goods' },
        { dataIndex: 'TotalExVat', type: 'float', header: 'Total Ex Vat' },
        { dataIndex: 'TotalAmount', type: 'float', header: 'Total Amount' },
        { dataIndex: 'PaymentConditionDays', type: 'int', header: 'Payment Condition Days' },
        { dataIndex: 'PaymentConditionCode', type: 'string', header: 'Payment Condition Code' },
        { dataIndex: 'PaymentConditionDiscount', type: 'string', header: 'Payment Condition Discount' },
        { dataIndex: 'PaymentConditionDiscountDescription', type: 'string', header: 'Payment Condition Discount Description' },
        { dataIndex: 'TrackAndTrace', type: 'string', header: 'Track And Trace' },
        { dataIndex: 'InvoiceDocumentNumber', type: 'string', header: 'Invoice Document Number' },
        { dataIndex: 'ShippingNumber', type: 'string', header: 'Shipping Number' },
        { dataIndex: 'ReqDeliveryDate', type: 'date', header: 'Req Delivery Date' },
        { dataIndex: 'InvoiceDate', type: 'date', header: 'Invoice Date' },
        { dataIndex: 'Currency', type: 'string', header: 'Currency' },
        { dataIndex: 'DespAdvice', type: 'string', header: 'Desp Advice' },
        { dataIndex: 'ShipToCustomerID', type: 'int', header: 'Ship To Customer ID' },
        { dataIndex: 'SoldToCustomerID', type: 'int', header: 'Sold To Customer ID' },
        { dataIndex: 'ReceiveDate', type: 'date', header: 'Receive Date' },
        { dataIndex: 'TrackAndTraceLink', type: 'string', header: 'TrackAndTraceLink' },
        { dataIndex: 'VendorDocumentReference', type: 'string', header: 'Vendor Document Reference' },
        { dataIndex: 'OrderID', type: 'int', header: 'OrderID' }
      ],
      rowActions: [
        {
          text: 'Order',
          iconCls: 'merge',
          handler: function (record) {

            var grid = new Diract.ui.Grid({
              pluralObjectName: 'Order Reponse Lines',
              singularObjectName: 'Order Response Line',
              url: Concentrator.route("GetList", "EdiOrderResponseLine"),
              params: {
                ediOrderResponseID: record.get('EdiOrderResponseID')
              },
              permissions: {
                list: 'GetEdiOrderResponseLine',
                create: 'CreateEdiOrderResponseLine',
                update: 'UpdateEdiOrderResponseLine',
                remove: 'DeleteEdiOrderResponseLine'
              },
              primaryKey: 'EdiOrderResponseLineID',
              structure: [
                { dataIndex: 'EdiOrderResponseLineID', type: 'int' },
                { dataIndex: 'EdiOrderResponseID', type: 'int', header: 'Order Response ID' },
                { dataIndex: 'EdiOrderLineID', type: 'int', header: 'Order Line ID' },
                { dataIndex: 'Ordered', type: 'int', header: 'Ordered' },
                { dataIndex: 'Backordered', type: 'int', header: 'Backordered' },
                { dataIndex: 'Cancelled', type: 'int', header: 'Cancelled' },
                { dataIndex: 'Shipped', type: 'int', header: 'Shipped' },
                { dataIndex: 'Invoiced', type: 'int', header: 'Invoiced' },
                { dataIndex: 'Unit', type: 'string', header: 'Unit' },
                { dataIndex: 'Price', type: 'float', header: 'Price' },
                { dataIndex: 'DeliveryDate', type: 'date', header: 'Delivery Date' },
                { dataIndex: 'VendorLineNumber', type: 'string', header: 'Vendor Line Number' },
                { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor Item Number' },
                { dataIndex: 'OEMNumber', type: 'string', header: 'OEMNumber' },
                { dataIndex: 'Barcode', type: 'string', header: 'Barcode' },
                { dataIndex: 'Remark', type: 'string', header: 'Remark' },
                { dataIndex: 'Description', type: 'string', header: 'Description' },
                { dataIndex: 'Processed', type: 'boolean', header: 'Processed' },
                { dataIndex: 'RequestDate', type: 'date', header: 'Request Date' },
                { dataIndex: 'VatAmount', type: 'float', header: 'VatAmount' },
                { dataIndex: 'vatPercentage', type: 'float', header: 'Vat Percentage' },
                { dataIndex: 'CarrierCode', type: 'string', header: 'Carrier Code' },
                { dataIndex: 'NumberOfPallets', type: 'int', header: 'Number Of Pallets' },
                { dataIndex: 'NumberOfUnits', type: 'int', header: 'Number Of Units' },
                { dataIndex: 'TrackAndTrace', type: 'string', header: 'Track And Trace' },
                { dataIndex: 'SerialNumbers', type: 'string', header: 'Serial Numbers' },
                { dataIndex: 'Delivered', type: 'int', header: 'Delivered' },
                { dataIndex: 'TrackAndTraceLink', type: 'string', header: 'Track And Trace Link' },
                { dataIndex: 'ProductName', type: 'string', header: 'Product Name' },
                { dataIndex: 'Html', type: 'string', header: 'html' }
              ]
            });

            var window = new Ext.Window({
              width: 1000,
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

    return grid;
  }
});