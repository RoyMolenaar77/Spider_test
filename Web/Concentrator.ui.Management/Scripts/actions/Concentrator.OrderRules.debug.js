Concentrator.OrderRules = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {
    var grid = new Diract.ui.Grid({
      url: Concentrator.route('GetList', 'OrderRule'),
      updateUrl: Concentrator.route('Update', 'OrderRule'),
      newUrl: Concentrator.route('Create', 'OrderRule'),
      deleteUrl: Concentrator.route('Delete', 'OrderRule'),
      permissions: {
        list: 'GetOrderRule',
        create: 'CreateOrderRule',
        remove: 'DeleteOrderRule',
        update: 'UpdateOrderRule'
      },
      primaryKey: ['ConnectorID', 'RuleID', 'VendorID'],
      singularObjectName: 'Order Rule',
      pluralObjectName: 'Order Rules',
      sortBy: 'ConnectorID',
      groupField: 'ConnectorID',
      structure:
      [
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          editable: false,
          editor: {
            xtype: 'connector'
          },
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'RuleID', type: 'int', header: 'Rule', renderer: Concentrator.renderers.field('orderRules', 'Name'),
          editable: false,
          editor: {
            xtype: 'orderRule'
          }
        },
        { dataIndex: 'Value', type: 'string', header: 'Value', editor: {
          xtype: 'textfield', allowBlank: false
        }
        },
        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor',
          renderer: Concentrator.renderers.field('vendors', 'VendorName'),
          editor: { xtype: 'vendor' }
        }
      ]
    });
    return grid;
  }
});