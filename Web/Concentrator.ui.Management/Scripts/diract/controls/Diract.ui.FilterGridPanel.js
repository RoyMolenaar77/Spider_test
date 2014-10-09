/**
* @class Diract.ui.FilterGridPanel
* @extends Ext.Panel
*
* Built on Ext JS Library 3.0.3
* 
* @param {Object} config The configuration options
* 
* @author Coen van der Wel, Diract IT <c.wel@diract-it.nl>
* @copyright Diract IT 2010-2011
*/

Diract.ui.FilterGridPanel = Ext.extend(Ext.Panel, {

    /**
    * Defaults.
    * All params will be applied to the Diract.ui.Grid, except filterItems, panelConfig, region and filterPanelConfig!
    * You can also pass a grid config, this will prevent a new Diract.ui.Grid from being created.
    */

    /**
    * @cfg {Object} filterItems The items to be used as filter items.
    *
    * If a Field in filterItems specifies a forceDynamicPageSize property which validates as true, then
    * submitting this field with a value will cause all results to be fit in one page.
    */
    filterItems: [],

    /**
    * @cfg {Object} panelConfig The config to apply to the main panel.
    *
    * panelConfig.items will be overridden.
    */
    panelConfig: {},

    /**
    * @cfg {Object} filterPanelConfig The config to apply to the filter panel.
    *
    * filterPanelConfig.items will be overridden. Use filterItems to specify those.
    */
    filterPanelConfig: {},

    /**
    * @cfg {Bool} autoLoadStore Automatically load store on render. Defaults to True.
    *
    * Setting this to False will prevent the Store from loading when the Grid rendered.
    */
    autoLoadStore: true,

    /**
    * Functions.
    */

    // private
    clearFilters: function (panel) {
        // for each field
        for (var i = 0; i < panel.items.length; i++) {
            // get the field
            var item = panel.items.items[i];
            // if it's a parent, recurse
            if (item.items) this.clearFilters(item);
            // not a parent, simply clear
            else panel.items.items[i].setValue(panel.items.items[i].defaultValue);
        }
    },

    // private
    getFieldValues: function (panel) {
        // initialise
        var params = {};
        // for each field
        for (var i = 0; i < panel.items.length; i++) {
            // get the field
            var item = panel.items.items[i];
            // if it's a parent, recurse
            if (item.items) Ext.apply(params, this.getFieldValues(item));
            // not a parent, simply add
            else {
                // get name-value pair data
                var name = panel.items.items[i].hiddenName || panel.items.items[i].name;
                var value = panel.items.items[i].getValue();
                // but prevent adding null values
                if (!Ext.isEmpty(value, false)) {
                    if (value == "") continue; // bugfix
                    if (panel.items.items[i].forceDynamicPageSize) this.grid.setPageSize(999999);
                    params[name] = value;
                }
            }
        }
        // return
        return params;
    },

    /**
    * Filter the specified grid with the current values of the specified fields.
    *
    * @return void
    */
    filterGrid: function () {
        // if there was forced paging earlier, revert
        this.grid.resetPageSize();
        // apply as base params
        this.grid.store.baseParams = Ext.applyIf(this.getFieldValues(this.filterPanel), this.grid.params);
        // and load grid's store
        this.grid.store.load({
            params: { start: 0, limit: this.grid.pageSize }
        });
    },

    addFilterListener: function (item, type) {
        var self = this;
        if (type != 'callback') {
            if (!item.hasListener(type)) {
                item.on(type, function () {
                    self.filterGrid();
                });
            }
        } else {
            if (!item.callback) {
                item.callback = function () {
                    self.filterGrid();
                };
            }
        }
    },

    addClearListener: function (item) {
        this.addFilterListener(item, 'clear');
    },

    addSelectListener: function (item) {
        this.addFilterListener(item, 'select');
    },

    addCheckListener: function (item) {
        this.addFilterListener(item, 'check');
    },

    addCallbackListener: function (item) {
        this.addFilterListener(item, 'callback');
    },

    addDeselectListener: function (item) {
        this.addFilterListener(item, 'deselect');
    },

    constructor: function (config) {

        /**
        * Process config.
        */

        // base applies
        this.panelConfig = config.panelConfig;
        this.filterPanelConfig = config.filterPanelConfig;
        this.filterItems = config.filterItems;
        this.region = config.region;
        if (config.autoLoadStore !== undefined) this.autoLoadStore = config.autoLoadStore;

        // and cleanup
        delete config.panelConfig;
        delete config.filterPanelConfig;
        delete config.filterItems;
        delete config.region;
        // process columns config
        var columns = [], column = 0;
        // for each item
        for (var i = 0; i < this.filterItems.length; i++) {
            var filter = this.filterItems[i];
            // if the item is "-", move to next column
            if (filter == "-") column++;
            // else, if it's a normal item
            else {
                // add ENTER key handler
                if (!filter.hasListener("specialkey")) filter.on("specialkey", function (field, ev) {
                    if (ev.getKey() == ev.ENTER) {
                        ev.preventDefault();
                        this.filterGrid();
                    }
                }, this);

                if (filter instanceof Diract.ui.DateField) {
                    this.addClearListener(filter);
                    this.addSelectListener(filter);
                }
                //        } else if (filter instanceof Diract.ui.Select) {
                //          this.addSelectListener(filter);
                //          this.addDeselectListener(filter);
                //          this.addClearListener(filter);
                //        } else if (filter instanceof Ext.form.Checkbox) {
                //          this.addCheckListener(filter);
                //        } else if (filter instanceof Ext.form.TwinTriggerField) {
                //          this.addCallbackListener(filter);
                //          this.addClearListener(filter);
                //        }

                // set consistent width
                if (!filter.width) filter.anchor = "90%";
                // save default value
                filter.defaultValue = filter.getValue();
                if (Ext.isEmpty(filter.defaultValue)) filter.defaultValue = undefined;
                // create column panel if needed (with item as child)
                if (!columns[column]) columns[column] = new Ext.FormPanel({ items: [filter] });
                // or append the item as child
                else columns[column].items.add(filter);
            } // eo else
        } // eo for i

        /**
        * Create filter panel.
        */

        // process filter panel config
        this.filterPanelConfig = Ext.apply({
            title: 'Filter', //$('FilterGridPanel.PanelTitle'),
            defaults: {},
            autoHeight: true,
            region: "north",
            collapsible: true,
            layout: "column",
            bodyStyle: "padding: 10px; border-width: 0 0 1px 0;",
            buttons: [
                new Ext.Button({
                    text: 'Reset', //$('FilterGridPanel.ResetButton'),
                    pressed: true,
                    scope: this,
                    handler: function () {
                        this.clearFilters(this.filterPanel);
                        this.filterGrid();
                    }
                }),
                new Ext.Button({
                    text: 'Search', //$('FilterGridPanel.SearchButton'),
                    scope: this,
                    handler: this.filterGrid
                })
            ]
        }, this.filterPanelConfig);

        // enforce filter panel config
        Ext.apply(this.filterPanelConfig, {
            // apply default config, these are not forced
            defaults: Ext.apply({
                columnWidth: 1 / columns.length,
                labelWidth: 150,
                border: false
            }, this.filterPanelConfig.defaults),
            items: columns,
            columns: columns.length
        });

        // create filter panel first (0)
        this.filterPanel = new Ext.Panel(this.filterPanelConfig);

        /**
        * Create grid.
        */

        // allow for direct grid override
        if (config.grid) this.grid = config.grid;
        else {
            // process grid config
            config = Ext.apply(config, {
                autoLoadStore: false,
                region: "center"
            });

            // then create the grid (1)
            if (config.excelPlease) {
                this.grid = new Diract.ui.ExcelGrid(config);
            } else {
                this.grid = new Diract.ui.Grid(config);
            }
        }

        /**
        * Create main panel.
        */

        // process panel config
        this.panelConfig = Ext.apply({
            defaults: {},
            layout: "border"
        }, this.panelConfig);

        // enforce panel config
        Ext.apply(this.panelConfig, {
            // apply default config, these are not forced
            defaults: Ext.apply({
                border: false
            }, this.panelConfig.defaults),
            items: [
                this.filterPanel,
                this.grid
            ]
        });

        // core to this element; the actual panel constructing
        Diract.ui.FilterGridPanel.superclass.constructor.call(this, this.panelConfig);

        /**
        * Post processing.
        */

        // and apply base params initially
        if (this.autoLoadStore) {
            this.filterGrid();
        }

    } // eo constructor

});                                // eo Ext.extend

// adapt prototype to display title in collapsed panels
Ext.layout.BorderLayout.Region.prototype.getCollapsedEl = Ext.layout.BorderLayout.Region.prototype.getCollapsedEl.createSequence(function () {
    if (!this.collapsedEl.titleEl) {
        this.collapsedEl.titleEl = this.collapsedEl.createChild({ cls: 'x-collapsed-title', cn: this.panel.title });
    }
}); 