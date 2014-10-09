using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;
using AuditLog4Net.Adapter;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.SFTP;
using Concentrator.Objects.Ftp;
using Concentrator.Plugins.PFA;
using Concentrator.Plugins.PFA.Configuration;

namespace Concentrator.Objects.Ordering.Dispatch
{
  public class TNTDispatcher : IDispatchable
  {
    private Dictionary<string, List<PaymentMethodDescription>> PaymentMethodDescriptions { get; set; }

    private const Decimal DefaultVAT = 21M;

    private IAuditLogAdapter Log
    {
      get;
      set;
    }

    private IUnitOfWork Unit
    {
      get;
      set;
    }

    private Vendor Vendor
    {
      get;
      set;
    }

    public Int32 DispatchOrders(Dictionary<Order, List<OrderLine>> orders, Vendor vendor, IAuditLogAdapter log, IUnitOfWork unit)
    {
      Log = log;
      Unit = unit;
      Vendor = vendor;

      PaymentMethodDescriptions = ((from u in unit.Scope.Repository<PaymentMethod>().GetAll().ToList()
                                    select new
                                    {
                                      Code = u.Code,
                                      Values = (from p in u.PaymentMethodDescriptions
                                                select p).ToList()
                                    }).ToDictionary(c => c.Code, c => c.Values));

      var archiveDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Archive"));

      if (!archiveDirectory.Exists)
        archiveDirectory.Create();

      var ftpManager = new FtpManager(GetDestinationUri(vendor), log, usePassive: true);

      foreach (var order in orders)
      {
        var fileName = CreateXmlFileName(order.Key);

        var document = CreatePickTicket(order.Key, order.Value);

        document.Save(Path.Combine(archiveDirectory.FullName, fileName));

        if (document.Element("Pickticket").Element("PickticketDetail") != null)
        {
          using (var memoryStream = new MemoryStream())
          {
            document.Save(memoryStream);

            ftpManager.Upload(memoryStream.Reset(), fileName);
          }
        }
      }

      return -1;
    }

    public void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, String logPath, IUnitOfWork unit)
    {
      var shipmentNotificationImporter = new ShipmentNotificationImporter(vendor, unit, log);

      shipmentNotificationImporter.Execute(GetSourceUri(vendor), vendor);

      var returnNotificationImporter = new ReturnNotificationImporter(vendor, unit, log);

      returnNotificationImporter.Execute(GetSourceUri(vendor), vendor);

      var receivedNotificationImporter = new ReceivedNotificationImporter(vendor, unit, log);

      receivedNotificationImporter.Execute(GetSourceUri(vendor), vendor);
    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, String logPath)
    {
      throw new NotImplementedException();
    }

    public void LogOrder(Object orderInformation, Int32 vendorID, String fileName, IAuditLogAdapter log)
    {
      throw new NotImplementedException();
    }

    private XDocument CreatePickTicket(Order order, IEnumerable<OrderLine> orderLines)
    {
      bool useKialaShipmentCosts = order.ShippedToCustomer != null && !string.IsNullOrEmpty(order.ShippedToCustomer.ServicePointID);

      return new XDocument(new XDeclaration("1.0", "utf-8", "yes")
        , new XElement("Pickticket"
          , CreatePickTicketHeader(order, useKialaShipmentCosts)
          , CreatePickTicketDetails(order, orderLines, useKialaShipmentCosts)));
    }

    private XElement CreatePickTicketHeader(Order order, bool useKialaShipmentCosts)
    {
      var languageCode = !string.IsNullOrEmpty(order.OrderLanguageCode) ? order.OrderLanguageCode : "NL";
      var shippingCostsVendorItemNumber = GetShipmentCostsProduct(order.ConnectorID, Unit, useKialaShipmentCosts);
      var shippingCosts = order.OrderLines.Try(c => c.FirstOrDefault(l => l.Product.VendorItemNumber == shippingCostsVendorItemNumber).Price.Value, 0);

      var vatAmount = order.OrderLines.Sum(c => ((c.Price != null ? c.Price.Value : 0) - c.LineDiscount.Try(l => l.Value, 0)) * Convert.ToDouble((c.Try(l => l.Product.VendorAssortments.FirstOrDefault(p => p.VendorID == Vendor.VendorID).VendorPrices.FirstOrDefault().TaxRate, DefaultVAT) / 100)));

      var paymentMethod = order.PaymentTermsCode;

      List<PaymentMethodDescription> overridePaymentMethod = null;
      PaymentMethodDescriptions.TryGetValue(order.PaymentTermsCode, out overridePaymentMethod);

      if (overridePaymentMethod != null)
      {
        var description = overridePaymentMethod.FirstOrDefault(c => c.Language.DisplayCode == languageCode || languageCode.StartsWith(c.Language.DisplayCode)).Try(c => c.Description, null);

        if (!string.IsNullOrEmpty(description))
          paymentMethod = description;
      }

      var pickTicketHeader = new XElement("PickticketHeader");
      pickTicketHeader.Add(new XElement("PickticketNumber", order.OrderID));
      pickTicketHeader.Add(new XElement("WebsiteOrderNumber", order.WebSiteOrderNumber));
      pickTicketHeader.Add(new XElement("OrderDate", order.ReceivedDate));
      pickTicketHeader.Add(new XElement("Currency", "EUR"));
      pickTicketHeader.Add(new XElement("Language", languageCode));
      pickTicketHeader.Add(new XElement("PaymentMethod", paymentMethod));
      pickTicketHeader.Add(new XElement("ShippingCosts", shippingCosts.ToString("F2")));
      pickTicketHeader.Add(new XElement("VATAmount", vatAmount.ToString("F2")));

      switch (order.OrderType)
      {
        case (int)OrderTypes.PurchaseOrder:
          pickTicketHeader.Add(new XElement("ShipToShop", false));
          pickTicketHeader.Add(CreatePickTicketCustomer("ShipTo", order.ShippedToCustomer));
          pickTicketHeader.Add(CreatePickTicketCustomer("SoldTo", order.SoldToCustomer));
          pickTicketHeader.Add(new XElement("OrderType", (OrderTypes)order.OrderType));
          pickTicketHeader.Add(CreatePurchaseOrderSupplier("Supplier", order.CustomerOrderReference));
          break;
        case (int)OrderTypes.PickTicketOrder:
          pickTicketHeader.Add(new XElement("ShipToShop", true));
          pickTicketHeader.Add(CreatePickTicketCustomerID("ShipTo", order.ShippedToCustomer, order.CustomerOrderReference));
          pickTicketHeader.Add(CreatePickTicketCustomerID("SoldTo", order.SoldToCustomer, order.CustomerOrderReference));
          break;
        case (int)OrderTypes.SalesOrder:
          pickTicketHeader.Add(new XElement("ShipToShop", order.CustomerOrderReference.StartsWith("Winkel#")));
          pickTicketHeader.Add(CreatePickTicketCustomer("ShipTo", order.ShippedToCustomer));
          pickTicketHeader.Add(CreatePickTicketCustomer("SoldTo", order.SoldToCustomer));
          break;

        default:
          throw new NotImplementedException();
      }

      if (useKialaShipmentCosts)
        pickTicketHeader.Add(new XElement("Kiala",
          new XElement("ServicepointId", order.ShippedToCustomer.ServicePointID),
          new XElement("ServicepointCode", "KI"),
          new XElement("ServicepointName", order.ShippedToCustomer.ServicePointName)
          ));

      return pickTicketHeader;
    }

    private IEnumerable<XElement> CreatePickTicketDetails(Order order, IEnumerable<OrderLine> orderLines, bool useKialaShipmentCosts)
    {
      foreach (var orderLine in orderLines)
      {
        var product = orderLine.Product;

        if (product == null)
        {
          var message = String.Format("Order line '{0}' has no valid product.", order.OrderID);

          Log.AuditError(message);

          throw new InvalidOperationException(message);
        }

        if (product.VendorItemNumber == GetShipmentCostsProduct(order.ConnectorID, Unit, useKialaShipmentCosts))  //dont dispatch the shipment costs as part of the order
          continue;

        var vendorAssortment = product.VendorAssortments.SingleOrDefault(va => va.Vendor == Vendor);

        if (vendorAssortment == null)
        {
          var childVendors = Unit
            .Scope
            .Repository<Vendor>()
            .GetAll(vendor => vendor.ParentVendorID == Vendor.VendorID)
            .Select(vendor => vendor.VendorID);

          vendorAssortment = product.VendorAssortments.FirstOrDefault(va => childVendors.Contains(va.VendorID));
          if (vendorAssortment == null)
          {
            var message = String.Format("Product '{0}' has no assortment of vendor '{1}', or of the child vendors.", product.VendorItemNumber, Vendor.Name);

            Log.AuditError(message);

            throw new InvalidOperationException(message);
          }
        }

        var vendorPrice = vendorAssortment.VendorPrices.SingleOrDefault(vp => vp.MinimumQuantity == 0);

        if (vendorPrice == null)
        {
          var message = String.Format("Product '{0}' has no vendor prices with a minimum quantity of 0.", product.VendorItemNumber);

          Log.AuditError(message);

          throw new InvalidOperationException(message);
        }

        var skuParts = product.VendorItemNumber.Split(' ');

        var productBarcode = product.ProductBarcodes.ToList().FirstOrDefault(c => ((c.BarcodeType ?? 0) == 0) && c.Barcode.Length == 13);

        var orderLinePrice = Math
            .Max((orderLine.Price.GetValueOrDefault() - orderLine.LineDiscount.GetValueOrDefault()), 0D);

        var pricePerItem = (orderLinePrice / orderLine.Quantity);

        var sizeCode = product.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == 41).Try(c => c.Value, string.Empty);


        if (string.IsNullOrEmpty(sizeCode))
          sizeCode = skuParts[2];

        string colorCode = skuParts[1];

        var fetchColorCodeFromAttribute = vendorAssortment.Vendor.VendorSettings.GetValueByKey("TNTDispatcherGetColorCodeFromAttribute", false);
        if (fetchColorCodeFromAttribute)
        {
          colorCode = product.ProductAttributeValues.FirstOrDefault(c => c.AttributeID == 40).Try(c => c.Value, string.Empty);
        }

        yield return new XElement("PickticketDetail"
        , new XElement("PickticketDetailNumber", orderLine.OrderLineID)
        , new XElement("WebsiteOrderLineNumber", orderLine.OrderLineID)
        , new XElement("ArtikelCode", skuParts[0])
        , skuParts.Length > 2 ? new XElement("ColorCode", colorCode) : null
        , skuParts.Length > 2 ? new XElement("SizeCode", sizeCode) : null
        , new XElement("Barcode", productBarcode != null ? productBarcode.Barcode : String.Empty)
        , new XElement("Quantity", orderLine.Quantity)
        , new XElement("VAT", vendorPrice.TaxRate.GetValueOrDefault(DefaultVAT).ToString("F4"))
        , new XElement("VATAmount", ((Math.Max((orderLine.Price.GetValueOrDefault() - orderLine.LineDiscount.GetValueOrDefault()), 0D)) * Convert.ToDouble(vendorPrice.TaxRate.GetValueOrDefault(DefaultVAT) / 100)).ToString("F4"))
        , new XElement("UnitPrice", orderLine.BasePrice.GetValueOrDefault((Double)vendorPrice.Price.GetValueOrDefault()).ToString("F4"))
        , new XElement("SpecialPrice", pricePerItem.ToString("F4"))
        , new XElement("OrderLinePrice", orderLinePrice.ToString("F4"))
        );
      }
    }

    private XElement CreatePickTicketCustomer(String elementName, Customer customer)
    {
      return new XElement(elementName
       , new XElement("CustomerID", customer != null ? customer.CustomerID.ToString() : string.Empty)
       , new XElement("Name", customer != null ? customer.CustomerName : string.Empty)
       , new XElement("Address", customer != null ? customer.CustomerAddressLine1 : string.Empty)
        // todo: Stan kan je een oplossing vinden voor BE postcodes
        //, new XElement("ZipCode", customer != null ? FormatPostcode(customer.PostCode) : string.Empty)
        , new XElement("ZipCode", customer != null ? customer.PostCode : string.Empty)
       , new XElement("City", customer != null ? customer.City : string.Empty)
       , new XElement("Country", customer != null ? customer.Country.ToUpper() : string.Empty)
       , new XElement("Telephone", customer != null && !string.IsNullOrEmpty(customer.CustomerTelephone) ? customer.CustomerTelephone : "0")
       , new XElement("E-Mail", customer != null ? customer.CustomerEmail : string.Empty)
       );
    }

    private XElement CreatePickTicketCustomerID(String elementName, Customer customer, string customerId)
    {
      var tntMappingTable = Unit.ExecuteStoreQuery<string>(string.Format("SELECT ClientCode from ClientMappingTNT where ClientName = '{0}'", customerId)).FirstOrDefault();

      if (tntMappingTable != null)
        customerId = tntMappingTable;

      return new XElement(elementName
       , new XElement("CustomerID", customerId ?? string.Empty)
       , new XElement("Name", customer != null ? customer.CustomerName : string.Empty)
       , new XElement("Address", customer != null ? customer.CustomerAddressLine1 : string.Empty)
        // todo: Stan kan je een oplossing vinden voor BE postcodes
        //, new XElement("ZipCode", customer != null ? FormatPostcode(customer.PostCode) : string.Empty)
       , new XElement("ZipCode", customer != null ? customer.PostCode : string.Empty)
       , new XElement("City", customer != null ? customer.City : string.Empty)
       , new XElement("Country", customer != null ? customer.Country.ToUpper() : string.Empty)
       , new XElement("Telephone", customer != null && !string.IsNullOrEmpty(customer.CustomerTelephone) ? customer.CustomerTelephone : "0")
       , new XElement("E-Mail", customer != null ? customer.CustomerEmail : string.Empty));
    }

    private XElement CreatePurchaseOrderSupplier(String elementName, String customerOrderReference)
    {
      string[] vendorData = customerOrderReference.Split(new string[] { " - " }, StringSplitOptions.None);
      string vendorCode = vendorData[0];
      string vendorName = vendorData[1];

      return new XElement(elementName
       , new XElement("VendorCode", vendorCode)
       , new XElement("VendorName", vendorName));
    }

    private static string CreateXmlFileName(Order order)
    {
      string fileName = string.Empty;

      if ((OrderTypes)order.OrderType == OrderTypes.PurchaseOrder)
        fileName = String.Format("PurchaseOrder_{0}.xml", order.WebSiteOrderNumber);
      else
        fileName = String.Format("Pickticket_{0:yyyyMMdd}_{0:HHmmssffffff}.xml", DateTime.UtcNow);

      return fileName;
    }

    //todo: change with a better way of parsing and exception handling
    private string FormatPostcode(string postcode)
    {
      string formattedPostcode = string.Empty;
      try
      {
        //replace with regex
        if (postcode.Contains(" ")) formattedPostcode = postcode; //already formatted

        else formattedPostcode = postcode.Substring(0, 4) + " " + postcode.Substring(4, 2); //not formattedl
      }
      catch (Exception e)
      {
        formattedPostcode = postcode;
      }

      return formattedPostcode;
    }

    private static string GetDestinationUri(Vendor vendor)
    {
      string uri = string.Empty;

      uri = vendor.VendorSettings.GetValueByKey<string>("TNTDestinationURI", string.Empty);
      uri.ThrowIfNullOrEmpty(new InvalidOperationException("TNT Destination URI must be defined for vendor " + vendor.Name));

      return uri;
    }

    private static string GetSourceUri(Vendor vendor)
    {
      string uri = string.Empty;

      uri = vendor.VendorSettings.GetValueByKey<string>("TNTSourceURI", string.Empty);
      uri.ThrowIfNullOrEmpty(new InvalidOperationException("TNT Source URI must be defined for vendor " + vendor.Name));

      return uri;
    }

    public static string GetShipmentCostsProduct(Connector connector, bool useKialaShipmentCost)
    {
      string shippingCostProduct = string.Empty;

      if (useKialaShipmentCost)
      {
        shippingCostProduct = connector.ConnectorSettings.GetValueByKey("KialaShipmentCostsVendorItemNumber", string.Empty);
        shippingCostProduct.ThrowIfNullOrEmpty(new InvalidOperationException("KialaShipmentCostsVendorItemNumber must be defined for connector " + connector.Name));
      }
      else
      {
        shippingCostProduct = connector.ConnectorSettings.GetValueByKey("ShipmentCostsVendorItemNumber", string.Empty);
        shippingCostProduct.ThrowIfNullOrEmpty(new InvalidOperationException("ShipmentCostsVendorItemNumber must be defined for connector " + connector.Name));
      }

      return shippingCostProduct;
    }

    public static string GetShipmentCostsProduct(int connectorID, IUnitOfWork unit, bool useKialaShipmentCosts)
    {
      Connector connector = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorID);

      return GetShipmentCostsProduct(connector, useKialaShipmentCosts);
    }
  }
}
