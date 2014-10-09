Concentrator.ui.PreferredProductGroupVendor = Ext.extend(Ext.Panel, {
  title: 'Preferred product group vendors',
  iconCls: 'molecule-preferences',
  layout: 'fit',

  constructor: function (config) {
    Ext.apply(this, config);
    var self = this;    

    this.connectorID = config;

    Concentrator.ui.PreferredProductGroupVendor.superclass.constructor.call(this, config);
  },

  refresh: function (pgmID) {

    var id = pgmID;

    if (!this.vendorsGrid) {
      this.vendorsGrid = new Concentrator.ui.Grid({
        singularObjectName: 'Content vendor setting',
        pluralObjectName: 'Content vendor settings',
        primaryKey: ['ContentVendorSettingID'],
        permissions: {
          list: 'GetContentVendorSetting',
          create: 'CreateContentVendorSetting',
          update: 'UpdateContentVendorSetting',
          remove: 'DeleteContentVendorSetting'
        },
        params: {
          productGroupMappingID: id,
          connectorID: this.connectorID
        },
        newParams: {
          productGroupMappingID: id,
          connectorID: this.connectorID
        },
        url: Concentrator.route('GetByProductGroupMapping', 'ContentVendorSetting'),
        newUrl: Concentrator.route('Create', 'ContentVendorSetting'),
        deleteUrl: Concentrator.route('Delete', 'ContentVendorSetting'),
        updateUrl: Concentrator.route('Update', 'ContentVendorSetting'),
        sortBy: 'ContentVendorIndex',
        structure: [
            { dataIndex: 'ContentVendorSettingID', type: 'int' },
            { dataIndex: 'ProductGroupID', type: 'int', header: 'Product group',
              renderer: function (val, m, rec) {
                return rec.get('ProductGroup');
              }
            },
            { dataIndex: 'ProductGroup', type: 'string' },
            { dataIndex: 'VendorID', type: 'int', header: 'Vendor', editable: false, editor: { xtype: 'vendor', allowBlank: false },
              renderer: function (val, m, rec) {
                return rec.get('Vendor');
              }
            },
            { dataIndex: 'Vendor', type: 'string' },
            { dataIndex: 'BrandID', type: 'int', header: 'Brand', editable: false, editor: { xtype: 'brand', allowBlank: true },
              renderer: function (val, m, rec) {
                return rec.get('Brand');
              }
            },
            { dataIndex: 'Brand', type: 'string' },
            { dataIndex: 'ContentVendorIndex', header: 'Index', editor: { xtype: 'numberfield', allowBlank: false} }
        ]
      });

      this.add(this.vendorsGrid);
    }

    this.vendorsGrid.store.reload({
      params: {
        productGroupID: id
      }
    });

    this.doLayout();
  }

});