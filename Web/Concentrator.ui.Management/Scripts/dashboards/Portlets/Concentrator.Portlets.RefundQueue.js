/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Ext.ns('Concentrator.Portlets');

//Displays the current status of the refund queue.
//Uses Views/Shared/RefundQueue.cshtml as GUI
Concentrator.Portlets.RefundQueue = Ext.extend(Concentrator.Portlets.HtmlPortlet,
{
  constructor: function (config) {
    var that = this;
    config.title = 'Refund queue';
    config.height = 100;
    config.width = 300;
    config.loadUrl = Concentrator.route('WidgetData', 'Refund');

    Ext.apply(that, config);

    Concentrator.Portlets.RefundQueue.superclass.constructor.call(that, config);
  }
});