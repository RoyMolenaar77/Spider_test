Concentrator.RefundQueue = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {
    var grid = new Diract.ui.Grid({
      primaryKey: ['OrderID', 'OrderResponseID'],
      singularObjectName: 'Refund queue item',
      pluralObjectName: 'Refund queue items',
      sortBy: 'CreationTime',
      permissions: { list: 'ViewOrders', },
      url: Concentrator.route('List', 'Refund'),
      structure: [
        { dataIndex: 'OrderID', type: 'int' },
        { dataIndex: 'WebsiteOrderNumber', type: 'string', header: 'Website order number' },
        {
          dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'OrderResponseID', type: 'int' },
        { dataIndex: 'ResponseType', type: 'string', header: 'Refund type' },
        { dataIndex: 'CreationTime', type: 'date', header: 'Creation time', renderer: Ext.util.Format.dateRenderer('d-m-Y H:i:s') },
        { dataIndex: 'Amount', type: 'float', header: 'Amount' }
      ]
    });
    return grid;
  }
});