using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using Concentrator.Plugins.Axapta.Models;

namespace Concentrator.Plugins.Axapta.Repositories
{
  public class OrderRepository : UnitOfWorkPlugin, IOrderRepository
  {
    #region "Order"

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
          foreach (var order in orders)
          {
            db.Scope.Repository<Order>().Add(order);
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

    public IEnumerable<Order> GetOrders(int connectorID, OrderTypes orderType)
    {
      using (var db = GetUnitOfWork())
      {
        var orders = db
          .Scope
          .Repository<Order>()
          .GetAll(x => x.ConnectorID == connectorID && x.OrderType == (int) orderType);
        return orders;
      }
    } 

    public IList<Order> GetOrdersForShipment(int connectorID)
    {
      using (var db = GetUnitOfWork())
      {
        var listOfOrderIds = db
          .Scope
          .Repository<Order>()
          .GetAll(x => x.ConnectorID == connectorID && x.OrderResponses.Any(o => o.OrderResponseLines.Any(r => r.html.Contains("WaitingForShipmentToAxapta"))))
          .ToList();

        return listOfOrderIds;
      }
    } 

    public IList<Order> GetOrderByContainsWebOrderNumber(string webOrderNumber)
    {
      using (var db = GetUnitOfWork())
      {
        var listOfOrders = db
          .Scope
          .Repository<Order>()
          .GetAll(x=>x.WebSiteOrderNumber.Contains(webOrderNumber))
          .ToList();

        return listOfOrders;
      }
    } 
     
    public IEnumerable<OrderNotification> GetListOfOrderWithNotification(int connectorID, string notification)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        var listOfOrders = db.Query<OrderNotification>(string.Format(@";
WITH  ShippedOrders
        AS ( SELECT DISTINCT
                    OrderID
             FROM   dbo.OrderLine ol
             INNER JOIN dbo.OrderResponseLine orl ON ol.OrderLineID = orl.OrderLineID
             WHERE html LIKE '%{0}%' AND Processed = 0
           )
  SELECT  o.OrderID
	,				Document
  ,       WebSiteOrderNumber
  ,       OrderType
  ,       ol.OrderLineID
  ,       Quantity
  ,       OriginalLine
	,				OrderResponseLineID
  ,       Ordered
  ,       Cancelled
  ,       Shipped
  ,       html AS Html
  FROM    dbo.[Order] o
  INNER JOIN dbo.OrderLine ol ON o.OrderID = ol.OrderID
  INNER JOIN ShippedOrders so ON o.OrderID = so.OrderID
  LEFT JOIN dbo.OrderResponseLine orl ON ol.OrderLineID = orl.OrderLineID AND Processed = 0
  WHERE ConnectorID = {1}
	ORDER BY o.OrderID
  ,       ol.OrderLineID", notification, connectorID));

        return listOfOrders;
      }
    } 

    public bool IsWebOrderNumberExist(string webOrderNumber, int connectorID, out int orderID)
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

    public bool IsPartialWebOrderNumberExist(string webOrderNumber, int connectorID, out int orderID)
    {
      orderID = 0;

      using (var db = GetUnitOfWork())
      {
        var orders = db
          .Scope
          .Repository<Order>()
          .GetAll(x => x.WebSiteOrderNumber.Contains(webOrderNumber) && x.ConnectorID == connectorID)
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

    #endregion

    #region "OrderLine"
    
    public Boolean InsertOrderLine(OrderLine orderLine)
    {
      using (var db = GetUnitOfWork())
      {
        try
        {
          db.Scope.Repository<OrderLine>().Add(orderLine);
          db.Save();

          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }

    public OrderLine GetOrderLineByID(int orderLineID)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<OrderLine>()
          .GetSingle(x => x.OrderLineID == orderLineID);
      }
    }

    public IEnumerable<OrderLine> GetOrderLinesToExport(int connectorID)
    {
      using (var db = GetUnitOfWork())
      {
        var orderlinesReady = db
          .Scope
          .Repository<OrderLine>()
          .GetAll(x => x.isDispatched &&
                      x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ReadyToOrder) &&
                      x.Order.PaymentTermsCode != "Shop" &&
                      x.Order.ConnectorID == connectorID &&
                      x.Order.OrderType == (int)OrderTypes.SalesOrder)
          .ToList();

        var orderlinesExport = db
          .Scope
          .Repository<OrderLine>()
          .GetAll(x => x.isDispatched &&
                      x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedExportNotification) &&
                      x.Order.PaymentTermsCode != "Shop" &&
                      x.Order.ConnectorID == connectorID &&
                      x.Order.OrderType == (int)OrderTypes.SalesOrder)
          .ToList();

        return orderlinesReady.Except(orderlinesExport).ToList();
      }
    }

    public IEnumerable<OrderLine> GetOrderLinesToReturn(int connectorID)
    {
      using (var db = GetUnitOfWork())
      {
        var returnOrderlinesReady = db
          .Scope
          .Repository<OrderLine>()
          .GetAll(x => x.isDispatched && 
            x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedReturnNotification) &&
            x.Order.ConnectorID == connectorID &&
            x.Order.OrderType == (int)OrderTypes.SalesOrder)
          .ToList();

        var returnOrderlinesExport = db
          .Scope
          .Repository<OrderLine>()
          .GetAll(x => x.isDispatched && 
            x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedReturnExportNotification) &&
            x.Order.ConnectorID == connectorID &&
            x.Order.OrderType == (int)OrderTypes.SalesOrder)
          .ToList();

        return returnOrderlinesReady.Except(returnOrderlinesExport).ToList();
      }
    }

    public List<OrderLine> GetOrderLinesToCancel(int connectorID)
    {
      using (var db = GetUnitOfWork())
      {
        var cancelLinesReady = db
          .Scope
          .Repository<OrderLine>()
          .GetAll(x => x.isDispatched && 
            x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.Cancelled) &&
            x.Order.ConnectorID == connectorID &&
            x.Order.OrderType == (int)OrderTypes.SalesOrder)
          .ToList();

        var cancelLinesExported = db
          .Scope
          .Repository<OrderLine>()
          .GetAll(x => x.isDispatched && 
            x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.ProcessedCancelExportNotification) &&
            x.Order.ConnectorID == connectorID &&
            x.Order.OrderType == (int)OrderTypes.SalesOrder)
          .ToList();

        return cancelLinesReady.Except(cancelLinesExported).ToList();
      }
    }

    public Boolean UpgradeOrderLineStatus(int orderLineID, OrderLineStatus status, int? quantity = null, bool useStatusOnNonAssortmentItems = false)
    {
      using (var db = GetUnitOfWork())
      {
        var orderLine = db
          .Scope
          .Repository<OrderLine>()
          .GetAll(x => x.OrderLineID == orderLineID)
          .FirstOrDefault();
        if (orderLine == null)
        {
          return false;
        }
        orderLine.SetStatus(status, db.Scope.Repository<OrderLedger>(), quantity, useStatusOnNonAssortmentItems);
        return true;
      }
    }

    public Boolean UpgradeOrderLinesStatus(Dictionary<int, int?> orderLines, OrderLineStatus status, bool useStatusOnNonAssortmentItems = false)
    {
      using (var db = GetUnitOfWork())
      {
        try
        {
          foreach (var orderLineID in orderLines)
          {
            var id = orderLineID;
            var orderLine = db
              .Scope
              .Repository<OrderLine>()
              .GetAll(x => x.OrderLineID == id.Key)
              .FirstOrDefault();
            if (orderLine == null)
            {
              return false;
            }
            orderLine.SetStatus(status, db.Scope.Repository<OrderLedger>(), orderLineID.Value, useStatusOnNonAssortmentItems);
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

    //todo: refactor this code
    public Boolean UpdateOriginalLineInOrderLine(OrderLine orderLine)
    {
      try
      {
        using (var db = GetUnitOfWork())
        {
          var currentOrderLine = db
            .Scope
            .Repository<OrderLine>()
            .GetSingle(x => x.OrderLineID == orderLine.OrderLineID);

          currentOrderLine.OriginalLine = orderLine.OriginalLine;
          db.Save();
        }
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    public Boolean UpdateOriginalLinesInOrderLine(IEnumerable<OrderLine> orderLines)
    {
      try
      {
        using (var db = GetUnitOfWork())
        {
          foreach (var orderLine in orderLines)
          {
            var currentOrderLine = db
              .Scope
              .Repository<OrderLine>()
              .GetSingle(x => x.OrderLineID == orderLine.OrderLineID);

            currentOrderLine.OriginalLine = orderLine.OriginalLine;            
          }
          db.Save();
        }
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
    #endregion

    #region "OrderResponse"

    public OrderResponse GetOrderResponseByID(int orderResponseID)
    {
      using (var db = GetUnitOfWork())
      {
        return db
          .Scope
          .Repository<OrderResponse>()
          .GetSingle(x => x.OrderResponseID == orderResponseID);
      }
    }
    
    #endregion

    #region "Order Response Line"

    public bool UpdateHtmlOfOrderResponseLines(IEnumerable<Int32> listOfOrderResponseLineIDs, string html)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        try
        {
          var sqlQuery = string.Format(@"
            UPDATE dbo.OrderResponseLine
            SET html = '{0}'
            WHERE OrderResponseLineID IN ({1})
          "
            , html.Replace("'", "''")
            , string.Join(",", listOfOrderResponseLineIDs));
          
          db.Execute(sqlQuery);

          return true;
        }
        catch
        {
          return false;
        }
      }
    }

    public Boolean SetHtmlForOrderResponseLines(IEnumerable<OrderResponseLine> orderResponseLines, string html)
    {
      using (var db = new PetaPoco.Database(Connection, "System.Data.SqlClient"))
      {
        try
        {
          foreach (var orderResponseLine in orderResponseLines)
          {
            db.Execute(string.Format(@"
              UPDATE dbo.OrderResponseLine
              SET html = '{0}'
              WHERE OrderResponseLineID = {1}
            ", html, orderResponseLine.OrderResponseLineID));            
          }
          return true;
        }
        catch (Exception)
        {
          return false;
        }
      }
    }

    #endregion
  }
}
