Concentrator.ui.ProductMatchWizard = (function () {

  var wiz = Ext.extend(Diract.ui.GridWizard, {
    title: 'Product matching wizard',
    lazyLoad: true,
    width: 1000,
    height: 510,
    layout: 'border',

    constructor: function (config) {
      Ext.apply(this, config);

      var self = this;

      this.initMasterLayout();

      Concentrator.ui.ProductMatchWizard.superclass.constructor.call(this, config);

      self.show();
      self.itemForm();
      self.doLayout();
    },

    initMasterLayout: function () {
      var id = this.currentRecord();

      this.productForm = new Diract.ui.Form({
        region: 'west',
        disableButton: true,
        width: 400,
        border: false,
        bodyStyle: 'padding: 20px;',
        iconCls: 'merge',
        items: [

          new Ext.form.TextField({
            hidden: true,
            name: 'ProductID',
            width: 205
          }),

          new Ext.form.TextField({
            fieldLabel: 'Vendor item number',
            name: 'VendorItemNumber',
            width: 200,
            readOnly: true
          }),

          new Ext.form.TextArea({
            fieldLabel: 'Barcode',
            name: 'ProductBarcode',
            width: 200,
            height: 125,
            readOnly: true
          }),

          new Ext.form.TextField({
            fieldLabel: 'Brand',
            name: 'BrandName',
            width: 200,
            readOnly: true
          })

        ]
      });

      this.vendorLabel = new Ext.form.Label({
        cls: 'labelStyle2',
        height: 16,
        width: 17
      });

      this.percentageLabel = new Ext.form.Label({
        cls: 'labelStyle4',
        height: 17
      });

      this.statusLabel = new Ext.form.Label({
        cls: 'statusStyle',
        height: 17
      });

      this.barcodeLabel = new Ext.form.Label({
        cls: 'labelStyle3',
        height: 16,
        width: 17
      });

      this.brandLabel = new Ext.form.Label({
        cls: 'labelStyle5',
        height: 17
      });

      this.matchForm = new Ext.Panel({
        region: 'center',
        width: 200,
        disableButton: true,
        border: false,
        bodyStyle: 'padding:20px 20px 20px 0px',
        items: [
          this.vendorLabel,
          this.barcodeLabel,
          this.brandLabel,
          this.statusLabel,
          this.percentageLabel
        ]
      });

      this.correspondingProductForm = new Diract.ui.Form({
        region: 'east',
        width: 400,
        disableButton: true,
        border: false,
        bodyStyle: 'padding: 20px;',
        items: [

          new Ext.form.TextField({
            hidden: true,
            name: 'CorrespondingProductID',
            width: 200
          }),

          new Ext.form.TextField({
            fieldLabel: 'Vendor item number',
            name: 'CorrespondingVendorItemNumber',
            width: 200,
            readOnly: true
          }),

          new Ext.form.TextArea({
            fieldLabel: 'Barcode',
            name: 'CorrespondingProductBarcode',
            width: 200,
            heigth: 200,
            height: 125,
            readOnly: true
          }),

          new Ext.form.TextField({
            fieldLabel: 'Brand',
            name: 'CorrespondingBrandName',
            width: 200,
            readOnly: true
          })

        ]
      });

      this.headerColLeft = new Ext.Panel({
        columnWidth: .50,
        border: false,
        padding: 10
      });

      this.headerColRight = new Ext.Panel({
        columnWidth: .50,
        border: false,
        padding: 10
      });

      this.headerPanel = new Ext.Panel({
        border: false,
        region: 'north',
        height: 160,
        layout: 'column',
        items: [
          this.headerColLeft,
          this.headerColRight
        ]
      });

      this.panel = new Ext.Panel({
        region: 'center',
        border: false,
        layout: 'border',
        items: [
          this.productForm,
          this.matchForm,
          this.correspondingProductForm
        ]
      });

      this.items = [this.headerPanel, this.panel];
    },

    populateMiddlePanel: function () {

      var id = this.currentRecord();

      this.productID = id.get('ProductID');
      this.correspondingProductID = id.get('CorrespondingProductID');
      this.vendorItemNumber = id.get('VendorItemNumber');
      this.productBarcode = id.get('ProductBarcode');
      this.correspondingProductBarcode = id.get('CorrespondingProductBarcode');
      this.correspondingVendorItemNumber = id.get('CorrespondingVendorItemNumber');
      this.baseVpn = this.vendorItemNumber.length > this.correspondingVendorItemNumber ? this.vendorItemNumber : this.correspondingVendorItemNumber;
      this.shortTF = this.vendorItemNumber.length > this.correspondingVendorItemNumber ? this.correspondingVendorItemNumber : this.vendorItemNumber;
      this.percentage = id.get('MatchPercentage');
      this.status = id.get('MatchStatus');

      var contains = this.baseVpn.indexOf(this.shortTF) >= 0;

      this.ven = 0;

      this.ven = this.vendorItemNumber.indexOf(this.correspondingVendorItemNumber) >= 0;

      this.perc = 0;

      if (contains) {
        this.perc = (100 / this.baseVpn.length) * (this.shortTF.length);
      }

      this.barcodePercentage = 0;

      var barcodesLeft = this.productBarcode.split(',');
      var barcodesRight = this.correspondingProductBarcode.split(',');

      var that = this;

      var barcodeLeftCount = barcodesLeft.length;
      var barcodeRightCount = barcodesRight.length;

      Ext.each(barcodesLeft, function (left) {

        Ext.each(barcodesRight, function (right) {
          if (left == right)
            that.barcodePercentage++;
        });
      });

      if (barcodeLeftCount > barcodeRightCount) {
        this.highestBarcodeCount = barcodeLeftCount;
      }
      else {
        this.highestBarcodeCount = barcodeRightCount;
      }

      // Calculation for the barcode percentage
      this.endPercentage = (100 / this.highestBarcodeCount) * that.barcodePercentage;

      this.barcodeValue = false;

      if (this.productBarcode.toString().split(',').indexOf(this.correspondingProductBarcode) >= 0) {
        this.barcodeValue = true
      }
      else {
        this.barcodeValue = false
      }

      var brandMatch = false

      var corBrandName = id.get("CorrespondingBrandName");
      var brName = id.get("BrandName");

      if (corBrandName == brName) {
        brandMatch = true;
      }

      this.percentage = this.percentage.toString();
      this.perc = this.perc.toString();
      this.vendor = this.perc.toString();
      this.barcodeValue = this.endPercentage.toString();
      this.brandMatchValue = brandMatch;
      this.matchStatus = this.status.toString();

      if (this.vendor > 60) {
        // sets the color to green
        this.vendorLabel.removeClass('labelStyle2');
        this.vendorLabel.addClass('altLabelStyle2');
      }

      if (this.vendor < 60) {
        // sets the color to red
        this.vendorLabel.removeClass('altLabelStyle2');
        this.vendorLabel.addClass('labelStyle2');
      }

      if (this.barcodeValue > 60) {
        // sets the color to green        
        this.barcodeLabel.removeClass('labelStyle3');
        this.barcodeLabel.addClass('altLabelStyle3');
      }

      if (this.barcodeValue < 60) {
        // sets the color to red        
        this.barcodeLabel.removeClass('altLabelStyle3');
        this.barcodeLabel.addClass('labelStyle3');
      }

      if (!this.brandMatchValue) {
        // sets a delete icon
        this.brandLabel.removeClass('labelStyle5');
        this.brandLabel.addClass('altLabelStyle5');
      }

      if (this.brandMatchValue) {
        // sets a check icon
        this.brandLabel.removeClass('altLabelStyle5');
        this.brandLabel.addClass('labelStyle5');
      }

      if (this.percentage < 60) {
        // Sets the text to red
        this.percentageLabel.removeClass('labelStyle4');
        this.percentageLabel.removeClass('altLabelStyle6');
        this.percentageLabel.addClass('altLabelStyle4');
      }

      if (this.percentage > 60) {
        // Sets the text to green
        this.percentageLabel.removeClass('altLabelStyle4');
        this.percentageLabel.addClass('labelStyle4');
      }

      if (this.percentage == 100) {
        // Slightly less margin         
        this.percentageLabel.removeClass('labelStyle4');
        this.percentageLabel.addClass('altLabelStyle6');
      }

      //  Sets the percentage amount
      this.vendorLabel.setText('' + Math.round(this.perc) + '%');
      this.barcodeLabel.setText('' + Math.round(this.endPercentage) + '%');
      this.percentageLabel.setText('Total Percentage: ' + this.percentage + '%');      

      this.statusLabel.setText('Status: ' + Concentrator.renderers.field('statusTypes', 'Name')(this.matchStatus));      

      this.matchForm.doLayout();
    },

    itemForm: function () {

      var id = this.currentRecord();

      this.productForm.load({
        url: Concentrator.route("GetById", "ProductMatch"),
        params: {
          productID: id.get('ProductID'),
          correspondingProductID: id.get('CorrespondingProductID'),
          vendorItemNumber: id.get('VendorItemNumber'),
          vendor: id.get('Vendor')
        }
      });

      this.correspondingProductForm.load({
        url: Concentrator.route("GetById", "ProductMatch"),
        params: {
          productID: id.get('ProductID'),
          correspondingProductID: id.get('CorrespondingProductID'),
          vendorItemNumber: id.get('CorrespondingVendorItemNumber'),
          vendor: id.get('CorrespondingVendor')
        }
      });

      this.image = new Ext.form.Label({
        html: '<img src="' + Concentrator.GetImageUrl(id.get('MediaPath'), 95, 95, true, id.get('MediaUrl')) + '" />',
        cls: 'MediaImage',
        region: 'west'
      });

      this.displayProductID = new Ext.form.Label({
        html: 'Product ID:' + ' ' + id.get('ProductID'),
        cls: 'ConcentratorId',
        width: 250
      });

      this.displayVendor = new Ext.form.Label({
        html: 'Vendor:' + ' ' + id.get('Vendor') + '(' + id.get('VendorCount') + ')',
        cls: 'ConcentratorId',
        width: 250
      });

      this.displayVendorItemNumber = new Ext.form.Label({
        html: 'Vendor item number:' + ' ' + id.get('VendorItemNumber'),
        cls: 'VendorItemNumber',
        width: 250
      });

      this.displayProductDescription = new Ext.form.Label({
        html: id.get('Description'),
        cls: 'ProductDescription',
        width: 250,
        height: 100
      });

      this.correspondingImage = new Ext.form.Label({
        html: '<img src="' + Concentrator.GetImageUrl(id.get('CorrespondingMediaPath'), 95, 95, true, id.get('CorrespondingMediaUrl')) + '" />',
        //html: '<a href="#" onclick="Concentrator.ViewInstance.open(null, null, new Concentrator.ProductBrowserFactory({ productID: '+id.get('CorrespondingProductID')+' }) )"><img src="' + Concentrator.GetImageUrl(id.get('CorrespondingMediaUrl'), 95, 95, true) + '" /></a>',
        cls: 'CorrespondingMediaImage',
        region: 'west'
      });

      this.displayCorrespondingProductID = new Ext.form.Label({
        html: 'Product ID:' + ' ' + id.get('CorrespondingProductID'),
        cls: 'CorrespondingConcentratorId',
        width: 250
      });

      this.displayCorrespondingVendor = new Ext.form.Label({
        html: 'CorrespondingVendor:' + ' ' + id.get('CorrespondingVendor') + '(' + id.get('CorrespondingVendorCount') + ')',
        cls: 'ConcentratorId',
        width: 250
      });

      this.displayCorrespondingVendorItemNumber = new Ext.form.Label({
        html: 'Vendor item number:' + ' ' + id.get('CorrespondingVendorItemNumber'),
        cls: 'CorrespondingVendorItemNumber',
        width: 250
      });

      this.displayCorrespondingProductDescription = new Ext.form.Label({
        html: id.get('CorrespondingDescription'),
        cls: 'CorrespondingProductDescription',
        width: 250,
        height: 100
      });

      this.headerColLeft.add([this.image,
                              this.displayProductID,
                              this.displayVendor,
                              this.displayVendorItemNumber,
                              this.displayProductDescription]);

      this.headerColRight.add([this.correspondingImage,
                               this.displayCorrespondingVendor,
                               this.displayCorrespondingProductID,
                               this.displayCorrespondingVendorItemNumber,
                               this.displayCorrespondingProductDescription]);

      this.populateMiddlePanel();

    },

    populateHeaderPanel: function () {

      var self = this;

      var id = self.currentRecord();

      self.image = new Ext.form.Label({
        html: '<img src="' + Concentrator.GetImageUrl(id.get('MediaUrl'), 95, 95, true) + '" />',
        cls: 'MediaImage',
        region: 'west'
      });

      self.displayProductID = new Ext.form.Label({
        html: 'Product ID:' + ' ' + id.get('ProductID'),
        cls: 'ConcentratorId',
        width: 250
      });

       self.displayVendor = new Ext.form.Label({
        html: 'Vendor:' + ' ' + id.get('Vendor') + '(' + id.get('VendorCount') + ')',
        cls: 'ConcentratorId',
        width: 250
      });

      self.displayVendorItemNumber = new Ext.form.Label({
        html: 'Vendor item number:' + ' ' + id.get('VendorItemNumber'),
        cls: 'VendorItemNumber',
        width: 250
      });

      self.displayProductDescription = new Ext.form.Label({
        html: id.get('Description'),
        cls: 'ProductDescription',
        width: 250,
        height: 100
      });

      self.correspondingImage = new Ext.form.Label({
        html: '<img src="' + Concentrator.GetImageUrl(id.get('CorrespondingMediaUrl'), 95, 95, true) + '" />',
        cls: 'CorrespondingMediaImage',
        region: 'west'
      });

      self.displayCorrespondingProductID = new Ext.form.Label({
        html: 'Product ID:' + ' ' + id.get('CorrespondingProductID'),
        cls: 'CorrespondingConcentratorId',
        width: 250
      });

     self.displayCorrespondingVendor = new Ext.form.Label({
        html: 'CorrespondingVendor:' + ' ' + id.get('CorrespondingVendor') + '(' + id.get('CorrespondingVendorCount') + ')',
        cls: 'ConcentratorId',
        width: 250
      });

      self.displayCorrespondingVendorItemNumber = new Ext.form.Label({
        html: 'Vendor item number:' + ' ' + id.get('CorrespondingVendorItemNumber'),
        cls: 'CorrespondingVendorItemNumber',
        width: 250
      });

      self.displayCorrespondingProductDescription = new Ext.form.Label({
        html: id.get('CorrespondingDescription'),
        cls: 'CorrespondingProductDescription',
        width: 250,
        height: 100
      });

      self.headerColLeft.removeAll();
      self.headerColRight.removeAll();

      self.headerColLeft.add([self.image,
                              self.displayProductID,
                              self.displayVendor,
                              self.displayVendorItemNumber,
                              self.displayProductDescription]);

      self.headerColRight.add([self.correspondingImage,
                               self.displayCorrespondingProductID,
                               self.displayCorrespondingVendor,
                               self.displayCorrespondingVendorItemNumber,
                               self.displayCorrespondingProductDescription]);

      self.headerColLeft.doLayout();
      self.headerColRight.doLayout();

    },

    moveBackward: function () {
      var myMask = new Ext.LoadMask(this.id, { msg: "Previous match is loading..." });
      myMask.show();

      var self = this;
      var onNext = function () {

        var oneIsFinished = false;

        self.productForm.load({
          url: Concentrator.route("GetById", "ProductMatch"),
          params: {
            productID: self.currentRecord().get('ProductID'),
            correspondingProductID: self.currentRecord().get('CorrespondingProductID'),
            vendorItemNumber: self.currentRecord().get('VendorItemNumber')
          },
          success: function () {
            myMask.hide();
          }
        });

        self.matchForm.doLayout();
        self.populateMiddlePanel();

        self.correspondingProductForm.load({
          url: Concentrator.route("GetById", "ProductMatch"),
          params: {
            productID: self.currentRecord().get('ProductID'),
            correspondingProductID: self.currentRecord().get('CorrespondingProductID'),
            vendorItemNumber: self.currentRecord().get('CorrespondingVendorItemNumber')
          }
        });

        self.populateHeaderPanel();

      }
      Concentrator.ui.ProductMatchWizard.superclass.moveBackward.call(this, onNext);
    },

    moveForward: function () {

      var myMask = new Ext.LoadMask(this.id, { msg: "Next match is loading..." });

      myMask.show();

      var self = this;

      var onNext = function () {

        self.productForm.load({
          url: Concentrator.route("GetById", "ProductMatch"),
          params: {
            productID: self.currentRecord().get('ProductID'),
            correspondingProductID: self.currentRecord().get('CorrespondingProductID'),
            vendorItemNumber: self.currentRecord().get('VendorItemNumber')
          }
        });

        self.matchForm.doLayout();

        self.correspondingProductForm.load({
          url: Concentrator.route("GetById", "ProductMatch"),
          params: {
            productID: self.currentRecord().get('ProductID'),
            correspondingProductID: self.currentRecord().get('CorrespondingProductID'),
            vendorItemNumber: self.currentRecord().get('CorrespondingVendorItemNumber')
          },
          success: function () {
            myMask.hide();
          }
        });

        self.backButton.setDisabled(false);

        self.populateMiddlePanel();

        self.populateHeaderPanel();
      }

      Concentrator.ui.ProductMatchWizard.superclass.moveForward.call(this, onNext);
    },
    match: function () {

      var that = this;

      Diract.request({
        url: Concentrator.route("CreateMatch", "ProductMatch"),
        params: {
          productID: this.currentRecord().get('ProductID'),
          correspondingProductID: this.currentRecord().get('CorrespondingProductID'),
          productMatchID: this.currentRecord().get('ProductMatchID')
        },
        success: function () {
          that.moveForward();
        }
      });

    },
    nonMatch: function () {

      var self = this;

      Diract.request({
        url: Concentrator.route("RemoveMatch", "ProductMatch"),
        params: {
          productID: this.currentRecord().get('ProductID'),
          correspondingProductID: this.currentRecord().get('CorrespondingProductID'),
          productMatchID: this.currentRecord().get('ProductMatchID')
        },
        success: function () {
        }
      });

      Concentrator.ui.ProductMatchWizard.superclass.noMatch.call(this, self);

    },
    callback: function (data) {

//      Diract.request({
//        url: Concentrator.route("MapProducts", "ProductMatching"),
//        params: data,
//        success: function () {
//          self.grid.store.reload();
//        }
//      });
    }
  });

  return wiz;
})();