Concentrator.EDIOrderListener = Ext.extend(Concentrator.GridAction, {

  getPanel: function () {
    var self = this;

    function ExportDocument(id, message, processed) {

      var textbox = new Ext.form.TextArea({
        anchor: "100%",
        disabled: true,
        value: message
      });

      if (processed === false) {
        textbox.enable();
      }

      var buttons = [];
      var msg = textbox.getValue();

      buttons.push(new Ext.Button({
        text: "Export",
        handler: function () {
          window.location = Concentrator.route("ExportDocument", "EdiOrderListener", id);
        }
      }));

      var form = new Ext.Panel({
        layout: "fit",
        border: false,
        items: [
          textbox
        ]
      });

      var detailWindow = new Ext.Window({
        layout: "fit",
        width: 600,
        height: 500,
        title: "Show request message",
        items: [
          form
        ],
        buttons: buttons
      });

      detailWindow.show();
    };

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'EDi Order Listener',
      pluralObjectName: 'EDi Order Listeners',
      groupOnSort: true,
      permissions: {
        list: 'GetEdiOrderListener',
        create: 'CreateEdiOrderListener',
        update: 'UpdateEdiOrderListener',
        remove: 'DeleteEdiOrderListener'
      },
      url: Concentrator.route('GetList', 'EdiOrderListener'),
      primaryKey: 'EdiRequestID',
      structure: [
        { dataIndex: 'EdiRequestID', type: 'int' },
        { dataIndex: 'CustomerName', type: 'string', header: 'Customer Name' },
        { dataIndex: 'CustomerIP', type: 'string', header: 'Customer IP' },
        { dataIndex: 'CustomerHostName', type: 'string', header: 'Customer Host Name' },
        { dataIndex: 'RequestDocument', type: 'string', header: 'Request Document' },
        { dataIndex: 'ReceivedDate', type: 'date', header: 'Received Date' },
        { dataIndex: 'Processed', type: 'boolean', header: 'Processed' },
        { dataIndex: 'ResponseRemark', type: 'string', header: 'Response Remark' },
        { dataIndex: 'ResponseTime', type: 'int', header: 'Response Time' },
        { dataIndex: 'ErrorMessage', type: 'string', header: 'Error Message' }
      ],
      rowFormat: function (row, index) {
        var data = row.data,
        cls = '';
        if (data.ErrorMessage) {
          cls = 'grid-row-red';
        }
        return cls;
      },
      //sortInfo: { field: 'ReceivedDate', direction: 'DESC' },
      rowActions: [
        {
          text: 'Request Document',
          iconCls: 'merge',
          handler: function (rec) {
            ExportDocument(rec.get('EdiRequestID'), rec.get('RequestDocument'), rec.get('Processed'));
          }
        }
      ]
    });

    return grid;
  }

});
