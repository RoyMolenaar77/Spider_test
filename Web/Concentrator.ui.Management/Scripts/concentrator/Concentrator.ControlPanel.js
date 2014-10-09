/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.ControlPanel = (function () {

  var panel = Ext.extend(Ext.Panel, {
    layout: 'border',
    border: false,
    //Ctor
    constructor: function (config) {
      Ext.apply(this, config);

      this.initLayout();
      this.initMenu(this.west);

      Concentrator.ControlPanel.superclass.constructor.call(this, config);
      this.doLayout();
    },

    /**
    Creates and initialiez the layout : center element and west elements
    */
    initLayout: function () {
      this.center = new Ext.TabPanel({
        region: 'center',
        padding: 10,
        plugins: [new Ext.ux.TabCloseMenu()],
        defaults: {
          closable: true,
          autoDestroy: false,
          autoScroll: true
        },
        border: true
      });

      this.west = new Ext.Panel({
        region: 'west',
        width: 200,
        margins: '0 0 0 0',
        padding: 10,
        collapseMode: 'mini',
        split: true,
        border: true
      });

      this.items = [this.center, this.west];
    },

    /** 
    Initialize the left side menu
    */
    initMenu: function (menuPanel) {
      var that = this;

      menuPanel.add([
            {
              xtype: 'menuitem',
              text: 'Content',
              iconCls: 'package-view',
              id: 'content',
              overCls: 'menu-item-over',
              handler: that.openItem(),
              activeClass: 'x-menu-item-active'
            },
            {
              xtype: 'menuitem',
              id: 'change-password-tab',
              text: 'Change password',
              iconCls: 'wrench',
              overCls: 'menu-item-over',
              handler: that.openItem(),
              activeClass: 'x-menu-item-active'
            },
            {
              xtype: 'menuitem',
              id: 'change-timeout-tab',
              text: 'Set session timeout',
              iconCls: 'wrench',
              overCls: 'menu-item-over',
              handler: that.openItem(),
              activeClass: 'x-menu-item-active'
            },
            {
              xtype: 'menuitem',
              id: 'change-solden-period-tab',
              text: 'Set solden period',
              iconCls: 'wrench',
              overCls: 'menu-item-over',
              handler: that.openItem(),
              activeClass: 'x-menu-item-active'
            }
            ]);
    },

    openItem: function (item, evt) {

      return (function (item, evt) {
        var id = item.el.id;
        var tab = this.center.items.find(function (i) { return i.id === id });
        if (!tab) {
          switch (id) {
            case 'change-password-tab':
              tab = this.getAccountPanel();
              break;
            case 'content':
              tab = this.getContentPanel();
              break;
            case 'change-timeout-tab':
              tab = this.getTimeoutPanel();
              break;
            case 'change-solden-period-tab':
              tab = this.getSoldenPeriodPanel();

          }

          this.center.add(tab);
          this.center.doLayout();
        }
        this.center.setActiveTab(tab);


      }).createDelegate(this);

    },

    getAccountPanel: function () {
      var accountForm = new Ext.Panel({
        title: 'Change password',
        autoDestroy: false,
        id: 'change-password-tab',
        layout: 'fit',
        items: new Diract.ui.Form({
          url: Concentrator.route("ChangePassword", "User"),
          buttonText: "Change",
          cancelButton: true,
          forceFit: false,
          items: [
        new Ext.form.TextField({
          fieldLabel: 'Current Password',
          name: 'CurrentPassword',
          maskRe: /([a-zA-Z0-9\s]+)$/,
          allowBlank: false,
          width: 183,
          inputType: 'password'
        }),
        new Ext.form.TextField({
          fieldLabel: 'New Password',
          name: 'Password',
          id: 'password-1',
          maskRe: /([a-zA-Z0-9\s]+)$/,
          width: 183,
          allowBlank: false,
          inputType: 'password'
        }),
        new Ext.form.TextField({
          fieldLabel: 'Confirm new Password',
          name: 'Password2',
          id: 'password-2',
          maskRe: /([a-zA-Z0-9\s]+)$/,
          allowBlank: false,
          width: 183,
          inputType: 'password',
          vtype: 'password',
          initialPassField: 'password-1'
        })
      ],
          params: {
            userID: Concentrator.user.userID
          }
        })
      });

      return accountForm;
    },
    getTimeoutPanel: function () {
      var self = this;

      var timeoutPanel = new Ext.Panel(
        {
          title: 'Timeout settings',
          layout: 'fit',
          id: 'timeout-settings',
          items: new Diract.ui.Form({
            buttonText: 'Save',
            id: 'timout-panel-item',
            method: 'POST',
            url: Concentrator.route('SetTimeout', 'User'),
            loadUrl: Concentrator.route('GetTimeout', 'User'),
            items: [
              {
                fieldLabel: 'Timeout (in minutes)',
                xtype: 'textfield',
                value: Concentrator.user.timeout,
                hiddenName: 'timeout',
                name: 'timeout',
                invalidText: 'Only numbers greater than 1',
                allowBlank: true,
                validator: function (value) {
                  re = new RegExp('[1-9]+');
                  return re.test(value);
                },
                width: 40
              }
            ],
            params: {
              userID: Concentrator.user.userID
            }
          })
        });

      return timeoutPanel;
    },
    getSoldenPeriodPanel: function () {
      var self = this;

      var soldenPeriodPanel = new Ext.Panel(
        {
          title: 'Solden period settings',
          layout: 'fit',
          id: 'solden-period-settings',
          items: new Diract.ui.Form({
            buttonText: 'Save',
            id: 'solden-period-panel-item',
            method: 'POST',
            url: Concentrator.route('SetSoldenPeriod', 'Config'),
            loadUrl: Concentrator.route('GetSoldenPeriod', 'Config'),
            items: [
              {
                fieldLabel: 'Solden period',
                xtype: 'checkbox',
                //value: Concentrator.user.timeout,
                name: 'isSolden',
                allowBlank: true,
                width: 40
              }
            ]
          })
        });

      return soldenPeriodPanel;
    },
    getContentPanel: function () {
      var self = this;

      var contentForm = new Ext.Panel({
        title: 'Content settings',
        layout: 'fit',
        id: 'content',
        items: [
          {
            region: 'north',
            height: 120,
            items: [
          new Diract.ui.Form({

            buttonText: 'Save',
            id: 'content-panel-item',
            method: 'POST',
            success : function(){
              setTimeout(function () {
                window.location.reload();
              }, 2000);
            },
            url: Concentrator.route('SetContentSettings', 'User'),
            items: [
                    {
                      label: 'See content in',
                      xtype: 'language',
                      value: Concentrator.user.languageID,
                      hiddenName: 'languageID',
                      allowBlank: true,
                      width: 183
                    }
//                    ,
//                    {
//                      xtype: 'connector',
//                      fieldLabel: 'See content for',
//                      value: Concentrator.user.connectorID,
//                      allowBlank: true,
//                      searchInID: true,
//                      notGridContext: true,
//                      width: 183
//                    }
                ]
          })
          ]
                }
//          ,
//          {
//            region: 'center',
//            borders: false,
//            margins: '3 0 0 0',
//            layout: 'fit',
//            items: this.getConnectorVendorManagementGrid()
//          }
         ]

      });
      return contentForm;
    },

    /**
    Grid containing the connector/vendor management settings
    */
    getConnectorVendorManagementGrid: function () {
      return new Diract.ui.Grid({
        singularObjectName: "connector vendor management setting",
        pluralObjectName: 'connector vendor management settings',
        border: false,
        permissions: { all: 'Default' },
        url: Concentrator.route('GetList', 'ConnectorVendorManagementContent'),
        updateUrl: Concentrator.route('Update', 'ConnectorVendorManagementContent'),
        newUrl: Concentrator.route('Create', 'ConnectorVendorManagementContent'),
        deleteUrl: Concentrator.route('Delete', 'ConnectorVendorManagementContent'),
        primaryKey: ['ConnectorID', 'VendorID'],
        sortBy: 'ConnectorID',
        structure: [
          { dataIndex: 'ConnectorID', type: 'string', header: 'Connector', editable: false, renderer: function (val, m, rec) { return rec.get('Connector') }, editor: { xtype: 'connector', allowBlank: false },
            filter: {
              type: 'list',
              labelField: 'Connector',
              store: Concentrator.stores.connectors
            }
          },
          { dataIndex: 'Connector', type: 'string' },

          { dataIndex: 'VendorID', type: 'string', header: 'Vendor', editable: false, renderer: function (val, m, rec) { return rec.get('Vendor') }, editor: { xtype: 'vendor', allowBlank: false },
            filter: {
              type: 'list',
              labelField: 'VendorName',
              store: Concentrator.stores.vendors
            }
          },
          { dataIndex: 'Vendor', type: 'string' },
          { dataIndex: 'IsDisplayed', header: 'Show content', type: 'boolean', editor: { xtype: 'boolean'} }
        ]

      });
    }

  });

  return panel;
})();