/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.ActiveProductGroup = Ext.extend(Concentrator.Portlets.AnychartPortlet,
{  
  constructor: function (config) {
    config.title = 'Active product groups';
    config.loadUrl = Concentrator.route('GetActiveProductGroups', 'Notifications');
    config.height = 350;
    Ext.apply(this, config);

    Concentrator.Portlets.ActiveProductGroup.superclass.constructor.call(this, config);
  }
});