Concentrator.ContentProduct = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {
    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Product content',
      pluralObjectName: 'Product contents',
      primaryKey: ['ProductContentID'],
      sortBy: 'ProductContentID',
      url: Concentrator.route('GetList', 'ContentProduct'),
      updateUrl: Concentrator.route('Update', 'ContentProduct'),
      deleteUrl: Concentrator.route('Delete', 'ContentProduct'),
      newUrl: Concentrator.route('Create', 'ContentProduct'),
      permissions: {
        list: 'GetContentProduct',
        create: 'CreateContentProduct',
        remove: 'DeleteContentProduct',
        update: 'UpdateContentProduct'
      },
      structure: [
             { dataIndex: 'ConnectorID', type: 'int', header: 'Connector',

               renderer: Concentrator.renderers.field('connectors', 'Name'),

               filter: {
                 type: 'list',
                 labelField: 'Name',
                 store: Concentrator.stores.connectors
               },
               editor:
               {
                 xtype: 'connector'
               }
             },
             { dataIndex: 'ProductContentID', type: 'int' },
             { dataIndex: 'VendorID', type: 'int', header: 'Vendor', renderer: Concentrator.renderers.field('vendors', 'VendorName'),
               filter: {
                 type: 'list',
                 store: Concentrator.stores.vendors,
                 labelField: 'VendorName'
               },
               editor: {
                 xtype: 'vendor'
               }
             },
             { dataIndex: 'ProductGroupID', type: 'int', header: 'Product Group',
               renderer: function (val, m, rec) {
                 return rec.get('ProductGroupName');
               },
               filter: {
                 type: 'string',
                 filterField: 'ProductGroupName'
               }
               , editor: {
                 xtype: 'productgroup'
               }

             },
             { dataIndex: 'ProductDescription', type: 'string' },

             { dataIndex: 'ProductGroupName', type: 'string' },

              { dataIndex: 'ProductID', type: 'int', header: 'Product',
                renderer: function (val, m, rec) {
                  return rec.get('ProductDescription');
                },
                editor: { xtype: 'product', allowBlank: true }
              },
      // { dataIndex: 'ProductDescription', type: 'string' }

              {dataIndex: 'BrandID', type: 'int', header: 'Brand',
              filter: {
                type: 'string',
                filterField: 'BrandName'
              },

              editor: { xtype: 'brand', hiddenName: 'BrandID' },
              renderer: function (val, metadata, record) {
                return record.get('BrandName');
              },
              width: 150
            },
                { dataIndex: 'BrandName', type: 'string' },
                { dataIndex: 'ProductContentIndex', type: 'int', header: 'Product Content Index', editable: true, editor: { xtype: 'int', allowBlank: false} },
                { dataIndex: 'IsAssortment', type: 'boolean', header: 'Is Assortment', editable: true, editor: { xtype: 'boolean', allowBlank: false} }
      ],
      rowActions: [
        {
          text: 'View Products',
          iconCls: 'default', //TODO: Change Icon
          roles: ['GetContentProduct'],
          handler: function (record) {

            var grid = new Diract.ui.Grid({
              pluralObjectName: 'Products',
              singularObjectName: 'Product',
              primaryKey: 'ProductID',
              url: Concentrator.route("ViewProducts", "ContentProduct"),
              params: {
                productContentID: record.get('ProductContentID')
              },
              permissions: { all: 'Default' },
              structure: [
                { dataIndex: 'ProductID', type: 'int' },
                { dataIndex: 'ShortDescription', type: 'string', header: 'Short Description' },
                { dataIndex: 'LongDescription', type: 'string', header: 'Long Description' },
                { dataIndex: 'LineType', type: 'string', header: 'Line Type' },
                { dataIndex: 'CreationTime', type: 'date', header: 'Creation Time' },
                { dataIndex: 'LastModificationTime', type: 'date', header: 'Last Modification Time' },
                { dataIndex: 'CustomItemNumber', type: 'string', header: 'Custom Item Number' }
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
              title: 'Product Info',
              width: 1000,
              height: 450,
              modal: true,
              layout: 'fit',
              items: [grid]
            });

            window.show();

          }
        }
      ]

    });
    return grid;
  }
});