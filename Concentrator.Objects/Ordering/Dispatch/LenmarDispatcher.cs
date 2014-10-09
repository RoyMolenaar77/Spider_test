using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using AuditLog4Net.Adapter;

using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Ordering.Dispatch
{
  class LenmarDispatcher : IDispatchable
  {
    #region IDispatchable Members
    public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, IUnitOfWork unit)
    {
      XNamespace aw = "http://logictec.com/schemas/internaldocuments";

      foreach (var Order in orderLines.Keys)
      {
        var envelopeHeader = new XElement("EnvelopeHeader",
                                new XElement("Mode", new XAttribute("xmlns", ""), "Production"),
                                new XElement("PartnerIdentifier", new XAttribute("xmlns", ""), "BASGROUP"),
                               new XElement("EnvelopeControlNumber", new XAttribute("xmlns", ""), "000000002"),
                                new XElement("DocumentType", new XAttribute("xmlns", ""), "PurchaseOrder"),
                               new XElement("DocumentDateTime", new XAttribute("xmlns", ""), DateTime.Now.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffffffK"))
                                );


        var messages = new XElement("{http://logictec.com/schemas/internaldocuments}PurchaseOrder", new XAttribute(XNamespace.Xmlns + "int", "http://logictec.com/schemas/internaldocuments"),

                                    new XElement("{http://logictec.com/schemas/internaldocuments}OrderHeader",
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}PartnerPO", Order.OrderID),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}SupplierPO", Order.WebSiteOrderNumber),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}EndUserPO", Order.CustomerOrderReference),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}Email", Order.ShippedToCustomer.CustomerEmail),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}PODateTime", DateTime.Now.ToString("yyyy-MM-dd")),
         new XElement("{http://logictec.com/schemas/internaldocuments}ShipToCompany"),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToAddress1", Order.ShippedToCustomer.CustomerAddressLine1 + " " + Order.ShippedToCustomer.HouseNumber),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToAddress2", Order.ShippedToCustomer.CustomerAddressLine2),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToCity", Order.ShippedToCustomer.City),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToState"),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToZip", Order.ShippedToCustomer.PostCode),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToContact", Order.ShippedToCustomer.CustomerName),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToPhone", Order.ShippedToCustomer.CustomerTelephone),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToFName"),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToLName", Order.ShippedToCustomer.CustomerName),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToFax"),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}ShipToCountryCode", Order.ShippedToCustomer.Country),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}HandlingAmt"),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}HandlingTax"),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}MiscAmt"),
                                                 new XElement("{http://logictec.com/schemas/internaldocuments}MiscTax")
                                      ),

                                      new XElement("{http://logictec.com/schemas/internaldocuments}OrderItems",
                                         from line in orderLines[Order]
                                         let assortmentItem =  line.Product.VendorAssortments.FirstOrDefault(c => c.VendorID == vendor.VendorID)
                                         where line.Product != null
                                         select
                                           new XElement("{http://logictec.com/schemas/internaldocuments}OrderItem",

                                            new XElement("{http://logictec.com/schemas/internaldocuments}LineID", Order.OrderLines.ToList().IndexOf(line)),
                                            new XElement("{http://logictec.com/schemas/internaldocuments}PartnerLineID", line.OrderLineID),
                                            new XElement("{http://logictec.com/schemas/internaldocuments}Qty", line.GetDispatchQuantity()),
                                            new XElement("{http://logictec.com/schemas/internaldocuments}PartnerSku", line.ProductID),
                                            new XElement("{http://logictec.com/schemas/internaldocuments}SupplierSKU", assortmentItem.CustomItemNumber),
                                            new XElement("{http://logictec.com/schemas/internaldocuments}CustomerPrice", Math.Floor((assortmentItem.VendorPrices.FirstOrDefault() != null ? (double)assortmentItem.VendorPrices.FirstOrDefault().Price.Value : line.Price.Value) * 100)),
                                       new XElement("{http://logictec.com/schemas/internaldocuments}Description", assortmentItem.ShortDescription),
                                            new XElement("{http://logictec.com/schemas/internaldocuments}RequestedShipDate", DateTime.Now.ToString(@"yyyy-MM-dd"))


                                        ))


                                      );
        XDocument docs = new XDocument(
              new XElement(aw + "Envelope", new XAttribute("xmlns", "http://logictec.com/schemas/internaldocuments"),
                           new XAttribute(XNamespace.Xmlns + "xsi", "http://www.w3.org/2001/XMLSchema-instance"),

                                          envelopeHeader,
                                          new XElement("Messages", messages

                                            )));
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(docs.ToString().Replace("<EnvelopeHeader xmlns=\"\">", "<EnvelopeHeader>"));

        FtpManager manager = new FtpManager(vendor.VendorSettings.GetValueByKey("LenmarFtpUrl", string.Empty),
                                             vendor.VendorSettings.GetValueByKey("LenmarFtpPath", string.Empty),
                                            vendor.VendorSettings.GetValueByKey("LenmarUserName", string.Empty),
                                            vendor.VendorSettings.GetValueByKey("LenmarPassword", string.Empty), true, true, log);

        string fileName = string.Format("LenmarExport-{0}.xml", Order.OrderID);

        using (Stream s = new MemoryStream())
        {
          doc.Save(s);
          manager.Upload(s, fileName);
        }

        LogOrder(doc, vendor.VendorID, fileName, log);

      }
      return 0;
    }

    public void LogOrder(object orderInformation, int vendorID,string fileName, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      try
      {
        var logPath = ConfigurationManager.AppSettings["ConcentratorOrderLog"];

        logPath = Path.Combine(logPath,DateTime.Now.ToString("yyyyMMdd"),vendorID.ToString());

        if(!Directory.Exists(logPath))
          Directory.CreateDirectory(logPath);

        ((XmlDocument)orderInformation).Save(Path.Combine(logPath, fileName));
      }
      catch (Exception ex)
      {
        log.AuditError("Failed to log order information for " + vendorID, ex);
      }
    }

    public void GetAvailableDispatchAdvices(Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath, IUnitOfWork unit)
    {
      FtpManager acknowledgementManager = new FtpManager(vendor.VendorSettings.GetValueByKey("LenmarFtpUrl", string.Empty),
                                             vendor.VendorSettings.GetValueByKey("LenmarFtpPathACK", string.Empty),
                                            vendor.VendorSettings.GetValueByKey("LenmarUserName", string.Empty),
                                            vendor.VendorSettings.GetValueByKey("LenmarPassword", string.Empty), true, true, log);

      ProcessNotifications(acknowledgementManager, OrderResponseTypes.Acknowledgement, log, vendor, logPath, unit);

      FtpManager shipmentNotificationManager = new FtpManager(vendor.VendorSettings.GetValueByKey("LenmarFtpUrl", string.Empty),
                                             vendor.VendorSettings.GetValueByKey("LenmarFtpPathASN", string.Empty),
                                            vendor.VendorSettings.GetValueByKey("LenmarUserName", string.Empty),
                                            vendor.VendorSettings.GetValueByKey("LenmarPassword", string.Empty), true, true, log);

      ProcessNotifications(shipmentNotificationManager, OrderResponseTypes.ShipmentNotification, log, vendor, logPath, unit);

      FtpManager InvoiceNotificationManager = new FtpManager(vendor.VendorSettings.GetValueByKey("LenmarFtpUrl", string.Empty),
                                                  vendor.VendorSettings.GetValueByKey("LenmarFtpPathInvoice", string.Empty),
                                                 vendor.VendorSettings.GetValueByKey("LenmarUserName", string.Empty),
                                                 vendor.VendorSettings.GetValueByKey("LenmarPassword", string.Empty), true, true, log);

      ProcessNotifications(InvoiceNotificationManager, OrderResponseTypes.InvoiceNotification, log, vendor, logPath, unit);
    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }

    #endregion

    static string ReadMessage(SslStream sslStream)
    {
      // Read the  message sent by the server. The end of the message is signaled using the "<EOF>" marker.
      byte[] buffer = new byte[2048];
      StringBuilder messageData = new StringBuilder();
      int bytes = -1;
      do
      {
        bytes = sslStream.Read(buffer, 0, buffer.Length);
        // Use Decoder class to convert from bytes to UTF8 in case a character spans two buffers.
        Decoder decoder = Encoding.UTF8.GetDecoder();
        char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
        decoder.GetChars(buffer, 0, bytes, chars, 0);
        messageData.Append(chars);
        // Check for EOF.
        if (messageData.ToString().IndexOf("<EOF>") != -1)
        {
          break;
        }
      } while (bytes != 0);
      return messageData.ToString();
    }

    private static byte[] ReadFully(Stream input)
    {
      using (StreamReader reader = new StreamReader(input))
      {
        return Encoding.ASCII.GetBytes(reader.ReadToEnd());
      }
    }

    private void ProcessNotifications(FtpManager manager, OrderResponseTypes responseType, IAuditLogAdapter log, Vendor vendor, string logPath, IUnitOfWork unit)
    {
      //DataLoadOptions options = new DataLoadOptions();
      //options.LoadWith<Concentrator.Objects.Orders.Order>(x => x.OrderLines);

      foreach (var file in manager)
      {
        bool error = false;

        try
        {
          using (var reader = XmlReader.Create(file.Data))
          {
            reader.MoveToContent();
            XDocument xdoc = XDocument.Load(reader);

            Guid vendorDocumentReference = Guid.NewGuid();

            string fileName = "Lenmar_" + responseType.ToString() + "_" + vendorDocumentReference + ".xml";

            xdoc.Save(Path.Combine(logPath, fileName));

            XNamespace xName = "http://logictec.com/schemas/internaldocuments";

            OrderResponse orderResponse = null;
            List<OrderResponseLine> orderResponseLines = null;

            IEnumerable<XElement> orderDocuments = null;

            if (responseType == OrderResponseTypes.InvoiceNotification)
              orderDocuments = xdoc.Elements(xName + "Envelope").Elements("Messages").Elements(xName + "Invoice");
            else if (responseType == OrderResponseTypes.ShipmentNotification)
              orderDocuments = xdoc.Elements(xName + "Envelope").Elements("Messages").Elements(xName + "AdvancedShipNotice");
            else if (responseType == OrderResponseTypes.Acknowledgement)
              orderDocuments = xdoc.Elements(xName + "Envelope").Elements("Messages").Elements(xName + "POAck");

            foreach (var orderDocument in orderDocuments)
            {

              if (responseType == OrderResponseTypes.ShipmentNotification)
              {
                orderResponse = new OrderResponse
                                 {
                                   InvoiceDocumentNumber = orderDocument.Element("ASNHeader").Element("InvoiceNumber").Value,
                                   VendorDocumentNumber = orderDocument.Element("ASNHeader").Element("OrderID").Value,
                                   OrderID = int.Parse(orderDocument.Element("ASNHeader").Element("PartnerPO").Value),
                                   AdministrationCost = Decimal.Parse(orderDocument.Element("ASNHeader").Element("HandlingAmt").Value),
                                   TrackAndTrace = orderDocument.Element("ASNTrackingNumbers").Element("ASNTrackingNumberItem").Element("TrackingNumber").Value,
                                   OrderDate = DateTime.Parse(orderDocument.Element("ASNHeader").Element("PODateTime").Value),
                                   ResponseType = responseType.ToString()
                                 };

                orderResponseLines = (from d in orderDocument.Elements("ASNItems").Elements("ASNItem")
                                      select new OrderResponseLine
                                      {
                                        OrderResponse = orderResponse,
                                        VendorLineNumber = d.Element("LineID").Value,
                                        OrderLineID = int.Parse(d.Element("PartnerLineID").Value),
                                        Ordered = int.Parse(d.Element("Qty").Value),
                                        Shipped = int.Parse(d.Element("QtyShipped").Value),
                                        VendorItemNumber = d.Element("SupplierSKU").Value,
                                        Price = decimal.Parse(d.Element("ItemPrice").Value) / 100,
                                        Barcode = d.Element("UPCCode").Value,
                                        Description = d.Element("Description").Value,
                                        OEMNumber = d.Element("MfgSKU").Value,
                                        DeliveryDate = DateTime.Parse(d.Element("ExpectedDeliveryDate").Value),
                                        TrackAndTrace = orderDocument.Element("ASNTrackingNumbers").Element("ASNTrackingNumberItem").Element("TrackingNumber").Value,
                                        RequestDate = DateTime.Parse(d.Element("RequestedShipDate").Value)
                                      }).ToList();
              }
              else if (responseType == OrderResponseTypes.InvoiceNotification)
              {
                orderResponse = new OrderResponse
                                 {
                                   InvoiceDocumentNumber = orderDocument.Element("InvoiceHeader").Element("InvoiceNumber").Value,
                                   InvoiceDate = DateTime.Parse(orderDocument.Element("InvoiceHeader").Element("InvoiceDateTime").Value),
                                   PaymentConditionDays = int.Parse(orderDocument.Element("InvoiceHeader").Element("InvoiceDaysDue").Value),
                                   PaymentConditionDiscount = orderDocument.Element("InvoiceHeader").Element("DiscountDaysDue").Value,
                                   PaymentConditionDiscountDescription = orderDocument.Element("InvoiceHeader").Element("DiscountPercent").Value,
                                   TotalExVat = decimal.Parse(orderDocument.Element("InvoiceHeader").Element("InvoiceTotal").Value),
                                   VatAmount = decimal.Parse(orderDocument.Element("InvoiceHeader").Element("InvoiceTaxAmt").Value),
                                   VendorDocumentNumber = orderDocument.Element("InvoiceHeader").Element("OrderID").Value,
                                   OrderID = int.Parse(orderDocument.Element("InvoiceHeader").Element("PartnerPO").Value),
                                   AdministrationCost = Decimal.Parse(orderDocument.Element("InvoiceHeader").Element("HandlingAmt").Value),
                                   TrackAndTrace = orderDocument.Element("InvoiceTrackingNumbers").Element("InvoiceTrackingNumberItem").Element("TrackingNumber").Value,
                                   OrderDate = DateTime.Parse(orderDocument.Element("InvoiceHeader").Element("PODateTime").Value),
                                   ResponseType = responseType.ToString()
                                 };

                orderResponseLines = (from d in orderDocument.Elements("InvoiceItems").Elements("InvoiceItem")
                                      select new OrderResponseLine
                                      {
                                        OrderResponse = orderResponse,
                                        VendorLineNumber = d.Element("LineID").Value,
                                        OrderLineID = int.Parse(d.Element("PartnerLineID").Value),
                                        Ordered = int.Parse(d.Element("Qty").Value),
                                        Invoiced = int.Parse(d.Element("QtyShipped").Value),
                                        VendorItemNumber = d.Element("SupplierSKU").Value,
                                        Price = decimal.Parse(d.Element("ItemPrice").Value) / 100,
                                        Barcode = d.Element("UPCCode").Value,
                                        Description = d.Element("Description").Value,
                                        OEMNumber = d.Element("MfgSKU").Value,
                                        DeliveryDate = DateTime.Parse(d.Element("ActualShipDate").Value),
                                        RequestDate = DateTime.Parse(d.Element("RequestedShipDate").Value)
                                      }).ToList();
              }
              else if (responseType == OrderResponseTypes.Acknowledgement)
              {
                orderResponse = new OrderResponse
                                 {
                                   VendorDocumentNumber = orderDocument.Element("POAckHeader").Element("OrderID").Value,
                                   OrderID = int.Parse(orderDocument.Element("POAckHeader").Element("PartnerPO").Value),
                                   AdministrationCost = Decimal.Parse(orderDocument.Element("POAckHeader").Element("HandlingAmt").Value),
                                   OrderDate = DateTime.Parse(orderDocument.Element("POAckHeader").Element("PODateTime").Value),
                                   ResponseType = responseType.ToString()
                                 };

                orderResponseLines = (from d in orderDocument.Elements("POAckItems").Elements("POAckItem")
                                      select new OrderResponseLine
                                      {
                                        OrderResponse = orderResponse,
                                        VendorLineNumber = d.Element("LineID").Value,
                                        OrderLineID = int.Parse(d.Element("PartnerLineID").Value),
                                        Ordered = int.Parse(d.Element("Qty").Value),
                                        Backordered = int.Parse(d.Element("QtyBackOrdered").Value),
                                        VendorItemNumber = d.Element("SupplierSKU").Value,
                                        Price = decimal.Parse(d.Element("ItemPrice").Value) / 100,
                                        Barcode = d.Element("UPCCode").Value,
                                        Description = d.Element("Description").Value,
                                        RequestDate = DateTime.Parse(d.Element("RequestedShipDate").Value)
                                      }).ToList();
              }

              var order = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetSingle(o => o.OrderID == orderResponse.OrderID);


              if (order != null)
              {
                orderResponse.VendorID = vendor.VendorID;
                orderResponse.ReceiveDate = DateTime.Now;
                orderResponse.VendorDocument = xdoc.ToString();
                orderResponse.VendorDocumentDate = DateTime.Now;
                orderResponse.DocumentName = fileName;
                unit.Scope.Repository<OrderResponse>().Add(orderResponse);
                unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLines);
              }
              else
              {
                log.AuditInfo("Received response does not match any order, ignore response");
                manager.MarkAsError(file.FileName);
                error = true;
                continue;
              }
            }
          }

          unit.Save();
          if (!error)
            manager.Delete(file.FileName);
        }
        catch (Exception e)
        {
          throw e;
        }
      }

    }
  }
}
