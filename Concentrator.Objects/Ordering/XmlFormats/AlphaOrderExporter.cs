using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Configuration;
using System.Xml;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Utility.Address;
using Concentrator.Objects;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Ordering.XmlFormats
{
  public class AlphaOrderExporter : BaseOrderExporter<XDocument>
  {
    public override XDocument GetOrder(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor)
    {
      var orderGroups = orderLines;

      XDocument doc = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"),
                                   new XElement("Orders", new XAttribute("Version", vendor.VendorSettings.GetValueByKey("AlphaCurrentVersion", string.Empty)),
        //orders
                                     from ln in orderGroups
                                     let centralDelivery = ln.Value.FirstOrDefault().CentralDelivery.HasValue ? ln.Value.FirstOrDefault().CentralDelivery.Value : false
                                     select new XElement("Order",

                                       //sender element
                                       new XElement("Sender",
                                         new XElement("CustNr", vendor.VendorSettings.GetValueByKey("AlphaCustomerNumber", string.Empty)),
                                         new XElement("Address", new XAttribute("Type", "Shipping"), new XAttribute("ID", !centralDelivery ? "D" : "S"),
                                           new XElement("Name", ln.Key.ShippedToCustomer.CustomerName),
                                           new XElement("Street", AddressUtility.FormatAddress(ln.Key.ShippedToCustomer.CustomerAddressLine1).Street),
                                           new XElement("Number", !string.IsNullOrEmpty(ln.Key.ShippedToCustomer.HouseNumber) ? ln.Key.ShippedToCustomer.HouseNumber : AddressUtility.FormatAddress(ln.Key.ShippedToCustomer.CustomerAddressLine1).HouseNumber),
                                           new XElement("PC", ln.Key.ShippedToCustomer.PostCode),
                                           new XElement("City", ln.Key.ShippedToCustomer.City),
                                           new XElement("Country", ln.Key.ShippedToCustomer.Country)
                                           )),
                                       //recepient element
                                         new XElement("Recipient",
                                           new XElement("Address", new XAttribute("Type", "Billing"),
                                             new XElement("Name", vendor.VendorSettings.GetValueByKey("AlphaInfoName", string.Empty)),
                                             new XElement("Street", vendor.VendorSettings.GetValueByKey("AlphaInfoStreet", string.Empty)),
                                             new XElement("PC", vendor.VendorSettings.GetValueByKey("AlphaInfoPC", string.Empty)),
                                             new XElement("City", vendor.VendorSettings.GetValueByKey("AlphaInfoCity", string.Empty)),
                                             new XElement("Country", vendor.VendorSettings.GetValueByKey("AlphaInfoCountry", string.Empty)))),
                                         new XElement("OrderDate", ln.Key.ReceivedDate.ToString("yyyy-MM-dd")),
                                         new XElement("Reference", ln.Key.OrderID + "/" + ln.Key.WebSiteOrderNumber), //(purchaseOrder != null ? "/" + purchaseOrder.VendorDocumentNumber : string.Empty)),
                                         new XElement("PartialDelivery", "N"),
                                         new XElement("Currency", "EUR"),
                                         new XElement("Remark", ln.Key.Remarks),
                                       //#if DEBUG
                                       // new XElement("Environment", "T"),
                                       //#else
new XElement("Environment", "L"),
                                       //#endif
new XElement("OrderLines",
                                           from ol in ln.Value
                                           let assortmentItem = ol.Product.VendorAssortments.FirstOrDefault(c => c.VendorID == vendor.VendorID)
                                           let purchaseLine = ol.OrderResponseLines.Where(x => x.OrderLineID == ol.OrderLineID && x.OrderResponse.ResponseType == OrderResponseTypes.PurchaseAcknowledgement.ToString()).FirstOrDefault()
                                           select new XElement("OrderLine", new XAttribute("ID", ol.OrderLineID),
                                             new XElement("Item", new XAttribute("Type", "S"),
                                               new XElement("ItemCode", assortmentItem.CustomItemNumber)),
                                               new XElement("Item", new XAttribute("Type", "C"),
                                               new XElement("ItemCode", purchaseLine != null ? purchaseLine.VendorItemNumber : assortmentItem.ProductID.ToString())),
                                               new XElement("Description", assortmentItem.ShortDescription),
                                               new XElement("Quantity", ol.GetDispatchQuantity()),
                                               new XElement("Unit", "PCS"),
                                               new XElement("Price", assortmentItem.VendorPrices.FirstOrDefault() != null ? assortmentItem.VendorPrices.FirstOrDefault().Price.Value : (decimal)ol.Price)
                                               )
                                           )
                                       )
                                     ));
      return doc;
    }

    public override Concentrator.Web.Objects.EDI.DirectShipment.DirectShipmentRequest GetDirectShipmentOrder(Concentrator.Objects.Models.Orders.Order order, List<OrderLine> orderLines, Vendor administrativeVendor, Vendor vendor)
    {
      throw new NotImplementedException();
    }
  }
}
