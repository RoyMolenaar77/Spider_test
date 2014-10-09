/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.ProductsPerVendor = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    config.title = 'Products per vendor';
    config.loadUrl = Concentrator.route('GetProductsPerVendor', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.ProductsPerVendor.superclass.constructor.call(this, config);
  },

  settingsHandler: function (evt, btn, component) {

  }
});