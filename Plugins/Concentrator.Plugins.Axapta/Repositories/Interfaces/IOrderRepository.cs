using Concentrator.Objects.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public interface IOrderRepository
  {
    Boolean InsertOrder(Order order);
    Boolean InsertOrders(IEnumerable<Order> orders);
    Order GetOrderByID(int orderID);
    IEnumerable<Order> GetOrders(int connectorID, OrderTypes orderType);
    IList<Order> GetOrdersForShipment(int connectorID);
    IList<Order> GetOrderByContainsWebOrderNumber(string webOrderNumber);
    IEnumerable<OrderNotification> GetListOfOrderWithNotification(int connectorID, string notification);
    bool IsWebOrderNumberExist(string webOrderNumber, int connectorID, out int orderID);
    bool IsPartialWebOrderNumberExist(string webOrderNumber, int connectorID, out int orderID);

    Boolean InsertOrderLine(OrderLine orderLine);
    OrderLine GetOrderLineByID(int orderLineID);
    IEnumerable<OrderLine> GetOrderLinesToExport(int connectorID);
    List<OrderLine> GetOrderLinesToCancel(int connectorID);
    IEnumerable<OrderLine> GetOrderLinesToReturn(int connectorID);
    Boolean UpgradeOrderLineStatus(int orderLineID, OrderLineStatus status, int? quantity = null, bool useStatusOnNonAssortmentItems = false);
    Boolean UpgradeOrderLinesStatus(Dictionary<int, int?> orderLines, OrderLineStatus status, bool useStatusOnNonAssortmentItems = false);
    Boolean UpdateOriginalLineInOrderLine(OrderLine orderLine);
    Boolean UpdateOriginalLinesInOrderLine(IEnumerable<OrderLine> orderLines);

    OrderResponse GetOrderResponseByID(int orderResponseID);

    bool UpdateHtmlOfOrderResponseLines(IEnumerable<Int32> listOfOrderResponseLineIDs, string html);
    Boolean SetHtmlForOrderResponseLines(IEnumerable<OrderResponseLine> orderResponseLines, string html);
  }
}
