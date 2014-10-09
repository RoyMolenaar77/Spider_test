Concentrator.AttributeMatching = Ext.extend(Concentrator.BaseAction, {

  setParams: function () {

  },
  getPanel: function () {

    var self = this;

    self.grid = new Diract.ui.StripedGrid({
      singularObjectName: 'Attribute match',
      pluralObjectName: 'Attribute matches',
      primaryKey: ['AttributeID', 'CorrespondingAttributeID'],
      sortBy: 'ProductID',
      url: Concentrator.route('GetList', 'AttributeMatch'),
      stripeRows: true,
      permissions: {
        list: 'ViewAttributeMatch',
        create: 'CreateAttributeMatch',
        remove: 'DeleteAttributeMatch',
        update: 'UpdateAttributeMatch'
      },
      customButtons: [
        { xtype: 'tMButton', roles: ['CreateAttributeMatch'], grid: function () { return self.grid; }, unmappedText: 'Show unmatched attributes', mappingField: 'isMatched', unmappedValue: 'false' },
        {
          text: 'Attribute Wizard',
          iconCls: 'wizard-hat',
          handler: function () {
            self.getProductWizard(self);
          }
        }
      ],
        updateUrl: Concentrator.route('Update', 'AttributeMatch'),
        deleteUrl: Concentrator.route('Delete', 'AttributeMatch'),
        newUrl: Concentrator.route('Create', 'AttributeMatch'),
      structure: [
        { dataIndex: 'AttributeID', type: 'int',
          renderer: function (val, m, rec) { return rec.get('Description') },
          header: 'Attribute', editable: false,
          editor: { xtype: 'product', hiddenName: 'AttributeID', allowBlank: 'false' }
        },
        { dataIndex: 'Description', type: 'string' },
        { dataIndex: 'VendorItemNumber', type: 'string', header: 'Vendor Item Number' },
        { dataIndex: 'Brand', type: 'int',
          renderer: function (val, m, rec) { return rec.get('BrandName') }, header: 'Brand'
        },
        { dataIndex: 'BrandName', type: 'string' },
        { dataIndex: 'ProductBarcode', type: 'string', header: 'Barcode' },
        { dataIndex: 'CorrespondingProductID', type: 'int',
          renderer: function (val, m, rec) { return rec.get('CorrespondingDescription') },
          header: 'Corresponding product', editable: false,
          editor: { xtype: 'product', hiddenName: 'CorrespondingProductID', allowBlank: true }
        },
        { dataIndex: 'CorrespondingVendorItemNumber', type: 'string', header: 'Corresponding Vendor Item Number' },
        { dataIndex: 'CorrespondingDescription', type: 'string' },
        { dataIndex: 'CorrespondingBrandName', type: 'string', header: 'Brand Name' },
        { dataIndex: 'CorrespondingProductBarcode', type: 'string', header: 'Corresponding Barcode' },
        { dataIndex: 'ConnectorID', type: 'int',
          editor: { xtype: 'connector', allowBlank: true, fieldLabel: 'Connector' }
        },
        { dataIndex: 'isMatched', header: 'Match', type: 'boolean', editor: { xtype: 'checkbox'} },
        { dataIndex: 'MatchPercentage', header: 'Match Percentage', type: 'int' },
        { dataIndex: 'ConnectorID', type: 'int' },
        { dataIndex: 'VendorID', type: 'int' }
      ]
    });

    return self.grid;
  },

  getAttributeProductWizard: function (self) {

    var vendorField = new Ext.form.TextField({
      fieldLabel: 'Vendor item number',
      name: 'VendorItemNumber',
      width: 165
    });

    var vendorIDField = new Diract.ui.Select({
      label: 'Vendor',
      allowBlank: true,
      displayField: 'VendorName',
      valueField: 'VendorID',
      name: 'VendorID',
      store: Concentrator.stores.vendors
    });

    var connectorField = new Diract.ui.Select({
      label: 'Connector',
      allowBlank: true,
      displayField: 'Name',
      valueField: 'ID',
      name: 'ConnectorID',
      store: Concentrator.stores.connectors
    });

    var percentageText = new Ext.form.Label({
      cls: 'matchStyle',
      height: 17,
      html: 'Match Percentage'
    });

    var matchPercentageFieldSmaller = new Ext.form.NumberField({
      fieldLabel: '<img src="' + Concentrator.content("Content/images/less_than.png") + '" />',
      name: 'MatchPercentage',
      width: 165
    });

    var matchPercentageFieldEqual = new Ext.form.NumberField({
      fieldLabel: '<img src="' + Concentrator.content("Content/images/equals.png") + '" />',
      name: 'MatchPercentage',
      width: 165
    });

    var matchPercentageFieldBigger = new Ext.form.NumberField({
      fieldLabel: '<img src="' + Concentrator.content("Content/images/greater_than.png") + '" />',
      name: 'MatchPercentage',
      width: 165
    });

    var self = this;

    var form = new Diract.ui.FormWindow({
      width: 400,
      height: 310,
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

            if (matchPercentageFieldEqual.getValue()) {
              params["filter[" + counter + "][data][comparison]"] = 'eq';
              params["filter[" + counter + "][data][type]"] = 'nummeric';
              params["filter[" + counter + "][field]"] = 'MatchPercentage';
              params["filter[" + counter + "][data][value]"] = matchPercentageFieldEqual.getValue();
              counter++;
            }

            self.handler = function (store) {

              if (store.getCount() == 0) {
                Ext.Msg.alert('No records found', 'There are no records matching the filters', function () { });
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
                    },
                    'destroy': function () {
                      form.hide();
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