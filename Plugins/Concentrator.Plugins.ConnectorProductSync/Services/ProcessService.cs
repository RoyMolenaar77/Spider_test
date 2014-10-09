using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using Concentrator.Plugins.ConnectorProductSync.Models;
using Concentrator.Plugins.ConnectorProductSync.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public class ProcessService : IProcessService
  {
    private ILogging log;
    private IProductRepository productRepo;
    private ISyncContentService contentService;
    private IConnectorRepository connectorRepo;
    private ISyncProductService syncProductService;
    private IMasterGroupMappingRepository masterGroupMappingRepo;
    private ISyncProductGroupMappingService syncProductGroupMapping;
    private ISyncContentProductGroupService contentProductGroupService;
    private IFlattenHierachyProductGroupService flattenHierachyService;
    private IConnectorPublicationRuleRepository connectorPublicarionRuleRepo;
    private IFilterByParentProductGroupService filterByParentProductGroupService;

    public ProcessService(
      ILogging log,
      IProductRepository productRepo,
      ISyncContentService contentService,
      IConnectorRepository connectorRepo,
      ISyncProductService syncProductService,
      IMasterGroupMappingRepository masterGroupMappingRepo,
      ISyncProductGroupMappingService syncProductGroupMapping,
      ISyncContentProductGroupService contentProductGroupService,
      IFlattenHierachyProductGroupService flattenHierachyService,
      IConnectorPublicationRuleRepository connectorPublicarionRuleRepo,
      IFilterByParentProductGroupService filterByParentProductGroupService
      )
    {
      this.log = log;
      this.productRepo = productRepo;
      this.connectorRepo = connectorRepo;
      this.contentService = contentService;
      this.syncProductService = syncProductService;
      this.masterGroupMappingRepo = masterGroupMappingRepo;
      this.flattenHierachyService = flattenHierachyService;
      this.syncProductGroupMapping = syncProductGroupMapping;
      this.contentProductGroupService = contentProductGroupService;
      this.connectorPublicarionRuleRepo = connectorPublicarionRuleRepo;
      this.filterByParentProductGroupService = filterByParentProductGroupService;
    }

    public void Process()
    {
      //op dit moment is er al een mgm
      //het doel van deze plugin is het vullen ven de content en content product tabel 

      List<Connector> connectors = new List<Connector>();
      connectors = connectorRepo.GetListOfActiveConnectors();

#if DEBUG
      //connectors = connectorRepo.GetListOfActiveConnectors().Where(x => x.ConnectorID == 1 || x.ConnectorID == 5468 || x.ConnectorID == 5474).OrderByDescending(x => x.ConnectorID).ToList();
      connectors = connectorRepo.GetListOfActiveConnectors().Where(x => x.ConnectorID == 5480).ToList();
#else
        connectors =  connectorRepo.GetListOfActiveConnectors();  
#endif

      int countConnectors = 0;
      connectors.ForEach(connector =>
      {
        log.DebugFormat("");
        log.DebugFormat("------------------------- Syncing Connector: {0} ({1}). Connector to sync: {2} -------------------------------------",
          connector.ConnectorID,
          connector.Name,
          connectors.Count - countConnectors);

        //kijkt of deze connector een parent heeft, zo ja dan gaat deze synchroniseren
        if (connector.ParentConnectorID.HasValue && connector.ParentConnectorID.Value > 0)
        {
          ProcessSyncChildConnector(connector);
        }

        // Sync Products 
        //kijken welke producten volgens connector publication rule gekopieerd moeten worden vanuit mgm naar connector mgm.
        ProcessSyncProductGroups(connector);

        // Filter By Parent
        //als een mgm 'filter by parent' optie heeft dan kijkt de plugin naar de child van die mgm. Zitten er producten in de child die niet in de parent voorkomen dan zullen die verwijderd worden
        ProcessFilterByParentProductGroups(connector);

        EmptyParentProductGroupOfFilterByParentProductGroups(connector);

        // Flatten Hierachy
        //kijkt of een mgm flatten by hierarchy heeft, zo ja dan voeg je alle producten (van de childs van die mgm) bij elkaar
        ProcessFlattenHierachyProductGroups(connector);


        // Sync Contents 
        //vanuit connector mgm gaan we de content tabel vullen
        ProcessSyncContent(connector);

        // Sync Content Product Groups
        //vanuit connector mgm gaan we de content product group tabel vullen
        ProcessSyncContentProductGroup(connector);

        countConnectors++;
      });
    }

    private void ProcessSyncChildConnector(Connector connector)
    {
      log.DebugFormat("");
      log.DebugFormat("------> Start Syncing Product Group Mapping Process");

      Connector parentConnector = connectorRepo.GetConnectorByID(connector.ParentConnectorID.Value);
      log.DebugFormat("---> Connector {0} is a child Connector and have {1} as parent connector", connector.Name, parentConnector.Name);

      List<MasterGroupMapping> listOfParentProductGroupMappings = syncProductGroupMapping.GetListOfProductGroupMapping(parentConnector);
      log.DebugFormat("---> Parent Connector have {0} Product Group Mappings", listOfParentProductGroupMappings.Count);

      syncProductGroupMapping.SyncChildConnectorMapping(listOfParentProductGroupMappings, connector);

      log.DebugFormat("------> End Syncing Product Group Mapping Process");
    }

    private void ProcessSyncProductGroups(Connector connector)
    {
      Dictionary<int, List<MasterGroupMappingProduct>> listOfProductsToSync = new Dictionary<int, List<MasterGroupMappingProduct>>();

      // Get Product Groups By Connector + Source MasterGroupMapping
      List<MasterGroupMapping> productGroups = masterGroupMappingRepo
        .GetListOfProductGroupsByConnector(connector.ConnectorID)
        .Where(pg => pg.SourceMasterGroupMappingID != null)
        .ToList();

      if (productGroups.Count > 0)
      {
        // Get Products By ConnectorPublicationRules
        List<VendorProductInfo> products = GetListOfProductsByConnectorConnectorPublicationRules(connector.ConnectorID);

        log.DebugFormat("");
        log.DebugFormat("-----> Start: Get Mapped Products");
        productGroups.ForEach(productGroup =>
        {
          // Fill syncList with products
          List<MasterGroupMappingProduct> mappedProductsBySourceMasterGroupMapping = GetListOfMappedProductsBySourceMasterGroupMapping(productGroup, products);
          listOfProductsToSync.Add(productGroup.MasterGroupMappingID, mappedProductsBySourceMasterGroupMapping);
        });
        log.DebugFormat("-----> End: Get Mapped Products. Count product Groups: {0}, Product To Sync: {1}", listOfProductsToSync.Count, listOfProductsToSync.Values.SelectMany(y => y).Count());

        syncProductService.SyncProductGroup(listOfProductsToSync);
      }
    }

    private List<MasterGroupMappingProduct> GetListOfMappedProductsBySourceMasterGroupMapping(MasterGroupMapping productGroup, List<VendorProductInfo> productsPerConnector)
    {
      List<MasterGroupMappingProduct> mappedProducts = new List<MasterGroupMappingProduct>();
      if (productGroup.SourceMasterGroupMappingID.HasValue && productGroup.SourceMasterGroupMappingID.Value > 0)
      {
        if (masterGroupMappingRepo.IsMasterGroupMappingExists(productGroup.SourceMasterGroupMappingID.Value))
        {
          var currentProductsInMasterGroupMapping = masterGroupMappingRepo.GetListOfMappedProductsByMasterGroupMapping(productGroup.SourceMasterGroupMappingID.Value);
          
          var mappedProductsInMasterGroupMapping =
            (from p in productsPerConnector
             join mp in currentProductsInMasterGroupMapping on p.ProductID equals mp.ProductID
             select mp)
            .ToList();

          if (mappedProductsInMasterGroupMapping.Count > 0)
          {
            mappedProductsInMasterGroupMapping.ForEach(mappedProduct =>
            {
              MasterGroupMappingProduct toSyncMasterGroupMappingProduct = new MasterGroupMappingProduct()
              {
                MasterGroupMappingID = productGroup.MasterGroupMappingID,
                ProductID = mappedProduct.ProductID,
                IsApproved = mappedProduct.IsApproved,
                IsProductMapped = true,                
                ConnectorPublicationRuleID = productsPerConnector.Where(x => x.ProductID == mappedProduct.ProductID).Select(x => x).FirstOrDefault().ConnectorPublicationRuleID
              };
              mappedProducts.Add(toSyncMasterGroupMappingProduct);
            });            
          }
        }
      }
      return mappedProducts;
    }

    private List<VendorProductInfo> GetListOfProductsByConnectorConnectorPublicationRules(int connectorID)
    {
      log.DebugFormat("");
      log.DebugFormat("-----> Start Getting Product By Connector Publication Rules");
      List<VendorProductInfo> products = new List<VendorProductInfo>();
      var connectorRules = connectorPublicarionRuleRepo
        .GetListOfConnectorPublicationRuleByConnector(connectorID)
        .Where(x => (x.FromDate.HasValue ? (DateTime.Now > x.FromDate) : true))
        .Where(x => (x.ToDate.HasValue ? (DateTime.Now < x.ToDate) : true))
        .OrderBy(x => x.PublicationIndex);

      connectorRules.ForEach(connectorRule =>
      {
        var productsByConnectorPublicationRule = productRepo.GetListOfProductsByConnectorPublicationRule(connectorRule);
        if (productsByConnectorPublicationRule.Count > 0)
        {
          switch (connectorRule.PublicationType)
          {
            case (int)ConnectorPublicationRuleType.Include:
              log.DebugFormat("Connector Publication Rule ID: {0}({2}), Count Products: {1} - Include",
                connectorRule.ConnectorPublicationRuleID,
                productsByConnectorPublicationRule.Count,
                connectorRule.PublicationIndex);

              var productsToRemove =
                (
                  from p in products
                  join cprp in productsByConnectorPublicationRule on p.ProductID equals cprp.ProductID
                  select p
                ).ToList();

              if (productsToRemove.Count > 0)
              {
                productsToRemove.ForEach(vendorProduct =>
                {
                  products.RemoveAll(x => x.ProductID == vendorProduct.ProductID);
                });
              }

              products.AddRange(productsByConnectorPublicationRule);

              break;
            case (int)ConnectorPublicationRuleType.Exclude:
              log.DebugFormat("Connector Publication Rule ID: {0}({2}), Count Products: {1} - Exclude",
                connectorRule.ConnectorPublicationRuleID,
                productsByConnectorPublicationRule.Count,
                connectorRule.PublicationIndex);
              productsByConnectorPublicationRule.ForEach(vendorProduct =>
              {
                products.RemoveAll(x => x.ProductID == vendorProduct.ProductID);
              });
              break;
          }
        }
      });
      log.DebugFormat("List of products by Connector Publication Rule: {0}", products.Count);
      log.DebugFormat("-----> End Getting Product By Connector Publication Rules");
      return products;
    }

    private void ProcessFlattenHierachyProductGroups(Connector connector)
    {
      log.DebugFormat("");
      log.DebugFormat("------> Start FlattenHierachy Product Group Process");
      List<MasterGroupMapping> listOfFlattenHierachyProductGroups = flattenHierachyService.GetListOfProductGroupsWithFlattenHierachy(connector.ConnectorID);
      List<MasterGroupMapping> listOfHighestFlattenHierachyProductGroups = flattenHierachyService.GetListOfHighestProductGroupWithFlattenHierachy(listOfFlattenHierachyProductGroups);
      flattenHierachyService.MoveProductsToHighestFlattenHierachyProductGroup(listOfHighestFlattenHierachyProductGroups);
      log.DebugFormat("------> End FlattenHierachy Product Group Process");
    }

    private void ProcessFilterByParentProductGroups(Connector connector)
    {
      log.DebugFormat("");
      log.DebugFormat("------> Start Filter By Parent Product Group Process");

      List<MasterGroupMapping> listOfProductGroups = filterByParentProductGroupService.GetListOfProductGroupsWithFilterByParent(connector);
      List<MasterGroupMapping> listOfValideProductGroups = new List<MasterGroupMapping>();
      listOfProductGroups.ForEach(productGroup =>
      {
        if (filterByParentProductGroupService.IsProductGroupValide(productGroup))
        {
          listOfValideProductGroups.Add(productGroup);
        }
      });
      filterByParentProductGroupService.FilterProductGroupByParent(listOfValideProductGroups);

      log.DebugFormat("------> End Filter By Parent Product Group Process");
    }

    private void EmptyParentProductGroupOfFilterByParentProductGroups(Connector connector)
    {
      log.DebugFormat("");
      log.DebugFormat("------> Start Empty Parent Product Groups of Filter By Parent Product Groups");

      List<MasterGroupMapping> listOfProductGroups = filterByParentProductGroupService.GetListOfProductGroupsWithFilterByParent(connector);
      var listOfParentProductGroups = listOfProductGroups.Where(x => x.ParentMasterGroupMappingID != null).Select(x => x.ParentMasterGroupMappingID).Distinct();

      listOfParentProductGroups.ForEach(masterGroupMappingID =>
      {
        masterGroupMappingRepo.DeleteProductsByMasterGroupMappingID(masterGroupMappingID.Value);
      });
    }

    private void ProcessSyncContent(Connector connector)
    {
      int counter = 0;
      int logCount = 0;
      int totalContentsCount = 0;

      log.DebugFormat("");
      log.DebugFormat("------> Start Sync Content Process");

      List<Content> listOfCurrentContents = contentService.GetListOfContentByConnector(connector);
      log.DebugFormat("---> Current Contents: {0} Contents", listOfCurrentContents.Count);

      List<ContentInfo> listOfMappedProducts = contentService.GetListOfMappedProductsByConnector(connector);
      log.DebugFormat("---> Mapped products: {0} Contents", listOfMappedProducts.Count);

      List<Content> listOfContentToDelete = contentService.GetListOfContentToDelete(listOfCurrentContents, listOfMappedProducts);
      log.DebugFormat("---> {0} Contents to Delete", listOfContentToDelete.Count);

      List<Content> listOfContentToUpdate = contentService.GetListOfContentToUpdate(listOfCurrentContents, listOfMappedProducts);
      log.DebugFormat("---> {0} Contents to Update", listOfContentToUpdate.Count);

      List<Content> listOfContentToInsert = contentService.GetListOfContentToInsert(listOfCurrentContents, listOfMappedProducts, connector);
      log.DebugFormat("---> {0} Contents to Insert", listOfContentToInsert.Count);

      totalContentsCount = listOfContentToInsert.Count + listOfContentToDelete.Count + listOfContentToUpdate.Count;
      if (totalContentsCount > 0)
      {
        log.DebugFormat("Waiting: 60 sec");
        //Thread.Sleep(60000); 

        if (listOfContentToInsert.Count > 0)
        {
          log.DebugFormat("---> Start inserting Contents");
          listOfContentToInsert.ForEach(content =>
          {
            counter++;
            logCount++;
            if (logCount == 100)
            {
              log.DebugFormat("Contents Processed: {0}/{1}", counter, totalContentsCount);
              logCount = 0;
            }
            contentService.InsertContent(content);
          });
        }

        if (listOfContentToDelete.Count > 0)
        {
          log.DebugFormat("---> Start deleting Contents");
          listOfContentToDelete.ForEach(content =>
          {
            counter++;
            logCount++;
            if (logCount == 100)
            {
              log.DebugFormat("Contents Processed: {0}/{1}", counter, totalContentsCount);
              logCount = 0;
            }
            contentService.DeleteContent(content);
          });
        }

        if (listOfContentToUpdate.Count > 0)
        {
          log.DebugFormat("---> Start updating Contents");
          listOfContentToUpdate.ForEach(content =>
          {
            counter++;
            logCount++;
            if (logCount == 100)
            {
              log.DebugFormat("Contents Processed: {0}/{1}", counter, totalContentsCount);
              logCount = 0;
            }
            contentService.UpdateContent(content);
          });
        }
      }
      log.DebugFormat("------> End Sync Content Process");
    }

    private void ProcessSyncContentProductGroup(Connector connector)
    {
      int counter = 0;
      int logCount = 0;
      int totalContentProductGroupsCount = 0;

      log.DebugFormat("");
      log.DebugFormat("------> Start Sync Content Product Group Process");

      Dictionary<int, List<Content>> listOfContentsPerProductGroup = contentProductGroupService.GetListOfContentsPerProductGroupByConnector(connector);

      List<ContentProductGroup> listOfCurrentContentProductGroups = contentProductGroupService.GetListOfContentProductGroups(connector);
      log.DebugFormat("---> Current Content Product Groups: {0}", listOfCurrentContentProductGroups.Count);

      List<ContentProductGroup> listOfContentProductGroupsToInsert = contentProductGroupService.GetListOfContentProductGroupsToInsert(listOfContentsPerProductGroup, listOfCurrentContentProductGroups);
      log.DebugFormat("---> {0} Content Product Groups to Insert", listOfContentProductGroupsToInsert.Count);

      List<ContentProductGroup> listOfContentProductGroupsToDelete = contentProductGroupService.GetListOfContentProductGroupsToDelete(listOfContentsPerProductGroup, listOfCurrentContentProductGroups);
      log.DebugFormat("---> {0} Content Product Groups to Delete", listOfContentProductGroupsToDelete.Count);

      totalContentProductGroupsCount = listOfContentProductGroupsToInsert.Count + listOfContentProductGroupsToDelete.Count;
      log.DebugFormat("---> Start inserting Content Product Groups");
      listOfContentProductGroupsToInsert.ForEach(contentProductGroup =>
      {
        counter++;
        logCount++;
        if (logCount == 100)
        {
          log.DebugFormat("Content Product Groups Processed: {0}/{1}", counter, totalContentProductGroupsCount);
          logCount = 0;
        }
        contentProductGroupService.InsertContentProductGroup(contentProductGroup);
      });

      log.DebugFormat("---> Start deleting Content Product Groups");
      listOfContentProductGroupsToDelete.ForEach(contentProductGroup =>
      {
        counter++;
        logCount++;
        if (logCount == 100)
        {
          log.DebugFormat("Content Product Groups Processed: {0}/{1}", counter, totalContentProductGroupsCount);
          logCount = 0;
        }
        contentProductGroupService.DeleteContentProductGroup(contentProductGroup);
      });

      log.DebugFormat("------> End Sync Content Product Group Process");
    }
  }
}
