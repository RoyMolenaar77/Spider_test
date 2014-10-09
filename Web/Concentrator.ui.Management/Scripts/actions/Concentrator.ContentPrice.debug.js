Concentrator.ContentPrice = Ext.extend(Concentrator.BaseAction, {

  getPanel: function () {

    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Content price',
      pluralObjectName: 'Content prices',
      primaryKey: 'ContentPriceRuleID',
      sortBy: 'ContentPriceRuleID',
      windowConfig: { height: 466, width: 483 },
      url: Concentrator.route('GetList', 'ContentPrice'),
      updateUrl: Concentrator.route('Update', 'ContentPrice'),
      newUrl: Concentrator.route('Create', 'ContentPrice'),
      deleteUrl: Concentrator.route('Delete', 'ContentPrice'),
      permissions: {
        list: 'GetContentPrice',
        create: 'CreateContentPrice',
        remove: 'DeleteContentPrice',
        update: 'UpdateContentPrice'
      },
      structure: [
        { dataIndex: 'ContentPriceRuleID', type: 'int' },
        { dataIndex: 'ContentPriceLabel', type: 'string', header: 'Description', editor: { xtype: 'textfield' }, defaultValue: 'Add a label' },
        {
          dataIndex: 'VendorID',
          type: 'int',
          header: 'Vendor',
          renderer: Concentrator.renderers.field('vendors', 'VendorName'),
          filter: { type: 'list', store: Concentrator.stores.vendors, labelField: 'VendorName' },
          editor: { xtype: 'vendor', allowBlank: false }
        },
        {
          dataIndex: 'ConnectorID', type: 'int',
          header: 'Connector',
          renderer: Concentrator.renderers.field('connectors', 'Name'),
          filter: { type: 'list', store: Concentrator.stores.connectors, labelField: 'Name' },
          editor: { xtype: 'connector', allowBlank: false }
        },
        { dataIndex: 'ProductGroupName', type: 'string' },
        {
          dataIndex: 'ProductGroupID', type: 'int',
          header: 'Product Group',
          renderer: function (val, m, rec) { return rec.get('ProductGroupName'); },
          filter: { type: 'string', filterField: 'ProductGroupName' },
          editor: { xtype: 'productgroup', allowBlank: true }
        },
        { dataIndex: 'BrandName', type: 'string' },
        {
          dataIndex: 'BrandID',
          type: 'int',
          header: 'Brand',
          renderer: function (val, m, rec) {
            return rec.get('BrandName');
          },
          filter: { type: 'string', filterField: 'BrandName' },
          editor: {
            xtype: 'brand', allowBlank: true
          }
        },
        {
          dataIndex: 'ProductID', type: 'int', header: 'Product',
          renderer: function (val, m, rec) {
            return rec.get('ProductDescription');
          },
          editor: { xtype: 'product', allowBlank: true, id: 'product-field' }
        },
        { dataIndex: 'ProductDescription', type: 'string' },
        {
          dataIndex: 'Margin', type: 'string', header: 'Operator',
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
        {
          dataIndex: 'UnitPriceIncrease', type: 'float', header: 'Unit price increase (1.01 = 1%)',
          editor: {
            xtype: 'numberfield',
            allowBlank: true,
            decimalPrecision: 3
          }
        },
        {
          dataIndex: 'CostPriceIncrease', type: 'float', header: 'Cost price increase (1.01 = 1%)',
          editor: {
            xtype: 'numberfield',
            allowBlank: true,
            decimalPrecision: 3
          }
        },
        {
          dataIndex: 'FixedPrice', type: 'float', header: 'Fixed price',
          editor: {
            xtype: 'numberfield',
            allowBlank: true//,
            //vtype: 'otherIsNotNull',
            //otherField: 'product-field'
          }
        },
        { dataIndex: 'MinimumQuantity', header: 'Minimum Quantity', type: 'int', editor: { xtype: 'numberfield', allowBlank: true } },
        { dataIndex: 'ContentPriceRuleIndex', type: 'int', header: 'Content Price Rule Index', editable: true, editor: { xtype: 'numberfield', allowBlank: false } },
        {
          dataIndex: 'PriceRuleType', type: 'int', header: 'Price Rule Type',
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
        {
          dataIndex: 'ContentPriceCalculationID', type: 'int', header: 'Content Price Calculation',
          editor: { xtype: 'priceRounding' }, renderer: function (val, m, rec) {
            return rec.get('ContentPriceCalculationName');
          }
        },
        { dataIndex: 'ContentPriceCalculationName', type: 'string' },
        {
          dataIndex: 'FromDate', type: 'date', header: 'From Date', editor: {
            xtype: 'datefield', format: 'Y-m-d H:i:s', allowBlank: true
          }, renderer: Ext.util.Format.dateRenderer('Y-m-d')
        },
        {
          dataIndex: 'ToDate', type: 'date', header: 'To Date', editor: {
            xtype: 'datefield', format: 'Y-m-d H:i:s', allowBlank: true
          }, renderer: Ext.util.Format.dateRenderer('Y-m-d')
        },
        {
          dataIndex: 'AttributeID', header: 'Attribute', type: 'int',
          renderer: function (val, m, rec) {
            return rec.get('AttributeName');
          },
          editor: { xtype: 'attribute', allowBlank: true }
        },
        {
          dataIndex: 'AttributeValue', header: 'Attribute Value', type: 'string',
          editor: { xtype: 'textfield' }
        },
        { dataIndex: 'AttributeName', type: 'string' }
      ]
    });

    return grid;
  }
});