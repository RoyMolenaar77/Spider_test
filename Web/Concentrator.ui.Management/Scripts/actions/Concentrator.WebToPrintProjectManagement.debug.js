Concentrator.WebToPrintProjectManagement = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {

    var projectStore = new Ext.data.JsonStore({
      url: Concentrator.route('GetProjects', 'WebToPrint'),
      root: 'results',
      idProperty: 'ProjectID',
      fields: ['ProjectID','UserID', 'Name', 'Description'],
      listeners: {
        'load': function () {
          //bindinggrid.selModel.selectFirstRow();
        }
      }
    });

    var projectIDeditor = new Ext.form.Hidden({
      name: 'ProjectID'
    });

    var documentGrid = new Concentrator.ui.Grid({
      title: 'Documents',
      pluralObjectName: 'Documents',
      singularObjectName: 'Document',
      primaryKey: 'DocumentID',
      url: Concentrator.route("GetDocuments", "WebToPrint"),
      deleteUrl: Concentrator.route("DeleteDocument", "WebToPrint"),
      editUrl: Concentrator.route("EditDocument", "WebToPrint"),
      permissions: {
        list: 'GetBrand',
        create: 'CreateBrand',
        remove: 'DeleteBrand',
        update: 'UpdateBrand'
      },
      callback: function (a, b, c) {
        //bindingsStore.reload();
      },
      structure: [
        { dataIndex: 'DocumentID', type: 'int', header: 'Page ID', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'ProjectID', type: 'int', editor: projectIDeditor },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textfield', allowBlank: false} }
      ]
    });

    var compositeGrid = new Concentrator.ui.Grid({
      title: 'Composites',
      pluralObjectName: 'Composites',
      singularObjectName: 'Composite',
      primaryKey: 'CompositeID',
      url: Concentrator.route("GetComposites", "WebToPrint"),
      deleteUrl: Concentrator.route("DeleteComposite", "WebToPrint"),
      editUrl: Concentrator.route("EditComposite", "WebToPrint"),
      permissions: {
        list: 'GetBrand',
        create: 'CreateBrand',
        remove: 'DeleteBrand',
        update: 'UpdateBrand'
      },
      callback: function (a, b, c) {
        //bindingsStore.reload();
      },
      structure: [
        { dataIndex: 'CompositeID', type: 'int', header: 'Page ID', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'ProjectID', type: 'int', editor: projectIDeditor },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'Data', type: 'string', header: 'Data' }
        ]
    });

    var pageGrid = new Concentrator.ui.Grid({
      title: 'Pages',
      pluralObjectName: 'Pages',
      singularObjectName: 'Page',
      primaryKey: 'PageID',
      url: Concentrator.route("GetPages", "WebToPrint"),
      deleteUrl: Concentrator.route("DeletePage", "WebToPrint"),
      editUrl: Concentrator.route("EditPage", "WebToPrint"),
      permissions: {
        list: 'GetBrand',
        create: 'CreateBrand',
        remove: 'DeleteBrand',
        update: 'UpdateBrand'
      },
      callback: function (a, b, c) {
        //bindingsStore.reload();
      },
      structure: [
        { dataIndex: 'PageID', type: 'int', header: 'Page ID', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'ProjectID', type: 'int', editor: projectIDeditor },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'Data', type: 'string', header: 'Data' }
        ]
    });


    var projectgrid = new Concentrator.ui.Grid({
      pluralObjectName: 'Projects',
      singularObjectName: 'Project',
      primaryKey: 'ProjectID',
      url: Concentrator.route("GetProjects", "WebToPrint"),
      updateUrl: Concentrator.route("UpdateProject", "WebToPrint"),
      deleteUrl: Concentrator.route("DeleteProject", "WebToPrint"),
      editUrl: Concentrator.route("EditProject", "WebToPrint"),
      permissions: {
        list: 'GetBrand',
        remove: 'DeleteBrand',
        update: 'UpdateBrand'
      },
      viewConfig: {
        forceFit: true,
        enableRowBody: true,
        showPreview: false
      },

      listeners: {
        'rowclick': function (grid, rowindex, r) {
          var id = grid.store.getAt(rowindex).get('ProjectID');
          projectIDeditor.setValue(id);
          pageGrid.store.load({
            params: { 'ProjectID': id }
          });
          compositeGrid.store.load({
            params: { 'ProjectID': id }
          });
          documentGrid.store.load({
            params: { 'ProjectID': id }
          });
        }
      },
      callback: function (a, b, c) {
        //bindingsStore.reload();
      },
      structure: [
        { dataIndex: 'ProjectID', type: 'int', header: 'ID' },
        { dataIndex: 'UserID', type: 'int', header: 'User' },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield', allowBlank: false} },
        { dataIndex: 'Description', type: 'string', header: 'Description', editor: { xtype: 'textarea', allowBlank: false}}]
    });

    var panel = new Ext.Panel({
      layout: 'border',
      items: [
      { items: [projectgrid],
        region: 'north',
        height: 200,
        layout: 'fit'
      },
      { items: [documentGrid,pageGrid,compositeGrid],
        region: 'center',
        layout: 'accordion'
      }]
    });
    return panel;
  }
});