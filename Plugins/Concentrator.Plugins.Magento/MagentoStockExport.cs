﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Plugins.Magento.Exporters;
using Concentrator.Plugins.Magento.Helpers;

namespace Concentrator.Plugins.Magento
{
  [ConnectorSystem(2)]
  public class MagentoStockExport : MagentoBasePlugin
  {
    public override string Name
    {
      get { return "Magento stock export plugin"; }
    }


    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();

    protected override void Process()
    {

      SBLicenseManager.TElSBLicenseManager m = new SBLicenseManager.TElSBLicenseManager();
      m.LicenseKey = "8F1317CD48AC4E68BABA5E339D8B365414D7ADA0289CA037E9074D29AD95FF3EC5D796BEFF0FBADB3BD82F48644C9EB810D9B5A305E0D2A1885C874D8BF974B9608CE918113FBE2AA5EEF8264C93B25ABEA98715DB4AD265F47CE02FC9952D69F2C3530B6ABAAA4C43B45E7EF6A8A0646DA038E34FBFB629C2BF0E83C6B348726E622EBD52CA05CF74C68F1279849CCD0C13EA673916BA42684015D658B8E7626F15BD826A4340EDB36CE55791A051FDBCF9FA1456C3B5008AD9990A0185C0EA3B19F9938CB7DA1FE82736ED4C7A566D4BFD53411E8380F4B020CB50E762520EFAE190836FD253B00DB18D4A485C7DC918AA4DCEC856331DD231CC8DC9C741C3";

      _monitoring.Notify(Name, 0);

      using (var unit = GetUnitOfWork())
      {
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.WebAssortment)))
        {
#if DEBUG
          if (connector.ConnectorID != 11) continue;
#endif
          _monitoring.Notify(Name, connector.ConnectorID);
          try
          {
            log.DebugFormat("Start Magento stock export for {0}", connector.Name);

            if (connector.ConnectorSystemType == null)
            {
              log.AuditError(string.Format("No Connector System Settings found for {0}, Magento stock export can not be executed!", connector.Name), "Magento Export");
              continue;
            }

            StockExporter exporter = new StockExporter(connector, log);
            exporter.Execute();
          }
          catch (Exception e)
          {
            log.Fatal("Error in magento stock export", e);
            _monitoring.Notify(Name, -1);
          }
          var triggerIndex = connector.ConnectorSettings.GetValueByKey<bool>("TriggerIndexing", false);

          if (triggerIndex)
          {
            try
            {
              MySqlIndexerHelper helper = new MySqlIndexerHelper(connector.Connection);
              helper.ReindexStock();
            }
            catch (Exception e)
            {
              log.Fatal("Error in reindexing stock", e);
              _monitoring.Notify(Name, -2);
            }
          }
        }
      }
      _monitoring.Notify(Name, 1);
    }
  }
}
