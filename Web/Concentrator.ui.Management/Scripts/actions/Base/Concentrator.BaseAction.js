/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />


Concentrator.BaseAction = Ext.extend(Ext.Panel, {

  getPanel: function () {
    //if override not define throw an error;
    throw new "Subclass must implement the getPanel function. It is used to retrieve the content displayed on the page";
  },

  constructor: function (config) {
    var self = this;

    Concentrator.BaseAction.self = self;
    Ext.apply(this, config);

    this.params = this.params || {};

    this.tmpItemsReplacement = this.items;
    this.items = [];
    config.items = [];

    //this.items = items;
    Concentrator.BaseAction.superclass.constructor.call(this, config);
    this.initTempItems();
  },
  initTempItems: function () {
    var self = this;
    if (this.needsConnector) {
      if (!Concentrator.user.connectorID) {
        var conField = { xtype: 'connector', name: 'connector', fieldLabel: 'Connector' };

        var x = new Diract.ui.FormWindow({
          items: conField,
          width: 380,
          height: 200,
          title: 'Specify a connector for which to display content',
          closable: true,
          buttonHandler: function (a, b) {
            self.connectorID = parseInt(x.form.form.getFieldValues().ConnectorID);
            x.hide();
            x.destroy();
            self.handleTempItems();
          }
        });
        x.show();
      } else {
        self.handleTempItems();
      }
    } else {
      self.handleTempItems();
    }
  },

  handleTempItems: function () {
    var it = null;
    if (this.tmpItemsReplacement) it = this.tmpItemsReplacement[0];
    else it = this.getPanel();

    if (!it) throw "Items need to be specified";

    if (this.connectorID) {
      if (it.store) {
        if (it.store.baseParams) {
          Ext.apply(it.store.baseParams, { connectorID: this.connectorID });
        }
      }
    }

    this.add(it);
    this.doLayout();
  },
  refresh: Ext.emptyFn
});