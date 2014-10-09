/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.AdvancedPricingPortal = (function () {
  var AdvancedPricingPortal = Ext.extend(Concentrator.Portal, {
    //required menus
    brandMenu: null,
    productMenu: null,
    productGroupMenu: null,
    contentVendorMenu: null,
    activeProductMenu: null,

    constructor: function (config) {

      Ext.apply(this, config);
      //will be evaluated when the constructor is called
      this.initMenus();
      var self = this;

      var params =
      {
        productID: this.productID
      };

      self.refresh(params);

      this.refreshTask = new Ext.util.DelayedTask(function () {

        self.refresh.call(self);
      });

      Concentrator.AdvancedPricingPortal.superclass.constructor.call(this, config);
    },

    clickClearDate: function (btn) {
      this.fromDate = undefined;
      this.untilDate = undefined;
      this.beforeDate = undefined;
      this.parentMenu.removeClass('x-btn-pressed');

      this.refresh();
    },
    /**
    Creates and initializes all menus
    */
    initMenus: function () {

      if (!this.productID) {

        //create the product menu
        this.productMenu = new Ext.menu.Menu({
          items: [
          {
            xtype: 'product',
            enableCreateItem: false,
            selectOnFocus: false,
            listeners: {
              'select': (function (field) {
                this.handleMenuSelect(this.productMenu, field, undefined, this.productGroup);

              }).createDelegate(this)
            }
          }]
        });
      }
      //eo products menu

      //creation time
      this.ledgerDateMenu = new Ext.menu.Menu({
        defaults: {
        },
        items: [
          {
            text: 'From',
            showSeparator: false,
            menu: new Ext.menu.DateMenu({
              listeners: {
                'select': (function (picker, date) {

                  //this.untilDate = undefined;
                  this.fromDate = date;
                  this.ledgerDateMenuGroup.addClass('x-btn-pressed');

                  this.refresh();

                }).createDelegate(this)
              }
            })
          },
          {
            text: 'Until',
            menu: new Ext.menu.DateMenu({
              listeners: {
                'select': (function (picker, date) {

                  //                  this.fromDate = undefined;
                  //                  this.beforeDate = undefined;
                  this.untilDate = date;
                  this.ledgerDateMenuGroup.addClass('x-btn-pressed');

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
                this.fromDate = undefined;
                this.untilDate = undefined;
                this.beforeDate = undefined;
                this.ledgerDateMenuGroup.removeClass('x-btn-pressed');

                this.refresh();
              }).createDelegate(this)
            }
          })
        ]
      }); //eo creation time menu
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
      var itArray = [];

      if (!this.productID) {
        this.productGroup = new Ext.ButtonGroup({
          columns: 1,
          layout: 'table',
          height: 70,
          items: [{
            text: 'Product',
            iconCls: 'cubes',
            tooltip: 'Filter on product',
            rowspan: '3',
            scale: 'small',
            arrowAlign: 'bottom',
            iconAlign: 'top',
            width: 50,
            menu: this.productMenu
          }]
        });
        itArray.push(this.productGroup);

      }
      this.ledgerDateMenuGroup = new Ext.ButtonGroup({
        columns: 1,
        layout: 'table',
        height: 70,
        items: [
           {
             text: 'Ledger Date',
             iconCls: 'calander',
             rowspan: '3',
             scale: 'small',
             arrowAlign: 'bottom',
             iconAlign: 'top',
             width: 40,
             menu: this.ledgerDateMenu
           }]
      });
      itArray.push(this.ledgerDateMenuGroup);
      itArray.push('->');
      itArray.push(new Ext.ButtonGroup({
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
      }));
      return new Ext.Toolbar({
        allowOtherMenus: true,
        autoShow: false,
        floating: false,
        ignoreParentClicks: true,
        items: itArray
      });
    },

    /**
    Resets the filters in the toolbar 
    */
    resetFilters: function () {
      //iterate over filters and reset each filter

      if (!this.productID) {
        this.productMenu.uncheckAll();
        this.productMenu.removeAllCheckItems();
      }

      this.ledgerDateMenu.uncheckAll();

      this.ledgerDateMenuGroup.removeClass('x-btn-pressed');
      this.fromDate = undefined;
      this.beforeDate = undefined;
      this.untilDate = undefined;

      this.refresh();
    },

    /**
    Override the refresh function from the superclass
    */
    refresh: function () {
      //serialize the filters into a params object
      var params = this.collectFilters();

      //call the base to actually perform the refresh
      Concentrator.AdvancedPricingPortal.superclass.refresh.call(this, params);
    },

    /**
    Collects the values from all filters and puts them in an object
    */
    collectFilters: function () {
      var filters = {};

      filters.productID = null;
      filters.fromDate = null;
      filters.untilDate = null;

      //all filters are applied as a check box, so collect only those values

      // Date filters
      if (this.fromDate) {
        filters.fromDate = this.fromDate;
      }

      if (this.untilDate) {
        filters.untilDate = this.untilDate;
      }

      if (this.productID) filters.productID = this.productID;
      else {
        this.productMenu.findBy(function (it) {
          if (it instanceof Ext.menu.CheckItem) {
            if (it.checked) {
              filters.productID = it.id;
            }
            return true;
          }
        }, this);
      }

      return filters;
    },

    _getPortlet: function (portlet, params) {
      if (!params) params = {};

      //attach the portal filters
      Ext.apply(params, this.collectFilters());
      var self = this;
      return Concentrator.AdvancedPricingPortal.superclass._getPortlet.call(this, portlet, params, {
        getFilterParams: function () {
          var params = self.collectFilters();
          return params;
        }
      });
    }

  });

  return AdvancedPricingPortal;
})();