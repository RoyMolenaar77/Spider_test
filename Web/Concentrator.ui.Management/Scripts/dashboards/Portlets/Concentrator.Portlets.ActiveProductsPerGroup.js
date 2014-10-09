/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.ActiveProductsPerGroup = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    config.title = 'Active Product Per group';
    config.loadUrl = Concentrator.route('GetActiveProductPerGroup', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.ActiveProductsPerGroup.superclass.constructor.call(this, config);
  },

  settingsHandler: function (evt, btn, component) {

  }
});