/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.ContentPortal = (function () {
  var contentPortal = Ext.extend(Concentrator.Portal, {

    //required menus
    brandMenu: null,
    connectorMenu: null,
    productGroupMenu: null,
    contentVendorMenu: null,
    activeProductMenu: null,

    constructor: function (config) {
      Ext.apply(this, config);
      //will be evaluated when the constructor is called
      this.initMenus();
      var self = this;
      this.refreshTask = new Ext.util.DelayedTask(function () {
        self.refresh.call(self);
      });

      Concentrator.ContentPortal.superclass.constructor.call(this, config);
    },

    clickClearDate: function (btn) {
      this.onDate = undefined;
      this.afterDate = undefined;
      this.beforeDate = undefined;
      this.parentMenu.removeClass('x-btn-pressed');

      this.refresh();
    },
    /**
    Creates and initializes all menus
    */
    initMenus: function () {

      //create the connector menu
      this.connectorMenu = new Ext.menu.Menu({
        items: [
          {
            xtype: 'connector',
            enableCreateItem: false,
            selectOnFocus: false,
            listeners: {
              'select': (function (field) {
                this.handleMenuSelect(this.connectorMenu, field, undefined, this.connectorGroup);

              }).createDelegate(this)
            }
          }]
      });
      //eo connector menu

      //create the vendor menu
      this.contentVendorMenu = new Ext.menu.Menu({
        items: [
          {
            xtype: 'vendor',
            enableCreateItem: false,
            selectOnFocus: false,
            listeners: {
              'select': (function (field) {
                this.handleMenuSelect(this.contentVendorMenu, field, undefined, this.contentVendorGroup);
              }).createDelegate(this)
            }
          }]
      }); //eo vendor menu

      //create pg menu
      this.productGroupMenu = new Ext.menu.Menu({
        items: [
          {
            xtype: 'productgroup',
            enableCreateItem: false,
            selectOnFocus: false,
            listeners: {
              'select': (function (field) {
                this.handleMenuSelect(this.productGroupMenu, field, undefined, this.productgroupGroup);
              }).createDelegate(this)
            }
          }]
      }); //eo pg menu

      //brand menu
      this.brandMenu = new Ext.menu.Menu({
        items: [
          {
            xtype: 'brand',
            enableCreateItem: false,
            selectOnFocus: false,
            listeners: {
              'select': (function (field) {
                this.handleMenuSelect(this.brandMenu, field, undefined, this.brandGroup);
              }).createDelegate(this)
            }
          }]
      }); //eo brand menu

      // Creates the status menu
      this.statusMenu = new Ext.menu.Menu({
        items: [
          {
            xtype: 'concentratorstatus',
            enableCreateItem: false,
            selectOnFocus: false,
            listeners: {
              'select': (function (field) {
                this.handleMenuSelect(this.statusMenu, field, undefined, this.stockGroup);
              }).createDelegate(this)
            }
          }
        ]
      });

      this.lowerStockCount,
      this.geaterStockCount,
      this.equalStockCount = null;

      //create stock menu
      this.stockMenu = new Ext.menu.Menu({
        items: [
          {
            text: 'Status',
            menu: this.statusMenu
          },
          new Ext.ux.grid.filter.NumericFilter({
            text: 'Quantity',
            listeners: {
              'update': (function (c) {

                if (c.activeItem) {
                  var selectedItem = c.activeItem.initialConfig.itemId;

                  if (selectedItem == 'range-lt') {
                    this.lowerStockCount = c.getValue().lt;

                    if (!this.lowerStockCount == 0 || !this.lowerStockCount) {
                      this.stockGroup.addClass('x-btn-pressed');
                    }
                    else {
                      this.stockGroup.removeClass('x-btn-pressed');
                    }
                    this.refresh();
                  }

                  if (selectedItem == 'range-gt') {
                    this.greaterStockCount = c.getValue().gt;

                    if (!this.greaterStockCount == 0 || !this.greaterStockCount) {
                      this.stockGroup.addClass('x-btn-pressed');
                    }
                    else {
                      this.stockGroup.removeClass('x-btn-pressed');
                    }
                    this.refresh();
                  }

                  if (selectedItem == 'range-eq') {

                    this.equalStockCount = c.getValue().eq;

                    if (!this.equalStockCount == 0 || !this.equalStockCount) {
                      this.stockGroup.addClass('x-btn-pressed');
                    }
                    else {
                      this.stockGroup.removeClass('x-btn-pressed');
                    }
                    this.refresh();
                  }
                }

              }).createDelegate(this)
            }
          })
        ]
      }); //eo stock menu

      this.beforeDate,
      this.afterDate,
      this.onDate = null;

      //creation time
      this.creationTimeMenu = new Ext.menu.Menu({
        defaults: {

        },
        items: [
          {
            text: 'Before',
            showSeparator: false,
            menu: new Ext.menu.DateMenu({
              listeners: {
                'select': (function (picker, date) {

                  this.onDate = undefined;
                  this.afterDate = undefined;
                  this.beforeDate = date;
                  this.creationTimeMenuGroup.addClass('x-btn-pressed');

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

                  this.onDate = undefined;
                  this.beforeDate = undefined;
                  this.afterDate = date;
                  this.creationTimeMenuGroup.addClass('x-btn-pressed');

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

                  this.beforeDate = undefined;
                  this.afertDate = undefined;
                  this.onDate = date;
                  this.creationTimeMenuGroup.addClass('x-btn-pressed');

                  this.refresh();

                }).createDelegate(this)
              }
            })
          },
          '-',
          new Ext.menu.Item({
            text: 'Clear Filter',
            listeners: {
              'click': (function () {
                this.onDate = undefined;
                this.afterDate = undefined;
                this.beforeDate = undefined;
                this.creationTimeMenuGroup.removeClass('x-btn-pressed');

                this.refresh();
              }).createDelegate(this)
            }
          })
        ]
      }); //eo creation time menu

      //active product menu
      this.activeProductMenu = new Ext.menu.Menu({
        defaults: {
          listeners: {
            'checkchange': (function (field) {
              var it = field.ownerCt.items;
              var checkedItems = false;

              it.each(function (i) {
                if (i.checked) checkedItems = true
              });

              if (!checkedItems) {
                this.activeProductMenuGroup.removeClass('x-btn-pressed');
              }
              else {
                this.activeProductMenuGroup.addClass('x-btn-pressed');
              }
              this.refresh();
            }).createDelegate(this)
          }
        },
        items: [
          {
            text: 'Active',
            checked: false,
            name: 'true',
            hideOnClick: false

          },
          {
            text: 'Non-active',
            checked: false,
            name: 'false',
            hideOnClick: false
          }
        ]
      }); //eo active product menu

      //advanced filters menu
      this.advancedMenu = new Ext.menu.Menu({
        handler: function (dp, date) {
          this.selecteddate = date;
        },
        width: 200,
        defaults: { width: 175 },
        items:
      [
        {
          xtype: 'datefield',
          name: 'startdt',
          id: 'startdt',
          fieldLabel: 'Start date',
          vtype: 'daterange',
          endDateField: 'enddt'
        },
        {
          xtype: 'datefield',
          name: 'enddt',
          id: 'enddt',
          fieldLabel: 'End date',
          vtype: 'daterange',
          startDateField: 'startdt'
        }
      ]
      }); //eo advance filter menu
    },

    /**
    Handler for a selection, change of menu
    */
    handleMenuSelect: function (menu, field, disableAddToMenu, ownerButtonGroup) {

      if (ownerButtonGroup) {
        ownerButtonGroup.addClass('x-btn-pressed');
      }

      //check for existing item and add to menu
      if (!disableAddToMenu) {
        var value = field.getValue(),
            id = field.id;

        var itemExists = menu.findBy(function (it) {
          return (it.id == value && it.id != id) //check for same value and skip editor
        }, this);

        if (itemExists.length > 0) return; //short circuit --> there is already such an item

        menu.addItem({
          text: field.lastSelectionText,
          checked: true,
          hideOnClick: false,
          id: field.getValue(),
          value: field.getValue(),
          listeners: {
            'checkchange': (function (field) {
              var it = field.ownerCt.items;
              var checkedItems = false;

              it.each(function (i) {
                if (i.checked) checkedItems = true
              });

              if (!checkedItems) {
                ownerButtonGroup.removeClass('x-btn-pressed');
              }
              else {
                ownerButtonGroup.addClass('x-btn-pressed');
              }

              this.refresh();
            }).createDelegate(this)
          }
        });
      }

      this.refreshTask.delay(1000);

    },
    getToolbar: function () {

      this.connectorGroup = new Ext.ButtonGroup({
        columns: 1,
        layout: 'table',
        height: 70,
        items: [{
          text: 'Connector',
          iconCls: 'usb',
          tooltip: 'Filter on connector',
          rowspan: '3',
          scale: 'small',
          arrowAlign: 'bottom',
          iconAlign: 'top',
          width: 50,
          menu: this.connectorMenu
        }]

      });

      this.contentVendorGroup = new Ext.ButtonGroup({
        columns: 1,
        layout: 'table',
        height: 70,
        items: [{
          text: 'Content vendor',
          iconCls: 'node',
          rowspan: '3',
          scale: 'small',
          arrowAlign: 'bottom',
          iconAlign: 'top',
          width: 40,
          menu: this.contentVendorMenu
        }]
      });

      this.productgroupGroup = new Ext.ButtonGroup({
        columns: 1,
        layout: 'table',
        height: 70,
        items: [{
          text: 'Product group',
          iconCls: 'package-view',
          rowspan: '3',
          scale: 'small',
          arrowAlign: 'bottom',
          iconAlign: 'top',
          width: 40,
          menu: this.productGroupMenu
        }]
      });

      this.brandGroup = new Ext.ButtonGroup({
        columns: 1,
        layout: 'table',
        parentMenu: 'test3',
        height: 70,
        items: [
        {
          text: 'Brand',
          iconCls: 'package-view',
          rowspan: '3',
          scale: 'small',
          arrowAlign: 'bottom',
          iconAlign: 'top',
          width: 40,
          menu: this.brandMenu
        }]
      });

      this.activeProductMenuGroup = new Ext.ButtonGroup({
        columns: 1,
        layout: 'table',
        height: 70,
        items: [{
          text: 'Active product',
          iconCls: 'package-view',
          rowspan: '3',
          scale: 'small',
          arrowAlign: 'bottom',
          iconAlign: 'top',
          width: 40,
          menu: this.activeProductMenu
        }]
      });

      this.stockGroup = new Ext.ButtonGroup({
        columns: 1,
        layout: 'table',
        parentMenu: 'test3',
        height: 70,
        items: [
            {
              text: 'Stock',
              iconCls: 'package-view',
              rowspan: '3',
              scale: 'small',
              arrowAlign: 'bottom',
              iconAlign: 'top',
              width: 40,
              menu: this.stockMenu
            }]
      });

      this.creationTimeMenuGroup = new Ext.ButtonGroup({
        columns: 1,
        layout: 'table',
        height: 70,
        items: [
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
      });

      return new Ext.Toolbar({
        allowOtherMenus: true,
        autoShow: false,
        floating: false,
        ignoreParentClicks: true,
        items: [
        this.connectorGroup,
        this.contentVendorGroup,
        this.creationTimeMenuGroup,
        this.activeProductMenuGroup,
        this.productgroupGroup,
        this.brandGroup,
        this.stockGroup,

          '->',

          new Ext.ButtonGroup({
            columns: 1,
            layout: 'table',
            height: 70,
            items: [
              {
                text: 'Reset all filters',
                iconCls: 'reset',
                rowspan: '3',
                scale: 'small',
                arrowAlign: 'bottom',
                iconAlign: 'top',
                width: 40,
                handler: (function () {
                  this.resetFilters();
                }).createDelegate(this)
              }
            ]
          })
      ]
      });
    },

    /**
    Resets the filters in the toolbar 
    */
    resetFilters: function () {
      //iterate over filters and reset each filter
      this.brandMenu.uncheckAll();
      this.connectorMenu.uncheckAll();
      this.productGroupMenu.uncheckAll();
      this.contentVendorMenu.uncheckAll();
      this.activeProductMenu.uncheckAll();
      this.creationTimeMenu.uncheckAll();
      this.statusMenu.uncheckAll();

      this.connectorMenu.removeAllCheckItems();
      this.brandMenu.removeAllCheckItems();
      this.productGroupMenu.removeAllCheckItems();
      this.contentVendorMenu.removeAllCheckItems();
      this.statusMenu.removeAllCheckItems();

      this.creationTimeMenuGroup.removeClass('x-btn-pressed');
      this.onDate = undefined;
      this.beforeDate = undefined;
      this.afterDate = undefined;

      this.lowerStockCount = undefined;
      this.greaterStockCount = undefined;
      this.equalStockCount = undefined;

      this.refresh();
    },

    /**
    Override the refresh function from the superclass
    */
    refresh: function () {
  
      //serialize the filters into a params object
      var params = this.collectFilters();

      //call the base to actually perform the refresh
      Concentrator.ContentPortal.superclass.refresh.call(this, params);
    },

    /**
    Collects the values from all filters and puts them in an object
    */
    collectFilters: function () {
      var filters = {};

      filters.connectors = [];
      filters.vendors = [];
      filters.productGroups = [];
      filters.brands = [];
      filters.IsActive = [];
      filters.statuses = [];

      //all filters are applied as a check box, so collect only those values
      this.connectorMenu.findBy(function (it) {
        if (it instanceof Ext.menu.CheckItem) {
          if (it.checked) {
            filters.connectors.push(it.id);
          }
          return true;
        }
      }, this);

      //all filters are applied as a check box, so collect only those values
      this.contentVendorMenu.findBy(function (it) {
        if (it instanceof Ext.menu.CheckItem) {
          if (it.checked) {
            filters.vendors.push(it.id);
          }
          return true;
        }
      }, this);

      var base = new Date();
      var ranges = [];

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

      //Stock filters
      if (this.lowerStockCount) {
        filters.lowerStockCount = this.lowerStockCount;
      }

      if (this.greaterStockCount) {
        filters.greaterStockCount = this.greaterStockCount;
      }

      if (this.equalStockCount) {
        filters.equalStockCount = this.equalStockCount;
      }

      // Stock status
      //all filters are applied as a check box, so collect only those values
      this.statusMenu.findBy(function (it) {
        if (it instanceof Ext.menu.CheckItem) {
          if (it.checked) {
            filters.statuses.push(it.value);
          }
          return true;
        }
      }, this);

      //all filters are applied as a check box, so collect only those values
      this.activeProductMenu.findBy(function (it) {
        if (it instanceof Ext.menu.CheckItem) {
          if (it.checked) {
            filters.IsActive.push(it.name);
          }
          return true;
        }
      }, this);

      //product groups
      //all filters are applied as a check box, so collect only those values
      this.productGroupMenu.findBy(function (it) {
        if (it instanceof Ext.menu.CheckItem) {
          if (it.checked) {
            filters.productGroups.push(it.id);
          }
          return true;
        }
      }, this);
      //brands

      //all filters are applied as a check box, so collect only those values
      this.brandMenu.findBy(function (it) {
        if (it instanceof Ext.menu.CheckItem) {
          if (it.checked) {
            filters.brands.push(it.id);
          }
          return true;
        }
      }, this);
      return filters;
    },

    _getPortlet: function (portlet, params) {
      if (!params) params = {};

      //attach the portal filters
      Ext.apply(params, this.collectFilters());
      var self = this;
      return Concentrator.ContentPortal.superclass._getPortlet.call(this, portlet, params, {
        getFilterParams: function () {
          var params = self.collectFilters();
          return params;
        }
      });
    }

  });

  return contentPortal;
})();