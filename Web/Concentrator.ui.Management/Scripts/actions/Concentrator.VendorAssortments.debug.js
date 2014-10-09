Concentrator.VendorAssortments = Ext.extend(Concentrator.GridAction, {

  getPanel: function() {
    var self = this;
    var grid = new Concentrator.ui.Grid({
      singularObjectName: 'Vendor Assortment',
      pluralObjectName: 'Vendor Assortments',
      primaryKey: 'VendorAssortmentID',
      sortBy: 'VendorAssortmentID',
      permissions: {
        list: 'GetVendorAssortment',
        update: 'UpdateVendorAssortment'
      },
      url: Concentrator.route('GetList', 'VendorAssortment'),
      updateUrl: Concentrator.route('SetActive', 'VendorAssortment'),
      structure: [
      { dataIndex: 'VendorAssortmentID', type: 'int' },
      { dataIndex: 'ProductID', type: 'int', header: 'Concentrator identifier' },
      { dataIndex: 'VendorItemNumber', type: 'string', header: Concentrator.VendorItemNumberField },
      //      { dataIndex: 'ProductName', type: 'string', header: 'Product' },
      { dataIndex: 'CustomItemNumber', type: 'string', header: Concentrator.CustomItemNumberField },
      { dataIndex: 'VendorID', type: 'int', header: 'Vendor', renderer: Concentrator.renderers.field('vendors', 'VendorName'), filter: { type: 'list', store: Concentrator.stores.vendors, labelField: 'VendorName'} },
      { dataIndex: 'ShortDescription', type: 'string', header: 'Description' },
      { dataIndex: 'LineType', type: 'string', header: 'Line Type' },
      { dataIndex: 'QuantityOnHand', type: 'string', header: 'Quantity on hand (all locations)' },
        { dataIndex: 'PromisedDeliveryDate', type: 'date', header: 'Delivery date', renderer: Ext.util.Format.dateRenderer('d-m-Y') },
        { dataIndex: 'QuantityToReceive', type: 'int', header: 'Quantity to receive' },
      //{ dataIndex: 'VendorStatus', type: 'string', header: 'Vendor Status' },
        {dataIndex: 'UnitCost', type: 'float', header: 'Unit cost' },
        {dataIndex: 'UnitPrice', type: 'float', header: 'Unit price' },
        { dataIndex: 'IsActive', type: 'boolean', header: 'Is Active', editable: true, editor: { xtype: 'boolean'} }
    ],
      listeners: {
        'celldblclick': (function(grid, rowIndex, columnIndex, evt) {
          var rec = grid.store.getAt(rowIndex);
          var productID = rec.get('ProductID');

          var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
        }).createDelegate(this)
      },
      rowActions: [
        {
          iconCls: 'cabinet',
          text: 'View Details',
          handler: function(record) {
            self.getDetails(record.get('VendorAssortmentID'));
          }
        }
    ]
    });
    return grid;
  },

  getDetails: function(vendorAssortmentID) {

    var self = this;
    var priceGrid = new Concentrator.ui.Grid({
      singularObjectName: 'Vendor price',
      pluralObjectName: 'Vendor prices',
      primaryKey: ['VendorAssortmentID', 'Price', 'MinimumQuantity'],
      sortBy: 'VendorAssortmentID',
      //updateUrl: Concentrator.route("UpdateVendor", "VendorPrice"),
      permissions: {
        list: 'GetVendorPrice'
      },
      height: 240,
      url: Concentrator.route('GetList', 'VendorPrice'),
      params: { vendorAssortmentID: vendorAssortmentID },
      structure: [
        { dataIndex: 'VendorAssortmentID', type: 'int' },
        { dataIndex: 'Price', type: 'float', header: 'Price' },
        { dataIndex: 'CostPrice', type: 'float', header: 'Cost Price' },
        { dataIndex: 'TaxRate', type: 'float', header: 'Tax Rate', 
          renderer: function(val) {
            return Ext.util.Format.number(val, '0%');
          }
        },
        { dataIndex: 'MinimumQuantity', type: 'int', header: 'Minimum Quantity' },
        { dataIndex: 'CommercialStatus', type: 'string', header: 'Commercial Status' }
      ]
    });

    var productGroupGrid = new Concentrator.ui.Grid({
      pluralObjectName: 'Product Group Vendors',
      singularObjectName: 'Product Group Vendor',
      primaryKey: 'ProductGroupVendorID',
      updateUrl: Concentrator.route("Update", "ProductGroupVendor"),
      sortBy: 'ProductGroupID',
      url: Concentrator.route("GetListFiltered", "ProductGroupVendor"),
      permissions: {
        update: 'UpdateProductGroupVendor',
        list: 'GetProductGroupVendor'
      },
      params: {
        vendorAssortmentID: vendorAssortmentID
      },
      structure: [
        { dataIndex: 'ProductGroupVendorID', type: 'int' },
        { dataIndex: 'ProductGroupID', type: 'int', header: "Product Group", width: 200,
          renderer: function(val, m, record) {
            return record.get('ProductGroupName')
          },
          filter: {
            type: 'string',
            filterField: 'ProductGroupName'
          },
          editor: {
            xtype: 'productgroup'
          }
        },
        { dataIndex: 'ProductGroupName' },
        { dataIndex: 'VendorID', type: 'int', header: "Vendor", width: 175, renderer: Concentrator.renderers.field('vendors', 'VendorName'),
          editor: {
            xtype: 'vendor',
            allowBlank: false
          }
        },
        { dataIndex: 'VendorProductGroupCode1', header: 'Vendor Product Group Code 1', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode2', header: 'Vendor Product Group Code 2', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode3', header: 'Vendor Product Group Code 3', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode4', header: 'Vendor Product Group Code 4', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode5', header: 'Vendor Product Group Code 5', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode6', header: 'Vendor Product Group Code 6', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode7', header: 'Vendor Product Group Code 7', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode8', header: 'Vendor Product Group Code 8', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode9', header: 'Vendor Product Group Code 9', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'VendorProductGroupCode10', header: 'Vendor Product Group Code 10', editor: { xtype: 'textfield', allowBlank: true} },
        { dataIndex: 'BrandCode', header: 'Brand Code', editor: { xtype: 'textfield', allowBlank: true } }
      ]
    });

    var tab = new Ext.TabPanel({
      id: "assortment-details",
      region: 'center',
      enableTabScroll: true,
      activeTab: 0,
      border: true,
      autoDestroy: true,
      deferredRender: false,
      layoutOnTabChange: true,
      items: [
        new Ext.Panel({
          items: priceGrid,
          title: 'Vendor prices',
          layout: 'fit',
          closable: false
        }),
        new Ext.Panel({
          items: productGroupGrid,
          title: 'Vendor Product groups',
          layout: 'fit',
          closable: false
        })
      ]
    });


    var priceWindow = new Ext.Window({
      modal: true,
      items: tab,
      layout: 'fit',
      width: 600,
      height: 300
    });

    priceWindow.show();
  }
});