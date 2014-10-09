using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Configuration;
using System.Threading;
using System.Net;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Concentrator.Web.Objects.EDI;
using Concentrator.Web.Objects.EDI.Purchase;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Order.OrderDispatch
{
  public class ResponseListener : ConcentratorPlugin
  {
    HttpListener _httpListener = new HttpListener();
    ManualResetEvent _signalStop = new ManualResetEvent(false);
    private const int _vendorID = 1;

    public override string Name
    {
      get { return "Concentrator Listener started"; }
    }

    protected override void Process()
    {
      _httpListener.Prefixes.Clear();
      //Set prefix from Config file to listen to specific URI
      _httpListener.Prefixes.Add(GetConfiguration().AppSettings.Settings["ListenerPrefixes"].Value);
      _httpListener.Start();

      _httpListener.BeginGetContext(ListenerCallback, _httpListener);

      log.Info("HttpListener started succesfully");

      _signalStop.WaitOne();
    }

    void ListenerCallback(IAsyncResult result)
    {
      XmlDocument doc = new XmlDocument();
      HttpListenerContext context = null;
      try
      {
        //Get listener instance from async result
        HttpListener listener = (HttpListener)result.AsyncState;
        if (!listener.IsListening)
          return;

        context = listener.EndGetContext(result);
        log.Info("Response received");

        string responseString;
        using (StreamReader reader = new StreamReader(context.Request.InputStream))
        {
          responseString = reader.ReadToEnd();
        }

        string logPath = Path.Combine(GetConfiguration().AppSettings.Settings["XMLlogReceive"].Value, DateTime.Now.ToString("dd-MM-yyyy"));

        if (!Directory.Exists(logPath))
          Directory.CreateDirectory(logPath);

        string fileName = LogFile(logPath, responseString);

        try
        {
          XmlDocument xmlDoc = new XmlDocument();
          doc.LoadXml(responseString);
          ProcessResponse(doc, doc.DocumentElement.Name, fileName);
        }
        catch (Exception ex)
        {
          log.Error("Error processing response", ex);
        }

        context.Response.StatusCode = 200;
        using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
        {
          writer.WriteLine("Response received succesfully");
        }
        context.Response.Close();
      }
      catch (Exception ex)
      {
        log.Error("Writing of response to xml failed", ex);
      }
      finally
      {
        if (_httpListener != null && _httpListener.IsListening)
        {
          _httpListener.BeginGetContext(ListenerCallback, _httpListener);
        }
      }     

    }

    public static string LogFile(string path, string fileContents)
    {
      if (!path.EndsWith(@"\"))
        path += @"\";

      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      if (!path.EndsWith(@"\"))
        path += @"\";

      string fileName = "BAS_" + Guid.NewGuid() + ".xml";
      string filePath = Path.Combine(path, fileName);

      File.WriteAllText(filePath, fileContents);

      return fileName;
    }

    public T ExtractResponseMessage<T>(XmlDocument doc)
    {
      //// Get XML information
      XmlSerializer xs = new XmlSerializer(typeof(T));
      XmlNodeReader nodeReader = new XmlNodeReader(doc.DocumentElement);
      return (T)xs.Deserialize(nodeReader);
    }

    private Dictionary<string, decimal> _taxrates;

    public Dictionary<string, decimal> TaxRates
    {
      get
      {
        _taxrates = new Dictionary<string, decimal>();
        _taxrates.Add("NLH", 19);
        _taxrates.Add("NLL", 6);
        _taxrates.Add("BEL", 6);
        _taxrates.Add("BEH", 21);
        _taxrates.Add("BEM", 12);
        _taxrates.Add("NUL", 0);
        _taxrates.Add("VRY", -1);
        _taxrates.Add("UNKNOWN", 99);
        return _taxrates;
      }
    }

    private string BuildTrackAndTraceNumber(string trackAndTraceNumber, string zipCode)
    {
      return string.Format("http://www.postnlpakketten.nl/klantenservice/tracktrace/basicsearch.aspx?lang=nl&B={0}&P={1}", trackAndTraceNumber, zipCode);
    }


    private void ProcessResponse(XmlDocument responseString, string prefix, string fileName)
    {
      using (var unit = GetUnitOfWork())
      {
        object ediDocument = null;
        string websiteNumber;
        Concentrator.Objects.Models.Orders.Order order = null;
        int bskIdentifier = 0;
        Connector connector = null;

        switch (prefix)
        {
          case "OrderResponse":
            ediDocument = ExtractResponseMessage<Concentrator.Web.Objects.EDI.OrderResponse>(responseString);
            Concentrator.Web.Objects.EDI.OrderResponse ediOrderResponse = (Concentrator.Web.Objects.EDI.OrderResponse)ediDocument;

            bskIdentifier = ediOrderResponse.OrderHeader.BSKIdentifier;
            connector = unit.Scope.Repository<Connector>().GetSingle(x => x.BSKIdentifier == bskIdentifier);

            if (connector == null)
            {
              var bsk = bskIdentifier.ToString();
              connector = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetAllAsQueryable(x => x.BSKIdentifier == bsk  && x.WebSiteOrderNumber == ediOrderResponse.OrderHeader.WebSiteOrderNumber).Select(x => x.Connector).FirstOrDefault();

              if (connector == null)
              {
                log.WarnFormat("Process response failed for {0} message", prefix);
                return;
              }
            }

            websiteNumber = ediOrderResponse.OrderHeader.WebSiteOrderNumber;

            order = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetSingle(x => x.ConnectorID == connector.ConnectorID && x.WebSiteOrderNumber == websiteNumber);

            if (order != null)
            {
              if (order.SoldToCustomerID.HasValue && order.SoldToCustomer.EANIdentifier == "0"  
                && ediOrderResponse.OrderHeader.SoldToCustomer != null
                && !string.IsNullOrEmpty(ediOrderResponse.OrderHeader.SoldToCustomer.EanIdentifier))
              {
                order.SoldToCustomer.EANIdentifier = ediOrderResponse.OrderHeader.SoldToCustomer.EanIdentifier;
              }

              Concentrator.Objects.Models.Orders.OrderResponse orderResponse = new Concentrator.Objects.Models.Orders.OrderResponse()
              {
                OrderID = order.OrderID,
                Currency = "EUR",
                ReceiveDate = DateTime.Now,
                ReqDeliveryDate = ediOrderResponse.OrderHeader.RequestedDate.Year >= DateTime.Now.Year ? ediOrderResponse.OrderHeader.RequestedDate : DateTime.Now,
                ResponseType = OrderResponseTypes.Acknowledgement.ToString(),
                VendorID = _vendorID,
                VendorDocumentNumber = ediOrderResponse.OrderHeader.OrderNumber,
                VendorDocumentDate = DateTime.Now,
                VendorDocument = responseString.OuterXml,
                DocumentName = fileName
              };

              foreach (var line in ediOrderResponse.OrderDetails)
              {
                int orderLineID = 0;
                if (line.CustomerReference != null && !string.IsNullOrEmpty(line.CustomerReference.CustomerOrderLine))
                  orderLineID = int.Parse(line.CustomerReference.CustomerOrderLine);
                else if (order.OrderLines.Any(x => x.OrderLineID == line.LineNumber))
                {
                  orderLineID = line.LineNumber;
                }


                var orderLine = order.OrderLines.Where(x => x.OrderLineID == orderLineID).FirstOrDefault();

                if (orderLine == null)
                {
                  orderLineID = order.OrderLines.Where(x => x.CustomerItemNumber == line.ProductIdentifier.ProductNumber).Select(x => x.OrderLineID).FirstOrDefault();
                }


                if (orderLineID > 0)
                {
                  OrderResponseLine orderResponseLine = new OrderResponseLine()
                  {
                    Backordered = line.Quantity.QuantityBackordered,
                    Cancelled = line.Quantity.QuantityCancelled,
                    Ordered = line.Quantity.QuantityOrdered,
                    Shipped = line.Quantity.QuantityShipped,
                    Delivered = line.Quantity.QuantityShipped,
                    Unit = line.UnitOfMeasure.ToString(),
                    VendorItemNumber = line.ProductIdentifier.ProductNumber,
                    VendorLineNumber = line.LineNumber.ToString(),
                    Price = line.UnitPrice,
                    VatAmount = line.TaxAmount,
                    vatPercentage = TaxRates.ContainsKey(line.TaxRate.ToString()) ? TaxRates[line.TaxRate.ToString()] : 0,
                    Processed = false,
                    OrderLineID = orderLineID,
                    OEMNumber = line.ProductIdentifier.ManufacturerItemID,
                    Barcode = line.ProductIdentifier.EANIdentifier,
                    DeliveryDate = line.PromisedDeliveryDate,
                    RequestDate = line.RequestedDate,
                    OrderResponse = orderResponse,
                  };
                  unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);
                }
              }

              unit.Scope.Repository<Concentrator.Objects.Models.Orders.OrderResponse>().Add(orderResponse);
              unit.Save();
            }

            break;
          case "ShippingNotification":
            ediDocument = ExtractResponseMessage<ShippingNotification>(responseString);
            Concentrator.Web.Objects.EDI.ShippingNotification ediShipmentNotification = (Concentrator.Web.Objects.EDI.ShippingNotification)ediDocument;

            bskIdentifier = ediShipmentNotification.ShipmentOrderHeader.BSKIdentifier;
            connector = unit.Scope.Repository<Connector>().GetSingle(x => x.BSKIdentifier == bskIdentifier);

            if (connector == null)
            {
              var bsk = bskIdentifier.ToString();
              connector = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetAllAsQueryable(x => x.BSKIdentifier == bsk  && x.WebSiteOrderNumber == ediShipmentNotification.ShipmentOrderHeader.WebSiteOrderNumber).Select(x => x.Connector).FirstOrDefault();

              if (connector == null)
              {
                log.WarnFormat("Process response failed for {0} message", prefix);
                return;
              }
            }


            websiteNumber = ediShipmentNotification.ShipmentOrderHeader.WebSiteOrderNumber;

            order = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetSingle(x => x.ConnectorID == connector.ConnectorID && x.WebSiteOrderNumber == websiteNumber);

            if (order != null)
            {

              if (order.SoldToCustomerID.HasValue && order.SoldToCustomer.EANIdentifier == "0"
                && ediShipmentNotification.ShipmentOrderHeader.SoldToCustomer != null
                && !string.IsNullOrEmpty(ediShipmentNotification.ShipmentOrderHeader.SoldToCustomer.EanIdentifier))
              {
                order.SoldToCustomer.EANIdentifier = ediShipmentNotification.ShipmentOrderHeader.SoldToCustomer.EanIdentifier;
              }

              Concentrator.Objects.Models.Orders.OrderResponse shipOrderResponse = new Concentrator.Objects.Models.Orders.OrderResponse()
              {
                OrderID = order.OrderID,
                Currency = "EUR",
                ReceiveDate = DateTime.Now,
                ReqDeliveryDate = ediShipmentNotification.ShipmentOrderHeader.RequestedDate.Year >= DateTime.Now.Year ? ediShipmentNotification.ShipmentOrderHeader.RequestedDate : DateTime.Now,
                ResponseType = OrderResponseTypes.ShipmentNotification.ToString(),
                VendorID = _vendorID,
                VendorDocumentNumber = ediShipmentNotification.ShipmentOrderHeader.OrderNumber,
                ShippingNumber = ediShipmentNotification.ShipmentOrderHeader.PackingInformation.PackingNumber,
                TrackAndTrace = string.Join(",", ediShipmentNotification.ShipmentOrderDetails.Select(x => x.ShipmentInformation.TrackAndTraceNumber).ToArray()),
                VendorDocumentDate = DateTime.Now,
                VendorDocument = responseString.OuterXml,
                DocumentName = fileName
              };

              foreach (var line in ediShipmentNotification.ShipmentOrderDetails)
              {
                int orderLineID = 0;
                if (line.CustomerReference != null && line.CustomerReference.CustomerOrderLine != null)
                  orderLineID = int.Parse(line.CustomerReference.CustomerOrderLine);
                else if (!string.IsNullOrEmpty(line.LineNumber))
                {
                  if (order.OrderLines.Any(x => x.OrderLineID == int.Parse(line.LineNumber)))
                    orderLineID = int.Parse(line.LineNumber);
                }

                var orderLine = order.OrderLines.Where(x => x.OrderLineID == orderLineID).FirstOrDefault();

                if (orderLine == null)
                {
                  orderLineID = order.OrderLines.Where(x => x.CustomerItemNumber == line.ProductIdentifier.ProductNumber).Select(x => x.OrderLineID).FirstOrDefault();
                }

                if (orderLineID > 0)
                {
                  OrderResponseLine orderResponseLine = new OrderResponseLine()
                  {
                    Backordered = line.Quantity.QuantityBackordered,
                    Cancelled = line.Quantity.QuantityCancelled,
                    Ordered = line.Quantity.QuantityOrdered,
                    Shipped = line.Quantity.QuantityShipped,
                    Delivered = line.Quantity.QuantityShipped,
                    Unit = line.UnitOfMeasure.ToString(),
                    VendorItemNumber = line.ProductIdentifier.ProductNumber,
                    VendorLineNumber = line.LineNumber.ToString(),
                    Price = line.UnitPrice,
                    VatAmount = line.TaxAmount,
                    Processed = false,
                    OrderLineID = orderLineID,
                    OEMNumber = line.ProductIdentifier.ManufacturerItemID,
                    Barcode = line.ProductIdentifier.EANIdentifier,
                    DeliveryDate = line.PromissedDeliveryDate,
                    RequestDate = line.RequestedDate,
                    OrderResponse = shipOrderResponse,
                    NumberOfPallets = int.Parse(line.ShipmentInformation.NumberOfPallet),
                    NumberOfUnits = int.Parse(line.ShipmentInformation.NumberOfColli),
                    TrackAndTrace = line.ShipmentInformation.TrackAndTraceNumber,
                    TrackAndTraceLink = string.IsNullOrEmpty(line.ShipmentInformation.TrackAndTraceNumber) ? string.Empty : BuildTrackAndTraceNumber(line.ShipmentInformation.TrackAndTraceNumber, order.ShippedToCustomer.PostCode)

                  };
                  unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);
                }
              }

              unit.Scope.Repository<Concentrator.Objects.Models.Orders.OrderResponse>().Add(shipOrderResponse);
              unit.Save();
            }

            break;
          case "InvoiceNotification":
            ediDocument = ExtractResponseMessage<InvoiceNotification>(responseString);
            Concentrator.Web.Objects.EDI.InvoiceNotification ediInvoiceNotification = (Concentrator.Web.Objects.EDI.InvoiceNotification)ediDocument;

            bskIdentifier = ediInvoiceNotification.InvoiceOrderHeader.BSKIdentifier;
            connector = unit.Scope.Repository<Connector>().GetSingle(x => x.BSKIdentifier == bskIdentifier);

            if (connector == null)
            {
              var bsk = bskIdentifier.ToString();
              connector = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetAll(x => x.BSKIdentifier == bsk && x.WebSiteOrderNumber == ediInvoiceNotification.InvoiceOrderHeader.WebSiteOrderNumber).Select(x => x.Connector).FirstOrDefault();

              if (connector == null)
              {
                log.WarnFormat("Process response failed for {0} message", prefix);
                return;
              }
            }
            websiteNumber = ediInvoiceNotification.InvoiceOrderHeader.WebSiteOrderNumber;

            order = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetSingle(x => x.ConnectorID == connector.ConnectorID && x.WebSiteOrderNumber == websiteNumber);

            if (order != null)
            {

              Concentrator.Objects.Models.Orders.OrderResponse invoiceOrderResponse = new Concentrator.Objects.Models.Orders.OrderResponse()
              {
                OrderID = order.OrderID,
                Currency = "EUR",
                ReceiveDate = DateTime.Now,
                ReqDeliveryDate = ediInvoiceNotification.InvoiceOrderHeader.RequestedDate.Year >= DateTime.Now.Year ? ediInvoiceNotification.InvoiceOrderHeader.RequestedDate : DateTime.Now,
                ResponseType = OrderResponseTypes.InvoiceNotification.ToString(),
                VendorID = _vendorID,
                VendorDocumentNumber = ediInvoiceNotification.InvoiceOrderHeader.OrderNumber,
                ShippingNumber = ediInvoiceNotification.InvoiceOrderHeader.PackingInformation != null ? ediInvoiceNotification.InvoiceOrderHeader.PackingInformation.PackingNumber : null,
                TrackAndTrace = ediInvoiceNotification.InvoiceOrderHeader.ShipmentInformation != null ? ediInvoiceNotification.InvoiceOrderHeader.ShipmentInformation.TrackAndTraceNumber : null,
                VendorDocumentDate = DateTime.Now,
                VendorDocument = responseString.OuterXml,
                InvoiceDate = ediInvoiceNotification.InvoiceOrderHeader.InvoiceDate,
                InvoiceDocumentNumber = ediInvoiceNotification.InvoiceOrderHeader.InvoiceNumber,
                TotalAmount = decimal.Parse(ediInvoiceNotification.InvoiceOrderHeader.InvoiceTotalInc),
                TotalExVat = decimal.Parse(ediInvoiceNotification.InvoiceOrderHeader.InvoiceTotalInc) - decimal.Parse(ediInvoiceNotification.InvoiceOrderHeader.InvoiceTaxableAmount),
                VatAmount = decimal.Parse(ediInvoiceNotification.InvoiceOrderHeader.InvoiceTaxableAmount),
                VatPercentage = decimal.Parse(ediInvoiceNotification.InvoiceOrderHeader.InvoiceTax),
                DocumentName = fileName
              };

              foreach (var line in ediInvoiceNotification.InvoiceOrderDetails)
              {
                int orderLineID = 0;
                if (line.CustomerReference != null && !string.IsNullOrEmpty(line.CustomerReference.CustomerOrderLine))
                  orderLineID = int.Parse(line.CustomerReference.CustomerOrderLine);
                else if (!string.IsNullOrEmpty(line.LineNumber))
                {
                  if (order.OrderLines.Any(x => x.OrderLineID == int.Parse(line.LineNumber)))
                    orderLineID = int.Parse(line.LineNumber);
                }

                var orderLine = order.OrderLines.Where(x => x.OrderLineID == orderLineID).FirstOrDefault();

                if (orderLine == null)
                {
                  orderLineID = order.OrderLines.Where(x => x.CustomerItemNumber == line.ProductIdentifier.ProductNumber).Select(x => x.OrderLineID).FirstOrDefault();
                }

                if (orderLineID > 0)
                {
                  OrderResponseLine orderResponseLine = new OrderResponseLine()
                  {
                    Backordered = line.Quantity.QuantityBackordered,
                    Cancelled = line.Quantity.QuantityCancelled,
                    Ordered = line.Quantity.QuantityOrdered,
                    Shipped = line.Quantity.QuantityShipped,
                    Delivered = line.Quantity.QuantityShipped,
                    Unit = line.UnitOfMeasure.ToString(),
                    VendorItemNumber = line.ProductIdentifier.ProductNumber,
                    VendorLineNumber = line.LineNumber.ToString(),
                    Price = line.UnitPrice,
                    VatAmount = line.TaxAmount,
                    Processed = false,
                    OrderLineID = orderLineID,
                    OEMNumber = line.ProductIdentifier.ManufacturerItemID,
                    Barcode = line.ProductIdentifier.EANIdentifier,
                    DeliveryDate = line.PromissedDeliveryDate,
                    RequestDate = line.RequestedDate.Year >= DateTime.Now.Year ? line.RequestedDate : DateTime.Now,
                    OrderResponse = invoiceOrderResponse,
                    Invoiced = line.Quantity.QuantityOrdered
                  };

                  if (line.ShipmentInformation != null)
                    orderResponseLine.NumberOfPallets = int.Parse(line.ShipmentInformation.NumberOfPallet);

                  if (line.ShipmentInformation != null)
                    orderResponseLine.NumberOfUnits = int.Parse(line.ShipmentInformation.NumberOfColli);

                  unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);
                }
              }

              unit.Scope.Repository<Concentrator.Objects.Models.Orders.OrderResponse>().Add(invoiceOrderResponse);
              unit.Save();
            }
            break;
          case "PurchaseAcknowledgement":
            ediDocument = ExtractResponseMessage<PurchaseAcknowledgement>(responseString);
            PurchaseAcknowledgement purchaseAcknowledgement = (PurchaseAcknowledgement)ediDocument;

            bskIdentifier = int.Parse(purchaseAcknowledgement.bskIdentifier);
            int orderID = int.Parse(purchaseAcknowledgement.Reference);

            Concentrator.Objects.Models.Orders.OrderResponse pruchaseResponse = new Concentrator.Objects.Models.Orders.OrderResponse()
            {
              OrderID = orderID,
              Currency = "EUR",
              ReceiveDate = DateTime.Now,
              ResponseType = OrderResponseTypes.PurchaseAcknowledgement.ToString(),
              VendorID = _vendorID,
              VendorDocumentNumber = purchaseAcknowledgement.PurchaseOrderNumber,
              VendorDocumentDate = DateTime.Now,
              VendorDocument = responseString.OuterXml,
              DocumentName = fileName
            };

            int counter = 0;

            foreach (var line in purchaseAcknowledgement.PurchaseAcknowledgementLine)
            {
              int orderLineID = unit.Scope.Repository<OrderLine>().GetAll(x => x.OrderID == orderID && x.Product.VendorItemNumber.Trim() == line.LineReference.Trim()).Select(x => x.OrderLineID).FirstOrDefault();

              if (orderLineID > 0)
              {
                OrderResponseLine orderResponseLine = new OrderResponseLine()
                {
                  VendorItemNumber = line.ItemNumber,
                  VendorLineNumber = line.LineNumber,
                  Ordered = int.Parse(line.Quantity),
                  OEMNumber = line.LineReference,
                  OrderResponse = pruchaseResponse,
                  OrderLineID = orderLineID
                };
                unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);
                counter++;
              }
            }

            if (counter > 0)
            {
              unit.Scope.Repository<Concentrator.Objects.Models.Orders.OrderResponse>().Add(pruchaseResponse);
              unit.Save();
            }
            else
            {
              log.DebugFormat("No lines to process for order {0}", orderID);
            }

            break;
        }
      }
    }
  }
}
