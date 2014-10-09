using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using AuditLog4Net.Adapter;
using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Plugins.PFA
{
  using Configuration;

  /// <summary>
  /// Represents the shipment notification importer from TNT Fashion.
  /// </summary>
  public class ShipmentNotificationImporter : TNTImporter
  {
    protected override Regex FileNameRegex
    {
      get
      {
        return new Regex(@"^shipment_notification", RegexOptions.IgnoreCase);
      }
    }

    protected override string ValidationFileName
    {
      get
      {
        return TNTFashionSection.Current.ShipmentNotification != null
          ? TNTFashionSection.Current.ShipmentNotification.ValidationFileName
          : null;
      }
    }

    protected override bool Process(String file, Vendor vendor)
    {
      var document = Documents[file];
      var vendorProcessesAxaptaMessages = vendor.VendorSettings.GetValueByKey<bool>("UseAxapta", false);

      var elementGroups =
        from element in document.XPathSelectElements("shipment_notifications/shipment_notification")
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

        List<OrderResponseLine> shippedResponseLines = new List<OrderResponseLine>();
        List<OrderResponseLine> cancelledResponseLines = new List<OrderResponseLine>();
        List<OrderResponseLine> axaptaResponseLines = new List<OrderResponseLine>();

        foreach (var element in elementGroup)
        {
          var trackingNumber = (String)element.XPathEvaluate("string(tracking_number/text())");

          if (String.IsNullOrWhiteSpace(trackingNumber))
          {
            Log.Warn("Tracking number is missing.");
          }

          var sku = GetSku(element, true).Replace(" ", "");
          var product = Unit.Scope.Repository<Product>().GetSingle(p => p.VendorItemNumber.Replace(" ", "") == sku);

          if (product == null)
          {
            var message = String.Format("No product found with vendor item number '{0}'.", sku);

            Log.AuditError(message);

            throw new Exception(message);
          }

          var ordered = Convert.ToInt32(element.XPathEvaluate("number(ordered/text())"));
          var shipped = Convert.ToInt32(element.XPathEvaluate("number(shipped/text())"));
          var cancelled = ordered - shipped;

          OrderLine orderLine;

          if (element.Element("WebSiteOrderLineNumber") != null)
          {
            var orderLineID = Convert.ToInt32(element.XPathEvaluate("number(WebSiteOrderLineNumber/text())"));
            orderLine = order.OrderLines.FirstOrDefault(x => x.OrderLineID == orderLineID && x.Product == product && x.Quantity == ordered);
          }
          else
          {
            orderLine = order.OrderLines.FirstOrDefault(row => row.Product == product && row.Quantity == ordered);
          }


          if (orderLine == null)
          {
            var message = String.Format("No order line found with product '{0}' and quantity {1}.", sku, ordered);

            Log.AuditError(message);

            throw new Exception(message);
          }

          if (shipped > 0)
          {
            shippedResponseLines.Add(ConstructOrderResponseLine(orderLine, 0, ordered, shipped, trackingNumber, vendorProcessesAxaptaMessages));
          }
          if (cancelled > 0)
          {
            cancelledResponseLines.Add(ConstructOrderResponseLine(orderLine, cancelled, ordered, 0, trackingNumber, vendorProcessesAxaptaMessages));
          }
        }

        if (cancelledResponseLines.Count > 0)
        {
          order.OrderResponses.Add(ConstructOrderResponse(cancelledResponseLines, order, document, OrderResponseTypes.CancelNotification));
        }

        if (shippedResponseLines.Count > 0)
        {
          order.OrderResponses.Add(ConstructOrderResponse(shippedResponseLines, order, document, OrderResponseTypes.ShipmentNotification));
        }
      }

      Unit.Save();
      return true;
    }

    private OrderResponseLine ConstructOrderResponseLine(OrderLine orderLine, int cancelled, int ordered, int shipped, string trackAndTraceNumber = null, bool supportsAxaptaMessages = false)
    {
      return new OrderResponseLine
          {
            OrderLine = orderLine,
            Cancelled = cancelled,
            Shipped = shipped,
            Ordered = ordered,
            TrackAndTrace = trackAndTraceNumber,
            Delivered = cancelled,
            html = supportsAxaptaMessages ? string.Format("WaitingForShipmentToAxapta {0:dd-MM-yyyy H:mm:ss}", DateTime.Now) : string.Empty
          };
    }

    private OrderResponse ConstructOrderResponse(List<OrderResponseLine> responseLines, Order order, XDocument document, OrderResponseTypes type)
    {
      return new OrderResponse()
      {

        Order = order,
        OrderResponseLines = responseLines,
        ReceiveDate = DateTime.Now.ToUniversalTime(),
        ResponseType = type.ToString(),
        Vendor = Vendor,
        VendorDocument = document.ToString(),
        VendorDocumentDate = DateTime.Now.ToUniversalTime(),
        VendorDocumentReference = order.WebSiteOrderNumber,
        VendorDocumentNumber = order.WebSiteOrderNumber,
      };
    }

    public ShipmentNotificationImporter(Vendor vendor, IUnitOfWork unit, IAuditLogAdapter logAdapter)
      : base(vendor, unit, logAdapter)
    {
    }

    public object OrderReponseLine { get; set; }
  }
}
