using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml;
using System.Xml.Linq;
using Spider.Objects;
using Spider.Objects.Web;
using Spider.Objects.Orders;
using Spider;
using System.Xml.Serialization;
using System.Web.UI.WebControls;

namespace Spider.Order.WebService
{
  /// <summary>
  /// Summary description for OrderInbound
  /// </summary>
  [WebService(Namespace = "http://spider.orders.nl/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [System.ComponentModel.ToolboxItem(false)]
  // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
  [System.Web.Script.Services.ScriptService]
  public class OrderInbound : System.Web.Services.WebService
  {

    [WebMethod(Description = "Import orders into the concentrator")]
    public void ImportOrder(string order, int connectorID)
    {
      try
      {
        XDocument document = XDocument.Parse(order);
        SpiderPrincipal.Login("SYSTEM", "SYS");
        using (ConcentratorDataContext context = new ConcentratorDataContext())
        {
          var header = document.Element("WebOrderRequest").Element("WebOrderHeader");
          var details = document.Element("WebOrderRequest").Element("WebOrderDetails").Elements("WebOrderDetail");
          var customer = document.Element("WebOrderRequest").Element("WebCustomer");
          var ord = new
                      {
                        OrderHeader = new
                                        {
                                          isDropShipment =
                                            header.Element("CustomerOverride").Element("Dropshipment").Try(
                                              c => bool.Parse(c.Value), false),
                                          CustomerOrderReference = header.Element("CustomerOrderReference").Value,
                                          EdiVersion = header.Element("EdiVersion").Value,
                                          BSKIdentifier = header.Element("BSKIdentifier").Value,
                                          WebSiteOrderNumber = header.Element("WebSiteOrderNumber").Value,
                                          PaymentTermsCode = header.Element("PaymentTermsCode").Value,
                                          PaymentInstrument = header.Element("PaymentInstrument").Value,
                                          BackOrdersAllowed =
                                            header.Try(c => bool.Parse(c.Element("BackOrdersAllowed").Value), false),
                                          RouteCode = header.Try(c => c.Element("RouteCode").Value, string.Empty),
                                          EANIdentifier =
                                            header.Element("ShipToCustomer").Try(
                                              c => c.Element("EanIdentifier").Value, string.Empty),
                                          NonDropDetails = new
                                                             {
                                                               ShipToShopID =
                                                                 header.Try(c => c.Element("ShipToShopID").Value,
                                                                            string.Empty)
                                                             }
                                        },
                        Customer = new
                                    {
                                      Name = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("Name").Value, string.Empty),
                                      AddressLine1 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine1").Value, string.Empty),
                                      AddressLine2 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine2").Value, string.Empty),
                                      AddressLine3 = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("AddressLine3").Value, string.Empty),
                                      PostCode = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("ZipCode").Value, string.Empty),
                                      City = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("City").Value, string.Empty),
                                      Country = header.Try(dc => dc.Element("CustomerOverride").Element("OrderAddress").Element("Country").Value, string.Empty),
                                    },
                        OrderLines = (from o in details
                                      let custReference = o.Element("CustomerReference")
                                      let product = o.Element("ProductIdentifier")
                                      select new
                                               {
                                                 CustomerReference = custReference == null ? null : new
                                                                       {
                                                                         CustomerOrder = custReference.Try(c => c.Element("CustomerOrder").Value, string.Empty),
                                                                         CustomerOrderLine = custReference.Try(c => c.Element("CustomerOrderLine").Value, string.Empty)
                                                                       },
                                                 Product = product == null ? null : new
                                                             {
                                                               ID = product.Element("ProductNumber").Value,
                                                               EAN = product.Try(c => c.Element("EANIdentifier").Value, string.Empty),
                                                               ManufacturerItemID = product.Try(c => c.Element("ManufacturerItemID").Value, string.Empty),
                                                               Qty = product.Try(c => int.Parse(c.Element("Quantity").Value), 1),
                                                               VendorItemNumber = product.Try(c => c.Element("VendorItemNumber").Value, string.Empty),
                                                               WarehouseCode = product.Try(c => c.Element("WareHouseCode").Value, string.Empty)
                                                             }



                                               }).ToList(),
                        CustomerContactInfo = customer == null ? null : new
                                                                          {
                                                                            Email = customer.Try(c => c.Element("CustomerContact").Element("Name").Value, string.Empty)
                                                                          }
                      };


          #region Dropshipment/Non-Drop

          Customer customerE = (from c in context.Customers
                                where c.EANIdentifier == ord.OrderHeader.EANIdentifier
                                select c).FirstOrDefault();

          if (customerE == null)
          {
            customerE = new Customer
                          {
                            EANIdentifier = ord.OrderHeader.EANIdentifier
                          };
            context.Customers.InsertOnSubmit(customerE);
          }
          if (ord.Customer != null)
          {
            customerE.CustomerAddressLine1 = ord.Customer.AddressLine1;
            customerE.CustomerAddressLine2 = ord.Customer.AddressLine2;
            customerE.CustomerAddressLine3 = ord.Customer.AddressLine3;
            customerE.City = ord.Customer.City;
            customerE.Country = ord.Customer.Country;
            customerE.PostCode = ord.Customer.PostCode;
            customerE.CustomerName = ord.Customer.Name;
            customerE.CustomerEmail = ord.Try(c => c.CustomerContactInfo.Email, string.Empty);
          }

          Spider.Objects.Orders.Order newOrd = new Objects.Orders.Order()
          {
            Customer = customerE,
            Document = order,
            ConnectorID = connectorID,
            IsDropShipment = ord.OrderHeader.isDropShipment,
            ReceivedDate = DateTime.Now,
            BackOrdersAllowed = ord.OrderHeader.BackOrdersAllowed,
            CustomerOrderReference = ord.OrderHeader.CustomerOrderReference,
            EdiVersion = ord.OrderHeader.EdiVersion,
            BSKIdentifier = ord.OrderHeader.BSKIdentifier,
            WebSiteOrderNumber = ord.OrderHeader.WebSiteOrderNumber,
            PaymentInstrument = ord.OrderHeader.PaymentInstrument,
            PaymentTermsCode = ord.OrderHeader.PaymentTermsCode,
            RouteCode = ord.OrderHeader.RouteCode,
          };


          #endregion


          context.Orders.InsertOnSubmit(newOrd);

          #region Order Lines
          foreach (var line in ord.OrderLines)
          {
            int? productid = null;
            var product = (from p in context.Products where p.ProductID == int.Parse(line.Product.ID) select p).FirstOrDefault();

            if (product != null)
              productid = product.ProductID;

            OrderLine ol = new OrderLine
                             {
                               CustomerOrderLineNr = line.CustomerReference.CustomerOrderLine,
                               CustomerOrderNr = line.CustomerReference.CustomerOrder,
                               Order = newOrd,
                               Quantity = line.Product.Qty,
                               ProductID = productid
                             };

            context.OrderLines.InsertOnSubmit(ol);
          }
          #endregion


          context.SubmitChanges();
        }
      }
      catch (Exception e)
      {
        throw new Exception("Error inserting order", e);
      }
    }
  }
}
