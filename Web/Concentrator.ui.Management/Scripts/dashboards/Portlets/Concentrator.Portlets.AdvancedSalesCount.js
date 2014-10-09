/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.AdvancedSalesCount = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{
  constructor: function (config) {
    var that = this;
    config.title = 'Sales Count';
    config.loadUrl = Concentrator.route('GetSalesCount', 'Product');
    config.height = 350;

    Ext.apply(this, config);

    Concentrator.Portlets.AdvancedSalesCount.superclass.constructor.call(this, config);
  },
  refresh: function () {    
    var params = this.portal.collectFilters();
    Concentrator.Portlets.AdvancedPricingGraph.superclass.refresh.call(this, params);
  }
});