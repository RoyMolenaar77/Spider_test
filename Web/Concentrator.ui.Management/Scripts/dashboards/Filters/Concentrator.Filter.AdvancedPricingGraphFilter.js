Ext.ns('Concentrator.Filter');

Concentrator.Filter.AdvancedPricingGraphFilter = (function () {

  var filter = Ext.extend(Ext.ButtonGroup, {
    columns: 1,
    title: 'Price graph',
    height: 70,
    portlet: null,
    cls: 'custom-filter',
    defaults: {
      scale: 'small',
      rowspan: '3',
      iconAlign: 'top'
    },
    constructor: function (config) {
      Ext.apply(this, config);
      if (!this.portlet) throw "Portlet must be passed in to be bound to this filter";
      this.initFilter();

      Concentrator.Filter.AdvancedPricingGraphFilter.superclass.constructor.call(this, config);

    },

    initFilter: function () {

      var filter = this;

      this.cpCheckBoxItem = new Ext.menu.CheckItem({
        text: 'Competitor Prices',
        checked: false,
        listeners: {
          'checkchange': (function (f) {
            this.portlet.filterUpdate(this.portlet, filter);
          }).createDelegate(this)
        }
      });

      this.items = [
        {
          iconCls: 'monitor',
          width: 40,
          text: 'Optional Lines',
          menu: new Ext.menu.Menu({
            defaults: { width: 175 },
            items:
      [
         this.cpCheckBoxItem
      ]
          })
        }];

    },
    //    getValue: function () {

    //    },
    refresh: function () {
      //serialize the filters into a params object
      var params = this.collectFilters();

      Concentrator.Portlets.AdvancedPricingGraph.superclass.refresh.call(this.portlet, params);
    },
    getValue: function () {
      return {
        cp: this.cpCheckBoxItem.checked
      };
    }
  });
  return filter;
})();