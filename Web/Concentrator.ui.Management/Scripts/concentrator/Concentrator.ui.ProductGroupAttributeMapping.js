Concentrator.ui.ProductGroupAttributeMapping = Ext.extend(Ext.Panel, {
  title: 'Product group attribute mapping',
  iconCls: 'transform',
  layout: 'border',

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;

    this.getPanelItems();

    Concentrator.ui.ProductGroupAttributeMapping.superclass.constructor.call(this, config);
  },

  refresh: function (pgmID) {
    var self = this;
    self.pgmID = pgmID;

    if (!this.productGroupAttributeGrid) {

      this.productGroupAttributeGrid = new Concentrator.ui.Grid({
        singularObjectName: 'Product group attribute',
        pluralObjectName: 'Product group attributes',
        primaryKey: ['AttributeID'],
        sortBy: 'AttributeGroupID',
        permissions: {
          list: 'GetProductGroupAttributeMapping',
          create: 'CreateProductGroupAttributeMapping',
          update: 'UpdateProductGroupAttributeMapping',
          remove: 'DeleteProductGroupAttributeMapping'
        },
        params: {
          productGroupMappingID: self.pgmID
        },
        newParams: {
          productGroupMappingID: self.pgmID
        },
        ddGroup: 'attributetree',
        enableDragDrop: true,
        url: Concentrator.route('GetList', 'ProductGroupAttributeMapping'),
        updateUrl: Concentrator.route('Update', 'ProductGroupAttributeMapping'),
        newUrl: Concentrator.route('Create', 'ProductGroupAttributeMapping'),
        deleteUrl: Concentrator.route('Delete', 'ProductGroupAttributeMapping'),
        groupField: 'AttributeGroupID',
        newFormConfig: Ext.apply(Concentrator.FormConfigurations.existingAttributeValue, { suppressSuccessMsg: true, suppressFailureMsg: true }),
        structure: [
          { dataIndex: 'AttributeID', type: 'int' },
          { dataIndex: 'Attribute', header: 'Attribute', type: 'string' },
          { dataIndex: 'AttributeGroupID', type: 'int', header: 'Group', type: 'int', editor: { xtype: 'productattributegroup', hiddenName: 'ProductAttributeGroupID' },
            renderer: function (val, m, rec) {
              return rec.get('AttributeGroup');
            }
          },
          { dataIndex: 'AttributeGroup', type: 'string' },
          { dataIndex: 'Sign', header: 'Sign', type: 'string', editor: { xtype: 'textfield', allowBlank: false} },
          { dataIndex: 'Index', header: 'Attribute Index', type: 'int', editor: { xtype: 'numberfield', allowBlank: false} },
          { dataIndex: 'IsVisible', header: 'Visible in filter', type: 'boolean', editor: { xtype: 'checkbox', allowBlank: false} },
          { dataIndex: 'IsSearchable', header: 'Searchable', type: 'boolean', editor: { xtype: 'checkbox', allowBlank: false} },
          { dataIndex: 'ProductGroupMappingID', type: 'int' }
        ],
        rowActions: [
          {
            text: 'View associated products',
            iconCls: 'cubes',
            handler: function (row) {
              var productGroupMappingID = self.pgmID;

              self.GetAssociatedProductsWindow(row.get('AttributeID'), productGroupMappingID);
            }
          }
        ]
      });

      this.productGroupAttributeCenter.add(this.productGroupAttributeGrid);
      this.doLayout();
    }

    this.productGroupAttributeGrid.store.reload({
      params: {
        productGroupMappingID: self.pgmID
      }
    });

  },

  getPanelItems: function () {

    this.productGroupAttributeCenter = new Ext.Panel({
      region: 'center',
      border: false,
      margins: '0 5 0 0',
      layout: 'fit'
    });

    this.productGroupAttributeEast = new Ext.Panel({
      region: 'east',
      ddGroup: 'tree',
      width: 200
    });

    this.items = [this.productGroupAttributeCenter, this.productGroupAttributeEast];

    //    this.productGroupAttributeMappingPanel = new Ext.Panel({
    //      id: 'ProductGroupAttributeMapping',
    //      title: 'Product group attribute mapping',
    //      iconCls: 'transform',
    //      layout: 'border',
    //      items: []
    //    });
  },

  GetAssociatedProductsWindow: function (attributeid, productGroupMappingID) {

    this.attributesgrid = new Diract.ui.ExcelGrid({
      singularObjectName: 'Product',
      pluralObjectName: 'Products',
      primaryKey: ['AttributeValueID'],
      chooseColumns: true,
      permissions: {
        list: 'GetProduct',
        create: 'CreateProduct',
        update: 'UpdateProduct',
        remove: 'DeleteProduct'
      },
      customButtons: [
        {
          text: 'Show Unmapped Records',
          iconCls: 'lightbulb-off',
          alwaysEnabled: true,
          enableToggle : true,
          listeners: {
            'toggle': (function (bt, pressed) {                            
                            
              if (pressed == true) {
                bt.setIconClass('lightbulb-on');
                
                var params = {
                  showUnmappedValues: true
                };
                
                Ext.apply(this.attributesgrid.baseParams, params);
                this.attributesgrid.store.load({ params: params });
              }
              else {
                bt.setIconClass('lightbulb-off');

                var params = {
                  showUnmappedValues: false
                };

                Ext.apply(this.attributesgrid.baseParams, params);
                this.attributesgrid.store.load({ params: params });
              }

            }).createDelegate(this)
          }          
        }
      ],
      params: {
        AttributeID: attributeid,
        ProductGroupMappingID: productGroupMappingID
      },
      excelRequestParams: {
        AttributeID: attributeid,
        ProductGroupMappingID: productGroupMappingID
      },
      url: Concentrator.route('GetList', 'ProductAttributeValue'),
      updateUrl: Concentrator.route('Update', 'ProductAttributeValue'),
      sortBy: 'ProductID',
      listeners: {
        'celldblclick': (function (grid, rowIndex, columnIndex, evt) {
          var rec = grid.store.getAt(rowIndex);
          var productID = rec.get('ProductID');

          var factory = new Concentrator.ProductBrowserFactory({ productID: productID });
        }).createDelegate(this)
      },
      structure: [
        { dataIndex: 'AttributeValueID', type: 'int' },
        { dataIndex: 'ProductID', header: 'Identifier', type: 'int', editable: false },
        { dataIndex: 'BrandID', header: 'Brand', type: 'int', editable: false,
          renderer: function (val, met, record) {
            return record.get("BrandName");
          }
        },
        { dataIndex: 'BrandName', type: 'string' },
        { dataIndex: 'VendorItemNumber', header: 'Vendor item number', type: 'string', editable: false },
        { dataIndex: 'Value', header: 'Attribute value', type: 'string', editor: { xtype: 'textfield'} }
      ]
    });

    var window = new Ext.Window({
      title: 'Associated Products',
      width: 830,
      height: 450,
      modal: true,
      layout: 'fit',
      items: [
        this.attributesgrid
      ]
    });

    window.show();
  }

});