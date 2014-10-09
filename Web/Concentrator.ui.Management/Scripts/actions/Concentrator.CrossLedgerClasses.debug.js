Concentrator.CrossLedgerClasses = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Diract.ui.Grid({
      singularObjectName: 'Cross Ledger Class',
      pluralObjectName: 'Cross Ledger Classes',
      primaryKey: ['ConnectorID', 'LedgerclassCode'],
      sortBy: 'ConnectorID',
      groupField: 'ConnectorID',
      url: Concentrator.route('GetList', 'CrossLedger'),
      newUrl: Concentrator.route('Create', 'CrossLedger'),
      updateUrl: Concentrator.route('Update', 'CrossLedger'),
      deleteUrl: Concentrator.route('Delete', 'CrossLedger'),
      permissions: {
        list: 'CrossLedger',
        create: 'CrossLedger',
        remove: 'CrossLedger',
        update: 'CrossLedger'
      },
      structure: [
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector', renderer: Concentrator.renderers.field('connectors', 'Name'),
          editable: false,
          editor: { xtype: 'connector'},
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'LedgerclassCode', type: 'string', editable: false, header: 'Code', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'CrossLedgerclassCode', type: 'string', header: 'Cross Code', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textfield', allowBlank: false} }
      ]
    });
    return grid;
  }
});