using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Configuration;
using AuditLog4Net.Adapter;
using AuditLog4Net.AuditLog;
using Concentrator.Objects.Ftp;
using System.Data.Linq;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using Concentrator.Objects.Utility;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Parse;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Objects.Ordering.Dispatch
{

  public class VSNDispatcher : IDispatchable
  {
    private static IAuditLogAdapter log;

    static VSNDispatcher()
    {
      log = new AuditLogAdapter(log4net.LogManager.GetLogger(typeof(VSNDispatcher)), new AuditLog(new ConcentratorAuditLogProvider()));
    }

    #region IDispatchable Members

    public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor, IAuditLogAdapter log, IUnitOfWork unit)
    {
      try
      {
        var orderDoc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                                     new XElement("SalesOrders",
                                         from o in orderLines
                                         select new XElement("SalesOrder",
                                           new XElement("CustomerCode", vendor.VendorSettings.GetValueByKey("VSNUser", string.Empty)),
                                           new XElement("Reference", o.Key.OrderID),
                                          new XElement("DeliveryDate", DateTime.Now.ToString("dd-MM-yyyy")),
                                          new XElement("DeliveryAddress",
                                            new XElement("AddressName", o.Key.ShippedToCustomer.CustomerName),
                                            new XElement("Street", o.Key.ShippedToCustomer.CustomerAddressLine1),
                                            new XElement("HouseNumber", o.Key.ShippedToCustomer.HouseNumber),
                                            new XElement("ZipCode", o.Key.ShippedToCustomer.PostCode.Length < 7 ? o.Key.ShippedToCustomer.PostCode.Substring(0, 4) + " " + o.Key.ShippedToCustomer.PostCode.Substring(4, 2).ToUpper() : o.Key.ShippedToCustomer.PostCode.ToUpper()),
                                            new XElement("City", o.Key.ShippedToCustomer.City),
                                            new XElement("CountryCode", o.Key.ShippedToCustomer.Country)),
                                            from ol in o.Value
                                            let vendorAss = VendorUtility.GetMatchedVendorAssortment(unit.Scope.Repository<VendorAssortment>(), vendor.VendorID, ol.ProductID.Value)
                                            let purchaseLine = ol.OrderResponseLines.Where(x => x.OrderLineID == ol.OrderLineID && x.OrderResponse.ResponseType == OrderResponseTypes.PurchaseAcknowledgement.ToString()).FirstOrDefault()
                                            select new XElement("SalesOrderLine",
                                             new XElement("ProductCode", vendorAss.CustomItemNumber),
                                             new XElement("Quantity", ol.GetDispatchQuantity()),
                                             new XElement("Reference", ol.OrderLineID + "/" + ol.Order.WebSiteOrderNumber)//(purchaseLine != null ? purchaseLine.VendorItemNumber : vendorAss.ProductID.ToString()))
                                             )
                                           )
                                       )
                                       );

        var ftp = new FtpManager(vendor.VendorSettings.GetValueByKey("VSNFtpUrl", string.Empty), "orders/",
                                  vendor.VendorSettings.GetValueByKey("VSNUser", string.Empty),
                                  vendor.VendorSettings.GetValueByKey("VSNPassword", string.Empty), false, false, log);
        var fileName = String.Format("{0}.xml", DateTime.Now.ToString("yyyyMMddhhmmss"));

        using (var inStream = new MemoryStream())
        {
          using (XmlWriter writer = XmlWriter.Create(inStream))
          {
            orderDoc.WriteTo(writer);
            writer.Flush();
          }
          ftp.Upload(inStream, fileName);
        }

        LogOrder(orderDoc,vendor.VendorID, fileName, log); 

        return -1;
      }
      catch (Exception e)
      {
        throw new Exception("VSN dispatching failed", e);
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

        ((XDocument)orderInformation).Save(Path.Combine(logPath, fileName));
      }
      catch (Exception ex)
      {
        log.AuditError("Failed to log order information for " + vendorID, ex);
      }
    }


    public void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, string logPath, IUnitOfWork unit)
    {
      try
      {
        FtpManager AcknowledgementManager = new FtpManager(vendor.VendorSettings.GetValueByKey("VSNFtpUrl", string.Empty),
                                           "orderresponse/",
                                           vendor.VendorSettings.GetValueByKey("VSNUser", string.Empty),
                                           vendor.VendorSettings.GetValueByKey("VSNPassword", string.Empty), false, false, log);

        ProcessNotifications(AcknowledgementManager, OrderResponseTypes.Acknowledgement, log, vendor, logPath, unit);
      }
      catch (Exception ex)
      {
        log.AuditWarning("Acknowledment VSN failed", ex);
      }

      try
      {
        FtpManager ShipmentNotificationManager = new FtpManager(vendor.VendorSettings.GetValueByKey("VSNFtpUrl", string.Empty),
                                           "pakbonnen/",
                                           vendor.VendorSettings.GetValueByKey("VSNUser", string.Empty),
                                           vendor.VendorSettings.GetValueByKey("VSNPassword", string.Empty), false, false, log);

        ProcessNotifications(ShipmentNotificationManager, OrderResponseTypes.ShipmentNotification, log, vendor, logPath, unit);
      }
      catch (Exception ex)
      {
        log.Warn("Shipment Notification VSN failed", ex);
      }

      try
      {
        FtpManager InvoiceNotificationManager = new FtpManager(vendor.VendorSettings.GetValueByKey("VSNFtpUrl", string.Empty),
                                           vendor.VendorSettings.GetValueByKey("InvoicePath", string.Empty) + "/",
                                           vendor.VendorSettings.GetValueByKey("VSNUser", string.Empty),
                                           vendor.VendorSettings.GetValueByKey("VSNPassword", string.Empty), false, false, log);

        ProcessInvoiceNotifications(InvoiceNotificationManager, OrderResponseTypes.InvoiceNotification, log, vendor, logPath, unit);
      }
      catch (Exception ex)
      {
        log.AuditWarning("Invoice Notification VSN failed", ex);
      }

      try
      {
        FtpManager CancelNotificationManager = new FtpManager(vendor.VendorSettings.GetValueByKey("VSNFtpUrl", string.Empty),
                                     "cancellations/",
                                     vendor.VendorSettings.GetValueByKey("VSNUser", string.Empty),
                                     vendor.VendorSettings.GetValueByKey("VSNPassword", string.Empty), false, false, log);

        ProcessNotifications(CancelNotificationManager, OrderResponseTypes.CancelNotification, log, vendor, logPath, unit);
      }
      catch (Exception ex)
      {
        log.AuditWarning("Cancel Notification VSN failed", ex);
      }
    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }

    public LazyCsvParser GetInvoiceCSVFile(Stream fileStream, Type invoiceType)
    {
      Array enumValArray = null;
      List<string> enumValList = null;

      enumValArray = Enum.GetValues(invoiceType);

      enumValList = new List<string>(enumValArray.Length);

      foreach (int val in enumValArray)
      {
        enumValList.Add(Enum.Parse(invoiceType, val.ToString()).ToString());
      }

      return new LazyCsvParser(fileStream, enumValList, true, '\t');
    }

    public LazyCsvParser GetCSVFile(Stream fileStream, OrderResponseTypes type)
    {
      Array enumValArray = null;
      List<string> enumValList = null;

      switch (type)
      {
        case OrderResponseTypes.Acknowledgement:
          enumValArray = Enum.GetValues(typeof(VSNAcknowledgement));

          enumValList = new List<string>(enumValArray.Length);

          foreach (int val in enumValArray)
          {
            enumValList.Add(Enum.Parse(typeof(VSNAcknowledgement), val.ToString()).ToString());
          }
          break;
        case OrderResponseTypes.ShipmentNotification:
          enumValArray = Enum.GetValues(typeof(VSNShipment));

          enumValList = new List<string>(enumValArray.Length);

          foreach (int val in enumValArray)
          {
            enumValList.Add(Enum.Parse(typeof(VSNShipment), val.ToString()).ToString());
          }
          break;
        case OrderResponseTypes.CancelNotification:
          enumValArray = Enum.GetValues(typeof(VSNCancel));

          enumValList = new List<string>(enumValArray.Length);

          foreach (int val in enumValArray)
          {
            enumValList.Add(Enum.Parse(typeof(VSNCancel), val.ToString()).ToString());
          }
          break;
      }

      return new LazyCsvParser(fileStream, enumValList, true, '\t');

      //return new LazyCsvParser(enumValList, true, '\t');
    }

    private static byte[] ReadFully(Stream input)
    {
      using (StreamReader reader = new StreamReader(input))
      {
        return Encoding.ASCII.GetBytes(reader.ReadToEnd());
      }
    }

    private void ProcessInvoiceNotifications(FtpManager manager, OrderResponseTypes responseType, IAuditLogAdapter log, Vendor vendor, string logPath, IUnitOfWork unit)
    {
      foreach (var file in manager)
      {
        try
        {
          using (MemoryStream fileStream = new MemoryStream(ReadFully(file.Data)))
          {
            string fileName = "VSN_" + responseType.ToString() + "_" + Guid.NewGuid() + ".csv";
            string filePath = Path.Combine(logPath, fileName);
            using (FileStream s = File.Create(filePath))
            {
              s.Write(fileStream.ToArray(), 0, (int)fileStream.Length);
            }

            using (System.IO.TextReader readFile = new StreamReader(filePath))
            {
              string line = string.Empty;

              using (MemoryStream salesLines = new MemoryStream(),
               salesInvoiceTotal = new MemoryStream(),
               salesInvoiceGrandTotal = new MemoryStream())
              {
                using (
                  StreamWriter sw = new StreamWriter(salesLines),
                  sw2 = new StreamWriter(salesInvoiceTotal),
                  sw3 = new StreamWriter(salesInvoiceGrandTotal))
                {

                  int lineCount = 0;
                  while ((line = readFile.ReadLine()) != null)
                  {
                    lineCount++;

                    if (line.Contains("SalesInvoiceLine") || lineCount == 1)
                      sw.WriteLine(line);
                    else if (line.Contains("SalesInvoiceTotal"))
                      sw2.WriteLine(line);
                    else if (line.Contains("SalesInvoiceGrandTotal"))
                      sw3.WriteLine(line);
                    else if (!line.Contains("SalesInvoiceLine") && lineCount > 1 && !line.Contains("SalesInvoiceGrandTotal"))
                      sw3.WriteLine(line);
                  }

                  sw.Flush();
                  salesLines.Position = 0;
                  sw2.Flush();
                  salesInvoiceTotal.Position = 0;
                  sw3.Flush();
                  salesInvoiceGrandTotal.Position = 0;

                  var parser = GetInvoiceCSVFile(salesLines, typeof(VSNInvoice)).ToList();
                  var invoiveTotalParser = GetInvoiceCSVFile(salesInvoiceTotal, typeof(VSNInvoiceTotal));
                  var invoiceGrandTotalParser = GetInvoiceCSVFile(salesInvoiceGrandTotal, typeof(VSNInvoiceGrandTotal));

                  var firstOrderInInvoice = parser.FirstOrDefault();
                  var invoiceTotals = invoiveTotalParser.ToList();
                  int orderLineCounter = 0;

                  var totalAmount = invoiceTotals.Sum(x => decimal.Parse(x[VSNInvoiceTotal.AmountVATIncluded.ToString()]));
                  var totalExVat = invoiceTotals.Sum(x => decimal.Parse(x[VSNInvoiceTotal.AmountVATExcluded.ToString()]));
                  var vatAmount = invoiceTotals.Sum(x => decimal.Parse(x[VSNInvoiceTotal.AmountVAT.ToString()]));
                  var vatPercentage = invoiceTotals.Sum(x => decimal.Parse(x[VSNInvoiceTotal.VATPercentage.ToString()]));
                  var shipmentCost = invoiceTotals.Where(x => x[VSNInvoiceTotal.SalesInvoiceTotal.ToString()].Trim().ToLower() == "orderkosten").Sum(x => decimal.Parse(x[VSNInvoiceTotal.AmountVATExcluded.ToString()]));

                  #region InvoiceNotification
                  var vsnResponseType = responseType.ToString();
                  var vsnInvoiceID = firstOrderInInvoice[VSNInvoice.SalesInvoiceID.ToString()];
                  OrderResponse response = unit.Scope.Repository<OrderResponse>().GetSingle(x => x.VendorID == vendor.VendorID && x.InvoiceDocumentNumber == vsnInvoiceID && x.ResponseType == vsnResponseType);

                  if (response == null)
                  {
                    response = new OrderResponse()
                    {
                      Currency = "EUR",
                      ResponseType = responseType.ToString(),
                      //OrderID = orderID,
                      VendorDocumentNumber = string.Empty,
                      InvoiceDate = DateTime.ParseExact(firstOrderInInvoice[VSNInvoice.InvoiceDate.ToString()], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                      InvoiceDocumentNumber = firstOrderInInvoice[VSNInvoice.SalesInvoiceID.ToString()],
                      ShippingNumber = firstOrderInInvoice[VSNInvoice.PacklistID.ToString()],
                      PaymentConditionCode = firstOrderInInvoice[VSNInvoice.PatymentConditionName.ToString()],
                      ReqDeliveryDate = DateTime.ParseExact(firstOrderInInvoice[VSNInvoice.PacklistDate.ToString()], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                      PaymentConditionDiscount = firstOrderInInvoice[VSNInvoice.DiscountPercentage.ToString()],
                      TotalAmount = totalAmount,
                      TotalExVat = totalExVat,
                      VatAmount = vatAmount,
                      VatPercentage = vatPercentage,
                      ShipmentCost = shipmentCost
                    };
                  }

                  foreach (var p in parser)
                  {
                    try
                    {
                      int lineReference = 0;
                      string referenceField = p[VSNInvoice.LineCustomerReference.ToString()];
                      if (referenceField.Contains('/'))
                        int.TryParse(referenceField.Split('/')[0], out lineReference);
                      else
                        int.TryParse(referenceField, out lineReference);

                      OrderLine oLine = unit.Scope.Repository<OrderLine>().GetSingle(x => x.OrderLineID == lineReference);
                                            
                      if (oLine != null && !oLine.OrderResponseLines.Any(x => x.OrderResponse.ResponseType == vsnResponseType))
                      {
                        OrderResponseLine rLine = new OrderResponseLine()
                        {
                          OrderResponse = response,
                          Backordered = 0,
                          Cancelled = 0,
                          Ordered = oLine.GetDispatchQuantity(),
                          Invoiced = int.Parse(p[VSNInvoice.Quantity.ToString()]),
                          Barcode = p[VSNInvoice.EANNumberProduct.ToString()],
                          OrderLine = oLine,
                          Price = decimal.Parse(p[VSNInvoice.PriceDiscountIncluded.ToString()]),
                          VatAmount = decimal.Parse(p[VSNInvoice.LineTotal.ToString()]),
                          vatPercentage = decimal.Parse(p[VSNInvoice.VATPercentage.ToString()]),
                          CarrierCode = p[VSNInvoice.DeliveryMethodName.ToString()],
                          Processed = false,
                          Shipped = 0,
                          Remark = p[VSNInvoice.CustomerReference.ToString()]
                        };

                        unit.Scope.Repository<OrderResponseLine>().Add(rLine);
                        orderLineCounter++;
                      }

                  #endregion
                    }
                    catch (Exception)
                    {
                      log.AuditError("Failed to invoice line for VSN");
                    }
                  }

                  if (orderLineCounter > 0)
                  {
                    response.VendorID = vendor.VendorID;
                    response.ReceiveDate = DateTime.Now;
                    response.VendorDocument = parser + invoiveTotalParser.Document + invoiceGrandTotalParser.Document;
                    response.DocumentName = fileName;
                    if (!response.VendorDocumentDate.HasValue)
                      response.VendorDocumentDate = DateTime.Now;

                    response.ReceiveDate = DateTime.Now;

                    unit.Scope.Repository<OrderResponse>().Add(response);
                  }

                  unit.Save();
                  manager.Delete(file.FileName);
                }
              }
            }
          }
        }
        catch (Exception ex)
        {
          log.AuditError("Error reading file", ex);
        }
      }

    }

    private void ProcessNotifications(FtpManager manager, OrderResponseTypes responseType, IAuditLogAdapter log, Vendor vendor, string logPath, IUnitOfWork unit)
    {
      foreach (var file in manager)
      {
        try
        {
          using (MemoryStream fileStream = new MemoryStream(ReadFully(file.Data)))
          {

            string fileName = "VSN_" + responseType.ToString() + "_" + Guid.NewGuid() + ".csv";
            using (FileStream s = File.Create(Path.Combine(logPath, fileName)))
            {
              s.Write(fileStream.ToArray(), 0, (int)fileStream.Length);
            }

            int orderID = 0;
            var parser = GetCSVFile(fileStream, responseType);

            var groupedOrders = (from p in parser
                                 group p by p["CustomerReference"] into od
                                 select new
                                 {
                                   OrderID = od.Key,
                                   OrderInf = od.ToList()
                                 }).ToDictionary(x => x.OrderID, x => x.OrderInf);

            string vsnResponseType = responseType.ToString();
            foreach (var orderResp in groupedOrders)
            {
              int.TryParse(orderResp.Key, out orderID);

              var order = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetSingle(o => o.OrderID == orderID);

              OrderResponse response = null;

              if (order != null)
              {
                var responseHeader = orderResp.Value.FirstOrDefault();
                int orderLineCounter = 0;

                switch (responseType)
                {
                  case OrderResponseTypes.Acknowledgement:
                    #region Acknowledgement
                    response = new OrderResponse()
                   {
                     Currency = "EUR",
                     ResponseType = responseType.ToString(),
                     OrderID = orderID,
                     OrderDate = DateTime.ParseExact(responseHeader[VSNAcknowledgement.OrderDate.ToString()], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                     VendorDocumentNumber = responseHeader[VSNAcknowledgement.SalesOrderID.ToString()]
                   };

                    foreach (var line in orderResp.Value)
                    {
                      int lineReference = 0;
                      string referenceField = line[VSNAcknowledgement.CustomerLineReference.ToString()];
                      if (referenceField.Contains('/'))
                        int.TryParse(referenceField.Split('/')[0], out lineReference);
                      else
                        int.TryParse(referenceField, out lineReference);

                      //int.TryParse(line[VSNAcknowledgement.CustomerLineReference.ToString()], out lineReference);

                      OrderLine oLine = order.OrderLines.Where(x => x.OrderLineID == lineReference).FirstOrDefault();

                      if (oLine == null)
                      {
                        string vendorItemNumber = line[VSNAcknowledgement.EANNumber.ToString()];
                        oLine = order.OrderLines.Where(x => x.Product != null && x.Product.VendorItemNumber == vendorItemNumber).FirstOrDefault();

                      }

                      if (oLine != null && !oLine.OrderResponseLines.Any(x => x.OrderResponse.ResponseType == vsnResponseType))
                      {
                        OrderResponseLine rLine = new OrderResponseLine()
                        {
                          OrderResponse = response,
                          Backordered = StatusCodes(line[VSNAcknowledgement.StatusCode.ToString()]) == VSNStatusCode.Backorder ? int.Parse(line[VSNAcknowledgement.Quantity.ToString()]) : 0,
                          Cancelled = StatusCodes(line[VSNAcknowledgement.StatusCode.ToString()]) == VSNStatusCode.Canceled ? int.Parse(line[VSNAcknowledgement.Quantity.ToString()]) : 0,
                          Ordered = oLine.GetDispatchQuantity(),
                          Description = line[VSNAcknowledgement.ProductName.ToString()],
                          Invoiced = 0,
                          Barcode = line[VSNAcknowledgement.EANNumber.ToString()],
                          OrderLine = oLine,
                          Price = ((decimal)(oLine.Price.HasValue ? oLine.Price.Value : 0)),
                          Processed = false,
                          Shipped = StatusCodes(line[VSNAcknowledgement.StatusCode.ToString()]) == VSNStatusCode.Picklist ? int.Parse(line[VSNAcknowledgement.Quantity.ToString()]) : 0,
                          VendorItemNumber = line[VSNAcknowledgement.ProductCode.ToString()],
                          Remark = string.Format("ReleaseDate {0}", line[VSNAcknowledgement.ReleaseDate.ToString()])
                        };

                        unit.Scope.Repository<OrderResponseLine>().Add(rLine);
                        orderLineCounter++;
                      }
                      else
                        continue;
                    }
                    #endregion
                    break;
                  case OrderResponseTypes.CancelNotification:
                    #region CancelNotification
                    response = new OrderResponse()
                    {
                      Currency = "EUR",
                      ResponseType = responseType.ToString(),
                      OrderID = orderID,
                      OrderDate = DateTime.ParseExact(responseHeader[VSNCancel.OrderDate.ToString()], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                      VendorDocumentNumber = responseHeader[VSNCancel.SalesOrderID.ToString()],
                      VendorDocumentDate = DateTime.ParseExact(responseHeader[VSNCancel.Timestamp.ToString()], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture)
                    };

                    foreach (var line in orderResp.Value)
                    {
                      int lineReference = 0;
                      string referenceField = line[VSNCancel.CustomerLineReference.ToString()];
                      if (referenceField.Contains('/'))
                        int.TryParse(referenceField.Split('/')[0], out lineReference);
                      else
                        int.TryParse(referenceField, out lineReference);

                      OrderLine oLine = order.OrderLines.Where(x => x.OrderLineID == lineReference).FirstOrDefault();

                      if (oLine == null)
                      {
                        string vendorItemNumber = line[VSNAcknowledgement.EANNumber.ToString()];
                        oLine = order.OrderLines.Where(x => x.Product != null && x.Product.VendorItemNumber == vendorItemNumber).FirstOrDefault();

                      }

                      if (oLine != null && !oLine.OrderResponseLines.Any(x => x.OrderResponse.ResponseType == vsnResponseType))
                      {
                        OrderResponseLine rLine = new OrderResponseLine()
                        {
                          OrderResponse = response,
                          Backordered = 0,
                          Cancelled = int.Parse(line[VSNCancel.Quantity.ToString()]),
                          Ordered = oLine.GetDispatchQuantity(),
                          Description = line[VSNCancel.ProductName.ToString()],
                          Invoiced = 0,
                          Barcode = line[VSNCancel.EANNumber.ToString()],
                          OrderLine = oLine,
                          Price = ((decimal)(oLine.Price.HasValue ? oLine.Price.Value : 0)),
                          Processed = false,
                          Shipped = 0,
                          VendorItemNumber = line[VSNCancel.ProductCode.ToString()],
                          Remark = line[VSNCancel.SalesOrderLineCancelReason.ToString()]
                        };

                        unit.Scope.Repository<OrderResponseLine>().Add(rLine);
                        orderLineCounter++;
                      }
                      else
                        continue;
                    }
                    #endregion
                    break;
                  case OrderResponseTypes.ShipmentNotification:
                    #region ShipmentNotification
                    response = new OrderResponse()
                    {
                      Currency = "EUR",
                      ResponseType = responseType.ToString(),
                      OrderID = orderID,
                      OrderDate = DateTime.ParseExact(responseHeader[VSNShipment.OrderDate.ToString()], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                      VendorDocumentNumber = responseHeader[VSNShipment.SalesOrderID.ToString()],
                      ShippingNumber = responseHeader[VSNShipment.PackListID.ToString()],
                      ReqDeliveryDate = DateTime.ParseExact(responseHeader[VSNShipment.PacklistDate.ToString()], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture)
                    };

                    Customer cus = order.ShippedToCustomer;

                    if (cus.CustomerName != responseHeader[VSNShipment.AddressName.ToString()]
                      || cus.CustomerAddressLine1 != responseHeader[VSNShipment.Street.ToString()]
                      || cus.HouseNumber != responseHeader[VSNShipment.HouseNumber.ToString()]
                      || cus.PostCode.Replace(" ", "").ToUpper() != responseHeader[VSNShipment.ZipCode.ToString()].Replace(" ", "").ToUpper()
                      || cus.City.ToUpper() != responseHeader[VSNShipment.City.ToString()]
                      || (!string.IsNullOrEmpty(responseHeader[VSNShipment.CountryName.ToString()]) && cus.Country != responseHeader[VSNShipment.CountryName.ToString()]))
                      cus = new Customer()
                      {
                        CustomerName = responseHeader[VSNShipment.AddressName.ToString()],
                        CustomerAddressLine1 = responseHeader[VSNShipment.Street.ToString()],
                        HouseNumber = responseHeader[VSNShipment.HouseNumber.ToString()],
                        PostCode = responseHeader[VSNShipment.ZipCode.ToString()],
                        Country = responseHeader[VSNShipment.CountryName.ToString()],
                        City = responseHeader[VSNShipment.City.ToString()]
                      };

                    response.ShippedToCustomer = cus;

                    foreach (var line in orderResp.Value)
                    {
                      int lineReference = 0;
                      string referenceField = line[VSNShipment.CustomerLineReference.ToString()];
                      if (referenceField.Contains('/'))
                        int.TryParse(referenceField.Split('/')[0], out lineReference);
                      else
                        int.TryParse(referenceField, out lineReference);

                      OrderLine oLine = order.OrderLines.Where(x => x.OrderLineID == lineReference).FirstOrDefault();

                      if (oLine == null)
                      {
                        string vendorItemNumber = line[VSNAcknowledgement.EANNumber.ToString()];
                        oLine = order.OrderLines.Where(x => x.Product != null && x.Product.VendorItemNumber == vendorItemNumber).FirstOrDefault();

                      }

                      if (oLine != null && (!oLine.OrderResponseLines.Any(x => x.OrderResponse.ResponseType == vsnResponseType)
                        || (!string.IsNullOrEmpty(line[VSNShipment.LabelReference.ToString()]) && !oLine.OrderResponseLines.Any(x => x.OrderResponse.ResponseType == responseType.ToString() && !string.IsNullOrEmpty(x.TrackAndTrace)))))
                      {
                        OrderResponseLine rLine = new OrderResponseLine()
                   {
                     OrderResponse = response,
                     Backordered = 0,
                     Cancelled = 0,
                     Ordered = oLine.GetDispatchQuantity(),
                     Description = line[VSNShipment.ProductName.ToString()],
                     Invoiced = 0,
                     Barcode = line[VSNShipment.EANNumber.ToString()],
                     OrderLine = oLine,
                     Price = ((decimal)(oLine.Price.HasValue ? oLine.Price.Value : 0)),
                     Processed = false,
                     Shipped = int.Parse(line[VSNShipment.Quantity.ToString()]),
                     VendorItemNumber = line[VSNShipment.ProductCode.ToString()],
                     NumberOfUnits = line[VSNShipment.PackageType.ToString()] == "Doos" ? int.Parse(line[VSNShipment.PackageCount.ToString()]) : 0,
                     NumberOfPallets = line[VSNShipment.PackageType.ToString()] != "Doos" ? int.Parse(line[VSNShipment.PackageCount.ToString()]) : 0,
                     Unit = line[VSNShipment.PackageType.ToString()],
                     Remark = string.Format("ReleaseDate {0}", line[VSNShipment.ReleaseDate.ToString()]),
                     TrackAndTrace = line[VSNShipment.LabelReference.ToString()],
                     TrackAndTraceLink = string.IsNullOrEmpty(line[VSNShipment.LabelReference.ToString()]) ? string.Empty : BuildTrackAndTraceNumber(line[VSNShipment.LabelReference.ToString()], responseHeader[VSNShipment.ZipCode.ToString()].Replace(" ", "").ToUpper())
                   };

                        unit.Scope.Repository<OrderResponseLine>().Add(rLine);
                        orderLineCounter++;
                      }
                      else
                        continue;
                    }
                    #endregion
                    break;
                }

                if (orderLineCounter > 0)
                {
                  response.VendorID = vendor.VendorID;
                  response.ReceiveDate = DateTime.Now;
                  response.VendorDocument = parser.Document;
                  response.DocumentName = fileName;
                  if (!response.VendorDocumentDate.HasValue)
                    response.VendorDocumentDate = DateTime.Now;

                  response.ReceiveDate = DateTime.Now;

                  unit.Scope.Repository<OrderResponse>().Add(response);
                }
              }
            }


            unit.Save();
            manager.Delete(file.FileName);
          }
        }
        catch (Exception ex)
        {
          log.AuditError("Error reading file", ex);
        }
      }
    }

    private string BuildTrackAndTraceNumber(string trackAndTraceNumber, string zipCode)
    {
      return string.Format("https://secure.postplaza.nl/TPGApps/tracktrace/findByBarcodeServlet?BARCODE={0}&ZIPCODE={1}", trackAndTraceNumber, zipCode);
    }

    public static void LogFile(string path, string fileContents)
    {
      if (!path.EndsWith(@"\"))
        path += @"\";

      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      if (!path.EndsWith(@"\"))
        path += @"\";

      Guid g = Guid.NewGuid();

      File.WriteAllText(Path.Combine(path, "VSN_" + g.ToString() + ".xml"), fileContents);
    }

    #endregion

    private VSNStatusCode StatusCodes(string status)
    {
      Dictionary<string, VSNStatusCode> codes = new Dictionary<string, VSNStatusCode>();
      codes.Add("R", VSNStatusCode.Reserved);
      codes.Add("B", VSNStatusCode.Backorder);
      codes.Add("L", VSNStatusCode.Picklist);
      codes.Add("P", VSNStatusCode.Pakbon);
      codes.Add("H", VSNStatusCode.Hold);
      codes.Add("C", VSNStatusCode.Canceled);

      return codes[status];
    }
  }

  public enum VSNAcknowledgement
  {
    CustomerCode, SalesOrderID, OrderDate, ProductCode, EANNumber, ProductName, ReleaseDate, CustomerReference, CustomerLineReference, Quantity, StatusCode
  }

  public enum VSNShipment
  {
    CustomerCode, OrderCustomerCode, SalesOrderID, OrderDate, PackListID, PacklistDate, AddressName, Street, HouseNumber, ZipCode, City, CountryName, PackageCount, PackageType, ProductCode, EANNumber, ProductName, ReleaseDate, CustomerReference, CustomerLineReference, Quantity, LabelReference
  }

  public enum VSNInvoice
  {
    RecordType, CustomerCode, SalesInvoiceID, InvoiceDate, VATNumber, PatymentConditionName, EANNumberAddress, PacklistID, PacklistDate, DeliveryMethodName, CustomerReference, EANNumberProduct, Quantity, Price, DiscountPercentage, PriceDiscountIncluded, VATPercentage, VatCodeID, LineTotal, LineCustomerReference
  }

  public enum VSNInvoiceTotal
  {
    RecordType, SalesInvoiceTotalID, SalesInvoiceTotal, SalesInvoiceID, AmountVATExcluded, VATCodeID, VATPercentage, AmountVAT, AmountVATIncluded
  }

  public enum VSNInvoiceGrandTotal
  {
    RecordType, AmountVATExcluded, AmountVAT, AmountVATIncluded
  }

  public enum VSNCancel
  {
    CustomerCode, OrderCustomerCode, SalesOrderID, OrderDate, ProductCode, EANNumber, ProductName, ReleaseDate, CustomerReference, CustomerLineReference, Quantity, SalesOrderLineCancelReasonID, SalesOrderLineCancelReason, Timestamp

  }

  public enum VSNStatusCode
  {
    Reserved, Backorder, Picklist, Pakbon, Hold, Canceled
  }
}
