/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />
Ext.ns('Concentrator.Portlets');
Concentrator.Portlets.HtmlPortlet = (function () {

  var portlet = Ext.extend(Concentrator.Portlet, {

    listeners: {
      'render': function (component) {
        component.load({
          url: component.loadUrl,
          params: component.getFilterParams()
        });
      }
    },
    refresh: function () {
      this.load({
        url: this.loadUrl,
        params: this.getFilterParams()
      });
    },
    constructor: function (config) {
      Ext.apply(this, config);

      Concentrator.Portlets.HtmlPortlet.superclass.constructor.call(this, config);
    }
  });

  return portlet;

})();

Ext.reg('htmlPortlet', Concentrator.Portlets.HtmlPortlet);