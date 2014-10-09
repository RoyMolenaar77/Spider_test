Concentrator.ui.ConnectorRelationWizard = (function () {

  var wiz = Ext.extend(Diract.ui.Wizard, {
    title: 'Connector Relation',
    lazyLoad: true,
    width: 1000,
    height: 530,
    layout: 'fit',
    modal: true,

    constructor: function (config) {
      Ext.apply(this, config);

      this.items = this.getItems();
      this.url = Concentrator.route('Create', 'ConnectorRelation');
      this.grid = this.self.relationGrid;

      Concentrator.ui.ConnectorRelationWizard.superclass.constructor.call(this, config);
    },

    getItems: function () {

      var that = this;

      if (this.pages) {
        return this.pages;
      }

      var accountPrivilegeStore = new Ext.data.JsonStore({
        autoDestroy: false,
        url: Concentrator.route("GetAccountPrivileges", "ConnectorRelation"),
        method: 'GET',
        root: 'results',
        idProperty: 'AccountPrivilegeID',
        fields: ['AccountPrivilegeID', 'AccountPrivilegeName']
      });

      var connectorTypeStore = new Ext.data.JsonStore({
        autoDestroy: false,
        url: Concentrator.route("GetConnectorTypes", "ConnectorRelation"),
        method: 'GET',
        root: 'results',
        idProperty: 'ConnectorTypeID',
        fields: ['ConnectorTypeID', 'ConnectorTypeName']
      });

      var outboundMessageTypeStore = new Ext.data.JsonStore({
        autoDestroy: false,
        url: Concentrator.route("GetOutboundMessageTypes", "ConnectorRelation"),
        method: 'GET',
        root: 'results',
        idProperty: 'OutboundMessageTypeID',
        fields: ['OutboundMessageTypeID', 'OutboundMessageTypeName']
      });

      var providerTypeStore = new Ext.data.JsonStore({
        autoDestroy: false,
        url: Concentrator.route("GetProviderTypes", "ConnectorRelation"),
        method: 'GET',
        root: 'results',
        idProperty: 'ProviderTypeID',
        fields: ['ProviderTypeID', 'ProviderTypeName']
      });

      var ftpTypeStore = new Ext.data.JsonStore({
        autoDestroy: false,
        url: Concentrator.route("GetFtpTypes", "ConnectorRelation"),
        method: 'GET',
        root: 'results',
        idProperty: 'FtpTypeID',
        fields: ['FtpTypeID', 'FtpTypeName']
      });

      var xtractTypeStore = new Ext.data.JsonStore({
        autoDestroy: false,
        url: Concentrator.route("GetXtractTypes", "ConnectorRelation"),
        method: 'GET',
        root: 'results',
        idProperty: 'XtractTypeID',
        fields: ['XtractTypeID', 'XtractTypeName']
      });

      this.accountPrivilegeStore = accountPrivilegeStore;
      this.connectorTypeStore = connectorTypeStore;
      this.outboundMessageTypeStore = outboundMessageTypeStore;
      this.providerTypeStore = providerTypeStore;
      this.ftpTypeStore = ftpTypeStore;
      this.xtractTypeStore = xtractTypeStore;

      var firstPage = new Ext.form.FormPanel({
        bodyStyle: 'padding: 20px;',
        border: false,
        height: 400,
        items: [

            new Ext.form.NumberField({
              fieldLabel: 'Customer ID',
              name: 'CustomerID',
              allowBlank: false,
              width: 175
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Order Confirmation',
              name: 'OrderConfirmation',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Shipment Confirmation',
              name: 'ShipmentConfirmation',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Invoice Confirmation',
              name: 'InvoiceConfirmation',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.TextField({
              fieldLabel: 'Order Confirmation Email',
              name: 'OutboundOrderConfirmation',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.TextField({
              fieldLabel: 'Shipment Confirmation Email',
              name: 'OutboundShipmentConfirmation',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.TextField({
              fieldLabel: 'Invoice Confirmation Email',
              name: 'OutboundInvoiceConfirmation',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.TextField({
              fieldLabel: 'Outbound To',
              name: 'OutboundTo',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.TextField({
              fieldLabel: 'Outbound Password',
              name: 'OutboundPassword',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.TextField({
              fieldLabel: 'Outbound Username',
              name: 'OutboundUsername',
              width: 175,
              allowBlank: true
            }),

            new Diract.ui.Select({
              label: 'Connector Type',
              hiddenName: 'ConnectorType',
              allowBlank: true,
              store: this.connectorTypeStore,
              xtype: 'connectorType',
              displayField: 'ConnectorTypeName',
              width: 175,
              valueField: 'ConnectorTypeID'
            })
        ]
      });

      var secondPage = new Ext.form.FormPanel({
        bodyStyle: 'padding: 20px;',
        border: false,
        height: 550,
        items: [

            new Diract.ui.Select({
              label: 'Outbound Message Type',
              hiddenName: 'OutboundMessageType',
              allowBlank: true,
              width: 175,
              displayField: 'OutboundMessageTypeName',
              valueField: 'OutboundMessageTypeID',
              store: this.outboundMessageTypeStore
            }),

            new Ext.form.TextField({
              fieldLabel: 'Authorisation Addresses',
              name: 'AuthorisationAddresses',
              width: 175,
              allowBlank: true
            }),

            new Diract.ui.Select({
              label: 'AccountPrivileges',
              hiddenName: 'AccountPrivileges',
              allowBlank: true,
              width: 175,
              displayField: 'AccountPrivilegeName',
              valueField: 'AccountPrivilegeID',
              store: this.accountPrivilegeStore
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Use Ftp',
              name: 'UseFtp',
              allowBlank: true
            }),

            new Diract.ui.Select({
              label: 'Provider Type',
              hiddenName: 'ProviderType',
              allowBlank: true,
              displayField: 'ProviderTypeName',
              valueField: 'ProviderTypeID',
              store: this.providerTypeStore,
              width: 175
            }),

            new Diract.ui.Select({
              label: 'Ftp Type',
              hiddenName: 'FtpType',
              allowBlank: true,
              displayField: 'FtpTypeName',
              valueField: 'FtpTypeID',
              width: 175,
              store: this.ftpTypeStore
            }),

            new Ext.form.NumberField({
              fieldLabel: 'Ftp Frequency',
              name: 'FtpFrequency',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.TextField({
              fieldLabel: 'Ftp Address',
              name: 'FtpAddress',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.TextField({
              fieldLabel: 'Ftp UserName',
              name: 'FtpUserName',
              width: 175,
              allowBlank: true
            }),
            

            new Ext.form.TextField({
              fieldLabel: 'Ftp Pass',
              name: 'FtpPass',
              width: 175,
              allowBlank: true
            }),

            new Ext.form.NumberField({
              fieldLabel: 'Ftp Port',
              name: 'FtpPort',
              width: 175,
              allowBlank: true
            }),

            new Diract.ui.Select({
              label: 'Language',
              hiddenName: 'LanguageID',
              allowBlank: true,
              displayField: 'Name',
              valueField: 'ID',
              width: 175,
              store: Concentrator.stores.languages
            })
          ]
      });

      this.pages = [firstPage, secondPage];

      return this.pages;
    }


  });

  return wiz;
})();