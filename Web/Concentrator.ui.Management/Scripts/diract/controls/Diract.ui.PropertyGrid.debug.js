/// <reference path="~/Scripts/ext/ext-base-debug.js" />
/// <reference path="~/Scripts/ext/ext-all-debug.js" />

Diract.ui.PropertyGrid = (function () {
  var propertyGrid = Ext.extend(Ext.grid.PropertyGrid, {
    border: false,
    hideHeaders: true,
    constructor: function (config) {
      Ext.apply(this, config);
      Diract.ui.PropertyGrid.superclass.constructor.call(this, config);
    }
  });
  return propertyGrid;
})();
  
