/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.PreferredContentVendor = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    config.title = 'Preferred content per vendor';
    config.loadUrl = Concentrator.route('GetPreferredContentPerVendor', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.PreferredContentVendor.superclass.constructor.call(this, config);
  },

  settingsHandler: function (evt, btn, component) {

  }
});