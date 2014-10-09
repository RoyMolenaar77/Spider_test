using System.Linq;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Connectors;
using System.Collections.Generic;
using Concentrator.Objects.Models.Magento;
using System;
using Concentrator.Plugins.PFA.ConcentratorRepos;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Objects.Models.MastergroupMapping;

namespace Concentrator.Plugins.PFA
{
  public class GetTheLookRelated : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Create get the look sets"; }
    }

    private const string ENABLE_GET_THE_LOOK_SETTING = "EnableGetTheLook";

    protected override void Process()
    {
      var config = GetConfiguration();
      int concentratorVendorID = GetConcentratorVendorID(config);
      var coolcatVendorID = GetCoolcatVendorID(config);

      using (var unit = GetUnitOfWork())
      {
        var connectors = GetConnectors(unit.Scope.Repository<Connector>());
        var getTheLookRepo = new GetTheLookRepository(Connection);

        int groupID = int.Parse(config.AppSettings.Settings["LookGroupID"].Value);
        int getTheLookGroupID = int.Parse(config.AppSettings.Settings["GetTheLookGroupID"].Value);

        var cpRepo = unit.Scope.Repository<ContentProductGroup>();
        var mgmRepo = unit.Scope.Repository<MasterGroupMapping>();
        var contentRepo = unit.Scope.Repository<Content>();
        var magentoRepo = unit.Scope.Repository<MagentoProductGroupSetting>();
        var mgmProductsRepo = unit.Scope.Repository<MasterGroupMappingProduct>();

        log.Info("Retrieving matches");
        var gr = getTheLookRepo.GetMatchedLookGroups(PfaCoolcatConfiguration.Current.TargetGroupAttributeID, PfaCoolcatConfiguration.Current.InputCodeAttributeID, PfaCoolcatConfiguration.Current.SeasonAttributeID, 1);
        log.Info("Found " + gr.Count + " matches");
        foreach (var connector in connectors.ToList())
        {
          log.Info("Start processing connector " + connector.Name);

          GetTheLookProcessor processor = new GetTheLookProcessor(
              cpRepo,
              mgmRepo,
              contentRepo,
              magentoRepo,mgmProductsRepo,
              connector,
              groupID,
              getTheLookGroupID,
              concentratorVendorID,
              coolcatVendorID,
              gr
              
          );

          processor.Process();
          unit.Save();
          log.Info("Finished processing connector " + connector.Name);

        }
}
    }

    private List<Connector> GetConnectors(Concentrator.Objects.DataAccess.Repository.IRepository<Connector> repository)
    {
      return repository.GetAll().ToList().Where(c => c.ConnectorSettings.GetValueByKey<bool>(ENABLE_GET_THE_LOOK_SETTING, false)).ToList();
    }

    private int GetConcentratorVendorID(System.Configuration.Configuration config)
    {
      var setting = config.AppSettings.Settings["ConcentratorVendorID"];

      setting.ThrowIfNull(e: new InvalidOperationException("Concentrator Vendor ID is not specified in the configuration."));

      return int.Parse(setting.Value);
    }

    private int GetCoolcatVendorID(System.Configuration.Configuration config)
    {
      var setting = config.AppSettings.Settings["ccVendorID"];

      setting.ThrowIfNull(e: new InvalidOperationException("CC Vendor ID is not specified in the configuration."));

      return int.Parse(setting.Value);
    }
  }
}
