/// <reference path="~/Content/js/ext/ext-base-debug.js" /> 
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.EventLogs = Ext.extend(Concentrator.GridAction, {

getPanel: function() {
    var self = this;
    var grid = new Diract.ui.Grid({
      singularObjectName: 'Event Log',
      pluralObjectName: 'Event Logs',
      primaryKey: 'EventID',
      url: Concentrator.route('GetList', 'Event'),
      orderBy: 'CreationTime',
      permissions: {
        list: 'GetEvent',
        create: 'CreateEvent',
        remove: 'DeleteEvent',
        update: 'UpdateEvent'
      },
      structure: [
        { dataIndex: 'TypeIcon', header: ' ', renderer: function(value, metadata, record) {
          var cssClass = self.getEventClass(record);

          return '<div class="' + cssClass + ' event-type-icon"></div>';

        }, filterable: false, width: 20
        },
        { dataIndex: 'EventID', type: 'int', header: 'Event ID', renderer: function(value, metadata, record) { return "#" + value; } },
        { dataIndex: 'TypeID', type: 'int', header: 'Type', renderer: Concentrator.renderers.field('eventTypes', 'Type'),
          filter: {
            type: 'list',
            labelField: 'Type',
            store: Concentrator.stores.eventTypes
          }
        },
        { dataIndex: 'CreationTime', type: 'date', header: 'Date', renderer: Ext.util.Format.dateRenderer('F j, Y, g:i a') },
        { dataIndex: 'Message', type: 'string', header: 'Message' },
        { dataIndex: 'ProcessName', type: 'string', header: 'Process' },
        { dataIndex: 'User', type: 'string', header: 'User' },
        { dataIndex: 'ExceptionMessage', type: 'string' },
        { dataIndex: 'ExceptionLocation', type: 'string' },
        { dataIndex: 'StackTrace', type: 'string' }
      ],
      rowActions: [
        {
          text: 'View details',
          handler: function(record) {
            self.getEventDetails(record);
          },
          roles: ['Event'],
          iconCls: 'gear-view'
}]
    });
    return grid;
  },

  getEventClass: function(record) {
    var eType = record.get('TypeID');
    switch (eType) {
      case 1:
        return 'info-event';
        break;
      case 2:
        return 'warning-event'
        break;
      case 3:
        return 'error-event'
        break;
      case 4:
        return 'fatal-event'
        break;
      case 5:
        return 'success-event'
        break;
      case 6:
        return 'critical-event';
        break;
      case 7:
        return 'complete-event';
        break;
    }
  },

  getEventDetails: function(record) {

    var eventType = Concentrator.stores.eventTypes.getById(record.get('TypeID')).get('Type');
    var htmlTemp =

    '<div class="event-type-image ' + this.getEventClass(record) + '-large event-large"></div>' +
      '<p class="event-detail"><span class="caption">Event ID:</span> #{EventID}</p> ' +
      '<p class="event-detail"><span class="caption">Event Type:</span>' + eventType + ' </p>' +
      '<p class="event-detail"><span class="caption">Message:</span> {Message}</p>' +
      '<p class="event-detail"><span class="caption">Process:</span> {ProcessName}</p>' +
      '<p class="event-detail"><span class="caption">Creation Time:</span> {CreationTime:date("F j, Y, g:i a")}</p>' +
      '<p class="event-detail"><span class="caption">Created By:</span> {User}</p>' +
      '<div class = "exception-details">' +
      '<p class="event-detail"><span class="caption">Exception Details</span></p>' +
      '<p class="event-detail"><span class="caption">ExceptionMessage:</span> {ExceptionMessage}</p>' +
      '<p class="event-detail"><span class="caption">Stack trace:</span> {StackTrace}</p>' +
      '</div>';

    var tp = new Ext.Template(htmlTemp);
    tp.compile();

    var panel = new Ext.Panel({
      data: record.data,
      panel: 10,
      tpl: tp,
      padding: 10

    });

    var roleWindow = new Ext.Window({
      modal: true,
      title: 'Event details (advanced)',
      items: panel,
      layout: 'fit',
      width: 600,
      height: 300
    });
    roleWindow.show();
  }
});