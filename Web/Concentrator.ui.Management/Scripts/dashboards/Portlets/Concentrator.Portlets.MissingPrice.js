/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.MissingPrice = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{  
  constructor: function (config) {
    config.title = 'Missing prices';
    config.loadUrl = Concentrator.route('GetMissingPrices', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.MissingPrice.superclass.constructor.call(this, config);
  }
});