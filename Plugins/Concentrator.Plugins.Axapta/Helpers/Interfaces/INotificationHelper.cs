using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.Axapta.Helpers
{
  public interface INotificationHelper
  {
    bool IsValidSalesNotification(IEnumerable<OrderNotification> listOfTransferOrderLines, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false);

    bool IsValidTransferNotification(IEnumerable<OrderNotification> listOfSalesOrderLines, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false);
    bool IsAllSubTransferOrdersAreShipped(IEnumerable<OrderNotification> listOfSalesOrderLines, IEnumerable<Order> listOfCurrenctTransferOrders, out List<DatColErrorMessage> listOfErrors, bool returnErrorLog = false);
  }
}
