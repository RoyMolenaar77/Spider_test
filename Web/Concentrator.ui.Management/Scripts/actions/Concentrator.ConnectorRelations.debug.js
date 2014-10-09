Concentrator.ConnectorRelations = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {
    var self = this;

    

    var outboundMessageTypeStore = new Ext.data.JsonStore({
      autoDestroy: false,
      url: Concentrator.route("GetOutboundMessageTypes", "ConnectorRelation"),
      method: 'GET',
      root: 'results',
      idProperty: 'OutboundMessageTypeID',
      fields: ['OutboundMessageTypeID', 'OutboundMessageTypeName']
    });

    this.outboundMessageTypeStore = outboundMessageTypeStore;

    this.relationGrid = new Concentrator.ui.Grid({
      pluralObjectName: 'Connector relations',
      singularObjectName: 'Connector relation',
      primaryKey: ['ConnectorRelationID'],
      sortBy: 'ConnectorRelationID',
      url: Concentrator.route("GetList", "ConnectorRelation"),
      deleteUrl: Concentrator.route("Delete", "ConnectorRelation"),
      updateUrl: Concentrator.route("Update", "ConnectorRelation"),
      permissions: {
        list: 'GetConnectorRelation',
        create: 'CreateConnectorRelation',
        update: 'UpdateConnectorRelation',
        remove: 'DeleteConnectorRelation'
      },
      structure: [
        { dataIndex: 'ConnectorRelationID', type: 'int', header: 'Connector Relation ID' },
        { dataIndex: 'CustomerID', type: 'int', header: 'Customer ID', width: 50, editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'ConnectorType', type: 'int', header: 'Connector Type',
          editor: {
            xtype: 'connectorType',
            allowBlank: true,
            labelStyle: 'width: 160px'
          },
          filter: {
            type: 'list',
            store: Concentrator.stores.connectorType,
            labelField: 'ConnectorTypeName'
          },
          renderer: function (val, m, record) {
            var rec = Concentrator.stores.connectorType.getById(val);
            return rec.get('ConnectorTypeName');
          } //Concentrator.renderers.field('connectorType', 'ConnectorTypeName')
        },
        { dataIndex: 'LanguageID', type: 'int', header: 'Language',
          editor: { xtype: 'language', allowBlank: false },
          renderer: Concentrator.renderers.field('languages', 'Name')
        },
        { dataIndex: 'Name', type: 'string', header: 'Name', editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'Contact', type: 'string', header: 'Contact', editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'AdministrationCode', type: 'string', header: 'Administration Code', editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'OrderType', type: 'string', header: 'Administration Code', editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'IsActive', type: 'boolean', header: 'Is Active', editable: true, editor: { xtype: 'bool'} },
        { dataIndex: 'FreightProduct', type: 'string', header: 'Freight Product', editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'FinChargesProduct', type: 'string', header: 'Fin Charges Product', editable: true, editor: { xtype: 'textfield'} },
        { dataIndex: 'EdiVendorID', type: 'int', header: 'Edi Vendor', editable: true,
          renderer: function (val, meta, record) {
            return record.get('EdiVendor');
          },
          editor: { xtype: 'textfield' }
        },
        { dataIndex: 'EdiVendor', type: 'string' },
        { dataIndex: 'XtractType', type: 'int', header: 'Xtract Type', editable: true,
          editor: {
            xtype: 'xtractType',
            allowBlank: true,
            labelStyle: 'width: 160px'
          },
          filter: {
            type: 'list',
            store: Concentrator.stores.xtractTypes,
            labelField: 'XtractTypeName'
          },
          renderer: function (val, m, record) {
            var rec = Concentrator.stores.xtractTypes.getById(val);
            if (!rec) {
              return "";
            }
            return rec.get('XtractTypeName');
          } //Concentrator.renderers.field('connectorType', 'ConnectorTypeName')
        }
      ],
      windowConfig: {
        height: 600,
        width: 450
      },
      rowActions: [
         {
           text: 'Settings',
           iconCls: 'merge',
           handler: (function (record) {

             var grid = new Concentrator.ui.Grid({
               pluralObjectName: 'Connector relations',
               singularObjectName: 'Connector relation',
               primaryKey: ['ConnectorRelationID'],
               sortBy: 'ConnectorRelationID',
               params: {
                 connectorRelationID: record.get('ConnectorRelationID')
               },
               permissions: {
                 list: 'GetConnectorRelation',
                 update: 'UpdateConnectorRelation'
               },
               url: Concentrator.route("GetList", "ConnectorRelation"),
               updateUrl: Concentrator.route('Update', 'ConnectorRelation'),
               structure: [
                { dataIndex: 'ConnectorRelationID', type: 'int' },
                { dataIndex: 'AuthorisationAddresses', type: 'string', header: 'Authorisation Addresses', editable: true, width: 150,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'AccountPrivileges', type: 'int', header: 'Account Privileges',
                  editor: {
                    xtype: 'accountPrivilegesType',
                    allowBlank: true,
                    labelStyle: 'width: 160px'
                  },
                  filter: {
                    type: 'list',
                    store: Concentrator.stores.accountPrivilegeStore,
                    labelField: 'AccountPrivilegeName'
                  },
                  renderer: function (val, m, record) {
                    var rec = Concentrator.stores.accountPrivilegeStore.getById(val);
                    if (!rec) {
                      return "";
                    }
                    return rec.get('AccountPrivilegeName');
                  } 
                },
                { dataIndex: 'UseFtp', type: 'boolean', header: 'Use Ftp', width: 150,
                  editor: { xtype: 'checkbox', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'ProviderType', type: 'string', header: 'Provider Type', editable: true,
                  editor: {
                    xtype: 'providerType',
                    allowBlank: true,
                    labelStyle: 'width: 160px'
                  },
                  filter: {
                    type: 'list',
                    store: Concentrator.stores.providerTypeStore,
                    labelField: 'ProviderTypeName'
                  },
                  renderer: function (val, m, record) {
                    var rec = Concentrator.stores.providerTypeStore.getById(val);
                    if (!rec) {
                      return "";
                    }
                    return rec.get('ProviderTypeName');
                  } 
                },
                { dataIndex: 'FtpType', type: 'string', header: 'Ftp Type', editable: true,
                  editor: {
                    xtype: 'ftpType',
                    allowBlank: true,
                    labelStyle: 'width: 160px'
                  },
                  filter: {
                    type: 'list',
                    store: Concentrator.stores.ftpTypeStore,
                    labelField: 'FtpTypeName'
                  },
                  renderer: function (val, m, record) {
                    var rec = Concentrator.stores.ftpTypeStore.getById(val);
                    if (!rec) {
                      return "";
                    }
                    return rec.get('FtpTypeName');
                  } 
                },
                { dataIndex: 'FtpFrequency', type: 'int', header: 'Ftp Frequency', editable: true, width: 150,
                  editor: { xtype: 'int', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'FtpAddress', type: 'string', header: 'Ftp Address', editable: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'FtpUserName', type: 'string', header: 'Ftp UserName', editable: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'FtpPass', type: 'string', header: 'Ftp Pass', editable: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'FtpPort', type: 'int', header: 'Ftp Port', editable: true,
                  editor: { xtype: 'int', labelStyle: 'width: 160px' }
                }
              ]
             });

             var window = new Ext.Window({
               width: 1200,
               height: 300,
               modal: true,
               layout: 'fit',
               items: [
                grid
              ]
             });

             window.show();
           }).createDelegate(this)

         },
          {
            text: 'Exports',
            iconCls: 'merge',
            handler: (function (record) {

              var grid = new Concentrator.ui.Grid({
                pluralObjectName: 'Connector Relations Export',
                singularObjectName: 'Connector Relation Export',
                primaryKey: ['ConnectorRelationExportID'],
                sortBy: 'ConnectorRelationExportID',
                params: {
                  connectorRelationID: record.get('ConnectorRelationID')
                },
                newParams: {
                  ConnectorRelationID: record.get('ConnectorRelationID')
                },
                url: Concentrator.route("GetExportList", "ConnectorRelation"),
                deleteUrl: Concentrator.route("DeleteConnectorRelationExport", "ConnectorRelation"),
                updateUrl: Concentrator.route("UpdateConnectorRelationExport", "ConnectorRelation"),
                newUrl: Concentrator.route("CreateConnectorRelationExport", "ConnectorRelation"),
                permissions: {
                  list: 'GetConnectorRelation',
                  create: 'CreateConnectorRelation',
                  update: 'UpdateConnectorRelation',
                  remove: 'DeleteConnectorRelation'
                },
                structure: [
        { dataIndex: 'ConnectorRelationExportID', type: 'int' },
        { dataIndex: 'ConnectorRelationID', type: 'int' },
                { dataIndex: 'SourcePath', type: 'string', header: 'File Source Path', editable: true, 
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px'}
                },
                { dataIndex: 'DestinationPath', type: 'string', header: 'Ftp Destination Path', editable: true, allowBlank: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px'}
                }
                ]
              });

              var window = new Ext.Window({
                width: 1200,
                height: 300,
                modal: true,
                layout: 'fit',
                items: [
                grid
              ]
              });

              window.show();
            }).createDelegate(this)

          },
        {
          text: 'Outbound',
          iconCls: 'default',
          handler: function (record) {

            var grid = new Concentrator.ui.Grid({
              pluralObjectName: 'Connector relations',
              singularObjectName: 'Connector relation',
              primaryKey: ['ConnectorRelationID'],
              sortBy: 'ConnectorRelationID',
              params: {
                connectorRelationID: record.get('ConnectorRelationID')
              },
              permissions: {
                list: 'GetConnectorRelation',
                update: 'UpdateConnectorRelation'
              },
              url: Concentrator.route("GetList", "ConnectorRelation"),
              updateUrl: Concentrator.route('Update', 'ConnectorRelation'),
              structure: [
                { dataIndex: 'ConnectorRelationID', type: 'int' },
                { dataIndex: 'OrderConfirmation', type: 'boolean', header: 'Order Confirmation', width: 150, editor: { xtype: 'checkbox', labelStyle: 'width: 160px'} },
                { dataIndex: 'ShipmentConfirmation', type: 'boolean', header: 'Shipment Confirmation', width: 150,
                  editor: { xtype: 'checkbox', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'InvoiceConfirmation', type: 'boolean', header: 'Invoice Confirmation', width: 150,
                  editor: { xtype: 'checkbox', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'OutboundOrderConfirmation', type: 'string', header: 'Order Confirmation Email', editable: true, width: 150,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'OutboundShipmentConfirmation', type: 'string', header: 'Shipment Confirmation Email', width: 150, editable: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'OutboundInvoiceConfirmation', type: 'string', header: 'Invoice Confirmation Email', width: 150, editable: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'OutboundTo', type: 'string', header: 'Outbound To', width: 150, editable: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'OutboundPassword', type: 'string', header: 'Outbound Password', width: 150, editable: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                },
                { dataIndex: 'OutboundUsername', type: 'string', header: 'Outbound Username', width: 150, editable: true,
                  editor: { xtype: 'textfield', labelStyle: 'width: 160px' }
                }
              ]
            });

            var window = new Ext.Window({
              width: 1200,
              height: 300,
              modal: true,
              layout: 'fit',
              items: [
                grid
              ]
            });

            window.show();
          }
        }
      ],
      customButtons: [
        {
          text: 'Add new Connector Relation',
          iconCls: 'wizard-hat',
          alignRight: true,
          handler: function () {
            self.getWizard();
          }
        }
      ]
    });

    return this.relationGrid;
  },

  getWizard: function () {
    var self = this;

    var wiz = new Concentrator.ui.ConnectorRelationWizard({
      self: self
    });

    wiz.show();
  }

});