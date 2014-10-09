Concentrator.ModelDescriptions = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Model',
      pluralObjectName: 'Models',
      primaryKey: ['ModelCode'],
      sortBy: 'ModelCode',
      url: Concentrator.route('GetAll', 'Model'),
      updateUrl: Concentrator.route('Update', 'Model'),
      deleteUrl: Concentrator.route('Delete', 'Model'),
      newUrl: Concentrator.route('Create', 'Model'),
      permissions: {
        list: 'Default',
        create: 'Default',
        remote: 'Default',
        update: 'Default'
      },
      structure: [
                    { dataIndex: 'ModelCode', type: 'string', header: 'Type', editor: { xtype: 'textfield', allowBlank: false }, editable: false },
                    { dataIndex: 'Translation', type: 'string', header: 'Translation', editor: { xtype: 'textfield' } }
      ]
    });
    return grid;
  }
});