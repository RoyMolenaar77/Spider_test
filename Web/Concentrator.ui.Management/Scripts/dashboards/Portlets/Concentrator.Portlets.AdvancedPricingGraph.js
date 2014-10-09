/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.AdvancedPricingGraph = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  requireGearTool: true,
  constructor: function (config) {
    config.title = 'Advanced pricing graph';
    config.loadUrl = Concentrator.route('GetPriceProgression', 'Product');
    config.height = 350;

    Ext.apply(this, config);

    this.customFilter = new Concentrator.Filter.AdvancedPricingGraphFilter({
      portlet: this
    });

    Concentrator.Portlets.AdvancedPricingGraph.superclass.constructor.call(this, config);

    this.registerCustomFilter(this, this.customFilter);
  },

  refresh: function () {
    var params = this.portal.collectFilters();
    Concentrator.Portlets.AdvancedPricingGraph.superclass.refresh.call(this, params);
  },
  getCustomFilters: function () {

    if (this.customFilter.isDestroyed)
      this.customFilter = new Concentrator.Filter.AdvancedPricingGraphFilter({
        portlet: this,
        productID: this.productID
      });

    this.registerCustomFilter(this, this.customFilter);

    return [this.customFilter];
  },

  settingsHandler: function (evt, btn, component) {

  }
});