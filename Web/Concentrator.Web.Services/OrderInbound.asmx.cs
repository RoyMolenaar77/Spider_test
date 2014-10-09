using System;
using System.Linq;
using System.Web.Services;
using System.Xml.Linq;
using Concentrator.Objects;
using Concentrator.Objects.Web;
using Concentrator.Web.Services.Base;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Connectors;
using System.Globalization;

namespace Concentrator.Web.Services
{
  /// <summary>
  /// Summary description for OrderInbound
  /// </summary>
  [WebService(Namespace = "http://Concentrator.bascomputers.nl/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class OrderInbound : BaseConcentratorService
  {

    [WebMethod(Description = "Import orders into the concentrator with no shipping costs")]
    public OrderInboundResponse ImportOrder(string document, int connectorID)
    {
      return ImportOrderWithShipmentCosts(document, connectorID, false);
    }


    [WebMethod(Description = "Import orders into the concentrator")]
    public OrderInboundResponse ImportOrderWithShipmentCosts(string document, int connectorID, bool useShipmentCosts = false)
    {
      try
      {
        var response = new OrderInboundResponse()
      {
        StatusCode = 200,
        Message = "order successfully imported"
      };

        using (var unit = GetUnitOfWork())
        {
          var user = unit.Scope.Repository<User>().GetSingle(x => x.Username == "SYSTEM");
          var documentX = XDocument.Parse(document);

          ConcentratorPrincipal.Login("SYSTEM", "SYS");
          var header = documentX.Element("WebOrderRequest").Element("WebOrderHeader");
          var details = documentX.Element("WebOrderRequest").Element("WebOrderDetails").Elements("WebOrderDetail");
          var customer = documentX.Element("WebOrderRequest").Element("WebCustomer");
          var ord = new
                      {
                        OrderHeader = new
                                        {
                                          isDropShipment =
                                            header.Element("CustomerOverride").Element("Dropshipment").Try(
                                              c => bool.Parse(c.Value), false),
                                          CustomerOrderReference = header.Element("CustomerOrderReference").Value,
                                          CreationTime = header.Element("CreationTime").Try<XElement, DateTime?>(c => DateTime.Parse(c.Value), (DateTime?)null),
                                          EdiVersion = header.Element("EdiVersion").Value,
                                          BSKIdentifier = header.Element("BSKIdentifier").Value,
                                          WebSiteOrderNumber = header.Element("WebSiteOrderNumber").Value,
                                          OrderLanguageCode = header.Element("OrderLanguageCode").Try(x => x.Value, string.Empty),
                                          ShipmentCosts = useShipmentCosts ? (decimal?)decimal.Parse(header.Element("ShipmentCosts").Value, new CultureInfo("en-US")) : null,
                                          PaymentTermsCode = header.Element("PaymentTermsCode").Value,
                                          PaymentInstrument = header.Element("PaymentInstrument").Value,
                                          BackOrdersAllowed = header.Try(c => bool.Parse(c.Element("BackOrdersAllowed").Value), false),
                                          RouteCode = header.Try(c => c.Element("RouteCode").Value, string.Empty),
                                          EANIdentifier =
                                            header.Element("ShipToCustomer").Try(
                                              c => c.Element("EanIdentifier").Value, string.Empty).Trim(),
                                          HoldCode = header.Try(c => c.Element("HoldCode").Value, string.Empty),
                                          NonDropDetails = new
                                                             {
                                                               ShipToShopID =
                                                                 header.Try(c => c.Element("ShipToShopID").Value,
                                                                            string.Empty)
                                                             }
                                        },
                        ShipToCustomer = new
                                    {
                                      Name = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("Name").Value, string.Empty),
                                      AddressLine1 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine1").Value, string.Empty),
                                      AddressLine2 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine2").Value, string.Empty),
                                      AddressLine3 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine3").Value, string.Empty),
                                      HouseNumber = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("HouseNumber").Value, string.Empty),
                                      HouseNumberExt = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("HouseNumberExt").Value, string.Empty),
                                      Street = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("Street").Value, string.Empty),
                                      PostCode = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("ZipCode").Value, string.Empty),

                                      City = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("City").Value, string.Empty),
                                      Country = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("Country").Value, string.Empty),
                                      Email = header.Try(dc => dc.Element("CustomerOverride").Element("CustomerContact").Element("Email").Value, string.Empty),
                                      ServicePointID = header.Try(dc => dc.Element("CustomerOverride").Element("CustomerContact").Element("ServicePointID").Value, string.Empty),
                                      ServicePointCode = header.Try(dc => dc.Element("CustomerOverride").Element("CustomerContact").Element("ServicePointCode").Value, string.Empty),
                                      KialaCompanyName = header.Try(dc => dc.Element("CustomerOverride").Element("CustomerContact").Element("KialaCompanyName").Value, string.Empty)

                                    },
                        OrderLines = (from o in details
                                      let custReference = o.Element("CustomerReference")
                                      let product = o.Element("ProductIdentifier")
                                      let hasDiscountRules = o.Element("LineDiscounts") != null && o.Element("LineDiscounts").Elements("WebOrderRequestOrderLineDiscount").Count() > 0
                                      select new
                                               {
                                                 CustomerReference = custReference == null ? null : new
                                                                       {
                                                                         CustomerOrder = custReference.Try(c => c.Element("CustomerOrder").Value, string.Empty),
                                                                         CustomerOrderLine = custReference.Try(c => c.Element("CustomerOrderLine").Value, string.Empty),
                                                                         CustomerItemNumber = custReference.Try(c => c.Element("CustomerItemNumber").Value, string.Empty)
                                                                       },
                                                 Product = product == null ? null : new
                                                             {
                                                               ID = product.Element("ProductNumber").Value,
                                                               EAN = product.Try(c => c.Element("EANIdentifier").Value, string.Empty),
                                                               ManufacturerItemID = product.Try(c => c.Element("ManufacturerItemID").Value, string.Empty),
                                                               Qty = o.Try(c => int.Parse(c.Element("Quantity").Value), 1),
                                                               VendorItemNumber = o.Try(c => c.Element("VendorItemNumber").Value, string.Empty),
                                                               WarehouseCode = o.Try(c => c.Element("WareHouseCode").Value, string.Empty),
                                                               LinePrice = o.Try(c => double.Parse(c.Element("UnitPrice").Value, new CultureInfo("en-US")), 0),
                                                               BasePrice = o.Try(c => double.Parse(c.Element("BasePrice").Value, new CultureInfo("en-US")), 0),
                                                               LineDiscount = o.Try<XElement, double?>(c => double.Parse(c.Element("LineDiscount").Value, new CultureInfo("en-US")), null),
                                                               PriceOverride = o.Try(c => bool.Parse(c.Element("PriceOverride").Value), false)
                                                             },
                                                 AppliedRuleDiscounts = hasDiscountRules ?
                                                                       (from p in o.Element("LineDiscounts").Elements("WebOrderRequestOrderLineDiscount")
                                                                        select new
                                                                        {
                                                                          Code = p.Element("Code").Value,
                                                                          RuleID = int.Parse(p.Element("RuleID").Value),
                                                                          DiscountAmount = decimal.Parse(p.Element("DiscountAmount").Value, new CultureInfo("en-US")),
                                                                          Percentage = bool.Parse(p.Element("Percentage").Value),
                                                                          IsSet = bool.Parse(p.Element("IsSet").Value)
                                                                        }).ToList() : null

                                               }).ToList(),
                        CustomerContactInfo = customer == null ? null : new
                                                                          {
                                                                            Name = customer.Try(c => c.Element("CustomerAddress").Element("Name").Value, string.Empty),
                                                                            AddressLine1 = customer.Try(c => c.Element("CustomerAddress").Element("AddressLine1").Value, string.Empty),
                                                                            AddressLine2 = customer.Try(c => c.Element("CustomerAddress").Element("AddressLine2").Value, string.Empty),
                                                                            AddressLine3 = customer.Try(c => c.Element("CustomerAddress").Element("AddressLine3").Value, string.Empty),
                                                                            Number = customer.Try(c => c.Element("CustomerAddress").Element("Number").Value, string.Empty),
                                                                            ZipCode = customer.Try(c => c.Element("CustomerAddress").Element("ZipCode").Value, string.Empty),
                                                                            City = customer.Try(c => c.Element("CustomerAddress").Element("City").Value, string.Empty),
                                                                            Country = customer.Try(c => c.Element("CustomerAddress").Element("Country").Value, string.Empty),
                                                                            Email = customer.Try(c => c.Element("CustomerContact").Element("Email").Value, string.Empty),
                                                                            CompanyName = customer.Try(c => c.Element("CustomerContact").Element("Name").Value, string.Empty),
                                                                            CoCNumber = customer.Try(c => c.Element("CustomerContact").Element("CoCNumber").Value, string.Empty),
                                                                            Street = customer.Try(c => c.Element("CustomerAddress").Element("Street").Value, string.Empty),
                                                                            HouseNumberExtension = customer.Try(c => c.Element("CustomerAddress").Element("NumberExtension").Value, string.Empty),
                                                                          }
                      };


          #region Dropshipment/Non-Drop

          Customer customerShipping = new Customer
                                    {
                                      EANIdentifier = string.Format("S_{0}", ord.OrderHeader.EANIdentifier)
                                    };
          unit.Scope.Repository<Customer>().Add(customerShipping);
          //}
          if (ord.ShipToCustomer != null)
          {
            customerShipping.CustomerAddressLine1 = ord.ShipToCustomer.AddressLine1;
            customerShipping.CustomerAddressLine2 = ord.ShipToCustomer.AddressLine2;
            customerShipping.CustomerAddressLine3 = ord.ShipToCustomer.AddressLine3;
            customerShipping.HouseNumber = ord.ShipToCustomer.HouseNumber;
            customerShipping.City = ord.ShipToCustomer.City;
            customerShipping.Country = ord.ShipToCustomer.Country;
            customerShipping.PostCode = ord.ShipToCustomer.PostCode;
            customerShipping.CustomerName = ord.ShipToCustomer.Name;
            customerShipping.CustomerEmail = ord.ShipToCustomer.Email;//ord.Try(c => c.CustomerContactInfo.Email, string.Empty);
            customerShipping.ServicePointID = ord.ShipToCustomer.ServicePointID;
            customerShipping.ServicePointCode = ord.ShipToCustomer.ServicePointCode;
            customerShipping.ServicePointName = ord.ShipToCustomer.KialaCompanyName;
            customerShipping.CompanyName = ord.OrderHeader.NonDropDetails.ShipToShopID != "0" ? ord.OrderHeader.NonDropDetails.ShipToShopID : null;
            customerShipping.Street = ord.ShipToCustomer.Street;
            customerShipping.HouseNumber = ord.ShipToCustomer.HouseNumber;
            customerShipping.HouseNumberExt = ord.ShipToCustomer.HouseNumberExt;
          }

          Customer customerBilling = null;
          if (ord.CustomerContactInfo != null)
          {
            customerBilling = new Customer
            {
              EANIdentifier = ord.OrderHeader.EANIdentifier
            };
            customerBilling.CustomerAddressLine1 = ord.CustomerContactInfo.AddressLine1;
            customerBilling.CustomerAddressLine2 = ord.CustomerContactInfo.AddressLine2;
            customerBilling.CustomerAddressLine3 = ord.CustomerContactInfo.AddressLine3;
            customerBilling.HouseNumber = ord.CustomerContactInfo.Number;
            customerBilling.HouseNumberExt = ord.CustomerContactInfo.HouseNumberExtension;
            customerBilling.Street = ord.CustomerContactInfo.Street;
            customerBilling.City = ord.CustomerContactInfo.City;
            customerBilling.Country = ord.CustomerContactInfo.Country;
            customerBilling.PostCode = ord.CustomerContactInfo.ZipCode;
            customerBilling.CustomerName = ord.CustomerContactInfo.Name;
            customerBilling.CustomerEmail = ord.CustomerContactInfo.Email;
            customerBilling.CoCNumber = ord.CustomerContactInfo.CoCNumber;
            customerBilling.CompanyName = ord.OrderHeader.NonDropDetails.ShipToShopID != "0" ? ord.OrderHeader.NonDropDetails.ShipToShopID : null;

            unit.Scope.Repository<Customer>().Add(customerBilling);
          }

          Order newOrd = new Order()
          {
            ShippedToCustomer = customerShipping,
            SoldToCustomer = customerBilling,
            Document = document.ToString(),
            ConnectorID = connectorID,
            isDropShipment = ord.OrderHeader.isDropShipment,
            ReceivedDate = (ord.OrderHeader.CreationTime ?? DateTime.Now).ToUniversalTime(),
            BackOrdersAllowed = ord.OrderHeader.BackOrdersAllowed,
            CustomerOrderReference = ord.OrderHeader.CustomerOrderReference,
            EdiVersion = ord.OrderHeader.EdiVersion,
            BSKIdentifier = ord.OrderHeader.BSKIdentifier,
            WebSiteOrderNumber = ord.OrderHeader.WebSiteOrderNumber,
            PaymentInstrument = ord.OrderHeader.PaymentInstrument,
            PaymentTermsCode = ord.OrderHeader.PaymentTermsCode,
            RouteCode = ord.OrderHeader.RouteCode,
            HoldCode = ord.OrderHeader.HoldCode,
            OrderLanguageCode = ord.OrderHeader.OrderLanguageCode
          };
          #endregion
          var orderRepo = unit.Scope.Repository<Order>();
          var duplicateOrder = orderRepo.GetSingle(d => d.ConnectorID == newOrd.ConnectorID
                                && d.WebSiteOrderNumber == newOrd.WebSiteOrderNumber);


          if (duplicateOrder != null)
          {
            newOrd.HoldOrder = true;
            //response.StatusCode = 400;
            //response.Message = "Duplicate WebsiteOrdernumber for Connector";
            return new OrderInboundResponse()
            {
              StatusCode = 400,
              Message = "Duplicate WebsiteOrdernumber for Connector"
            };

          }
          else
            newOrd.HoldOrder = false;


          var orderType = (int)OrderTypes.SalesOrder;

          newOrd.OrderType = orderType;
          orderRepo.Add(newOrd);

          var kialaOrder = ord.ShipToCustomer != null && !string.IsNullOrEmpty(ord.ShipToCustomer.ServicePointID);

          #region Order Lines
          foreach (var line in ord.OrderLines)
          {
            int? productid = null;
            int lineProductID = int.Parse(line.Product.ID);

            var product = unit.Scope.Repository<Product>().GetSingle(p => p.ProductID == lineProductID);

            if (product != null)
              productid = product.ProductID;

            var ol = new OrderLine
            {
              CustomerOrderLineNr = line.CustomerReference.CustomerOrderLine,
              CustomerOrderNr = line.CustomerReference.CustomerOrder,
              CustomerItemNumber = line.CustomerReference.CustomerItemNumber,
              Order = newOrd,
              Quantity = line.Product.Qty,
              LineDiscount = line.Product.LineDiscount,
              ProductID = productid,
              Price = line.Product.LinePrice,
              UnitPrice = line.Product.LinePrice / line.Product.Qty,
              WareHouseCode = line.Product.WarehouseCode,
              PriceOverride = line.Product.PriceOverride,
              BasePrice = line.Product.BasePrice == 0 ? (double)product.VendorAssortments.FirstOrDefault().VendorPrices.FirstOrDefault().Price : line.Product.BasePrice
            };

            if (line.AppliedRuleDiscounts != null)
            {
              foreach (var rule in line.AppliedRuleDiscounts)
              {
                var appliedRule = new OrderLineAppliedDiscountRule()
                {
                  OrderLine = ol,
                  RuleID = rule.RuleID,
                  Code = rule.Code,
                  DiscountAmount = rule.DiscountAmount,
                  IsSet = rule.IsSet,
                  Percentage = rule.Percentage
                };
                unit.Scope.Repository<OrderLineAppliedDiscountRule>().Add(appliedRule);
              }
            }


            unit.Scope.Repository<OrderLine>().Add(ol);
          }


          if (useShipmentCosts)
          {
            if ((string.IsNullOrEmpty(ord.OrderHeader.NonDropDetails.ShipToShopID) || ord.OrderHeader.NonDropDetails.ShipToShopID == "0") || ord.OrderHeader.ShipmentCosts.Value > 0)
            {
              var connector = unit.Scope.Repository<Connector>().GetSingle(c => c.ConnectorID == connectorID);

              var shipmentCostsProductVendorItemNumer = kialaOrder ? connector.ConnectorSettings.GetValueByKey("KialaShipmentCostsProduct", string.Empty) : connector.ConnectorSettings.GetValueByKey("ShipmentCostsVendorItemNumber", string.Empty);

              if (!string.IsNullOrEmpty(shipmentCostsProductVendorItemNumer))
              {
                var shipmentCostProduct = unit.Scope.Repository<Product>().GetSingle(c => c.VendorItemNumber == shipmentCostsProductVendorItemNumer);
                var shipmentCostPrice = shipmentCostProduct.VendorAssortments.FirstOrDefault(c => c.VendorID == connector.ConnectorPublicationRules.FirstOrDefault().VendorID).VendorPrices.FirstOrDefault().Price;
                //new order line
                OrderLine shOl = new OrderLine()
                {
                  Product = shipmentCostProduct,
                  CustomerItemNumber = shipmentCostProduct.VendorItemNumber,
                  Order = newOrd,
                  isDispatched = false,
                  Price = Convert.ToDouble(ord.OrderHeader.ShipmentCosts.Value),
                  UnitPrice = Convert.ToDouble(shipmentCostPrice.Value),
                  LineDiscount = Convert.ToDouble(shipmentCostPrice.Value - ord.OrderHeader.ShipmentCosts.Value),
                  BasePrice = Convert.ToDouble(shipmentCostPrice.Value),
                  Quantity = 1
                };

                unit.Scope.Repository<OrderLine>().Add(shOl);
              }
            }
          }

          #endregion

          unit.Save();
        }

        return response;
      }
      catch (Exception e)
      {
        return new OrderInboundResponse()
        {
          StatusCode = 500,
          Message = e.InnerException != null ? e.InnerException.Message : e.Message
        };
      }
    }
  }

  /// <summary>
  /// Status Code (200 = OK, 400 = error, 500 = server error)
  /// </summary>
  public class OrderInboundResponse
  {
    public OrderInboundResponse() { }

    public int StatusCode { get; set; }
    public string Message { get; set; }
  }
}
