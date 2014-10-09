/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.ContentImageSizes = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  requireGearTool: true,
  constructor: function (config) {
    config.title = 'Content image sizes';
    config.loadUrl = Concentrator.route('GetSizeGroups', 'Image');
    config.height = 350;
    Ext.apply(this, config);

    this.customFilter = new Concentrator.Filter.ImageSizeFilter({
      portlet: this
    });

    Concentrator.Portlets.ContentImageSizes.superclass.constructor.call(this, config);

    this.registerCustomFilter(this, this.customFilter);

  },

  getCustomFilters: function () {

    if (this.customFilter.isDestroyed)
      this.customFilter = new Concentrator.Filter.ImageSizeFilter({
        portlet: this
      });

    this.registerCustomFilter(this, this.customFilter);

    return [this.customFilter];
  },

  settingsHandler: function (evt, btn, component) {

  }
});