Concentrator.ConnectorProductStatus = Ext.extend(Concentrator.GridAction, {

  getPanel: function () {
    var btn = new Concentrator.ui.ToggleMappingsButton({ roles: ['ShowUnmappedProducts'], grid: function () { return grid; }, mappingField: 'ConcentratorStatusID' });

    var grid = new Concentrator.ui.Grid({

      singularObjectName: 'Connector Status',
      pluralObjectName: 'Connector Statuses',
      url: Concentrator.route('GetList', 'ConnectorProductStatus'),
      updateUrl: Concentrator.route('Update', 'ConnectorProductStatus'),
      customButtons: [],
      newUrl: Concentrator.route('Create', 'ConnectorProductStatus'),
      deleteUrl: Concentrator.route('Delete', 'ConnectorProductStatus'),
      onGridFilterInitialized: function () {
        btn.toggleClass(true);
      },
      permissions: {
        list: 'GetConnectorProductStatus',
        create: 'CreateConnectorProductStatus',
        remove: 'DeleteConnectorProductStatus',
        update: 'UpdateConnectorProductStatus'
      },
      primaryKey: ['ConnectorID', 'ConnectorStatusID', 'ConnectorStatus', 'ConcentratorStatusID', 'ConnectorProductStatusID'],
      sortBy: 'ConnectorID',
      structure: [
        { dataIndex: 'ConnectorProductStatusID', type: 'int' },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          renderer: function (val, meta, record) {
            return record.get("Connector");
          },
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' },
          editor: {
            xtype: 'connector',
            allowBlank: false
          }
        },
        { dataIndex: 'Connector', type: 'string' },

        { dataIndex: 'ConnectorStatusID', type: 'int', header: 'Connector Status',
          renderer: function (val, m, re) {
            return re.get('ConnectorStatus');
          },
          editor: {
            xtype: 'connectorStatus',
            roles: ['CreateProductStatus']
            //hiddenName: 'ConnectorStatusID'
          },
          filter: {
            type: 'string',
            fieldLabel: 'Status',
            filterField: 'ConnectorStatus'
          }
        },
        { dataIndex: 'ConnectorStatus', type: 'string' },

        { dataIndex: 'ConcentratorStatusID', type: 'int', header: 'Concentrator Status',
          renderer: function (val, m, re) {
            return re.get('ConcentratorStatus');
          },
          editor: {
            xtype: 'concentratorstatus',
            roles: ['CreateProductStatus'],
            hiddenName: 'ConcentratorStatusID'
          },
          filter: {
            type: 'string',
            fieldLabel: 'Status',
            filterField: 'ConcentratorStatus'
          }
        },
        { dataIndex: 'ConcentratorStatus', type: 'string' }
      ]
    });
    return grid;
  }
});