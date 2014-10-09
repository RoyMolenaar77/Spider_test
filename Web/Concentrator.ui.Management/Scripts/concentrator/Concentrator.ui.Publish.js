Concentrator.ui.Publish = (function () {
  var panel = Ext.extend(Ext.Panel, {
    layout: 'border',
    constructor: function (config) {
      Ext.apply(this, config);

      Concentrator.ui.Publish.superclass.constructor.call(this, config);
    },

    /**
    * Initializes the main layout
    */
    initLayout: function () {
      var center = new Ext.TabPanel({
        
      });
  },

  /**
  *Adds and inits the tree component
  */
  initTreeComponent: function () {
    
  }

});
return panel;
})();