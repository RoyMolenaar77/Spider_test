using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using Concentrator.Web.Objects.EDI.DirectShipment;
using Concentrator.Web.Objects.EDI;
using Concentrator.Objects;
using Concentrator.Web.Objects.EDI.Purchase;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Logic;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Orders;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.DataAccess.UnitOfWork;

namespace Concentrator.Objects.Ordering.XmlFormats
{
  public class BasOrderExporter : BaseOrderExporter<List<WebOrderRequest>>
  {
    public override List<WebOrderRequest> GetOrder(Dictionary<Concentrator.Objects.Models.Orders.Order, List<Concentrator.Objects.Models.Orders.OrderLine>> orderLines, Vendor vendor)
    {
      using (IUnitOfWork unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {

        var groupedOrders = orderLines;
        var msgID = groupedOrders.FirstOrDefault().Value.FirstOrDefault().OrderLineID;

        List<WebOrderRequest> orders = new List<WebOrderRequest>();


        foreach (var ord in groupedOrders)
        {
          var customer = ord.Key.ShippedToCustomer;

          var soldToCustomer = ord.Key.SoldToCustomer;

          if (soldToCustomer == null)
            soldToCustomer = customer;

          string backendCustomerID = "0";

          if (!string.IsNullOrEmpty(ord.Key.Connector.BackendEanIdentifier))
            backendCustomerID = ord.Key.Connector.BackendEanIdentifier;

          if (backendCustomerID == "0" && !string.IsNullOrEmpty(ord.Key.ShippedToCustomer.EANIdentifier))
            backendCustomerID = ord.Key.ShippedToCustomer.EANIdentifier;
          ContentLogic logic = new ContentLogic(unit.Scope, 0);
          WebOrderRequest order = new WebOrderRequest()
                                    {

                                      WebCustomer = new CreateCustomer
                                                      {
                                                        CustomerAddress = new WebAddress
                                                                            {
                                                                              AddressLine1 = soldToCustomer.CustomerAddressLine1,
                                                                              AddressLine2 = soldToCustomer.CustomerAddressLine2,
                                                                              AddressLine3 = soldToCustomer.CustomerAddressLine3,
                                                                              Number = soldToCustomer.HouseNumber,
                                                                              City = soldToCustomer.City,
                                                                              Country = soldToCustomer.Country,
                                                                              Name = soldToCustomer.CustomerName,
                                                                              ZipCode = soldToCustomer.PostCode
                                                                            },
                                                        CustomerContact = new WebContact
                                                                            {
                                                                              HomePhoneNumber = !string.IsNullOrEmpty(soldToCustomer.CustomerTelephone) ? soldToCustomer.CustomerTelephone : string.Empty,
                                                                              Email = soldToCustomer.CustomerEmail,
                                                                              Name = soldToCustomer.CustomerName,
                                                                              FaxNumber = string.Empty,
                                                                              BusinessPhoneNumber = soldToCustomer.CoCNumber,
                                                                              MobilePhoneNumber = string.Empty,
                                                                              Website = string.Empty,
                                                                            }

                                                      },
                                      WebOrderDetails = (from c in ord.Value
                                                         select new WebOrderRequestDetail
                                                         {
                                                           CustomerReference = new CustomerReference
                                                                                 {
                                                                                   CustomerOrder = (string.IsNullOrEmpty(c.CustomerOrderNr) ? customer.CustomerName : c.CustomerOrderNr).Cap(25),
                                                                                   CustomerOrderLine = c.OrderLineID.ToString(),
                                                                                 },
                                                           ProductIdentifier = new ProductIdentifier
                                                                                 {
                                                                                   //ProductNumber = c.Product != null ? (c.Product.VendorAssortment.Where(x => x.VendorID == vendor.VendorID).FirstOrDefault() != null ? c.Product.VendorAssortment.Where(x => x.VendorID == vendor.VendorID).FirstOrDefault().CustomItemNumber : c.CustomerItemNumber) : c.CustomerItemNumber
                                                                                   ProductNumber = logic.GetVendorItemNumber(c.Product, c.CustomerItemNumber, vendor.VendorID)
                                                                                 },
                                                           //VendorItemNumber = c.Product != null ? c.Product.VendorItemNumber : string.Empty,
                                                           Quantity = c.GetDispatchQuantity(),
                                                           WareHouseCode = c.WareHouseCode,
                                                           UnitPrice = c.PriceOverride && c.Price.HasValue ? decimal.Floor((decimal)c.Price.Value * 10000).ToString() : string.Empty

                                                         }).ToArray(),
                                      WebOrderHeader = new WebOrderRequestHeader
                                                         {
                                                           EdiVersion = ord.Key.EdiVersion,
                                                           BSKIdentifier = int.Parse(ord.Key.BSKIdentifier),
                                                           CustomerOrderReference = (string.IsNullOrEmpty(ord.Key.CustomerOrderReference) ? ord.Key.ShippedToCustomer.CustomerName : ord.Key.CustomerOrderReference).Cap(25),
                                                           ShipToCustomer = new Concentrator.Web.Objects.EDI.Customer
                                                                                                       {
                                                                                                         Contact = new Contact
                                                                                                         {
                                                                                                           Email = customer.CustomerEmail,
                                                                                                           Name = customer.CustomerName,
                                                                                                           PhoneNumber = !string.IsNullOrEmpty(customer.CustomerTelephone) ? customer.CustomerTelephone : string.Empty
                                                                                                         },
                                                                                                         EanIdentifier = backendCustomerID // customer.EANIdentifier
                                                                                                       },
                                                           CustomerOverride = new CustomerOverride
                                                           {
                                                             CustomerContact = new Contact
                                                             {
                                                               Email = customer.CustomerEmail,
                                                               Name = customer.CustomerName,
                                                               PhoneNumber = !string.IsNullOrEmpty(customer.CustomerTelephone) ? customer.CustomerTelephone : string.Empty
                                                             },
                                                             Dropshipment = ord.Key.isDropShipment.HasValue ? ord.Key.isDropShipment.Value : false,
                                                             OrderAddress = new Address
                                                             {
                                                               AddressLine1 = customer.CustomerAddressLine1 + " " + customer.HouseNumber,
                                                               AddressLine2 = customer.CustomerAddressLine2,
                                                               AddressLine3 = customer.CustomerAddressLine3,
                                                               City = customer.City,
                                                               Country = customer.Country,
                                                               Name = customer.CustomerName.Cap(40),
                                                               ZipCode = customer.PostCode
                                                             }
                                                           },
                                                           WebSiteOrderNumber = ord.Key.WebSiteOrderNumber,
                                                           PaymentInstrument = ord.Key.PaymentInstrument,
                                                           PaymentTermsCode = ord.Key.PaymentTermsCode,
                                                           RouteCode = ord.Key.RouteCode,
                                                           HoldCode = ord.Key.HoldCode,
                                                           BackOrdersAllowed = ord.Key.BackOrdersAllowed.Value,
                                                           BackOrdersAllowedSpecified = ord.Key.BackOrdersAllowed.Value
                                                         },
                                      Version = "1.0"
                                    };

          orders.Add(order);
        }
        return orders;
      }
    }

    public override DirectShipmentRequest GetDirectShipmentOrder(Concentrator.Objects.Models.Orders.Order order, List<Concentrator.Objects.Models.Orders.OrderLine> orderLines, Vendor administrativeVendor, Vendor vendor)
    {
      var customer = order.ShippedToCustomer;
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        Dictionary<int, string> customItemNumbers = new Dictionary<int, string>();
        Dictionary<int, VendorAssortment> VendorAssortments = new Dictionary<int, VendorAssortment>();
        Dictionary<string, string> additionalItems = new Dictionary<string, string>();
        ContentLogic logic = new ContentLogic(unit.Scope);
        foreach (var l in orderLines)
        {
          if (l.Product != null)
          {
            string customItemNumber1 = logic.GetVendorItemNumber(l.Product, l.CustomerItemNumber, administrativeVendor.VendorID);
            var customItemNumber = l.Product.VendorAssortments.Where(x => x.VendorID == administrativeVendor.VendorID && x.Product == l.Product).Select(x => x.CustomItemNumber).FirstOrDefault();
            //l.ProductID.HasValue ? ContentLogic.GetVendorProductID(l.ProductID.Value, administrativeVendor.VendorID, l.CustomerItemNumber) : l.CustomerItemNumber;

            customItemNumbers.Add(l.ProductID.Value, customItemNumber);

            var vass = l.Product.VendorAssortments.Where(x => x.VendorID == vendor.VendorID).Select(x => x).FirstOrDefault();
            VendorAssortments.Add(l.ProductID.Value, vass);
          }
          else
          {

            var additionalOrderProduct = (from aop in unit.Scope.Repository<AdditionalOrderProduct>().GetAllAsQueryable()
                                          where aop.ConnectorID == l.Order.ConnectorID
                                          && aop.ConnectorProductID == l.CustomerItemNumber
                                          && aop.VendorID == administrativeVendor.VendorID
                                          select aop).FirstOrDefault();

            if (additionalOrderProduct != null && !additionalItems.ContainsKey(l.CustomerItemNumber))
              additionalItems.Add(l.CustomerItemNumber, additionalOrderProduct.VendorProductID);
          }

        }

        var administrativeVendorSettings = administrativeVendor.PreferredConnectorVendors.Where(x => x.ConnectorID == order.ConnectorID).FirstOrDefault();
        var connectorVendorSettings = vendor.PreferredConnectorVendors.Where(x => x.ConnectorID == order.ConnectorID).FirstOrDefault();

        DirectShipmentRequest directShipmentOrder = new DirectShipmentRequest()
        {
          bskIdentifier = !string.IsNullOrEmpty(administrativeVendorSettings.VendorIdentifier) ? administrativeVendorSettings.VendorIdentifier : order.BSKIdentifier.ToString(),
          Version = "1.0",
          DirectShipmentCustomer = new DirectShipmentCustomer()
          {
            DirectShipmentCustomerAddress = new DirectShipmentCustomerAddress()
            {
              AddressLine1 = order.ShippedToCustomer.CustomerAddressLine1,
              AddressLine2 = order.ShippedToCustomer.CustomerAddressLine2,
              AddressLine3 = order.ShippedToCustomer.CustomerAddressLine3,
              City = order.ShippedToCustomer.City,
              Country = order.ShippedToCustomer.Country,
              Name = order.ShippedToCustomer.CustomerName,
              ZipCode = order.ShippedToCustomer.PostCode,
              Number = order.ShippedToCustomer.HouseNumber
            },
            DirectShipmentCustomerContact = new DirectShipmentCustomerContact()
            {
              Email = order.ShippedToCustomer.CustomerEmail,
              Name = order.ShippedToCustomer.CustomerName
            }
          },
          DirectShipmentLines = (from l in orderLines
                                 let customItemNumber = l.Product != null ? customItemNumbers[l.ProductID.Value] : additionalItems[l.CustomerItemNumber]
                                 let vendorAss = l.Product != null ? VendorAssortments[l.ProductID.Value] : null
                                 let description = l.Product != null ? (l.Product.ProductDescriptions.Where(x => x.LanguageID == (int)LanguageTypes.English).Select(x => x.ShortContentDescription).FirstOrDefault() != null ?
                                       l.Product.ProductDescriptions.Where(x => x.LanguageID == (int)LanguageTypes.English).Select(x => x.ShortContentDescription).FirstOrDefault() : l.Product.Contents.Where(x => x.ConnectorID == order.ConnectorID).Select(x => x.ShortDescription).FirstOrDefault()) : "Additional Item"
                                 let unitCost = (l.ProductID.HasValue ? logic.CalculatePrice(l.ProductID.Value, l.GetDispatchQuantity(), l.Order.Connector, PriceRuleType.CostPrice) : (decimal)l.Price.Value)
                                 let unitPrice = (l.ProductID.HasValue ? logic.CalculatePrice(l.ProductID.Value, l.GetDispatchQuantity(), l.Order.Connector, PriceRuleType.UnitPrice) : (decimal)l.Price.Value)
                                 select new DirectShipmentLine
                                 {
                                   ItemNumber = customItemNumber,
                                   Price = l.Price.HasValue ? l.Price.Value.ToString() : vendorAss.VendorPrices.FirstOrDefault().Price.Value.ToString(),
                                   Product = (l.ProductID.HasValue ? new Concentrator.Web.Objects.EDI.DirectShipment.Product()
                                   {
                                     BrandCode = l.Product.Brand.BrandVendors.Where(x => x.VendorID == administrativeVendorSettings.Vendor.VendorID).Select(x => x.VendorBrandCode).FirstOrDefault(),
                                     Description = string.IsNullOrEmpty(description) ? vendorAss.ShortDescription : description,
                                     Description2 = string.Empty,
                                     EAN = l.Product.ProductBarcodes.FirstOrDefault() != null ? (l.Product.ProductBarcodes.FirstOrDefault().Barcode.Length == 13 ? l.Product.ProductBarcodes.FirstOrDefault().Barcode : string.Empty) : string.Empty,
                                     UPC = l.Product.ProductBarcodes.FirstOrDefault() != null ? (l.Product.ProductBarcodes.FirstOrDefault().Barcode.Length == 12 ? l.Product.ProductBarcodes.FirstOrDefault().Barcode : string.Empty) : string.Empty,
                                     ModelNumber = string.Empty,
                                     VendorItemNumber = l.Product != null ? l.Product.VendorItemNumber : string.Empty,
                                     UnitCost = unitCost > 0 ? unitCost : (vendorAss != null ?
                                                  (vendorAss.VendorPrices.FirstOrDefault().CostPrice.HasValue ? vendorAss.VendorPrices.FirstOrDefault().CostPrice.Value : (vendorAss.VendorPrices.FirstOrDefault().Price.HasValue ? vendorAss.VendorPrices.FirstOrDefault().Price.Value : 0))
                                                  : 0),
                                     UnitPrice = new Concentrator.Web.Objects.EDI.DirectShipment.ProductUnitPrice()
                                     {
                                       HighTaxRate = true,
                                       LowTaxRate = false,
                                       Text = new string[] { unitPrice > 0 ? unitPrice.ToString() : (l.Price.HasValue ? l.Price.Value.ToString() : (vendorAss.VendorPrices.FirstOrDefault().Price.HasValue ? vendorAss.VendorPrices.FirstOrDefault().Price.Value.ToString() : "0")) }
                                     }
                                   } : new Concentrator.Web.Objects.EDI.DirectShipment.Product() { }),
                                   Quantity = l.GetDispatchQuantity(),
                                   Reference = !string.IsNullOrEmpty(order.ShippedToCustomer.CustomerName) ? order.ShippedToCustomer.CustomerName : string.Empty,
                                   RequestDate = order.ReceivedDate,
                                   ShipToNumber = order.ShippedToCustomer.EANIdentifier,
                                   SupplierNumber = (connectorVendorSettings != null && !string.IsNullOrEmpty(connectorVendorSettings.VendorIdentifier)) ? connectorVendorSettings.VendorIdentifier : string.Empty,
                                   SupplierSalesOrder = l.OrderID,
                                   WebsiteNumber = order.WebSiteOrderNumber,
                                   Remark = l.Remarks//,
                                   //SupplierSalesOrder = order.OrderID
                                 }).ToArray()

        };
        return directShipmentOrder;
      }
    }

    public PurchaseRequest GetPurchaseOrder(Concentrator.Objects.Models.Orders.Order order, List<OrderLine> orderLines, Vendor administrativeVendor, Vendor vendor)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        var customer = order.ShippedToCustomer;

        Dictionary<int, string> customItemNumbers = new Dictionary<int, string>();
        Dictionary<int, VendorAssortment> VendorAssortments = new Dictionary<int, VendorAssortment>();
        Dictionary<string, string> additionalItems = new Dictionary<string, string>();
        ContentLogic logic = new ContentLogic(unit.Scope);
        foreach (var l in orderLines)
        {

          if (l.Product != null)
          {

            //var customItemNumber = l.Product.VendorAssortment.Where(x => x.VendorID == administrativeVendor.VendorID && x.Product == l.Product).Select(x => x.CustomItemNumber).FirstOrDefault();
            string customItemNumber = logic.GetVendorItemNumber(l.Product, l.CustomerItemNumber, administrativeVendor.VendorID);
            customItemNumbers.Add(l.ProductID.Value, customItemNumber);

            var vass = l.Product.VendorAssortments.Where(x => x.VendorID == vendor.VendorID).Select(x => x).FirstOrDefault();
            VendorAssortments.Add(l.ProductID.Value, vass);
          }
          else
          {

            var additionalOrderProduct = (from aop in unit.Scope.Repository<AdditionalOrderProduct>().GetAllAsQueryable()
                                          where aop.ConnectorID == l.Order.ConnectorID
                                          && aop.ConnectorProductID == l.CustomerItemNumber
                                          && aop.VendorID == administrativeVendor.VendorID
                                          select aop).FirstOrDefault();

            if (additionalOrderProduct != null)
              additionalItems.Add(l.CustomerItemNumber, additionalOrderProduct.VendorProductID);

          }
        }

        var administrativeVendorSettings = administrativeVendor.PreferredConnectorVendors.Where(x => x.ConnectorID == order.ConnectorID).FirstOrDefault();
        var connectorVendorSettings = vendor.PreferredConnectorVendors.Where(x => x.ConnectorID == order.ConnectorID).FirstOrDefault();

        PurchaseRequest purchaseRequest = new PurchaseRequest()
        {
          bskIdentifier = administrativeVendorSettings.VendorIdentifier,
          Version = "1.0",
          PurchaseCustomer = new PurchaseCustomer()
          {
            PurchaseCustomerAddress = new PurchaseCustomerAddress()
            {
              AddressLine1 = order.ShippedToCustomer.CustomerAddressLine1,
              AddressLine2 = order.ShippedToCustomer.CustomerAddressLine2,
              AddressLine3 = order.ShippedToCustomer.CustomerAddressLine3,
              City = order.ShippedToCustomer.City,
              Country = order.ShippedToCustomer.Country,
              Name = order.ShippedToCustomer.CustomerName,
              ZipCode = order.ShippedToCustomer.PostCode
            },
            PurchaseCustomerContact = new PurchaseCustomerContact()
            {
              Email = order.ShippedToCustomer.CustomerEmail,
              Name = order.ShippedToCustomer.CustomerName
            }
          },
          PurchaseLines = (from l in orderLines
                           let customItemNumber = l.Product != null ? customItemNumbers[l.ProductID.Value] : additionalItems[l.CustomerItemNumber]
                           let vendorAss = l.Product != null ? VendorAssortments[l.ProductID.Value] : null
                           select new PurchaseLine
                           {
                             ItemNumber = customItemNumber,
                             Price = l.Price.HasValue ? l.Price.Value.ToString() : vendorAss.VendorPrices.FirstOrDefault().Price.Value.ToString(),
                             Product = new Concentrator.Web.Objects.EDI.Purchase.Product()
                             {
                               BrandCode = l.Product.Brand.BrandVendors.Where(x => x.VendorID == administrativeVendorSettings.Vendor.VendorID).Select(x => x.VendorBrandCode).FirstOrDefault(),
                               Description = l.Product.ProductDescriptions.Where(x => x.LanguageID == (int)LanguageTypes.English).Select(x => x.ShortContentDescription).FirstOrDefault() != null ?
                                 l.Product.ProductDescriptions.Where(x => x.LanguageID == (int)LanguageTypes.English).Select(x => x.ShortContentDescription).FirstOrDefault() : l.Product.Contents.Where(x => x.ConnectorID == order.ConnectorID).Select(x => x.ShortDescription).FirstOrDefault(),
                               Description2 = string.Empty,
                               EAN = l.Product.ProductBarcodes.FirstOrDefault() != null ? (l.Product.ProductBarcodes.FirstOrDefault().Barcode.Length == 13 ? l.Product.ProductBarcodes.FirstOrDefault().Barcode : string.Empty) : string.Empty,
                               UPC = l.Product.ProductBarcodes.FirstOrDefault() != null ? (l.Product.ProductBarcodes.FirstOrDefault().Barcode.Length == 12 ? l.Product.ProductBarcodes.FirstOrDefault().Barcode : string.Empty) : string.Empty,
                               ModelNumber = string.Empty,
                               VendorItemNumber = l.Product != null ? l.Product.VendorItemNumber : string.Empty,
                               UnitCost = vendorAss != null ? (vendorAss.VendorPrices.FirstOrDefault().CostPrice.HasValue ? vendorAss.VendorPrices.FirstOrDefault().CostPrice.Value : vendorAss.VendorPrices.FirstOrDefault().Price.Value) : 0,
                               UnitPrice = new Concentrator.Web.Objects.EDI.Purchase.ProductUnitPrice()
                               {
                                 HighTaxRate = true,
                                 LowTaxRate = false,
                                 Text = new string[] { l.Price.HasValue ? l.Price.Value.ToString() : vendorAss.VendorPrices.FirstOrDefault().Price.Value.ToString() }
                               }
                             },
                             Quantity = l.GetDispatchQuantity(),
                             Reference = l.OrderLineID,
                             RequestDate = order.ReceivedDate,
                             ShipToNumber = order.ShippedToCustomer.EANIdentifier,
                             SupplierNumber = (connectorVendorSettings != null && !string.IsNullOrEmpty(connectorVendorSettings.VendorIdentifier)) ? connectorVendorSettings.VendorIdentifier : string.Empty,
                             SupplierSalesOrder = l.OrderID,
                             Remark = l.Remarks//,
                             //SupplierSalesOrder = order.OrderID
                           }).ToArray()

        };
        return purchaseRequest;
      }
    }

  }
}
