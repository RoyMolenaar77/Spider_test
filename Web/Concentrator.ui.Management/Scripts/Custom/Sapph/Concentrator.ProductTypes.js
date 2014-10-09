Concentrator.ProductTypes = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {
    var grid = new Diract.ui.TranslationGrid({
      singularObjectName: 'Type',
      pluralObjectName: 'Types',
      primaryKey: ['Type'],
      sortBy: 'Type',
      getParams: function (rec) {
        return {
          type: rec.get('Type')
        };
      },
      translationsUrl: Concentrator.route('GetTranslations', 'ProductType'),
      translationsUrlUpdate: Concentrator.route('SetTranslation', 'ProductType'),
      translationsGridStructure: [
        { dataIndex: 'type', type: 'string' },
        { dataIndex: 'Language', type: 'string', header: 'Language' },
        { dataIndex: 'Name', type: 'string', header: 'Name', editor: { xtype: 'textfield' } },
        { dataIndex: 'LanguageID', type: 'int' }
      ],
      url: Concentrator.route('GetAll', 'ProductType'),
      updateUrl: Concentrator.route('Update', 'ProductType'),
      deleteUrl: Concentrator.route('Delete', 'ProductType'),
      newUrl: Concentrator.route('Create', 'ProductType'),
      permissions: {
        list: 'Default',
        create: 'Default',
        remote: 'Default',
        update: 'Default'
      },
      structure: [
										{ dataIndex: 'Type', type: 'string', header: 'Type', editor: { xtype: 'textfield', allowBlank: false }, editable: false },										
										{ dataIndex: 'IsBra', type: 'boolean', header: 'Is Bra', editor: { xtype: 'checkbox', allowBlank: false } },
                    {
                      dataIndex: 'ProductType', type: 'string', header: 'Group', editor:
                      {
                        xtype: 'combo',
                        typeAhead: true,
                        triggerAction: 'all',
                        lazyRender: true,
                        mode: 'local',
                        store: new Ext.data.ArrayStore({
                          id: 0,
                          fields: [
                              'ProductType',
                              'DisplayText'
                          ],
                          data: [['Tops', 'Tops'], ['Bottoms', 'Bottoms'], ['', 'Unknown/Other']]
                          }),
                        valueField: 'ProductType',
                        displayField: 'DisplayText'
                      }
                    }                    
      ]

    });
    return grid;
  }
});