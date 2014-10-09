Concentrator.ui.ProductGroupMediaSelector = (function () {

  var groupSelector = Ext.extend(Ext.Window, {
    title: 'Product group media',
    modal: true,
    layout: 'fit',
    width: 326,
    height: 260,
    constructor: function (config) {

      Ext.apply(this, config);

      this.addItemsToWindowObject();

      Concentrator.ui.ProductGroupMediaSelector.superclass.constructor.call(this, config);
    },

    addItemsToWindowObject: function () {
      var that = this;
      var id = 0;
      this.imageForm = new Diract.ui.Form({
        fileUpload: true,
        url: Concentrator.route('AddImage', 'ProductGroup'),
        items: [
            {
              xtype: 'productgroup',
              fieldLabel: 'Product group',
              enableCreateItem: false,
              allowBlank: false,
              listeners: {
                'select': function (field, record, index) {
                  id = record.get('ProductGroupID');
                  that.refreshImage(id);
                }
              }
            },
             {
               xtype: 'fileuploadfield',
               emptyText: 'Select Image...',
               name: 'ImagePath',
               hiddenName: 'ImagePath',
               fieldLabel: 'Image:',
               buttonText: '',
               width: 183,
               buttonCfg: { iconCls: 'upload-icon' }
             }
          ],
        success: function () {
          that.refreshImage(id);
        }
      });
      this.items = [this.imageForm]
    },

    /**
    Refreshes an image placeholder
    */
    refreshImage: function (productGroupID) {

      if (this.imageLabel) {
        this.imageForm.remove(this.imageForm.remove(this.imageLabel.id));
      }

      this.imageLabel = new Ext.Component({
        html: '<center><img src="' + Concentrator.route('GetImage', 'ProductGroup', { productGroupID: productGroupID, width: 100, height: 100, guid: Ext.guid() }) + '" alt="product-group-image"/></center>',
        id: 'product-group-image'
      });

      this.imageForm.add(this.imageLabel);
      this.imageForm.doLayout();
      this.doLayout();

    }
  });
  return groupSelector;
})();