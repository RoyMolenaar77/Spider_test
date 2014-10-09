using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using AuditLog4Net.Adapter;
using Concentrator.Objects;
using System.Configuration;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Ordering.XmlClasses;
using Concentrator.Objects.RED;

namespace Concentrator.Objects.Ordering.Dispatch
{
  public class ArvatoDispatcher : IDispatchable
  {
    public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor, IAuditLogAdapter log, IUnitOfWork unit)
    {
      var vendorID = vendor.VendorID;

      List<string> orders = new List<string>();
      //List<Offer> offers = new List<Offer>();

      using (RetailerSoapClient client = new RetailerSoapClient())
      {
        AuthenticationHeader authHeader = new AuthenticationHeader();

        authHeader.Username = vendor.VendorSettings.GetValueByKey("ArvatoUserName", string.Empty);
        authHeader.Password = vendor.VendorSettings.GetValueByKey("ArvatoPassword", string.Empty);

        foreach (var order in orderLines.Keys)
        {
          var Order = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Order>().GetSingle(o => o.OrderID == order.OrderID);

          try
          {
            //***Begin order reserveration request***///
            if (client.BeginOrderReservationRequest(authHeader, order.OrderID.ToString()) == "OK")
            {
              log.Info("Successfully begun order reservation");

              Order_Shipment_Request orderShiptMentRequest = new Order_Shipment_Request();
              orderShiptMentRequest.Order_ID = order.OrderID.ToString();

              Shipment shipment = new Shipment();

              List<Shipment_Item> shipmentItems = new List<Shipment_Item>();

              //Track shipmentitem amount
              int shipmentItemsCount = 0;

              //Counter for loop
              int count = 0;

              //Total ESD product price
              decimal totalPrice = 0;

              //Total price for items who need shipping
              decimal totalShipmentPrice = 0;

              //List of items that need to be shipped
              List<OrderLine> orderLineReserved = new List<OrderLine>();

              foreach (var detail in orderLines[order])
              {
                var orderLine = (from line in Order.OrderLines
                                 where line.OrderLineID == detail.OrderLineID
                                 select line).FirstOrDefault();

                var relatedProducts = (from prod in orderLine.Product.RelatedProductsRelated
                                       select prod).ToList();

                //var productType = orderLine.Product.ProductAttributeValues.Where(value => value.Value)
                var vendorAssortment = unit.Scope.Repository<VendorAssortment>().GetAll(assortment => assortment.VendorID == vendor.VendorID && assortment.ProductID == orderLine.ProductID).FirstOrDefault();

                var Offer_ID = vendorAssortment.ActivationKey.ToString();
                var Shipment_ReferenceID = vendorAssortment.ShipmentRateTableReferenceID;
                var Zone_ReferenceID = vendorAssortment.ZoneReferenceID;
                var Price = (vendorAssortment.VendorPrices.FirstOrDefault() != null ? vendorAssortment.VendorPrices.FirstOrDefault().Price.Value : (decimal)orderLine.Price) * 100;
                totalPrice += Price;
                var Quantity = orderLine.GetDispatchQuantity();

                //check if product is primary
                var productType =
                  orderLine.Product.ProductAttributeValues.Where(
                    name => name.ProductAttributeMetaData.AttributeCode == "Product_Type").FirstOrDefault().Value;

                if (productType == "Supplementary")
                {
                  shipmentItems.Add(new Shipment_Item
                  {
                    Line_Item_ID = orderLine.OrderLineID.ToString(),
                    Quantity = Quantity.ToString()
                  });
                  shipmentItemsCount++;
                }

                ////***Add order item***//
                XDocument addOrderItemResponse =
                XDocument.Parse(client.AddOrderItemRequest(authHeader, order.OrderID.ToString(),
                                                           orderLine.OrderLineID.ToString(), Offer_ID, (decimal)Price,
                                                       "EUR", Quantity, "", ""));

                int relCount = 1;
                foreach (var relatedProduct in relatedProducts)
                {
                  //check if product is supplementary
                  var relProductType =
                    relatedProduct.SourceProduct.ProductAttributeValues.Where(
                      name => name.ProductAttributeMetaData.AttributeCode == "Product_Type").FirstOrDefault().Value;

                  var relatedProductPrice = relatedProduct.SourceProduct.VendorAssortments.FirstOrDefault().VendorPrices.FirstOrDefault().Price;

                  var relVendorAssortment = unit.Scope.Repository<VendorAssortment>().GetAllAsQueryable(assortment => assortment.VendorID == vendor.VendorID && assortment.ProductID == relatedProduct.ProductID);

                  var relOffer_ID = relVendorAssortment.FirstOrDefault().ActivationKey.ToString();

                  XDocument addRelatedOrderItemResponse =
                      XDocument.Parse(client.AddOrderItemRequest(authHeader, order.OrderID.ToString(),
                                                                 (orderLine.OrderLineID.ToString() + "_" +
                                                                  relCount.ToString()), relOffer_ID, relatedProductPrice.Value,
                                                                 "EUR", Quantity, "", ""));

                  if (relProductType == "Supplementary")
                  {
                    totalShipmentPrice += relatedProductPrice.Value;

                    shipmentItems.Add(new Shipment_Item
                    {
                      Line_Item_ID = (orderLine.OrderLineID.ToString() + "_" + relCount.ToString()),
                      Quantity = Quantity.ToString()
                    });
                    shipmentItemsCount++;
                  }
                  relCount++;
                }

                var status = (from orderResponseStatus in addOrderItemResponse.Element("Order_Fulfillment_Reservation").Elements()
                              where orderResponseStatus.Name == "Order_Status"
                              select orderResponseStatus).FirstOrDefault();

                log.InfoFormat("Order item {0} status {1}", orderLine.OrderLineID.ToString(), status.Value);

                //set item status
                if (status.Value.Equals("Reserved"))
                {
                  orderLineReserved.Add(orderLine);
                  var item = unit.Scope.Repository<OrderLine>().GetSingle(prod => prod.OrderLineID == orderLine.OrderLineID);

                  item.SetStatus(OrderLineStatus.Processed, unit.Scope.Repository<OrderLedger>());
                }

                shipment.Zone_Reference_ID = Zone_ReferenceID;
                shipment.Shipment_Rate_Table_Reference_ID = Shipment_ReferenceID;

                count++;

              }

              /************************Fill shipping information**************************/
              string firstName = order.ShippedToCustomer.CustomerName.Split(' ')[0];
              string lastName = order.ShippedToCustomer.CustomerName.Substring((firstName.Length));

              shipment.Shipment_Packages = new Shipment_Package[1];
              shipment.Shipment_Packages[0] = new Shipment_Package
              {
                Currency_Code = redCurrencyCode.EUR,
                Shipment_Items = shipmentItems.ToArray(),
                Shipment_Amount_Total = (decimal)totalPrice
              };

              shipment.Shipment_Address = new Shipment_Address();
              shipment.Shipment_Address.Address_One = order.ShippedToCustomer.CustomerAddressLine1;
              //shipment.Shipment_Address.Address_Two = order.ShipToCustomer.CustomerAddressLine2;
              //shipment.Shipment_Address.Address_Three = order.ShipToCustomer.CustomerAddressLine3;
              shipment.Shipment_Address.City = order.ShippedToCustomer.City;
              shipment.Shipment_Address.Country_Code = order.ShippedToCustomer.Country;
              //shipment.Shipment_Address.State_Code = "";
              shipment.Shipment_Address.Zip_Code = order.ShippedToCustomer.PostCode;
              shipment.Shipment_Address.Phone = order.ShippedToCustomer.CustomerTelephone;
              shipment.Shipment_Address.First_Name = firstName;
              shipment.Shipment_Address.Last_Name = lastName;

              orderShiptMentRequest.Shipment = new Shipment[1];
              orderShiptMentRequest.Shipment[0] = shipment;
              /************************End Fill shipping information************************/

              log.AuditInfo("Successfully added order items to order");

              if (client.CompleteOrderReservationRequest(authHeader, order.OrderID.ToString(), "") == "OK")
              {
                log.AuditInfo("Order reservation successfully made");

                string requestXml = string.Empty;
                StringBuilder requestString = new StringBuilder();
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Encoding = Encoding.UTF8;
                using (XmlWriter xw = XmlWriter.Create(requestString, settings))
                {
                  xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
                  XmlSerializer serializer = new XmlSerializer(orderShiptMentRequest.GetType());
                  XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
                  nm.Add("xsi", "urn:Order_Shipment_Request");
                  serializer.Serialize(xw, orderShiptMentRequest);

                  XmlDocument document = new XmlDocument();
                  document.LoadXml(requestString.ToString());
                  document.DocumentElement.RemoveAttribute("xmlns:xsi");
                  document.DocumentElement.RemoveAttribute("xmlns:xsd");
                  requestXml = document.OuterXml;

                  LogOrder(document, vendor.VendorID, string.Format("{0}.xml", order.OrderID), log);
                }

                ////***Order shipment request***//
                if (shipmentItemsCount > 0)
                {
                  string response = client.OrderShipmentRequest(authHeader, requestXml.ToString());
                  if (response == "OK")
                  {
                    log.InfoFormat("Order reservation successfully made");
                  }
                }
                else
                {
                  log.InfoFormat("No items need shipping");
                }

                if (client.CommitToPurchaseOrderRequest(authHeader, order.OrderID.ToString(), order.OrderID.ToString()) == "OK")
                {
                  log.Info("Order succesfully purchased");

                  log.Info("Processing product information");

                  XDocument[] orderInformation = new XDocument[orderLineReserved.Count];

                  int oc = 0;
                  if (orderLineReserved.Count > 0)
                  {
                    OrderResponse orderResponse = null;

                    foreach (var orderLine in orderLineReserved)
                    {
                      OrderResponseLine orderResponseLine = null;

                      orderInformation[oc] =
                        XDocument.Parse(client.PurchaseOrderItemFulfillmentRequest(authHeader, order.OrderID.ToString(),
                                                                                   orderLine.OrderLineID.ToString(), ""));
                      //#endif
                      var OrderStatus = orderInformation[oc].Element("Order_Fulfillment").Element("Order_Status").Value;
                      var OrderDate = orderInformation[oc].Element("Order_Fulfillment").Element("Order_Date").Value;
                      var Quantity = orderInformation[oc].Element("Order_Fulfillment").Element("Quantity").Value;

                      var getMaxShippingQuantity = orderLine.Product.ProductAttributeValues.Where(
                      name => name.ProductAttributeMetaData.AttributeCode == "Max_Quantity_Per_Shipment_Charge").FirstOrDefault();

                      var MaxShippingQuantity = getMaxShippingQuantity != null ? Int32.Parse(getMaxShippingQuantity.Value) : 0;
                      var _orderResponseRepo = unit.Scope.Repository<OrderResponse>();

                      switch (OrderStatus)
                      {
                        case "Shipped":
                          if (orderResponse == null)
                          {
                            orderResponse = new OrderResponse
                            {
                              Orders = new List<Concentrator.Objects.Models.Orders.Order>() { Order },
                              VendorDocument = orderInformation[oc].Document.ToString(),
                              VendorID = vendorID,
                              OrderDate = DateTime.Parse(OrderDate),
                              ReceiveDate = DateTime.Now,
                              PartialDelivery = MaxShippingQuantity != 0 ? Int32.Parse(Quantity) < MaxShippingQuantity
                                  ? false
                                  : true : false,
                              VendorDocumentNumber = Order.OrderID.ToString()
                            };
                            _orderResponseRepo.Add(orderResponse);
                          }
                          orderResponse.ResponseType = OrderResponseTypes.ShipmentNotification.ToString();

                          break;
                        case "ShipmentPending":
                          if (orderResponse == null)
                          {
                            orderResponse = new OrderResponse
                            {
                              Orders = new List<Concentrator.Objects.Models.Orders.Order>() { Order },
                              VendorDocument = orderInformation[oc].Document.ToString(),
                              VendorID = vendorID,
                              OrderDate = DateTime.Parse(OrderDate),
                              ReceiveDate = DateTime.Now,
                              PartialDelivery =
                                Int32.Parse(Quantity) < MaxShippingQuantity
                                  ? false
                                  : true,
                              VendorDocumentNumber = Order.OrderID.ToString()
                            };
                            _orderResponseRepo.Add(orderResponse);
                          }
                          orderResponse.ResponseType = OrderResponseTypes.Acknowledgement.ToString();

                          break;
                        default:
                          log.WarnFormat("Unknown order status");
                          break;
                      }

                      var formattedBlocks =
                        (from attribute in
                           orderInformation[oc].Element("Order_Fulfillment").Element("Fulfillment_Block").Elements()
                         where attribute.Name == "Preformatted_Block"
                         select new
                         {
                           Type = attribute.Attribute("Type").Value,
                           Text = attribute.Value
                         });

                      var productName =
                        (from attribute in
                           orderInformation[oc].Element("Order_Fulfillment").Element("Fulfillment_Block").Element(
                           "Fulfillment_Set").Elements()
                         where attribute.Name == "Header"
                         select attribute.Value).FirstOrDefault();

                      var productInformation =
                        (from attribute in
                           orderInformation[oc].Element("Order_Fulfillment").Element("Fulfillment_Block").Element(
                           "Fulfillment_Set").
                           Elements(
                           "Fulfillment_Item")
                         select new
                         {

                           FullfillmentAtoms =
                  (from attribute2 in
                     attribute.Elements("Fulfillment_Atom_Section").Elements("Fulfillment_Atom")
                   select new
                   {
                     Header = attribute2.Parent.Element("Header") != null ? attribute2.Parent.Element("Header").Value : string.Empty,
                     Fulfillment_Atom = (from att in attribute2.Attributes()
                                         select new
                                         {
                                           Name = att.Name,
                                           Value = att.Value
                                         }).ToDictionary(x => x.Name, x => x.Value),
                     Value = attribute2.Value
                   })
                         });

                      orderResponseLine = new OrderResponseLine
                      {
                        OrderResponse = orderResponse,
                        OrderLineID = orderLine.OrderLineID,
                        Ordered = OrderStatus != "Shipped" ? 0 : Int32.Parse(Quantity),
                        Backordered = OrderStatus != "Shipped" ? Int32.Parse(Quantity) : 0,
                        Cancelled = 0,
                        Shipped = OrderStatus != "Shipped" ? 0 : Int32.Parse(Quantity),
                        //Invoiced 
                        Price = (decimal)orderLine.Price.Value,
                        Processed = false,
                        Delivered = 0,
                        ProductName = productName,
                        html = (from data in formattedBlocks
                                where data.Type == "Html"
                                select data.Text).FirstOrDefault(),
                        Remark = (from data in formattedBlocks
                                  where data.Type == "Text"
                                  select data.Text).FirstOrDefault(),


                      };
                      unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);

                      foreach (var inf in productInformation)
                      {
                        foreach (var att in inf.FullfillmentAtoms)
                        {
                          var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation
                          {
                            OrderResponseLine = orderResponseLine
                          };

                          foreach (var atom in att.Fulfillment_Atom.Keys)
                          {
                            if (orderItemFullfilmentInformation.GetType().GetProperties().Any(x => x.Name == atom.LocalName))
                            {
                              Type type =
                                orderItemFullfilmentInformation.GetType().GetProperty(atom.LocalName).PropertyType;

                              string value = att.Fulfillment_Atom[atom];

                              orderItemFullfilmentInformation.GetType().GetProperty(atom.LocalName).SetValue(
                                orderItemFullfilmentInformation, Convert.ChangeType(value, type), null);
                            }
                          }

                          orderItemFullfilmentInformation.Value = att.Value;
                          orderItemFullfilmentInformation.Header = att.Header;
                          unit.Scope.Repository<OrderItemFullfilmentInformation>().Add(orderItemFullfilmentInformation);
                        }
                      }
                      oc++;
                    }
                  }
                }
              }
            }
          }
          catch (Exception ex)
          {
            log.AuditError(ex);
          }
          unit.Save();
        }
        return 0;
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

        ((XmlDocument)orderInformation).Save(Path.Combine(logPath, fileName));
      }
      catch (Exception ex)
      {
        log.AuditError("Failed to log order information for " + vendorID, ex);
      }
    }

    public void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, string logPath, IUnitOfWork unit)
    {
      using (RetailerSoapClient client = new RetailerSoapClient())
      {
        AuthenticationHeader authHeader =
          new AuthenticationHeader();

        authHeader.Username = vendor.VendorSettings.GetValueByKey("ArvatoUserName", string.Empty);
        authHeader.Password = vendor.VendorSettings.GetValueByKey("ArvatoPassword", string.Empty);

        var orderLines = unit.Scope.Repository<OrderLine>().GetAllAsQueryable(x => x.DispatchedToVendorID == vendor.VendorID && x.isDispatched
            && x.OrderLedgers.Any(y => y.Status == (int)OrderLineStatus.WaitingForAcknowledgement)
            && !x.OrderLedgers.Any(z => z.Status == (int)OrderLineStatus.Processed)
            ).ToList();

        XDocument[] orderInformation = new XDocument[orderLines.Count];

        int oc = 0;

        OrderResponse orderResponse = null;

        foreach (var orderLine in orderLines)
        {
          var order = orderLine;

          OrderResponseLine orderResponseLine = null;

          orderInformation[oc] =
            XDocument.Parse(client.PurchaseOrderItemFulfillmentRequest(authHeader, order.OrderID.ToString(),
                                                                       orderLine.OrderLineID.ToString(), ""));

          var OrderStatus = orderInformation[oc].Element("Order_Fulfillment").Element("Order_Status").Value;
          var OrderDate = orderInformation[oc].Element("Order_Fulfillment").Element("Order_Date").Value;
          var Quantity = orderInformation[oc].Element("Order_Fulfillment").Element("Quantity").Value;

          var getMaxShippingQuantity = orderLine.Product.ProductAttributeValues.Where(
          name => name.ProductAttributeMetaData.AttributeCode == "Max_Quantity_Per_Shipment_Charge").FirstOrDefault();

          var MaxShippingQuantity = getMaxShippingQuantity != null ? Int32.Parse(getMaxShippingQuantity.Value) : 0;


          switch (OrderStatus)
          {
            case "Shipped":
              if (orderResponse == null)
              {
                orderResponse = new OrderResponse
                {
                  Orders = new List<Concentrator.Objects.Models.Orders.Order>() { order.Order },
                  VendorDocument = orderInformation[oc].Document.ToString(),
                  VendorID = order.Vendor.VendorID,
                  OrderDate = DateTime.Parse(OrderDate),
                  ReceiveDate = DateTime.Now,
                  PartialDelivery = MaxShippingQuantity != 0 ? Int32.Parse(Quantity) < MaxShippingQuantity
                      ? false
                      : true : false,
                  VendorDocumentNumber = order.Order.OrderID.ToString()
                };
                unit.Scope.Repository<OrderResponse>().Add(orderResponse);
              }
              orderResponse.ResponseType = OrderResponseTypes.ShipmentNotification.ToString();

              break;
            case "ShipmentPending":
              if (orderResponse == null)
              {
                orderResponse = new OrderResponse
                {
                  Orders = new List<Concentrator.Objects.Models.Orders.Order>() { order.Order },
                  VendorDocument = orderInformation[oc].Document.ToString(),
                  VendorID = order.Vendor.VendorID,
                  OrderDate = DateTime.Parse(OrderDate),
                  ReceiveDate = DateTime.Now,
                  PartialDelivery =
                    Int32.Parse(Quantity) < MaxShippingQuantity
                      ? false
                      : true,
                  VendorDocumentNumber = order.Order.OrderID.ToString()
                };
                unit.Scope.Repository<OrderResponse>().Add(orderResponse);
              }
              orderResponse.ResponseType = OrderResponseTypes.Acknowledgement.ToString();

              break;
            default:
              log.WarnFormat("Unknown order status");
              break;
          }

          var formattedBlocks =
            (from attribute in
               orderInformation[oc].Element("Order_Fulfillment").Element("Fulfillment_Block").Elements()
             where attribute.Name == "Preformatted_Block"
             select new
             {
               Type = attribute.Attribute("Type").Value,
               Text = attribute.Value
             });

          var productName =
            (from attribute in
               orderInformation[oc].Element("Order_Fulfillment").Element("Fulfillment_Block").Element(
               "Fulfillment_Set").Elements()
             where attribute.Name == "Header"
             select attribute.Value).FirstOrDefault();

          var productInformation =
            (from attribute in
               orderInformation[oc].Element("Order_Fulfillment").Element("Fulfillment_Block").Element(
               "Fulfillment_Set").
               Elements(
               "Fulfillment_Item")
             select new
             {

               FullfillmentAtoms =
      (from attribute2 in
         attribute.Elements("Fulfillment_Atom_Section").Elements("Fulfillment_Atom")
       select new
       {
         Header = attribute2.Parent.Element("Header") != null ? attribute2.Parent.Element("Header").Value : string.Empty,
         Fulfillment_Atom = (from att in attribute2.Attributes()
                             select new
                             {
                               Name = att.Name,
                               Value = att.Value
                             }).ToDictionary(x => x.Name, x => x.Value)
       })
             });

          orderResponseLine = new OrderResponseLine
          {
            OrderResponse = orderResponse,
            OrderLineID = orderLine.OrderLineID,
            Ordered = OrderStatus != "Shipped" ? 0 : Int32.Parse(Quantity),
            Backordered = OrderStatus != "Shipped" ? Int32.Parse(Quantity) : 0,
            Cancelled = 0,
            Shipped = OrderStatus != "Shipped" ? 0 : Int32.Parse(Quantity),
            //Invoiced 
            Price = (decimal)orderLine.Price.Value,
            Processed = false,
            Delivered = 0,
            ProductName = productName,
            html = (from data in formattedBlocks
                    where data.Type == "Html"
                    select data.Text).FirstOrDefault(),
            Remark = (from data in formattedBlocks
                      where data.Type == "Text"
                      select data.Text).FirstOrDefault(),


          };
          unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);

          foreach (var inf in productInformation)
          {
            foreach (var att in inf.FullfillmentAtoms)
            {
              var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation
              {
                OrderResponseLine = orderResponseLine
              };

              foreach (var atom in att.Fulfillment_Atom.Keys)
              {
                if (orderItemFullfilmentInformation.GetType().GetProperties().Any(x => x.Name == atom.LocalName))
                {
                  Type type =
                    orderItemFullfilmentInformation.GetType().GetProperty(atom.LocalName).PropertyType;

                  string value = att.Fulfillment_Atom[atom];

                  orderItemFullfilmentInformation.GetType().GetProperty(atom.LocalName).SetValue(
                    orderItemFullfilmentInformation, Convert.ChangeType(value, type), null);
                }
              }
              orderItemFullfilmentInformation.Header = att.Header;
              unit.Scope.Repository<OrderItemFullfilmentInformation>().Add(orderItemFullfilmentInformation);
            }
          }
          oc++;
        }
        unit.Save();
      }
    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }

  }
}
