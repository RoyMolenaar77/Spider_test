using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using Concentrator.Plugins.ConnectorProductSync.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public class ProcessImportService : IProcessImportService
  {
    private ILogging log;
    private IConnectorRepository connectorRepo;
    private IProductGroupRepository productGroupRepo;
    private IMasterGroupMappingRepository masterGroupMappingRepo;
    private IProductGroupMappingRepository productGroupMappingRepo;
    private IMagentoProductGroupSettingRepository magentoSettingRepo;

    public ProcessImportService(
      ILogging log,
      IConnectorRepository connectorRepo,
      IProductGroupRepository productGroupRepo,
      IMasterGroupMappingRepository masterGroupMappingRepo,
      IProductGroupMappingRepository productGroupMappingRepo,
      IMagentoProductGroupSettingRepository magentoSettingRepo
      )
    {
      this.log = log;
      this.connectorRepo = connectorRepo;
      this.productGroupRepo = productGroupRepo;
      this.masterGroupMappingRepo = masterGroupMappingRepo;
      this.productGroupMappingRepo = productGroupMappingRepo;
      this.magentoSettingRepo = magentoSettingRepo;
    }

    public void ImportProductGroups()
    {
      log.DebugFormat("");
      log.DebugFormat("-------> Start Importing ProductGroups");

      log.DebugFormat("Create Parent Master Group Mapping 'Old Product Groups'");
      int masterGroupMappingID = CreateNewMasterGroupMapping("Old Product Groups", null);
      int productGroupID = CreateNewMasterGroupMapping("Product Groups", masterGroupMappingID);
      int brandProductGroupID = CreateNewMasterGroupMapping("Brand", masterGroupMappingID);
      log.DebugFormat("MasterGroupMappingID: {0}", masterGroupMappingID);

      log.DebugFormat("Set Product GroupID to -1 by all current Master Group Mappings");
      UpdateCurrentMasterGroupMappings();

      log.DebugFormat("Copy Product Groups To Master Group Mapping");
      CopyProductGroupsToMasterGroupMapping(productGroupID, brandProductGroupID);

      log.DebugFormat("-------> End Importing ProductGroups");
    }

    private int CreateNewMasterGroupMapping(string masterGroupMappingName, int? parentMasterGroupMappingID)
    {
      MasterGroupMapping newMasterGroupMapping = new MasterGroupMapping()
      {
        ProductGroupID = -1,
        Score = 100
      };

      if (parentMasterGroupMappingID.HasValue && parentMasterGroupMappingID.Value > 0)
      {
        newMasterGroupMapping.ParentMasterGroupMappingID = parentMasterGroupMappingID.Value;
      }

      int masterGroupMappingID = masterGroupMappingRepo.InsertMasterGroupMapping(newMasterGroupMapping);

      MasterGroupMappingLanguage newMasterGroupMappingLanguage = new MasterGroupMappingLanguage()
      {
        MasterGroupMappingID = masterGroupMappingID,
        LanguageID = 2,
        Name = masterGroupMappingName
      };

      masterGroupMappingRepo.InsertMasterGroupMappingLanguage(newMasterGroupMappingLanguage);
      return masterGroupMappingID;
    }
    private void UpdateCurrentMasterGroupMappings()
    {
      List<MasterGroupMapping> listOfMasterGroupMappings = masterGroupMappingRepo.GetListOfAllMasterGroupMappings();
      listOfMasterGroupMappings
        .Where(x => x.ConnectorID == null)
        .Where(x => x.ProductGroupID != -1)
        .ForEach(masterGroupMapping =>
        {
          masterGroupMapping.ProductGroupID = -1;
          masterGroupMappingRepo.UpdateMasterGroupMapping(masterGroupMapping);
        });
    }
    private void CopyProductGroupsToMasterGroupMapping(int productGroupID, int brandProductGroupID)
    {
      List<ProductGroup> listOfActiveProductGroups = productGroupRepo.GetListOfActiveProductGroups();

      int scoreCounter = 0;
      listOfActiveProductGroups.ForEach(productGroup =>
      {

        var productGroupLanguages = productGroupRepo.GetListOfProductGroupLanguagesByProductGroupID(productGroup.ProductGroupID);
        var vendorProductGroups = productGroupRepo.GetListOfMappedVendorProductGroupsByProductGroupID(productGroup.ProductGroupID);

        if (productGroupLanguages.Count > 0 && vendorProductGroups.Count > 0)
        {
          MasterGroupMapping masterGroupMapping = new MasterGroupMapping()
          {
            ProductGroupID = productGroup.ProductGroupID,
            Score = scoreCounter
          };

          if (vendorProductGroups.Where(x => x.BrandCode == null).Count() > 0)
          {
            masterGroupMapping.ParentMasterGroupMappingID = productGroupID;
          }
          else
          {
            masterGroupMapping.ParentMasterGroupMappingID = brandProductGroupID;
          }

          int masterGroupMappingID = masterGroupMappingRepo.InsertMasterGroupMapping(masterGroupMapping);

          productGroupLanguages.ForEach(language =>
          {
            MasterGroupMappingLanguage masterGroupMappingLanguage = new MasterGroupMappingLanguage()
            {
              MasterGroupMappingID = masterGroupMappingID,
              LanguageID = language.LanguageID,
              Name = language.Name
            };

            masterGroupMappingRepo.InsertMasterGroupMappingLanguage(masterGroupMappingLanguage);
          });

          vendorProductGroups.ForEach(vendorProductGroup =>
          {
            masterGroupMappingRepo.MapVendorProductGroupToMasterGroupMapping(vendorProductGroup.ProductGroupVendorID, masterGroupMappingID);
          });

          scoreCounter++;
        }
      });
    }

    public void ImportProductGroupMappings()
    {

      // Connector Blueberry 5480
      // Connector Dixons 5468
      // Connector WPOS MyCom Webconnector 5474
      // Connector WPOS MyCom Shopconnector 5478

      log.DebugFormat("");

      log.DebugFormat("-------> Start Importing ProductGroupMappings");
      var connectors = connectorRepo
        .GetListOfActiveConnectors()
        .Where(x => x.ConnectorID == 5474)
        ;
      connectors.ForEach(connector =>
      {
        log.DebugFormat("---> Connector: {0}", connector.Name);

        ImportProductGroupMappings(connector);
        SyncSourceMasterGroupMapping(connector);
        //FillExportIDColumn(connector);
      });
      log.DebugFormat("-------> End Importing ProductGroupMappings");
    }

    private void ImportProductGroupMappings(Connector connector)
    {
      log.DebugFormat("Importing Product Group Mappings");
      masterGroupMappingRepo.ImportProductGroupMappingPerConnector(connector.ConnectorID);
    }
    private void SyncSourceMasterGroupMapping(Connector connector)
    {
      List<MasterGroupMapping> listOfProductGroups = masterGroupMappingRepo.GetListOfProductGroupsByConnector(connector.ConnectorID);
      log.DebugFormat("Syncing {0} Source Master Group Mappings", listOfProductGroups.Count);

      listOfProductGroups.ForEach(productGroup =>
      {
        List<MasterGroupMapping> masterGroupMappings = masterGroupMappingRepo.GetListOfMasterGroupMappingsByProductGroupID(productGroup.ProductGroupID);
        if (masterGroupMappings.Count > 0)
        {
          productGroup.SourceMasterGroupMappingID = masterGroupMappings.First().MasterGroupMappingID;
          masterGroupMappingRepo.UpdateMasterGroupMapping(productGroup);
        }
      });
    }
    private void FillExportIDColumn(Connector connector)
    {
      List<MasterGroupMapping> listOfProductGroups = masterGroupMappingRepo.GetListOfProductGroupsByConnector(connector.ConnectorID);
      List<ProductGroupMapping> listOfProductGroupMappings = productGroupMappingRepo.GetListOfProductGroupMappingByConnector(connector.ConnectorID);
      log.DebugFormat("Syncing {0} ExportIDs in Master Group Mapping", listOfProductGroups.Count);

      listOfProductGroups.ForEach(productGroup =>
      {
        List<ProductGroupMapping> productGroupMappings = listOfProductGroupMappings.Where(x => x.MasterGroupMappingID == productGroup.MasterGroupMappingID).ToList();
        if (productGroupMappings.Count == 1)
        {
          productGroup.ExportID = productGroupMappings.First().ProductGroupMappingID;
          masterGroupMappingRepo.UpdateMasterGroupMapping(productGroup);
        }
        else
        {
          log.DebugFormat("Error: The system cannot find correct ProductGroupMappingID for Master Group MappingID: {0}", productGroup.MasterGroupMappingID);
        }
      });
    }
    private void SyncMagentoProductGroupSetting(Connector connector)
    {
      List<MagentoProductGroupSetting> listOfMagentoSetting = magentoSettingRepo.GetListOfMagentoSettingByConnector(connector.ConnectorID);
      List<ProductGroupMapping> listOfProductGroupMapping = productGroupMappingRepo.GetListOfProductGroupMappingByConnector(connector.ConnectorID);

      listOfMagentoSetting.ForEach(magentoSetting => {
        
        ProductGroupMapping productGroupMapping = listOfProductGroupMapping
          .Where(x => x.ProductGroupMappingID == magentoSetting.ProductGroupmappingID)
          .FirstOrDefault();

        if (productGroupMapping != null)
        {
          magentoSetting.MasterGroupMappingID = productGroupMapping.MasterGroupMappingID;

        }
        else
        {
          log.DebugFormat("Error: ");
        }
      });

      // get magento setting per connector
      // get product group mappings
      
      // foreach magento setting
        // find mastergroupmappingid
        // set id
    }
  }
}
