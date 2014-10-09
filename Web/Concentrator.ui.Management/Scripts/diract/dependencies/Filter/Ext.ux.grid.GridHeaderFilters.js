Ext.namespace("Ext.ux.grid");

/**
* @class Ext.ux.grid.GridHeaderFilters
* @extends Ext.util.Observable
* 
* Plugin that enables filters in columns headers.
* 
* To add a grid header filter, put the "filter" attribute in column configuration of the grid column model.
* This attribute is the configuration of the Ext.form.Field to use as filter in the header or an array of fields configurations.<br>
* <br>
* The filter configuration object can include some special attributes to manage filter configuration:
* <ul>
* <li><code>filterName</code>: to specify the name of the filter and the corresponding HTTP parameter used to send filter value to server. 
* If not specified column "dataIndex" attribute will be used, if more than one filter is configured for the same column, the filterName will be the "dataIndex" followed by filter index (if index &gt; 0)</li>
* <li><code>value</code>: to specify default value for filter. If no value is provided for filter (in <code>filters</code> plugin configuration parameter or from loaded status), 
* this value will be used as default filter value</li>
* <li><code>filterEncoder</code>: a function used to convert filter value returned by filter field "getValue" method to a string. Useful if the filter field getValue() method
* returns an object that is not a string</li>
* <li><code>filterDecoder</code>: a function used to convert a string to a valid filter field value. Useful if the filter field setValue(obj) method
*                         needs an object that is not a string</li>
* <li><code>applyFilterEvent</code></li>: a string that specifies the event that starts filter application for this filter field. If not specified, the "applyMode" is used. (since 1.0.10)</li>
*    </ul>
* <br>
* Filter fields are rendered in the header cells within an <code>Ext.Panel</code> with <code>layout='form'</code>.<br>
* For each filter you can specify <code>fieldLabel</code> or other values supported by this layout type.<br>
* You can also override panel configuration using <code>containerConfig</code> attribute.<br>
* <br>
* This plugin enables some new grid methods:
* <ul>
* <li>getHeaderFilter(name)</li>
* <li>getHeaderFilterField(name)</li> 
* <li>setHeaderFilter(name, value)</li> 
* <li>setHeaderFilters(object, [bReset], [bReload])</li>
* <li>resetHeaderFilters([bReload])</li>
* <li>applyHeaderFilters([bReload])</li>
* </ul>
* The "name" is the filterName (see filterName in each filter configuration)
* 
* @author Damiano Zucconi - http://www.isipc.it
* @version 2.0.6 - 03/03/2011
*/
Ext.ux.grid.GridHeaderFilters = function (cfg) { if (cfg) Ext.apply(this, cfg); };

Ext.extend(Ext.ux.grid.GridHeaderFilters, Ext.util.Observable,
{
  filterData: [],
  paramPrefix: 'filter',
  /**
  * @cfg {Number} fieldHeight
  * Height for each filter field used by <code>autoHeight</code>.
  */
  fieldHeight: 22,

  /**
  * @cfg {Number} padding
  * Padding for filter fields. Default: 2
  */
  fieldPadding: 1,

  /**
  * @cfg {Boolean} highlightOnFilter
  * Enable grid header highlight if active filters 
  */
  highlightOnFilter: true,

  /**
  * @cfg {String} highlightColor
  * Color for highlighted grid header
  */
  highlightColor: 'yellow',

  /**
  * @cfg {String} highlightCls
  * Class to apply to filter header when filters are highlighted. If specified overrides highlightColor.
  * See <code>highlightOnFilter</code>. 
  */
  highlightCls: null,

  /**
  * @cfg {Boolean} stateful
  * Enable or disable filters save and restore through enabled Ext.state.Provider
  */
  stateful: true,

  /**
  * @cfg {String} applyMode
  * Sets how filters are applied. If equals to "auto" (default) the filter is applyed when filter field value changes (change, select, ENTER).
  * If set to "enter" the filters are applied only when user push "ENTER" on filter field.<br> 
  * See also <code>applyFilterEvent</code> in columnmodel filter configuration: if this option is specified in
  * filter configuration, <code>applyMode</code> value will be ignored and filter will be applied on specified event.
  * @since Ext.ux.grid.GridHeaderFilters 1.0.6
  */
  applyMode: "auto",

  /**
  * @cfg {Object} filters
  * Initial values for filters (mapped with filters names). If this object is defined,
  * its attributes values overrides the corresponding filter values loaded from grid status or <code>value</code> specified in column model filter configuration.<br>
  * Values specified into column model configuration (filter <code>value</code> attribute) are ignored if this object is specified.<br>
  * See <code>filtersInitMode</code> to understand how these values are mixed with values loaded from grid status.
  * @since Ext.ux.grid.GridHeaderFilters 1.0.9
  */
  filters: null,

  /**
  * @cfg {String} filtersInitMode
  * If <code>filters</code> config value is specified, this parameter defines how these values are used:
  * <ul>
  * <li><code>replace</code>: these values replace all values loaded from grid status (status is completely ignored)</li>
  * <li><code>merge</code>: these values overrides values loaded from status with the same name. Other status values are keeped and used to init filters.</li>
  * </ul>
  * This parameter doesn't affect how filter <code>value</code> attribute is managed: it will be always ignored if <code>filters</code> object is specified.<br>
  * Default = 'replace'
  */
  filtersInitMode: 'replace',

  /**
  * @cfg {Boolean} ensureFilteredVisible
  * If true, forces hidden columns to be made visible if relative filter is set. Default = true.
  */
  ensureFilteredVisible: true,

  cfgFilterInit: false,

  /**
  * @cfg {Object} containerConfig
  * Base configuration for filters container of each column. With this attribute you can override filters <code>Ext.Container</code> configuration.
  */
  containerConfig: null,

  /**
  * @cfg {Number} labelWidth
  * Label width for filter containers Form layout. Default = 50.
  */
  labelWidth: 50,

  fcc: null,

  filterFields: null,

  filterContainers: null,

  refreshHeader: false,

  filterContainerCls: 'x-ghf-filter-container',

  init: function (grid) {
    this.grid = grid;
    Ext.QuickTips.init();
    var gv = this.grid.getView();
    gv.onDataChange = function () {
      this.refresh(this.refreshHeader);
      this.updateHeaderSortState();
      this.syncFocusEl(0);

    };
    gv.updateHeaders = gv.updateHeaders.createSequence(function () {
      this.renderFilters.call(this);
    }, this).createInterceptor(function () {
      //this.destroyFilters.call(this);
      return true;
    }, this);
    this.grid.on({
      scope: this,
      render: this.onRender,
      resize: this.onResize,
      columnresize: this.onResize,
      reconfigure: this.onReconfigure,
      beforedestroy: this.destroyFilters
    });
    this.grid.store.on('beforeload', this.onBeforeLoad, this);

    if (this.stateful) {
      this.grid.on("beforestatesave", this.saveFilters, this);
      this.grid.on("beforestaterestore", this.loadFilters, this);
    }

    //Column hide event managed
    this.grid.getColumnModel().on("hiddenchange", this.onColHidden, this);

    this.grid.addEvents(
    /**
    * @event filterupdate
    * <b>Event enabled on the GridPanel</b>: fired when a filter is updated
    * @param {String} name Filter name
    * @param {Object} value Filter value
    * @param {Ext.form.Field} el Filter field
    */
        'filterupdate');

    this.addEvents(
    /**
    * @event render
    * Fired when filters render on grid header is completed
    * @param {Ext.ux.grid.GridHeaderFilters} this
    */
            { 'render': true }
        );

    //Must ignore filter config value ?
    this.cfgFilterInit = Ext.isDefined(this.filters) && this.filters !== null;
    if (!this.filters)
      this.filters = {};

    //Configuring filters
    this.configure(this.grid.getColumnModel());

    Ext.ux.grid.GridHeaderFilters.superclass.constructor.call(this);

    if (this.stateful) {
      if (!Ext.isArray(this.grid.stateEvents))
        this.grid.stateEvents = [];
      this.grid.stateEvents.push('filterupdate');
    }

    //Enable new grid methods
    Ext.apply(this.grid, {
      headerFilters: this,
      getHeaderFilter: function (sName) {
        if (!this.headerFilters)
          return null;
        return this.headerFilters.filters[sName];
      },
      setHeaderFilter: function (sName, sValue) {
        if (!this.headerFilters)
          return;
        var fd = {};
        fd[sName] = sValue;
        this.setHeaderFilters(fd);
      },
      setHeaderFilters: function (obj, bReset, bReload) {
        if (!this.headerFilters)
          return;
        if (bReset)
          this.resetHeaderFilters(false);
        if (arguments.length < 3)
          var bReload = true;
        var bOne = false;
        for (var fn in obj) {
          if (this.headerFilters.filterFields[fn]) {
            var el = this.headerFilters.filterFields[fn];
            this.headerFilters.setFieldValue(el, obj[fn]);
            this.headerFilters.applyFilter(el, false);
            bOne = true;
          }
        }
        if (bOne && bReload)
          this.headerFilters.storeReload();
      },
      getHeaderFilterField: function (fn) {
        if (!this.headerFilters)
          return;
        if (this.headerFilters.filterFields[fn])
          return this.headerFilters.filterFields[fn];
        else
          return null;
      },
      resetHeaderFilters: function (bReload) {
        if (!this.headerFilters)
          return;
        if (arguments.length == 0)
          var bReload = true;
        for (var fn in this.headerFilters.filterFields) {
          var el = this.headerFilters.filterFields[fn];
          if (Ext.isFunction(el.clearValue)) {
            el.clearValue();
          }
          else {
            this.headerFilters.setFieldValue(el, '');
          }
          this.headerFilters.applyFilter(el, false);
        }
        if (bReload)
          alert
        this.headerFilters.storeReload();
      },
      applyHeaderFilters: function (bReload) {
        if (arguments.length == 0)
          var bReload = true;
        this.headerFilters.applyFilters(bReload);
      }
    });

  },

  /**
  * @private
  * Configures filters and containers starting from grid ColumnModel
  * @param {Ext.grid.ColumnModel} cm The column model to use
  */
  configure: function (cm) {

    /*Filters config*/
    var filteredColumns = cm.config;

    /*Building filters containers configs*/
    this.fcc = {};
    for (var i = 0; i < filteredColumns.length; i++) {
      var co = filteredColumns[i];
      var fca = this.buildFilterConfig(co);
      if (!Ext.isArray(fca))
        fca = [fca];
      for (var ci = 0; ci < fca.length; ci++) {
        var fc = Ext.apply({
          filterName: ci > 0 ? co.dataIndex + ci : co.dataIndex
        }, fca[ci]);
        Ext.apply(fc, {
          columnId: co.id,
          dataIndex: co.dataIndex,
          hideLabel: Ext.isEmpty(fc.fieldLabel),
          anchor: '100%',
          type: fca[0].type
        });

        if (!this.cfgFilterInit && !Ext.isEmpty(fc.value)) {
          this.filters[fc.filterName] = Ext.isFunction(fc.filterEncoder) ? fc.filterEncoder.call(this, fc.value) : fc.value;
        }
        delete fc.value;

        /*
        * Se la configurazione del field di filtro specifica l'attributo applyFilterEvent, il filtro verrà applicato
        * in corrispondenza di quest'evento specifico
        */
        if (fc.applyFilterEvent) {
          fc.listeners = { scope: this };
          fc.listeners[fc.applyFilterEvent] = function (field) { this.applyFilter(field); };
          delete fc.applyFilterEvent;
        }
        else {
          var tempListeners = fc.listeners;
          //applyMode: auto o enter
          if (this.applyMode === 'auto' || this.applyMode === 'change' || Ext.isEmpty(this.applyMode)) {
            //Legacy mode and deprecated. Use applyMode = "enter" or applyFilterEvent
            fc.listeners =
                        {
                          change: function (field) {
                            var t = field.getXType();
                            if (t == 'combo' || t == 'lovcombo' || t == 'datefield') { //avoid refresh twice for combo select 
                              return;
                            } else {
                              this.applyFilter(field, null);
                              if (field.focus) field.focus();
                            }
                          },
                          specialkey: function (el, ev) {
                            ev.stopPropagation();
                            if (ev.getKey() == ev.ENTER)
                              el.el.dom.blur();
                          },
                          select: function (field, rec, idx) {
                            this.applyFilter(field, null, idx);
                          },
                          scope: this
                        };
          }
          else if (this.applyMode === 'enter') {
            fc.listeners =
                        {
                          specialkey: function (el, ev) {
                            ev.stopPropagation();
                            if (ev.getKey() == ev.ENTER) {
                              this.applyFilters();
                            }
                          },
                          scope: this
                        };
          }
          Ext.apply(fc.listeners, tempListeners);
        }

        //Looking for filter column index
        var containerCfg = this.fcc[fc.columnId];
        if (!containerCfg) {
          containerCfg = {
            cls: this.filterContainerCls,
            border: false,
            bodyBorder: false,
            labelSeparator: '',
            labelWidth: this.labelWidth,
            layout: 'form',
            style: {},
            items: []
          };
          if (this.containerConfig)
            Ext.apply(containerCfg, this.containerConfig);
          this.fcc[fc.columnId] = containerCfg;
        }
        containerCfg.items.push(fc);
      }
    }
  },

  onBeforeLoad: function (store, options) {
    options.params = options.params || {};
    var params = Ext.apply(this.grid.store.baseParams, options.params);
    var clean = this.cleanParams(params);
    var filters = this.getFilterData();

    var params = this.buildQuery(filters);

    if (filters.length == 0) {
      params = clean;
    }
    this.grid.store.baseParams = {}; //clear actual params
    Ext.apply(this.grid.store.baseParams, Ext.apply(params, clean));
  },

  getFilterData: function () {

    //apply all filters
    var filters = [];
    for (var fn in this.filterFields) {
      var filter = this.filterFields[fn];
      if (filter.getValue() == null) continue;

      switch (filter.type) {
        case 'string':
          var filterData = {
            data: {
              type: filter.type,
              value: filter.getValue()
            }, field: filter.filterField || filter.dataIndex
          };

          if (filter.getValue() != "") {
            filters.push(filterData);
          }
          break;
        case 'boolean':
          var filterData = {
            data: {
              type: filter.type,
              value: filter.getValue()
            }, field: filter.filterField || filter.dataIndex
          };

          var val = filter.getValue();

          if (val !== null && val != "" || val === false) {
            filters.push(filterData);
          } else {
            filter.setValue(null);
          }
          break;
        case 'numeric':

          var val = filter.getValue();
          if (filter.isValid() && val != "") {
            var vals = val.match(/([\>\<]\d+){1}/g);

            if (vals && vals.length > 0) {//its an expression
              for (var i = 0; i < vals.length; i++) {
                var filterData = {
                  data: {
                    type: filter.type
                  }, field: filter.dataIndex
                };

                var match = vals[i];
                var comparisonChar = match[0];
                var comparison = comparisonChar == '>' ? "gt" : "lt";
                filterData.data.comparison = comparison;
                filterData.data.value = match.substring(1);
                filters.push(filterData);
              }
            } else {//its just a number
              var filterData = {
                data: {
                  type: filter.type
                }, field: filter.filterField || filter.dataIndex
              };
              filterData.data.comparison = "eq";
              filterData.data.value = val;
              filters.push(filterData);
            }
          }

          break;
        case 'date':
          var vals = filter.getValue();
          if (filter.getValue() != "") {
            for (var v = 0; v < vals.length; v++) {
              var val = vals[v];
              if (val.value != "") {
                var filterData = {
                  data: {
                    type: filter.type
                  }, field: filter.filterField || filter.dataIndex
                };
                filterData.data.comparison = val.comparison;
                filterData.data.value = val.value;
                filters.push(filterData);
              }
            }
          }
          break;
        case 'list':
          if (filter.getValue() != "") {
            var filterData = {
              data: {
                type: filter.type,
                value: filter.getValue().split(',')
              }, field: filter.filterField || filter.dataIndex
            };
            filters.push(filterData);
          }
          break;
      }
    }
    return filters;

  },

  cleanParams: function (p) {
    // if encoding just delete the property
    if (this.encode) {
      delete p[this.paramPrefix];
      // otherwise scrub the object of filter data
    } else {
      var regex, key;
      regex = new RegExp('^' + this.paramPrefix + '\[[0-9]+\]');
      for (key in p) {
        if (regex.test(key)) {
          delete p[key];
        }
      }
    }
    return p;
  },

  buildFilterConfig: function (column) {


    var f = column.filter;
    if (f != null) {
      var xtype = '';
      if (!f.xtype) {
        if (f.type) {

          switch (f.type) {
            case 'list':
              //there must be a store/there must be a labelfield
              if (!f.store || !f.labelField) {
                f.xtype = "combo";
                f.triggerAction = "all";

                f.editable = false;
                break;
              }
              if (!f.store || f.store.data) {
                f.mode = "local";
              }
              f.xtype = "lovcombo";
              f.triggerAction = "all";
              
              f.editable = false;
              var valueField = '';
              for (var idx = 0; idx < f.store.fields.keys.length; idx++) {
                var field = f.store.fields.keys[idx];
                if (field != f.labelField) {
                  //this is the ID
                  valueField = field;
                  break;
                }
              }
              f.valueField = valueField;
              f.displayField = f.labelField;
              break;
            case 'auto':
            case 'string':
              f.xtype = 'textfield';
              f.type = 'string';
              break;
            case 'int':
            case 'float':
              f.xtype = 'numberfield';
              break;
          }
        } else {
          f.xtype = 'textfield';
        }
      }
    } else {
      switch (column.colType) {
        case 'string':
        case 'auto':
          f = { xtype: 'textfield', type: 'string' };
          break;
        case 'int':
        case 'float':
          var validRegex = /^([\>\<]\d+){1,2}$/;
          //validate input
          f = {
            validator: function (value) {
              if (isNaN(value)) {
                return validRegex.test(value);
              }
              return true;
            },
            xtype: 'textfield',
            type: 'numeric',
            qtip: 'Valid input: <br> > + number <br>  < + number <br> or a number',
            invalidText: 'Valid input: >, <, number',
            listeners: {
              afterrender: function (c) {
                Ext.QuickTips.register({
                  target: c.getEl(),
                  text: c.qtip
                });
              }
            },
            maskRe: /[< > = 0-9]/
          };
          break;
        case 'bool':
        case 'boolean':
          f = {
            xtype: 'combo',
            editable: false,
            value: null,
            mode: 'local',
            triggerAction: 'all',
            type: 'boolean',
            store: [["", "None"], ["true", "Yes"], ["false", "No"]]
          }; //default
          break;
        case 'date':
          f = { xtype: 'dualDateField', type: 'date' }; //default
          break;
        default:
          f = { xtype: 'textfield', type: 'string' }; //default
          break;
      }

    }
    if (column.filterable === false) {
      //set disabled
      f.disabled = true;
    }

    return f;
  },

  renderFilterContainer: function (columnId, fcc) {
    if (!this.filterContainers)
      this.filterContainers = {};
    //Associated column index
    var ci = this.grid.getColumnModel().getIndexById(columnId);
    //Header TD
    var td = this.grid.getView().getHeaderCell(ci);
    td = Ext.get(td);
    //Patch for field text selection on Mozilla
    if (Ext.isGecko)
      td.dom.style.MozUserSelect = "text";
    td.dom.style.verticalAlign = 'top';
    //Render filter container
    fcc.width = td.getWidth() - 3;
    var fc = new Ext.Container(fcc);
    fc.render(td);
    //Container cache
    this.filterContainers[columnId] = fc;
    //Fields cache    
    var height = 0;
    if (!this.filterFields)
      this.filterFields = {};
    var fields = fc.findBy(function (cmp) { return !Ext.isEmpty(cmp.filterName); });
    if (!Ext.isEmpty(fields)) {
      for (var i = 0; i < fields.length; i++) {
        var filterName = fields[i].filterName;
        this.filterFields[filterName] = fields[i];
        height += fields[i].getHeight();
      }
    }

    return fc;
  },

  renderFilters: function () {
    if (!this.fcc)
      return;
    for (var cid in this.fcc) {
      this.renderFilterContainer(cid, this.fcc[cid]);
    }
    this.setFilters(this.filters);
    this.highlightFilters(this.isFiltered());
  },

  onRender: function () {
    this.renderFilters();
    if (this.isFiltered()) {
      this.applyFilters(false);
    }
    this.fireEvent("render", this);
  },

  getFilterField: function (filterName) {
    return this.filterFields ? this.filterFields[filterName] : null;
  },

  /**
  * Sets filter values by values specified into fo.
  * @param {Object} fo Object with attributes filterName = value
  * @param {Boolean} clear If current values must be cleared. Default = false
  */
  setFilters: function (fo, clear) {
    this.filters = fo;
    if (this.filters && this.filterFields) {
      //Delete filters that doesn't match with any field
      for (var fn in this.filters) {
        if (!this.filterFields[fn])
          delete this.filters[fn];
      }

      for (var fn in this.filterFields) {
        var field = this.filterFields[fn];
        var value = this.filters[field.filterName];
        if (Ext.isEmpty(value)) {
          if (clear)
            this.setFieldValue(field, '');
        }
        else {
          if (field.getValue && !(field.getValue() == value)) {
            this.setFieldValue(field, value);
          }
        }
      }
    }
  },

  onColResize: function (index, iWidth) {
    if (!this.filterContainers)
      return;
    var colId = this.grid.getColumnModel().getColumnId(index);
    var cnt = this.filterContainers[colId];
    if (cnt) {
      if (isNaN(iWidth))
        iWidth = 0;
      var filterW = (iWidth < 3) ? 0 : (iWidth - 3);
      cnt.setWidth(filterW);
      //Thanks to ob1
      cnt.doLayout(false, true);
    }
  },

  /**
  * @private
  * Resize filters containers on grid resize
  * Thanks to dolittle
  */
  onResize: function () {
    var n = this.grid.getColumnModel().getColumnCount();
    for (var i = 0; i < n; i++) {
      var td = this.grid.getView().getHeaderCell(i);
      td = Ext.get(td);
      this.onColResize(i, td.getWidth());
    }
  },

  resizeAll: function () {
    this.onResize(); //Recalculate the widths of all columns
  },

  onColHidden: function (cm, index, bHidden) {
    var cw = this.grid.getColumnModel().getColumnWidth(index);
    this.onColResize(index, cw);
    this.grid.getView().updateColumnHidden(index, bHidden);
    this.resizeAll();
  },

  onReconfigure: function (grid, store, cm) {
    this.destroyFilters();
    this.configure(cm);
    this.renderFilters();
  },

  saveFilters: function (grid, status) {
    var vals = {};
    for (var name in this.filters) {
      vals[name] = this.filters[name];
    }
    status["gridHeaderFilters"] = vals;
    return true;
  },

  loadFilters: function (grid, status) {
    var vals = status.gridHeaderFilters;
    if (vals) {
      if (this.cfgFilterInit) {
        if (this.filtersInitMode === 'merge')
          Ext.apply(vals, this.filters);
      }
      else
        this.filters = vals;
    }
  },

  isFiltered: function () {
    for (var k in this.filters) {
      if (!Ext.isEmpty(this.filters[k]))
        return true;
    }
    return false;
  },

  highlightFilters: function (enable) {
    if (!this.highlightOnFilter)
      return;
    if (!this.filterContainers)
      return;
    if (!this.grid.getView().mainHd)
      return;

    var tr = this.grid.getView().mainHd.child('.x-grid3-hd-row');
    if (!Ext.isEmpty(this.highlightCls)) {
      if (enable)
        tr.addClass(this.highlightCls);
      else
        tr.removeClass(this.highlightCls);
    }
    else {
      tr.setStyle('background-color', enable ? this.highlightColor : '');
    }
  },

  getFieldValue: function (eField) {
    if (Ext.isFunction(eField.filterEncoder))
      return eField.filterEncoder.call(eField, eField.getValue());
    else
      return eField.getValue();
  },

  setFieldValue: function (eField, value) {
    if (Ext.isFunction(eField.filterDecoder))
      value = eField.filterDecoder.call(eField, value);
    eField.setValue(value);
  },

  applyFilter: function (el, bLoad, idx) {
    if (bLoad === null)
      bLoad = true;
    if (!el)
      return;

    if (!el.isValid())
      return;

    if (el.disabled && !Ext.isDefined(this.grid.store.baseParams[el.filterName]))
      return;

    var sValue = this.getFieldValue(el);

    if (el.disabled || Ext.isEmpty(sValue)) {
      delete this.grid.store.baseParams[el.filterName];
      delete this.filters[el.filterName];
    }
    else {
      this.filters[el.filterName] = sValue;
    }
    if (bLoad)
      this.grid.store.load();
  },

  buildQuery: function (filters) {
    var p = {}, i, f, root, dataPrefix, key, tmp,
            len = filters.length;
    if (!this.encode) {
      for (i = 0; i < len; i++) {
        f = filters[i];
        root = [this.paramPrefix, '[', i, ']'].join('');
        p[root + '[field]'] = f.field;

        dataPrefix = root + '[data]';
        for (key in f.data) {
          p[[dataPrefix, '[', key, ']'].join('')] = f.data[key];
        }
      }
    } else {
      tmp = [];
      for (i = 0; i < len; i++) {
        f = filters[i];
        tmp.push(Ext.apply(
                    {},
                    { field: f.field },
                    f.data
                ));
      }
      // only build if there is active filter 
      if (tmp.length > 0) {
        p[this.paramPrefix] = Ext.util.JSON.encode(tmp);
      }
    }

    return p;
  },

  applyFilters: function (bLoad) {
    if (arguments.length < 1)
      bLoad = true;
    for (var fn in this.filterFields) {

      this.applyFilter(this.filterFields[fn], false);
    }
    if (bLoad)
      this.storeReload();
  },

  storeReload: function () {
    this.grid.store.load();
  },

  getFilterContainer: function (columnId) {
    return this.filterContainers ? this.filterContainers[columnId] : null;
  },

  destroyFilters: function () {
    if (this.filterFields) {
      for (var ff in this.filterFields) {
        Ext.destroy(this.filterFields[ff]);
        delete this.filterFields[ff];
      }
    }

    if (this.filterContainers) {
      for (var ff in this.filterContainers) {
        Ext.destroy(this.filterContainers[ff]);
        delete this.filterContainers[ff];
      }
    }
    Ext.destroy(this);
  }
});


Ext.ux.DualDateField = Ext.extend(Ext.form.FormPanel, {
  initComponent: function () {

    var items = [{
      fieldLabel: 'From:',
      xtype: 'datefield',
      startDay: 1,
      showWeekNumber: true,
      anchor: '95%',
      listeners: {
        change: this.onChange.createDelegate(this)
      }
    }, {
      fieldLabel: 'To:',
      xtype: 'datefield',
      showWeekNumber: true,
      startDay: 1,
      anchor: '95%',
      listeners: {
        change: this.onChange.createDelegate(this)
      }
    }];
    this.bodyStyle = ' background: none; border:none; padding-left: 5px;';
    this.labelWidth = 50;
    this.items = items;
    this.isvalid = true;
    Ext.ux.DualDateField.superclass.initComponent.call(this);
  },
  isValid: function () {
    return this.isvalid;
  },
  getValue: function () {
    var vals = [];
    var comps = this.items.keys;
    for (var i = 0; i < comps.length; i++) {
      var comp = this.items.get(i);
      if (comp.getValue() != "") {
        vals.push({ value: comp.getValue(), comparison: comp.fieldLabel == "From:" ? 'gt' : 'lt' });
      }
    }
    return this.value || vals.length > 0 ? vals : "";
  },
  onChange: function (a, b, c) {
    this.fireEvent('select', this);
  },
  setValue: function (value) {
    var vals = [];
    var comps = this.items.keys;
    for (var i = 0; i < comps.length; i++) {
      var comp = this.items.get(i);
      comp.setValue(value);
    }
  }
});

Ext.reg('dualDateField', Ext.ux.DualDateField);