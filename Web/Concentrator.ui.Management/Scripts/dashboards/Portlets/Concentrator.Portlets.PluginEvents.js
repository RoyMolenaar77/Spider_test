/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.PluginEvents = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  requireGearTool: true,
  constructor: function (config) {
    config.title = 'Plugin Events';
    config.loadUrl = Concentrator.route('GetEventsChart', 'Plugin');
    config.height = 350;
    Ext.apply(this, config);

    //    this.customFilter = new Concentrator.Filter.ImageSizeFilter({
    //      portlet: this
    //    });

    Concentrator.Portlets.PluginErrors.superclass.constructor.call(this, config);

    this.registerCustomFilter(this, this.customFilter);

  }
});