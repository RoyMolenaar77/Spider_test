using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

using Concentrator.Objects.Logic;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Complex;
using Concentrator.Objects.Models.Localization;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Services.DTO;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IProductService
  {
    /// <summary>
    /// Get catalog view for ipad
    /// </summary>
    /// <param name="vendorID">the vendorID for which it will query</param>
    /// <returns></returns>
    IQueryable<ProductResult> GetForIpad(int vendorID);

    /// <summary>
    /// Creates a product attribute value group
    /// </summary>
    /// <param name="group">Group entity</param>
    /// <param name="names">Dictionary (languageName, translation) of names</param>
    void CreateProductAttributeValueGroup(ProductAttributeValueGroup group, Dictionary<string, string> names);



    /// <summary>
    /// Creates a product attribute
    /// </summary>
    /// <param name="metadata">The metadata of this attribute</param>
    /// <param name="names">Dictionary (languageName, translation) of names</param>
    /// <param name="imageStream">Stream of the image associated with this attribute</param>
    /// <param name="imagePath">The path to which to save the image(if provided)</param>
    void CreateProductAttribute(ProductAttributeMetaData metadata, Dictionary<string, string> names, Stream imageStream = null, string imagePath = null);


    /// <summary>
    /// Creates a product attribute group
    /// </summary>
    /// <param name="metadata">The metadata for this attribute group</param>
    /// <param name="names">Dictionary (languageName, translation) of names</param>
    void CreateProductAttributeGroup(ProductAttributeGroupMetaData metadata, Dictionary<string, string> names);

    /// <summary>
    /// Searches for products
    /// </summary>
    /// <param name="query"></param>
    /// <param name="includeDescriptions"></param>
    /// <param name="includeBrands"></param>
    /// <param name="includeIds"></param>
    /// <param name="includeProductGroups"></param>
    /// <param name="languageID"></param>
    /// <returns></returns>
    IQueryable<ProductSearchResult> SearchProducts(int languageID, string query, bool? includeDescriptions = null, bool? includeBrands = null, bool? includeIds = null, bool? includeProductGroups = null);

    /// <summary>
    /// Searches connector specific products
    /// </summary>
    /// <param name="query"></param>
    /// <param name="includeDescriptions"></param>
    /// <param name="includeBrands"></param>
    /// <param name="includeIds"></param>
    /// <param name="includeProductGroups"></param>
    /// <param name="languageID"></param>
    /// <returns></returns>
    List<ProductSearchResult> SearchProducts(int languageID, int connectorID, string query, bool? includeDescriptions = null, bool? includeBrands = null, bool? includeIds = null, bool? includeProductGroups = null);


    /// Retrieves vendors of product content
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="includeConcentratorVendor">
    /// Includes the concentrator vendors(Base & Overlay) 
    /// if applicable when set to true
    /// </param>
    /// <returns></returns>
    IQueryable<Vendor> GetContentVendors(int productID, bool? includeConcentratorVendor = true);

    /// <summary>
    /// Pushes a product
    /// </summary>        
    void PushProducts();

    /// <summary>
    ///  Creates product media
    /// </summary>
    /// <param name="file"></param>
    /// <param name="filename"></param>
    /// <param name="mediaurl"></param>
    /// <param name="productID"></param>
    /// <param name="typeID"></param>
    /// <param name="vendorID"></param>
    /// <param name="description"></param>
    void CreateProductMedia(Stream file, string filename, string mediaurl, int productID, int typeID, int vendorID, string description, List<int> updateForMultipleIDs = null);

    /// <summary>
    /// Creates product media if it contains a url    
    /// </summary>
    /// <param name="mediaurl"></param>
    /// <param name="productID"></param>
    /// <param name="typeID"></param>
    /// <param name="vendorID"></param>
    /// <param name="description"></param>
    void CreateProductMediaByUrl(string mediaurl, int productID, int typeID, int vendorID, string description, List<int> updateForMultipleIDs = null);

    /// <summary>
    /// Updates Product media  
    /// </summary>
    /// <param name="MediaID"></param>
    /// <param name="ProductID"></param>
    /// <param name="VendorID"></param>
    /// <param name="sequence_new"></param>
    /// <param name="sequence_old"></param>
    /// <param name="TypeID"></param>
    /// <param name="MediaPath"></param>
    /// <param name="MediaUrl"></param>
    /// <param name="Description"></param>
    /// <param name="TypeID_Old"></param>
    void UpdateProductMedia(int MediaID, int ProductID, int VendorID, int sequence_new, int sequence_old, int TypeID, string MediaPath, string MediaUrl, string Description, int TypeID_Old);

    /// <summary>
    /// Deletes product media 
    /// </summary>
    /// <param name="mediaID"></param>
    void DeleteProductMedia(int mediaID);

    /// <summary>
    /// gets details for a single product id
    /// </summary>
    /// <param name="productID"></param>
    /// <returns></returns>
    ProductDetailDto getProductDetailsBySingleID(int productID);

    /// <summary>
    ///  Creates a product match
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="correspondingProductID"></param>
    /// <param name="productMatchID"></param>
    void CreateMatchForProduct(int productID, int correspondingProductID, int productMatchID);

    /// <summary>
    /// Deletes the match  
    /// </summary>
    /// <param name="correspondingProductID"></param>
    /// <param name="productMatchID"></param>
    void RemoveMatchForProduct(int correspondingProductID, int productMatchID);
    /// <summary>
    /// Copies the product description
    /// </summary>
    /// <param name="vendorID"></param>
    /// <param name="productID"></param>
    /// <param name="languageID"></param>
    void CopyProductDescription(int vendorID, int productID, int languageID);

    /// <summary>
    /// Retrieves the count of products with missing long descriptions per language
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>A list of key value pairs - language obj and count of missing descriptions</returns>
    Dictionary<Language, int> GetMissingLongDescriptionsCount(int[] connectors = null, int[] vendors = null, DateTime? beforeDate = null, DateTime? afterDate = null, DateTime? onDate = null, bool? isActive = null, int[] productGroups = null, int[] brands = null, int? lowerStockQuantity = null, int? greaterStockQuantity = null, int? equalStockQuantity = null, int[] statuses = null, int? descriptionVendorID = null);

    /// <summary>
    ///  MergeProductDescription 
    /// </summary>
    /// <param name="pd"></param>
    void MergeProductDescription(ProductDescription pd);

    /// <summary>
    /// Merge productDescription by id
    /// 
    /// </summary>
    /// <param name="pd"></param>
    void MergeProductDescriptionByID(ProductDescription pd, int productID);

    /// <summary>
    /// get product biy the id's
    /// </summary>
    /// <param name="IDlist"></param>
    /// <returns></returns>
    List<ProductDto> GetProductDetailsByIDs(int[] IDlist, int connectorID);



    /// <summary>
    /// Creates a product group and assigns its language specific translations
    /// </summary>
    /// <param name="pg"></param>
    /// <param name="names"></param>
    void CreateProductGroup(ProductGroup pg, Dictionary<string, string> names);

    /// <summary>
    /// Gets all products for a specific connector directly under a specific product group mapping
    /// </summary>
    /// <param name="productGroupMappingID"></param>
    /// <param name="connectorID">If not supplied, the connector of the currently logged in user will be used</param>
    /// <returns></returns>
    List<ProductDto> GetByProductGroupMapping(int? productGroupMappingID = null, string lineage = null, int? connectorID = null);

    void SetProductGroupTranslations(int languageID, int productGroupID, string name);

    void SetAttributeTranslations(int languageID, int attributeID, string name);

    void SetAttributeValueTranslations(int languageID, int attributeID, int connectorID, string translation, string Value);

    ContentLogic FillPriceInformation(int connectorID);

    ///// <summary>
    ///// Retrieves all products in all levels under a productgroupmapping
    ///// </summary>
    ///// <param name="productGroupMappingID"></param>
    ///// <param name="connectorID"></param>
    ///// <returns></returns>
    //IQueryable GetUnderProductGroupMapping(int productGroupMappingID, int connectorID, IQueryable q);

    /// <summary>
    /// Adds a product to a connector publication
    /// </summary>
    /// <param name="vendorItemNumber">Vendor item number of a product</param>
    /// <param name="connectorID">ConnectorID for which to add the product. VendorID will be inferred from the publication rules</param>
    /// <param name="propagate">Propagate for child products (if any)</param>    
    void AddToConnectorPublication(string vendorItemNumber, int connectorID, bool propagate = true);

    /// <summary>
    /// Removes a product from a connector publication
    /// </summary>
    /// <param name="vendorItemNumber"></param>
    /// <param name="connectorID"></param>
    /// <param name="propagate"></param>
    void RemoveFromConnectorPublication(string vendorItemNumber, int connectorID, bool propagate = true);

    /// <summary>
    /// Sets or updates a product's description.
    /// </summary>
    /// <param name="p">Product</param>
    /// <param name="propagate">If true, it will propagate to the children</param>
    /// <param name="longContentDescription"></param>
    /// <param name="longSummaryDescription"></param>
    /// <param name="modelName"></param>
    /// <param name="pdfSize"></param>
    /// <param name="pdfUrl"></param>
    /// <param name="productName"></param>
    /// <param name="quality"></param>
    /// <param name="shortContentDescription"></param>
    /// <param name="shortSummaryDescription"></param>
    /// <param name="url"></param>
    /// <param name="warrantyInfo"></param>
    void SetOrUpdateProductDescription(Product p, bool propagate, string longContentDescription, int? pdfSize, string pdfUrl, string productName, string quality, string shortContentDescription, string url, int vendorID, int languageID);

    /// <summary>
    /// Updates the productAttributeValue that has an attributeOption (in table ProductAttributeOption)
    /// </summary>
    /// <param name="p"></param>
    /// <param name="AttributeID"></param>
    /// <param name="AttributeOptionID"></param>
    void SetOrUpdateAttributeOption(Product p, int AttributeID, int? AttributeOptionID, bool propagate);

    /// <summary>
    /// Updates an attributeValue of a product
    /// </summary>
    /// <param name="p"></param>
    /// <param name="AttributeID"></param>
    /// <param name="AttributeValue"></param>
    /// <param name="propagate"></param>
    void SetOrUpdateAttributeValue(Product p, int AttributeID, string AttributeValue, bool propagate, int? languageID = null);
  }
}
