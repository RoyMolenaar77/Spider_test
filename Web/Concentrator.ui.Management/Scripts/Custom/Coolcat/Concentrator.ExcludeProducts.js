Concentrator.ExcludeProducts = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'VendorItemNumber to exclude',
      pluralObjectName: 'VendorItemNumbers excluded ',
      primaryKey: ['ExcludeProductID'],
      url: Concentrator.route('GetAll', 'ExcludeProduct'),
      deleteUrl: Concentrator.route('Delete', 'ExcludeProduct'),
      newUrl: Concentrator.route('Create', 'ExcludeProduct'),
      permissions: {
        list: 'GetExcludeProducts',
        create: 'CreateExcludeProducts',
        remove: 'DeleteExcludeProducts'
      },
      structure: [
        { dataIndex: 'ExcludeProductID', type: 'int' },
        { dataIndex: 'Value', type: 'string', header: 'Value', editor: {xtype: 'textfield', allowBlank: false}, editable: false }
      ]

    });
    return grid;
  }
});