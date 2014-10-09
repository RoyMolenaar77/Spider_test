Concentrator.BrandVendors = Ext.extend(Concentrator.GridAction, {
  getPanel: function () {

    var btn = new Concentrator.ui.ToggleMappingsButton({ roles: ['ShowUnmappedProducts'],
      grid: function () {
        return gr;
      },
      mappingField: 'BrandID'
    });

    var gr = new Diract.ui.Grid({
      primaryKey: ['BrandID', 'VendorID', 'VendorBrandCode'],
      singularObjectName: 'Vendor brand',
      pluralObjectName: 'Vendor brands',
      sortBy: 'BrandID',
      height: 400,
      width: 500,
      url: Concentrator.route("GetList", "BrandVendor"),
      customButtons: [btn],
      updateUrl: Concentrator.route("Update", "BrandVendor"),
      newUrl: Concentrator.route('Create', 'BrandVendor'),
      deleteUrl: Concentrator.route('Delete', 'BrandVendor'),
      permissions: {
        list: 'GetBrandVendor',
        update: 'UpdateBrandVendor',
        create: 'CreateBrandVendor',
        remove: 'DeleteBrandVendor'
      },
      onGridFilterInitialized: function () {
        btn.toggleClass(true);
      },
      structure: [
                { dataIndex: 'BrandID', type: 'int', header: 'Brand',
                  filter: {
                    type: 'string',
                    filterField: 'BrandName'
                  },
                  editor: {
                    xtype: 'brand',
                    //hiddenName: 'BrandID',
                    listeners: {
                      'select': (function (val, meta, data) {

                        btn.handleParameter(meta.get('BrandID'));

                      }).createDelegate(this)
                    }
                  },
                  renderer: function (val, metadata, record) {
                    return record.get('BrandName');
                  },
                  width: 150
                },
                { dataIndex: 'BrandName', type: 'string' },
                { dataIndex: 'VendorBrandCode', type: 'string', header: 'Brand Vendor Code',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'VendorID', type: 'int', header: 'Vendor',
                  editable: false,
                  editor: { xtype: 'vendor', allowBlank: false },
                  renderer: Concentrator.renderers.field('vendors', 'VendorName'), filter: {
                    type: 'list',
                    store: Concentrator.stores.vendors,
                    labelField: 'VendorName'
                  }
                },
                { dataIndex: 'Name', header: 'Name from Vendor', type: 'string',
                  editable: false,
                  editor: { xtype: 'textfield', allowBlank: true }
                },
                { dataIndex: 'BrandVendorLogo', type: 'string', header: 'Logo' }
      ],
      rowActions: [
        {
          text: 'View Logo',
          iconCls: 'merge',
          predicate: function (record) { return (record.get('BrandID') > 0); },
          handler: function (record) {

            var box = new Ext.BoxComponent(
              { autoEl: {
                tag: 'img',
                src: Concentrator.GetBrandLogoUrl(record.get('BrandVendorLogo'), 256, 256)
              }
              });

            var closeButton = new Ext.Button({
              text: 'Close',
              handler: function () {
                window.destroy();
              }
            });

            var form = new Ext.form.FormPanel({
              padding: 10,
              items: [
                box
              ],
              buttons: [
                closeButton
              ]
            });

            var window = new Ext.Window({
              title: 'Logo',
              height: 200,
              width: 300,
              modal: true,
              layout: 'fit',
              items: [
                form
              ]
            });

            var self = this;
            var that = record;

            window.show();
          }
        }
      ]
    });
    if (!this.grid)
      this.grid = gr;
    return this.grid;
  }
});


