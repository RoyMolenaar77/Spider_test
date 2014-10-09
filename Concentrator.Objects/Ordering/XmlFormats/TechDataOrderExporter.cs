using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Configuration;
using System.Xml;
using Concentrator.Objects;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.Objects.Ordering.XmlFormats
{
  public class TechDataOrderExporter : BaseOrderExporter<XDocument>
  {
    #region Example Xml Format
    //    <?xml version="1.0" encoding="ISO-8859-1" ?>
    //<!DOCTYPE OrderEnv SYSTEM " here goes the DTD ">
    //<OrderEnv AuthCode="123456" MsgID="1">
    //<Order Currency="CUR">
    //<Head>
    //<Title>MyPO</Title>
    //<OrderDate>20071210</OrderDate>
    //<DeliverTo>
    //<Address>
    //<Name1>Company</Name1>
    //<Name2>Attention</Name2>
    //<Name3 />
    //<Name4 />
    //<Street>The road 1</Street>
    //<ZIP>zip code</ZIP>
    //<City>City</City>
    //<Country>XY</Country>
    //<ContactName>Contact</ContactName>
    //<ContactPhone>089123456789</ContactPhone>
    //<ContactMail>contact@xxx.com</ContactMail>
    //</Address>
    //</DeliverTo>
    //<Delivery Type="XY" Full="n" />
    //<OrigPO>Reference on order header</OrigPO>
    //<FreeTxt Type="hold">Please check price! /Customer</FreeTxt>
    //</Head>
    //<Body>
    //<Line ID="1">
    //<ItemID>123456</ItemID>
    //<Qty>1</Qty>
    //<Price>100</Price>
    //<FreeTxt>Free text on line level no1</FreeTxt>
    //<FreeTxt>Free text on line level no2</FreeTxt>
    //</Line>
    //<Line ID="4">
    //<ItemID>123456</ItemID>
    //<Qty>2</Qty>
    //<Price>300</Price>
    //<OrigPO>Reference on line level</OrigPO>
    //</Line>
    //</Body>
    //</Order>
    //</OrderEnv>
    #endregion

    public override XDocument GetOrder(Dictionary<Concentrator.Objects.Models.Orders.Order,List<OrderLine>> orderLines, Vendor vendor)
    {
      string dtd = vendor.VendorSettings.GetValueByKey("TechDataXsdURL",string.Empty);
      var msgID = orderLines.FirstOrDefault().Value.FirstOrDefault().OrderLineID;
      XDocument doc = new XDocument(new XDeclaration("1.0", "ISO-8859-1", "yes"),
                                  new XDocumentType("OrderEnv", null, dtd, null),
                                  new XElement("OrderEnv",
                                  new XAttribute("AuthCode", vendor.VendorSettings.GetValueByKey("TechDataAuthorizationCode",string.Empty)),
                                  new XAttribute("MsgID", msgID),
                                  from ln in orderLines
                                  let order = ln.Key
                                  let orderLinesList = ln.Value
                                  //order segment
                                  select new XElement("Order", new XAttribute("Currency", "EUR"),
                                    //head segment
                                          new XElement("Head",
                                            new XElement("Title", ln.Key.OrderID),
                                            new XElement("OrderDate", order.ReceivedDate.ToString("yyyymmdd")),
                                            new XElement("DeliverTo",
                                              new XElement("Address",
                                                new XElement("Name1", ln.Key.ShippedToCustomer.CustomerName),
                                                new XElement("Street", ln.Key.ShippedToCustomer.CustomerAddressLine1),
                                                new XElement("ZIP", ln.Key.ShippedToCustomer.PostCode),
                                                new XElement("City", ln.Key.ShippedToCustomer.City),
                                                new XElement("Country", ln.Key.ShippedToCustomer.Country),
                                                new XElement("ContactName", ln.Key.ShippedToCustomer.CustomerName),
                                                new XElement("ContactPhone", ln.Key.ShippedToCustomer.CustomerTelephone),
                                                new XElement("ContactMail", ln.Key.ShippedToCustomer.CustomerEmail)
                                      )),
                                      new XElement("Delivery", new XAttribute("Type", "XY"), new XAttribute("Full", "n"))
                                        ),
                                    //body segment
                                          new XElement("Body",
                                            (from c in order.OrderLines
                                             select new XElement("Line", new XAttribute("ID", c.OrderLineID),
                                               new XElement("ItemID", c.Product.VendorAssortments.FirstOrDefault(va => va.VendorID == vendor.VendorID).CustomItemNumber),
                                               new XElement("Qty", c.GetDispatchQuantity())
                                               ))
                                        ))));

      return doc;

    }

    public override Concentrator.Web.Objects.EDI.DirectShipment.DirectShipmentRequest GetDirectShipmentOrder(Concentrator.Objects.Models.Orders.Order order, List<OrderLine> orderLines, Vendor administrativeVendor, Vendor vendor)
    {
      throw new NotImplementedException();
    }
  }
}
