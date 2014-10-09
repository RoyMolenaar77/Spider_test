Concentrator.ProductGroups = Ext.extend(Concentrator.BaseAction, {
  constructor: function (config) {
    Ext.apply(this, config);
    Concentrator.ProductGroups.superclass.constructor.call(this, config);
  },

  getPanel: function () {
    this.grid = new Concentrator.ui.TranslationGrid({
      pluralObjectName: 'Product Groups',
      singularObjectName: 'Product Group',
      primaryKey: 'ProductGroupID',
      url: Concentrator.route("GetList", "ProductGroup"),
      height: 900,
      width: 600,
      permissions: {
        list: 'GetProductGroup',
        create: 'CreateProductGroup',
        remove: 'DeleteProductGroup',
        update: 'UpdateProductGroup'
      },
      getParams: function (rec) {
        return {
          productGroupID: rec.get('ProductGroupID')
        };
      },
      updateUrl: Concentrator.route("Update", "ProductGroup"),
      translationsUrl: Concentrator.route('GetTranslations', 'ProductGroup'),
      translationsUrlUpdate: Concentrator.route('SetTranslation', 'ProductGroup'),
      translationsGridStructure: [
        { dataIndex: 'ProductGroupID', type: 'int' },
        { dataIndex: 'Language', type: 'string', header: 'Language' },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield'} },
        { dataIndex: 'LanguageID', type: 'int' }
      ],
      newUrl: Concentrator.route("Create", "ProductGroup"),
      deleteUrl: Concentrator.route("Delete", "ProductGroup"),
      newFormConfig: Concentrator.FormConfigurations.newProductGroup,
      structure: [
        { dataIndex: 'ProductGroupID', type: 'int' },
        { dataIndex: 'Name', type: 'string', header: 'Name', editable: false, editor: { xtype: 'languageManager'} },
        { dataIndex: 'ProductGroupVendorCount', type: 'int', header: 'Vendor Product Groups' },
        { dataIndex: 'Languages', type: 'int', header: 'Languages' },
        { dataIndex: 'ContentProductGroup', type: 'int', header: 'In productgroupmappings' },
        { dataIndex: 'ActiveProducts', type: 'int', header: 'Active Products' },
         { dataIndex: 'Score', type: 'int', header: 'Score', editable: true,
           editor: [{
             xtype: 'textfield',
             fieldLabel: "Score",
             allowBlank: false,
             name: 'score'
           }]
         }],
      customButtons: [
        new Ext.Toolbar.Button({
          text: 'View Media',
          iconCls: 'merge',
          predicate: function (record) { return (record.get('ProductGroupID') > 0); },
          handler: function (record) {
          var p = new Concentrator.ui.ProductGroupMediaSelector();
            p.show();
          }
        })
      ]

    });

    return this.grid;
  }
});
  





















