using System;
using System.Globalization;
using System.Linq;
using Concentrator.Objects.Models;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects;
using Concentrator.Plugins.Magento.Helpers;
using Concentrator.Plugins.Magento.Exporters;
using Concentrator.Objects.Models.Configuration;
using System.IO;
using System.Collections.Generic;
using Concentrator.Objects.Constant;

namespace Concentrator.Plugins.Magento
{
  [ConnectorSystem(Constants.Connectors.ConnectorSystems.Magento)]
  public class MagentoExportReloaded : MagentoBasePlugin
  {
    public override string Name
    {
      get { return "Magento Export Plugin"; }
    }

    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    protected override void Process()
    {
      SBLicenseManager.TElSBLicenseManager m = new SBLicenseManager.TElSBLicenseManager();
      m.LicenseKey = "8F1317CD48AC4E68BABA5E339D8B365414D7ADA0289CA037E9074D29AD95FF3EC5D796BEFF0FBADB3BD82F48644C9EB810D9B5A305E0D2A1885C874D8BF974B9608CE918113FBE2AA5EEF8264C93B25ABEA98715DB4AD265F47CE02FC9952D69F2C3530B6ABAAA4C43B45E7EF6A8A0646DA038E34FBFB629C2BF0E83C6B348726E622EBD52CA05CF74C68F1279849CCD0C13EA673916BA42684015D658B8E7626F15BD826A4340EDB36CE55791A051FDBCF9FA1456C3B5008AD9990A0185C0EA3B19F9938CB7DA1FE82736ED4C7A566D4BFD53411E8380F4B020CB50E762520EFAE190836FD253B00DB18D4A485C7DC918AA4DCEC856331DD231CC8DC9C741C3";

      var connectorOverridesSetting = GetConfiguration().AppSettings.Settings["ConnectorOverrides"];
      List<int> connectorOverrides = null;
      if (connectorOverridesSetting != null)
      {
        connectorOverrides = (from p in connectorOverridesSetting.Value.Split(',') select int.Parse(p)).ToList();
      }

      _monitoring.Notify(Name, 0);
      using (var unit = GetUnitOfWork())
      {
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.WebAssortment)))
        {

          if (connectorOverrides != null && !connectorOverrides.Contains(connector.ConnectorID))
            continue;

#if DEBUG
          if (connector.ConnectorID != 5)
            continue;
#endif
          _monitoring.Notify(Name, connector.ConnectorID);


#if !DEBUG
          SetSoldenPeriod(connector);
#endif

          string singleProduct = string.Empty;
          try
          {
            log.DebugFormat("Start Magento Assortment Export for {0}", connector.Name);


            if (connector.ConnectorSystemType == null)
            {
              log.AuditError(string.Format("No Connector System Settings found for {}0, Magento Export can not be executed!", connector.Name), "Magento Export");
              continue;
            }

            DateTime start = DateTime.Now;

            string serializationPath = @"C:\Magento";//default
            var config = GetConfiguration();
            if (config.AppSettings.Settings["AssortmentSerializationPath"] != null)
            {
              serializationPath = config.AppSettings.Settings["AssortmentSerializationPath"].Value;
            }

            var connectorSerializationPath = serializationPath;
            if (!Directory.Exists(connectorSerializationPath))
            {
              Directory.CreateDirectory(connectorSerializationPath);
            }
          
#if DEBUG
            connector.Connection = "server=127.0.0.1;User Id=root;password=Phoh9ooLaing3FieZahb7if8;database=coolcat;Connect Timeout=30000;Default Command Timeout=30000;port=5014";
#endif

            MagentoExporter exporter = new MagentoExporter(connector, log, connectorSerializationPath);
            exporter.Execute();

            CustomerExporter cExporter = new CustomerExporter(connector, log, GetConfiguration());
            cExporter.Execute();


            log.DebugFormat("Finished Magento Export For {0}", connector.Name);
          }
          catch (OutOfMemoryException ex)
          {
            log.AuditCritical("Magento export -> Out of memory exception for " + connector.ConnectorID, ex, "Magento export");
            _monitoring.Notify(Name, -1);
          }
          catch (Exception ex)
          {
            log.AuditError("Error in Magento Plugin", ex);
            _monitoring.Notify(Name, -2);
          }

          var triggerIndex = connector.ConnectorSettings.GetValueByKey<bool>("TriggerIndexing", false);

          if (triggerIndex)
          {
            log.Info("Will place trigger for indexing");
            try
            {
              var info = SftpHelper.GetFtpTriggerIndexInfo(connector);

              IndexerHelper hlp = new IndexerHelper(info, "cache");
              hlp.CreateAssortmentTrigger();

              log.Info("Placed a trigger file");
            }
            catch (Exception e)
            {
              log.AuditError("Couldnt upload a trigger file", e, "Magento export");
              _monitoring.Notify(Name, -3);
            }
          }
        }
        _monitoring.Notify(Name, 1);
      }
    }

    /// <summary>
    /// Set Sale Period
    /// In Connector Setting moeten de volgende waardes aanwezig zijn
    /// WebsiteCodeInCoreTable: De code van de website in de Magento (Table: core_website)
    /// SoldenStartPeriod: Start datum (ex. 01-12-2013) format(day-month-year)
    /// SoldenEndPeriod: Eind datum (ex. 31-12-2013) format(day-month-year)
    /// </summary>
    /// <param name="connector"></param>
    private void SetSoldenPeriod(Connector connector)
    {
      const string settingKeyWebsiteCodeInMagentoCoreTable = "WebsiteCodeInCoreTable";

      using (var helper = new MagentoMySqlHelper(connector.Connection))
      {
        var coreWebsiteCode =
          connector.ConnectorSettings
          .Where(x => x.SettingKey == settingKeyWebsiteCodeInMagentoCoreTable)
          .Select(x => x.Value)
          .FirstOrDefault();

        if (coreWebsiteCode == null)
        {
          log.WarnFormat("Connector {0} does not have Setting Key WebsiteCodeInCoreTable", connector.Name);
        }
        else
        {
          bool isSolden;
          if (!GetSoldenPeriod(connector.ConnectorID, out isSolden))
            throw new Exception(string.Format("Solden period values for connector {0} are corrupt.", connector.Name));

          helper.InsertSoldenPeriod(isSolden, coreWebsiteCode);
        }
      }
    }

    protected bool GetSoldenPeriod(int connectorID, out bool soldenPeriod)
    {
      using (var unit = GetUnitOfWork())
      {
        soldenPeriod = false;

        var connectorSettings =
          unit.Scope.Repository<ConnectorSetting>()
              .GetAll(x => x.ConnectorID == connectorID)
              .ToDictionary(x => x.SettingKey, y => y.Value);

        if (!connectorSettings.ContainsKey("SoldenStartPeriod")) return false;
        if (!connectorSettings.ContainsKey("SoldenEndPeriod")) return false;

        DateTime soldenStartPeriod;
        DateTime soldenEndPeriod;

        if (!DateTime.TryParse(connectorSettings["SoldenStartPeriod"], new CultureInfo("nl-NL"), DateTimeStyles.None, out soldenStartPeriod)) return false;
        if (!DateTime.TryParse(connectorSettings["SoldenEndPeriod"], new CultureInfo("nl-NL"), DateTimeStyles.None, out soldenEndPeriod)) return false;

        if (DateTime.Now > soldenStartPeriod && DateTime.Now < soldenEndPeriod)
          soldenPeriod = true;

        return true;
      }
    }
  }

  public static class MySqlExtensions
  {
    public static string SafeGetString(this MySql.Data.MySqlClient.MySqlDataReader reader, string column)
    {
      if (reader.IsDBNull(reader.GetOrdinal(column)))
        return null;
      else
        return reader.GetString(column);
    }
  }
}
