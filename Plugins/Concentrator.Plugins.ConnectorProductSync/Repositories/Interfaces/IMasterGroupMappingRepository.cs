using Concentrator.Objects.Models.MastergroupMapping;
using System.Collections.Generic;

namespace Concentrator.Plugins.ConnectorProductSync.Repositories
{
  public interface IMasterGroupMappingRepository
  {
    /// <summary>
    /// No working, need code review
    /// </summary>
    /// <param name="masterGroupMappingID"></param>
    /// <returns></returns>
    MasterGroupMapping GetMasterGroupMappingByMasterGroupMappingID(int masterGroupMappingID);
    MasterGroupMappingProduct GetMasterGroupMappingProductByID(int masterGroupMappingID, int productID);

    List<MasterGroupMapping> GetListOfAllMasterGroupMappings();
    List<MasterGroupMapping> GetListOfProductGroupsByConnector(int connectorID);
    List<MasterGroupMapping> GetListOfMasterGroupMappingChildren(int masterGroupMappingID);
    List<MasterGroupMapping> GetListOfMasterGroupMappingsByProductGroupID(int productGroupID);
    List<MasterGroupMappingProduct> GetListOfMappedProductsByMasterGroupMapping(int masterGroupMappingID);
    List<MasterGroupMappingLanguage> GetListOfMasterGroupMappingLanguagesByMasterGroupMappingID(int masterGroupMappingID);

    int InsertMasterGroupMapping(MasterGroupMapping newMasterGroupMapping);

    void InsertMasterGroupMappingLanguage(MasterGroupMappingLanguage newMasterGroupMappingLanguage);
    void InsertMasterGroupMappingProduct(int masterGroupMappingID, int productID);
    void InsertMasterGroupMappingProduct(int masterGroupMappingID, MasterGroupMappingProduct masterGroupMappingProduct);

    // remove this function and use UpdateMasterGroupMappingProduct
    void UpdateMasterGroupMappingIDOfMasterGroupMappingProduct(MasterGroupMappingProduct masterGroupMappingProduct, int masterGroupMappingID);

    void UpdateMasterGroupMappingProduct(MasterGroupMappingProduct masterGroupMappingProduct);
    void UpdateMasterGroupMapping(MasterGroupMapping masterGroupMapping);
    void UpdateMasterGroupMapping(MasterGroupMapping currentMasterGroupMapping, MasterGroupMapping compareMasterGroupMapping, List<string> ignoreProperties);

    void DeleteMasterGroupMapping(MasterGroupMapping masterGroupMapping);
    void DeleteHierarchy(MasterGroupMapping masterGroupMapping);
    void DeleteMasterGroupMappingProduct(MasterGroupMappingProduct masterGroupMappingProduct);
    void DeleteMasterGroupMappingLanguage(MasterGroupMapping masterGroupMapping);
    void DeleteProductsByMasterGroupMappingID(int masterGroupMappingID);

    void MapVendorProductGroupToMasterGroupMapping(int vendorProductGroupID, int masterGroupMappingID);
    void ImportProductGroupMappingPerConnector(int connectorID);
    /// <summary>
    /// No working, need code review
    /// </summary>
    /// <param name="masterGroupMappingID"></param>
    /// <returns></returns>
    bool IsMasterGroupMappingExists(int masterGroupMappingID);
  }
}
