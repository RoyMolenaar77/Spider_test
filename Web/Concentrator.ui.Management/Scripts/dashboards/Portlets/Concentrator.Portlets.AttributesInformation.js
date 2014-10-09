/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.AttributesInformation = Ext.extend(Concentrator.Portlets.HtmlPortlet,
{
  constructor: function (config) {
    var that = this;
    config.title = 'Attribute actions';
    config.height = 250;
    config.loadUrl = Concentrator.route('GetNotification', 'ProductAttribute');
    Ext.apply(that, config);
    Concentrator.Portlets.AttributesInformation.superclass.constructor.call(that, config);
  }

});