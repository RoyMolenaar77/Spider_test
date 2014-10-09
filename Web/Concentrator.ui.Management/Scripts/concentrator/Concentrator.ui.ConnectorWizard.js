Concentrator.ui.ConnectorWizard = (function () {

  var wiz = Ext.extend(Diract.ui.Wizard, {
    title: 'Connector',
    lazyLoad: true,
    width: 350,
    height: 370,
    layout: 'fit',
    modal: true,

    constructor: function (config) {
      Ext.apply(this, config);

      this.items = this.getItems();
      this.url = Concentrator.route('Create', 'Connector');
      this.grid = this.self.grid;

      Concentrator.ui.ConnectorWizard.superclass.constructor.call(this, config);
    },
    getItems: function () {

      var that = this;

      if (this.pages) {
        return this.pages;
      }
      
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

      var firstPage = new Ext.form.FormPanel({
        bodyStyle: 'padding: 20px;',
        border: false,
        height: 400,
        items: [

            new Ext.form.TextField({
              fieldLabel: 'Name',
              name: 'Name',
              width: 175,
              allowBlank: false
            }),

            new Diract.ui.MultiSelect({
              multi: true,
              label: 'Connector Type',
              allowBlank: false,
              width: 175,
              hiddenName: 'ConnectorType',
              displayField: 'ConnectorTypeName',
              valueField: 'ConnectorType',
              store: this.connectorStore
            }),

            new Diract.ui.Select({
              label: 'Connector System ID',
              hiddenName: 'ConnectorSystemID',
              allowBlank: true,
              width: 175,
              displayField: 'ConnectorSystemName',
              valueField: 'ConnectorSystemID',
              store: this.connectorSystemStore
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Concatenate Brand Name',
              name: 'ConcatenateBrandName',
              allowBlank: false
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Obsolete Products',
              name: 'ObsoleteProducts'
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Zip Codes',
              name: 'ZipCodes'
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Selectors',
              name: 'Selectors'
            }),

            new Ext.form.Checkbox({
              fieldLabel: 'Override Descriptions',
              name: 'OverrideDescriptions'
            })

        ]
      });

      var secondPage = new Ext.form.FormPanel({
        padding: 20,
        border: false,
        height: 400,
        items: [

          new Ext.form.NumberField({
            fieldLabel: 'BSK Identifier',
            name: 'BSKIdentifier',
            width: 175,
            allowBlank: true
          }),

          new Ext.form.TextField({
            fieldLabel: 'Backend EAN Identifier',
            name: 'BackendEanIdentifier',
            width: 175,
            allowBlank: true
          }),

          new Ext.form.Checkbox({
            fieldLabel: 'Use Concentrator Product ID',
            name: 'UseConcentratorProductID'
          }),

          new Ext.form.TextField({
            fieldLabel: 'Connection',
            name: 'Connection',
            width: 175,
            allowBlank: true
          }),

          new Ext.form.Checkbox({
            fieldLabel: 'Import Commercial Text',
            name: 'ImportCommercialText'
          }),

          new Ext.form.Checkbox({
            fieldLabel: 'IsActive',
            name: 'IsActive'
          }),

          new Ext.form.NumberField({
            fieldLabel: 'Administrative Vendor ID',
            name: 'AdministrativeVendorID',
            width: 175,
            allowBlank: true
          }),

          new Ext.form.TextField({
            fieldLabel: 'Outbound Url',
            name: 'OutboundUrl',
            width: 175,
            allowBlank: true
          }),

          new Diract.ui.Select({
            label: 'Parent Connector',
            hiddenName: 'ParentConnectorID',
            allowBlank: true,
            width: 175,
            displayField: 'ParentConnectorName',
            valueField: 'ParentConnectorID',
            store: this.parentConnectorStore
          })
        ]
      });

      this.pages = [firstPage, secondPage];

      return this.pages;
    }


  });

  return wiz;
})();