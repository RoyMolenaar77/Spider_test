/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlet = (function () {

  var portlet = Ext.extend(Ext.ux.Portlet, {
    base: "test",
    params: {},
    layout: 'fit',


    close: function (evt, btn, component) {

      var that = this;
      // Create an array that contains all the active portlets
      var portlets = this.portal.activePortlets;

      // Remove portlet from active portlets
      portlets.remove(component);

      component.destroy();
      that.portal.doLayout();

      // Calls the saveLayout() function in portal
      this.portal.saveLayout();
    },

    getFilterParams: function () {
      return this.params || {};
    },

    constructor: function (config) {
      Ext.apply(this, config);
      this.title = this.title || 'Portlet';
      this.height = this.height || 300;      

      var params = {
        productID: this.params.productID
      }

      

      this.initTools();

      this.addEvents('filterUpdate', 'registerCustomFilter');

      Concentrator.Portlet.superclass.constructor.call(this, config);
    },

    registerCustomFilter: function (portlet, filter) {
      this.fireEvent('registerCustomFilter', portlet, filter);
    },

    filterUpdate: function (portlet, filter) {
      this.fireEvent('filterUpdate', portlet, filter);
    },

    /**
    Initializes the tools. 
    Will initilize an extra tool item if portlet requires extra filters
    */
    initTools: function () {
      var tools = [
         {
           id: 'refresh',
           handler: function (evt, btn, component) {
             component.refresh();
           }
         },
         {
           id: 'close',
           handler: function (evt, btn, component) {
             component.close(evt, btn, component);
           }
         }
    ];

      if (this.requireGearTool) {
        tools.push
        ({
          id: 'gear',
          handler: this.settingsHandler.createDelegate(this)
        });
      }
      this.tools = tools;
    },

    /**
    Callback to the settings tool button. Needs to be implemented by component if required
    */
    settingsHandler: function (evt, btn, component) {

      if (this.requireGearTool) throw "This method needs to be overwritten by child portlet.";
    }

  });

  return portlet;

})();

Ext.reg('portletLoader', Concentrator.Portlet);