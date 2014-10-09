Concentrator.ConnectorLanguage = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Connector Languages',
      singularObjectName: 'Connector Language',
      primaryKey: 'ConnectorLanguageID',
      url: Concentrator.route("GetList", "ConnectorLanguage"),
      newUrl: Concentrator.route("Create", "ConnectorLanguage"),
      updateUrl: Concentrator.route("Update", "ConnectorLanguage"),
      deleteUrl: Concentrator.route("Delete", "ConnectorLanguage"),
      permissions: {
        list: 'GetConnectorLanguage',
        create: 'CreateConnectorLanguage',
        remove: 'DeleteConnectorLanguage',
        update: 'UpdateConnectorLanguage'
      },
      structure: [
        { dataIndex: 'ConnectorLanguageID', type: 'int' },
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          editor: { xtype: 'connector', allowBlank: false },
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'LanguageID', type: 'int', header: 'Language',
          editor: { xtype: 'language', allowBlank: false },
          renderer: Concentrator.renderers.field('languages', 'Name')
        },
        { dataIndex: 'Country', type: 'string', header: 'Country', editor: { xtype: 'textfield', allowBlank: true } }
      ]
    });
    return grid;
  }
});