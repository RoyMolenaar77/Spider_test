/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.ActiveProducts = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{  
  constructor: function (config) {
    config.title = 'Active products';
    config.loadUrl = Concentrator.route('GetActiveProducts', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.ActiveProducts.superclass.constructor.call(this, config);
  }
});