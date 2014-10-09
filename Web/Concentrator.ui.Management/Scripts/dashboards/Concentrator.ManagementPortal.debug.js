/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.ManagementPortal = (function () {
  var managementPortal = Ext.extend(Concentrator.Portal, {


    constructor: function (config) {
      Ext.apply(this, config);
      //will be evaluated when the constructor is called
      this.initMenus();
      var self = this;
      this.refreshTask = new Ext.util.DelayedTask(function () {
        self.refresh.call(self);
      });

      Concentrator.ManagementPortal.superclass.constructor.call(this, config);
    },

    clickClearDate: function (btn) {
      this.UntilDate = undefined;
      this.FromDate = undefined;
      this.parentMenu.removeClass('x-btn-pressed');

      this.refresh();
    },
    /**
    Creates and initializes all menus
    */
    initMenus: function () {

      this.FromDate,
      this.UntilDate,
      this.OnDate = null;

      //creation time
      this.filterDateMenu = new Ext.menu.Menu({
        defaults: {

      },
      items: [
          {
            text: 'From',
            showSeparator: false,
            menu: new Ext.menu.DateMenu({
              listeners: {
                'select': (function (picker, date) {

                  this.OnDate = undefined;
                  this.UntilDate = undefined;
                  this.FromDate = date;
                  this.filterDateMenuGroup.addClass('x-btn-pressed');

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

                  this.OnDate = undefined;
                  this.FromDate = undefined;
                  this.UntilDate = date;
                  this.filterDateMenuGroup.addClass('x-btn-pressed');

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
                this.OnDate = undefined;
                this.UntilDate = undefined;
                this.FromDate = undefined;
                this.filterDateMenuGroup.removeClass('x-btn-pressed');

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


    this.filterDateMenuGroup = new Ext.ButtonGroup({
      columns: 1,
      layout: 'table',
      height: 70,
      items: [
           {
             text: 'Filter Date',
             iconCls: 'calander',
             rowspan: '3',
             scale: 'small',
             arrowAlign: 'bottom',
             iconAlign: 'top',
             width: 40,
             menu: this.filterDateMenu
           }]
    });

    return new Ext.Toolbar({
      allowOtherMenus: true,
      autoShow: false,
      floating: false,
      ignoreParentClicks: true,
      items: [
               this.filterDateMenuGroup,
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

    this.filterDateMenu.uncheckAll();

    this.filterDateMenuGroup.removeClass('x-btn-pressed');
    this.OnDate = undefined;
    this.FromDate = undefined;
    this.UntilDate = undefined;

    this.refresh();
  },


  /**
  Override the refresh function from the superclass
  */
  refresh: function () {
    //serialize the filters into a params object
    var params = this.collectFilters();
    //call the base to actually perform the refresh
    Concentrator.ManagementPortal.superclass.refresh.call(this, params);
  },

  /**
  Collects the values from all filters and puts them in an object
  */
  collectFilters: function () {
    var filters = {};


    // Date filters
    if (this.FromDate) {
      filters.OnDate = null;
      filters.FromDate = this.FromDate;
    }

    if (this.UntilDate) {
      filters.OnDate = null;
      filters.UntilDate = this.UntilDate;
    }

    return filters;
  },

  _getPortlet: function (portlet, params) {
    if (!params) params = {};

    //attach the portal filters
    Ext.apply(params, this.collectFilters());
    var self = this;
    return Concentrator.ManagementPortal.superclass._getPortlet.call(this, portlet, params, {
      getFilterParams: function () {
        var params = self.collectFilters();
        return params;
      }
    });
  }

});

return managementPortal;
})();