using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using Concentrator.Plugins.ConnectorProductSync.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public class FilterByParentProductGroupService : IFilterByParentProductGroupService
  {

    private ILogging log; 
    private IProductRepository productRepo;
    private IMasterGroupMappingRepository masterGroupMappingRepo;

    public FilterByParentProductGroupService(
      ILogging log,
      IProductRepository productRepo,
      IMasterGroupMappingRepository masterGroupMappingRepo
      )
    {
      this.log = log;
      this.productRepo = productRepo;
      this.masterGroupMappingRepo = masterGroupMappingRepo;
    }
    
    public List<MasterGroupMapping> GetListOfProductGroupsWithFilterByParent(Connector connector)
    {
      List<MasterGroupMapping> productGroups = masterGroupMappingRepo
        .GetListOfProductGroupsByConnector(connector.ConnectorID)
        .Where(x=> x.FilterByParentGroup == true)
        .ToList()
        ;
      return productGroups;
    }

    public bool IsProductGroupValide(MasterGroupMapping productGroup)
    {
      if (productGroup.ParentMasterGroupMappingID.HasValue && productGroup.ParentMasterGroupMappingID.Value > 0)
      {
        List<Product> productInProductGroup = productRepo.GetListOfMappedProductsByMasterGroupMapping(productGroup.MasterGroupMappingID);
        if (productInProductGroup.Count > 0)
        {
          if (masterGroupMappingRepo.IsMasterGroupMappingExists(productGroup.ParentMasterGroupMappingID.Value))
          {
            //List<Product> productInParentProductGroup = productRepo.GetListOfMappedProductsByMasterGroupMapping(productGroup.ParentMasterGroupMappingID.Value);
            //if (productInParentProductGroup.Count > 0)
            //{
            //  return true;
            //}
            return true;
          }
        }
      }
      return false;
    }

    public void FilterProductGroupByParent(List<MasterGroupMapping> listOfProductGroups)
    {
      listOfProductGroups.ForEach(productGroup => {
        List<Product> productsInProductGroup = productRepo.GetListOfMappedProductsByMasterGroupMapping(productGroup.MasterGroupMappingID);
        List<Product> productsInParentProductGroup = productRepo.GetListOfMappedProductsByMasterGroupMapping(productGroup.ParentMasterGroupMappingID.Value);

        List<Product> productsToDelete = (
            from p in productsInProductGroup
            join pp in productsInParentProductGroup on p.ProductID equals pp.ProductID into notExistProducts
            from nep in notExistProducts.DefaultIfEmpty()
            where nep == null
            select p
          )
          .ToList();

        productsToDelete.ForEach(product => {
          MasterGroupMappingProduct newMasterGroupMappingProduct = new MasterGroupMappingProduct()
          {
            MasterGroupMappingID = productGroup.MasterGroupMappingID,
            ProductID = product.ProductID
          };
          masterGroupMappingRepo.DeleteMasterGroupMappingProduct(newMasterGroupMappingProduct);
        });
        log.DebugFormat("{0} Products Deleted From Product Group {1}", productsToDelete.Count, productGroup.MasterGroupMappingID);
      });
    }
  }
}
