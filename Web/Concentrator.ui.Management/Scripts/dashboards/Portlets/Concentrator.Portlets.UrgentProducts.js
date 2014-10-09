/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.UrgentProducts = Ext.extend(Concentrator.Portlets.HtmlPortlet,
{
  constructor: function (config) {
    var that = this;
    config.title = 'Products that have been sent to Wehkamp without enrichment';
    config.height = 100;
    config.width = 300;
    config.loadUrl = Concentrator.route('GetUrgentProducts', 'MissingContent');

    Ext.apply(that, config);

    Concentrator.Portlets.UrgentProducts.superclass.constructor.call(that, config);
  }
});