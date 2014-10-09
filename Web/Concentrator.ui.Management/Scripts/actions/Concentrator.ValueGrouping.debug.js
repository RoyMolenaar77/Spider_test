Concentrator.ValueGrouping = Ext.extend(Concentrator.GridAction, {

  getPanel: function () {
    var button = new Concentrator.ui.ToggleMappingsButton({
      roles: 'Default',
      grid: function () {
        return grid;
      },
      mappingField: 'isMatched',
      unmappedValue: 'false',
      unmappedText: 'Show ungrouped values'
    });

    var grid = new Diract.ui.TranslationGrid({
      getParams: function (rec) {
        return {
          AttributeID: rec.get('AttributeID'),
          Value: rec.get('Value')
        };
      },
      translationsUrl: Concentrator.route('GetTranslations', 'ProductAttributeValueTranslation'),
      translationsUrlUpdate: Concentrator.route('SetTranslation', 'ProductAttributeValueTranslation'),
      translationsGridStructure: [
            { dataIndex: 'Language', type: 'string', header: 'Language' },
            { dataIndex: 'Translation', type: 'string', header: 'Name', editor: { xtype: 'textfield'} },
            { dataIndex: 'LanguageID', type: 'int' },
            { dataIndex: 'AttributeID', type: 'int' }

             ],
      translationsGridPrimaryKey: ['AttributeID', 'Value', 'LanguageID'],
      translationsGridSortBy: 'AttributeID',
      singularObjectName: 'ungrouped attribute value',
      singularObjectName: 'ungrouped attribute values',
      permissions: { 
				list: 'GetValueGrouping',
				create: 'CreateValueGrouping',
      	remote: 'DeleteValueGrouping',
      	update: 'UpdateValueGrouping'
      },
      url: Concentrator.route('GetUngrouped', 'ProductAttributeValue'),
      updateUrl: Concentrator.route('AddValueToGroup', 'ProductAttributeValue'),
      primaryKey: ['AttributeID', 'Value'],
      customButtons: [button],
      onGridFilterInitialized: function () {
        button.toggleClass(true);
      },
      autoLoadStore: false,
      structure: [
      //product
      {dataIndex: 'Value', type: 'string', header: 'Value' },
      { dataIndex: 'AttributeID', type: 'int', header: 'Attribute', renderer: function (val, m, rec) { return rec.get('Attribute') }, editable: false,
        editor: { xtype: 'attribute' }, filter: { type: 'string', filterField: 'Attribute' }
      },
      { dataIndex: 'Attribute', type: 'string' },
       { dataIndex: 'isMatched', type: 'boolean', header: 'Grouped' }
      ],
      rowActions: [
        {
          text: 'Manage groups',
          iconCls: 'package',
          handler: function (record) {
            var groupGrid = new Concentrator.ui.Grid({
              singularObjectName: 'ungrouped attribute value',
              singularObjectName: 'ungrouped attribute values',
              permissions: { all: 'Default' },
              url: Concentrator.route('GetValueGroups', 'ProductAttributeValue'),
              deleteUrl: Concentrator.route('DeleteValueGroup', 'ProductAttributeValue'),
              newUrl: Concentrator.route('AddValueToGroup', 'ProductAttributeValue'),
              primaryKey: ['AttributeValueGroupID', 'Value', 'AttributeID'],
              params: {
                value: record.get('Value'),
                attributeID: record.get('AttributeID')
              },
              newParams: {
                value: record.get('Value'),
                attributeID: record.get('AttributeID')
              },
              structure: [
                   { dataIndex: 'AttributeValueGroupID', type: 'int', header: 'Value group', renderer: function (val, m, rec) { return rec.get('AttributeValueGroup') }, editable: false,
                     editor: { xtype: 'attributeValueGroup' }, filter: { type: 'string', filterField: 'AttributeValueGroup' }
                   }
                   ,
                  { dataIndex: 'AttributeValueGroup', type: 'string' },
                  { dataIndex: 'Value', type: 'string' },
                  { dataIndex: 'AttributeID', type: 'int' }
                ]
            });
            var window = new Ext.Window({
              title: 'Groups',
              layout: 'fit',
              height: 319,
              width: 621,
              items: groupGrid,
              listeners: {
                'close': function () {
                  grid.store.reload();
                }
              }
            });
            window.show();

          }
        }
      ]

    });
    return grid;
  }
});



