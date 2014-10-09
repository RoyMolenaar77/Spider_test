/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Concentrator.ui.ProductInfoViewer = (function () {
  var pan = Ext.extend(Ext.Panel, {
    autoScroll: true,
    border: false,
    constructor: function (config) {
      Ext.apply(this, config);

      this.initToolbar();

      Concentrator.ui.ProductInfoViewer.superclass.constructor.call(this, config);

      this.initLanguageCombo();
      this.doLayout();
    },
    initToolbar: function () {
      var store = new Ext.data.JsonStore({
        url: Concentrator.route('GetAvailableContentVendorsForProduct', 'ProductDescription'),
        baseParams: { productID: this.productID, includeConcentratorVendor: true },
        root: 'results',
        idProperty: 'VendorID',
        fields: ['VendorName', 'VendorID'],
        listeners: {
          'load': (function (store, records) {
            this.infoVendorCombo.setValue(records[0].get('VendorID'));

            if (!this.infoForm) {
              this.initInfoForm();
            }

            this.doLayout();
          }).createDelegate(this)
        }
      });

      store.load();

      this.infoVendorCombo = new Ext.form.ComboBox(
      {
        xtype: 'combobox',
        store: store,
        displayField: 'VendorName',
        valueField: 'VendorID',
        triggerAction: 'all',
        editable: false,
        typeAhead: false,
        forceSelection: false,
        listeners: {
          'select': (function () {
            this.reloadForm();
          }).createDelegate(this)
        }
      });

      this.infoLanguageCombo = new Ext.form.ComboBox(
      {
        xtype: 'combobox',
        store: Concentrator.stores.languages,
        displayField: 'Name',
        valueField: 'ID',
        triggerAction: 'all',
        editable: false,
        typeAhead: false,
        value: Concentrator.user.languageID,
        listeners: {
          'select': (function () {
            this.reloadForm();
          }).createDelegate(this)
        }
      });

      this.tbar = new Ext.Toolbar({
        items: [
          'Vendor',
          this.infoVendorCombo,
          { xtype: 'tbseparator' },
          'Language',
          this.infoLanguageCombo,
          { xtype: 'tbseparator' },
          {
            xtype: 'button',
            text: 'Save changes',
            iconCls: 'save',
            handler: (function () {
              this.infoForm.submit();
            }).createDelegate(this)
          },
          {
            xtype: 'button',
            text: 'Discard changes',
            iconCls: 'delete',
            handler: (function () {
              this.infoForm.getForm().reset();
            }).createDelegate(this)
          },
          {
            xtype: 'button',
            text: 'Clone content...',
            iconCls: 'merge',
            handler: (function () {
              var sourceVendorStore = new Ext.data.JsonStore({
                url: Concentrator.route('GetExistingContentVendorsForProduct', 'ProductDescription'),
                baseParams: { productID: this.productID, includeConcentratorVendor: false },
                root: 'results',
                idProperty: 'VendorID',
                fields: ['VendorName', 'VendorID'],
                listeners:
                {
                  'load': function (sender, records) {
                    if (sourceVendorSelector.selectedIndex == -1 && records.length > 0) {
                      sourceVendorSelector.setValue(records[0].get('VendorID'));
                    }
                  }
                }
              });

              sourceVendorStore.load();

              var sourceVendorSelector = new Ext.form.ComboBox({
                displayField: 'VendorName',
                valueField: 'VendorID',
                triggerAction: 'all',
                editable: false,
                hiddenName: 'sourceVendorID',
                name: 'vendorID',
                fieldLabel: 'Source',
                width: 170,
                store: sourceVendorStore
              });


              var targetVendorStore = new Ext.data.JsonStore({
                url: Concentrator.route('GetAvailableContentVendorsForProduct', 'ProductDescription'),
                baseParams: { productID: this.productID },
                root: 'results',
                idProperty: 'VendorID',
                fields: ['VendorName', 'VendorID'],
                listeners:
                {
                  'load': function (sender, records) {
                    if (targetVendorSelector.selectedIndex == -1 && records.length > 0) {
                      targetVendorSelector.setValue(records[0].get('VendorID'));
                    }
                  }
                }
              });

              targetVendorStore.load();

              var targetVendorSelector = new Ext.form.ComboBox({
                displayField: 'VendorName',
                valueField: 'VendorID',
                triggerAction: 'all',
                editable: false,
                hiddenName: 'targetVendorID',
                name: 'targetVendorID',
                fieldLabel: 'Target',
                width: 170,
                store: targetVendorStore
              });

              var window = new Diract.ui.FormWindow({
                title: 'Clone content',
                url: Concentrator.route('Clone', 'ProductDescription'),
                items: [{ xtype: 'hidden', name: 'productID', value: this.productID }, sourceVendorSelector, targetVendorSelector],
                width: 320,
                height: 150,
                success: (function () {
                  window.destroy();
                  this.infoVendorCombo.store.reload();
                }).createDelegate(this)
              });

              window.show();
            }).createDelegate(this)
          },
          {
            xtype: 'button',
            text: 'Remove content',
            iconCls: 'delete',
            handler: (function () {
              this.vendorID = this.infoVendorCombo.getValue();

              Diract.request({
                url: Concentrator.route("DeleteVendorContent", "ProductDescription"),
                params: {
                  productID: this.productID,
                  vendorID: this.vendorID
                },
                success: function (result) {
                },
                failure: function (result) {
                }
              });

              this.doLayout();
            }).createDelegate(this)
          }]
      });
    },
    initLanguageCombo: function () {
      this.languageCombo = new Ext.form.ComboBox({
        store: Concentrator.stores.languages,
        displayField: 'Name',
        valueField: 'ID'
      });
    },
    initInfoForm: function () {
      var self = this;

      self.getMyParams = function () {
        var params = {
          productID: self.productID,
          vendorID: self.infoVendorCombo.getValue(),
          languageID: self.infoLanguageCombo.getValue(),
          isSearched: self.isSearched || false
        }

        return params;
      };

      this.infoForm = new Diract.ui.Form({
        url: Concentrator.route('UpdateByAttribute', 'ProductDescription'),
        loadUrl: Concentrator.route('GetByProduct', 'ProductDescription'),
        noButton: true,
        autoLoadForm: false,
        method: 'POST',
        labelWidth: 200,
        params: self.getMyParams,
        listeners: {
          afterrender: (function () {
            this.reloadForm();
          }).createDelegate(this)
        },
        defaults: {
          labelSeparator: ' '
        }
      });

      this.add(this.infoForm);
    },
    reloadForm: function () {
      var self = this;


      self.infoForm.getForm().reset();

      var vendorID = this.infoVendorCombo.getValue();
      var languageID = this.infoLanguageCombo.getValue();

      if (!self.infoForm.loadedStructure) {
        Diract.silent_request({
          url: Concentrator.route('GetProductAttributesByProduct', 'ProductDescription'),
          params: { productID: this.productID, vendorID: vendorID, languageID: languageID },
          success: function (response) {
            self.infoForm.removeAll(true);

            var data = {};

            for (var i = 0; i < response.data.length; i++) {
              var field = response.data[i];

              self.infoForm.add({
                xtype: field.Type,
                fieldLabel: field.Label + ":",
                name: field.Name,
                width: 640,
                hiddenName: field.HiddenName
              });
            }

            self.infoForm.getForm().setValues(data);
            self.infoForm.doLayout();
            self.infoForm.loadedStructure = true;
          }
        });
      }
      self.infoForm.loadForm({ productID: this.productID, vendorID: vendorID, languageID: languageID });
    },
    initMainPanel: function () {
      this.mainPanel = new Ext.Panel({
        items: [
          this.languageCombo,
          this.infoForm
        ]
      });
    }
  });

  return pan;
})();