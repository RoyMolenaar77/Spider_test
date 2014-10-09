Concentrator.VendorPriceRules = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {
    var grid = new Diract.ui.Grid({
      primaryKey: 'VendorPriceRuleID',
      singularObjectName: 'Vendor price rule',
      pluralObjectName: 'VendorPriceRules',
      url: Concentrator.route('GetList', 'VendorPriceRule'),
      updateUrl: Concentrator.route('Update', 'VendorPriceRule'),
      deleteUrl: Concentrator.route('Delete', 'VendorPriceRule'),
      newUrl: Concentrator.route('Create', 'VendorPriceRule'),
      permissions: {
        list: 'GetVendorPriceRule',
        create: 'CreateVendorPriceRule',
        update: 'UpdateVendorPriceRule',
        remove: 'DeleteVendorPriceRule'
      },

      windowConfig: {
        width: 400,
        height: 432
      },

      structure: [
        { dataIndex: 'VendorPriceRuleID', type: 'int' },
        { dataIndex: 'VendorID', type: 'int', renderer: Concentrator.renderers.field('vendors', 'VendorName'), header: 'Vendor',
          filter: { type: 'list', store: Concentrator.stores.vendors, labelField: 'VendorName' },
          editor: { xtype: 'vendor', allowBlank: false }
        },
        { dataIndex: 'ProductID', type: 'int',
          renderer: function (val, m, rec) { return rec.get('Product') }, header: 'Product',
          editor: { xtype: 'product', allowBlank: true, id: 'product-field' }

        },
        { dataIndex: 'Product', type: 'string' },
        { dataIndex: 'ProductGroupID', type: 'int', renderer: function (val, m, rec) { return rec.get('ProductGroupName') },
          filter: { type: 'string', filterField: 'ProductGroupName' },
          editor: { xtype: 'productgroup', allowBlank: true },
          header: 'Product group'
        },

        { dataIndex: 'ProductGroupName', type: 'string' },
        { dataIndex: 'BrandID', type: 'int',
          filter: { type: 'string', filterField: 'BrandName' },
          editor: {
            xtype: 'brand', allowBlank: true
          },
          header: 'Brand', renderer: function (val, m, rec) { return rec.get('BrandName') }
        },
        { dataIndex: 'BrandName', type: 'string' },
        { dataIndex: 'Margin', type: 'string', header: 'Operator',
          editor: {
            xtype: 'signs',
            allowBlank: false,
            fieldLabel: 'Multiplier'
          },
          filter: {
            type: 'list',
            store: Concentrator.stores.signs,
            labelField: 'display'
          }
        },
        { dataIndex: 'UnitPriceIncrease', type: 'float', header: 'Unit price increase (1.01 = 1%)',
          editor: {
            xtype: 'numberfield',
            allowBlank: true,
            decimalPrecision: 3
          }
        },
        { dataIndex: 'CostPriceIncrease', type: 'float', header: 'Cost price increase (1.01 = 1%)',
          editor: {
            xtype: 'numberfield',
            allowBlank: true,
            decimalPrecision: 3
          }
        },
        { dataIndex: 'MinimumQuantity', header: 'Minimum Quantity', type: 'int', editor: { xtype: 'numberfield', allowBlank: true} },
        { dataIndex: 'ContentPriceRuleIndex', type: 'int', header: 'Vendor Price Rule Index', editable: true, editor: { xtype: 'numberfield', allowBlank: false} },
        { dataIndex: 'PriceRuleType', type: 'int', header: 'Price Rule Type',
          renderer: Concentrator.renderers.field('priceRuleTypes', 'Name'),
          editor: {
            xtype: 'priceruletypes',
            allowBlank: true
          }, filter: {
            type: 'list',
            store: Concentrator.stores.priceRuleTypes,
            labelField: 'Name'
          }
        },
         { dataIndex: 'VendorPriceCalculationID', type: 'int', header: 'Rounding calculation',
           editor: { xtype: 'vendorPriceRounding' }, renderer: function (val, m, rec) {
             return rec.get('VendorPriceCalculationName');
           }
         },
        { dataIndex: 'VendorPriceCalculationName', type: 'string' }
      ]
    });


    return grid;
  }

}); 