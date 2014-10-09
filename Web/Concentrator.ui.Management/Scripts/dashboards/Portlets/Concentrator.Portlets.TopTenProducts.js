/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.TopTenProducts = Ext.extend(Concentrator.Portlets.HtmlPortlet,
{
  constructor: function (config) {
    var that = this;
    config.title = 'Top Ten Products';
    config.height = 250;
    config.loadUrl = Concentrator.route('GetTopTenProducts', 'Notifications');
    Ext.apply(that, config);
    Concentrator.Portlets.TopTenProducts.superclass.constructor.call(that, config);
  }
});