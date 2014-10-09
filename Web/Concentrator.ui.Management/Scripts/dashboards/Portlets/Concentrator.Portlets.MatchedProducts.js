/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.MatchedProducts = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    config.title = 'Matched  Products';
    config.loadUrl = Concentrator.route('GetMatchedProducts', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.MatchedProducts.superclass.constructor.call(this, config);
  },

  settingsHandler: function (evt, btn, component) {

  }
});