using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using AuditLog4Net.Adapter;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.PFA
{
  using Configuration;

  /// <summary>
  /// Represents the received notification importer from TNT Fashion.
  /// </summary>
  public class ReceivedNotificationImporter : TNTImporter
  {
    protected override Regex FileNameRegex
    {
      get
      {
        return new Regex(@"^received_notification", RegexOptions.IgnoreCase);
      }
    }

    protected override string ValidationFileName
    {
      get
      {
       return string.Empty;
      }
    }

    protected override bool Process(String file, Vendor vendor)
    {
      var document = Documents[file];

      var elementGroups =
        from element in document.XPathSelectElements("received_notifications/received_notification")
        let websiteOrderNumber = (String)element.XPathEvaluate("string(website_order_number/text())")
        group element by websiteOrderNumber;

      foreach (var elementGroup in elementGroups)
      {
        var websiteOrderNumber = elementGroup.Key;

        if (String.IsNullOrWhiteSpace(websiteOrderNumber))
        {
          const string message = "Website order number is missing.";

          Log.AuditError(message);

          throw new Exception(message);
        }

        int connectorID = vendor.VendorSettings.FirstOrDefault(c => c.SettingKey == "RelatedConnectorID").Try(c => int.Parse(c.Value));

        var order = Unit.Scope.Repository<Order>().GetSingle(o => o.WebSiteOrderNumber == websiteOrderNumber && o.ConnectorID == connectorID);

        if (order == null)
        {

          return false;
        }

        List<OrderResponseLine> receivedResponseLines = new List<OrderResponseLine>();
        List<OrderResponseLine> cancelledResponseLines = new List<OrderResponseLine>();
        List<OrderResponseLine> axaptaResponseLines = new List<OrderResponseLine>();

        foreach (var element in elementGroup)
        {
          var trackingNumber = (String)element.XPathEvaluate("string(tracking_number/text())");

          var sku = GetSku(element, true).Replace(" ", "");

          var product = Unit.Scope.Repository<Product>().GetSingle(p => p.VendorItemNumber.Replace(" ", "") == sku);

          if (product == null)
          {
            var message = String.Format("No product found with vendor item number '{0}'.", sku);

            Log.AuditError(message);

            throw new Exception(message);
          }

          var ordered = Convert.ToInt32(element.XPathEvaluate("number(ordered/text())"));
          var received = Convert.ToInt32(element.XPathEvaluate("number(received/text())"));
          var cancelled = ordered - received;

          var orderLine = order.OrderLines.FirstOrDefault(row => row.Product == product && row.Quantity == ordered);

          if (orderLine == null)
          {
            var message = String.Format("No order line found with product '{0}' and quantity {1}.", sku, ordered);

            Log.AuditError(message);

            throw new Exception(message);
          }

          if (received > 0)
          {
            receivedResponseLines.Add(ConstructOrderResponseLine(orderLine, 0, ordered, received, trackingNumber));
            axaptaResponseLines.Add(ConstructAxaptaOrderResponseLine(orderLine, 0, ordered, received, trackingNumber));
          }
          if (cancelled > 0)
          {
            cancelledResponseLines.Add(ConstructOrderResponseLine(orderLine, cancelled, ordered, 0, trackingNumber));
            axaptaResponseLines.Add(ConstructAxaptaOrderResponseLine(orderLine, cancelled, ordered, 0, trackingNumber));
          }
        }

        if (cancelledResponseLines.Count > 0)
        {
          order.OrderResponses.Add(ConstructOrderResponse(cancelledResponseLines, order, document, OrderResponseTypes.CancelNotification));
        }

        if (receivedResponseLines.Count > 0)
        {
          order.OrderResponses.Add(ConstructOrderResponse(receivedResponseLines, order, document, OrderResponseTypes.ReceivedNotification));
        }

        SendToAxapta(order, OrderResponseTypes.ReceivedNotification, axaptaResponseLines);

        Unit.Save();

      }
      return true;
    }

    private OrderResponseLine ConstructOrderResponseLine(OrderLine orderLine, int cancelled, int ordered, int shipped, string trackAndTraceNumber = null)
    {
      return new OrderResponseLine
          {
            OrderLine = orderLine,
            Cancelled = cancelled,
            Shipped = shipped,
            Ordered = ordered,
            TrackAndTrace = trackAndTraceNumber,
            Delivered = cancelled

          };
    }

    private OrderResponseLine ConstructAxaptaOrderResponseLine(OrderLine orderLine, int cancelled, int ordered, int shipped, string trackAndTraceNumber = null)
    {
      return new OrderResponseLine
      {
        OrderLine = orderLine,
        Cancelled = cancelled,
        Shipped = shipped,
        Ordered = ordered,
        TrackAndTrace = trackAndTraceNumber,
        Delivered = cancelled,
        OrderLineID = orderLine.OrderLineID

      };
    }

    private OrderResponse ConstructOrderResponse(List<OrderResponseLine> responseLines, Order order, XDocument document, OrderResponseTypes type)
    {
      return new OrderResponse()
      {

        Order = order,
        OrderResponseLines = responseLines,
        ReceiveDate = DateTime.Now,
        ResponseType = type.ToString(),
        Vendor = Vendor,
        VendorDocument = document.ToString(),
        VendorDocumentDate = DateTime.Now,
        VendorDocumentReference = order.WebSiteOrderNumber,
        VendorDocumentNumber = order.WebSiteOrderNumber,
      };
    }

    public ReceivedNotificationImporter(Vendor vendor, IUnitOfWork unit, IAuditLogAdapter logAdapter)
      : base(vendor, unit, logAdapter)
    {
    }

    public object OrderReponseLine { get; set; }
  }
}
