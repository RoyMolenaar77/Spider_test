using System;
using System.Linq;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Plugins.Magento.Exporters;
using System.Collections;
using System.Collections.Generic;
using Concentrator.Objects.Constant;

namespace Concentrator.Plugins.Magento
{
  [ConnectorSystem(Constants.Connectors.ConnectorSystems.Magento)]
  public class OrderSynchronization : MagentoBasePlugin
  {
    public override string Name
    {
      get { return "Magento Order Synchronization Plugin"; }
    }

    private readonly Monitoring.Monitoring _monitoring = new Monitoring.Monitoring();
    private Action<int> notifyMonitor;


    protected override void Process()
    {

      var connectorOverridesSetting = GetConfiguration().AppSettings.Settings["OrderConnectorOverrides"];
      List<int> connectorOverrides = null;
      if (connectorOverridesSetting != null)
      {
        connectorOverrides = (from p in connectorOverridesSetting.Value.Split(',') select int.Parse(p)).ToList();
      }


      _monitoring.Notify(Name, 0);
      using (var unit = GetUnitOfWork())
      {

        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.OrderHandling)))
        {
          Enumerable.Range(0, 100);

          if (connectorOverrides != null && !connectorOverrides.Contains(connector.ConnectorID))
            continue;

#if DEBUG
          if (connector.ConnectorID != 15)
            continue;
#endif
          notifyMonitor = (c) =>
          {
            _monitoring.Notify(Name, c);
          };
          string singleProduct = string.Empty;
          try
          {
            log.DebugFormat("Start Magento Order Synchronization for {0}", connector.Name);


            if (connector.ConnectorSystemType == null)
            {
              log.AuditError(string.Format("No Connector System Settings found for {0}, Magento Export can not be executed!", connector.Name), "Magento Export");
              continue;
            }

            DateTime start = DateTime.Now;

            OrderExporter orderExporter = new OrderExporter(connector, log, GetConfiguration(), notifyMonitor);
            orderExporter.Process();

            log.DebugFormat("Finished Magento Order Synchronization For {0}", connector.Name);
          }
          catch (Exception ex)
          {
            log.Error("Error in Magento Plugin", ex);
            _monitoring.Notify(Name, -1);
          }
        }
      }
      _monitoring.Notify(Name, 1);
    }
  }
}
