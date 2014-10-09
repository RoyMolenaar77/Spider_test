using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.MastergroupMapping;
using Concentrator.Plugins.ConnectorProductSync.Helpers;
using Concentrator.Plugins.ConnectorProductSync.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Plugins.ConnectorProductSync.Services
{
  public class SyncProductGroupMappingService : ISyncProductGroupMappingService
  {

    private ILogging log;
    private IGenerateUpdateProperties generateIgnoreList;
    private IMasterGroupMappingRepository masterGroupMappingRepo;
    private IMagentoProductGroupSettingRepository magentoSettingRepo;

    public SyncProductGroupMappingService(
      ILogging log,
      IGenerateUpdateProperties generateIgnoreList,
      IMasterGroupMappingRepository masterGroupMappingRepo,
      IMagentoProductGroupSettingRepository magentoSettingRepo
      )
    {
      this.log = log;
      this.generateIgnoreList = generateIgnoreList;
      this.magentoSettingRepo = magentoSettingRepo;
      this.masterGroupMappingRepo = masterGroupMappingRepo;
    }

    public List<MasterGroupMapping> GetListOfProductGroupMapping(Connector connector)
    {
      List<MasterGroupMapping> listOfProductGroupMappings = masterGroupMappingRepo.GetListOfProductGroupsByConnector(connector.ConnectorID);
      return listOfProductGroupMappings;
    }

    public void SyncChildConnectorMapping(List<MasterGroupMapping> parentProductGroupMappings, Connector childConnector)
    {
      syncProductGroupMapping(parentProductGroupMappings, null, null, childConnector);
    }

    private void syncProductGroupMapping(List<MasterGroupMapping> listOfSourceProductGroupMappings, int? sourceParentProductGroupMappingID, int? destParentProductGroupMappingID, Connector destConnector)
    {
      List<MasterGroupMapping> listOfSourceProductGroupMappingsByParentID = new List<MasterGroupMapping>();
      List<MasterGroupMapping> listOfDestProductGroupMappingsByParentID = new List<MasterGroupMapping>();

      if (sourceParentProductGroupMappingID.HasValue && sourceParentProductGroupMappingID.Value > 0)
      {
        listOfSourceProductGroupMappingsByParentID = listOfSourceProductGroupMappings
          .Where(x => x.ParentMasterGroupMappingID == sourceParentProductGroupMappingID.Value)
          .ToList();
        listOfDestProductGroupMappingsByParentID = masterGroupMappingRepo
          .GetListOfProductGroupsByConnector(destConnector.ConnectorID)
          .Where(x => x.SourceProductGroupMappingID != null && x.ParentMasterGroupMappingID == destParentProductGroupMappingID)
          .ToList();
      }
      else
      {
        listOfSourceProductGroupMappingsByParentID = listOfSourceProductGroupMappings
          .Where(x => x.ParentMasterGroupMappingID == null)
          .ToList();
        listOfDestProductGroupMappingsByParentID = masterGroupMappingRepo
          .GetListOfProductGroupsByConnector(destConnector.ConnectorID)
          .Where(x => x.ParentMasterGroupMappingID == null && x.SourceProductGroupMappingID != null)
          .ToList();
      }

      List<MasterGroupMapping> listOfProductGroupMappingsToInsert =
        (
          from parentPGM in listOfSourceProductGroupMappingsByParentID
          join childPGM in listOfDestProductGroupMappingsByParentID on parentPGM.MasterGroupMappingID equals childPGM.SourceProductGroupMappingID into mappedPGM
          from pgm in mappedPGM.DefaultIfEmpty()
          where pgm == null
          select parentPGM
        ).ToList();

      List<MasterGroupMapping> listOfProductGroupMappingsToUpdate =
        (
          from parentPGM in listOfSourceProductGroupMappingsByParentID
          join childPGM in listOfDestProductGroupMappingsByParentID on parentPGM.MasterGroupMappingID equals childPGM.SourceProductGroupMappingID
          where 
            parentPGM.Score != childPGM.Score || 
            parentPGM.FlattenHierarchy != childPGM.FlattenHierarchy || 
            parentPGM.FilterByParentGroup != childPGM.FilterByParentGroup ||
            parentPGM.ExportID != childPGM.ExportID
          select childPGM
        ).ToList();

      List<MasterGroupMapping> listOfProductGroupMappingsToDelete =
        (
          from childPGM in listOfDestProductGroupMappingsByParentID
          join parentPGM in listOfSourceProductGroupMappingsByParentID on childPGM.SourceProductGroupMappingID equals parentPGM.MasterGroupMappingID into mappedPGM
          from pgm in mappedPGM.DefaultIfEmpty()
          where pgm == null
          select childPGM
        ).ToList();

      if (listOfProductGroupMappingsToInsert.Count > 0)
        InsertProductGroupMappingToDestConnector(listOfProductGroupMappingsToInsert, destConnector, (destParentProductGroupMappingID.HasValue && destParentProductGroupMappingID.Value > 0) ? destParentProductGroupMappingID : null);

      if (listOfProductGroupMappingsToUpdate.Count > 0)
        UpdateProductGroupMappingInDestConnector(listOfProductGroupMappingsToUpdate, listOfSourceProductGroupMappingsByParentID);

      if (listOfProductGroupMappingsToDelete.Count > 0)
        DeleteProductGroupMappingFromDestConnector(listOfProductGroupMappingsToDelete);

      if (listOfProductGroupMappingsToInsert.Count + listOfProductGroupMappingsToDelete.Count > 0)
      {
        if (sourceParentProductGroupMappingID.HasValue && sourceParentProductGroupMappingID.Value > 0)
        {
          listOfDestProductGroupMappingsByParentID = masterGroupMappingRepo
            .GetListOfProductGroupsByConnector(destConnector.ConnectorID)
            .Where(x => x.SourceProductGroupMappingID != null && x.ParentMasterGroupMappingID == destParentProductGroupMappingID)
            .ToList();
        }
        else
        {
          listOfDestProductGroupMappingsByParentID = masterGroupMappingRepo
            .GetListOfProductGroupsByConnector(destConnector.ConnectorID)
            .Where(x => x.ParentMasterGroupMappingID == null && x.SourceProductGroupMappingID != null)
            .ToList();
        }
      }
      listOfSourceProductGroupMappingsByParentID.ForEach(productGroupMapping => {
        if (!productGroupMapping.ParentMasterGroupMappingID.HasValue)
        {
          string productGroupMappingName = masterGroupMappingRepo.GetListOfMasterGroupMappingLanguagesByMasterGroupMappingID(productGroupMapping.MasterGroupMappingID).Where(x=>x.LanguageID == 2).Select(x=>x.Name).FirstOrDefault();
          log.DebugFormat("Sync Product Group Mapping {0}({1})", productGroupMappingName, productGroupMapping.MasterGroupMappingID);
        }

        int countProductGroupMappingChildren = listOfSourceProductGroupMappings
          .Where(x => x.ParentMasterGroupMappingID == productGroupMapping.MasterGroupMappingID)
          .Count();

        if (countProductGroupMappingChildren > 0)
        {
          MasterGroupMapping childProductGroupMapping = listOfDestProductGroupMappingsByParentID.Single(x => x.SourceProductGroupMappingID == productGroupMapping.MasterGroupMappingID);
          if (childProductGroupMapping != null)
          {
            syncProductGroupMapping(listOfSourceProductGroupMappings, productGroupMapping.MasterGroupMappingID, childProductGroupMapping.MasterGroupMappingID, destConnector);
          }
          else
          {
            log.DebugFormat("ProductGroupMapping with SourceProductGroupMappingID {0} in Connector {1} should exist! But i cant find it.", productGroupMapping.MasterGroupMappingID, destConnector);
          }
        }
      });
    }

    private void InsertProductGroupMappingToDestConnector(List<MasterGroupMapping> listOfProductGroupMappingToInsert, Connector connector, int? parentProductGroupMappingID)
    {
      listOfProductGroupMappingToInsert.ForEach(productGroupMapping =>
      {
        productGroupMapping.SourceProductGroupMappingID = productGroupMapping.MasterGroupMappingID;
        productGroupMapping.ConnectorID = connector.ConnectorID;
        if (parentProductGroupMappingID.HasValue && parentProductGroupMappingID.Value > 0)
          productGroupMapping.ParentMasterGroupMappingID = parentProductGroupMappingID.Value;
        int newProductGroupMappingID = masterGroupMappingRepo.InsertMasterGroupMapping(productGroupMapping);

        CopyProductGroupMappingLanguage(productGroupMapping.MasterGroupMappingID, newProductGroupMappingID);
        if (connector.ConnectorSystemID == 2)
        {
          CopyMagentoProductGroupSetting(productGroupMapping.MasterGroupMappingID, newProductGroupMappingID);
        }
      });
    }

    private void UpdateProductGroupMappingInDestConnector(List<MasterGroupMapping> listOfProductGroupMappingToUpdate, List<MasterGroupMapping> listOfParentProductGroupMapping)
    {
      List<string> ignoreList = generateIgnoreList.GenerateIgnoreProperties(new MasterGroupMapping(),
        x => x.MasterGroupMappingID,
        x => x.ConnectorID,
        x => x.ParentMasterGroupMappingID,
        x => x.SourceProductGroupMappingID);
      listOfProductGroupMappingToUpdate.ForEach(productGroupMapping =>
      {
        MasterGroupMapping compareProductGroupMapping = listOfParentProductGroupMapping.SingleOrDefault(x => x.MasterGroupMappingID == productGroupMapping.SourceProductGroupMappingID);
        if (compareProductGroupMapping != null)
        {
          masterGroupMappingRepo.UpdateMasterGroupMapping(productGroupMapping, compareProductGroupMapping, ignoreList);
        }        
      });
    }

    private void CopyProductGroupMappingLanguage(int fromProductGroupMappingID, int toProductGroupMappingID)
    {
      List<MasterGroupMappingLanguage> productGroupMappingLanguages = masterGroupMappingRepo.GetListOfMasterGroupMappingLanguagesByMasterGroupMappingID(fromProductGroupMappingID);
      productGroupMappingLanguages.ForEach(language =>
      {
        MasterGroupMappingLanguage masterGroupMappingLanguage = new MasterGroupMappingLanguage()
        {
          MasterGroupMappingID = toProductGroupMappingID,
          LanguageID = language.LanguageID,
          Name = language.Name
        };

        masterGroupMappingRepo.InsertMasterGroupMappingLanguage(masterGroupMappingLanguage);
      });
    }

    private void CopyMagentoProductGroupSetting(int fromProductGroupMappingID, int toProductGroupMappingID)
    {
      MagentoProductGroupSetting fromMagentoProductGroupSetting = magentoSettingRepo.GetMagentoSettingByMasterGroupMappingID(fromProductGroupMappingID);
      if (fromMagentoProductGroupSetting != null)
      {
        MagentoProductGroupSetting toMagentoProductGroupSetting = magentoSettingRepo.GetMagentoSettingByMasterGroupMappingID(toProductGroupMappingID);
        if (toMagentoProductGroupSetting == null)
        {
          MagentoProductGroupSetting newMagentoProductGroupSetting = new MagentoProductGroupSetting()
          {
            ProductGroupmappingID = fromMagentoProductGroupSetting.ProductGroupmappingID, //todo: remove this
            ShowInMenu = fromMagentoProductGroupSetting.ShowInMenu,
            DisabledMenu = fromMagentoProductGroupSetting.DisabledMenu,
            IsAnchor = fromMagentoProductGroupSetting.IsAnchor,
            MasterGroupMappingID = toProductGroupMappingID
          };
          magentoSettingRepo.InsertMagentoSetting(newMagentoProductGroupSetting);
        }
        else
        {
          if (fromMagentoProductGroupSetting.ShowInMenu != toMagentoProductGroupSetting.ShowInMenu ||
            fromMagentoProductGroupSetting.DisabledMenu != toMagentoProductGroupSetting.DisabledMenu ||
            fromMagentoProductGroupSetting.IsAnchor != toMagentoProductGroupSetting.IsAnchor
            )
          {
            List<string> listOfIngnoreProperties = generateIgnoreList.GenerateIgnoreProperties(new MagentoProductGroupSetting(), x => x.MagentoProductGroupSettingID, x => x.ProductGroupmappingID, x => x.CreatedBy, x => x.CreationTime, x => x.LastModifiedBy, x => x.LastModificationTime, x => x.MasterGroupMappingID);
            magentoSettingRepo.UpdateMagentoProductGroupSetting(toMagentoProductGroupSetting, fromMagentoProductGroupSetting, listOfIngnoreProperties);
          }
        }
      }
    }

    private void DeleteProductGroupMappingFromDestConnector(List<MasterGroupMapping> listOfProductGroupMappingToDelete)
    {
      listOfProductGroupMappingToDelete.ForEach(productGroupMapping =>
      {
        masterGroupMappingRepo.DeleteHierarchy(productGroupMapping);
      });
    }
  }
}
