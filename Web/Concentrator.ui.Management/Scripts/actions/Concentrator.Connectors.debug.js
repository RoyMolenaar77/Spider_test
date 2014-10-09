/// <reference path="~/Content/js/ext/ext-base-debug.js" /> 
/// <reference path="~/Content/js/ext/ext-all-debug.js" />
Concentrator.Connectors = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {
    var self = this;

    var connectorSystemStore = new Ext.data.JsonStore({
      autoDestroy: false,
      url: Concentrator.route("GetConnectorSystemStore", "Connector"),
      method: 'GET',
      root: 'results',
      idProperty: 'ConnectorSystemID',
      fields: ['ConnectorSystemID', 'ConnectorSystemName']
    });

    var connectorStore = new Ext.data.JsonStore({
      autoDestroy: false,
      url: Concentrator.route("GetConnectorTypeStore", "Connector"),
      method: 'GET',
      root: 'results',
      idProperty: 'ConnectorType',
      fields: ['ConnectorType', 'ConnectorTypeName']
    });

    var parentConnectorStore = new Ext.data.JsonStore({
      autoDestroy: false,
      url: Concentrator.route("GetParentConnectorStore", "Connector"),
      method: 'GET',
      root: 'results',
      idProperty: 'ParentConnectorID',
      fields: ['ParentConnectorID', 'ParentConnectorName']
    });

    this.parentConnectorStore = parentConnectorStore;
    this.connectorStore = connectorStore;
    this.connectorSystemStore = connectorSystemStore;

    this.grid = new Diract.ui.Grid({
      pluralObjectName: 'Connectors',
      singularObjectName: 'Connector',
      primaryKey: 'ConnectorID',
      url: Concentrator.route("GetList", "Connector"),
      permissions: {
        list: 'GetConnector',
        create: 'CreateConnector',
        remove: 'DeleteConnector',
        update: 'UpdateConnector'
      },
      callback: function () {
        Concentrator.stores.connectors.reload();
      },
      structure: [
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',
          editor: { xtype: 'connector', allowBlank: false },
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'Name', header: 'Connector', type: 'string', editable: false }, //editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'System', header: 'System', type: 'string' }
      ],
      customButtons:
      [
        {
          text: 'Add new Connector',
          iconCls: 'add',
					roles : 'CreateConnector',
          handler: function () {
            self.addConnector();
          }
        }
      ],
      rowActions: [
          {
            iconCls: 'upload-icon',
            text: 'Connector logo',
            handler: function (record) {
              self.addConnectorLogo(record);
            },
            roles: ['UpdateConnector']
          },
          {
            iconCls: 'upload-icon',
            text: 'Default image',
            handler: function (record) {
              self.addDefaultimage(record);
            },
            roles: ['UpdateConnector']
          },
          {
            iconCls: 'wrench',
            text: 'Settings',
            handler: function (record) {
              self.getConnectorDetails(record);
            },
            roles: ['UpdateConnector']
          },
          {
            iconCls: 'wrench',
            text: 'Connector Settings',
            handler: function (record) {
              self.getSettingKeyWindow(record);
            },
            roles: ['GetConnector']
          }
        ]
    });

    return this.grid;
  },

  addConnector: function () {
    var self = this;

    var wiz = new Concentrator.ui.ConnectorWizard({
      self: self
    });

    wiz.show();
  },

  addDefaultimage: function (record) {

    var wind = new Diract.ui.FormWindow({
      url: Concentrator.route('AddDefaultImage', 'Connector'),
      title: 'Connector settings',
      buttonText: 'Save',
      loadUrl: Concentrator.route('Get', 'Connector'),
      loadParams: { connectorID: record.get('ConnectorID') },
      params: {
        connectorID: record.get('ConnectorID')
      },
      fileUpload: true,
      cancelButton: true,
      bodyCfg: { tag: 'center' },
      items: [
            {
              xtype: 'fileuploadfield',
              emptyText: 'Default image',
              name: 'DefaultImage',
              id: 'defaultimgid',
              hiddenName: 'DefaultImage',
              fieldLabel: 'Default image:',
              buttonText: '',
              width: 165,
              buttonCfg: { iconCls: 'upload-icon' },
              allowBlank: true
            },
            {
              xtype: 'button',
              handler: function () {
                Diract.request({
                  url: Concentrator.route('DeleteDefaultImage', 'Connector'),
                  success: function () {
                    wind.destroy();
                  },
                  params: {
                    connectorID: record.get('ConnectorID')
                  }

                });
              },
              text: 'Delete Default image',
              name: 'deleteDefault',
              hiddenName: 'deleteDefault',
              buttonText: '',
              width: 120,
              style: 'margin-top: 10px'
            }
            ],
      layout: 'fit',
      height: 150,
      width: 310,
      success: (function () {
        this.grid.store.reload();
        wind.destroy();
      }).createDelegate(this)
    });
    wind.show();

  },

  addConnectorLogo: function (record) {

    var wind = new Diract.ui.FormWindow({
      url: Concentrator.route('AddImage', 'Connector'),
      title: 'Connector settings',
      buttonText: 'Save',
      loadUrl: Concentrator.route('Get', 'Connector'),
      loadParams: { connectorID: record.get('ConnectorID') },
      params: {
        connectorID: record.get('ConnectorID')
      },
      fileUpload: true,
      cancelButton: true,
      bodyCfg: { tag: 'center' },
      items: [
            {
              xtype: 'fileuploadfield',
              emptyText: 'Connector Logo',
              name: 'ConnectorLogoPath',
              id: 'conlogoid',
              hiddenName: 'ConnectorLogoPath',
              fieldLabel: 'Connector Logo:',
              buttonText: '',
              width: 165,
              buttonCfg: { iconCls: 'upload-icon' },
              allowBlank: true
            },
            {
              xtype: 'button',
              handler: function () {
                Diract.request({
                  url: Concentrator.route('DeleteLogo', 'Connector'),
                  success: function () {
                    wind.destroy();
                  },
                  params: {
                    connectorID: record.get('ConnectorID')
                  }

                });
              },
              text: 'Delete Connector logo',
              name: 'deleteLogo',
              hiddenName: 'deleteLogo',
              buttonText: '',
              width: 120,
              style: 'margin-top: 10px'
            }
            ],
      layout: 'fit',
      height: 150,
      width: 310,
      success: (function () {
        this.grid.store.reload();
        wind.destroy();
      }).createDelegate(this)
    });
    wind.show();

  },

  getConnectorDetails: function (record) {
    var window = new Diract.ui.FormWindow({
      url: Concentrator.route('Update', 'Connector'),
      title: 'Connector settings',
      buttonText: 'Save',
      autoScroll: true,
      labelWidth: 200,
      fileUpload: true,
      loadUrl: Concentrator.route('Get', 'Connector'),
      loadParams: { connectorID: record.get('ConnectorID') },
      params: {
        connectorID: record.get('ConnectorID')
      },
      cancelButton: true,
      items: [
            {
              xtype: 'textfield',
              name: 'Name',
              fieldLabel: 'Name',
              width: 175
            },
            {
              xtype: 'multiselect',
              label: 'Connector Type',
              multi: true,
              width: 165,
              label: 'Connector Type',
              displayField: 'ConnectorTypeName',
              valueField: 'ConnectorType',
              store: this.connectorStore,
              allowBlank: false,
              width: 175
            },
            {
              xtype: 'select',
              label: 'Connector System ID',
              hiddenName: 'ConnectorSystemID',
              allowBlank: true,
              displayField: 'ConnectorSystemName',
              valueField: 'ConnectorSystemID',
              store: this.connectorSystemStore,
              width: 175
            },
            {
              xtype: 'select',
              label: 'Parent connector ID',
              hiddenName: 'ParentConnectorID',
              allowBlank: true,
              displayField: 'ParentConnectorName',
              valueField: 'ParentConnectorID',
              store: this.parentConnectorStore,
              width: 175
            },
            { xtype: 'textfield', name: 'BSKIdentifier', fieldLabel: 'BSK identifier', width: 165/*, value: rec.BSKIdentifier */ },
            { xtype: 'textfield', name: 'BackendEanIdentifier', fieldLabel: 'Ean identifier', width: 165/*, value: rec.BackendEanIdentifier */ },
            { xtype: 'textfield', name: 'Connection', fieldLabel: 'Connection', width: 165/*, value: rec.Connection */ },
            { xtype: 'checkbox', name: 'ObsoleteProducts', fieldLabel: 'Include obsolete products' },
            { xtype: 'checkbox', name: 'ZipCodes', fieldLabel: 'Include zip codes' },
            { xtype: 'checkbox', name: 'ConcatenateBrandName', fieldLabel: 'Concatenate brand names' },
            { xtype: 'checkbox', name: 'Selectors', fieldLabel: 'Include selectors' },
            { xtype: 'checkbox', name: 'OverrideDescriptions', fieldLabel: 'Override descriptions' },
            { xtype: 'checkbox', name: 'UseConcentratorProductID', fieldLabel: 'Use concentrator identifiers' },
            { xtype: 'checkbox', name: 'ImportCommercialText', fieldLabel: 'Import commercial text' },
            { xtype: 'checkbox', name: 'IsActive', fieldLabel: 'Active' }
      ],
      layout: 'fit',
      height: 565,
      width: 320,
      success: (function () {
        this.grid.store.reload();
        window.destroy();
      }).createDelegate(this)

    });

    window.show();

  },
  
  getSettingKeyWindow: function(record) {
    var params = {
      connectorID: record.id
    };

    var connectorSettingGrid = new Diract.ui.Grid({
      singularObjectName: 'setting',
      pluralObjectName: 'settings',
      permissions: { all: 'Default' },
      url: Concentrator.route('GetConnectorSettings', 'Connector'),
      //newUrl: Concentrator.route('Create', 'VendorSettings'),
      //deleteUrl: Concentrator.route('Delete', 'VendorSettings'),
      updateUrl: Concentrator.route("UpdateConnectorSetting", 'Connector'),
      params: params,
      newParams: params,
      primaryKey: ['SettingKey', 'ConnectorID'],
      sortBy: 'SettingKey',
      structure: [
        { dataIndex: 'ConnectorID', type: 'int' },
        { dataIndex: 'SettingKey', type: 'string', header: 'Setting', editable: false, editor: { xtype: 'textfield' } },
        { dataIndex: 'Value', type: 'string', header: 'Value', editable: true, editor: { xtype: 'textfield' } }
      ]
    });

    var connectorSettingWindow = new Ext.Window({
      title: 'Connector Settings',
      width: 900,
      layout: 'fit',
      height: 600,
      items: [connectorSettingGrid]
    });

    connectorSettingWindow.show();

  }
});