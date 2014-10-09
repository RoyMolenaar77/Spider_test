Concentrator.Plugins = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Plugin',
      pluralObjectName: 'Plugins',
      permissions: {
        list: 'GetPlugin',
        create: 'CreatePlugin',
        update: 'UpdatePlugin',
        remove: 'DeletePlugin'
      },
      url: Concentrator.route('GetList', 'Plugin'),
      updateUrl: Concentrator.route('Update', 'Plugin'),
      primaryKey: 'PluginID',
      structure: [
          { dataIndex: 'PluginID', type: 'int' },
          { dataIndex: 'PluginName', header: 'Plugin Name', type: 'string', editor: { xtype: 'textfield', allowBlank: false} },
          { dataIndex: 'PluginType', header: 'Type', type: 'string' },
          { dataIndex: 'PluginGroup', header: 'Plugin Group', type: 'string' },
          { dataIndex: 'CronExpression', header: 'CronExpression', type: 'string', editor: { xtype: 'textfield', allowBlank: false} },
          { dataIndex: 'ExecuteOnStartup', header: 'Execute on startup', type: 'boolean', editable: true },
          { dataIndex: 'LastRun', header: 'Last Run', type: 'date' },
          { dataIndex: 'NextRun', header: 'Next Run', type: 'date' },
          { dataIndex: 'Duration', header: 'Duration', type: 'string' },
          { dataIndex: 'IsActive', header: 'Is Active', type: 'boolean', editable: true }
      ],

      rowActions: [
//          {
//            text: 'Edit CronExpression',
//            iconCls: 'default',
//            handler: function (record) {
//              
//              var editCron = new Diract.ui.FormWindow({
//                url: Concentrator.route('EditCron', 'plugin'),
//                cancelButton: true,
//                title: 'Edit CronExpression',
//                autoDestroy: true,
//                height: 240,
//                width: 215,
//                modal: true,
//                layout: 'fit',
//                items: [
//                  {
//                    xtype: 'numberfield',
//                    name: 'Month',
//                    //hiddenName: 'month',
//                    fieldLabel: 'Month',
//                    buttonText: '',
//                    width: 75,
//                    allowBlank: true
//                  },
//                  {
//                    xtype: 'numberfield',
//                    name: 'Day',
//                    //hiddenName: 'day',
//                    fieldLabel: 'Day',
//                    buttonText: '',
//                    width: 75,
//                    allowBlank: true
//                  },
//                  {
//                    xtype: 'numberfield',
//                    name: 'Hour',
//                    //hiddenName: 'hour',
//                    fieldLabel: 'Hour',
//                    buttonText: '',
//                    width: 75,
//                    allowBlank: true
//                  },
//                  {
//                    xtype: 'numberfield',
//                    name: 'Minute',
//                    //hiddenName: 'minute',
//                    fieldLabel: 'Minute',
//                    buttonText: '',
//                    width: 75,
//                    allowBlank: true
//                  },
//                  {
//                    xtype: 'numberfield',
//                    name: 'Second',
//                    //hiddenName: 'second',
//                    fieldLabel: 'Second',
//                    buttonText: '',
//                    width: 75,
//                    allowBlank: true
//                  }
//                ],
//                params: {
//                  pluginID: record.get('PluginID')
//                }
//              });

//              editCron.show();             
//            }
//          },
          {
            text: 'View Connector Schedules',
            iconCls: 'default',
            handler: function (record) {
              var view = new Concentrator.ui.Grid({
                permissions: {
                  list: 'GetConnectorSchedule',
                  create: 'CreateConnectorSchedule',
                  update: 'UpdateConnectorSchedule',
                  remove: 'DeleteConnectorSchedule'
                },
                url: Concentrator.route('GetList', 'ConnectorSchedule'),
                newUrl: Concentrator.route("Create", "ConnectorSchedule"),
                deleteUrl: Concentrator.route("Delete", "ConnectorSchedule"),
                updateUrl: Concentrator.route("Update", "ConnectorSchedule"),
                singularObjectName: 'Connector schedule',
                pluralObjectName: 'Connector schedules',
                primaryKey: 'ConnectorScheduleID',
                params: {
                  pluginID: record.get('PluginID')
                },
                newParams: {
                  pluginID: record.get('PluginID')
                },
                structure: [
                    { dataIndex: 'ConnectorScheduleID', type: 'int' },
                    { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
                      renderer: function (val, meta, record) {

                        return record.get('ConnectorName');
                      }, editor: { xtype: 'connector', allowBlank: false }
                    },
                    { dataIndex: 'ConnectorName', type: 'string' },
                    { dataIndex: 'PluginID', type: 'int', header: 'Plugin',
                      renderer: function (val, meta, record) {

                        return record.get('PluginName');
                      }
                    },
                    { dataIndex: 'PluginName', type: 'string' },
                    { dataIndex: 'LastRun', header: 'LastRun', type: 'date' },
                    { dataIndex: 'Duration', header: 'Duration', type: 'string' },
                    { dataIndex: 'ScheduledNextRun', header: 'Scheduled Next Run', type: 'date' },
                    { dataIndex: 'ConnectorSchedulestatus', header: 'Connector schedule status', type: 'int' },
                    { dataIndex: 'ExecuteOnStartup', header: 'Execute on startup', type: 'boolean', editable: true, editor: { xtype: 'boolean' } },
                    { dataIndex: 'CronExpression', header: 'CronExpression', type: 'string', editor: { xtype: 'textfield', allowBlank: false} }
                ]
              });

              var window = new Ext.Window({
                width: 800,
                height: 400,
                modal: true,
                layout: 'fit',
                items: [
                   view
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