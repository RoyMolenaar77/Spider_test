using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI;
using System.Configuration;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Enumerations;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.EDI
{
  public class ProcessEdiOrders : ConcentratorEDIPlugin
  {
    public override string Name
    {
      get { return "EDI Order process"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var orders = (from o in unit.Scope.Repository<EdiOrderLine>().GetAll()
                      where o.EdiOrder.Status == (int)EdiOrderStatus.ProcessOrder
                      select o.EdiOrder).ToList();

        foreach (var order in orders)
        {
          try
          {
            var connectorRelation = unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.ConnectorRelationID == order.ConnectorRelationID);

            if (connectorRelation != null)
            {
              ediProcessor.ProcessOrderToVendor(order, connectorRelation, Config, unit);
              order.Status = (int)EdiOrderStatus.WaitForOrderResponse;
              order.EdiOrderLines.ForEach((x, idx) =>
              {
                x.SetStatus(EdiOrderStatus.WaitForAcknowledgement, unit);
              });
              order.IsDispatched = true;
            }
            unit.Save();
          }
          catch (Exception ex)
          {
            order.EdiOrderListener.ErrorMessage = string.Format("Validation failed: {0}", ex.Message);
            log.AuditError(string.Format("Validation failed for order {0}", order.EdiOrderID), ex, "Validation EDI orders");
          }
        }
      }
    }

    public override System.Configuration.Configuration Config
    {
      get { return GetConfiguration(); }
    }

  }
}
