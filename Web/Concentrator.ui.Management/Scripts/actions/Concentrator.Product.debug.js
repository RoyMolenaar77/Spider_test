Concentrator.Products = Ext.extend(Concentrator.GridAction, {
  constructor: function (config)
  {
    Ext.apply(this, config);

    Concentrator.Products.superclass.constructor.call(this, config);
  },
  getPanel: function ()
  {
    var self = this;

    var mandatoryFields = [];
    var fields = Concentrator.getMandatoryFields();

    if (fields.length > 0)
    {
      mandatoryFields = fields;
    }

    var that = this;

    var btn = new Concentrator.ui.ToggleMappingsButton({
      grid: function () { return productsGrid; },
      mappingField: 'IsConfigurable',
      unmappedValue: true,
      unmappedText: 'Show configurable products'
    });

    //debugger;

    var productsGrid = new Diract.ui.Grid({
      primaryKey: ['ProductID'],
      singularObjectName: 'Product',
      pluralObjectName: 'Products',
      sortBy: 'ProductID',
      customButtons: [btn],
      height: 400,
      autoLoadStore: false,
      width: 600,
      mandatoryFields: fields,
      url: Concentrator.route('GetList', 'Product'),
      newUrl: Concentrator.route('Create', 'Product'),
      deleteUrl: Concentrator.route('Delete', 'Product'),
      params: this.params,      
      addMenuActions: [new Ext.Action({
        text: "Customized",
        tooltip: 'Add customized product',
        iconCls: "add",
        scope: this,
        handler: function () {

            var productWizard = new Concentrator.ui.ProductWizard({
              creationAction: function ()
              {
                window.destroy();
                that.productsGrid.store.reload();
              }
            });

            var window = new Ext.Window({
              title: 'Create a product',
              width: 1200,
              height: 680,
              layout: 'fit',
              items: productWizard,
              modal: true
            });

            window.show();
        }
      })],
      createCallback: function (store, response)
      {
        var id = response.ProductID;
        var factory = new Concentrator.ProductBrowserFactory({ productID: id });
      },     
      permissions: {
        list: 'GetProduct',
        create: 'CreateProduct',
        remove: 'DeleteProduct',
        update: 'UpdateProduct'
      },
      structure: [
  { dataIndex: 'ProductID', type: 'int', header: 'Identifier' },
  { dataIndex: 'VendorItemNumber', type: 'string', header: Concentrator.VendorItemNumberField, editor: { xtype: 'textfield' }, editable: false },
  { dataIndex: 'CustomItemNumber', type: 'string', header: Concentrator.CustomItemNumberField, editor: { xtype: 'textfield' }, editable: false },
  { dataIndex: 'ProductName', type: 'string', header: 'Name', editor: { xtype: 'textfield' }, editable: false },
  { dataIndex: 'ProductDescription', type: 'string', header: 'Description', editor: { xtype: 'textarea' }, editable: false },
  { dataIndex: 'Quality', type: 'string', header: 'Quality' },
  { dataIndex: 'Barcode', type: 'string', header: 'Barcode' },
  { dataIndex: 'ProductID', type: 'string', editor: { xtype: 'productgroup', fieldLabel: 'Product group' } },
  { dataIndex: 'IsConfigurable', type: 'boolean', header: 'Configurable' },
  {
    dataIndex: 'BrandID', type: 'int', header: 'Brand',
    filter: {
      type: 'string',
      filterField: 'BrandName'
    },

    editor: { xtype: 'brand', hiddenName: 'BrandID' }, editable: false,
    renderer: function (val, metadata, record)
    {
      return record.get('BrandName');
    },
    width: 150
  },
  { dataIndex: 'BrandName', type: 'string' },
  {
    dataIndex: 'VendorID', type: 'int', header: 'Source vendor',
    renderer: Concentrator.renderers.field('vendors', 'VendorName'),
    filter: {
      type: 'list',
      store: Concentrator.stores.vendors,
      labelField: 'VendorName'
    }
  }
      ],
      listeners: {
        'celldblclick': (function (grid, rowIndex, columnIndex, evt)
        {
          var rec = grid.store.getAt(rowIndex);
          var productID = rec.get('ProductID');

          var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
        }).createDelegate(this)
      },
      windowConfig: {
        height: 300
      }
    });

    return productsGrid;
  },

  refresh: function (params)
  {

  }
});