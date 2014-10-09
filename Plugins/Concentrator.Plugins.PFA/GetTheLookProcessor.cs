using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Magento;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.PFA.ConcentratorRepos;
using Concentrator.Plugins.PFA.ConcentratorRepos.Models;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Plugins.PFA
{
  public class GetTheLookProcessor
  {
    private IRepository<ContentProductGroup> _repoContentProductGroup;
    private IRepository<MasterGroupMapping> _repoMasterGroupMapping;
    private IRepository<Content> _repoContent;
    private IRepository<MagentoProductGroupSetting> _repoMagentoSetting;
    private IRepository<MasterGroupMappingProduct> _repoMasterGroupMappingProduct;

    private Connector _connector;
    private int _groupID;
    private int _getTheLookGroupID;
    private int _concentratorVendorID;
    private int _coolcatVendorID;
    private List<LookGroup> _groups;
    public GetTheLookProcessor
      (
        IRepository<ContentProductGroup> repoContentProductGroup,
        IRepository<MasterGroupMapping> repoMasterGroupMapping,
        IRepository<Content> repoContent,
        IRepository<MagentoProductGroupSetting> repoMagentoSetting,
        IRepository<MasterGroupMappingProduct> repoMasterGroupMappingProduct,
        Connector connector,
        int groupID,
        int getTheLookGroupID,
        int concentratorVendorID,
        int coolcatVendorID,
      List<LookGroup> groups
      )
    {
      _repoContentProductGroup = repoContentProductGroup;
      _repoMasterGroupMapping = repoMasterGroupMapping;
      _repoContent = repoContent;
      _repoMagentoSetting = repoMagentoSetting;
      _repoMasterGroupMappingProduct = repoMasterGroupMappingProduct;
      _groups = groups;
      _connector = connector;
      _groupID = groupID;
      _getTheLookGroupID = getTheLookGroupID;
      _concentratorVendorID = concentratorVendorID;
      _coolcatVendorID = coolcatVendorID;
    }

    public void Process()
    {
      var connectorID = (_connector.ParentConnectorID.HasValue ? _connector.ParentConnectorID.Value : _connector.ConnectorID);
      var score = 0;

      Dictionary<int, bool> existingCPGs = _repoContentProductGroup.GetAll(c => c.ConnectorID == _connector.ConnectorID && c.IsCustom && c.MasterGroupMappingID != null).Select(c => c.ContentProductGroupID).Select(c => new { ContentProductGroupID = c, Active = false }).ToDictionary(c => c.ContentProductGroupID, c => c.Active);
      List<string> allMasterGroupMappingBackendLabels = _repoMasterGroupMapping.GetAll(c => c.ConnectorID == _connector.ConnectorID && c.BackendMatchingLabel != null).Select(c => c.BackendMatchingLabel).ToList().Where(c => !string.IsNullOrEmpty(c)).ToList();

      foreach (var grp in _groups)
      {
        var parentMasterGroupMapping = _repoMasterGroupMapping.GetAll(c => c.ConnectorID == connectorID && c.ProductGroupID == _getTheLookGroupID).Where(c => c.MasterGroupMappingParent.ProductGroup.ProductGroupVendors.FirstOrDefault(l => l.VendorID == 1).VendorProductGroupCode1 == grp.TargetGroup).FirstOrDefault();

        if (parentMasterGroupMapping == null) continue;

        var label = grp.BackendLabel;
        var mapping = _repoMasterGroupMapping.GetSingle(c => c.ProductGroupID == _groupID && c.BackendMatchingLabel == label && c.ParentMasterGroupMappingID == parentMasterGroupMapping.MasterGroupMappingID);

        allMasterGroupMappingBackendLabels.Remove(label);

        if (mapping == null)
        {
          mapping = new MasterGroupMapping()
          {
            CustomProductGroupLabel = label,
            ProductGroupID = _groupID,
            ConnectorID = connectorID,
            MasterGroupMappingParent = parentMasterGroupMapping,
            ParentMasterGroupMappingID = parentMasterGroupMapping.MasterGroupMappingID,
            BackendMatchingLabel = label,
            Score = score,
            MasterGroupMappingProducts = new List<MasterGroupMappingProduct>()
          };

          var magentoProductGroupSetting = new MagentoProductGroupSetting()
          {
            MasterGroupMapping = mapping,
            DisabledMenu = true,
            ShowInMenu = true
          };

          _repoMasterGroupMapping.Add(mapping);
          _repoMagentoSetting.Add(magentoProductGroupSetting);
        }

        foreach (var prod in grp.Products)
        {
          var activeCPG = _repoContentProductGroup.GetSingle(c =>
                          c.ProductID == prod &&
                          c.ConnectorID == _connector.ConnectorID &&
                          c.IsCustom &&
                          c.MasterGroupMapping.BackendMatchingLabel == mapping.BackendMatchingLabel &&
                          c.MasterGroupMapping.ParentMasterGroupMappingID == mapping.ParentMasterGroupMappingID);

          if (activeCPG == null)
          {
            var content = _repoContent.GetSingle(c => c.ProductID == prod && c.ConnectorID == _connector.ConnectorID);
            if (content != null)
            {
              activeCPG = new ContentProductGroup()
              {
                ProductID = prod,
                ConnectorID = _connector.ConnectorID,
                IsCustom = true,
                MasterGroupMapping = mapping,
                ProductGroupMappingID = 933 //TODO: for now hardcoded, remove when total migration is complete
              };
              _repoContentProductGroup.Add(activeCPG);

              var masterGroupMappingProduct = mapping.MasterGroupMappingProducts.FirstOrDefault(x => x.ProductID == prod);

              if (masterGroupMappingProduct == null)
              {
                masterGroupMappingProduct = new MasterGroupMappingProduct()
                {
                  ProductID = prod,
                  IsCustom = true
                };

                mapping.MasterGroupMappingProducts.Add(masterGroupMappingProduct);
              }
            }
          }
          else
          {
            existingCPGs[activeCPG.ContentProductGroupID] = true;
          }
        }
        score++;

      }

      foreach (var cp in existingCPGs.Where(c => !c.Value))
      {
        var contentProductGroup = _repoContentProductGroup.GetSingle(c => c.ContentProductGroupID == cp.Key);
        if (contentProductGroup.CreatedBy == 1) ///quick fix
        {
          _repoContentProductGroup.Delete(contentProductGroup);
        }
      }
    }
  }
}
