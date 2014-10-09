Concentrator.ProductGroupLanguages = Ext.extend(Concentrator.BaseAction, {
  getPanel: function() {

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Product Group Languages',
      singularObjectName: 'Product Group Language',
      primaryKey: ['ProductGroupID', 'LanguageID'],
      sortBy: 'ProductGroupID',
      url: Concentrator.route("GetList", "ProductGroupLanguage"),
      permissions: {
        list: 'GetProductGroup',
        create: 'CreateProductGroup',
        remove: 'DeleteProductGroup',
        update: 'UpdateProductGroup'
      },
      updateUrl: Concentrator.route("Update", "ProductGroupLanguage"),
      newUrl: Concentrator.route("Create", "ProductGroupLanguage"),
      deleteUrl: Concentrator.route("Delete", "ProductGroupLanguage"),
      groupField: "ProductGroupID",
      structure: [
                { dataIndex: 'ProductGroupID', type: 'int', header: "Product Group ID", width: 25,
                  editable: false,

                  editor: {
                    allowBlank: false,
                    xtype: 'productgroup'
                  }
                },
                { dataIndex: 'EnglishName', type: 'string', header: "English Name", width: 100 },
                { dataIndex: 'LanguageID', type: 'int', width: 100, header: 'Language', renderer: Concentrator.renderers.language,
                  editor: {
                    xtype: 'language'
                  },
                  filter: {
                    type: 'list',
                    store: Concentrator.stores.languages,
                    labelField: 'Name'
                  }
                },
                { dataIndex: 'Name', type: 'string',
                  editor: {
                    xtype: 'textfield',
                    allowBlank: false
                  },
                  header: "Name"
                }

        ]
    });

    return grid;
  }
});