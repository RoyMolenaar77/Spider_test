/**
Contains all shared form configurations
*/
Concentrator.FormConfigurationsRepository = function () {

  /**
  Used to create a new price rounding calculation
  */
  this.newPriceRoundingCalculation = {
    url: Concentrator.route('Create', 'ContentPriceCalculation'),
    items: [
      {
        xtype: 'textfield',
        name: 'Name',
        fieldLabel: 'Name',
        allowBlank: false,
        width: 250

      },
      {
        xtype: 'formula',
        name: 'Calculation',
        fieldLabel: 'Calculation',
        allowBlank: false
      }
    ]
  };

  this.newVendorPriceRoundingCalculation = {
    url: Concentrator.route('Create', 'VendorPriceCalculation'),
    items: [
      {
        xtype: 'textfield',
        name: 'Name',
        fieldLabel: 'Name',
        allowBlank: false,
        width: 250

      },
      {
        xtype: 'formula',
        name: 'Calculation',
        fieldLabel: 'Calculation',
        allowBlank: false
      }
    ]
  };

  /**
  Used when adding a value to an existing attribute
  */
  this.existingAttributeValue = {
    items: [{
      xtype: 'attribute',
      name: 'AttributeID',
      fieldLabel: 'Attribute'
    },
    {
      xtype: 'textfield',
      name: 'Value',
      fieldLabel: 'Value'
    },
    {
      xtype: 'language'
    }
    ]
  };

  this.productCompetitor = {
    url: Concentrator.route('GetProductCompetitorList', 'ProductCompetitor'),
    items: [
    { xtype: 'textfield', name: 'Name', allowBlank: false, fieldLabel: 'Product Competitor Name' },
    { xtype: 'numberfield', name: 'Reliability', allowBlank: false, fieldLabel: 'Reliability' },
    { xtype: 'datefield', name: 'DeliveryDate', allowBlank: false, fieldLabel: 'Delivery Date' },
    { xtype: 'numberfield', name: 'ShippingCostPerOrder', allowBlank: false, fieldLabel: 'Shipping Cost Per Order' },
    { xtype: 'numberfield', name: 'ShippingCost', allowBlank: false, fieldLabel: 'Shipping Cost' }
    ]
  };


  this.newProductGroup = {
    url: Concentrator.route('Create', 'ProductGroup'),
    items: [
     {
       xtype: 'numberfield',
       name: 'Score',
       allowBlank: false,
       width: 183,
       fieldLabel: 'Score',
       value: '0'
     },
      {
        xtype: 'languageManager',
        fieldLabel: 'Name'
      }
    ]
  };

  this.newRelatedProduct = {
    url: Concentrator.route('Create', 'RelatedProduct'),
    items: [
      {
        xtype: 'product',
        hiddenName: 'RelatedProductID',
        name: 'RelatedProductID',
        allowBlank: false,
        width: 183,
        fieldLabel: 'Related Product'
      },
      {
        xtype: 'checkbox',
        name: 'IsActive',
        allowBlank: false,
        width: 183,
        fieldLabel: 'Active',
        checked: true
      },
      {
        xtype: 'textfield',
        name: 'Index',
        allowBlank: true,
        width: 183,
        fieldLabel: 'Score/Index'
      },
      {
        xtype: 'vendorSelect',
        name: 'VendorID',
        allowBlank: false,
        width: 183,
        fieldLabel: 'Vendor',
        value: Concentrator.VendorIDForRelatedProducts
      },
      {
        xtype: 'relatedProductTypeSelect',
        name: 'RelatedProductTypeID',
        allowBlank: false,
        width: 183,
        fieldLabel: 'Related Product Type',
        value: Concentrator.RelatedProductTypeIDForRelatedProducts
      }
    ]
  };

  this.newProductCompetitorLedger = {
    url: Concentrator.route('CreateProductCompetitorLedger', 'ProductCompetitor'),
    items: [
     {
       xtype: 'product',
       name: 'Product',
       allowBlank: false,
       width: 183,
       fieldLabel: 'Product'
     },
     {
       xtype: 'productCompetitor',
       name: 'Competitor',
       allowBlank: false,
       width: 183,
       fieldLabel: 'Competitor'
     },
     {
       xtype: 'productSources',
       //name: 'ProductCompareSource',
       allowBlank: false,
       width: 183,
       fieldLabel: 'ProductCompareSource'
     },
     {
       xtype: 'deliveryStatus',
       name: 'Stock',
       allowBlank: false,
       width: 183,
       fieldLabel: 'Stock'
     },
     {
       xtype: 'numberfield',
       name: 'Price',
       allowBlank: false,
       width: 183,
       fieldLabel: 'Price'
     },
     {
       xtype: 'checkbox',
       name: 'InTaxPrice',
       allowBlank: false,
       width: 183,
       fieldLabel: 'InTaxPrice'
     },
     {
       xtype: 'checkbox',
       name: 'IncludeShippingCost',
       allowBlank: false,
       width: 183,
       fieldLabel: 'IncludeShippingCost'
     }
    ]
  };

  this.newProductAttributeValueGroup = {
    url: Concentrator.route('Create', 'ProductAttributeValueGroup'),
    items: [
        {
          xtype: 'languageManager',
          fieldLabel: 'Group name'
        },
        {
          xtype: 'numberfield',
          name: 'Score',
          allowBlank: false,
          width: 183,
          fieldLabel: 'Index',
          value: '0'
        },
        {
          xtype: 'connector',
          allowBlank: true,
          value: Concentrator.user.connectorID,
          fieldLabel: 'Connector'
        }
    ]
  };

  /**
  Used when creating a new product attribute group
  */
  this.newProductGroupAttribute = {
    url: Concentrator.route('Create', 'ProductAttributeGroup'),
    items: [
      {
        xtype: 'languageManager',
        fieldLabel: 'Name'
      },
      {
        xtype: 'numberfield',
        name: 'Index',
        allowBlank: false,
        width: 183,
        fieldLabel: 'Index',
        value: '0'
      },
      {
        xtype: 'textfield',
        name: 'GroupCode',
        allowBlank: false,
        width: 183,
        fieldLabel: 'Code'
      },
      {
        xtype: 'connector',
        allowBlank: true,
        fieldLabel: 'Connector'
      },
      {
        xtype: 'vendor',
        allowBlank: false,
        fieldLabel: 'Vendor'
      }
    ]
  }


  /**
  Used when creating a new product attribute
  */
  this.newProductAttribute = {
    url: Concentrator.route('Create', 'ProductAttribute'),
    items: [
     {
       fieldLabel: 'Name',
       xtype: 'languageManager'
     },

      { xtype: 'productattributegroup', allowBlank: false, fieldLabel: 'Attribute Group' },
      {
        xtype: 'numberfield',
        name: 'Index',
        allowBlank: false,
        width: 200,
        fieldLabel: 'Index',
        value: '0'
      },
      {
        xtype: 'textfield',
        allowBlank: true,
        fieldLabel: 'Format',
        name: 'FormatString',
        width: 200
      },
      {
        xtype: 'textfield',
        allowBlank: true,
        fieldLabel: 'Data type',
        name: 'DataType',
        width: 200
      },
      {
        xtype: 'checkbox',
        allowBlank: false,
        name: 'IsVisible',
        fieldLabel: 'Visible'
      },
      {
        xtype: 'checkbox',
        allowBlank: false,
        name: 'IsSearchable',
        fieldLabel: 'Searchable'
      },
       {
         xtype: 'checkbox',
         allowBlank: false,
         name: 'Mandatory',
         fieldLabel: 'Mandatory'
       },
       {
         xtype: 'checkbox',
         allowBlank: false,
         name: 'IsConfigurable',
         fieldLabel: 'Configurable'
       },
      {
        xtype: 'textfield',
        name: 'Sign',
        allowBlank: true,
        fieldLabel: 'Sign',
        width: 200
      },
      {
        xtype: 'textfield',
        name: 'DefaultValue',
        allowBlank: true,
        fieldLabel: 'Default Value',
        width: 200
      },
      {
        xtype: 'vendor',
        allowBlank: false,
        fieldLabel: 'Vendor',
        width: 200,
        name: 'VendorID'
      },
      {
        xtype: 'fileuploadfield',
        id: 'form-file',
        emptyText: 'Select Image...',
        name: 'AttributePath',
        hiddenName: 'AttributePath',
        fieldLabel: 'Image',
        buttonText: '',
        width: 200,
        buttonCfg: { iconCls: 'upload-icon' }
      }

    ]
  }
};