
using System;
using System.Collections.Generic;
using System.Linq;

namespace Concentrator.Tasks.Vlisco.Repositories
{
  using Objects.ConcentratorService;
  using Objects.Models.Orders;

  public class OrderRepository : UnitOfWorkPlugin, IOrderRepository
  {
    public Boolean InsertOrder(Order order)
    {
      using (var db = GetUnitOfWork())
      {
        try
        {
          db.Scope.Repository<Order>().Add(order);
          db.Save();

          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }

    public Boolean InsertOrders(IEnumerable<Order> orders)
    {
      using (var db = GetUnitOfWork())
      {
        try
        {
          var orderRepo = db.Scope.Repository<Order>();

          foreach (var order in orders)
          {
            orderRepo.Add(order);
          }

          db.Save();

          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }

    public Order GetOrderByID(int orderID)
    {
      using (var db = GetUnitOfWork())
      {
        var order = db
          .Scope
          .Repository<Order>()
          .GetSingle(x => x.OrderID == orderID);
        return order;
      }
    }

    public bool WebsiteOrderNumberAlreadyExists(string webOrderNumber, int connectorID, out int orderID)
    {
      orderID = 0;

      using (var db = GetUnitOfWork())
      {
        var orders = db
          .Scope
          .Repository<Order>()
          .GetAll(x => x.WebSiteOrderNumber == webOrderNumber && x.ConnectorID == connectorID)
          .ToList();

        if (orders.Any())
        {
          if (orders.Count == 1) 
            orderID = orders.First().OrderID;
          return true;
        }
        return false;
      }
    }
  }
}
