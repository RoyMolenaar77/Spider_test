Concentrator.ui.PriceRule = Ext.extend(Ext.Panel, {
  title: 'Price rule',
  iconCls: 'molecule-preferences',
  layout: 'fit',

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;

    Concentrator.ui.PriceRule.superclass.constructor.call(this, config);
  },

  refresh: function (pgmID, nodeText) {

    var id = pgmID;

    if (!this.priceRuleGrid) {

      this.priceRuleGrid = new Diract.ui.Grid({
        singularObjectName: 'Content price',
        pluralObjectName: 'Content prices',
        primaryKey: 'ContentPriceRuleID',
        sortBy: 'ContentPriceRuleID',
        windowConfig: { height: 466 },
        url: Concentrator.route('GetProductGroupList', 'ContentPrice'),
        newUrl: Concentrator.route("CreatePerProductGroup", "ContentPrice"),
        updateUrl: Concentrator.route("Update", "ContentPrice"),
        deleteUrl: Concentrator.route("Delet", "ContentPrice"),
        params: {
          productGroupMappingID: id
          // connectorID: getConnectorID
        },
        newParams: {
          productGroupMappingID: id
        },
        permissions: {
          list: 'GetContentPrice',
          create: 'CreateContentPrice',
          remove: 'DeleteContentPrice',
          update: 'UpdateContentPrice'
        },
        structure: [
            { dataIndex: 'ContentPriceRuleID', type: 'int' },
            { dataIndex: 'VendorID',
              type: 'int',
              header: 'Vendor',
              renderer: Concentrator.renderers.field('vendors', 'VendorName'),
              filter: { type: 'list', store: Concentrator.stores.vendors, labelField: 'VendorName' },
              editor: { xtype: 'vendor', allowBlank: false }
            },
            { dataIndex: 'ConnectorID', type: 'int',
              header: 'Connector',
              renderer: Concentrator.renderers.field('connectors', 'Name'),
              filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' },
              editor: { xtype: 'connector', allowBlank: false }
            },
            { dataIndex: 'ProductGroupName', type: 'string' },
            { dataIndex: 'ProductGroupID', type: 'int',
              header: 'Product Group',
              renderer: function (val, m, rec) { return rec.get('ProductGroupName') },
              filter: { type: 'string', filterField: 'ProductGroupName' }
              //editor: { xtype: 'productgroup'}
            },
            { dataIndex: 'BrandName', type: 'string' },
            { dataIndex: 'BrandID',
              type: 'int',
              header: 'Brand',
              renderer: function (val, m, rec) {
                return rec.get('BrandName');
              },
              filter: { type: 'string', filterField: 'BrandName' },
              editor: {
                xtype: 'brand',
                allowBlank: true
              }
            },
            { dataIndex: 'ProductID', type: 'int', header: 'Product',
              renderer: function (val, m, rec) {
                return rec.get('ProductDescription');
              },
              editor: { xtype: 'product', allowBlank: true, id: 'product-field' }
            },
            { dataIndex: 'ProductDescription', type: 'string' },
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
            { dataIndex: 'FixedPrice', type: 'float', header: 'Fixed price',
              editor: {
                xtype: 'numberfield',
                allowBlank: true,
                vtype: 'otherIsNotNull',
                otherField: 'product-field'
              }
            },
            { dataIndex: 'MinimumQuantity', header: 'Minimum Quantity', type: 'int', editor: { xtype: 'numberfield', allowBlank: true} },
            { dataIndex: 'ContentPriceRuleIndex', type: 'int', header: 'Content Price Rule Index', editable: true, editor: { xtype: 'numberfield', allowBlank: false} },
            { dataIndex: 'PriceRuleType', type: 'int', header: 'Price Rule Type',
              renderer: Concentrator.renderers.field('priceRuleTypes', 'Name'),
              editor: {
                xtype: 'priceruletypes',
                allowBlank: false
              },
              filter: {
                type: 'list',
                store: Concentrator.stores.priceRuleTypes,
                labelField: 'Name'
              }
            },
            { dataIndex: 'ContentPriceCalculationID', type: 'int', renderer: function (val, m, rec) { return rec.get('ContentPriceCalculation'); }, header: 'Content Price Calculation', editor: { xtype: 'priceRounding'} },
            { dataIndex: 'ContentPriceCalculation', type: 'string' }
          ]
      });

      if (!this.manager) {

        this.manager = new Concentrator.ui.PricingManager({
          connectorID: this.connectorID,
          productGroupMappingID: id,
          productGroupLabel: nodeText
        });

        this.add(this.manager);
      }
      
      // TODO: make this more dynamic!
      this.manager.items.items[1].add(this.priceRuleGrid);
    }

    this.priceRuleGrid.store.reload({
      params: {
      //productGroupID: id
    }
  });

  this.doLayout();
}
});