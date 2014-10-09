using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Model.Users;

namespace Concentrator.Objects.Models.Users
{
	public class FunctionalityRole : BaseModel<FunctionalityRole>
	{
		public Int32 RoleID { get; set; }

		public String FunctionalityName { get; set; }

		public virtual Role Role { get; set; }

		public override System.Linq.Expressions.Expression<Func<FunctionalityRole, bool>> GetFilter()
		{
			return null;
		}
	}

	public enum Functionalities
	{
		#region AdditionalOrderProduct
		[FunctionalityInfo(FunctionalityGroups.Orders, "Create AdditionalOrderProduct")]
		CreateAdditionalOrderProduct,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Delete AdditionalOrderProduct")]
		DeleteAdditionalOrderProduct,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Update AdditionalOrderProduct")]
		UpdateAdditionalOrderProduct,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Get AdditionalOrderProduct")]
		GetAdditionalOrderProduct,
		#endregion

		#region AttributeMatch
		[FunctionalityInfo(FunctionalityGroups.AttributeMatch, "Create AttributeMatch")]
		CreateAttributeMatch,

		[FunctionalityInfo(FunctionalityGroups.AttributeMatch, "Delete AttributeMatch")]
		DeleteAttributeMatch,

		[FunctionalityInfo(FunctionalityGroups.AttributeMatch, "Update AttributeMatch")]
		UpdateAttributeMatch,

		[FunctionalityInfo(FunctionalityGroups.AttributeMatch, "Get AttributeMatch")]
		GetAttributeMatch,
		#endregion

		#region AttributeMatchStore
		[FunctionalityInfo(FunctionalityGroups.AttributeMatchStore, "Update AttributeMatchStore")]
		UpdateAttributeMatchStore,
    #endregion

    #region Brand
    [FunctionalityInfo(FunctionalityGroups.Products, "Create Brand")]
		CreateBrand,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Brand")]
		UpdateBrand,

		[FunctionalityInfo(FunctionalityGroups.Products, "Get Brands")]
		GetBrand,
		#endregion

		#region BrandMedia
		[FunctionalityInfo(FunctionalityGroups.BrandMedia, "Create BrandMedia")]
		CreateBrandMedia,

		[FunctionalityInfo(FunctionalityGroups.BrandMedia, "Delete BrandMedia")]
		DeleteBrandMedia,

		[FunctionalityInfo(FunctionalityGroups.BrandMedia, "Update BrandMedia")]
		UpdateBrandMedia,

		[FunctionalityInfo(FunctionalityGroups.BrandMedia, "Get BrandMedia")]
		GetBrandMedia,
		#endregion

		#region BrandVendor
		[FunctionalityInfo(FunctionalityGroups.BrandVendor, "Get Brand Vendor")]
		GetBrandVendor,

		[FunctionalityInfo(FunctionalityGroups.BrandVendor, "Create Brand Vendor")]
		CreateBrandVendor,

		[FunctionalityInfo(FunctionalityGroups.BrandVendor, "Delete Brand Vendor")]
		DeleteBrandVendor,

		[FunctionalityInfo(FunctionalityGroups.BrandVendor, "Update Brand Vendor")]
		UpdateBrandVendor,
		#endregion

		#region CalculatedPrice
		[FunctionalityInfo(FunctionalityGroups.CalculatedPrice, "Get Calculated Price")]
		GetCalculatedPrice,

		[FunctionalityInfo(FunctionalityGroups.CalculatedPrice, "Create Calculated Price")]
		CreateCalculatedPrice,

		[FunctionalityInfo(FunctionalityGroups.CalculatedPrice, "Update Calculated Price")]
		UpdateCalculatedPrice,
		#endregion

		#region Comments
		[FunctionalityInfo(FunctionalityGroups.Management, "Get Comments")]
		GetComments,

		[FunctionalityInfo(FunctionalityGroups.Management, "Delete Comments")]
		DeleteComments,

		[FunctionalityInfo(FunctionalityGroups.Management, "Update Comments")]
		UpdateComments,
		#endregion

		#region Competitor
		[FunctionalityInfo(FunctionalityGroups.Competitor, "Get Competitor")]
		GetCompetitor,

		[FunctionalityInfo(FunctionalityGroups.Competitor, "Delete Competitor")]
		DeleteCompetitor,

		[FunctionalityInfo(FunctionalityGroups.Competitor, "Update Competitor")]
		UpdateCompetitor,
		#endregion

		#region Connector
		[FunctionalityInfo(FunctionalityGroups.Connectors, "Get Connector")]
		GetConnector,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Create Connector")]
		CreateConnector,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Update Connector")]
		UpdateConnector,
		#endregion

		#region ConnectorLanguage
		[FunctionalityInfo(FunctionalityGroups.Connectors, "Get Connector Language")]
		GetConnectorLanguage,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Create Connector Language")]
		CreateConnectorLanguage,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Update Connector Language")]
		UpdateConnectorLanguage,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Delete Connector Language")]
		DeleteConnectorLanguage,
		#endregion

		#region ConnectorProductStatus
		[FunctionalityInfo(FunctionalityGroups.Mapping, "Get Connector Product Status")]
		GetConnectorProductStatus,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Update Connector Product Status")]
		UpdateConnectorProductStatus,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Create Connector Product Status")]
		CreateConnectorProductStatus,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Delete Connector Product Status")]
		DeleteConnectorProductStatus,
		#endregion

		#region ConnectorPublication
		[FunctionalityInfo(FunctionalityGroups.Rules, "Get Connector Publication")]
		GetConnectorPublication,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Update Connector Publication")]
		UpdateConnectorPublication,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Create Connector Publication")]
		CreateConnectorPublication,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Delete Connector Publication")]
		DeleteConnectorPublication,
		#endregion

		#region ConnectorRelation
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Connector Relation")]
		GetConnectorRelation,

		[FunctionalityInfo(FunctionalityGroups.Edi, "Update Connector Relation")]
		UpdateConnectorRelation,

		[FunctionalityInfo(FunctionalityGroups.Edi, "Create Connector Relation")]
		CreateConnectorRelation,

		[FunctionalityInfo(FunctionalityGroups.Edi, "Delete Connector Relation")]
		DeleteConnectorRelation,
		#endregion

		#region ConnectorSchedule
		[FunctionalityInfo(FunctionalityGroups.Connectors, "Get Connector Schedule")]
		GetConnectorSchedule,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Update Connector Schedule")]
		UpdateConnectorSchedule,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Create Connector Schedule")]
		CreateConnectorSchedule,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Delete Connector Schedule")]
		DeleteConnectorSchedule,
		#endregion

		#region Content
		[FunctionalityInfo(FunctionalityGroups.Content, "Get Content")]
		GetContent,

		[FunctionalityInfo(FunctionalityGroups.Content, "Update Content")]
		UpdateContent,
		#endregion

		#region ContentPriceCalculation
		[FunctionalityInfo(FunctionalityGroups.ContentPriceCalculation, "Get Content Price Calculation")]
		GetContentPriceCalculation,

		[FunctionalityInfo(FunctionalityGroups.ContentPriceCalculation, "Create Content Price Calculation")]
		CreateContentPriceCalculation,
		#endregion

		#region ContentPrice
		[FunctionalityInfo(FunctionalityGroups.Rules, "Get Content Price")]
		GetContentPrice,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Create Content Price")]
		CreateContentPrice,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Update Content Price")]
		UpdateContentPrice,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Delete Content Price")]
		DeleteContentPrice,
		#endregion

		#region ContentProduct
		[FunctionalityInfo(FunctionalityGroups.Rules, "Get Content Product")]
		GetContentProduct,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Create Content Product")]
		CreateContentProduct,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Update Content Product")]
		UpdateContentProduct,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Delete Content Product")]
		DeleteContentProduct,
		#endregion

		#region ContentProductGroupMapping
		[FunctionalityInfo(FunctionalityGroups.ContentProductGroupMapping, "Get Content Product Group Mapping")]
		GetContentProductGroupMapping,

		[FunctionalityInfo(FunctionalityGroups.ContentProductGroupMapping, "Update Content Product Group Mapping")]
		UpdateContentProductGroupMapping,
		#endregion



    #region ContentProductGroupMapping
    [FunctionalityInfo(FunctionalityGroups.ContentProductGroupMapping, "Get Content Stock")]
    GetContentStock,

    [FunctionalityInfo(FunctionalityGroups.ContentProductGroupMapping, "Update Content Stock")]
    UpdateContentStock,

    [FunctionalityInfo(FunctionalityGroups.ContentProductGroupMapping, "Create Content Stock")]
    CreateContentStock,


    [FunctionalityInfo(FunctionalityGroups.ContentProductGroupMapping, "Delete Content Stock")]
    DeleteContentStock,
    #endregion



		#region ContentVendor
		[FunctionalityInfo(FunctionalityGroups.ContentVendor, "Get Content Vendor")]
		GetContentVendor,
		#endregion

		#region ContentVendorSetting
		[FunctionalityInfo(FunctionalityGroups.Rules, "Get Content Vendor Setting")]
		GetContentVendorSetting,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Update Content Vendor Setting")]
		UpdateContentVendorSetting,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Create Content Vendor Setting")]
		CreateContentVendorSetting,

		[FunctionalityInfo(FunctionalityGroups.Rules, "Delete Content Vendor Setting")]
		DeleteContentVendorSetting,
		#endregion

		#region CrossLedger
		[FunctionalityInfo(FunctionalityGroups.Connectors, "Cross Ledger")]
		CrossLedger,
		#endregion

		#region Customer
		[FunctionalityInfo(FunctionalityGroups.Orders, "Get Customer")]
		GetCustomer,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Update Customer")]
		UpdateCustomer,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Create Customer")]
		CreateCustomer,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Delete Customer")]
		DeleteCustomer,
		#endregion

		#region Default
		[FunctionalityInfo(FunctionalityGroups.Default, "Default")]
		Default,
		#endregion

		#region Grid
		[FunctionalityInfo(FunctionalityGroups.Grid, "Show Unmapped products")]
		ShowUnmappedProducts,
		#endregion

		#region EdiFieldMapping
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Field Mapping")]
		GetEdiFieldMapping,

		[FunctionalityInfo(FunctionalityGroups.Edi, "Create Edi Field Mapping")]
		CreateEdiFieldMapping,
		#endregion

		#region EdiOrder
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Order")]
		GetEdiOrder,
		#endregion

		#region EdiOrderLedger
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Order Ledger")]
		GetEdiOrderLedger,
		#endregion

		#region EdiOrderLine
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Order Line")]
		GetEdiOrderLine,
		#endregion

		#region EdiOrderListener
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Order Listener")]
		GetEdiOrderListener,
		#endregion

		#region EdiOrderPost
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Order Post")]
		GetEdiOrderPost,
		#endregion

		#region EdiOrderResponse
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Order Response")]
		GetEdiOrderResponse,
		#endregion

		#region EdiOrderResponseLine
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Order Response Line")]
		GetEdiOrderResponseLine,
		#endregion

		#region EdiValidate
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Validate")]
		GetEdiValidate,

		[FunctionalityInfo(FunctionalityGroups.Edi, "Update Edi Validate")]
		UpdateEdiValidate,
		#endregion

		#region EdiVendor
		[FunctionalityInfo(FunctionalityGroups.Edi, "Get Edi Vendor")]
		GetEdiVendor,

		[FunctionalityInfo(FunctionalityGroups.Edi, "Update Edi Vendor")]
		CreateEdiVendor,
		#endregion

		#region Event
		[FunctionalityInfo(FunctionalityGroups.Management, "Get Event")]
		GetEvent,
		#endregion

		#region EventType
		[FunctionalityInfo(FunctionalityGroups.Management, "Get Event Type")]
		GetEventType,
		#endregion

    #region ExcludeProduct
    [FunctionalityInfo(FunctionalityGroups.ExcludeProduct, "Get Exclude Products")]
    GetExcludeProducts,

    [FunctionalityInfo(FunctionalityGroups.ExcludeProduct, "Create Exclude Products")]
    CreateExcludeProducts,

    [FunctionalityInfo(FunctionalityGroups.ExcludeProduct, "Delete Exclude Products")]
    DeleteExcludeProducts,
    #endregion

		#region Faq
		[FunctionalityInfo(FunctionalityGroups.Products, "Get Faq")]
		GetFaq,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Faq")]
		UpdateFaq,

		[FunctionalityInfo(FunctionalityGroups.Products, "Create Faq")]
		CreateFaq,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Faq")]
		DeleteFaq,
		#endregion

		#region Files
		[FunctionalityInfo(FunctionalityGroups.Management, "Get Files")]
		GetFiles,
		#endregion

    #region Datcol
    [FunctionalityInfo(FunctionalityGroups.Datcol, "Datcol")]
    Datcol,
    #endregion

		#region Image
		[FunctionalityInfo(FunctionalityGroups.Image, "Get Image")]
		GetImage,

		[FunctionalityInfo(FunctionalityGroups.Image, "Update Image")]
		UpdateImage,
		#endregion

		#region Language
		[FunctionalityInfo(FunctionalityGroups.Management, "Get Language")]
		GetLanguage,

		[FunctionalityInfo(FunctionalityGroups.Management, "Create Language")]
		CreateLanguage,

		[FunctionalityInfo(FunctionalityGroups.Management, "Update Language")]
		UpdateLanguage,

		[FunctionalityInfo(FunctionalityGroups.Management, "Delete Language")]
		DeleteLanguage,
		#endregion

		#region ManagementPage
		[FunctionalityInfo(FunctionalityGroups.Management, "Get Management Page")]
		GetManagementPage,

		[FunctionalityInfo(FunctionalityGroups.Management, "Create Management Page")]
		CreateManagementPage,
		#endregion

    #region MasterGroupMapping
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Get Master Group Mapping")]
    GetMasterGroupMapping,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Create Master Group Mapping")]
    CreateMasterGroupMapping,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Delete Master Group Mapping")]
    DeleteMasterGroupMapping,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Copy Master Group Mapping")]
    CopyMasterGroupMapping,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Update Master Group Mapping")]
    UpdateMasterGroupMapping,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Assign User To Master Group Mapping")]
    AssignUserToMasterGroupMapping,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Set Master Group Mapping Translation")]
    SetMasterGroupMappingDescription,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Manage Master Group Mapping SEO Texts")]
    ManageSeoTexts,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Get Master Group Mapping Translation")]
    GetMasterGroupMappingDescription,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Default Master Group Mapping Rol")]
    DefaultMasterGroupMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access language wizard")]
    MasterGroupMappingLanguageWizard,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access attribute managemt")]
    MasterGroupMappingAttributeManagement,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access cross reference management")]
    MasterGroupMappingCrossReferenceManagement,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access related reference management")]
    MasterGroupMappingRelatedManagement,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access mapping product control")]
    MasterGroupMappingProductControl,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access control all products wizard")]
    MasterGroupMappingControlAllProductsWizard,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access product controle management")]
    MasterGroupMappingProductControleManagement,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access attribute selector management")]
    MasterGroupMappingAttributeSelectorManagement,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access vendor product groups management")]
    MasterGroupMappingVendorProductGroupsManagement,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access vendor products management")]
    MasterGroupMappingVendorProducsManagement,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Access connector mapping management")]
    ConnectorMappingManagement,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View vendor products")]
    MasterGroupMappingViewVendorProducts,

    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View vendor product groups")]
    MasterGroupMappingViewVendorProductGroups,


    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Add product group mapping")]
    MasterGroupMappingAddProductGroupMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Delete product group mapping")]
    MasterGroupMappingDeleteProductGroupMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Product group settings")]
    MasterGroupMappingProductGroupSettings,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Rename connectormapping")]
    MasterGroupMappingRenameConnectorMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Find source")]
    MasterGroupMappingFindSource,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View products")]
    MasterGroupMappingViewProducts,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View group attribute mapping")]
    MasterGroupMappingViewGroupAttributeMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View pricerule")]
    MasterGroupMappingViewPriceRule,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View pricetagmapping")]
    MasterGroupMappingViewPriceTagMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View connectormapping")]
    MasterGroupMappingViewConnectorMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Move productgroupmapping")]
    MasterGroupMappingMoveProductGroupMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "Copy productgroupmapping")]
    MasterGroupMappingCopyProductGroupMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "MasterGroupMapping Choose Source")]
    MasterGroupMappingChooseSource,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "MasterGroupMapping Add ConnectorMapping")]
    MasterGroupMappingAddConnectorMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "MasterGroupMapping Delete ConnectorMapping")]
    MasterGroupMappingDeleteConnectorMapping,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View Connector PublicationRule")]
    MasterGroupMappingViewConnectorPublicationRule,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "View unassigned mastergroup mappings")]
    MasterGroupMappingViewUnassigned,
    [FunctionalityInfo(FunctionalityGroups.MasterGroupMapping, "MasterGroupMapping Administrator")]
    MasterGroupMappingAdministrator,
    #endregion

		#region MediaType
		[FunctionalityInfo(FunctionalityGroups.Products, "Create Media Type")]
		GetMediaType,

		[FunctionalityInfo(FunctionalityGroups.Products, "Create Media Type")]
		CreateMediaType,
		#endregion

    #region MenuItems
    [FunctionalityInfo("View menu item - Master group mapping", FunctionalityGroups.MenuItem)]
    View_MasterGroupMappings,
    #endregion

    #region Middleware
    [FunctionalityInfo(FunctionalityGroups.MiddleWare, "Start Job")]
		StartJob,

		[FunctionalityInfo(FunctionalityGroups.MiddleWare, "View Jobs")]
		ViewJobs,

		[FunctionalityInfo(FunctionalityGroups.MiddleWare, "Stop Job")]
		StopJob,
		#endregion

		#region MissingContent
		[FunctionalityInfo(FunctionalityGroups.MissingContent, "Get Missing Content")]
		GetMissingContent,
		#endregion

		#region Notifications
		[FunctionalityInfo(FunctionalityGroups.Notifications, "Get Notifications")]
		GetNotifications,
		#endregion

		#region Order
		[FunctionalityInfo(FunctionalityGroups.Orders, "View Orders or Related order grids")]
		ViewOrders,
		#endregion

		#region OrderLedger
		[FunctionalityInfo(FunctionalityGroups.Orders, "Get Order Ledger")]
		GetOrderLedger,
		#endregion

		#region OrderLine
		[FunctionalityInfo(FunctionalityGroups.Orders, "View Customers")]
		ViewCustomers,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Get Order Line")]
		GetOrderLine,
		#endregion

		#region OrderOutbound
		[FunctionalityInfo(FunctionalityGroups.Orders, "Get OrderOutbound")]
		GetOrderOutbound,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Update OrderOutbound")]
		UpdateOrderOutbound,
		#endregion

		#region OrderResponse
		[FunctionalityInfo(FunctionalityGroups.Orders, "Get Order Response")]
		GetOrderResponse,
		#endregion

		#region OrderRule
		[FunctionalityInfo(FunctionalityGroups.Orders, "Get Order Rule")]
		GetOrderRule,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Create Order Rule")]
		CreateOrderRule,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Delete Order Rule")]
		DeleteOrderRule,

		[FunctionalityInfo(FunctionalityGroups.Orders, "Update Order Rule")]
		UpdateOrderRule,
		#endregion

		#region Plugin
		[FunctionalityInfo(FunctionalityGroups.Management, "Get Plugin")]
		GetPlugin,

		[FunctionalityInfo(FunctionalityGroups.Management, "Create Plugin")]
		CreatePlugin,

		[FunctionalityInfo(FunctionalityGroups.Management, "Delete Plugin")]
		DeletePlugin,

		[FunctionalityInfo(FunctionalityGroups.Management, "Update Plugin")]
		UpdatePlugin,
		#endregion

		#region Portal
		[FunctionalityInfo(FunctionalityGroups.Portal, "Get Portal")]
		GetPortal,

		[FunctionalityInfo(FunctionalityGroups.Portal, "Update Portal")]
		UpdatePortal,
		#endregion

		#region Portlet

		[FunctionalityInfo(FunctionalityGroups.Portlet, "Create Portlet Role")]
		CreatePortletRole,

		[FunctionalityInfo(FunctionalityGroups.Portlet, "Update Portlet Role")]
		UpdatePortletRole,


		#endregion

		#region ConfiguredProducts

		[FunctionalityInfo(FunctionalityGroups.ConfiguredProducts, "Update all configured products with same color")]
		UpdateAllMatchedProducts,

		[FunctionalityInfo(FunctionalityGroups.ConfiguredProducts, "Get all configured products with same color")]
		GetMatchedProducts,


		#endregion

		#region PreferredConnectorVendor
		[FunctionalityInfo(FunctionalityGroups.Connectors, "View Preferred Connector Vendor")]
		GetPreferredConnectorVendor,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Delete Preferred Connector Vendor")]
		DeletePreferredConnectorVendor,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Create Preferred Connector Vendor")]
		CreatePreferredConnectorVendor,

		[FunctionalityInfo(FunctionalityGroups.Connectors, "Update Preferred Connector Vendor")]
		UpdatePreferredConnectorVendor,
		#endregion

		#region ProductAttribute
		[FunctionalityInfo(FunctionalityGroups.Products, "Get Product Attribute")]
		GetProductAttribute,

		[FunctionalityInfo(FunctionalityGroups.Products, "Create Product Attribute")]
		CreateProductAttribute,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Product Attribute")]
		UpdateProductAttribute,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Product Attribute")]
		DeleteProductAttribute,
		#endregion

		#region ProductAttributeGroup
		[FunctionalityInfo(FunctionalityGroups.Products, "Create Product Attribute Group")]
		CreateProductAttributeGroup,

		[FunctionalityInfo(FunctionalityGroups.Products, "Get Product Attribute Group")]
		GetProductAttributeGroup,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Product Attribute Group")]
		UpdateProductAttributeGroup,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Product Attribute Group")]
		DeleteProductAttributeGroup,
		#endregion

		#region ProductAttributeValue
		[FunctionalityInfo(FunctionalityGroups.Products, "Get Product Attribute Value")]
		GetProductAttributeValue,

		[FunctionalityInfo(FunctionalityGroups.Products, "Create Product Attribute Value")]
		CreateProductAttributeValue,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Product Attribute Value")]
		DeleteProductAttributeValue,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Product Attribute Value")]
		UpdateProductAttributeValue,
		#endregion

		#region ProductBarCode
		[FunctionalityInfo(FunctionalityGroups.Products, "Get Product Barcode")]
		GetProductBarcode,

		[FunctionalityInfo(FunctionalityGroups.Products, "Create Product Barcode")]
		CreateProductBarcode,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Product Barcode")]
		UpdateProductBarcode,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Product Barcode")]
		DeleteProductBarcode,
		#endregion

		#region ProductCompetitor
		[FunctionalityInfo(FunctionalityGroups.Slurp, "Update Product Competitor")]
		UpdateProductCompetitor,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Get Product Competitor")]
		GetProductCompetitor,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Create Product Competitor")]
		CreateProductCompetitor,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Delete Product Competitor")]
		DeleteProductCompetitor,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Create Product Competitor Ledger")]
		CreateProductCompetitorLedger,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Update Product Competitor Ledger")]
		UpdateProductCompetitorLedger,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Delete Product Competitor Ledger")]
		DeleteProductCompetitorLedger,
		#endregion

		#region Product
		[FunctionalityInfo(FunctionalityGroups.Products, "Get Product")]
		GetProduct,

		[FunctionalityInfo(FunctionalityGroups.Products, "View Product")]
		ViewProducts,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Product")]
		UpdateProduct,

    [FunctionalityInfo(FunctionalityGroups.Products, "Create Product")]
    CreateProduct,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Product")]
		DeleteProduct,
		#endregion




		#region ProductDescription
		[FunctionalityInfo(FunctionalityGroups.Products, "Get ProductDescription")]
		GetProductDescription,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update ProductDescription")]
		UpdateProductDescription,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete ProductDescription")]
		DeleteProductDescription,
		#endregion

		#region ProductGroupAttributeMapping
		[FunctionalityInfo(FunctionalityGroups.Products, "Get Product Group Attribute Mapping")]
		GetProductGroupAttributeMapping,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Product Group Attribute Mapping")]
		UpdateProductGroupAttributeMapping,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Product Group Attribute Mapping")]
		DeleteProductGroupAttributeMapping,

		[FunctionalityInfo(FunctionalityGroups.Products, "Create Product Group Attribute Mapping")]
		CreateProductGroupAttributeMapping,
		#endregion

		#region ProductGroupConnectorVendor
		[FunctionalityInfo(FunctionalityGroups.ProductGroupConnectorVendor, "Get Group Connector Vendor")]
		GetProductGroupConnectorVendor,

		[FunctionalityInfo(FunctionalityGroups.ProductGroupConnectorVendor, "Create Product Group Connector Vendor")]
		CreateProductGroupConnectorVendor,

		[FunctionalityInfo(FunctionalityGroups.ProductGroupConnectorVendor, "Update Product Group Connector Vendor")]
		UpdateProductGroupConnectorVendor,

		[FunctionalityInfo(FunctionalityGroups.ProductGroupConnectorVendor, "Delete Product Group Connector Vendor")]
		DeleteProductGroupConnectorVendor,
		#endregion

		#region ProductGroup
		[FunctionalityInfo(FunctionalityGroups.Products, "Get Product Group")]
		GetProductGroup,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Product Group")]
		UpdateProductGroup,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Product Group")]
		DeleteProductGroup,

		[FunctionalityInfo(FunctionalityGroups.Products, "Create Product Group")]
		CreateProductGroup,
		#endregion


    #region ProductGroupRelatedProductGroup
    [FunctionalityInfo(FunctionalityGroups.ProductGroupRelatedProductGroup, "Get Product Group RelatedProductGroup")]
    GetProductGroupRelatedProductGroup,

    [FunctionalityInfo(FunctionalityGroups.ProductGroupRelatedProductGroup, "Update Product Group RelatedProductGroup")]
    UpdateProductGroupRelatedProductGroup,

    [FunctionalityInfo(FunctionalityGroups.ProductGroupRelatedProductGroup, "Delete Product Group RelatedProductGroup")]
    DeleteProductGroupRelatedProductGroup,

    [FunctionalityInfo(FunctionalityGroups.ProductGroupRelatedProductGroup, "Create Product Group RelatedProductGroup")]
    CreateProductGroupRelatedProductGroup,
    #endregion



		#region ProductGroupMapping
		[FunctionalityInfo(FunctionalityGroups.Mapping, "Get Product Group Mapping")]
		GetProductGroupMapping,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Create Product Group Mapping")]
		CreateProductGroupMapping,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Delete Product Group Mapping")]
		DeleteProductGroupMapping,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Copy Product Group Mapping")]
		CopyProductGroupMapping,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Update Product Group Mapping")]
		UpdateProductGroupMapping,
		#endregion

		#region ProductGroupSelector
		[FunctionalityInfo(FunctionalityGroups.Mapping, "Get Product Group Selector")]
		GetProductGroupSelector,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Update Product Group Selector")]
		UpdateProductGroupSelector,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Delete Product Group Selector")]
		DeleteProductGroupSelector,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Create Product Group Selector")]
		CreateProductGroupSelector,
		#endregion

		#region ProductGroupVendor
		[FunctionalityInfo(FunctionalityGroups.ProductGroupVendor, "Get Product Group Vendor")]
		GetProductGroupVendor,

		[FunctionalityInfo(FunctionalityGroups.ProductGroupVendor, "Update Product Group Vendor")]
		UpdateProductGroupVendor,

		[FunctionalityInfo(FunctionalityGroups.ProductGroupVendor, "Create Product Group Vendor")]
		CreateProductGroupVendor,

		[FunctionalityInfo(FunctionalityGroups.ProductGroupVendor, "Delete Product Group Vendor")]
		DeleteProductGroupVendor,
		#endregion

		#region ProductImage
		[FunctionalityInfo(FunctionalityGroups.ProductImage, "Get ProductImage")]
		GetProductImage,
		#endregion

		#region ProductMatch
		[FunctionalityInfo(FunctionalityGroups.Mapping, "View Product Match")]
		ViewProductMatch,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Update Product Match")]
		UpdateProductMatch,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Create Product Match")]
		CreateProductMatch,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Delete Product Match")]
		DeleteProductMatch,
		#endregion

		#region ProductMedia
		[FunctionalityInfo(FunctionalityGroups.Products, "Create Product Media")]
		GetProductMedia,

		[FunctionalityInfo(FunctionalityGroups.Products, "Create Product Media")]
		CreateProductMedia,

		[FunctionalityInfo(FunctionalityGroups.Products, "Update Product Media")]
		UpdateProductMedia,

		[FunctionalityInfo(FunctionalityGroups.Products, "Delete Product Media")]
		DeleteProductMedia,
		#endregion

		#region ProductStatus
		[FunctionalityInfo(FunctionalityGroups.ProductStatus, "Get Product Status")]
		GetProductStatus,

		[FunctionalityInfo(FunctionalityGroups.ProductStatus, "Create Product Status")]
		CreateProductStatus,

		[FunctionalityInfo(FunctionalityGroups.ProductStatus, "Delete Product Status")]
		DeleteProductStatus,

		[FunctionalityInfo(FunctionalityGroups.ProductStatus, "Update Product Status")]
		UpdateProductStatus,
		#endregion

		#region PushProduct
		[FunctionalityInfo(FunctionalityGroups.Products, "Push products")]
		PushProducts,
		#endregion

		#region RelatedProduct
		[FunctionalityInfo(FunctionalityGroups.RelatedProduct, "Get Related Product")]
		GetRelatedProduct,

		[FunctionalityInfo(FunctionalityGroups.RelatedProduct, "Create Related Product")]
		CreateRelatedProduct,

		[FunctionalityInfo(FunctionalityGroups.RelatedProduct, "Delete Related Product")]
		DeleteRelatedProduct,
		#endregion

		#region Role
		[FunctionalityInfo(FunctionalityGroups.Management, "View Roles")]
		ViewRoles,

		[FunctionalityInfo(FunctionalityGroups.Management, "Create Role")]
		CreateRole,

		[FunctionalityInfo(FunctionalityGroups.Management, "Delete Role")]
		DeleteRole,

		[FunctionalityInfo(FunctionalityGroups.Management, "Update Role")]
		UpdateRole,

		[FunctionalityInfo(FunctionalityGroups.Management, "Update Functionalities")]
		UpdateFunctionalities,

		[FunctionalityInfo(FunctionalityGroups.Management, "Get Functionalities")]
		GetFunctionalities,
		#endregion

		#region ScanData
		[FunctionalityInfo(FunctionalityGroups.ScanData, "Get ScanData")]
		GetScanData,

		[FunctionalityInfo(FunctionalityGroups.ScanData, "Delete ScanData")]
		DeleteScanData,

		[FunctionalityInfo(FunctionalityGroups.ScanData, "Update ScanData")]
		UpdateScanData,
		#endregion

		#region ScanProvider
		[FunctionalityInfo(FunctionalityGroups.ScanProvider, "Get ScanProvider")]
		GetScanProvider,

		[FunctionalityInfo(FunctionalityGroups.ScanProvider, "Delete ScanProvider")]
		DeleteScanProvider,

		[FunctionalityInfo(FunctionalityGroups.ScanProvider, "Update ScanProvider")]
		UpdateScanProvider,

		[FunctionalityInfo(FunctionalityGroups.ScanProvider, "Create ScanProvider")]
		CreateScanProvider,
		#endregion

		#region Selector
		[FunctionalityInfo(FunctionalityGroups.Selector, "Get Selector")]
		GetSelector,

		[FunctionalityInfo(FunctionalityGroups.Selector, "Create Selector")]
		CreateSelector,

		[FunctionalityInfo(FunctionalityGroups.Selector, "Delete Selector")]
		DeleteSelector,
		#endregion

		#region Slurp
		[FunctionalityInfo(FunctionalityGroups.Slurp, "Get Slurp")]
		GetSlurp,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Update Slurp")]
		UpdateSlurp,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Create Slurp")]
		CreateSlurp,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Delete Slurp")]
		DeleteSlurp,
		#endregion

    #region Sapph
    [FunctionalityInfo(FunctionalityGroups.Sapph, "Sapph")]
    Sapph,
    #endregion

    #region SlurpLedger
    [FunctionalityInfo(FunctionalityGroups.Slurp, "Get SlurpLedger")]
		GetSlurpLedger,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Update SlurpLedger")]
		UpdateSlurpLedger,

		[FunctionalityInfo(FunctionalityGroups.Slurp, "Create SlurpLedger")]
		DeleteSlurpLedger,
		#endregion

		#region UserComponentState
		[FunctionalityInfo(FunctionalityGroups.UserComponentState, "Update User Component State")]
		UpdateUserComponentState,
		#endregion

		#region PriceSet
		[FunctionalityInfo(FunctionalityGroups.PriceSet, "View Price Set")]
		ViewPriceSet,

		[FunctionalityInfo(FunctionalityGroups.PriceSet, "Create Price Set")]
		CreatePriceSet,

		[FunctionalityInfo(FunctionalityGroups.PriceSet, "Delete Price Set")]
		DeletePriceSet,

		[FunctionalityInfo(FunctionalityGroups.PriceSet, "Update Price Set")]
		UpdatePriceSet,

		#endregion

		#region User
		[FunctionalityInfo(FunctionalityGroups.Management, "View User")]
		ViewUser,

		[FunctionalityInfo(FunctionalityGroups.Management, "Create User")]
		CreateUser,

		[FunctionalityInfo(FunctionalityGroups.Management, "Delete User")]
		DeleteUser,

		[FunctionalityInfo(FunctionalityGroups.Management, "Update User")]
		UpdateUser,

		[FunctionalityInfo(FunctionalityGroups.Management, "Change Password User")]
		ChangePasswordUser,

		[FunctionalityInfo(FunctionalityGroups.Management, "Set Timeout User")]
		SetTimeoutUser,
		#endregion

		#region UserRole
		[FunctionalityInfo(FunctionalityGroups.UserRole, "Create User Role")]
		CreateUserRole,

		[FunctionalityInfo(FunctionalityGroups.UserRole, "Get User Role")]
		GetUserRole,

		[FunctionalityInfo(FunctionalityGroups.UserRole, "Delete User Role")]
		DeleteUserRole,

		[FunctionalityInfo(FunctionalityGroups.UserRole, "Update User Role")]
		UpdateUserRole,

		[FunctionalityInfo(FunctionalityGroups.UserRole, "Delete User From Role")]
		DeleteUserFromRole,
		#endregion

		#region VendorAccruel
		[FunctionalityInfo(FunctionalityGroups.Vendors, "Get Vendor Accruel")]
		GetVendorAccruel,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Delete Vendor Accruel")]
		DeleteVendorAccruel,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Update Vendor Accruel")]
		UpdateVendorAccruel,
		#endregion

		#region VendorAssortment
		[FunctionalityInfo(FunctionalityGroups.Vendors, "Get Vendor Assortment")]
		GetVendorAssortment,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Create Vendor Assortment")]
		CreateVendorAssortment,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Update Vendor Assortment")]
		UpdateVendorAssortment,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Delete Vendor Assortment")]
		DeleteVendorAssortment,
		#endregion

		#region Vendor
		[FunctionalityInfo(FunctionalityGroups.Vendors, "Get Vendor")]
		GetVendor,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Update Vendor")]
		UpdateVendor,
		#endregion

		#region VendorFreeGood
		[FunctionalityInfo(FunctionalityGroups.Vendors, "Get VendorFreeGood")]
		GetVendorFreeGood,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Update VendorFreeGood")]
		UpdateVendorFreeGood,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Create VendorFreeGood")]
		CreateVendorFreeGood,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Delete VendorFreeGood")]
		DeleteVendorFreeGood,
		#endregion

		#region VendorPriceCalculation
		[FunctionalityInfo(FunctionalityGroups.VendorPriceCalculation, "Get Vendor Price Calculation")]
		GetVendorPriceCalculation,

		[FunctionalityInfo(FunctionalityGroups.VendorPriceCalculation, "Create Vendor Price Calculation")]
		CreateVendorPriceCalculation,
		#endregion

		#region VendorPrice
		[FunctionalityInfo(FunctionalityGroups.VendorPrice, "Get Vendor Price")]
		GetVendorPrice,

		[FunctionalityInfo(FunctionalityGroups.VendorPrice, "Create Vendor Price")]
		CreateVendorPrice,

		[FunctionalityInfo(FunctionalityGroups.VendorPrice, "Update Vendor Price")]
		UpdateVendorPrice,

		[FunctionalityInfo(FunctionalityGroups.VendorPrice, "Delete Vendor Price")]
		DeleteVendorPrice,
		#endregion

		#region VendorPriceRule
		[FunctionalityInfo(FunctionalityGroups.Vendors, "Get Vendor Price Rule")]
		GetVendorPriceRule,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Create Vendor Price Rule")]
		CreateVendorPriceRule,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Update Vendor Price Rule")]
		UpdateVendorPriceRule,

		[FunctionalityInfo(FunctionalityGroups.Vendors, "Delete Vendor Price Rule")]
		DeleteVendorPriceRule,
		#endregion

		#region VendorProductStatus
		[FunctionalityInfo(FunctionalityGroups.Mapping, "Get Vendor Product Status")]
		GetVendorProductStatus,

		[FunctionalityInfo(FunctionalityGroups.Mapping, "Update Vendor Product Status")]
		UpdateVendorProductStatus,
		#endregion

		#region VendorStock
		[FunctionalityInfo(FunctionalityGroups.Vendors, "Get Vendor Stock")]
		GetVendorStock,
		#endregion

		#region WebToPrint
		[FunctionalityInfo(FunctionalityGroups.WebToPrint, "Get Web To Print")]
		GetWebToPrint,

		[FunctionalityInfo(FunctionalityGroups.WebToPrint, "Update Web To Print")]
		UpdateWebToPrint,

		[FunctionalityInfo(FunctionalityGroups.WebToPrint, "Create Web To Print")]
		CreateWebToPrint,

		[FunctionalityInfo(FunctionalityGroups.WebToPrint, "Delete Web To Print")]
		DeleteWebToPrint,

		[FunctionalityInfo(FunctionalityGroups.Management, "Manage connector vendor content filters")]
		ManageConnectorVendorContentFilter,
		#endregion

		#region Thumbnails
		[FunctionalityInfo(FunctionalityGroups.Thumbnail, "Get Thumbnails")]
		GetThumbnail,

		[FunctionalityInfo(FunctionalityGroups.Thumbnail, "Update Thumbnails")]
		UpdateThumbnail,
		#endregion

		#region CUSTOM
		[FunctionalityInfo(FunctionalityGroups.Custom, "Get Season code rules")]
		GetSeasonRules,

		[FunctionalityInfo(FunctionalityGroups.Custom, "Create Season code rule")]
		CreateSeasonRules,

		[FunctionalityInfo(FunctionalityGroups.Custom, "Delete Season code rule")]
		DeleteSeasonRules,

		[FunctionalityInfo(FunctionalityGroups.Custom, "Update Season code rule")]
		UpdateSeasonRules,
		#endregion

		#region ValueGroups
		[FunctionalityInfo(FunctionalityGroups.ProductAttribute, "Get value grouping")]
		GetValueGrouping,

		[FunctionalityInfo(FunctionalityGroups.ProductAttribute, "Add value grouping")]
		CreateValueGrouping,

		[FunctionalityInfo(FunctionalityGroups.ProductAttribute, "Delete value grouping")]
		DeleteValueGrouping,

		[FunctionalityInfo(FunctionalityGroups.ProductAttribute, "Update value grouping")]
		UpdateValueGrouping
		#endregion

	}

	public class FunctionalityGroups
	{
		public const string AttributeMatch = "Attribute Match";
		public const string AttributeMatchStore = "Attribute Match Store";

		public const string BrandMedia = "Brand Media";
		public const string BrandVendor = "Brand Vendor";
		public const string CalculatedPrice = "Calculated Price";

		public const string Competitor = "Competitor";
		public const string Connectors = "Connectors";

		public const string Content = "Content";
		public const string ContentPriceCalculation = "Content Price Calculation";
		public const string ContentProductGroupMapping = "Content Product Group Mapping";
		public const string ContentVendor = "Content Vendor";
		public const string Default = "Default";
		public const string Grid = "Grid";
		public const string Edi = "Edi";
    public const string Datcol = "Datcol";
    public const string ExcludeProduct = "Exclude Product";

		public const string Image = "Image";
		public const string Management = "Management";
		public const string Mapping = "Mapping";
    public const string MasterGroupMapping = "Master Group Mapping";
		public const string MediaType = "Media Type";
		public const string MiddleWare = "Middle Ware";
		public const string MissingContent = "Content";
		public const string Notifications = "Notifications";
    public const string MenuItem = "Menu items";
		public const string Orders = "Orders";

		public const string Portal = "Portal";
		public const string Portlet = "Portlet";

		public const string ProductAttribute = "Product Attribute";
		public const string ProductAttributeGroup = "Product Attribute Group";
		public const string ProductAttributeValue = "Product Attribute Value";
		public const string ProductBarcode = "Product Barcodes";
		public const string ProductCompetitor = "Product Competitor";

		public const string Products = "Products";

		public const string ProductDescription = "Product Description";
		public const string ProductGroupAttributeMapping = "Product Group Attribute Mapping";
		public const string ProductGroupConnectorVendor = "Product Group Connector Vendor";

	  public const string ProductGroupRelatedProductGroup = "ProductGroup Related ProductGroup";

		public const string ProductGroupVendor = "Product Group Vendor";
		public const string ProductImage = "Product Image";

		public const string ProductStatus = "Product Status";

		public const string PriceSet = "Price Set";

		public const string RelatedProduct = "Related Product";
		public const string Rules = "Rules";
		public const string ScanData = "Scan Data";
		public const string ScanProvider = "Scan Provider";
		public const string Selector = "Selector";
		public const string Slurp = "Slurp";
	  public const string Sapph = "Sapph";
		public const string UserComponentState = "User Component State";
		public const string UserRole = "UserRole";
		public const string Vendors = "Vendors";
		public const string VendorPriceCalculation = "Vendor Price Calculation";
		public const string VendorPrice = "Vendor Price";
		public const string WebToPrint = "Web To Print";
		public const string Thumbnail = "Thumbnail";
		public const string ConfiguredProducts = "Configured Products";
		public const string Custom = "Custom";
	}

	public class FunctionalityInfoAttribute : Attribute
	{
		public string Group { get; set; }
		public string DisplayName { get; set; }

		public FunctionalityInfoAttribute(string group, string displayName, params string[] defaultRoles)
		{
			Group = group;
			DisplayName = displayName;
		}
	}
}