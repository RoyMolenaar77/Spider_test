using System;
using System.Globalization;
using Concentrator.Objects.Environments;
using PetaPoco;

namespace Concentrator.Plugins.Wehkamp.Helpers
{
  internal static class SalesOrderHelper
  {
    internal static int CreateSalesOrderAndReturnOrderID(SalesOrderObject order, Database db)
    {
      const string insert = "INSERT INTO [Order] ([Document],[ConnectorID],[IsDispatched],[ReceivedDate],[isDropShipment],[WebSiteOrderNumber],[HoldOrder],[OrderType],[PaymentTermsCode]) OUTPUT Inserted.OrderID VALUES ";
      var sql = string.Format("{0} ('{1}', {2}, {3}, '{4}', {5}, '{6}', {7}, {8}, '{9}')", insert, order.Document, order.ConnectorID, order.IsDispatched, order.ReceivedDate.ToString(MessageHelper.ISO8601, CultureInfo.InvariantCulture), order.IsDropShipment, order.WebsiteOrderNumber, order.HoldOrder, order.OrderType, "Wehkamp");

      return db.ExecuteScalar<int>(sql);
    }

    internal static int CreateSalesOrderLineAndReturnOrderLineID(SalesOrderLineObject orderLine, int vendorID, bool isSalesOrder, DateTime orderDateTime, Database db)
    {
      //OrderLine
      var priceQuery = string.Format("SELECT vp.Price AS Price FROM VendorPrice vp INNER JOIN VendorAssortment va ON vp.VendorAssortmentID = va.VendorAssortmentID WHERE va.ProductID = {0} AND va.VendorID = {1}", orderLine.ProductID, vendorID);
      const string insert = "INSERT INTO [dbo].[OrderLine] ([OrderID],[ProductID],[Price],[Quantity],[isDispatched],[UnitPrice],[LineDiscount],[BasePrice]) OUTPUT Inserted.OrderLineID VALUES ";
      var sql = string.Format("{0} ({1}, {2}, {3}, {4}, {5}, {6}, {7}, ({8}))",
        insert,
        orderLine.SalesOrderID,                                                             // OrderID
        orderLine.ProductID,                                                                // ProductID
        String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", orderLine.Price),         // Price
        orderLine.Quantity,                                                                 // Quantity
        orderLine.IsDispatched,                                                             // IsDispatched
        String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", orderLine.UnitPrice),     // UnitPrice
        String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", orderLine.LineDiscount),  // LineDiscount
        String.Format(CultureInfo.InvariantCulture, "{0:0.0000}", priceQuery));             // BasePrice

      db.CommandTimeout = 30;
      var orderLineID = db.ExecuteScalar<int>(sql);

      //OrderLedger
      var sqlOrderLedger = isSalesOrder ?
        string.Format("INSERT INTO OrderLedger (OrderLineID, Status, LedgerDate) VALUES ({0}, {1}, getdate())", orderLineID, (int)OrderLineStatus.ReadyToOrder) :
        string.Format("INSERT INTO OrderLedger (OrderLineID, Status, LedgerDate, Quantity) VALUES ({0}, {1}, '{2}', {3})", orderLineID, (int)OrderLineStatus.ProcessedReturnNotification, orderDateTime.ToString(MessageHelper.ISO8601, CultureInfo.InvariantCulture), orderLine.Quantity);

      db.Execute(sqlOrderLedger);

      return orderLineID;
    }
  }
}
