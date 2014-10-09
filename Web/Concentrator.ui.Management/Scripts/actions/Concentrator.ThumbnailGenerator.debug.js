Concentrator.ThumbnailGenerator = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Vendor Accruel',
      pluralObjectName: 'Vendor Accruels',
      permissions: {
        list: 'GetThumbnail',
        update: 'UpdateThumbnail'
      },
      url: Concentrator.route('GetList', 'Thumbnail'),
      updateUrl: Concentrator.route('Update', 'Thumbnail'),
      primaryKey: 'ThumbnailGeneratorID',
      sortBy: 'ThumbnailGeneratorID',
      structure: [
        { dataIndex: 'ThumbnailGeneratorID', type: 'int' },
        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textfield'} },
        { dataIndex: 'Width', type: 'int', header: 'Width', editor: { xtype: 'numberfield', allowBlank: false }, editable: true },
        { dataIndex: 'Height', type: 'int', header: 'Height', editor: { xtype: 'numberfield', allowBlank: false }, editable: true },
        { dataIndex: 'Resolution', type: 'int', header: 'Resolution', editor: { xtype: 'numberfield', allowBlank: false }, editable: true }
      ],
      rowActions: [
        {
          text: 'View Thumbnails',
          iconCls: 'merge',
          handler: function (record) {

            var grid = new Concentrator.ui.Grid({
              pluralObjectName: 'Thumbnails',
              singularObjectName: 'Thumbnail',
              primaryKey: 'MediaID',
              url: Concentrator.route("GetThumbnails", "Thumbnail"),
              permissions: {
                list: 'GetThumbnail',
                update: 'UpdateThumbnail'
              },
              params: {
                thumbnailGeneratorID: record.get("ThumbnailGeneratorID")
              },
              structure: [
                  { dataIndex: 'MediaID', type: 'int' },
                  { dataIndex: 'MediaPath', type: 'string', header: 'Media Path' },
                  { dataIndex: 'FileName', type: 'string', header: 'File Name' },
                  { dataIndex: 'OriginalResolution', type: 'string', header: 'Original Resolution' },
                  { dataIndex: 'OriginalSize', type: 'string', header: 'Original Size' }
              ]
            });

            var window = new Ext.Window({
              title: 'View Thumbnails',
              modal: true,
              layout: 'fit',
              height: 300,
              width: 600,
              items: [
                grid
              ]
            });

            window.show();
          }
        }
      ]
    });

    return grid;
  }
});