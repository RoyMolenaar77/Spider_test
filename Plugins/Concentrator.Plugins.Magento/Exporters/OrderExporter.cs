using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Connectors;
using AuditLog4Net.Adapter;
using Concentrator.Plugins.Magento.Helpers;
using Concentrator.Web.Objects.EDI;
using System.Xml.Serialization;
using System.Configuration;
using System.Xml;
using System.Net;
using System.IO;
using System.Data;
using Concentrator.Objects.Models.Vendors;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects;
using System.Globalization;


namespace Concentrator.Plugins.Magento.Exporters
{
  public class OrderExporter
  {
    public Connector Connector { get; private set; }
    public IAuditLogAdapter Logger { get; private set; }
    public Configuration Configuration { get; private set; }
    private Action<int> _notifyOrderCount;


    public OrderExporter(Connector connector, IAuditLogAdapter logger, Configuration configuration, Action<int> notifyOrderCount)
    {
      Connector = connector;
      Logger = logger;
      Configuration = configuration;
      _notifyOrderCount = notifyOrderCount;
    }

    private List<AdditionalOrderProduct> AdditionalOrderProducts = null;

    public void Process()
    {
      SyncOrders();
      //  SyncUpdatedOrders();
    }

    private void SyncUpdatedOrders()
    {
      throw new NotImplementedException();
    }

    private void SyncOrders()
    {
      var websiteCodeInCoreTable = Connector.ConnectorSettings.GetValueByKey<string>("WebsiteCodeInCoreTable", string.Empty);

      Logger.Info("Processing new sales orders");
      if (Connector.AdministrativeVendorID.HasValue)
      {
        Logger.InfoFormat("Using {0} as administrative vendor", Connector.AdministrativeVendorID.Value);
        AdditionalOrderProducts = Connector.AdditionalOrderProducts.Where(x => x.VendorID == Connector.AdministrativeVendorID.Value).ToList();
      }
      else
      {
        AdditionalOrderProducts = new List<AdditionalOrderProduct>();
      }
      using (var pHelper = new PriceSetHelper(Connector.ConnectionString))
      using (var helper = new OrderHelper(Connector.ConnectionString))
      {
        var orders = helper.GetSalesOrders(websiteCodeInCoreTable);

        if (orders.Tables.Count == 0 || orders.Tables[0].Rows.Count == 0)
        {
          Logger.DebugFormat("No sales orders found");
          return;
        }

        Random r = new Random();
        // Convert DataSet to EDI Format
        var ediOrders = (from order in orders.Tables[0].AsEnumerable()
                         group order by order.Field<object>("orderid").ToString() into grouped
                         let wo = grouped.FirstOrDefault()
                         let middleName = (wo.Field<string>("middlename"))
                         let middleNameBillable = (wo.Field<string>("bMiddlename"))

                         let storeNumber = wo.Field<object>("StoreNumber") != null ? wo.Field<object>("StoreNumber").ToString() : "0"
                         let st = storeNumber != "0" ? helper.GetStoreInfo(storeNumber) : null
                         let storeAddress1 = st != null ? st.addressLine1 : string.Empty
                         let storeAddress2 = st != null ? st.addressLine2 : string.Empty

                         let customerID = wo.Field<object>("CustomerID") != null ? wo.Field<object>("CustomerID").ToString() : r.Next(20000, 100000).ToString()
                         select new EDISalesOrder
                         {
                           WebsiteOrderID = wo.Field<string>("orderid"),
                           CustomerID = customerID,
                           OrderLanguageCode = DetermineLanguageCode(wo.Field<string>("orderLanguageCode"), Connector.ConnectorID),

                           CustomerShipping = new EDISalesOrderCustomer()
                           {
                             CustomerPO = string.IsNullOrEmpty(wo.Field<string>("customer_note")) ? string.Empty : wo.Field<string>("customer_note"),
                             MailingName = (wo.Field<string>("firstname") + " " + (string.IsNullOrEmpty(middleName) ? string.Empty : middleName + " ") + wo.Field<string>("lastname")).Capitalize(),
                             //keep for backwards compatibility
                             Addressline1 = string.IsNullOrEmpty(storeAddress1) ? ((wo.Field<string>("address") + " " + wo.Field<string>("housenumber")).Trim() + " " + wo.Field<string>("housenumberextension")).Capitalize() : storeAddress1.Capitalize(),
                             Number = wo.Field<string>("housenumber"),
                             NumberExtension = wo.Field<string>("housenumberextension"),
                             Street = wo.Field<string>("address"),
                             Addressline2 = string.IsNullOrEmpty(storeAddress2) ? string.Empty : storeAddress2.Capitalize(),
                             City = wo.Field<string>("city").ToUpper(),
                             Country = wo.Field<string>("country").Capitalize(),
                             ShopID = storeNumber,
                             StoreNumber = storeNumber,
                             ZIPcode = wo.Field<string>("postcode"),
                             EmailAddress = wo.Field<string>("email"),
                             CustomerID = wo.Field<object>("CustomerID") != null ? wo.Field<object>("CustomerID").ToString() : "0",
                             ServicePointCode = wo.Field<string>("ServicePointCode"),
                             ServicePointID = wo.Field<string>("ServicePointID"),
                             KialaCompanyName = wo.Field<string>("CompanyName")
                           },

                           CustomerBilling = new EDISalesOrderCustomer()
                           {
                             CustomerPO = string.IsNullOrEmpty(wo.Field<string>("customer_note")) ? string.Empty : wo.Field<string>("customer_note"),
                             MailingName = (wo.Field<string>("bFirstname") + " " + (string.IsNullOrEmpty(middleNameBillable) ? string.Empty : middleNameBillable + " ") + wo.Field<string>("bLastname")).Capitalize(),
                             Addressline1 = ((wo.Field<string>("bAddress") + " " + wo.Field<string>("bHousenumber")).Trim() + " " + wo.Field<string>("bHousenumberextension")).Capitalize(),
                             Number = wo.Field<string>("bHousenumber"),
                             NumberExtension = wo.Field<string>("bHousenumberextension"),
                             Street = wo.Field<string>("bAddress"),
                             City = wo.Field<string>("bCity").ToUpper(),
                             Country = wo.Field<string>("bCountry").Capitalize(),
                             ZIPcode = wo.Field<string>("bPostcode"),
                             CustomerID = wo.Field<object>("CustomerID") != null ? wo.Field<object>("CustomerID").ToString() : "0",
                             EmailAddress = wo.Field<string>("email")

                           },


                           ShippingAmount = wo.Field<decimal>("ShippingAmount"),
                           Payment = wo.Field<string>("payment_method"),
                           Status = wo.Field<string>("OrderStatus"),
                           State = wo.Field<string>("OrderState"),
                           ShopID = storeNumber == "0" ? string.Empty : storeNumber,

                           OrderLines = (from line in orders.Tables[0].AsEnumerable()
                                         where line.Field<string>("orderid") == wo.Field<string>("orderid")
                                         let basePrice = decimal.Parse(line.Try(c => c.Field<object>("baseprice"), 0).ToString())
                                         let basePriceGlobal = decimal.Parse(line.Try(c => c.Field<object>("baseprice_global"), 0).ToString())
                                         let basePriceActual = basePrice == 0 ? basePriceGlobal : basePrice
                                         let appliedRules = line.Try(c => c.Field<string>("applied_rule_ids"), string.Empty)

                                         let rules = (from c in appliedRules.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToArray()
                                                      select pHelper.GetSalesRule(int.Parse(c))).ToList()

                                         select new EDISalesOrderLine
                                         {
                                           DiscountAmount = line.Field<decimal?>("LineDiscount"),
                                           ConcentratorProductID = line.Field<int>("concentrator_item_number"),
                                           ManufacturerID = line.Field<string>("itemcode"),
                                           BrandName = line.Field<string>("brandname"),
                                           Quantity = decimal.Parse(line.Field<object>("qty_ordered").ToString()),
                                           LineNumber = int.Parse(line.Field<object>("linenumber").ToString()),
                                           LinePrice = decimal.Parse(line.Try(c => c.Field<object>("LinePrice"), 0).ToString()),
                                           BasePrice = basePriceActual,
                                           DiscountRules = (from p in rules
                                                            where p != null
                                                            select new EDISalesOrderLineDiscountRules()
                                                            {
                                                              DiscountAmount = p.discount_amount,
                                                              RuleCode = p.name,
                                                              RuleID = p.rule_id,
                                                              IsSetRule = p.is_discountset,
                                                              Percentage = p.simple_action == "by_percent"
                                                            }).ToList()
                                         }).ToList()
                         }).Distinct().ToList();

        _notifyOrderCount(ediOrders.Count);

        // Loop
        foreach (var ediOrder in ediOrders)
        {
          Logger.DebugFormat("Processing Sales Order {0}", ediOrder.WebsiteOrderID);


          bool postToEdi = true;

          if (ediOrder.IsPickupOrder)
          {
            using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
            {
              ediOrder.OrderLines.ForEach(x =>
              {
                var vendorStock = unit.Scope.Repository<VendorStock>().GetAll(s => s.Vendor.BackendVendorCode == ediOrder.CustomerID
                  && s.ProductID == x.ConcentratorProductID).FirstOrDefault();

                x.InStock = (vendorStock != null && vendorStock.QuantityOnHand >= x.Quantity);
              });
            }

            if (ediOrder.OrderLines.All(x => x.InStock))
            {
              postToEdi = false;

              helper.UpdateOrderStatus(increment_id: ediOrder.WebsiteOrderID.ToString(),
                                        state: MagentoOrderState.Processing,
                                        status: MagentoOrderStatus.shipped_to_shop
                                      );

            }
          }
          if (postToEdi)
            PostSalesOrderToEDI(ediOrder);

        }
      }


      Logger.Info("Finished processing new sales orders");

    }

    public void PostSalesOrderToConcentrator(EDISalesOrder order)
    {

    }

    public void PostSalesOrderToEDI(EDISalesOrder ediOrder)
    {

      if (!Connector.BSKIdentifier.HasValue)
      {
        Logger.WarnFormat("Connector doesn't have a BSK Identifier");
        return;
      }

      WebOrderRequest request = new WebOrderRequest();
      request.Version = "1.0";
      request.WebOrderHeader = new WebOrderRequestHeader();
      request.WebOrderHeader.ShipToCustomer = new Concentrator.Web.Objects.EDI.Customer();

      //request.WebOrderHeader.ShipToCustomer.EanIdentifier = webOrderInfo.CustomerID;
      if (ediOrder.IsPickupOrder)
      {
        request.WebOrderHeader.ShipToCustomer.EanIdentifier = "F" + ediOrder.ShopID;
      }
      else
      {
        request.WebOrderHeader.ShipToCustomer.EanIdentifier = ediOrder.CustomerID;
      }

      request.WebOrderHeader.ShipToCustomer.Contact = new Contact();
      request.WebOrderHeader.ShipToCustomer.Contact.Name = Configuration.AppSettings.Settings["ContactName"].Value.ToString();
      request.WebOrderHeader.ShipToCustomer.Contact.Email = Configuration.AppSettings.Settings["ContactEmail"].Value.ToString();
      request.WebOrderHeader.ShipToCustomer.Contact.PhoneNumber = Configuration.AppSettings.Settings["ContactPhoneNumber"].Value.ToString();
      request.WebOrderHeader.ShipToShopID = ediOrder.CustomerShipping.StoreNumber;
      request.WebOrderHeader.ShipmentCosts = ediOrder.ShippingAmount;

      if (ediOrder.IsPickupOrder)
      {
        if (!string.IsNullOrEmpty(ediOrder.CustomerShipping.CustomerPO) && ediOrder.CustomerShipping.CustomerPO != null)
          request.WebOrderHeader.CustomerOrderReference = "Winkel#:" + ediOrder.CustomerShipping.CustomerPO;
        else
          request.WebOrderHeader.CustomerOrderReference = "Winkel#:" + ediOrder.WebsiteOrderID + "/" + ediOrder.CustomerShipping.MailingName;
      }
      else
      {
        if (!string.IsNullOrEmpty(ediOrder.CustomerBilling.CustomerPO) && ediOrder.CustomerBilling.CustomerPO != null)
          request.WebOrderHeader.CustomerOrderReference = ediOrder.CustomerBilling.CustomerPO;
        else
          request.WebOrderHeader.CustomerOrderReference = Connector.Name + " - " + ediOrder.CustomerBilling.MailingName;
      }
      request.WebOrderHeader.WebSiteOrderNumber = ediOrder.WebsiteOrderID.ToString();
      request.WebOrderHeader.OrderLanguageCode = ediOrder.OrderLanguageCode;

      request.WebOrderHeader.RequestedDate = DateTime.Now.AddDays(1);
      request.WebOrderHeader.EdiVersion = "2.0";
      if (ediOrder.IsPickupOrder)
        request.WebOrderHeader.BSKIdentifier = Connector.ConnectorSettings.GetValueByKey("ShopOrderBSK", Connector.BSKIdentifier.Value);
      else
        request.WebOrderHeader.BSKIdentifier = Connector.BSKIdentifier.Value;


      request.WebOrderHeader.RouteCode = "TRA";

      if (!string.IsNullOrEmpty(ediOrder.CustomerShipping.ServicePointID)) request.WebOrderHeader.RouteCode = "KIALA";

      if (ediOrder.IsPickupOrder)
      {
        request.WebOrderHeader.PaymentTermsCode = ediOrder.Payment;
        request.WebOrderHeader.PaymentInstrument = "T";
      }
      else
      {
        #region Determine Payment Codes
        try
        {

          request.WebOrderHeader.PaymentInstrument = "T";
          request.WebOrderHeader.PaymentTermsCode = ediOrder.Payment;

        }
        catch (Exception ex)
        {
          Dictionary<string, string> error = new Dictionary<string, string>();
          error.Add("--p", "Fout tijdens het verwerken van de betaling");
        }
        #endregion
      }

      request.WebOrderHeader.CustomerOverride = new CustomerOverride();

      request.WebOrderHeader.CustomerOverride.CustomerContact = new Contact();


      request.WebOrderHeader.CustomerOverride.Dropshipment = !ediOrder.IsPickupOrder;
      request.WebOrderHeader.CustomerOverride.CustomerContact.Email = ediOrder.CustomerShipping.EmailAddress;
      request.WebOrderHeader.CustomerOverride.CustomerContact.ServicePointCode = ediOrder.CustomerShipping.ServicePointCode;
      request.WebOrderHeader.CustomerOverride.CustomerContact.ServicePointID = ediOrder.CustomerShipping.ServicePointID;
      request.WebOrderHeader.CustomerOverride.CustomerContact.KialaCompanyName = ediOrder.CustomerShipping.KialaCompanyName;


      request.WebOrderHeader.CustomerOverride.OrderAddress = new Address();

      request.WebOrderHeader.CustomerOverride.OrderAddress.AddressLine1 = ediOrder.CustomerShipping.Addressline1;
      request.WebOrderHeader.CustomerOverride.OrderAddress.AddressLine2 = !string.IsNullOrEmpty(ediOrder.CustomerShipping.Addressline2) ? ediOrder.CustomerShipping.Addressline2 : string.Empty;
      request.WebOrderHeader.CustomerOverride.OrderAddress.Name = ediOrder.CustomerShipping.MailingName;
      request.WebOrderHeader.CustomerOverride.OrderAddress.ZipCode = ediOrder.CustomerShipping.ZIPcode;
      request.WebOrderHeader.CustomerOverride.OrderAddress.City = ediOrder.CustomerShipping.City;
      request.WebOrderHeader.CustomerOverride.OrderAddress.Country = ediOrder.CustomerShipping.Country;
      request.WebOrderHeader.CustomerOverride.OrderAddress.HouseNumber = ediOrder.CustomerShipping.Number;
      request.WebOrderHeader.CustomerOverride.OrderAddress.HouseNumberExt = ediOrder.CustomerShipping.NumberExtension;
      request.WebOrderHeader.CustomerOverride.OrderAddress.Street = ediOrder.CustomerShipping.Street;

      request.WebOrderDetails = new WebOrderRequestDetail[ediOrder.OrderLines.Count];

      request.WebCustomer = new CreateCustomer();
      request.WebCustomer.CustomerContact = new WebContact();

      request.WebCustomer.CustomerContact.Email = ediOrder.CustomerBilling.EmailAddress;
      request.WebCustomer.CustomerAddress = new WebAddress();
      request.WebCustomer.CustomerAddress.AddressLine1 = ediOrder.CustomerBilling.Addressline1;
      request.WebCustomer.CustomerAddress.AddressLine2 = ediOrder.CustomerBilling.Addressline2;
      request.WebCustomer.CustomerAddress.Country = ediOrder.CustomerBilling.Country;
      request.WebCustomer.CustomerAddress.City = ediOrder.CustomerBilling.City;
      request.WebCustomer.CustomerAddress.Name = ediOrder.CustomerBilling.MailingName;
      request.WebCustomer.CustomerAddress.ZipCode = ediOrder.CustomerBilling.ZIPcode;
      request.WebCustomer.CustomerAddress.Number = ediOrder.CustomerBilling.Number;
      request.WebCustomer.CustomerAddress.NumberExtension = ediOrder.CustomerBilling.NumberExtension;
      request.WebCustomer.CustomerAddress.Street = ediOrder.CustomerBilling.Street;

      int rowcount = 0;
      Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient();


      foreach (var salesDetail in ediOrder.OrderLines)
      {
        var additionalProduct = AdditionalOrderProducts.FirstOrDefault(x => x.ConnectorProductID == salesDetail.ManufacturerID);
        string productID = string.Empty;

        if (additionalProduct != null)
        {
          Logger.DebugFormat("Found additional productID for {0}, connectorid {1}", salesDetail.ManufacturerID, Connector.ConnectorID);
          productID = additionalProduct.VendorProductID;
        }
        else
        {
          Logger.DebugFormat("Try get productID for {0}, manufactuerID {1}, brand {2} connectorid {3}", salesDetail.ConcentratorProductID, salesDetail.ManufacturerID, salesDetail.BrandName, Connector.ConnectorID);
          productID = salesDetail.ConcentratorProductID.ToString();//oap.GetVendorItemNumber(salesDetail.ConcentratorProductID, salesDetail.ManufacturerID, salesDetail.BrandName, Connector.ConnectorID);
          Logger.DebugFormat("Found product {0} for {1}, connector {2}", productID, salesDetail.ConcentratorProductID, Connector.ConnectorID);
        }

        if (string.IsNullOrEmpty(productID))
        {
          productID = soap.GetVendorItemNumber(0, salesDetail.ManufacturerID, salesDetail.BrandName, Connector.ConnectorID);
          Logger.DebugFormat("Retry found product {0} for {1}, connector {2}", productID, salesDetail.ConcentratorProductID, Connector.ConnectorID);
        }

        request.WebOrderDetails[rowcount] = new WebOrderRequestDetail();
        request.WebOrderDetails[rowcount].ProductIdentifier = new ProductIdentifier();
        request.WebOrderDetails[rowcount].ProductIdentifier.ProductNumber = productID;
        request.WebOrderDetails[rowcount].ProductIdentifier.EANIdentifier = string.Empty;
        request.WebOrderDetails[rowcount].ProductIdentifier.ManufacturerItemID = salesDetail.ManufacturerID;
        request.WebOrderDetails[rowcount].Quantity = (int)salesDetail.Quantity;
        request.WebOrderDetails[rowcount].VendorItemNumber = string.Empty;
        request.WebOrderDetails[rowcount].WareHouseCode = string.Empty;
        request.WebOrderDetails[rowcount].CustomerReference = new CustomerReference();
        request.WebOrderDetails[rowcount].CustomerReference.CustomerOrder = ediOrder.WebsiteOrderID.ToString();
        request.WebOrderDetails[rowcount].UnitPrice = salesDetail.LinePrice.ToString(new CultureInfo("en-US"));
        request.WebOrderDetails[rowcount].LineDiscount = salesDetail.DiscountAmount.HasValue ? salesDetail.DiscountAmount.Value.ToString(new CultureInfo("en-US")) : null;
        request.WebOrderDetails[rowcount].BasePrice = salesDetail.BasePrice.ToString(new CultureInfo("en-US"));

        request.WebOrderDetails[rowcount].CustomerReference.CustomerOrderLine = salesDetail.LineNumber.ToString();

        if (salesDetail.DiscountRules != null && salesDetail.DiscountRules.Count > 0)
        {
          request.WebOrderDetails[rowcount].LineDiscounts = new WebOrderRequestOrderLineDiscount[salesDetail.DiscountRules.Count];
          for (int i = 0; i < salesDetail.DiscountRules.Count; i++)
          {
            request.WebOrderDetails[rowcount].LineDiscounts[i] = new WebOrderRequestOrderLineDiscount();
            request.WebOrderDetails[rowcount].LineDiscounts[i].Code = salesDetail.DiscountRules[i].RuleCode;
            request.WebOrderDetails[rowcount].LineDiscounts[i].DiscountAmount = salesDetail.DiscountRules[i].DiscountAmount.ToString(new CultureInfo("en-US"));
            request.WebOrderDetails[rowcount].LineDiscounts[i].IsSet = salesDetail.DiscountRules[i].IsSetRule;
            request.WebOrderDetails[rowcount].LineDiscounts[i].Percentage = salesDetail.DiscountRules[i].Percentage;
            request.WebOrderDetails[rowcount].LineDiscounts[i].RuleID = salesDetail.DiscountRules[i].RuleID;
          }
        }
        rowcount++;
      }

      StringBuilder requestString = new StringBuilder();

      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Encoding = Encoding.UTF8;
      XmlWriter xw = XmlWriter.Create(requestString, settings);
      xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");


      Logger.Info(request.WebOrderHeader.ShipmentCosts);
      Logger.Info(request.WebOrderHeader.ShipmentCosts.ToString(new CultureInfo("en-US")));

      XmlSerializer rxs = new XmlSerializer(typeof(WebOrderRequest));
      XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
      ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
      rxs.Serialize(xw, request, ns);

      //rxs.Serialize(xw, request);
      XmlDocument requestXml = new XmlDocument();
      requestXml.LoadXml(requestString.ToString());


      bool usesShipmentsCosts = Connector.ConnectorSettings.GetValueByKey<bool>("UsesShipmentCosts", false);

      Concentrator.Web.ServiceClient.OrderInbound.OrderInboundSoapClient client = new Web.ServiceClient.OrderInbound.OrderInboundSoapClient();
      var res = client.ImportOrderWithShipmentCosts(requestString.ToString(), Connector.ConnectorID, usesShipmentsCosts);


      Logger.AuditInfo(string.Format("For order : {0}, the status is: {1}, the message is :{2}", ediOrder.WebsiteOrderID, res.StatusCode, res.Message));

      if (res.StatusCode == 200 || res.StatusCode == 400)
      {
        using (var helper = new OrderHelper(Connector.Connection))
        {
          helper.UpdateOrderStatus(
          increment_id: ediOrder.WebsiteOrderID.ToString(),
          state: MagentoOrderState.Processing,
          status: MagentoOrderStatus.In_Transit
          );

        }
      }
    }

    private void LogFile(string path, string contents)
    {

      if (!path.EndsWith(@"\"))
        path += @"\";

      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      if (!path.EndsWith(@"\"))
        path += @"\";

      Guid g = Guid.NewGuid();

      File.WriteAllText(path + g.ToString() + ".xml", contents);
    }

    private string DetermineLanguageCode(string code, int connectorID)
    {
      //TODO: FIX IT
      if (connectorID == 7)
      {
        return string.Format("{0}-{1}", code.ToUpper(), "BE");
      }

      int numberOfCodes = 0;

      string[] codes = code.Split('_');

      numberOfCodes = codes.Length;

      if (numberOfCodes == 1 && codes[0].Equals("nl"))
        return "NL-NL";
      else if (numberOfCodes == 2 && codes[0].Equals("be") && codes[1].Equals("nl"))
        return "NL-BE";
      else if (numberOfCodes == 2 && codes[0].Equals("be") && codes[1].Equals("fr"))
        return "FR-BE";
      else
        return string.Empty;
    }

    public void HttpCallBack(IAsyncResult result)
    {
      try
      {
        HttpPostState state = (HttpPostState)result.AsyncState;

        using (HttpWebResponse httpResponse = (HttpWebResponse)state.Request.EndGetResponse(result))
        {

          switch (httpResponse.StatusCode)
          {
            case HttpStatusCode.OK:
              Logger.DebugFormat("Order {0} succesfully acknowledged by backend", state.TransactionID);

              using (var helper = new OrderHelper(state.Connector.Connection))
              {
                helper.UpdateOrderStatus(
                increment_id: state.TransactionID,
                state: MagentoOrderState.Processing,
                status: MagentoOrderStatus.In_Transit
                );

              }

              break;

            default:
              Logger.ErrorFormat("Error uploading document");
              break;
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error("Error POST EDI order", ex);
      }
    }

    public class HttpPostState
    {
      private HttpWebRequest _request;
      private Uri _url;
      private string _transactionID;
      private Connector _connector;

      public HttpPostState(string transactionID, HttpWebRequest request, Connector connector)
      {
        _transactionID = transactionID;
        _request = request;
        _connector = connector;

      }

      public Connector Connector
      {
        get { return _connector; }
      }

      public string TransactionID
      {
        get { return _transactionID; }
      }

      public Uri Url
      {
        get { return _url; }
      }

      public HttpWebRequest Request
      {
        get { return _request; }
      }
    }
  }
  public static class StringExtensions
  {
    public static string Capitalize(this string source)
    {
      return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(source.ToLower());
    }
  }
}
