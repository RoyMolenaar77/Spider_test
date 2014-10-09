/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');
Concentrator.Portlets.AnychartPortlet = (function () {
  var portlet = Ext.extend(Concentrator.Portlet, {
    layout: 'fit',
    constructor: function (config) {
      Ext.apply(this, config);

      this.initPortlet();
      this.initAnychart();
      this.initPanelItems();

      Concentrator.Portlets.AnychartPortlet.superclass.constructor.call(this, config);

    },

    getUrl: function (params) {

      var url = this.loadUrl;

      if (!params) params = {};

      if (this.customFilter) Ext.apply(params, this.customFilter.getValue());


      if (!Ext.objectEmpty(params)) {
        url += '?' + Ext.urlEncode(params);
      }

      return url;
    },
    refresh: function (params) {
      this.anychartComponent.setXMLFile(this.getUrl(params));
    },

    /**
    sets the config
    */
    initPortlet: function () {
      this.anychartID = this.anychartID || 'anychart-component' + Math.random();
    },


    initAnychart: function () {
      var that = this;

      this.anychartComponent = new AnyChart(Concentrator.anychartSource);
      this.anychartComponent.width = '100%';
      this.anychartComponent.height = (that.height - 30) + 'px';
      this.anychartComponent.wMode = 'opaque';

      this.anychartComponent.setXMLFile(this.getUrl());
    },

    initPanelItems: function () {
      var that = this,
          it = [];

      //any items that are passed in to be rendered above the anychart component
      if (this.topItems) {
        Ext.each(this.topItems, function (i) {
          it.push(i);
        });
      }

      it.push(
      new Ext.Panel({
        id: this.anychartID,
        listeners: {
          'afterrender': function (component) {
            that.anychartComponent.write(that.anychartID);
          }
        }
      })
      );
      this.items = it;
    }
  });
  return portlet;
})();

Ext.reg('anychartPortlet', Concentrator.Portlets.AnychartPortlet);