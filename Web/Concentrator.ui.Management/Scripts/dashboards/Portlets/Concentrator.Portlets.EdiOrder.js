/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.EdiOrder = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  requireGearTool: true,
  constructor: function (config) {
    config.title = 'Edi orders';
    config.loadUrl = Concentrator.route('GetEdiOrdersForToday', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    this.customFilter = new Concentrator.Filter.EdiFilter({
      portlet: this
    });

    Concentrator.Portlets.EdiOrder.superclass.constructor.call(this, config);

    this.registerCustomFilter(this, this.customFilter);
  },
  getCustomFilters: function () {

    if (this.customFilter.isDestroyed)
      this.customFilter = new Concentrator.Filter.EdiFilter({
        portlet: this
      });

    this.registerCustomFilter(this, this.customFilter);

    return [this.customFilter];
  }
});