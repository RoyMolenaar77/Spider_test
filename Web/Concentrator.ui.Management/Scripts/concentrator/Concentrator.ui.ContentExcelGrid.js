/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

Diract.addComponent({ dependencies: ['Diract.ui.ExcelGrid'] }, function () {

  Concentrator.ui.ContentExcelGrid = (function () {
    var gr = Ext.extend(Diract.ui.ExcelGrid, {
      constructor: function (config) {
        Ext.apply(this, config);

        Concentrator.ui.ContentExcelGrid.superclass.constructor.call(this, config);
      },

      getState : function(){
        var state = this.superclass.getState.call();

        //merge in the results with our new form - results
      },

      extraExportFunction: function () {
        if (that.chooseColumns) {

          var value = that.selModel.selections.items[0];

          var productBox = new Ext.form.Checkbox({
            fieldLabel: 'Product',
            name: value.get('ProductID')
          });

          var connectorBox = new Ext.form.Checkbox({
            fieldLabel: 'Connector',
            name: value.get('ConnectorID')
          });

          var activeBox = new Ext.form.Checkbox({
            fieldLabel: 'Name',
            name: value.get('Name')
          });

          var vendorBox = new Ext.form.Checkbox({
            fieldLabel: 'Vendor Item Number',
            name: value.get('VendorItemNumber')
          });

          var cutomBox = new Ext.form.Checkbox({
            fieldLabel: 'Custom Item Number',
            name: value.get('CustomItemNumber')
          });

          var brandBox = new Ext.form.Checkbox({
            fieldLabel: 'Brand Name',
            name: value.get('BrandName')
          });

          var shortBox = new Ext.form.Checkbox({
            fieldLabel: 'Short Description',
            name: value.get('ShortDescription')
          });

          var barcodeBox = new Ext.form.Checkbox({
            fieldLabel: 'Barcode',
            name: value.get('Barcode')
          });

          var imageBox = new Ext.form.Checkbox({
            fieldLabel: 'Image',
            name: value.get('Image')
          });

          var youtubeBox = new Ext.form.Checkbox({
            fieldLabel: 'Specifications',
            name: value.get('Specifications')
          });

          var creationBox = new Ext.form.Checkbox({
            fieldLabel: 'Creation Time',
            name: value.get('CreationTime')
          });

          var modificationBox = new Ext.form.Checkbox({
            fieldLabel: 'Last Modification',
            name: value.get('LastModification')
          });


          //if (form == "-") column++;

          var formLeft = new Ext.form.FormPanel({
            padding: 20,
            region: 'west',
            width: 350,
            height: 300,
            items: [
                    productBox,
                    connectorBox,
                    activeBox,
                    vendorBox,
                    cutomBox,
                    brandBox
                  ]
          });

          var formRight = new Ext.form.FormPanel({
            padding: 20,
            region: 'center',
            width: 350,
            items: [
                    shortBox,
                    barcodeBox,
                    imageBox,
                    youtubeBox,
                    creationBox,
                    modificationBox
                  ]
          });

          var window = new Ext.Window({
            width: 715,
            height: 300,
            layout: 'border',
            modal: true,
            items: [
                    formLeft,
                    formRight
                  ],
            buttons: [
                    new Ext.Button({
                      text: 'Export',
                      handler: function () {

                        var checkedArray = [];

                        Ext.each(formLeft.items.items, function (formItem) {

                          if (formItem.checked == true) {
                            checkedArray.push(formItem);
                          }

                        });

                        Ext.each(formRight.items.items, function (formItem) {

                          if (formItem.checked == true) {
                            checkedArray.push(formItem);
                          }

                        });

                        var newState = checkedArray;                      

                      }
                    })
                  ]
          });


          window.show();

        }
      }
    });
    return gr;
  })();

});