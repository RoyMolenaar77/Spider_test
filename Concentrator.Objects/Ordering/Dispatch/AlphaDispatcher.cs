using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Concentrator.Objects.Ftp;
using System.Configuration;
using System.IO;
using System.Xml;
using log4net;
using Concentrator.Objects;
using Concentrator.Objects.Enumerations;
using System.Net;

using AuditLog4Net.Adapter;
using System.Data.Linq;
using System.Web;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Ordering.XmlFormats;

namespace Concentrator.Objects.Ordering.Dispatch
{
  public class AlphaDispatcher : IDispatchable
  {
    #region IDispatchable Members

    public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor, IAuditLogAdapter log, IUnitOfWork work)
    {
      try
      {
        XDocument order = new AlphaOrderExporter().GetOrder(orderLines, vendor);
        var msgID = orderLines.FirstOrDefault().Value.FirstOrDefault().OrderLineID;



        FtpManager manager = new FtpManager(vendor.VendorSettings.GetValueByKey("AlphaFtpUrl", string.Empty),
                                            string.Empty,
                                            vendor.VendorSettings.GetValueByKey("AlphaUserName", string.Empty),
                                            vendor.VendorSettings.GetValueByKey("AlphaPassword", string.Empty), false, false, log);

        using (Stream s = new MemoryStream())
        {
          XmlDocument doc = new XmlDocument();
          doc.LoadXml(order.ToString());
          doc.Save(s);

          string fileName = vendor.VendorSettings.GetValueByKey("AlphaCustomerNumber", string.Empty) + "-" + "ORDER" + "-" + msgID + ".xml";

          manager.Upload(s, vendor.VendorSettings.GetValueByKey("AlphaRemoteOrderDirectory", string.Empty) + @"/WS-D" + fileName);

          LogOrder(doc, vendor.VendorID, fileName, log);
        }

        return msgID;
      }
      catch (Exception e)
      {
        throw new Exception("Alpha dispatching failed", e);
      }
    }

    public void LogOrder(object orderInformation, int vendorID, string fileName, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      try
      {
        var logPath = ConfigurationManager.AppSettings["ConcentratorOrderLog"];

        logPath = Path.Combine(logPath, DateTime.Now.ToString("yyyyMMdd"), vendorID.ToString());

        if (!Directory.Exists(logPath))
          Directory.CreateDirectory(logPath);

        ((XmlDocument)orderInformation).Save(Path.Combine(logPath, fileName));
      }
      catch (Exception ex)
      {
        log.AuditError("Failed to log order information for " + vendorID, ex);
      }
    }


    public void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, string logPath, IUnitOfWork unit)
    {
      FtpManager AcknowledgementManager = new FtpManager(vendor.VendorSettings.GetValueByKey("AlphaFtpUrl", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaRemoteResponseDirectory", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaUserName", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaPassword", string.Empty), false, false, null);

      ProcessNotifications(AcknowledgementManager, OrderResponseTypes.Acknowledgement, log, vendor, logPath, unit);

      FtpManager ShipmentNotificationManager = new FtpManager(vendor.VendorSettings.GetValueByKey("AlphaFtpUrl", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaRemoteDispadviceDirectory", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaUserName", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaPassword", string.Empty), false, false, null);

      ProcessNotifications(ShipmentNotificationManager, OrderResponseTypes.ShipmentNotification, log, vendor, logPath, unit);

      FtpManager InvoiceNotificationManager = new FtpManager(vendor.VendorSettings.GetValueByKey("AlphaFtpUrl", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaRemoteInvoiceDirectory", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaUserName", string.Empty),
                                          vendor.VendorSettings.GetValueByKey("AlphaPassword", string.Empty), false, false, null);

      ProcessNotifications(InvoiceNotificationManager, OrderResponseTypes.InvoiceNotification, log, vendor, logPath, unit);
    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }

    private void ProcessNotifications(FtpManager manager, OrderResponseTypes responseType, IAuditLogAdapter log, Vendor vendor, string logPath, IUnitOfWork unit)
    {
      foreach (var file in manager)
      {
        bool error = false;
        try
        {
          if (!file.FileName.EndsWith(".XML")) continue;

          using (var reader = XmlReader.Create(file.Data))
          {

            reader.MoveToContent();
            XDocument xdoc = XDocument.Load(reader);

            string fileName = "Alpha_" + responseType.ToString() + "_" + Guid.NewGuid() + ".xml";

            xdoc.Save(Path.Combine(logPath, fileName));

            var orderDocuments = xdoc.Root.Elements("Order");

            if (responseType == OrderResponseTypes.InvoiceNotification)
              orderDocuments = xdoc.Root.Element("Invoice").Element("Orders").Elements("Order");

            int orderID = 0;

            foreach (var orderDocument in orderDocuments)
            {
              if (orderDocument.Element("Reference").Value.Contains('/'))
                int.TryParse(orderDocument.Element("Reference").Value.Split('/')[0], out orderID);
              else
                int.TryParse(orderDocument.Element("Reference").Value, out orderID);

              var order = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetSingle(o => o.OrderID == orderID);

              if (order != null)
              {
                OrderResponse response = null;

                if (responseType == OrderResponseTypes.InvoiceNotification)
                {
                  response = new OrderResponse()
                  {
                    OrderID = order.OrderID,
                    ResponseType = responseType.ToString(),
                    VendorDocument = xdoc.ToString(),
                    InvoiceDocumentNumber = xdoc.Root.Element("Invoice").Attribute("ID").Value,
                    InvoiceDate = DateTime.Parse(xdoc.Root.Element("Invoice").Element("InvDate").Value),
                    VatPercentage = decimal.Parse(xdoc.Root.Element("Invoice").Element("VatPercentage").Value),
                    VatAmount = decimal.Parse(xdoc.Root.Element("Invoice").Element("VatAmount").Value),
                    TotalGoods = decimal.Parse(xdoc.Root.Element("Invoice").Element("TotalGoods").Value),
                    AdministrationCost = decimal.Parse(xdoc.Root.Element("Invoice").Element("AdministrationCost").Value),
                    DropShipmentCost = decimal.Parse(xdoc.Root.Element("Invoice").Element("DropShipmentCost").Value),
                    ShipmentCost = decimal.Parse(xdoc.Root.Element("Invoice").Element("ShipmentCost").Value),
                    TotalExVat = decimal.Parse(xdoc.Root.Element("Invoice").Element("TotalExVat").Value),
                    TotalAmount = decimal.Parse(xdoc.Root.Element("Invoice").Element("TotalAmount").Value),
                    PaymentConditionDays = int.Parse(xdoc.Root.Element("Invoice").Element("PaymentConditionDays").Value),
                    PaymentConditionDiscount = xdoc.Root.Element("Invoice").Element("PaymentConditionDiscount").Value,
                    PaymentConditionDiscountDescription = xdoc.Root.Element("Invoice").Element("PaymentConditionDescription").Value,
                    DespAdvice = orderDocument.Element("DespAdvice").Value,
                    VendorDocumentNumber = orderDocument.Attribute("ID").Value
                  };
                }
                else
                {
                  if (responseType == OrderResponseTypes.Acknowledgement)
                  {
                    response = new OrderResponse()
                     {
                       OrderID = order.OrderID,
                       ResponseType = responseType.ToString(),
                       VendorDocument = xdoc.ToString(),
                       AdministrationCost = decimal.Parse(orderDocument.Element("AdministrationCost").Value),
                       DropShipmentCost = decimal.Parse(orderDocument.Element("DropShipmentCost").Value),
                       ShipmentCost = decimal.Parse(orderDocument.Element("ShipmentCost").Value),
                       OrderDate = DateTime.Parse(orderDocument.Element("OrderDate").Value),
                       VendorDocumentNumber = orderDocument.Attribute("ID").Value
                     };
                  }

                  if (responseType == OrderResponseTypes.ShipmentNotification)
                  {
                    response = new OrderResponse()
                    {
                      OrderID = order.OrderID,
                      ResponseType = responseType.ToString(),
                      VendorDocument = xdoc.ToString(),
                      OrderDate = DateTime.Parse(orderDocument.Element("OrderDate").Value),
                      ReqDeliveryDate = DateTime.Parse(orderDocument.Element("ReqDelDate").Value),
                      ShippingNumber = orderDocument.Element("ShippingNr").Value,
                      DespAdvice = orderDocument.Element("DespAdvice").Value,
                      TrackAndTrace = orderDocument.Element("TrackAndTrace").Element("TrackAndTraceID").Value,
                      TrackAndTraceLink = orderDocument.Element("TrackAndTrace").Element("TrackAndTraceURL").Value,
                      VendorDocumentNumber = orderDocument.Attribute("ID").Value
                    };
                  }
                }

                response.VendorID = vendor.VendorID;
                response.ReceiveDate = DateTime.Now;
                response.VendorDocument = orderDocument.ToString();
                response.VendorDocumentDate = DateTime.Now;
                response.DocumentName = fileName;
                unit.Scope.Repository<OrderResponse>().Add(response);

                foreach (var line in orderDocument.Elements("OrderLines").Elements("OrderLine"))
                {
                  OrderLine orderLine = null;
                  if (line.Element("ReferenceOrderLine") != null)
                    orderLine = order.OrderLines.Where(x => x.OrderLineID == int.Parse(line.Element("ReferenceOrderLine").Value)).FirstOrDefault();

                  if (orderLine == null)
                  {
                    string vendorItemNumber = line.Elements("Item").Where(x => x.Attribute("Type").Value == "O").Try(x => x.FirstOrDefault().Value, string.Empty);
                    orderLine = order.OrderLines.Where(x => x.Product != null && x.Product.VendorItemNumber == vendorItemNumber.Replace("\n", "")).FirstOrDefault();
                  }


                  if (orderLine != null)
                  {
                    int invoiced = 0;
                    int ordered = 0;
                    int backordered = 0;
                    int shipped = 0;
                    int deliverd = 0;

                    if (responseType != OrderResponseTypes.InvoiceNotification)
                    {
                      ordered = line.Elements("Quantity").Where(x => x.Attribute("Type").Value == "Ordered") != null ?
                                    int.Parse(line.Elements("Quantity").Where(x => x.Attribute("Type").Value == "Ordered").FirstOrDefault().Value) : 0;
                      if (responseType == OrderResponseTypes.ShipmentNotification)
                      {
                        shipped = line.Elements("Quantity").Where(x => x.Attribute("Type").Value == "Shipped") != null ?
                                  int.Parse(line.Elements("Quantity").Where(x => x.Attribute("Type").Value == "Shipped").FirstOrDefault().Value) : 0;
                      }

                      if (responseType == OrderResponseTypes.Acknowledgement)
                      {
                        int reserved = line.Elements("Quantity").Where(x => x.Attribute("Type").Value == "Reserved") != null ?
                                     int.Parse(line.Elements("Quantity").Where(x => x.Attribute("Type").Value == "Reserved").FirstOrDefault().Value) : 0;
                        backordered = ordered - reserved;
                      }

                      deliverd = line.Elements("Quantity").Where(x => x.Attribute("Type").Value == "Delivered") != null ?
                                    int.Parse(line.Elements("Quantity").Where(x => x.Attribute("Type").Value == "Delivered").FirstOrDefault().Value) : 0;


                    }
                    else
                      invoiced = line.Elements("Quantity").Where(x => x.Attribute("Type") == null) != null ?
                        int.Parse(line.Elements("Quantity").Where(x => x.Attribute("Type") == null).FirstOrDefault().Value) : 0;

                    //Type of itemnumber:
                    //"S" = Internal itemnumber
                    //"O" = OEM number
                    //"C" = Customer number
                    //"E" = "EAN number"

                    OrderResponseLine responseLine = new OrderResponseLine()
                    {
                      OrderResponse = response,
                      OrderLineID = orderLine.OrderLineID,
                      Ordered = ordered != 0 ? ordered : orderLine.Quantity,
                      Backordered = backordered,
                      Delivered = deliverd,
                      Cancelled = responseType != OrderResponseTypes.InvoiceNotification ? orderLine.GetDispatchQuantity() - ordered : 0,
                      Shipped = shipped,
                      Invoiced = invoiced,
                      Unit = line.Element("Unit").Value,
                      Price = line.Element("Price") != null ? decimal.Parse(line.Element("Price").Value) : 0,
                      VendorLineNumber = line.Attribute("ID").Value,
                      VendorItemNumber = line.Elements("Item").Where(x => x.Attribute("Type").Value == "S").Try(x => x.FirstOrDefault().Value, string.Empty),
                      OEMNumber = line.Elements("Item").Where(x => x.Attribute("Type").Value == "O").Try(x => x.FirstOrDefault().Value, string.Empty),
                      Barcode = line.Elements("Item").Where(x => x.Attribute("Type").Value == "E").Try(x => x.FirstOrDefault().Value, string.Empty),
                      Description = line.Element("Description").Value
                    };

                    if (line.Element("ReqDelDate") != null)
                      responseLine.DeliveryDate = DateTime.Parse(line.Element("ReqDelDate").Value);

                    unit.Scope.Repository<OrderResponseLine>().Add(responseLine);

                  }
                }
              }
              else
              {
                log.AuditInfo("Received response does not match any order, ignore response");
                manager.MarkAsError(file.FileName);
                error = true;
                continue;
              }
            }

            unit.Save();
            if (!error)
              manager.Delete(file.FileName);
          }
        }
        catch (Exception e)
        {
          log.AuditError("Error reading file", e);
          //manager.MarkAsError(file.FileName);
        }
      }

    }

    #endregion


  }
}
