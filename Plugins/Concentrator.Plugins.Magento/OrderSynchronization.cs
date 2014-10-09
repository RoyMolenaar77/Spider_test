using System;
using System.Linq;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Plugins.Magento.Exporters;

namespace Concentrator.Plugins.Magento
{
  [ConnectorSystem(2)]
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

      _monitoring.Notify(Name, 0);
      using (var unit = GetUnitOfWork())
      {
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.OrderHandling)))
        {

#if DEBUG
          if (connector.ConnectorID != 7)
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
