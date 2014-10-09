using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using Concentrator.Plugins.ConnectorProductSync.Repositories;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public class SyncProductService : ISyncProductService
  {
    private ILogging log;
    private IMasterGroupMappingRepository masterGroupMappingRepo;
    private IDatabase petaPoco;

    public SyncProductService(IMasterGroupMappingRepository masterGroupMappingRepo, ILogging log, IDatabase petaPoco)
    {
      this.masterGroupMappingRepo = masterGroupMappingRepo;
      this.log = log;
      this.petaPoco = petaPoco;
    }

    /// <summary>
    /// Synchronize Product Groups in Connector Mapping
    /// </summary>
    /// <param name="productsToCopy">Dictionary(ProductGroupID, List<VendorProductInfo>)</param>
    public void SyncProductGroup(Dictionary<int, List<MasterGroupMappingProduct>> productsToSync)
    {
      log.DebugFormat("");
      log.DebugFormat("------> Start Syncing Product Groups");
      int syncedProducts = 0;


//      var prods = (from p in productsToSync
//                   from c in p.Value
//                   select new MasterGroupMappingBulkModel
//                   {
//                     ConnectorPublicationRuleID = c.ConnectorPublicationRuleID ?? 0,
//                     IsApproved = c.IsApproved,
//                     IsCustom = c.IsCustom,
//                     IsProductMapped = c.IsProductMapped,
//                     ProductID = c.ProductID,
//                     MasterGroupMappingID = p.Key
//                   }).ToList();

//      string tableName = string.Format("Temp_Master_Group_Mapping_{0}", connectorID);

//      petaPoco.Execute(string.Format(@"IF (EXISTS (SELECT * 
//                 FROM INFORMATION_SCHEMA.TABLES 
//                 WHERE TABLE_SCHEMA = 'dbo' 
//                 AND  TABLE_NAME = '{0}'))
//      BEGIN
//          drop table [{0}]
//      END
//      ", tableName));

//      petaPoco.Execute(string.Format(@"Create table [{0}](
//                                        MasterGroupMappingID int not null,
//                                        ProductID int not null,
//                                        IsApproved bit not null,
//                                        IsCustom bit not null, 
//                                        IsProductMapped bit not null, 
//                                        ConnectorPublicationRuleID int not null
//                                      )
//                                                                                
//", tableName));

//      using (var connection = new SqlConnection(connectionString))
//      {
//        connection.Open();
//        using (SqlBulkCopy copyBulk = new SqlBulkCopy(connection))
//        {
//          copyBulk.BatchSize = 100000;
//          copyBulk.BulkCopyTimeout = 180;
//          copyBulk.DestinationTableName = tableName;
//          copyBulk.NotifyAfter = 100000;
//          copyBulk.SqlRowsCopied += (s, e) => log.DebugFormat("{0} Records inserted ", e.RowsCopied);

//          using (var collection = new GenericCollectionReader<MasterGroupMappingBulkModel>(prods))
//          {
//            copyBulk.WriteToServer(collection);
//          }
//        }
//      }

//      petaPoco.Execute(string.Format(@"merge mastergroupmappingproduct trg
//using {0} src
//on trg.mastergroupmappingid = src.mastergroupmappingid and trg.productid = src.productid
//when not matched by target 
//	then insert (MasterGroupMappingID, ProductID, IsApproved, IsCustom, IsProductMapped, ConnectorPublicationRuleID)
//	values		(src.MasterGroupMappingID, src.ProductID, src.IsApproved, src.IsCustom, src.IsProductMapped, case when src.ConnectorPublicationRuleID = 0 then null else src.ConnectorPublicationRuleID end)
//when matched 
//then update 
//	set trg.ConnectorPublicationRuleID = case when src.ConnectorPublicationRuleID = 0 then null else src.ConnectorPublicationRuleID end
//when not matched by source and  trg.mastergroupmappingid in (select mastergroupmappingid from mastergroupmapping where connectorid = {1})
//	then delete;", tableName, connectorID));

//      petaPoco.Execute(string.Format(@"drop table {0}", tableName));
      productsToSync.ForEach(productGroup =>
      {
        var currentProductsInProductGroup = masterGroupMappingRepo.GetListOfMappedProductsByMasterGroupMapping(productGroup.Key);

        List<MasterGroupMappingProduct> productsToCopy =
          (from p in productGroup.Value.Distinct()
           join c in currentProductsInProductGroup on p.ProductID equals c.ProductID into notExistProducts
           from nep in notExistProducts.DefaultIfEmpty()
           where nep == null
           select p)
           .ToList();

        List<MasterGroupMappingProduct> productsToUpdate =
          (
            from p in productGroup.Value.Distinct()
            join cp in currentProductsInProductGroup on p.ProductID equals cp.ProductID
            where p.ConnectorPublicationRuleID != cp.ConnectorPublicationRuleID
            select p
          ).ToList();

        List<MasterGroupMappingProduct> productsToDelete =
          (from c in currentProductsInProductGroup.Where(x => x.IsCustom == false)
           join p in productGroup.Value on c.ProductID equals p.ProductID into existProducts
           from ep in existProducts.DefaultIfEmpty()
           where ep == null
           select c)
          .ToList();

        if (productsToCopy.Count() > 0)
        {
          CopyProducts(productGroup.Key, productsToCopy);
          syncedProducts += productsToCopy.Count;
        }
        if (productsToUpdate.Count > 0)
        {
          UpdateProducts(productGroup.Key, productsToUpdate);
          syncedProducts += productsToDelete.Count;
        }
        if (productsToDelete.Count > 0)
        {
          DeleteProducts(productGroup.Key, productsToDelete);
          syncedProducts += productsToDelete.Count;
        }
      });
      log.DebugFormat("------> End Syncing Product Groups done. {0} products are synchronized", syncedProducts);
    }
    
    private void UpdateProducts(int productGroupID, List<MasterGroupMappingProduct> listOfProductsToUpdate)
    {
      listOfProductsToUpdate.ForEach(masterGroupMappingProduct =>
      {
        masterGroupMappingRepo.UpdateMasterGroupMappingProduct(masterGroupMappingProduct);
      });
      log.DebugFormat("{1} Products are updated in product group {0}", productGroupID, listOfProductsToUpdate.Count);
    }
    private void CopyProducts(int productGroupID, List<MasterGroupMappingProduct> listOfProductsToCopy)
    {
      listOfProductsToCopy.ForEach(product =>
      {
        masterGroupMappingRepo.InsertMasterGroupMappingProduct(productGroupID, product);
      });
      log.DebugFormat("{1} Products are copied to product group {0}", productGroupID, listOfProductsToCopy.Count);
    }
    private void DeleteProducts(int productGroupID, List<MasterGroupMappingProduct> listOfProductsToDelete)
    {
      listOfProductsToDelete.ForEach(product =>
      {
        masterGroupMappingRepo.DeleteMasterGroupMappingProduct(product);
      });
      log.DebugFormat("{1} Products are deleted from product group {0}", productGroupID, listOfProductsToDelete.Count);
    }
  }
}
