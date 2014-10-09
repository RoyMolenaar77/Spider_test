using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Products;
using System.Web.UI.WebControls;
using Concentrator.Objects.Services.DTO;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IProductGroupMappingService
  {
    /// <summary>
    /// Searches within the product group mapping hierarchy
    /// </summary>
    /// <param name="query">Query to look for. Will compare to the product group name</param>
    /// <param name="languageSpecific">Whether to look in all languages for the term or in the current</param>
    /// <param name="levels">Number of levels to preload. Defaults to -1, Max</param>
    /// <param name="connectorID"></param>
    /// <returns></returns>
    List<ProductGroupMappingDto> Search(string query, bool languageSpecific = true, int levels = -1, int? connectorID = null);

    /// <summary>
    /// Retrieves the product group mappings per content
    /// </summary>
    /// <returns></returns>
    IQueryable<ContentProductGroupMapping> GetContentProductGroupMappings();

    /// <summary>
    /// Retrieves the product group mapping per by ParentID
    /// </summary>
    /// <param name="parentID"></param>
    /// <param name="ids"></param>
    /// <returns></returns>
    IQueryable<ProductGroupMapping> GetProductGroupPerParent(int parentID, string ids);

    /// <summary>
    /// Copies the content  
    /// </summary>
    /// <param name="sourceConnectorID"></param>
    /// <param name="destinationConnectorID"></param>
    /// <returns></returns>
    void CopyProductGroupMapping(int sourceConnectorID, int destinationConnectorID);

    /// <summary>
    /// Generates mapping for brand  
    /// </summary>
    /// <param name="connectorID"></param>
    /// <param name="Score"></param>
    /// <returns></returns>
    void GenerateBrandMapping(int connectorID, int Score);

    /// <summary>
    /// Creates a product group attribute mapping  
    /// </summary>
    /// <param name="AttributeID"></param>
    /// <param name="value"></param>
    /// <param name="productGroupMappingID"></param>
    void CreateProductGroupAttributeMapping(int AttributeID, string value, int productGroupMappingID);

    /// <summary>
    /// Deletes a product group mapping attribute
    /// </summary>
    /// <param name="AttributeID"></param>
    /// <param name="productGroupMappingID"></param>
    void DeleteProductGroupMappingAttribute(int AttributeID, int productGroupMappingID);

    /// <summary>
    /// Publish product group mappings
    /// </summary>
    /// <param name="sourceConnectorID"></param>
    /// <param name="destinationConnectorID"></param>
    /// <param name="root"></param>
    /// <param name="rootID"></param>
    /// <param name="copyAttributes"></param>
    /// <param name="copyPrices"></param>
    /// <param name="copyProducts"></param>
    /// <param name="copyContentVendorSettings"></param>
    /// <param name="copyPublications"></param>
    void Publish(int sourceConnectorID, int destinationConnectorID, int? root = null, int? rootID = null, bool? copyAttributes = null, bool? copyPrices = null, bool? copyProducts = null, bool? copyContentVendorSettings = null, bool? copyPublications = null, bool? copyConnectorProductStatuses = null, bool? preferreContentSettings = null);

    /// <summary>
    /// Deletes a product group mapping
    /// </summary>
    /// <param name="id"></param>
    void Delete(int id);

    /// <summary>
    /// Selects all product group mappings based on their lineage
    /// </summary>
    /// <param name="lineage">The lineage e.g /1/2/3/4/ </param>
    /// <param name="connectorID">If not supplied the current user's connector will be used</param>
    /// <param name="excludeLastLevel">If true the last level in the hierarchy will be omitted</param>
    /// <returns></returns>
    List<ProductGroupMappingDto> GetByLineage(string lineage, int? connectorID = null, bool wholeTree = true);

    /// <summary>
    /// Delete whole mapping for specific Connector
    /// </summary>
    /// <param name="connectorID"></param>
    void DeleteWholeConnectorMapping(int connectorID);

  }
}
