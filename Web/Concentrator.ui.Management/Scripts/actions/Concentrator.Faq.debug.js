Concentrator.Faq = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Faq Translations',
      singularObjectName: 'Faq Translation',
      primaryKey: 'FaqID',
      url: Concentrator.route("GetList", "Faq"),
      newUrl: Concentrator.route("Create", "Faq"),
      updateUrl: Concentrator.route("Update", "Faq"),
      deleteUrl: Concentrator.route("Delete", "Faq"),
      //newFormConfig: Concentrator.FormConfigurations.newFaq,
      permissions: {
        list: 'GetFaq',
        create: 'CreateFaq',
        remove: 'DeleteFaq',
        update: 'UpdateFaq'
      },
      structure: [
        { dataIndex: 'FaqID', type: 'int', header: 'Faq ID', width: 20 },
        { dataIndex: 'Question', type: 'string', header: 'Question', editor: { xtype: 'textfield', allowBlank: false} },
           { dataIndex: 'LanguageID', type: 'int', header: 'Language', editor: { xtype: 'language', allowBlank: false }, renderer: Concentrator.renderers.field('languages', 'Name') },
          { dataIndex: 'Mandatory', type: 'boolean', header: 'Mandatory', editor: { xtype: 'checkbox'} },
          { dataIndex: 'CreationTime', type: 'date', header: 'Creation Time' },
          { dataIndex: 'LastModificationTime', type: 'date', header: 'Last Modification Time' }
      ]

    });

    return grid;
  }

});

