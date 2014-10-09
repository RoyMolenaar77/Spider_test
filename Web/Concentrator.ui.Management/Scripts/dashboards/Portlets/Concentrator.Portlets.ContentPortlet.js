/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.ContentPortlet = Ext.extend(Concentrator.Portlets.HtmlPortlet,
{
  constructor: function (config) {
    var that = this;
    config.title = 'Media and specifications';
    config.height = 150;
    config.width = 800;
    config.loadUrl = Concentrator.route('GetMediaAndSpecifications', 'Content');

    Ext.apply(that, config);

    Concentrator.Portlets.ContentPortlet.superclass.constructor.call(that, config);
  }
});