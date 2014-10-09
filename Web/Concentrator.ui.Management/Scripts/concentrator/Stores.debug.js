/// <reference path="~/Content/js/ext/ext-base-debug.js" />
/// <reference path="~/Content/js/ext/ext-all-debug.js" />

StoreRepository = function () {

  this.selectors = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetStore", "Selector"),
    root: 'selectors',
    idProperty: 'SelectorID',
    fields: ['SelectorID', 'Name']
  });

  this.connectorType = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetConnectorTypes", "ConnectorRelation"),
    method: 'GET',
    root: 'connectorTypes',
    idProperty: 'ConnectorTypeID',
    fields: ['ConnectorTypeID', 'ConnectorTypeName']
  });


  this.connectorsForLoggedInUser = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetConnectorsForLoggedInUser", "ProductGroupMapping"),
    method: 'GET',
    root: 'connectors',
    idProperty: 'ConnectorID',
    fields: ['ConnectorID', 'ConnectorName']
  });


  this.xtractTypes = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetXtractTypes", "ConnectorRelation"),
    method: 'GET',
    root: 'xtractTypes',
    idProperty: 'XtractTypeID',
    fields: ['XtractTypeID', 'XtractTypeName']
  });

  this.accountPrivilegeStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetAccountPrivileges", "ConnectorRelation"),
    method: 'GET',
    root: 'results',
    idProperty: 'AccountPrivilegeID',
    fields: ['AccountPrivilegeID', 'AccountPrivilegeName']
  });

//  this.deliveryStatusesStore = new Ext.data.JsonStore({
//    autoDestroy: false,
//    url: Concentrator.route("GetDeliveryStatuses", "ProductCompetitor"),
//    method: 'GET',
//    root: 'results',
//    idProperty: 'DeliveryStatusName',
//    fields: ['DeliveryStatusName', 'DeliveryStatusName']
//  });

  this.providerTypeStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetProviderTypes", "ConnectorRelation"),
    method: 'GET',
    root: 'results',
    idProperty: 'ProviderTypeID',
    fields: ['ProviderTypeID', 'ProviderTypeName']
  });

  this.ftpTypeStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetFtpTypes", "ConnectorRelation"),
    method: 'GET',
    root: 'results',
    idProperty: 'FtpTypeID',
    fields: ['FtpTypeID', 'FtpTypeName']
  });

  this.connectors = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetStore", "Connector"),
    method: 'GET',
    root: 'connectors',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.connectorsWithFilter = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetStoreByConnector", "Connector"),
    method: 'GET',
    root: 'results',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.productGroups = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetStore", "ProductGroup"),
    method: 'GET',
    root: 'ProductGroups',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.languages = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetLanguages", "Language"),
    method: 'GET',
    root: 'languages',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.orderRules = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetStore", "OrderRule"),
    method: 'GET',
    root: 'OrderRules',
    idProperty: 'RuleID',
    fields: ['RuleID', 'Name']
  });

  this.vendors = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetAllVendors", "Vendor"),
    method: 'GET',
    root: 'results',
    idProperty: 'VendorID',
    fields: ['VendorID', 'VendorName']
  });
   
  this.attributeStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetList", "ProductAttribute"),
    method: 'GET',
    root: 'results',
    idProperty: 'AttributeID',
    fields: ['AttributeID', 'AttributeCode', 'VendorID']
  });

  this.AttributeMaterialStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetMaterialStore", "ProductAttribute"),
    method: 'GET',
    root: 'results',
    idProperty: 'MaterialOptionID',
    fields: ['MaterialOptionID', 'MaterialAttributeOption']
  });

  this.AttributeDessinStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetDessinStore", "ProductAttribute"),
    method: 'GET',
    root: 'results',
    idProperty: 'DessinOptionID',
    fields: ['DessinOptionID', 'DessinAttributeOption']
  });

  this.AttributePijpwijdteStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetPijpwijdteStore", "ProductAttribute"),
    method: 'GET',
    root: 'results',
    idProperty: 'PijpwijdteOptionID',
    fields: ['PijpwijdteOptionID', 'PijpwijdteAttributeOption']
  });

  this.AttributeKraagvormStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetKraagvormStore", "ProductAttribute"),
    method: 'GET',
    root: 'results',
    idProperty: 'KraagvormOptionID',
    fields: ['KraagvormOptionID', 'KraagvormAttributeOption']
  });

  this.relatedProductTypes = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("SearchRelatedProductType", "RelatedProduct"),
    method: 'GET',
    root: 'results',
    idProperty: 'RelatedProductTypeID',
    fields: ['RelatedProductTypeID', 'Type']
  });

  this.roles = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetRoles", "User"),
    method: 'GET',
    root: 'results',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.users = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetUsers", "User"),
    method: 'GET',
    root: 'results',
    idProperty: 'UserID',
    fields: ['UserID', 'Name']
  });

  this.signs = new Ext.data.SimpleStore({
    id: 0,
    mode: 'remote',
    fields: [
            'Id',
            'display'
        ],
    data: [['%', '%'], ['+', '+']],
    proxy: new Ext.data.MemoryProxy([['%', '%'], ['+', '+']])
  });

  this.priceRuleTypes = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetPriceRuleTypes", "ContentPrice"),
    method: 'GET',
    root: 'results',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.statusTypes = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetStatusTypes", "ProductMatch"),
    method: 'GET',
    root: 'results',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.productBarcodeTypes = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetBarcodeTypes", "ProductBarcode"),
    method: 'GET',
    root: 'results',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.orderLineStates = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetOrderLineStates", "OrderLine"),
    method: 'GET',
    root: 'results',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.mangementLabels = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetCustomLabels", "ManagementPage"),
    method: 'GET',
    root: 'results',
    idProperty: 'ID',
    fields: ['ID', 'Name']
  });

  this.pageLayouts = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetList", "MagentoPageLayout"),
    method: 'GET',
    root: 'results',
    idProperty: 'LayoutID',
    fields: ['LayoutID', 'LayoutName']
  });

  this.eventTypes = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetStore", "EventType"),
    method: 'GET',
    root: 'eventTypes',
    idProperty: 'TypeID',
    fields: ['TypeID', 'Type']
  });

  this.productCompareSource = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route('GetProductCompetitorStore', 'ProductCompetitor'),
    method: 'GET',
    root: 'productCompetitorSources',
    idProperty: 'ProductCompetitorID',
    fields: ['ProductCompetitorID', 'Name']
    });

  this.allVendors = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route("GetAllVendors", "Vendor"),
    method: 'GET',
    root: 'results',
    idProperty: 'VendorID',
    fields: ['VendorID', 'VendorName']
  });

  this.UserSaveStatesStore = new Ext.data.JsonStore({
    autoDestroy: false,
    url: Concentrator.route('GetSaveStates', 'UserComponentState'),
    method: 'GET',
    root: 'results',
    idProperty: 'StateID',
    fields: ['StateID', 'EntityName', "SavedState"]
  });

  //this.notificationStore = new Ext.data.JsonStore({
  //  autoDestroy: false,
  //  url: Concentrator.route('GetAllNotifications', 'Comments'),
  //  method: 'GET',
  //  root: 'results',
  //  idProperty: 'NotificationTypeID',
  //  fields: ['NotificationTypeID', 'NotificationType']
  //});

  for (var store in this)
  {
    if (this[store].mode !== 'remote')
    {
      this[store].load();
    }
  }
};