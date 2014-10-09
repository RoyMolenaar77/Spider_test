Concentrator.AttributeValueGroups = Ext.extend(Concentrator.BaseAction, {
  getPanel: function () {

    var gr = new Diract.ui.Grid({
      singularObjectName: 'value group',
      pluralObjectName: 'value groups',
      permissions: { all: 'Default' },
      url: Concentrator.route('GetList', 'ConnectorAttributeValueGroup'),
      newUrl: Concentrator.route('Create', 'ConnectorAttributeValueGroup'),
      deleteUrl: Concentrator.route('Delete', 'ConnectorAttributeValueGroup'),
      primaryKey: ['ConnectorID', 'AttributeValueGroupID', 'AttributeID'],
      sortBy: 'ConnectorID',

      structure: [
        { dataIndex: 'ConnectorID', type: 'int', header: 'Connector', renderer: function (val, m, rec) { return rec.get('Connector') }, editable: false, editor: { xtype: 'connector' },
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' }
        },
        { dataIndex: 'Connector', type: 'string' },

        { dataIndex: 'AttributeID', type: 'int', header: 'Attribute', renderer: function (val, m, rec) { return rec.get('Attribute') }, editable: false,
          editor: { xtype: 'attribute' }, filter: { type: 'string', filterField: 'Attribute' }
        },
        { dataIndex: 'Attribute', type: 'string' },

        { dataIndex: 'AttributeValueGroupID', type: 'int', header: 'Value group', renderer: function (val, m, rec) { return rec.get('AttributeValueGroup') }, editable: false,
          editor: { xtype: 'attributeValueGroup' }, filter: { type: 'string', filterField: 'AttributeValueGroup' }
        },
        { dataIndex: 'AttributeValueGroup', type: 'string' }
      ],

      rowActions: [
          {
            text: 'View associated attribute values',
            iconCls: 'box-view',
            handler: function (record) {
              var grid = new Concentrator.ui.Grid({
                singularObjectName: 'Attribute value',
                pluralObjectName: 'Attribute values',
                permissions: { all: 'Default' },
                primaryKey: 'AttributeValueID',
                url: Concentrator.route('GetGroupedValues', 'ProductAttributeValue'),
                params: {
                  valueGroupID: record.get("AttributeValueGroupID"),
                  attributeID: record.get("AttributeID")
                },

                deleteUrl: Concentrator.route('RemoveValueFromGroup', 'ProductAttributeValue'),
                deleteButtonText: 'Remove from group',
                structure: [
                                { dataIndex: 'ProductID', type: 'int', header: 'Item number', renderer: function (val, m, rec) {
                                  return rec.get('Product');
                                }
                                },
                                { dataIndex: 'Product', type: 'string' },
                                { dataIndex: 'ProductAttributeValueID', type: 'int' },
                                { dataIndex: 'Value', type: 'string' }
                           ],
                listeners: {
                  'celldblclick': (function (grid, rowIndex, columnIndex, evt) {
                    var rec = grid.store.getAt(rowIndex);
                    var productID = rec.get('ProductID');

                    var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
                  }).createDelegate(this)
                }

              });

              var window = new Ext.Window({
                layout: 'fit',
                height: 400,
                width: 400,
                items: grid,
                modal: true
              });

              window.show();
            }
          }
        ]
    });

    return gr;
  }

});