/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

//TODO: Allow both types of filters to be attached

Concentrator.GridAction = Ext.extend(Concentrator.BaseAction, {
  constructor: function (config) {
    Ext.apply(this, config);

    //get the grid
    this.grid = this.getPanel();
    this.grid.customFilters = [];
    this.filters = [];
    //attach it to the items so that the parent control will not call getpanel again

    this.items = [this.grid];

    if (!this.grid.customFilterCollection) {
      this.grid.customFilterCollection = [];
    }

    this.initGridRenderEvent();
    //params have been passed in: handle the initial load


    if (this.params) this.addFilters(this.params);

    Concentrator.GridAction.superclass.constructor.call(this, config);
  },

  /**
  Overrides the refresh function of the parent control
  */
  refresh: function (params) {

    //reset grid filter collection
    this.grid.clearFilters(true);

    //reset filter collection
    this.filters = [];

    //add the filter params
    this.addFilters(params);
    this.addFiltersToGrid(this.filters);

    this.grid.store.reload();
    //call the parent refresh function to adjust/sync the params 
    Concentrator.GridAction.superclass.refresh.call(this, params);
  },

  /**
  Adds a single filter to collection
  */
  addFilters: function (params) {

    if (Ext.isObject(params)) params = [params] //convert to array

    Ext.each(params, function (p) {
      var c = this.convertFilter(p);
      this.addFilter(c);
    }, this);
  },

  /**
  Converts the filters to a format used internally
  */
  convertFilter: function (filterObj) {
    var f = filterObj,
        filters = [];

    if (!filterObj.key) { //json regular notation
      for (var key in f) {
        filterObj = {};
        filterObj.key = key;
        filterObj.value = f[key];
        filterObj.isGridFilter = f.isGridFilter;
        filters.push(filterObj);
      }
    }

    filters.push(filterObj);

    return filters;
  },

  /**
  Adds a filter/param to this grid interface. Every filter needs to have the following structure
  {
  key : 'some',               ---> the name of the param/dataIndex
  value : 'value',            ---> the value of this filter
  isGridFilter : true/false   ---> (optional, false  by default) whether it is a column in some target grid
  }
  */
  addFilter: function (filterObj) {
    if (Ext.isObject(filterObj)) {
      this.filters.push(filterObj);
    }
    else if (Ext.isArray(filterObj)) {
      Ext.each(filterObj, function (f) {
        this.filters.push(f);
      }, this);
    }
  },

  /**
  Attaches the gridfilters to the grid object
  */
  addFiltersToGrid: function (filters) {
    if (!this.grid.customFilterCollection) this.grid.customFilterCollection = [];
    var params = {};

    Ext.each(filters, function (f) {
      var filter = this.grid.filters.filters.find(
                function (i) {
                  if (i.dataIndex == f.key) return true;
                });
      if (filter) {
        filter.setValue(f.value);
        filter.setActive(true);
      } else {
        this.grid.customFilterCollection.push(f);
        params[f.key] = f.value;
      }

    }, this);

    Ext.apply(this.grid.store.baseParams, params);
  },

  /**
  Attaches the render listener on the grid object
  */
  initGridRenderEvent: function () {
    this.grid.on('afterrender', function () {
      var filters = this.filters,
        store = this.grid.store,
        gridFilters = [],
        customFilters = [];

      //check for the presence of grid filters
      Ext.each(filters, function (f) {
        if (f.isGridFilter) {
          gridFilters.push(f);
        } else {
          customFilters.push(f);
        }
      }, this);

      if (gridFilters.length > 0)
        this.addFiltersToGrid(gridFilters);

      if (customFilters.length > 0) {
        this.grid.customFilterCollection = customFilters;
      }

      if (customFilters.length > 0 || gridFilters.length > 0) {
        if (this.grid.onGridFilterInitialized) this.grid.onGridFilterInitialized();
      }

      //check for the presence of custom filters
      if (customFilters.length > 0) {
        var params = {};
        //attach to store params
        Ext.each(customFilters, function (filter) {
          //attach custom filters to load params of the store
          params[filter.key] = filter.value;
        });
        Ext.apply(store.baseParams, params);
      }
      if (gridFilters.length == 0) //load the store because it will not be loaded otherwise
        store.load();


    }, this);
  }
});