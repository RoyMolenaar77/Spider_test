using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using Concentrator.Objects.Ordering.XmlClasses;
using System.Collections.Specialized;
using Concentrator.Objects.ConcentratorService;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.Web;
using Concentrator.Objects.Ftp;
using System.Xml.Linq;
using Concentrator.Objects.Models.Orders;
using System.Globalization;
using System.Configuration;

namespace Concentrator.Objects.Ordering.Dispatch
{
  class CopacoDispatcher : IDispatchable
  {
    public enum CopacoStatusCodes
    {
      AlreadySent = 010,
      Atexpedition = 030,
      AtICTservices = 050,
      Cancelled = 090,
      AppointedStock = 100,
      ConfirmedXXXXX = 200,
      Expectedsupplier = 300,
      ExpectedbyXXXXX = 400,
      Unknowndelivery500,
      IndicationXXXXX = 600,
      Outofstock = 700,
      Notavailableyet = 800

    }

    public int DispatchOrders(Dictionary<Models.Orders.Order, List<Models.Orders.OrderLine>> orderLines, Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, DataAccess.UnitOfWork.IUnitOfWork uni)
    {

      var CustNo = vendor.VendorSettings.GetValueByKey("CopacoCustomerNo", string.Empty);
      var SenderID = vendor.VendorSettings.GetValueByKey("CopacoSenderID", string.Empty);
      var FtpUrl = vendor.VendorSettings.GetValueByKey("FtpUrl", string.Empty);
      var FtpUserName = vendor.VendorSettings.GetValueByKey("Username", string.Empty);
      var FtpPassword = vendor.VendorSettings.GetValueByKey("Password", string.Empty);
      var UploadDirectory = vendor.VendorSettings.GetValueByKey("UploadDirectory", string.Empty);

      Concentrator.Objects.Ordering.XmlClasses.Customer customer = new Concentrator.Objects.Ordering.XmlClasses.Customer();
      customer.customerid = CustNo;

      foreach (var order in orderLines.Keys)
      {
        int count = 0;

        //new order
        XML_order orderxml = new XML_order();
        orderxml.documentsource = "order_in";
        orderxml.external_document_id = order.OrderID.ToString();
        orderxml.supplier = "COPACO";

        //order header with customer and shipping info
        orderheader header = new orderheader();
        header.Customer = customer; // set customer
        header.completedelivery = "N";
        header.requested_deliverydate = "";
        header.customer_ordernumber = order.OrderID.ToString();
        header.sender_id = SenderID;
        header.testflag = "Y";
        header.recipientsreference = order.CustomerOrderReference;


        // shipping info
        ShipTo ship = new ShipTo();
        adress adress = new adress();
        adress.name1 = order.ShippedToCustomer.CustomerName;


        //overige ook toevoegen?
        adress.street = order.ShippedToCustomer.CustomerAddressLine1 + " " + order.ShippedToCustomer.HouseNumber;
        adress.postalcode = order.ShippedToCustomer.PostCode.Substring(0, 4) + " " + order.ShippedToCustomer.PostCode.Substring(4);
        adress.city = order.ShippedToCustomer.City;
        adress.country = order.ShippedToCustomer.Country;
        ship.Items = new adress[1];
        ship.ItemsElementName = new ItemsChoiceType[1];
        ship.ItemsElementName[0] = ItemsChoiceType.adress;
        ship.Items[0] = adress;

        #region future use

        //notification notification = new notification();
        //ordertext ordertext = new ordertext();
        //License_data license_data = new License_data();
        //end_user_contact end_user_contact = new end_user_contact();

        //header.notification = notification;
        //header.ordertext = new ordertext[1];
        //header.ordertext[0] = ordertext;
        //header.License_data = license_data;
        //header.License_data.end_user_contact = end_user_contact;
        #endregion

        //orderline array
        orderxml.orderline = new orderline[order.OrderLines.Count];

        //add basic info to xml
        orderxml.orderheader = header;
        orderxml.orderheader.ShipTo = ship;


        foreach (var itemOrder in order.OrderLines)
        {
          //add orderline
          orderline orderline = new orderline();
          orderline.linenumber = itemOrder.OrderLineID.ToString();
          orderline.item_id = new item_id[1];
          orderline.item_id[0] = new item_id();
          orderline.item_id[0].tag = "PN";
          orderline.item_id[0].Value = itemOrder.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == vendor.VendorID).CustomItemNumber;
          orderline.quantity = new quantity();
          orderline.quantity.unit = "ST";
          orderline.quantity.Value = itemOrder.GetDispatchQuantity().ToString();
          //orderline.deliverydate = "";//DateTime.Today.AddDays(1).ToString("DD-MM-YYYY");
          orderline.price = new price();
          orderline.price.currency = "EUR";
          orderline.price.Value = itemOrder.Price.Value.ToString().Replace(',', '.');
          orderxml.orderline[count] = orderline;

          count++;
        }

        string fileName = string.Format("Order_{0}.xml", order.OrderID.ToString());
        StringBuilder requestString = new StringBuilder();
        XmlWriterSettings settings = new XmlWriterSettings();
        settings.Encoding = Encoding.UTF8;
        using (XmlWriter xw = XmlWriter.Create(requestString, settings))
        {
          xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\"");
          XmlSerializer serializer = new XmlSerializer(orderxml.GetType());
          XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
          nm.Add("", "");
          serializer.Serialize(xw, orderxml, nm);

          XmlDocument xml = new XmlDocument();
          xml.LoadXml(requestString.ToString());
          xml.DocumentElement.RemoveAttribute("xmlns:xsi");
          xml.DocumentElement.RemoveAttribute("xmlns:xsd");

          System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

          byte[] file = encoding.GetBytes(xml.InnerXml.ToString());

          SimpleFtp ftp = new SimpleFtp(FtpUrl, FtpUserName, FtpPassword, log, true);
          ftp.UploadFile(file, fileName, UploadDirectory);

          LogOrder(xml, vendor.VendorID, fileName, log);
        }
      }

      return 0;
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


    public void GetAvailableDispatchAdvices(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath, DataAccess.UnitOfWork.IUnitOfWork unit)
    {

      var FtpUrl = vendor.VendorSettings.GetValueByKey("FtpUrl", string.Empty);
      var FtpUserName = vendor.VendorSettings.GetValueByKey("DispUsername", string.Empty);
      var FtpPassword = vendor.VendorSettings.GetValueByKey("DispPassword", string.Empty);
      var DownloadDirectory = vendor.VendorSettings.GetValueByKey("DownloadDirectory", string.Empty);
      var vendorID = vendor.VendorID;

      OrderResponseTypes responseType = new OrderResponseTypes();

      FtpManager ftp = new FtpManager(
      FtpUrl,
       DownloadDirectory,
       FtpUserName,
       FtpPassword,
      false, true, log);

      //SimpleFtp ftp = new SimpleFtp(FtpUrl, FtpUserName, FtpPassword, log, true);

      //var files = ftp.AsEnumerable().ToList();

      foreach (var file in ftp)
      {

        if (file.FileName.Contains("CNOBV") || file.FileName.Contains("CNFAC") || file.FileName.Contains("CNPAK"))
        {
          OrderResponse orderresponse = null;

          switch (file.FileName.Substring(0, 5))
          {
            case "CNPAK":
              responseType = OrderResponseTypes.ShipmentNotification;
              break;
            case "CNFAC":
              responseType = OrderResponseTypes.InvoiceNotification;
              break;
            case "CNOBV":
              responseType = OrderResponseTypes.Acknowledgement;
              break;
          }

          using (var stream = ftp.OpenFile(file.FileName))
          {
            XDocument xml = XDocument.Load(stream.Data);
            List<OrderProcess> details = null;


            //parse
            if (responseType == OrderResponseTypes.Acknowledgement)
            {
              details = (from d in xml.Element("orderresponses").Elements("orderconfirmation")
                         select new OrderProcess
                         {
                           OrderID = d.Attribute("external_document_id").Value,
                           VendorDocNr = d.Element("orderheader").Attribute("order_number").Value,
                           Currency = d.Element("orderheader").Attribute("currency").Value,
                           ShipToName = d.Element("ShipTo").Element("name1").Value,
                           ShipToStreet = d.Element("ShipTo").Element("street").Value,
                           ShipToPostalCode = d.Element("ShipTo").Element("postalcode").Value,
                           ShipToCity = d.Element("ShipTo").Element("city").Value,
                           ShipToCountry = d.Element("ShipTo").Element("country").Value,
                           Tax = d.Element("VAT").Element("amount").Value,
                           Date = d.Element("orderheader").Attribute("orderdate").Value,
                           Costs = (from c in d.Elements("costs")
                                    select new OrderCost
                                    {
                                      Description = c.Element("description").Value,
                                      Amount = c.Element("amount").Value
                                    }).ToList(),
                           OrderLines = (from o in d.Elements("orderline")
                                         select new OrderProcessLine
                                         {
                                           VendorLineNr = o.Attribute("linenumber").Value,
                                           OrderlineID = o.Attribute("customer_linenumber").Value, //remove 0000's
                                           VendorItemNr = o.Attribute("item_id").Value,
                                           Description = o.Attribute("item_description").Value,
                                           Price = o.Attribute("price").Value,
                                           OEM = o.Attribute("manufacturer_item_id").Value,
                                           Ordered = o.Attribute("quantity_ordered").Value,
                                           StatusCode = o.Element("schedulelines").Element("atp_code").Value,
                                           StatusDate = o.Element("schedulelines").Element("atp_date").Value,
                                           StatusQuantity = o.Element("schedulelines").Element("quantity").Value
                                         }).ToList(),
                           TotalPrice = d.Element("ordertrailer").Element("order_amount_incl_VAT").Value
                         }).ToList();
            }

            if (responseType == OrderResponseTypes.InvoiceNotification)
            {
              try
              {
                details = (from d in xml.Element("orderresponses").Elements("invoice")
                           select new OrderProcess
                           {
                             OrderID = d.Element("invoiceline").Element("customerorder").Element("customer_ordernumber").Value,
                             InvoiceNumber = d.Element("invoiceheader").Element("invoice_number").Value,
                             InvoiceDate = d.Element("invoiceheader").Element("invoice_date").Value,
                             ShipToName = d.Element("invoiceline").Element("invoiceorder").Element("ShipTo").Element("name1").Value,
                             ShipToStreet = d.Element("invoiceline").Element("invoiceorder").Element("ShipTo").Element("street").Value,
                             ShipToPostalCode = d.Element("invoiceline").Element("invoiceorder").Element("ShipTo").Element("postalcode").Value,
                             ShipToCity = d.Element("invoiceline").Element("invoiceorder").Element("ShipTo").Element("city").Value,
                             ShipToCountry = d.Element("invoiceline").Element("invoiceorder").Element("ShipTo").Element("country").Value,
                             VendorDocNr = d.Element("invoiceline").Element("invoiceorder").Element("ordernumber").Value,
                             Currency = d.Element("invoiceheader").Element("invoice_currency").Value,
                             TrackingNumber = d.Element("invoiceheader").Element("TrackingNumber").Value,
                             TotalExVat = d.Element("invoicetrailer").Element("invoice_amount_ex_VAT").Value,
                             VatAmount = d.Element("invoicetrailer").Element("invoice_VAT_amount").Value,
                             //PaymentDays = d.Element("invoiceheader").Element("invoice_terms_of_payment_text").Value.Substring(16, 2), //get number: ex Betaling binnen 30 dagen netto
                             OrderDate = d.Element("invoiceline").Element("invoiceorder").Element("orderdate").Value,
                             Costs = (from c in d.Element("invoicetrailer").Elements("costs")
                                      select new OrderCost
                                      {
                                        Description = c.Element("description").Value,
                                        Amount = c.Element("amount").Value
                                      }).ToList(),
                             OrderLines = (from o in d.Elements("invoiceline")
                                           select new OrderProcessLine
                                           {
                                             VendorLineNr = o.Element("invoiceorder").Element("linenumber").Value,
                                             OrderlineID = o.Element("customerorder").Element("customer_ordernumber").Value, //remove 0000's 
                                             VendorItemNr = o.Element("invoice_item").Element("item_id").Value,
                                             Description = o.Element("invoice_item").Element("item_description").Value,
                                             Price = o.Element("invoice_item").Element("price").Value,
                                             OEM = o.Element("invoice_item").Element("manufacturer_item_id").Value,
                                             Ordered = o.Element("invoice_item").Element("quantity_ordered").Value,
                                             Invoiced = o.Element("invoice_item").Element("quantity_invoiced").Value,
                                             OrderDate = o.Element("invoiceorder").Element("orderdate").Value,
                                             VATPercentage = o.Element("invoice_item").Element("item_vat").Element("percentage").Value,
                                             VATAmount = o.Element("invoice_item").Element("item_vat").Element("amount").Value

                                           }).ToList(),
                             TotalPrice = d.Element("invoicetrailer").Element("invoice_amount_incl_VAT").Value
                           }).ToList();
              }
              catch (Exception)
              {
              }
            }

            else if (responseType == OrderResponseTypes.ShipmentNotification)
            {
              details = (from d in xml.Element("orderresponses").Elements("dispatchadvice")
                         select new OrderProcess
                         {
                           OrderID = d.Element("dispatchline").Element("customerorder").Element("customer_ordernumber").Value,
                           ReceiveDate = d.Element("dispatchheader").Element("dispatchdate").Value,
                           ShipmentNumber = d.Element("dispatchheader").Element("dispatchnumber").Value,
                           ShipmentDate = d.Element("dispatchheader").Element("dispatchdate").Value,
                           ShipToName = d.Element("ShipTo").Element("name1").Value,
                           ShipToStreet = d.Element("ShipTo").Element("street").Value,
                           ShipToPostalCode = d.Element("ShipTo").Element("postalcode").Value,
                           ShipToCity = d.Element("ShipTo").Element("city").Value,
                           ShipToCountry = d.Element("ShipTo").Element("country").Value,
                           VendorDocNr = d.Element("dispatchline").Element("order").Element("ordernumber").Value,
                           TrackingNumbers = (from nr in d.Element("dispatchline").Element("tracking_numbers").Elements("trackingnumber")
                                              select nr.Value).ToList(),
                           OrderLines = (from o in d.Elements("dispatchline")
                                         select new OrderProcessLine
                                         {
                                           VendorLineNr = o.Element("order").Element("linenumber").Value,
                                           OrderDate = o.Element("order").Element("orderdate").Value,
                                           OrderlineID = o.Element("customerorder").Element("customer_ordernumber").Value, //remove 0000's 
                                           VendorItemNr = o.Element("item").Element("item_id").Value,
                                           Description = o.Element("item").Element("item_description").Value,
                                           OEM = o.Element("item").Element("manufacturer_item_id").Value,
                                           Shipped = o.Element("item").Element("quantity").Value,
                                         }).ToList(),
                           TotalShipped = d.Element("dispatchtrailer").Element("total_number_of_units").Value
                         }).ToList();
            }

            var Order = details.FirstOrDefault();
            int OrderID = 0;

            Int32.TryParse(Order.OrderID, out OrderID);

            var orderInDb = unit.Scope.Repository<Order>().GetSingle(x => x.OrderID == OrderID);

            var orderResponseLines = new List<OrderResponseLine>();

            if (orderInDb != null)
            {
              string VendorDocument = "";

              StringBuilder builder = new StringBuilder();
              using (TextWriter writer = new StringWriter(builder))
              {
                xml.Save(writer);
              }

              VendorDocument = builder.ToString();

              if (responseType == OrderResponseTypes.Acknowledgement)
              {
                orderresponse = new OrderResponse()
                 {
                   VendorID = vendorID,
                   OrderID = Int32.Parse(Order.OrderID),
                   ResponseType = responseType.ToString(),
                   AdministrationCost = decimal.Parse(Order.Costs.FirstOrDefault(x => x.Description == "Handlingkosten" || x.Description == "Handlingskosten").Amount, CultureInfo.InvariantCulture),
                   DropShipmentCost = decimal.Parse(Order.Costs.FirstOrDefault(x => x.Description == "Dropshipmentkosten").Amount, CultureInfo.InvariantCulture),
                   OrderDate = DateTime.Parse(Order.Date),
                   VendorDocumentNumber = Order.VendorDocNr,
                   VendorDocument = VendorDocument,
                   ReceiveDate = DateTime.Now
                 };

                decimal TotalAmount;

                decimal.TryParse(Order.TotalPrice, NumberStyles.Any, CultureInfo.InvariantCulture, out TotalAmount);


                orderresponse.TotalAmount = TotalAmount;
                orderresponse.ShippedToCustomer = new Models.Orders.Customer();

                orderresponse.ShippedToCustomer.City = Order.Try(x => x.ShipToCity, null);
                orderresponse.ShippedToCustomer.CustomerAddressLine1 = Order.Try(x => x.ShipToStreet, null);
                orderresponse.ShippedToCustomer.PostCode = Order.Try(x => x.ShipToPostalCode, null);
                orderresponse.ShippedToCustomer.Country = Order.Try(x => x.ShipToCountry, null);
                orderresponse.ShippedToCustomer.CustomerName = Order.Try(x => x.ShipToName, null);


                foreach (var orderline in Order.OrderLines)
                {
                  var orderLine = orderInDb.OrderLines.FirstOrDefault(x => x.OrderLineID == Int32.Parse(orderline.OrderlineID));
                  if (orderLine != null)
                  {
                    var OrderResponseLine = new OrderResponseLine
                    {
                      OrderResponse = orderresponse,
                      VendorLineNumber = orderline.VendorLineNr,
                      VendorItemNumber = orderline.VendorItemNr,
                      Description = orderline.Description,
                      Ordered = 0,
                      Backordered = 0,
                      Invoiced = 0,
                      Shipped = 0,
                      Cancelled = 0,
                      Processed = false,
                      RequestDate = DateTime.Now //aanpassen indien word meegeven in orderplacement
                    };

                    decimal Price;
                    int OrderlineID;
                    int Ordered;

                    int.TryParse(orderline.OrderlineID, out OrderlineID);
                    decimal.TryParse(orderline.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out Price);
                    int.TryParse(orderline.Ordered, NumberStyles.Any, CultureInfo.InvariantCulture, out Ordered);

                    OrderResponseLine.Price = Price;
                    OrderResponseLine.OrderLineID = OrderlineID;
                    OrderResponseLine.Ordered = Ordered;

                    //check if backordered
                    if (Int32.Parse(orderline.StatusCode) == (int)CopacoStatusCodes.ExpectedbyXXXXX ||
                      Int32.Parse(orderline.StatusCode) == (int)CopacoStatusCodes.Expectedsupplier ||
                      Int32.Parse(orderline.StatusCode) == (int)CopacoStatusCodes.ExpectedbyXXXXX ||
                      Int32.Parse(orderline.StatusCode) == (int)CopacoStatusCodes.Unknowndelivery500 ||
                      Int32.Parse(orderline.StatusCode) == (int)CopacoStatusCodes.IndicationXXXXX ||
                      Int32.Parse(orderline.StatusCode) == (int)CopacoStatusCodes.Outofstock ||
                      Int32.Parse(orderline.StatusCode) == (int)CopacoStatusCodes.Notavailableyet)
                    {
                      int StatusQuantity;

                      int.TryParse(orderline.StatusQuantity, NumberStyles.Any, CultureInfo.InvariantCulture, out StatusQuantity);
                      OrderResponseLine.Backordered = StatusQuantity;
                    }
                    orderResponseLines.Add(OrderResponseLine);
                  }
                }

              }
              else
                if (responseType == OrderResponseTypes.ShipmentNotification)
                {
                  orderresponse = new OrderResponse
                  {
                    OrderID = Int32.Parse(Order.OrderID),
                    ResponseType = responseType.ToString(),
                    //ReceiveDate = DateTime.Parse(Order.ReceiveDate),
                    VendorDocumentNumber = Order.VendorDocNr,
                    TrackAndTrace = Order.TrackingNumbers.FirstOrDefault(),
                    VendorDocument = VendorDocument
                  };

                  DateTime ReceiveDate;

                  DateTime.TryParseExact(Order.ReceiveDate, "yyyyMMdd", null, DateTimeStyles.None, out ReceiveDate);

                  decimal TotalShipped;

                  decimal.TryParse(Order.TotalShipped, NumberStyles.Any, CultureInfo.InvariantCulture, out TotalShipped);

                  orderresponse.ReceiveDate = ReceiveDate;
                  orderresponse.TotalGoods = TotalShipped;
                  orderresponse.ShippedToCustomer = new Models.Orders.Customer();

                  orderresponse.ShippedToCustomer.City = Order.Try(x => x.ShipToCity, null);
                  orderresponse.ShippedToCustomer.CustomerAddressLine1 = Order.Try(x => x.ShipToStreet, null);
                  orderresponse.ShippedToCustomer.PostCode = Order.Try(x => x.ShipToPostalCode, null);
                  orderresponse.ShippedToCustomer.Country = Order.Try(x => x.ShipToCountry, null);
                  orderresponse.ShippedToCustomer.CustomerName = Order.Try(x => x.ShipToName, null);

                  foreach (var orderline in Order.OrderLines)
                  {
                    var orderLine = orderInDb.OrderLines.FirstOrDefault(x => x.OrderLineID == Int32.Parse(orderline.OrderlineID));
                    if (orderLine != null)
                    {
                      var OrderResponseLine = new OrderResponseLine
                      {
                        OrderResponse = orderresponse,
                        VendorLineNumber = orderline.VendorLineNr, //
                        OrderLineID = int.Parse(orderline.OrderlineID), //
                        VendorItemNumber = orderline.VendorItemNr, //
                        Description = orderline.Description, //
                        OEMNumber = orderline.OEM,//     
                        Ordered = 0,
                        Backordered = 0,
                        Invoiced = 0,
                        Shipped = 0,
                        Cancelled = 0,
                        Processed = false,
                      };

                      DateTime RequestDate;


                      DateTime.TryParseExact(orderline.OrderDate, "yyyyMMdd", null, DateTimeStyles.None, out RequestDate);

                      int Shipped;
                      int.TryParse(orderline.Shipped, NumberStyles.Any, CultureInfo.InvariantCulture, out Shipped);

                      OrderResponseLine.RequestDate = RequestDate;
                      OrderResponseLine.Shipped = Shipped;

                      orderResponseLines.Add(OrderResponseLine);
                    }
                  }
                }

                else if (responseType == OrderResponseTypes.InvoiceNotification)
                {
                  orderresponse = new OrderResponse
                  {
                    VendorID = vendorID,
                    InvoiceDocumentNumber = Order.InvoiceNumber,
                    AdministrationCost = decimal.Parse(Order.Costs.FirstOrDefault(x => x.Description == "Handlingkosten" || x.Description == "Handlingskosten").Amount),
                    DropShipmentCost = decimal.Parse(Order.Costs.FirstOrDefault(x => x.Description == "Dropshipmentkosten").Amount),
                    InvoiceDate = DateTime.Parse(Order.InvoiceDate),
                    PaymentConditionDays = int.Parse(Order.PaymentDays),
                    TotalExVat = decimal.Parse(Order.TotalExVat),
                    VatAmount = decimal.Parse(Order.VatAmount),
                    VendorDocumentNumber = Order.VendorDocNr,
                    OrderID = int.Parse(Order.OrderID),
                    TrackAndTrace = Order.TrackingNumber,
                    OrderDate = DateTime.Parse(Order.OrderDate),
                    ResponseType = responseType.ToString(),
                    VendorDocument = VendorDocument,
                    ReceiveDate = DateTime.Now
                  };


                  foreach (var orderline in Order.OrderLines)
                  {
                    var orderLine = orderInDb.OrderLines.FirstOrDefault(x => x.OrderLineID == Int32.Parse(orderline.OrderlineID));
                    if (orderLine != null)
                    {
                      var OrderResponseLine = new OrderResponseLine
                      {
                        OrderResponse = orderresponse,
                        VendorLineNumber = orderline.VendorLineNr,
                        OrderLineID = int.Parse(orderline.OrderlineID),
                        VendorItemNumber = orderline.VendorItemNr,
                        Description = orderline.Description,
                        OEMNumber = orderline.OEM,
                        Ordered = 0,
                        Backordered = 0,
                        Invoiced = 0,
                        Shipped = 0,
                        Cancelled = 0,
                        Processed = false
                      };

                      decimal Price;
                      int OrderlineID;
                      int Ordered;
                      int Invoiced;

                      DateTime RequestDate;

                      DateTime.TryParseExact(Order.OrderDate, "yyyyMMdd", null, DateTimeStyles.None, out RequestDate);

                      int.TryParse(orderline.Invoiced, NumberStyles.Any, CultureInfo.InvariantCulture, out Invoiced);
                      decimal.TryParse(orderline.Price, NumberStyles.Any, CultureInfo.InvariantCulture, out Price);
                      int.TryParse(orderline.Ordered, NumberStyles.Any, CultureInfo.InvariantCulture, out Ordered);

                      int.TryParse(orderline.OrderlineID, out OrderlineID);


                      OrderResponseLine.Price = Price;
                      OrderResponseLine.OrderLineID = OrderlineID;
                      OrderResponseLine.Ordered = Ordered;
                      OrderResponseLine.Invoiced = Invoiced;
                      OrderResponseLine.RequestDate = RequestDate;

                      orderResponseLines.Add(OrderResponseLine);
                    }
                  }
                }

              unit.Scope.Repository<OrderResponse>().Add(orderresponse);
              unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLines);

            }
          }
        }
      }


      unit.Save();
    }

    public void CancelOrder(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }


  }

  class OrderProcess
  {
    public string TotalShipped { get; set; }

    public string InvoiceNumber { get; set; }

    public string InvoiceDate { get; set; }

    public string TrackingNumber { get; set; }

    public string TotalExVat { get; set; }

    public string VatAmount { get; set; }

    public string OrderID { get; set; }

    public string VendorDocNr { get; set; }

    public string Currency { get; set; }

    public string ShipToName { get; set; }

    public string ShipToStreet { get; set; }

    public string ShipToPostalCode { get; set; }

    public string ShipToCity { get; set; }

    public string ShipToCountry { get; set; }

    public string Tax { get; set; }

    public string Date { get; set; }

    public List<OrderCost> Costs { get; set; }

    public List<OrderProcessLine> OrderLines { get; set; }

    public string TotalPrice { get; set; }

    public string OrderDate { get; set; }

    public string ShipmentNumber { get; set; }

    public string ShipmentDate { get; set; }

    public string PaymentDays { get; set; }

    public List<string> TrackingNumbers { get; set; }

    public string ShipDate { get; set; }

    public string ReceiveDate { get; set; }
  }

  class OrderProcessLine
  {
    public string OEM { get; set; }

    public string VendorLineNr { get; set; }

    public string OrderlineID { get; set; }

    public string VendorItemNr { get; set; }

    public string Description { get; set; }

    public string Price { get; set; }

    public string Ordered { get; set; }

    public string StatusCode { get; set; }

    public string StatusDate { get; set; }

    public string StatusQuantity { get; set; }

    public string VATAmount { get; set; }

    public string VATPercentage { get; set; }

    public string Invoiced { get; set; }

    public string OrderDate { get; set; }

    public string Shipped { get; set; }
  }

  class OrderCost
  {
    public string Description { get; set; }

    public string Amount { get; set; }
  }

}
