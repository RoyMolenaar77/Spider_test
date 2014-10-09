/// <reference path="~/Content/js/ext/ext-base-debug.js" /> 
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Diract.addComponent({ dependencies: ['Diract.ui.SearchBox'] }, function () {

  Diract.ui.AssortmentSearch = (function () {

    var search = Ext.extend(Ext.Panel, {
      height: 55,
      border: false,
      layout: 'form',

      constructor: function (config) {
        Ext.apply(this, config);

        this.vendorAssortmentID = null;
        this.productID = null;
        this.vendorID = null;

        this.productSearch = new Diract.ui.SearchBox({
          fieldLabel: 'Product',
          valueField: 'ProductID',
          displayField: 'Product',
          allowBlank: false,
          width: 249,
          searchUrl: Concentrator.route('GetProductStore', 'VendorAssortment'),
          listeners: {
            'select': (function (combo, record, index) {

              this.productID = record.get('ProductID');
              this.vendorSearch.store.setBaseParam('productID', this.productID);
              this.doLayout();

            }).createDelegate(this)
          }
        });

        this.vendorSearch = new Diract.ui.SearchBox({
          fieldLabel: 'Vendor',
          valueField: 'VendorID',
          displayField: 'Vendor',
          allowBlank: false,
          width: 249,
          searchUrl: Concentrator.route('GetVendorStore', 'VendorAssortment'),
          listeners: {
            'select': (function (combo, record, index) {

              this.vendorID = record.get('VendorID');
              this.productSearch.store.setBaseParam('vendorID', this.vendorID);
              this.doLayout();

              if (this.productID) {
                Diract.silent_request({
                  url: Concentrator.route('GetVendorAssortmentByID', 'VendorAssortment'),
                  params: {
                    productID: this.productID,
                    vendorID: this.vendorID
                  },
                  success: (function (result) {
                    this.vendorAssortmentID = result.vendorAssortmentID;
                  }).createDelegate(this)
                });
              }
            }).createDelegate(this)
          }
        });        

        this.items = [this.productSearch, this.vendorSearch];

        Diract.ui.AssortmentSearch.superclass.constructor.call(this, config);
      },

      getValue: function () {
        return this.vendorAssortmentID;
      }
    });

    return search;
  })();

  Ext.reg('assortmentBox', Diract.ui.AssortmentSearch);
});


