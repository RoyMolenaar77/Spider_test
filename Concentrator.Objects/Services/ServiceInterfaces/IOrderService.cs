using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Services.ServiceInterfaces
{
  public interface IOrderService
  {
    /// <summary>
    /// Dispatches a singel order line. Returns the vendor id 
    /// </summary>
    /// <param name="orderLineID"></param>
    /// <param name="unit"></param>
    /// <returns>id of the vendor to which the order was dispatched to</returns>
    int DispatchOrderLine(int orderLineID, IUnitOfWork unit);

    /// <summary>
    /// Marks an order as ready for dispatch.Will be dispatched on next dispatch run
    /// </summary>
    /// <param name="orderID"></param>
    /// <param name="vendorID"></param>
    void QueueForDispatch(int orderID, int vendorID);

    /// <summary>
    /// Validates an order rule value for
    /// </summary>
    /// <param name="ruleValue"></param>
    /// <returns></returns>
    bool IsOrderRuleValueValid(ConnectorRuleValue ruleValue);
  }
}
