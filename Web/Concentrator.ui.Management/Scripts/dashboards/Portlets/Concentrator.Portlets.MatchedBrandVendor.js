/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.MatchedBrandVendor = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    config.title = 'Mapped  Brand Vendors';
    config.loadUrl = Concentrator.route('GetMappedBrandVendors', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.MatchedBrandVendor.superclass.constructor.call(this, config);
  },

  settingsHandler: function (evt, btn, component) {

  }
});