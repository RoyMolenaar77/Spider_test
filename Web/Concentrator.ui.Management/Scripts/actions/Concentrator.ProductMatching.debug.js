Concentrator.ProductMatching = Ext.extend(Concentrator.GridAction, {

  getPanel: function () {

    var self = this;
    var cButton = new Concentrator.ui.ToggleMappingsButton({
      roles: ['CreateProductMatch'],
      listeners: {
        'toggle': function (bt, pressed) {

          if (pressed == false) {
            self.grid.showUnmatched = false;
          }
          else {
            self.grid.showUnmatched = true;
          }

        }
      },
      grid: function () {
        return self.grid;
      },
      unmappedText: 'Show unmatched products',
      mappingField: 'isMatched',
      unmappedValue: 'false'
    });

    self.grid = new Diract.ui.StripedGrid({
      singularObjectName: 'Product match',
      pluralObjectName: 'Product matches',
      primaryKey: ['ProductID', 'ProductMatchID'],
      autoLoadStore: false,
      sortBy: 'ProductID',
      url: Concentrator.route('GetList', 'ProductMatch'),
      stripeRows: true,
      permissions: {
        list: 'ViewProductMatch',
        create: 'CreateProductMatch',
        remove: 'DeleteProductMatch',
        update: 'UpdateProductMatch'
      },
      onGridFilterInitialized: function () {
        cButton.toggleClass(true);
      },
      customButtons: [
        cButton,
        {
          text: 'Product Wizard',
          iconCls: 'wizard-hat',
          alignRight: true,
          handler: function () {
            self.getProductWizard(self);
          }
        }
      ],
      updateUrl: Concentrator.route('Update', 'ProductMatch'),
      deleteUrl: Concentrator.route('Delete', 'ProductMatch'),
      newUrl: Concentrator.route('Create', 'ProductMatch'),
      groupField: 'ProductMatchID',
      hideGroupedColumn: 'ProductMatchID',
      structure: [
        { dataIndex: 'ProductMatchID', type: 'int', header: 'Matched Products' },
        { dataIndex: 'ProductID', type: 'int',
          renderer: function (val, m, rec) { return rec.get('Description') },
          header: 'Product', editable: false,
          editor: { xtype: 'product', hiddenName: 'ProductID', allowBlank: 'false' },
          filter: {
            type: 'string',
            filterField: 'Description'
          }
        },
        { dataIndex: 'Description', type: 'string' },
        { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor Item Number' },
        { dataIndex: 'Brand', type: 'int',
          renderer: function (val, m, rec) { return rec.get('BrandName') }, header: 'Brand',
          filter: {
            type: 'string',
            filterField: 'BrandName'
          }
        },
        { dataIndex: 'BrandName', type: 'string' },
        { dataIndex: 'ProductBarcode', type: 'string', header: 'Barcode' },
        { dataIndex: 'CorrespondingProductID', type: 'int',
          renderer: function (val, m, rec) { return rec.get('CorrespondingDescription') },
          header: 'Corresponding product', editable: false,
          editor: { xtype: 'product', hiddenName: 'CorrespondingProductID', allowBlank: true },
          filter: {
            type: 'string',
            filterField: 'CorrespondingDescription'
          }
        },
        { dataIndex: 'CorrespondingVendorItemNumber', type: 'string', header: 'Corresponding Vendor Item Number' },
        { dataIndex: 'CorrespondingDescription', type: 'string' },
        { dataIndex: 'CorrespondingBrandName', type: 'string', header: 'Corresponding Brand Name' },
        { dataIndex: 'CorrespondingProductBarcode', type: 'string', header: 'Corresponding Barcode' },
        { dataIndex: 'isMatched', header: 'Match', type: 'boolean', editor: { xtype: 'checkbox'} },
        { dataIndex: 'MatchPercentage', header: 'Match Percentage', type: 'int',
          filter: {
            type: 'int',
            filterField: 'MatchPercentage'
          }
        },
        { dataIndex: 'VendorID', type: 'int', header: 'Vendor' },
        { dataIndex: 'MediaUrl', type: 'string' },
        { dataIndex: 'CorrespondingMediaUrl', type: 'string' },
        { dataIndex: 'MediaPath', type: 'string' },
        { dataIndex: 'CorrespondingMediaPath', type: 'string' },
        { dataIndex: 'ActiveProduct', type: 'string', header: 'Active products' },
        { dataIndex: 'MatchStatus', type: 'string', header: 'Match Status',
          renderer: Concentrator.renderers.field('statusTypes', 'Name'),
          filter: {
            type: 'list',
            store: Concentrator.stores.statusTypes,
            labelField: 'Name'
          }
        }
      ],
      rowActions: [
        {
          text: 'Active Products',
          iconCls: 'merge',
          handler: function (record) {
            var grid = new Diract.ui.Grid({
              singularObjectName: 'Active Product',
              pluralObjectName: 'Active Products',
              sortBy: 'ProductID',
              url: Concentrator.route("GetActiveProductsPerProduct", "ProductMatch"),
              permissions: {
                list: 'ViewProductMatch'
              },
              params: {
                vendorID: record.get('VendorID'),
                productID: record.get('ProductID')
              },
              structure: [
                        { dataIndex: 'ProductID', type: 'int' },
                        { dataIndex: 'ShortDescription', type: 'string', header: 'Description' },
                        { dataIndex: 'CustomItemNumber', type: 'string', header: 'Custom Item Number' }
                    ]
            });

            var window = new Ext.Window({
              title: 'Active Products',
              layout: 'fit',
              modal: true,
              width: 600,
              height: 300,
              items: [
                        grid
                    ]
            });

            window.show();
          }
        }
      ],
      listeners: {
        'rowdblclick': (function (grid, rowIndex) {

          self.getAlternativeWizard(grid, rowIndex);

        }).createDelegate(this)
      }
    });

    return self.grid;
  },

  getAlternativeWizard: function (grid, rowIndex) {

    var self = this;

    var productMatchID = grid.getStore().getAt(rowIndex).get('ProductMatchID');
    var params = new Object(),
                          counter = 0;

    params["filter[" + counter + "][data][type]"] = 'int';
    params["filter[" + counter + "][field]"] = 'ProductMatchID';
    params["filter[" + counter + "][data][value]"] = productMatchID;
    counter++;

    var handler = function (store) {

      var alternativeWizard = new Concentrator.ui.ProductMatchWizard({
        grid: self.grid,
        params: params,
        listeners: {
          'close': function () {

            for (var key in self.grid.store.baseParams) {
              if (key.match("^filter")) {
                delete self.grid.store.baseParams[key];
              }
            }

            self.grid.store.reload();

          },
          'destroy': function () {

            for (var key in self.grid.store.baseParams) {
              if (key.match("^filter")) {
                delete self.grid.store.baseParams[key];
              }
            }

            self.grid.store.reload();

          }
        }
      });

      store.removeListener('load', handler);
    }

    self.grid.store.on('load', handler);

    self.grid.store.load({
      params: Ext.apply(self.grid.store.baseParams, params)
    });

  },

  getProductWizard: function (grid) {

    var self = this;

    var vendorField = new Ext.form.TextField({
      fieldLabel: 'Vendor item number',
      width: 175
    });

    var vendorIDField = new Diract.ui.Select({
      label: 'Vendor',
      allowBlank: true,
      displayField: 'VendorName',
      valueField: 'VendorID',
      name: 'VendorID',
      clearable: true,
      store: Concentrator.stores.vendors,
      width: 175
    });

    var connectorField = new Diract.ui.Select({
      label: 'Connector',
      allowBlank: true,
      displayField: 'Name',
      valueField: 'ID',
      name: 'ConnectorID',
      clearable: true,
      store: Concentrator.stores.connectors,
      width: 175
    });

    this.statusStore = new Ext.data.SimpleStore({
      id: 0,
      mode: 'remote',
      fields: [
        'ID',
        'Name'
      ],
      data: [['New', 'New'], ['Accepted', 'Accepted'], ['Declined', 'Declined']],
      proxy: new Ext.data.MemoryProxy([['New', 'New'], ['Accepted', 'Accepted'], ['Declined', 'Declined']])
    });

    var statusField = new Diract.ui.Select({
      label: 'Status',
      allowBlank: true,
      displayField: 'Name',
      valueField: 'ID',
      name: 'MatchStatus',
      clearable: true,
      store: this.statusStore,
      width: 175
    });

    var percentageText = new Ext.form.Label({
      cls: 'matchStyle',
      height: 17,
      html: 'Match Percentage'
    });

    var matchPercentageFieldSmaller = new Ext.form.NumberField({
      fieldLabel: '<img src="' + Concentrator.content("Content/images/less_than.png") + '" />',
      name: 'MatchPercentage',
      width: 175
    });

    var matchPercentageFieldEqual = new Ext.form.NumberField({
      fieldLabel: '<img src="' + Concentrator.content("Content/images/equals.png") + '" />',
      name: 'MatchPercentage',
      width: 175
    });

    var matchPercentageFieldBigger = new Ext.form.NumberField({
      fieldLabel: '<img src="' + Concentrator.content("Content/images/greater_than.png") + '" />',
      name: 'MatchPercentage',
      width: 175
    });

    var form = new Diract.ui.FormWindow({
      width: 400,
      height: 400,
      disableButton: true,
      modal: true,
      title: 'Filter your results',
      formStyle: 'padding: 15px;',
      layout: 'fit',
      listeners: {
        'hide': function () {

          for (var key in self.grid.store.baseParams) {
            if (key.match("^filter")) {
              delete self.grid.store.baseParams[key];
            }
          }

          self.grid.store.reload();
        }
      },
      items: [
        vendorField,
        connectorField,
        vendorIDField,
        statusField,
        percentageText,
        matchPercentageFieldSmaller,
        matchPercentageFieldBigger,
        matchPercentageFieldEqual
      ],
      buttons: [
        {
          text: 'Filter',
          handler: function () {

            var params = new Object(),
                          counter = 0;

            if (vendorField.getValue()) {
              params["filter[" + counter + "][data][type]"] = 'string';
              params["filter[" + counter + "][field]"] = 'VendorItemNumber';
              params["filter[" + counter + "][data][type]"] = 'string';
              params["filter[" + counter + "][field]"] = 'CorrespondingVendorItemNumber';
              params["filter[" + counter + "][data][value]"] = vendorField.getValue();
              counter++;
            }

            if (connectorField.getValue()) {
              params["filter[" + counter + "][data][type]"] = 'int';
              params["filter[" + counter + "][field]"] = 'ConnectorID';
              params["filter[" + counter + "][data][value]"] = connectorField.getValue();
              counter++;
            }

            if (vendorIDField.getValue()) {
              params["filter[" + counter + "][data][type]"] = 'int';
              params["filter[" + counter + "][field]"] = 'VendorID';
              params["filter[" + counter + "][data][value]"] = vendorIDField.getValue();
              counter++;
            }

            if (matchPercentageFieldSmaller.getValue()) {
              params["filter[" + counter + "][data][comparison]"] = 'lt';
              params["filter[" + counter + "][data][type]"] = 'nummeric';
              params["filter[" + counter + "][field]"] = 'MatchPercentage';
              params["filter[" + counter + "][data][value]"] = matchPercentageFieldSmaller.getValue();
              counter++;
            }

            if (matchPercentageFieldBigger.getValue()) {
              params["filter[" + counter + "][data][comparison]"] = 'gt';
              params["filter[" + counter + "][data][type]"] = 'nummeric';
              params["filter[" + counter + "][field]"] = 'MatchPercentage';
              params["filter[" + counter + "][data][value]"] = matchPercentageFieldBigger.getValue();
              counter++;
            }

            if (matchPercentageFieldEqual.getValue() === 0) {
              params["filter[" + counter + "][data][comparison]"] = 'eq';
              params["filter[" + counter + "][data][type]"] = 'nummeric';
              params["filter[" + counter + "][field]"] = 'MatchPercentage';
              params["filter[" + counter + "][data][value]"] = 0;
              counter++;
            }

            if (matchPercentageFieldEqual.getValue()) {
              params["filter[" + counter + "][data][comparison]"] = 'eq';
              params["filter[" + counter + "][data][type]"] = 'nummeric';
              params["filter[" + counter + "][field]"] = 'MatchPercentage';
              params["filter[" + counter + "][data][value]"] = matchPercentageFieldEqual.getValue();
              counter++;
            }

            if (statusField.getValue()) {

              var statusFieldValue = statusField.selectedIndex + 1;

              params["filter[" + counter + "][data][type]"] = 'list';
              params["filter[" + counter + "][field]"] = 'MatchStatus';
              params["filter[" + counter + "][data][value]"] = statusFieldValue;
              counter++;
            }

            self.handler = function (store) {

              if (store.getCount() == 0) {

                Ext.Msg.alert('No records found', 'There are no records matching the filters', function () {

                  for (var key in self.grid.store.baseParams) {
                    if (key.match("^filter")) {
                      delete self.grid.store.baseParams[key];
                    }
                  }

                  self.grid.store.reload();

                });

              }
              else {

                var wiz = new Concentrator.ui.ProductMatchWizard({
                  grid: self.grid,
                  form: form,
                  matchPercentageSmaller: matchPercentageFieldSmaller.getValue(),
                  matchPercentageBigger: matchPercentageFieldBigger.getValue(),
                  matchPercentageEqual: matchPercentageFieldEqual.getValue(),
                  vendorItemNumber: vendorField.getValue(),
                  connector: connectorField.getValue(),
                  vendor: vendorIDField.getValue(),
                  params: params,
                  listeners: {
                    'close': function () {
                      form.hide();
                      form.destroy();
                    },
                    'destroy': function () {
                      form.hide();
                      form.destroy();
                    }
                  }
                });
              }

              self.grid.store.removeListener('load', self.handler);

            }

            self.grid.store.on('load', self.handler);

            self.grid.store.load({
              params: Ext.apply(self.grid.store.baseParams, params)
            });

          }
        }
      ]

    });
    form.show();

  }
});