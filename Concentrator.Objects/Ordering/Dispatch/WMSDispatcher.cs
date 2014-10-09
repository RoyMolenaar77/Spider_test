using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using AuditLog4Net.Adapter;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using System.Text;
using Concentrator.Plugins.PFA.Configuration;

namespace Concentrator.Objects.Ordering.Dispatch
{
  public class WMSDispatcher : IDispatchable
  {
    public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor, IAuditLogAdapter log, IUnitOfWork unit)
    {
      try
      {
        System.Xml.Linq.XDocument xml = new XDocument();
        List<XElement> orderElements = new List<XElement>();

        foreach (var order in orderLines.Keys)
        {
          var shipto = order.ShippedToCustomer ?? order.SoldToCustomer;
          var soldto = order.SoldToCustomer ?? order.ShippedToCustomer;

          orderElements.Add(new XElement("OrderInfo",
                          new XElement("OrderID", order.OrderID),
                          new XElement("ShipmentMethod", order.RouteCode),

                          new XElement("ShipTo",
                            new XElement("CompanyName", shipto.CompanyName ?? string.Empty),
                            new XElement("Name", string.IsNullOrEmpty(shipto.ServicePointName) ? shipto.CustomerName : shipto.ServicePointName),
                            new XElement("Address1", shipto.CustomerAddressLine1),
                            new XElement("Address2", shipto.CustomerAddressLine2 ?? string.Empty),
                            new XElement("Address3", shipto.CustomerAddressLine3 ?? string.Empty),
                            new XElement("Street", shipto.Street),
                            new XElement("Number", shipto.HouseNumber ?? string.Empty),
                            new XElement("Extension", shipto.HouseNumberExt ?? string.Empty),
                            new XElement("ZipCode", shipto.PostCode),
                            new XElement("City", shipto.City),
                            new XElement("Country", !string.IsNullOrEmpty(order.OrderLanguageCode) ? order.OrderLanguageCode : "NL-NL"),
                            new XElement("Email", shipto.CustomerEmail ?? string.Empty),
                            new XElement("Phone", shipto.CustomerTelephone ?? string.Empty),
                            new XElement("KPID", shipto.ServicePointID ?? string.Empty)),
                        new XElement("SoldTo",
                            new XElement("CompanyName", soldto.CompanyName ?? string.Empty),
                            new XElement("Name", soldto.CustomerName),
                            new XElement("Address1", soldto.CustomerAddressLine1),
                            new XElement("Address2", soldto.CustomerAddressLine2 ?? string.Empty),
                            new XElement("Address3", soldto.CustomerAddressLine3 ?? string.Empty),
                            new XElement("Street", soldto.Street ?? string.Empty),
                            new XElement("Number", soldto.HouseNumber ?? string.Empty),
                            new XElement("Extension", soldto.HouseNumberExt ?? string.Empty),
                            new XElement("ZipCode", soldto.PostCode),
                            new XElement("City", soldto.City),
                            new XElement("Country", !string.IsNullOrEmpty(order.OrderLanguageCode) ? order.OrderLanguageCode : "NL-NL"),
                            new XElement("Email", soldto.CustomerEmail ?? string.Empty),
                            new XElement("Phone", soldto.CustomerTelephone ?? string.Empty)),
                        new XElement("CustomerReference", order.CustomerOrderReference ?? string.Empty),
            new XElement("Reference", order.OrderID),
            new XElement("WebsiteOrderNumber", order.WebSiteOrderNumber),
            new XElement("PaymentMethod", order.PaymentTermsCode ?? string.Empty),
                          new XElement("Details",
              (from line in orderLines[order]
               let vendorAssortment = line.Product.VendorAssortments.FirstOrDefault(c => c.VendorID == vendor.VendorID)
               let vendorPrice = vendorAssortment != null ? vendorAssortment.VendorPrices.FirstOrDefault(x => x.MinimumQuantity >= line.GetDispatchQuantity()) : null
               let vendorVatRate = vendorAssortment.Try(c => c.VendorPrices.FirstOrDefault().TaxRate, 21)
               let priceWithDiscount = line.Price - (line.LineDiscount.Try(c => c.Value, 0))
               select new XElement("Detail",
                 new XElement("LineID", line.OrderLineID),
                 new XElement("ProductID", line.ProductID),
                 new XElement("VendorItemNumber", line.Product.VendorItemNumber),
                 new XElement("Quantity", line.GetDispatchQuantity()),
                 new XElement("Discount", (line.LineDiscount ?? 0)),
            new XElement("UnitPrice", Math.Max(priceWithDiscount ?? 0, 0)), //in case of shipping costs
          new XElement("VatRate", Convert.ToInt32(vendorVatRate))
                )
            ))
               ));
        }

        XElement element = new XElement("Orders", orderElements.ToArray());
        xml.Add(element);

        var path = vendor.VendorSettings.GetValueByKey("OrderPath", string.Empty);

#if DEBUG
        path = @"D:\Concentrator\OUT\ORDER";
#endif
        if (string.IsNullOrEmpty(path))
          throw new Exception("Empty order path for " + vendor.Name);

        var orderID = orderLines.Keys.FirstOrDefault().OrderID;
        string fileName = string.Format("{0}.xml", orderID == 0 ? Guid.NewGuid().ToString() : orderID.ToString());

        xml.Save(Path.Combine(path, fileName));

        LogOrder(xml, vendor.VendorID, fileName, log);

        return -1;
      }
      catch (Exception e)
      {
        throw new Exception("WMS dispatching failed", e);
      }
    }

    public void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, string logPath, IUnitOfWork unit)
    {
      var orderStatusPath = vendor.VendorSettings.GetValueByKey("OrderStatus", string.Empty);
      var ccShipmentCostsProduct = PfaCoolcatConfiguration.Current.ShipmentCostsProduct;
      var ccReturnCostsProduct = PfaCoolcatConfiguration.Current.ReturnCostsProduct;
      var ccKialaShipmentCostsProduct = PfaCoolcatConfiguration.Current.KialaShipmentCostsProduct;
      var ccKialaReturnCostsProduct = PfaCoolcatConfiguration.Current.KialaReturnCostsProduct;
      var connectorIdOfVendorOrders = vendor.VendorSettings.GetValueByKey<int>("RelatedConnectorID", 0);

      if (connectorIdOfVendorOrders == 0) throw new Exception("RelatedConnectorID for this vendor has not been set");

#if DEBUG
      orderStatusPath = @"D:\tmp\cc\out";
#endif

      if (string.IsNullOrEmpty(orderStatusPath))
        throw new Exception("Empty order status path for " + vendor.Name);

      Directory.GetFiles(orderStatusPath).Where(x => x.EndsWith("XML", StringComparison.InvariantCultureIgnoreCase)).ForEach((file, idx) =>
      {
        try
        {
          XDocument XmlDocument = XDocument.Parse(System.IO.File.ReadAllText(file));

          Dictionary<string, string> statuses = new Dictionary<string, string>();
          statuses.Add("Aborted", OrderResponseTypes.CancelNotification.ToString());
          statuses.Add("Cancel", OrderResponseTypes.CancelNotification.ToString());
          statuses.Add("Returned", OrderResponseTypes.Return.ToString());
          statuses.Add("Shipped", OrderResponseTypes.ShipmentNotification.ToString());
          statuses.Add("Imported", OrderResponseTypes.Acknowledgement.ToString());

          var resp = (from r in XmlDocument.Root.Elements("Status")
                      group r by new { orderID = r.Element("OrderID").Value, status = r.Element("Status").Value } into grouped
                      let order = unit.Scope.Repository<Order>().GetSingle(x => x.WebSiteOrderNumber == grouped.Key.orderID && x.ConnectorID == connectorIdOfVendorOrders)
                      where order != null
                      select new Concentrator.Objects.Models.Orders.OrderResponse()
                     {
                       ResponseType = statuses.ContainsKey(grouped.Key.status) ? statuses[grouped.Key.status] : grouped.Key.status,
                       Vendor = vendor,
                       ReceiveDate = DateTime.Now.ToUniversalTime(),
                       Order = order,
                       VendorDocument = Path.GetFileName(file),// XmlDocument.ToString(),
                       VendorDocumentNumber = grouped.Key.status,
                       OrderResponseLines = (from l in XmlDocument.Root.Elements("Status").Where(c => c.Element("OrderID").Value == grouped.Key.orderID && c.Element("Status").Value == grouped.Key.status).GroupBy(c => c.Element("ProductID").Value) //grouped.GroupBy(c => c.Element("ProductID"))
                                             let line = l.First()
                                             let lineID = string.IsNullOrEmpty(line.Element("LineID").Value) ? 0 : int.Parse(line.Element("LineID").Value)
                                             let OrderLine = lineID == 0 ? null : unit.Scope.Repository<OrderLine>().GetSingle(x => x.OrderLineID == lineID)
                                             let quantity = l.Sum(c => int.Parse(c.Element("Quantity").Value))
                                             select new OrderResponseLine()
                                             {
                                               OrderLineID = lineID,
                                               Ordered = OrderLine == null ? 1 : OrderLine.GetDispatchQuantity(),
                                               Backordered = grouped.Key.status == "Backorder" ? quantity : 0,
                                               Cancelled = (grouped.Key.status == "Aborted" || grouped.Key.status == "Cancel") ? quantity : 0,
                                               Shipped = grouped.Key.status == "Shipped" ? quantity : 0,
                                               Remark = grouped.Key.status,
                                               Invoiced = 0,
                                               Delivered = quantity,
                                               Price = 0,
                                               TrackAndTrace = line.Element("Reference").Value,
                                               VendorItemNumber = line.Element("ProductID").Value
                                             }).Distinct().ToList()
                     });


          if (resp.Count() == 0) //found files but not for this vendor
          {
            return;
          }

          resp.ForEach((response, ridx) =>
          {

            var linesWithoutOrderLine = response.OrderResponseLines.Where(c => c.OrderLineID == 0 || c.VendorItemNumber == ccReturnCostsProduct || c.VendorItemNumber == ccKialaReturnCostsProduct).ToList();

            if (linesWithoutOrderLine.Count() != 0)
            {
              foreach (var orderLine in linesWithoutOrderLine)
              {
                //add those orderlines
                var order = unit.Scope.Repository<Order>().GetSingle(c => c.WebSiteOrderNumber == response.Order.WebSiteOrderNumber);
                var existingOline = unit.Scope.Repository<OrderLine>().GetSingle(c => c.OrderID == order.OrderID && c.Product.VendorItemNumber == orderLine.VendorItemNumber);
                var product = unit.Scope.Repository<Product>().GetSingle(c => c.VendorItemNumber == orderLine.VendorItemNumber);
                var price = product.VendorAssortments.FirstOrDefault(c => c.VendorID == vendor.VendorID).VendorPrices.FirstOrDefault(); //take default price if this scenario occurs

                if (orderLine.VendorItemNumber == ccReturnCostsProduct || orderLine.VendorItemNumber == ccKialaReturnCostsProduct)
                {
                  existingOline = new OrderLine()
                  {
                    Order = order,
                    CustomerItemNumber = orderLine.VendorItemNumber,
                    ProductID = product.ProductID,
                    Quantity = 1,
                    BasePrice = (double)price.Price.Try(c => c.Value, 0),
                    UnitPrice = (double)price.Price.Try(c => c.Value, 0),
                    LineDiscount = 0,
                    isDispatched = true,
                    Price = (double)price.Price.Try(c => c.Value, 0)
                  };

                  unit.Scope.Repository<OrderLine>().Add(existingOline);
                  unit.Save();
                }
                else
                {
                  if (existingOline == null)
                  {
                    existingOline = new OrderLine()
                    {
                      Order = order,
                      CustomerItemNumber = orderLine.VendorItemNumber,
                      ProductID = product.ProductID,
                      Quantity = 1,
                      BasePrice = (double)price.Price.Try(c => c.Value, 0),
                      UnitPrice = (double)price.Price.Try(c => c.Value, 0),
                      LineDiscount = 0,
                      isDispatched = true,
                      Price = (double)price.Price.Try(c => c.Value, 0)
                    };

                    unit.Scope.Repository<OrderLine>().Add(existingOline);
                    unit.Save();
                  }
                }
                orderLine.OrderLineID = existingOline.OrderLineID;
              }
            }

            unit.Scope.Repository<Concentrator.Objects.Models.Orders.OrderResponse>().Add(response);
            unit.Save();
          });

          FileInfo inf = new FileInfo(file);
          var processed = Path.Combine(orderStatusPath, "Processed");

          if (!Directory.Exists(processed))
            Directory.CreateDirectory(processed);

          var path = Path.Combine(processed, inf.Name);

          if (File.Exists(path))
          {
            File.Delete(path);
          }

          File.Move(file, path);
        }
        catch (Exception ex)
        {
          FileInfo inf = new FileInfo(file);

          String path = Path.Combine(orderStatusPath, inf.FullName);

          File.Move(path, Path.ChangeExtension(path, ".xml.err"));

          using (FileStream logFile = File.Create(Path.Combine(Path.ChangeExtension(path, ".log"))))
          {
            String text = ex.Message;

            using (StreamWriter writer = new StreamWriter(logFile, Encoding.UTF8))
            {
              writer.WriteLine("Error processing dispatch for vendor: " + vendor.Name);
              writer.WriteLine(String.Empty);
              writer.WriteLine(text);
            }
          }

          log.AuditError("Error process dispatch " + vendor.Name, ex);
        }
      });
    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }

    public void LogOrder(object orderInformation, int vendorID, string fileName, IAuditLogAdapter log)
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
  }
}
