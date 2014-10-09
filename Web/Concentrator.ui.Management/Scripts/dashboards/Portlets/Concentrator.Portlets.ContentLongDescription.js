/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.ContentLongDescription = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    config.title = ' Missing long description by language';
    config.loadUrl = Concentrator.route('GetMissingLongDescriptionsCount', 'ProductDescription');
    config.height = 450;
    Ext.apply(this, config);

    Concentrator.Portlets.ContentLongDescription.superclass.constructor.call(this, config);
  }
});