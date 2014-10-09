/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.PublishedProduct = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    config.title = 'Products published to other product groups';
    config.loadUrl = Concentrator.route('PuplishedProducts', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.PublishedProduct.superclass.constructor.call(this, config);
  }
});