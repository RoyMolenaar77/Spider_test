using Concentrator.Objects.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Tasks.Vlisco.Repositories
{
  interface IOrderRepository
  {
    Boolean InsertOrder(Order order);
    Boolean InsertOrders(IEnumerable<Order> orders);
    Order GetOrderByID(int orderID);
    bool WebsiteOrderNumberAlreadyExists(string webOrderNumber, int connectorID, out int orderID);  
  }
}
