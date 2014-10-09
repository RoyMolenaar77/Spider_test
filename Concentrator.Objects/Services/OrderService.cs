using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Services.Base;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Ordering.Rules;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Services
{
  public class OrderService : Service<Order>, IOrderService
  {
    #region IOrderService Members

    public int DispatchOrderLine(int orderLineID, IUnitOfWork work)
    {
      var orderLine = Repository<OrderLine>().GetSingle(c => c.OrderLineID == orderLineID);

      if (orderLine.isDispatched) throw new InvalidOperationException("Order line is already dispatched");

      var pipeline = new RulePipeline(new List<OrderLine>() { orderLine }, work);
      pipeline.Dispatch(true);

      if (pipeline.Exceptions.Count > 0)
        throw new Exception("Dispatching order line " + orderLineID + " failed.See logs for more information");

      return pipeline.VendorOrderLines.First().Key;
    }

    #endregion

    #region IOrderService Members


    public void QueueForDispatch(int orderID, int vendorID)
    {
      Repository<OrderLine>().GetAll(ol => ol.OrderID == orderID && !ol.DispatchedToVendorID.HasValue).ForEach((line, idx) =>
      {
        line.DispatchedToVendorID = vendorID;
      });
    }

    #endregion

    #region IOrderService Members


    public bool IsOrderRuleValueValid(ConnectorRuleValue ruleValue)
    {
      var points = (from m in Repository<ConnectorRuleValue>().GetAllAsQueryable()
                    where m.VendorID == ruleValue.VendorID && m.ConnectorID == ruleValue.ConnectorID
                    select m).ToList().Sum(c => c.Value);

      return !(points >= 100 || points + ruleValue.Value > 100);
    }

    #endregion
  }
}
