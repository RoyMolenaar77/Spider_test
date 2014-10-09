Ext.ns('Concentrator.Filter');

Concentrator.Filter.EdiFilter = (function () {

    var filter = Ext.extend(Ext.ButtonGroup, {
        columns: 1,
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
            if (!this.portlet) throw "Portlet must be passsed in to be bound to this filter";
            this.initFilter();

            Concentrator.Filter.EdiFilter.superclass.constructor.call(this, config);
        },

        initFilter: function () {

            var filter = this;

            this.creationTimeMenu = new Ext.menu.Menu({
                items: [
              {
                  text: 'Before',
                  showSeparator: false,
                  menu: new Ext.menu.DateMenu({
                      listeners: {
                          'select': (function (picker, date) {
                              // Reset Filter
                              this.onDate = null;
                              this.beforeDate = null;
                              this.afterdate = null;

                              // Add new Value
                              this.beforeDate = date;                              

                              this.refresh();

                          }).createDelegate(this)
                      }
                  })
              },
              {
                  text: 'After',
                  menu: new Ext.menu.DateMenu({
                      listeners: {
                          'select': (function (picker, date) {
                              // Reste Filter
                              this.onDate = null;
                              this.beforeDate = null;
                              this.afterdate = null;

                              // Add new Value
                              this.afterDate = date;

                              this.refresh();

                          }).createDelegate(this)
                      }
                  })
              },
              '-',
              {
                  text: 'On',
                  menu: new Ext.menu.DateMenu({
                      listeners: {
                          'select': (function (picker, date) {
                              // Reste Filter
                              this.onDate = null;
                              this.beforeDate = null;
                              this.afterdate = null;

                              // Add new Value
                              this.onDate = date;

                              this.refresh();

                          }).createDelegate(this)
                      }
                  })
              }
            ]
            });

            this.items = [
            {
                text: 'Creation time',
                iconCls: 'calander',
                rowspan: '3',
                scale: 'small',
                arrowAlign: 'bottom',
                iconAlign: 'top',
                width: 40,
                menu: this.creationTimeMenu
            }]

        },
        getValue: function () {

        },
        refresh: function () {
            //serialize the filters into a params object
            var params = this.collectFilters();

            Concentrator.Portlets.EdiOrder.superclass.refresh.call(this.portlet, params);
        },

        collectFilters: function () {
            var filters = {};

            // Date filters
            if (this.beforeDate) {
                filters.onDate = null;
                filters.beforeDate = this.beforeDate;
            }

            if (this.afterDate) {
                filters.onDate = null;
                filters.afterDate = this.afterDate;
            }

            if (this.onDate) {
                filters.afterDate = null;
                filters.beforeDate = null;
                filters.onDate = this.onDate;
            }

            return filters;
        }
    });
    return filter;
})();