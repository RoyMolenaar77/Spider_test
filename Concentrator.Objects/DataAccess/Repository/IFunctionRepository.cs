using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Complex;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Services;
using Concentrator.Objects.Models.Prices;

namespace Concentrator.Objects.DataAccess.EntityFramework
{
  /// <summary>
  /// Defines the functions that the storage needs to contain
  /// </summary>
  public interface IFunctionRepository
  {

    /// <summary>
    /// Fetch product catalog view
    /// </summary>
    /// <param name="vendorID">The vendor id for which the view will be filtered</param>
    /// <returns></returns>
    IEnumerable<ProductResult> GetCatalogResult(int vendorID);

    /// <summary>
    /// Gets all product matches for a specific productID and vendorID. CAUTION: Appears deprecated. Use "GetProductMatches"
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="vendorID"></param>
    /// <returns></returns>
    IEnumerable<FetchProductMatchesResult> FetchProductMatches(int? productID, int? vendorID);

    /// <summary>
    /// Retrieves all product matches
    /// </summary>
    /// <returns>A collection of ProductMatchResult</returns>
    IEnumerable<ProductMatchResult> GetProductMatches();

    /// <summary>
    /// Retrieves vendor assortment for a specific connector
    /// </summary>
    /// <param name="doc">An XML document of VendorIDs</param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    IEnumerable<VendorAssortmentResult> GetVendorAssortment(string doc, int connectorID);

    /// <summary>
    /// Retrieves a list of vendor prices
    /// </summary>
    /// <param name="doc">An XML document of VendorIDs</param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    IEnumerable<VendorPriceResult> GetVendorPrice(string doc, int connectorID);

    /// <summary>
    /// Retrieves custom item numbers for a connector
    /// </summary>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    IEnumerable<CustomItemNumberResult> GetCustomItemNumbers(int connectorID);

    /// <summary>
    /// Retrieves the vendor retail stock
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    IEnumerable<VendorStockResult> GetVendorRetailStock(string doc, int connectorID);

    /// <summary>
    /// Gets the product attributes for a specific product, language and connector
    /// </summary>
    /// <param name="productID"></param>
    /// <param name="languageID"></param>
    /// <param name="connectorID"></param>
    /// <param name="lastUpdate"></param>
    /// <returns></returns>
    IEnumerable<ContentAttribute> GetProductAttributes(int? productID, int languageID, int connectorID, DateTime? lastUpdate);

    /// <summary>
    /// Generates attributes
    /// </summary>
    void GenerateContentAttributes();


    /// <summary>
    /// Updates a product competitor
    /// </summary>
    /// <param name="compareProductID"></param>
    /// <param name="productCompetitorID"></param>
    void UpdateProductCompetitorPrice(int productCompareID, int productCompetitorID);

    /// <summary>
    /// Updates a product compare. Sets the last update time
    /// </summary>
    /// <param name="productCompareID"></param>
    void UpdateProductCompare(int productCompareID);


    /// <summary>
    /// Copies product group mappings
    /// </summary>
    /// <param name="sourceConnectorID"></param>
    /// <param name="destinationConnectorID"></param>
    /// <param name="root"></param>
    /// <param name="rootID"></param>
    /// <param name="copyAttributes"></param>
    /// <param name="copyPrices"></param>
    /// <param name="copyProducts"></param>
    /// <summary>

    /// <param name="copyContetnVendorSettings"></param>
    /// <param name="copyPublications"></param>
    void CopyProductGroupMappings(int sourceConnectorID, int destinationConnectorID, int? root = null, int? rootID = null, bool? copyAttributes = null, bool? copyPrices = null, bool? copyProducts = null, bool? copyContetnVendorSettings = null, bool? copyPublications = null, bool? copyConnectorProductStatuses = null, bool? preferredContentSettings = null);

    /// Searches products matching the query
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="includeDescriptions">Include product descriptions in the search</param>
    /// <param name="includeBrands">Include product brands in the search</param>
    /// <param name="includeProductGroups">Include product product groups in the search</param>
    /// <param name="includeIds">Include product identifiers in the search</param>
    /// <param name="languageID">Language for which to search</param>
    /// <returns></returns>
    IEnumerable<ProductSearchResult> SearchProducts(int languageID, string query, bool? includeDescriptions = null, bool? includeBrands = null, bool? includeProductGroups = null, bool? includeIds = null);

    /// <summary>
    /// Updates the vendor product status  
    /// </summary>
    /// <param name="vendorID"></param>
    /// <param name="concentratorStatusIDOld"></param>
    /// <param name="concentratorStatusIDNew"></param>
    /// <param name="vendorStatus"></param>
    void UpdateVendorProductStatus(int vendorID, int concentratorStatusIDOld, int concentratorStatusIDNew, string vendorStatus);

    /// <summary>
    /// Updates the connector product status
    /// </summary>
    /// <param name="connectorID"></param>
    /// <param name="concentratorStatusIDOld"></param>
    /// <param name="concentratorStatusIDNew"></param>
    /// <param name="connectorStatus"></param>
    void UpdateConnectorProductStatus(int connectorID, int connectorStatusIDOld, int connectorStatusIDNew, int concentratorStatusIDOld, int concentratorStatusIDNew, string connectorStatus);

    /// <summary>
    /// Regenerates all the missing content statistics in the data source
    /// </summary>
    void RegenerateMissingContent(int connectorID);

    /// <summary>
    /// Regenaretes all search results
    /// </summary>
    void RegenerateSearchResults();


    /// <summary>
    /// Retrieves the language <-> description relation for products without filtering by productIDs
    /// </summary>
    /// <param name="ConnectorID">The connectorID for which the products are checked</param>
    /// <param name="vendorID">If supplied, only products that have desc. for this vendor will be included</param>
    /// <returns></returns>
    List<Services.LanguageDescriptionModel> GetLanguageDescriptionCount(int ConnectorID, int? vendorID = null);

    /// <summary>
    /// Calculates and refreshes all vendor prices
    /// </summary>
    void CalculateVendorPrices(int? VendorID = null);

    /// <summary>
    /// Calculates and refreshes all connector prices
    /// </summary>
    void CalculateConnectorPrices(int? ConnectorID = null);

    /// <summary>
    /// Retrieves the assortment content view
    /// </summary>
    /// <param name="connectorId"></param>
    /// <returns></returns>
    IEnumerable<AssortmentContentView> GetAssortmentContentView(int connectorId);

    /// <summary>
    /// Retrieves the calculated price view
    /// </summary>
    /// <param name="connectorId"></param>
    /// <returns></returns>
    IEnumerable<CalculatedPriceView> GetCalculatedPriceView(int connectorId);

    /// <summary>
    /// Retrieve the attribute values and their attribute value groups. 
    /// Returns the attribute value group id or -1 if attribute value is not mapped
    /// </summary>
    /// <param name="connectorID"></param>
    /// <param name="languageID"></param>
    /// <returns></returns>
    IEnumerable<AttributeValueGroupingResult> GetAttributeValueGrouping(int? connectorID, int languageID);

    /// <summary>
    /// Regenerates the flat content table
    /// </summary>
    void RegenarateContentFlat(int connectorID);
  }

}
