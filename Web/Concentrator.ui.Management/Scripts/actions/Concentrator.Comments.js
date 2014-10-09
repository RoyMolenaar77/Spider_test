Concentrator.Comments = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Comments',
      singularObjectName: 'Comment',
      primaryKey: 'CommentID',
      url: Concentrator.route("GetList", "Comments"),
      updateUrl: Concentrator.route("Update", "Comments"),
      deleteUrl: Concentrator.route("Delete", "Comments"),
      permissions: {
        all: 'Default'
      },
      structure: [
        { dataIndex: 'CommentID', type: 'int' },
        { dataIndex: 'Username', type: 'string', header: 'Created by' },
        { dataIndex: 'Priority', type: 'string', header: 'Priority' },
        { dataIndex: 'ArticleNumber', type: 'string', header: 'Article number' },
        { dataIndex: 'ProductName', type: 'string', header: 'Product' },
        { dataIndex: 'NotificationType', type: 'string', header: 'NotificationType' },
        { dataIndex: 'Description', type: 'string', header: 'Description' },
        { dataIndex: 'IsResolved', type: 'boolean', header: 'Resolved', editable: true },
        { dataIndex: 'CreationTime', type: 'date', header: 'Created', renderer: Ext.util.Format.dateRenderer('F j, Y, g:i a') }
      ],
      rowActions: [
        {
          text: 'View details',
          iconCls: 'gear-view',
          handler: function (rec) {
            var form = new Diract.ui.Form({
              noButton: true,
              listeners: {
                'afterrender': function (form) {
                  form.load({
                    url: Concentrator.route('Get', 'Comments'),
                    params : { id: rec.get('CommentID') }
                  });
                }
              },
              items: [
                {
                  xtype: 'textfield',
                  name: 'Username',
                  fieldLabel: 'Created By',
                  width : 200
                },
                {
                  xtype: 'textfield',
                  name: 'ArticleNumber',
                  fieldLabel: 'Article number',
                  width: 200
                },
                {
                  xtype: 'textfield',
                  name: 'Product',
                  fieldLabel: 'Product',
                  width: 200
                },
              {
                xtype: 'textfield',
                name: 'NotificationType',
                fieldLabel: 'Notification',
                width: 200
              },
              {
                xtype: 'textarea',
                name: 'Description',
                fieldLabel: 'Description',
                width: 200
              },
              {
                xtype: 'checkbox',
                name: 'IsResolved',
                fieldLabel: 'Resolved'
              }
              ]
            });

            var window = new Ext.Window({
              items: form,
              title: 'Comment details',
              width: 456,
              layout : 'fit',
              height: 260
            });
            window.show();
          }
        }
      ]
    });

    return grid;
  }


});