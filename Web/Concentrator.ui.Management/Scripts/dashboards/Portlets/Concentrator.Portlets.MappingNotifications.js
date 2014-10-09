/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

Concentrator.Portlets.MappingNotifications = Ext.extend(Concentrator.Portlets.HtmlPortlet,
{
  constructor: function (config) {
    var that = this;
    config.title = 'Mapping actions';
    config.height = 270;
    config.loadUrl = Concentrator.route('GetNotifications', 'Notifications');
    Ext.apply(that, config);
    Concentrator.Portlets.MappingNotifications.superclass.constructor.call(that, config);
  }

});