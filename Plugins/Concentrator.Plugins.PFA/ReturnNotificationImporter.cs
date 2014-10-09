using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using AuditLog4Net.Adapter;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.PFA.Configuration;
using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Objects;

namespace Concentrator.Plugins.PFA
{
  /// <summary>
  /// Represents the return notification importer from TNT Fashion.
  /// </summary>
  public class ReturnNotificationImporter : TNTImporter
  {
    protected override Regex FileNameRegex
    {
      get
      {
        return new Regex(@"^return_notifications", RegexOptions.IgnoreCase);
      }
    }

    protected override string ValidationFileName
    {
      get
      {
        return TNTFashionSection.Current.ReturnNotification != null
          ? TNTFashionSection.Current.ReturnNotification.ValidationFileName
          : null;
      }
    }

    protected override bool Process(String file, Vendor vendor)
    {
      var document = Documents[file];

      var elementGroups =
        from element in document.XPathSelectElements("return_notifications/return_notification")
        let websiteOrderNumber = (String)element.XPathEvaluate("string(website_order_number/text())")
        group element by websiteOrderNumber;

      foreach (var elementGroup in elementGroups)
      {
        var websiteOrderNumber = elementGroup.Key;

        if (String.IsNullOrWhiteSpace(websiteOrderNumber))
        {
          var message = "Website_order_number is missing.";

          Log.AuditError(message);

          throw new Exception(message);

        }

        int connectorID = vendor.VendorSettings.FirstOrDefault(c => c.SettingKey == "RelatedConnectorID").Try(c => int.Parse(c.Value));

        var order = Unit.Scope.Repository<Order>().GetSingle(o => o.WebSiteOrderNumber == websiteOrderNumber && o.ConnectorID == connectorID);

        if (order == null) //no order for this connector. Leave message for next
        {
          return false;
        }

        var orderResponse = new OrderResponse
        {
          Order = order,
          OrderResponseLines = new List<OrderResponseLine>(),
          ReceiveDate = DateTime.Now.ToUniversalTime(),
          ResponseType = OrderResponseTypes.Return.ToString(),
          Vendor = Vendor,
          VendorDocument = document.ToString(),
          VendorDocumentDate = DateTime.Now.ToUniversalTime(),
          VendorDocumentReference = websiteOrderNumber,
          VendorDocumentNumber = websiteOrderNumber,
        };

        var isCompleteOrderReturnedInOne = IsTotalOrderReturnedAtOnce(elementGroup, order, vendor.Name);
        int currentComplaintCount = 0;
        foreach (var element in elementGroup)
        {
          var sku = GetSku(element, true).Replace(" ", "") ;
          var product = Unit.Scope.Repository<Product>().GetSingle(p => p.VendorItemNumber.Replace(" ", "") == sku);

          if (product == null)
          {
            var message = String.Format("No product found with vendor item number '{0}'.", sku);

            Log.AuditError(message);

            throw new Exception(message);

          }

          var returned = Convert.ToInt32(element.XPathEvaluate("number(returned/text())"));

          var vendorID = vendor.VendorID;
          int vendorOverrideID = vendor.VendorSettings.GetValueByKey<int>("VendorStockOverrideID", 0);
          VendorStock vendorStock = null;

          //Not good, rework
          if (vendorOverrideID != 0)
            vendorStock = product.VendorStocks.Single(vs => vs.VendorID == vendorOverrideID && vs.VendorStockType.StockType == "Webshop");
          else
            vendorStock = product.VendorStocks.Single(vs => vs.Vendor == Vendor && vs.VendorStockType.StockType == "Webshop");

          vendorStock.QuantityOnHand += returned;

          var complaint = Convert.ToBoolean(element.XPathEvaluate("string(complaint/text())"));
          if (complaint) currentComplaintCount++;

          var orderLine = order.OrderLines.FirstOrDefault(row => row.Product == product);

          if (orderLine == null)
          {
            var message = String.Format("No order line found with product '{0}'.", sku);

            Log.AuditError(message);

            throw new Exception(message);

          }

          orderResponse.OrderResponseLines.Add(new OrderResponseLine
          {
            OrderLine = orderLine,
            Ordered = orderLine.GetDispatchQuantity(),
            Delivered = returned,
            Remark = "Returned",
            Description = complaint ? "complaint" : null
          });

        }
        order.OrderResponses.Add(orderResponse);

        //check if total order is returned				
        var isWholeOrderReturned = TNTOrderHelper.IsTotalOrderReturned(order);
        var complaintCount = GetOrderComplaintCount(order);

        if (currentComplaintCount == 0)
        {
          //add a new line for the return costs
          orderResponse.OrderResponseLines.Add(AddReturnCosts(order));
        }

        if (isWholeOrderReturned)
        {
          bool useKialaShipmentCosts = !string.IsNullOrEmpty(order.ShippedToCustomer.ServicePointID);

          var line = order.OrderLines.FirstOrDefault(c => c.Product.VendorItemNumber == GetShipmentCostsProduct(order.ConnectorID, Unit, useKialaShipmentCosts));
          if (line != null)
          {
            //add shipment costs to returned collection
            orderResponse.OrderResponseLines.Add(new OrderResponseLine()
            {
              OrderLine = line,
              Delivered = 1,
              Remark = "Returned",
              Ordered = 1
            });
          }
        }


        Unit.Save();

      }
      return true;
    }

    private int GetOrderComplaintCount(Order order)
    {
      return order.OrderResponses.SelectMany(c => c.OrderResponseLines).Count(c => c.Description == "complaint");
    }

    private bool IsTotalOrderReturnedAtOnce(IGrouping<string, XElement> elementGroup, Order order, string vendorName)
    {
      return (from or in elementGroup
              let sku = GetSku(or, false)
              let qty = Convert.ToInt32(or.XPathEvaluate("number(returned/text())"))
              where order.OrderLines.FirstOrDefault(c => c.Product.VendorItemNumber == sku && c.Quantity == qty) != null
              select or).Count() == (order.OrderLines.Count - order.OrderLines.Where(c => c.Product.IsNonAssortmentItem ?? false).Count());
    }

    /// <summary>
    /// Add return costs to the orderline collection of the order
    /// If the orderline collection already contains them, ignore it and just add a return response
    /// </summary>
    private OrderResponseLine AddReturnCosts(Order order)
    {
      //check if orderline already exists

      bool useKialaReturnCosts = !string.IsNullOrEmpty(order.ShippedToCustomer.ServicePointID);
      string returnCosts = GetReturnCostsProduct(order.Connector, useKialaReturnCosts);
      var productReturnCosts = Unit.Scope.Repository<Product>().GetSingle(c => c.VendorItemNumber == returnCosts);

      if (productReturnCosts == null)
      {
        var message = "Return costs product is missing";

        Log.AuditError(message);

        throw new Exception(message);
      }

      var orderLine = order.OrderLines.FirstOrDefault(c => c.ProductID == productReturnCosts.ProductID);

      if (orderLine == null)
      {
        var vendorAssortmentReturnCosts = productReturnCosts.VendorAssortments.SingleOrDefault(va => va.Vendor == Vendor);

        if (vendorAssortmentReturnCosts == null)
        {
          var message = "Return costs vendor assortment is missing";

          Log.AuditError(message);

          throw new Exception(message);
        }

        var vendorPriceReturnCosts = vendorAssortmentReturnCosts.VendorPrices.Single(vp => vp.MinimumQuantity == 0);

        if (vendorPriceReturnCosts == null)
        {
          var message = "Return costs vendor price is missing";

          Log.AuditError(message);

          throw new Exception(message);
        }

        var returnCostsPrice = (Double)vendorPriceReturnCosts.Price.GetValueOrDefault();

        orderLine = new OrderLine()
        {
          Quantity = 1,
          Product = productReturnCosts,
          Price = returnCostsPrice,
          Order = order,
          BasePrice = returnCostsPrice,
          isDispatched = true
        };

        order.OrderLines.Add(orderLine);
      }

      return new OrderResponseLine()
      {
        OrderLine = orderLine,
        Delivered = 1,
        Remark = "Returned"
      };
    }

    public ReturnNotificationImporter(Vendor vendor, IUnitOfWork unit, IAuditLogAdapter logAdapter)
      : base(vendor, unit, logAdapter)
    {
    }
  }
}
