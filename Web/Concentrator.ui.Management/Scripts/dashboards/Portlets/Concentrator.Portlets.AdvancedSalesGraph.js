/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.AdvancedSalesGraph = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    config.title = 'Advanced sales graph';
    config.loadUrl = Concentrator.route('GetSalesProgression', 'Product');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.AdvancedSalesGraph.superclass.constructor.call(this, config);
  },
  refresh: function () {
    var params = this.portal.collectFilters();
    Concentrator.Portlets.AdvancedPricingGraph.superclass.refresh.call(this, params);
  }
});