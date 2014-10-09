Concentrator.ProductGroupVendors = Ext.extend(Concentrator.GridAction, {

  getPanel: function () {

    var btn = new Concentrator.ui.ToggleMappingsButton({
      grid: function () { return grid; },
      mappingField: 'ProductGroupID',
      unmappedValue: -1,
      pluralObjectName: 'vendor product groups'
    });

    var grid = new Concentrator.ui.Grid({
      pluralObjectName: 'Product Group Vendors',
      singularObjectName: 'Product Group Vendor',
      primaryKey: ['ProductGroupVendorID'],
      sortBy: 'ProductGroupID',
      autoLoadStore: false,
      url: Concentrator.route("GetList", "ProductGroupVendor"),
      customButtons: [btn],
      onGridFilterInitialized: function () {
        btn.toggleClass(true);
      },
      permissions: {
        list: 'GetProductGroupVendor',
        create: 'CreateProductGroupVendor',
        remove: 'DeleteProductGroupVendor',
        update: 'UpdateProductGroupVendor'
      },
      hasSearchBox: true,
      height: 400,
      width: 600,
      updateUrl: Concentrator.route("Update", "ProductGroupVendor"),
      newUrl: Concentrator.route('Create', 'ProductGroupVendor'),
      deleteUrl: Concentrator.route('Delete', 'ProductGroupVendor'),
      structure: [
                { dataIndex: 'ProductGroupVendorID', type: 'int' },

                { dataIndex: 'ProductGroupID', type: 'int', header: "Product Group", width: 200,
                  renderer: function (val, m, record) {
                    return record.get('ProductGroupName');
                  },
                  filter: {
                    type: 'string',
                    filterField: 'ProductGroupName'
                  },
                  editor: {
                    xtype: 'productgroup',
                    listeners: {
                      'select': (function (val, meta, data) {

                        btn.handleParameter(meta.get('ProductGroupID'));

                      }).createDelegate(this)
                    }
                  }
                },
                { dataIndex: 'ProductGroupName', type: 'string' },

                { dataIndex: 'VendorID', type: 'int', header: "Vendor", width: 45, renderer: Concentrator.renderers.field('vendors', 'VendorName'),
                  filter: {
                    type: 'list',
                    labelField: 'VendorName',
                    store: Concentrator.stores.vendors
                  },
                  editable: false,
                  editor: { xtype: 'vendor', allowBlank: false }
                },
                { dataIndex: 'BrandCode',
                  header: 'Brand Code',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupName',
                  header: 'Vendor Name',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: false }
                },
                { dataIndex: 'VendorProductGroupCode1',
                  header: 'Product Group Code 1',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCode2',
                  header: 'Product Group Code 2',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCode3',
                  header: 'Product Group Code 3',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCode4',
                  header: 'Product Group Code 4',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCode5',
                  header: 'Product Group Code 5',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCod6',
                  header: 'Product Group Code 6',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCode7',
                  header: 'Product Group Code 7',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCode8',
                  header: 'Product Group Code 8',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCode9',
                  header: 'Product Group Code 9',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorProductGroupCode10',
                  header: 'Product Group Code 10',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                }


      ],
      rowActions: [
        {
          text: 'View associated products',
          iconCls: 'merge',
          handler: function (record) {

            var grid = new Diract.ui.Grid({
              pluralObjectName: 'Associated products',
              singularObjectName: 'Associated product',
              url: Concentrator.route("ViewAssociatedVendorProducts", "ProductGroupVendor"),
              params: {
                ProductGroupVendorID: record.get('ProductGroupVendorID')
              },
              primaryKey: 'ProductID',
              permissions: {
                list: 'GetProductGroupVendor'
              },
              structure: [
                 { dataIndex: 'ProductGroupVendorID', type: 'int' },
                { dataIndex: 'ProductID', type: 'int', header: 'Product',
                  renderer: function (val, m, rec) {
                    return rec.get('ProductName');
                  }
                },
                { dataIndex: 'ProductName', type: 'string' },
                { dataIndex: 'BrandID', type: 'int', header: 'Brand',
                  filter: {
                    type: 'string',
                    filterField: 'BrandName'
                  },
                  renderer: function (val, metadata, record) {
                    return record.get('BrandName');
                  }
                },
                { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor Item Number' },
                { dataIndex: 'CustomItemNumber', type: 'string', header: 'Custom Item Number' },
                { dataIndex: 'VendorName', type: 'string', header: 'VendorName' },
                { dataIndex: 'BrandName', type: 'string' },
                { dataIndex: 'CreationTime', type: 'date', header: 'Creation Time' }
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
              width: 1000,
              height: 400,
              modal: true,
              layout: 'fit',
              items: [
                grid
              ]
            });


            window.show();
          }
        }
      ],
      windowConfig: {
        height: 450,
        width: 500
      }
    });

    return grid;
  }
});
