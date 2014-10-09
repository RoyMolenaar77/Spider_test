using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Xml.Linq;
using Concentrator.Objects.Models.Orders;
using System.Configuration;

namespace Concentrator.Objects.Ordering.Purchase
{
  public class ATPurchase : IPurchase
  {
    public bool PurchaseOrders(Models.Orders.Order order, List<Models.Orders.OrderLine> orderLines, Models.Vendors.Vendor administrativeVendor, Models.Vendors.Vendor vendor, bool directShipment, DataAccess.UnitOfWork.IUnitOfWork unit, log4net.ILog logger)
    {
      try
      {
        var webShopStockType = unit.Scope.Repository<VendorStockType>().GetSingle(x => x.StockType == "Webshop");

        if (webShopStockType == null)
          throw new Exception("Stocklocation Webshop does not exists, skip order process");



        orderLines.Where(c => (!c.Product.IsNonAssortmentItem.HasValue || (c.Product.IsNonAssortmentItem.HasValue && !c.Product.IsNonAssortmentItem.Value))).ToList().ForEach(line =>
       {
         var webshopStock = line.Product.VendorStocks.FirstOrDefault(x => x.VendorStockTypeID == webShopStockType.VendorStockTypeID && x.VendorID == int.Parse(ConfigurationManager.AppSettings["ATVendorID"]));
         if (webshopStock == null)
           throw new Exception(string.Format("Stocklocation Webshop does not exists for product {0}, skip order process", line.ProductID));



         if (line.Quantity > webshopStock.QuantityOnHand)
         {
           //CANCEL LINE
           int qtyCancelled = Math.Abs(line.Quantity - Math.Abs(webshopStock.QuantityOnHand));
           line.SetStatus(OrderLineStatus.Cancelled, unit.Scope.Repository<OrderLedger>(), qtyCancelled);
           CancelLine(line, unit, qtyCancelled, "Out of stock");
         }
         else
         {
           webshopStock.QuantityOnHand -= line.GetDispatchQuantity();
           line.SetStatus(OrderLineStatus.ReadyToOrder, unit.Scope.Repository<OrderLedger>());
         }
       });
      }
      catch (Exception e)
      {
        logger.Debug(e);
      }

      unit.Save();

      return false;
    }

    public void PurchaseConfirmation(Concentrator.Web.Objects.EDI.Purchase.PurchaseConfirmation purchaseConfirmation, Models.Vendors.Vendor administrativeVendor, List<Models.Orders.OrderLine> orderLines)
    {
      return;
    }

    public void InvoiceMessage(Concentrator.Web.Objects.EDI.InvoiceMessage invoiceMessage, Models.Vendors.Vendor administrativeVendor, List<Models.Orders.OrderLine> orderLines)
    {
      return;
    }

    public void OrderChange(Concentrator.Web.Objects.EDI.ChangeOrder.ChangeOrderRequest changeOrderRequest, Models.Vendors.Vendor administrativeVendor, List<Models.Orders.OrderLine> orderLines)
    {
      return;
    }

    private void CancelLine(Concentrator.Objects.Models.Orders.OrderLine line, IUnitOfWork unit, int quantity, string reason)
    {
      string vendorDocument = reason;

      OrderResponse orderResponse = new OrderResponse()
      {
        OrderID = line.OrderID,
        Currency = "EUR",
        ReceiveDate = DateTime.Now.ToUniversalTime(),
        ReqDeliveryDate = DateTime.Now.ToUniversalTime(),
        ResponseType = OrderResponseTypes.CancelNotification.ToString(),
        VendorID = int.Parse(ConfigurationManager.AppSettings["ATVendorID"]),
        VendorDocumentNumber = "Concentrator",
        VendorDocumentDate = DateTime.Now.ToUniversalTime(),
        VendorDocument = vendorDocument,
        DocumentName = string.Empty
      };
      unit.Scope.Repository<OrderResponse>().Add(orderResponse);

      OrderResponseLine orderResponseLine = new OrderResponseLine()
      {
        Backordered = 0,
        Cancelled = quantity,
        Ordered = line.Quantity,
        Shipped = 0,
        Delivered = quantity,
        VendorItemNumber = line.Product.VendorItemNumber,
        VendorLineNumber = "0",
        Price = line.Price.HasValue ? decimal.Parse(line.Price.Value.ToString()) : 0,
        Processed = false,
        Remark = reason,
        OrderLineID = line.OrderLineID,
        OrderResponse = orderResponse
      };

      unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);

      unit.Save();
    }
  }
}
