/// <reference path="~/Content/js/ext/ext-base-debug.js" /> 
/// <reference path="~/Content/js/ext/ext-all-debug.js" />
ConcentratorEditorRepository = function () {

  this.ConcentratorStatusSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'StatusID',
    displayField: 'ConcentratorStatus',
    searchUrl: Concentrator.route('Search', 'ProductStatus'),
    enableCreateItem: true,
    singularObjectName: 'Product Status',
    formItems: [
      {
        xtype: 'textfield',
        fieldLabel: 'Status',
        allowBlank: false,
        inputType: 'text',
        name: 'status'
      }
    ],
    createUrl: Concentrator.route('Create', 'ProductStatus')
  });

  this.ConnectorStatusSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ConnectorStatusID',
    displayField: 'ConnectorStatus',
    searchUrl: Concentrator.route('ConnectorSearch', 'ProductStatus'),
    enableCreateItem: true,
    singularObjectName: 'Product Status',
    formItems: [
      {
        xtype: 'textfield',
        fieldLabel: 'Status',
        allowBlank: false,
        inputType: 'text',
        name: 'status'
      }
    ],
    createUrl: Concentrator.route('Create', 'ProductStatus')
  });

  this.ConnectorTypes = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.connectorType,
    label: 'Connector Type',
    displayField: 'ConnectorTypeName'
    //    listeners: {
    //      'expand': function () {
    //        debugger;
    //      }
    //    }
    //    valueField: 'ConnectorTypeID',
    //    displayField: 'ConnectorTypeName',
    //    searchUrl: Concentrator.route('GetConnectorTypes', 'ConnectorRelation'),
    //    allowBlank: false
  });

  this.Connectors = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ID',
    displayField: 'Name',
    searchUrl: Concentrator.route('GetStore', 'Connector'),
    allowBlank: false
  });

  this.ConnectorSelectWithFilter = Ext.extend(Diract.ui.Select, {
    store: Concentrator.stores.connectorsWithFilter,
    label: 'Connector',
    displayField: 'Name',
    allowBlank: false
  });

  this.OutboundMessageTypes = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'OutboundMessageTypeID',
    displayField: 'OutboundMessageTypeName',
    searchUrl: Concentrator.route('GetOutboundMessageTypes', 'ConnectorRelation'),
    allowBlank: false
  });

  this.AccountPrivileges = Ext.extend(Diract.ui.Select, {
    store: Concentrator.stores.accountPrivilegeStore,
    label: 'Account Privileges',
    displayField: 'AccountPrivilegeName',
    allowBlank: false
  });

  this.DeliveryStatuses = Ext.extend(Diract.ui.Select, {
    store: Concentrator.stores.deliveryStatusesStore,
    label: 'Delivery Status',
    displayField: 'DeliveryStatusName',
    allowBlank: false
  });

  this.ProviderTypes = Ext.extend(Diract.ui.Select, {
    store: Concentrator.stores.providerTypeStore,
    label: 'Provider Type',
    displayField: 'ProviderTypeName',
    allowBlank: false
  });

  this.FtpTypes = Ext.extend(Diract.ui.Select, {
    store: Concentrator.stores.ftpTypeStore,
    label: 'Ftp Type',
    displayField: 'FtpTypeName',
    allowBlank: false
  });

  this.XtractTypes = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.xtractTypes,
    label: 'Xtract Type',
    displayField: 'XtractTypeName'
  });

  this.ProductGroupSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ProductGroupID',
    displayField: 'ProductGroupName',
    searchUrl: Concentrator.route('Search', 'ProductGroup'),
    createFormConfig: Concentrator.FormConfigurations.newProductGroup,
    singularObjectName: 'Product Group',
    enableCreateItem: true,
    formPanelWidth: 363,
    formPanelHeight: 400,
    hiddenName: 'ProductGroupID'
  });

  this.SelectorSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'SelectorID',
    displayField: 'Name',
    searchUrl: Concentrator.route('Search', 'Selector'),
    formItems: [
      {
        xtype: 'textfield',
        fieldLabel: 'Name',
        name: 'Name',
        allowBlank: false
      }
    ],
    createUrl: Concentrator.route('Create', 'Selector'),
    enableCreateItem: true
  });

  this.LanguageSelect = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.languages,
    label: 'Language'
  });

  this.MasterGroupMappingOptionSelect = Ext.extend(Diract.ui.Select, {
    
  });

  this.SignSelect = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.signs,
    label: 'Sign'
  });

  this.PriceRuleTypeSelect = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.priceRuleTypes,
    label: 'Price rule type',
    displayField: 'Name'
  });

  this.StatusTypeSelect = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.statusTypes,
    label: 'Status type',
    displayField: 'Name'
  });

  this.ProductBarcodeTypeSelect = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.productBarcodeTypes,
    label: 'Barcode type',
    displayField: 'Name'
  });

  this.ConnectorSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ConnectorID',
    displayField: 'Name',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'Connector')
  });

  this.VendorSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'VendorID',
    displayField: 'VendorName',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'Vendor')
  });
  
  this.VendorSelect = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.vendors,
    label: 'Vendor',
    displayField: 'VendorName'
  });

  this.RelatedProductTypeSelect = Ext.extend(Diract.ui.Select, {
    allowBlank: false,
    store: Concentrator.stores.relatedProductTypes,
    label: 'Related Product Type',
    displayField: 'Type'
  });

  this.ContentVendorSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'VendorID',
    displayField: 'VendorName',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'ContentVendor')
  });

  this.BrandSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'BrandID',
    displayField: 'BrandName',
    searchUrl: Concentrator.route('Search', 'Brand'),
    enableCreateItem: true,
    formItems: [
      {
        xtype: 'textfield',
        fieldLabel: "Brand name",
        name: 'Name',
        allowBlank: false,
        inputType: 'text'
      }
      ],
    createUrl: Concentrator.route('Create', 'Brand'),
    singularObjectName: 'Brand'
  });

  this.ContentVendorSearch = Ext.extend(Diract.ui.SearchField, {
    valueField: 'VendorID',
    displayField: 'VendorName',
    allowBlank: true,
    searchUrl: Concentrator.route('Search', 'ContentVendor')
  });

  this.AttributeMaterialSearch = Ext.extend(Diract.ui.SearchField, {
    valueField: 'MaterialOptionID',
    displayField: 'MaterialAttributeOption',
    allowBlank: true,
    searchStore: Concentrator.stores.AttributeMaterialStore
  });

  this.AttributeDessinSearch = Ext.extend(Diract.ui.SearchField, {
    valueField: 'DessinOptionID',
    displayField: 'DessinAttributeOption',
    allowBlank: true,
    searchStore: Concentrator.stores.AttributeDessinStore
  });

  this.AttributePijpwijdteSearch = Ext.extend(Diract.ui.SearchField, {
    valueField: 'PijpwijdteOptionID',
    displayField: 'PijpwijdteAttributeOption',
    allowBlank: true,
    searchStore: Concentrator.stores.AttributePijpwijdteStore
  });

  this.AttributeKraagvormSearch = Ext.extend(Diract.ui.SearchField, {
    valueField: 'KraagvormOptionID',
    displayField: 'KraagvormAttributeOption',
    allowBlank: true,
    searchStore: Concentrator.stores.AttributeKraagvormStore
  });

  this.VendorStockTypeSearch = Ext.extend(Diract.ui.SearchField, {
    valueField: 'VendorStockTypeID',
    displayField: 'StockType',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'VendorStock')
  });

  this.StockStatusSearch = Ext.extend(Diract.ui.SearchField, {
    valueField: 'StatusID',
    displayField: 'Status',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'VendorAssortment')
  });

  this.OrderRuleSearch = Ext.extend(Diract.ui.SearchField, {
    valueField: 'RuleID',
    displayField: 'Name',
    multi: true,
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'OrderRule')
  });

  this.ProductSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ProductID',
    displayField: 'ProductDescription',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'Product')
  });

  this.RelatedProductSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'RelatedProductID',
    displayField: 'RelatedProductDescription',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'RelatedProduct')
  });

  this.RelatedProductTypeSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'RelatedProductTypeID',
    displayField: 'Type',
    allowBlank: false,
    searchUrl: Concentrator.route('SearchRelatedProductType', 'RelatedProduct'),
    enableCreateItem: true,
    singularObjectName: 'Related product type',
    createUrl: Concentrator.route('CreateRelatedProductType', 'RelatedProduct'),
    formItems: [
      {
        xtype: 'textfield',
        name: 'type',
        allowBlank: false,
        fieldLabel: 'Type'
      }
    ]
  });

  this.MediaTypeSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'TypeID',
    displayField: 'Type',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'MediaType'),
    enableCreateItem: true,
    singularObjectName: 'Media type',
    createUrl: Concentrator.route('Create', 'MediaType'),
    formItems: [
      {
        xtype: 'textfield',
        name: 'type',
        allowBlank: false,
        fieldLabel: 'Media Type'
      }
    ]
  });

  this.PriceCalculationSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ContentPriceCalculationID',
    displayField: 'ContentPriceCalculationName',
    searchUrl: Concentrator.route('Search', 'ContentPriceCalculation'),
    enableCreateItem: true,
    formPanelWidth: 480,
    formPanelHeight: 500,
    singularObjectName: 'Price rounding calculation',
    createFormConfig: Concentrator.FormConfigurations.newPriceRoundingCalculation
  });

  this.VendorPriceCalculationSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'VendorPriceCalculationID',
    displayField: 'VendorPriceCalculationName',
    searchUrl: Concentrator.route('Search', 'VendorPriceCalculation'),
    enableCreateItem: true,
    formPanelWidth: 480,
    formPanelHeight: 500,
    singularObjectName: 'Price rounding calculation',
    createFormConfig: Concentrator.FormConfigurations.newVendorPriceRoundingCalculation
  });

  this.AttributeGroupSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ProductAttributeGroupID',
    displayField: 'AttributeGroupName',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'ProductAttributeGroup'),
    enableCreateItem: true,
    formPanelWidth: 400,
    formPanelHeight: 542,
    singularObjectName: 'Attribute group',
    createFormConfig: Concentrator.FormConfigurations.newProductGroupAttribute
  });

  this.ProductAttributeValueGroupSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'AttributeValueGroupID',
    displayField: 'AttributeValueGroup',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'ProductAttributeValueGroup'),
    enableCreateItem: true,
    singularObjectName: 'Attribute value group',
    createFormConfig: Concentrator.FormConfigurations.newProductAttributeValueGroup,
    formPanelWidth: 464,
    formPanelHeight: 336
  });

  this.Users = Ext.extend(Diract.ui.Select, {
    hiddenName: 'UserID',
    label: 'User',
    displayField: 'Name',
    allowBlank: false,
    emptyText: 'Please select a user',
    name: 'UserID',
    store: Concentrator.stores.users
  });

  this.CurrencyField = Ext.extend(Ext.form.NumberField, {
    constructor: function (config) {
      Ext.apply(this, config);

      this.fieldLabel = this.fieldLabel + '(&#8364)';
      this.superclass.constructor.call(this, config);
    }
  });

  this.AttributeSearch = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'AttributeID',
    displayField: 'AttributeName',
    allowBlank: false,
    searchUrl: Concentrator.route('Search', 'ProductAttribute'),
    enableCreateItem: true,
    formPanelWidth: 400,
    formPanelHeight: 400,
    singularObjectName: 'Attribute',
    createFormConfig: Concentrator.FormConfigurations.newProductAttribute
  });

  this.productSources = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ProductCompareSourceID',
    displayField: 'Name',
    searchUrl: Concentrator.route('GetPcSourceStore', 'ProductCompetitor'),
    allowBlank: false
  });

  this.ProductCompetitor = Ext.extend(Diract.ui.SearchBox, {
    valueField: 'ProductCompetitorID',
    displayField: 'Name',
    searchUrl: Concentrator.route('Search', 'ProductCompetitor'),
    enableCreateItem: true,
    hiddenName: 'ProductCompetitorID',
    submitValue: true,
    formItems: [
      {
        xtype: 'textfield',
        fieldLabel: "Competitor Name",
        name: 'Name',
        allowBlank: false,
        inputType: 'text'
      }
      ],
    createUrl: Concentrator.route('CreateProductCompetitor', 'ProductCompetitor'),
    singularObjectName: 'ProductCompetitor'
  });

  Ext.reg('connectorType', this.ConnectorTypes);
  Ext.reg('outboundMessageType', this.OutboundMessageTypes);
  Ext.reg('accountPrivilegesType', this.AccountPrivileges);
  Ext.reg('providerType', this.ProviderTypes);
  Ext.reg('ftpType', this.FtpTypes);
  Ext.reg('xtractType', this.XtractTypes);
  Ext.reg('productgroup', this.ProductGroupSearch);
  Ext.reg('language', this.LanguageSelect);
  Ext.reg('masterGroupMappingOptionSelect', this.MasterGroupMappingOptionSelect);
  Ext.reg('connector', this.ConnectorSearch);
  Ext.reg('connectorWithFilter', this.ConnectorSelectWithFilter);
  Ext.reg('vendor', this.VendorSearch);
  Ext.reg('vendorSelect', this.VendorSelect);
  Ext.reg('relatedProductTypeSelect', this.RelatedProductTypeSelect);
  Ext.reg('contentvendor', this.ContentVendorSearch);
  Ext.reg('attributeMaterialSearch', this.AttributeMaterialSearch);
  Ext.reg('attributePijpwijdteSearch', this.AttributePijpwijdteSearch);
  Ext.reg('attributeDessinSearch', this.AttributeDessinSearch);
  Ext.reg('attributeKraagvormSearch', this.AttributeKraagvormSearch);
  Ext.reg('user', this.Users);
  Ext.reg('brand', this.BrandSearch);
  Ext.reg('product', this.ProductSearch);
  Ext.reg('orderRule', this.OrderRuleSearch);
  Ext.reg('concentratorstatus', this.ConcentratorStatusSearch);
  Ext.reg('selector', this.SelectorSearch);
  Ext.reg('mediatype', this.MediaTypeSearch);
  Ext.reg('relatedProductType', this.RelatedProductTypeSearch);
  Ext.reg('productattributegroup', this.AttributeGroupSearch);
  Ext.reg('attribute', this.AttributeSearch);
  Ext.reg('relatedproduct', this.RelatedProductSearch);
  Ext.reg('signs', this.SignSelect);
  Ext.reg('priceruletypes', this.PriceRuleTypeSelect);
  Ext.reg('statustypes', this.StatusTypeSelect);
  Ext.reg('productBarcodetypes', this.ProductBarcodeTypeSelect);
  Ext.reg('currency', this.CurrencyField);
  Ext.reg('vendorStockType', this.VendorStockTypeSearch);
  Ext.reg('stockStatus', this.StockStatusSearch);
  Ext.reg('priceRounding', this.PriceCalculationSearch);
  Ext.reg('vendorPriceRounding', this.VendorPriceCalculationSearch);
  //    Ext.reg('activeProductCompareSource', this.activeProductCompareSource);
  Ext.reg('productSources', this.productSources);
  Ext.reg('productCompetitor', this.ProductCompetitor);
  Ext.reg('attributeValueGroup', this.ProductAttributeValueGroupSearch);
  Ext.reg('connectorStatus', this.ConnectorStatusSearch);
  Ext.reg('deliveryStatus', this.DeliveryStatuses);

};