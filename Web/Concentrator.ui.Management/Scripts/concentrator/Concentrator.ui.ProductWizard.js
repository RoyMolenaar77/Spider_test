/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />


Concentrator.ui.ProductWizard = (function () {

  var panel = Ext.extend(Ext.Panel, {

    layout: 'border',
    border: false,

    /**
    Constructor
    */
    constructor: function (config) {

      Ext.apply(this, config);
      this.itemsCache = {};

      this.initializeContainers();
      this.initWizardMenu(this.west);

      Concentrator.ui.ProductWizard.superclass.constructor.call(this, config);
      this.openItem('general');
    }
    ,

    /**
    Initializes the main containers 
    */
    initializeContainers: function () {

      this.backButton = new Ext.Button({
        text: 'Back',
        handler: (function () {
          this.moveBackward();
        }).createDelegate(this),
        disabled: true,
        iconCls: 'skip-backward'
      });

      this.nextButton = new Ext.Button({
        text: 'Next',
        handler: (function () {
          this.moveForward();
        }).createDelegate(this),
        iconCls: 'skip-forward'

      });

      this.commitButton =
      new Ext.Button({
        text: 'Save',
        handler: (function () {
          this.save();
        }).createDelegate(this),
        iconCls: 'disk',
        visible: false,
        hidden: true

      });

      this.bbar = [
          '->',
          this.backButton,
          this.nextButton,
          this.commitButton
      ];

      this.center = new Ext.Panel({
        region: 'center',
        padding: 20,
        closable: true,
        autoDestroy: false,
        autoScroll: true,
        border: true

      });

      this.west = new Ext.Panel({
        region: 'west',
        width: 150,
        margins: '0 0 0 0',
        padding: '20px 0 0 10px',
        collapseMode: 'mini',
        split: true,
        border: true
      });

      this.items = [this.center, this.west];
    },


    /**
    Saves all data from the wizard
    */
    save: function () {
      var params = {};

      for (var key in this.itemsCache) {
        Ext.apply(params, this.itemsCache[key].getForm().getValues());
      }

      //send to server
      var that = this;
      Diract.request({
        url: Concentrator.route('CreateFull', 'Product'),
        params: params,
        success: function () {
          if (that.creationAction) that.creationAction();
        }
      });
    },


    /** 
    Advances the wizard to the next step
    */
    moveForward: function () {

      var currentItemID;
      nextItemId = '',
      currIndex = 0;


      for (var key in this.itemsCache) {
        if (this.itemsCache[key] == this.currentItem) currentItemID = key;
      }

      for (var i = 0; i < this.sequence.length; i++) {
        if (this.sequence[i] === currentItemID) {
          nextItemId = this.sequence[i + 1];
          currIndex = i;
          break;
        }
      }

      //load next item
      this.openItem(nextItemId);

      //change buttons state

      if (currIndex + 1 > 0) this.backButton.setDisabled(false);
      if ((currIndex + 1) == this.sequence.length - 1) { //eof wizard

        this.nextButton.setDisabled(true);
        this.commitButton.show();
      }




      //add finish button
    },


    /** 
    Advances the wizard to the previous step
    */
    moveBackward: function () {
      var currentItemID = '',
      previousItemId = '',
      currIndex = 0;

      for (var key in this.itemsCache) {
        if (this.itemsCache[key] == this.currentItem) currentItemID = key;
      }

      for (var i = 0; i < this.sequence.length; i++) {
        if (this.sequence[i] === currentItemID && i > 0) {
          previousItemId = this.sequence[i - 1];
          currIndex = i;
          break;
        }
      }

      this.openItem(previousItemId);

      //if (currIndex + 1 > 0) this.backButton.setDisabled(false);
      //if ((currIndex + 1) == this.sequence.length) this.nextButton.setDisabled(true);      

      if (currIndex - 1 == this.sequence.length - 2) {
        this.nextButton.setDisabled(false);
        this.commitButton.hide();
      }

      if (currIndex - 1 == 0) this.backButton.setDisabled(true);

    },

    initWizardMenu: function (menuPanel) {
      var that = this;
      var generalStepId = 'general',
          localizationsStepId = 'localization',
          typeStepId = 'type';



      this.sequence = [
        generalStepId, localizationsStepId, typeStepId
      ]

      menuPanel.add([
    {
      xtype: 'menuitem',
      text: 'General',
      id: generalStepId,
      overCls: 'menu-item-over',
      handler: that.handleMenuClick.createDelegate(this),
      activeClass: 'x-menu-item-active'
    },
    {
      xtype: 'menuitem',
      text: 'Localization',
      id: localizationsStepId,
      overCls: 'menu-item-over',
      handler: that.handleMenuClick.createDelegate(this),
      activeClass: 'x-menu-item-active'
    },

    {
      xtype: 'menuitem',
      text: 'Product type',
      id: typeStepId,
      overCls: 'menu-item-over',
      handler: that.handleMenuClick.createDelegate(this),
      activeClass: 'x-menu-item-active'
    }
      ]);
    },

    handleMenuClick: function (item) {

      if (item.el) this.openItem(item.el.id);
      else this.openItem(el);
      //this.openItem(item);
    },

    openItem: function (item) {
      var panel;

      switch (item) {
        case 'general':
          panel = this.getGeneralInfoPanel();
          break;
        case 'localization':
          panel = this.getTranslationItems();
          break;
        case 'type':
          panel = this.getTypePanel();
          break;

      }
      var cachedItem = this.getOrCacheItem(item, panel);

      this.center.add(cachedItem);

      this.center.doLayout();


    },

    /**
    Stores the selected item for later viewing
    */
    getOrCacheItem: function (id, panel) {
      var visible,
          active;

      if (!this.itemsCache[id]) {
        this.itemsCache[id] = panel;

      }

      for (var key in this.itemsCache) {
        var item = this.itemsCache[key];

        if (key !== id) {
          visible = false;
          active = false;
        }
        else {
          visible = true;
          active = true;
        }

        var menuItem = Ext.getCmp(key);

        if (active) {
          menuItem.addClass('active-bold');
          menuItem.setIconClass('arrow-current');
        }
        else {

          menuItem.removeClass('active-bold');
          menuItem.setIconClass('');
        }

        item.setVisible(visible);
      }

      //set to actual cur item
      item = this.itemsCache[id];

      this.currentItem = item;
      return item;
    },

    getGeneralInfoPanel: function () {
      this.vendorItemNumberField = new Ext.form.TextField({
        fieldLabel: 'Vendor item number',
        name: 'VendorItemNumber',
        width: 218
      });

      this.customItemNumberField = new Ext.form.TextField({

        fieldLabel: 'Custom item number',
        name: 'CustomItemNumber',
        width: 218
      });

      return new Ext.FormPanel({
        layout: 'form',
        border: false,
        items: [
          {
            xtype: 'vendor',
            fieldLabel: 'Vendor',
            width: 210,
            allowBlank: false,
            searchUrl: Concentrator.route('SearchDefault', 'Vendor')
          },
          this.vendorItemNumberField,
          this.customItemNumberField,
          {
            xtype: 'productgroup',
            fieldLabel: 'Product group'
          },
          {
            xtype: 'brand',
            fieldLabel: 'Brand'
          }

        ]
      });
    },

    getTranslationItems: function () {

      this.nameField = new Concentrator.LanguageManager({
        fieldLabel: 'Name'
      });

      this.shortDescriptionField = new Concentrator.LanguageManager({
        fieldLabel: 'Short description'
      });

      this.longDescriptionField = new Concentrator.LanguageManager({
        fieldLabel: 'Long description'
      });

      return new Ext.FormPanel({
        layout: 'form',
        border: false,
        items: [
        this.nameField,
        this.shortDescriptionField,
        this.longDescriptionField
        ]
        //        getValues: (function () {
        //          var name = {},
        //          shortDescription = {},
        //          shortDescVals = this.shortDescriptionField.getHiddens(),
        //          longDescription = {},
        //          longDescVals = this.longDescriptionField.getHiddens(),
        //          vals = this.nameField.getHiddens();

        //          for (var key in vals) {
        //            name[key] = vals[key];
        //          }

        //          for (var key in shortDescVals) {
        //            shortDescription[key] = shortDescVals[key];
        //          }

        //          for (var key in longDescVals) {
        //            longDescription[key] = longDescVals[key];
        //          }



        //          return Ext.apply(Ext.apply(Ext.apply({}, name), shortDescription), longDescription)


        //        }).createDelegate(this)
      });
    },

    showPricingForSimpleProduct: function () {
      if (!this.priceAndStock) {
        this.priceAndStock = new Ext.form.FieldSet({
          title: 'Set price and stock',
          border: true,
          items: [
        {
          xtype: 'numberfield',
          fieldLabel: 'Quantity on stock',
          name: 'QuantityOnStock',
          allowBlank: false,
          width: 201
        },
        {
          xtype: 'numberfield',
          fieldLabel: 'Unit price',
          name: 'UnitPrice',
          allowBlank: false,
          width: 201
        },
        {
          xtype: 'numberfield',
          fieldLabel: 'Cost price',
          name: 'CostPrice',
          allowBlank: false,
          width: 201
        },
        {
          xtype: 'datefield',
          fieldLabel: 'Valid to (optional)',
          name: 'ValidTo',
          allowBlank: true,
          width: 201
        },
        {
          xtype: 'numberfield',
          fieldLabel: 'Minimum Quantity',
          name: 'MinimumQuantity',
          allowBlank: true,
          width: 201
        },
        {
          xtype: 'stockStatus',
          fieldLabel: 'Concentrator Status(Price)',
          name: 'ConcentratorStatusID',
          hiddenName: 'ConcentratorPriceStatus',
          allowBlank: false,
          width: 201
        },
         {
           xtype: 'stockStatus',
           fieldLabel: 'Concentrator Status(Stock)',
           name: 'ConcentratorStatusID',
           hiddenName: 'ConcentratorStatusID',
           allowBlank: false,
           width: 201
         },
        {
          xtype: 'vendorStockType',
          fieldLabel: 'Vendor Stock Type',
          name: 'VendorStockType',
          hiddenName: 'VendorStockType',
          allowBlank: false,
          width: 201
        }
          ]

        });
        this.currentItem.add(this.priceAndStock);
        this.currentItem.doLayout();
      }
      else {
        this.priceAndStock.setVisible(true);
      }

      if (this.attributeSet)
        this.attributeSet.setVisible(false);
    },

    getTypePanel: function () {

      return new Ext.FormPanel({
        border: false,
        items: [
          {
            xtype: 'fieldset',
            flex: 1,
            layout: 'anchor',
            title: 'Product type',
            defaultType: 'radio',
            defaults: {
              anchor: '100%',
              hideEmptyLabel: false
            },
            items:
            [
              {
                id: 'radio-simple',

                inputValue: 'true',
                name: 'isSimple',
                boxLabel: 'Simple product',
                listeners: {
                  'check': (function (radio) {
                    if (radio.checked)
                      this.showPricingForSimpleProduct();
                  }).createDelegate(this)
                }
              },
              {
                id: 'radio-configured',

                inputValue: 'false',
                name: 'isSimple',
                boxLabel: 'Configurable product',
                listeners: {
                  'check': (function (radio) {
                    if (radio.checked)
                      this.showAttributeSelector();
                  }).createDelegate(this)
                }
              }
            ]
          }
        ]
      });
    },


    showAttributeSelector: function () {
      var that = this;


      function getFields() {
        return [
      {
        xtype: 'textfield', name: 'Size', fieldLabel: 'Size'
      }
      ,
      {
        xtype: 'textfield', name: 'Color', fieldLabel: 'Color'
      }
      ,
      {
        xtype: 'numberfield', name: 'Stock', fieldLabel: 'Stock'
      },
      {
        xtype: 'numberfield', name: 'UnitPrice', fieldLabel: 'Unit price'
      },
      {
        xtype: 'numberfield', name: 'CostPrice', fieldLabel: 'Cost price'
      },
      {
        xtype: 'textfield', name: 'SKU', fieldLabel: 'SKU'
      },
      {
        xtype: 'stockStatus', name: 'ConcentratorPriceStatus', fieldLabel: 'Concentrator Status',
      },
      {
        xtype: 'vendorStockType', name: 'VendorStockType', fieldLabel: 'Vendor Stock Type'
      },
      {
        xtype: 'numberfield',
        fieldLabel: 'Minimum Quantity',
        name: 'MinimumQuantity',
        allowBlank: true

      },
      {
        xtype: 'button', text: 'Add more fields', listeners: {
          'click': function () {

            that.skuSet.add(getFields());
            that.skuSet.doLayout();
          }
        }
      }
        ];
      }



      if (!this.attributeSet) {
        this.skuSet = new Ext.form.FieldSet({

          title: 'Configure SKUs',

          items: getFields()
        });


        this.attributeSet = new Ext.form.FieldSet({
          title: 'Configure product',
          items: [

           this.skuSet
          ]
        });


        this.currentItem.add(this.attributeSet);
        this.currentItem.doLayout();
      }
      else {
        this.attributeSet.setVisible(true);
      }

      if (this.priceAndStock)
        this.priceAndStock.setVisible(false);

    },

    getMediaPanel: function () {
      return new Ext.FormPanel({
        layout: 'form',
        border: false,
        items: [
        new Ext.form.TextField({
          fieldLabel: 'Language',
          name: 'Language'
        })
        ]

      });
    }
  });

  return panel;
})();