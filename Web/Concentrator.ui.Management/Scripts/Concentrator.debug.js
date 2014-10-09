;/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator');
Ext.ns('Concentrator.DashboardItems');

Ext.Ajax.timeout = 18000000;

//fix for windows and editors : in a grid, a search box editor appears to be always infront of the window.
Ext.WindowMgr.zseed = 12000;

Concentrator = {
  ui: {},
  adapter: {},
  user: {},

  ResizeImageFromUrl: function (url, width, height) {
    return Concentrator.route('ResizeFromUrl', 'Image', { url: url, width: width, height: height });
  },
  GetImageUrl: function (path, width, height, headerImage, imageUrl) {
    return Concentrator.route('Fetch', 'Image', { imagePath: path, width: width, height: height, headerImage: headerImage, imageUrl: imageUrl });
  },
  GetProductAttributeValueGroupImageUrl: function (path, width, height) {
    return Concentrator.route('FetchImageUrl', 'ProductAttributeValueGroup', { imagePath: path, width: width, height: height });
  },
  GetBrandLogoUrl: function (path, width, height) {
    return Concentrator.route('FetchLogoUrl', 'BrandVendor', { imagePath: path, width: width, height: height });
  },
  getMandatoryFields: function () {
    var fields = [];
    if (Concentrator.mandatoryFieldsPointer) {
      fields = Concentrator[Concentrator.mandatoryFieldsPointer]
    }

    return fields;
  },

  init: function () {
    var self = this;


    Diract.init();
    self.initDate();
    Concentrator.adapter.createAliases();
    Concentrator.adapter.createOverrides();

    Concentrator.stores = new StoreRepository();
    Concentrator.dashboards = new Object();

    Concentrator.FormConfigurations = {};
    Ext.apply(Concentrator.FormConfigurations, new Concentrator.FormConfigurationsRepository());
    Ext.apply(Concentrator.editors, new ConcentratorEditorRepository());

    this.Navigation = new Diract.Navigation({
      menuItems: Concentrator.pages,
      height: 500,
      border: true
    });

    Concentrator.navigationPanel = new Ext.Panel({
      region: 'west',
      width: 200,
      minWidth: 150,
      layout: 'border',
      baseCls: 'x-plain',
      border: true,
      margins: '4 0 0 0',
      collapseMode: 'mini',
      split: true,
      items: [
        {
          title: 'Search',
          region: 'north',
          height: 110,
          layout: 'border',
          width: 'fit',
          border: true,
          padding: 4,
          items: [
            {
              region: 'north',
              xtype: 'connector',
              allowBlank: true,
              emptyText: 'Change connector...',
              margins: '5 5 5 5',
              notGridContext: true,
              onEnterAction: function (connectorID, connector) {
                Diract.silent_request({
                  url: Concentrator.route("SetContentSettings", "User"),
                  params: {
                    connectorID: connectorID
                  },
                  success: function (response) {
                    setTimeout(function () {
                      window.location.reload();
                    }, 2000);

                    Ext.Msg.alert('Status', response.message);
                  }
                });
              }
            },
            {
              xtype: 'product',
              region: 'center',
              allowBlank: true,
              emptyText: 'Product...',
              margins: '0 5 5 5',
              notGridContext: true,
              onEnterAction: function (productID, product) {
                var factory = new Concentrator.ProductBrowserFactory({ productID: productID, product: product, isSearched: true });
              }
            }
          ]
        },
        this.Navigation
      ]
    });

    Concentrator.ViewInstance = new Concentrator.Viewport({
      renderTo: Ext.getBody(),
      layout: 'border',
      //navigationPanel: this.Navigation,
      navigationPanel: Concentrator.navigationPanel,
      tabPanel: new Ext.TabPanel({
        id: 'content-panel',
        region: 'center',
        enableTabScroll: true,
        activeTab: 0,
        margins: '4 0 0 0',
        border: true,
        items: [
          {
            closable: false,
            title: 'Main',
            layout: 'border',
            items: [new Concentrator.Portal({ portalID: -1 })]
          }],
        deferredRender: false,
        layoutOnTabChange: true,
        // bodyStyle: 'padding:10px', Removed Padding
        autoDestroy: true,
        listeners: {
          'afterrender': function () {
            Ext.Ajax.on('beforerequest', function () {

              if (Concentrator.user.timeout != null) {
                if (Concentrator.user.loggedIn) {
                  if (self.timeout) {
                    window.clearTimeout(self.timeout);
                  }
                  self.timeout = window.setTimeout(function () {
                    Concentrator.mute_request({
                      url: Concentrator.route("DoLogout", "Account")
                    });
                    Concentrator.user.loggedIn = false;
                  }, (Concentrator.user.timeout * 60000));
                }
              }
            });
          }
        },

        plugins: [new Ext.ux.TabCloseMenu()]
      })
      ,
      headerPanel: {
        region: 'north',
        xtype: 'box',
        applyTo: 'header',
        height: 30

      }
    });
    //self.initLanguage();
    self.initControlPanel();
    Ext.QuickTips.init();
  },
  initDate: function () {
    Ext.data.Field.prototype = {
      dateFormat: 'M$',
      defaultValue: "",
      mapping: null,
      sortType: null,
      sortDir: "ASC"
    };
  },
  initLanguage: function () {
    //get value or default
    var self = this;

    //better performance. Note: Languages hardcoded
    var langStore = [[1, 'English'], [2, 'Nederlands']];

    var languageCombo = new Diract.ui.Select({
      fieldLabel: 'See content in: ',
      valueField: 'ID',
      store: langStore,
      mode: 'local',
      lazyInit: false,
      applyTo: 'language-selection',
      displayField: 'Name',
      emptyText: 'Choose a language',
      listeners: {
        change: function (el, newVal, olVal) {
          self.setLanguage(newVal);
        }
      }
    });

    languageCombo.setValue(Concentrator.user.languageID);
  },

  initControlPanel: function () {
    var a = new Ext.Element('control-panel-link');

    a.on("click", function () {
      var controlPanel = new Ext.Window({
        modal: true,
        width: 1010,
        padding: 10,
        title: 'Control Panel',
        height: 540,
        layout: 'fit',
        items: new Concentrator.ControlPanel()
      });

      controlPanel.show();
    }, this);
  },

  setLanguage: function (id) {
    if (Concentrator.user.languageID == id)
      return;

    Diract.request({
      url: Concentrator.route('SetLanguage', 'User'),
      params: {
        languageID: id
      },
      success: function () {
        Concentrator.stores = new StoreRepository();
        Concentrator.user.languageID = id;
      }
    });
  }
};

Ext.apply(Ext.form.VTypes, {
  sourceAndDestination: function (value, field) {
    var otherField = Ext.getCmp(field.otherField);
    var otherValue = otherField.value;

    return (otherValue != field.value);
  },
  sourceAndDestinationText: "The destination cannot be the same as the source"
});

Ext.apply(Ext, {
  objectEmpty: function (obj) {
    for (var key in obj)
      return false;

    return true;
  }
});

Ext.apply(Ext.form.Label.prototype, {
  setText: function (t, encode) {
    if (Concentrator.stores) {
      t = Concentrator.renderers.field('mangementLabels', 'Name')(t) || t;

      var e = encode === false;

      this[!e ? 'text' : 'html'] = t;
      delete this[e ? 'text' : 'html'];

      if (this.rendered) {
        this.el.dom.innerHTML = encode !== false ? Ext.util.Format.htmlEncode(t) : t;
      }
    }

    return this;
  }
});

Ext.apply(Ext, {
  guid: function () {
    var S4 = function () {
      return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
    };
    return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());

  }
});

Ext.apply(Ext.Component.prototype, {
  listeners: {
    'beforerender': function (comp) {
      if (Concentrator.stores) {
        var t = comp.fieldLabel;
        if (typeof comp.label != "string") {
          if (t && comp.label) {
            var text = Concentrator.renderers.field('mangementLabels', 'Name')(t) || t;

            comp.label.update(text);
          }
        }
      }
    }
  }
});
