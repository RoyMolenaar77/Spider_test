Concentrator.Files = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {
    var that = this;

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Files',
      singularObjectName: 'File',
      primaryKey: 'BrandID',
      url: Concentrator.route("GetList", "Files"),
      permissions: {
        list: 'GetFiles'
      },
      structure: [
        { dataIndex: 'FullName', type: 'string', header: 'Path' },
        { dataIndex: 'Name', type: 'string', header: 'Brand Name' },
        { dataIndex: 'CreationTime', type: 'date', header: 'Creation time', renderer: Ext.util.Format.dateRenderer('d-m-Y  g:i a') }
      ],
      customButtons: [
        {
          text: 'Upload File',
          iconCls: 'upload-icon',
          alignRight: true,
          handler: function (rec) {
            that.uploadFile();
          }
        }
      ],
      rowActions: [
        {
          text: 'Download',
          iconCls: 'default',
          handler: function (rec) {
            window.location = Concentrator.route('Download', 'Files', { path: rec.get('FullName') });
          }
        }
      ]
    });

    return grid;
  },

  uploadFile: function (record) {

    var window = new Diract.ui.FormWindow({
      url: Concentrator.route('UploadFile', 'Files'),
      title: 'Upload file',
      buttonText: 'Upload',
      fileUpload: true,
      layout: 'fit',
      height: 150,
      width: 310,
      items: [
        {
          xtype: 'fileuploadfield',
          fieldLabel: 'Upload a file',
          name: 'File',
          fieldLabel: 'Upload a file:',
          buttonText: '',
          width: 165,
          buttonCfg: { iconCls: 'upload-icon' },
          allowBlank: true
        }
      ],
      success: (function (message) {
        this.getPanel().store.reload();
        window.destroy();
      }).createDelegate(this)
    });

    window.show();
  }

});