using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using Concentrator.Plugins.ConnectorProductSync.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public class FlattenHierachyProductGroupService: IFlattenHierachyProductGroupService
  {
    private ILogging log;
    private IMasterGroupMappingRepository masterGroupMappingRepo;
    public FlattenHierachyProductGroupService(
      ILogging log,
      IMasterGroupMappingRepository masterGroupMappingRepo
      )
    {
      this.log = log;
      this.masterGroupMappingRepo = masterGroupMappingRepo;
    }

    public List<MasterGroupMapping> GetListOfProductGroupsWithFlattenHierachy(int connectorID)
    {
      List<MasterGroupMapping> listOfFlattenHierachyProductGroups = masterGroupMappingRepo
        .GetListOfProductGroupsByConnector(connectorID)
        .Where(x => x.FlattenHierarchy == true)
        .ToList();

      return listOfFlattenHierachyProductGroups;
    }

    public List<MasterGroupMapping> GetListOfHighestProductGroupWithFlattenHierachy(List<MasterGroupMapping> listOfFlattenHierachyProductGroups)
    {
      List<MasterGroupMapping> listOfHighestFlattenHierachyProductGroups = new List<MasterGroupMapping>();
      listOfHighestFlattenHierachyProductGroups.AddRange(listOfFlattenHierachyProductGroups);

      listOfFlattenHierachyProductGroups.ForEach(productGroup => {
        List<MasterGroupMapping> productGroupChildren = masterGroupMappingRepo.GetListOfMasterGroupMappingChildren(productGroup.MasterGroupMappingID);
        log.DebugFormat("Flatten Product Group {0}. Product Group {0} have {1} Sub Product Groups", productGroup.MasterGroupMappingID, productGroupChildren.Count);
        productGroupChildren.ForEach(productGroupChild => {
          listOfHighestFlattenHierachyProductGroups.RemoveAll(x => x.MasterGroupMappingID == productGroupChild.MasterGroupMappingID);
        });
      });
      return listOfHighestFlattenHierachyProductGroups;
    }

    public void MoveProductsToHighestFlattenHierachyProductGroup(List<MasterGroupMapping> listOfHighestFlattenHierachyProductGroups)
    {
      listOfHighestFlattenHierachyProductGroups.ForEach(productGroup => {
        List<MasterGroupMappingProduct> listOfMappedProductsInParentProductGroup = masterGroupMappingRepo.GetListOfMappedProductsByMasterGroupMapping(productGroup.MasterGroupMappingID);
        List<MasterGroupMapping> listOfProductGroupChildren = masterGroupMappingRepo.GetListOfMasterGroupMappingChildren(productGroup.MasterGroupMappingID);

        List<MasterGroupMappingProduct> tempListOfMappedProductsInAllProductGroup = new List<MasterGroupMappingProduct>();
        tempListOfMappedProductsInAllProductGroup.AddRange(listOfMappedProductsInParentProductGroup);
        List<MasterGroupMappingProduct> listOfProductsToMove = new List<MasterGroupMappingProduct>();
        List<MasterGroupMappingProduct> listOfProductsToDelete = new List<MasterGroupMappingProduct>();
        listOfProductGroupChildren.ForEach(masterGroupMapping =>
        {
          List<MasterGroupMappingProduct> listOfMappedProductsInChildProductGroup = masterGroupMappingRepo.GetListOfMappedProductsByMasterGroupMapping(masterGroupMapping.MasterGroupMappingID);
          List<MasterGroupMappingProduct> tempListofProductsToMove =
            (from mcp in listOfMappedProductsInChildProductGroup
             join mpp in tempListOfMappedProductsInAllProductGroup on mcp.ProductID equals mpp.ProductID into notExistProducts
             from nep in notExistProducts.DefaultIfEmpty()
             where nep == null
             select mcp)
             .ToList();
          listOfProductsToMove.AddRange(tempListofProductsToMove);

          List<MasterGroupMappingProduct> tempListofProductsToDelete =
            (from mcp in listOfMappedProductsInChildProductGroup
             join mpp in tempListOfMappedProductsInAllProductGroup on mcp.ProductID equals mpp.ProductID
             select mcp)
             .ToList();
          listOfProductsToDelete.AddRange(tempListofProductsToDelete);

          tempListOfMappedProductsInAllProductGroup.AddRange(tempListofProductsToMove);
        });

        if (listOfProductsToMove.Count > 0)
        {
          MoveMappedProducts(listOfProductsToMove, productGroup.MasterGroupMappingID);
        }

        if (listOfProductsToDelete.Count > 0)
        {
          DeleteMappedProducts(listOfProductsToDelete);
        }
      });
    }

    private void MoveMappedProducts(List<MasterGroupMappingProduct> listOfMappedProducts, int toMasterGroupMapping)
    {
      listOfMappedProducts.ForEach(product => {
        masterGroupMappingRepo.UpdateMasterGroupMappingIDOfMasterGroupMappingProduct(product, toMasterGroupMapping);
      });
    }

    private void DeleteMappedProducts(List<MasterGroupMappingProduct> listOfMappedProducts)
    {
      listOfMappedProducts.ForEach(product =>
      {
        masterGroupMappingRepo.DeleteMasterGroupMappingProduct(product);
      });
    }
  }
}
