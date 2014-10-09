using System;
using System.Globalization;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class StockReturnHelper
  {
    internal static int GetOrderIDByOrderLineID(int orderLineID)
    {
      var sql = string.Format("SELECT OrderID FROM OrderLine WHERE OrderLineID = {0}", orderLineID);

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        return database.ExecuteScalar<int>(sql);
      }
    }

    internal static bool OrderLineExistForProductAndOrderType(int orderType, int productID)
    {
      var sql = string.Format("SELECT COUNT(*) FROM OrderLine ol INNER JOIN [Order] o on o.OrderID = ol.OrderID LEFT JOIN OrderLedger old on ol.OrderLineID = old.OrderLineID AND old.Status = 160 WHERE o.OrderType = {0} AND ol.ProductID = {1} AND o.IsDispatched = 1 AND ol.IsDispatched = 1 AND old.OrderLineID IS NULL", orderType, productID);

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        var count = database.ExecuteScalar<int>(sql);
        return count > 0;
      }
    }

    internal static int GetLastOrderLineIDByOrderTypeAndProductID(int orderType, int productID)
    {
      var sql = string.Format("SELECT TOP 1 ol.OrderlineID FROM OrderLine ol INNER JOIN [Order] o on o.OrderID = ol.OrderID WHERE o.OrderType = {0} AND ol.ProductID = {1} AND o.IsDispatched = 1 AND ol.IsDispatched = 1 ORDER BY o.OrderID DESC, ol.OrderLineID DESC", orderType, productID);

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        return database.ExecuteScalar<int>(sql);
      }
    }

    internal static int CreateOrderRowForWehkampReturns(int connectorID, string websiteOrderNumber, string fileName, DateTime receivedDate)
    {
      const string insert = "INSERT INTO [Order] ([Document], [ConnectorID], [IsDispatched], [ReceivedDate], [IsDropShipment], [WebsiteOrderNumber], [HoldOrder], [OrderType]) OUTPUT Inserted.OrderID VALUES ";
      var sql = string.Format("{0} ('{1}', {2}, {3}, '{4}', {5}, '{6}', {7}, {8})", insert, fileName, connectorID, 1, receivedDate.ToString("yyyy-MM-dd HH:mm:ss"), 0, websiteOrderNumber, 0, 5);
      int orderID;

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {

        database.CommandTimeout = 5 * 60;
        orderID = database.ExecuteScalar<int>(sql);
      }

      return orderID;
    }

    internal static int CreateOrderLineRow(int orderID, int productID, int vendorID, string defaultWarehouseCode = null)
    {
      const string insert = "INSERT INTO [dbo].[OrderLine] ([OrderID],[ProductID],[Quantity],[isDispatched],[DispatchedToVendorID], [WarehouseCode]) OUTPUT Inserted.OrderLineID VALUES ";
      var sql = string.Format("{0} ({1}, {2}, {3}, {4}, {5}, '{6}')", insert, orderID, productID, 0, 1, vendorID, defaultWarehouseCode);
      int orderLineID;

      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.CommandTimeout = 5 * 60;
        orderLineID = database.ExecuteScalar<int>(sql);
      }

      return orderLineID;
    }

    internal static void CreateOrderLedgerRow(int orderLineID, int quantity, DateTime messageTime)
    {
      var sql = string.Format("INSERT INTO OrderLedger (OrderLineID, Status, LedgerDate, Quantity) VALUES ({0}, {1}, '{3}', {2})", orderLineID, (int)OrderLineStatus.StockReturnRequestConfirmation, quantity, messageTime.ToString("yyyy-MM-dd HH:mm:ss"));
      using (var database = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        database.Execute(sql);
      }
    }
  }
}
