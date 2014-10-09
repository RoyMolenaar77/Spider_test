using System;
using System.Collections.Generic;
using System.Linq;
using AuditLog4Net.Adapter;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using System.IO;
using System.Configuration;


namespace Concentrator.Objects.Ordering.Dispatch
{
  public class BrightPointDispatcher : IDispatchable
  {
    public enum OrderStatus
    {
      //Ordered = "Ordered",
      //Reserved = "Reserved",
      //Delivered = "Delivered", // Order has left Brightpoint
      //PartiallyDelivered = "PartiallyDelivered",
      //Invoiced = "Invoiced", // Order has left Brightpoint is now invoiced
      //CreditBlocked = "CreditBlocked", //Order has been blocked due to financial reasons
      //Cancelled = "Cancelled"// Order has been manually cancelled
    }



    public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, IUnitOfWork unit)
    {
      var vendorID = vendor.VendorID;

      var CustNo = vendor.VendorSettings.GetValueByKey("BrightPointCustomerNo", string.Empty);
      var Pass = vendor.VendorSettings.GetValueByKey("BrightPointPassword", string.Empty);
      var Instance = vendor.VendorSettings.GetValueByKey("BrightPointInstance", string.Empty);
      var Site = vendor.VendorSettings.GetValueByKey("BrightPointSite", string.Empty);
      var ConfirmMail = vendor.VendorSettings.GetValueByKey("ConfirmationMailAdress", string.Empty);

      var OrdersToProcess = new Dictionary<int, bool>();


      BrightPointOrder.clsWsOrderSoapClient client = new BrightPointOrder.clsWsOrderSoapClient();

      // Concentrator.Web.ServiceClient.B
      foreach (var order in orderLines.Keys)
      {
        OrdersToProcess.Add(order.OrderID, false);

        BrightPointOrder.Order_V11 newOrder = new BrightPointOrder.Order_V11();

        newOrder.SecurityToken = new BrightPointOrder.SecurityToken();
        newOrder.SecurityToken.customerNo = CustNo;
        newOrder.SecurityToken.password = Pass;

        newOrder.oOrders = new BrightPointOrder.Order1_1
        {
          //Header met order info
          oOrderH = new BrightPointOrder.OrderHeader1_1
          {
            sMessageType = "WEBSERVICE",
            sMessageDate = DateTime.Now.Date.ToString("yyyyMMdd"),
            sMessageTime = DateTime.Now.ToString("Hhmmss"),
            sSite = "GBG",
            sOrderType = "WSO",
            sSenderOrderNumberAtBP = order.OrderID.ToString(),
            sOrderDate = DateTime.Now.Date.ToString("yyyyMMdd"),
            sSenderCustomerNumberAtBP = CustNo,
            sSenderDeliveryNote = "TRUE",
            sOrderConfirmationEmailAddress = ConfirmMail,
            sConfirmationEmailAddress = ConfirmMail,
            sUse3PReceiver = "TRUE",
            sReceiverName = order.ShippedToCustomer.CustomerName,
            sReceiverDeliveryAddress1 = order.ShippedToCustomer.CustomerAddressLine1,
            sReceiverDeliveryPostalCode = order.ShippedToCustomer.PostCode,
            sReceiverDeliveryCity = order.ShippedToCustomer.City,
            sReceiverDeliveryCounty = order.ShippedToCustomer.Country,
            sReceiverEmail = order.ShippedToCustomer.CustomerEmail,
            sReceiverTelephone = order.ShippedToCustomer.CustomerTelephone,
            sSenderEAN = order.ShippedToCustomer.EANIdentifier,
            sSenderInvoice = "TRUE",
            sSenderOrderConfirmation = "TRUE"

          },
          oOrderR = new BrightPointOrder.OrderRows1_1[order.OrderLines.Count]
        };

        int count = 0;
        foreach (var itemOrder in order.OrderLines)
        {
          //order rows
          newOrder.oOrders.oOrderR[count] = new BrightPointOrder.OrderRows1_1
          {
            iOrderLineNumber = itemOrder.OrderLineID,
            sEANArticleNumber = itemOrder.Product.ProductBarcodes.FirstOrDefault() != null ? itemOrder.Product.ProductBarcodes.FirstOrDefault().Barcode : null,
            iNumberOfArticle = itemOrder.GetDispatchQuantity(),
            dReceiverPriceAtSender = 0//(double)itemOrder.Price.Value
          };
          count++;
        }
        
        BrightPointOrder.Response response = client.SendBPeDI(newOrder.SecurityToken, newOrder.oOrders);
        if (response.aErrorCode == null)
        {
          //order successfully placed
          foreach (var ob in OrdersToProcess.Keys)
          {
            //update
            OrdersToProcess[ob] = true;
          }
        }
        else
        {
          string error = string.Format("OrderID: {0} Error: ", order.OrderID);
          foreach (int code in response.aErrorCode)
          {
            switch (code)
            {
              case 110:
                error += "110 Internal Connection Error, ";
                break;
              case 120:
                error += "120 Internal Webservice Error, ";
                break;
              case 210:
                error += "210 Internal IFS Connection Error, ";
                break;
              case 225:
                error += "225 sOrderType (Invalid Order Type), ";
                break;
              case 410:
                error += "410 IFS SOAP Gateway Error, ";
                break;
              case 500:
                error += "500 The field is invalid, ";
                break;
              case 669:
                error += "669 Invalid request, ";
                break;
              case 700:
                error += "700 Mandatory field, ";
                break;
              case 701:
                error += "701 The field is to long, ";
                break;
              case 702:
                error += "702 The field must be a specified length, ";
                break;
              case 704:
                error += "704 Is mandatory because of another field, ";
                break;
              case 705:
                error += "705 Invalid date format, ";
                break;
              case 750:
                error += "750 Communication error, ";
                break;
              case 800:
                error += "800 Login failed, ";
                break;
              case 801:
                error += "801 You are not registered as a web service-user for BPeDI, ";
                break;
              case 900:
                error += "900 Duplicates of SenderOrderNumberAtBP, ";
                break;
              default:
                break;
            }
          }
          throw new Exception(error);
        }

        LogOrder(newOrder, vendor.VendorID, string.Format("{0}.xml", order.OrderID), log);
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

        var order = (BrightPointOrder.Order1_1)orderInformation;

        System.Xml.Serialization.XmlSerializer x = new System.Xml.Serialization.XmlSerializer(order.GetType());

        using (FileStream fs = File.Open(
                                         Path.Combine(logPath, fileName),
                                         FileMode.OpenOrCreate,
                                         FileAccess.Write,
                                         FileShare.ReadWrite))
        {
          x.Serialize(fs, order);
        }
      }
      catch (Exception ex)
      {
        log.AuditError("Failed to log order information for " + vendorID, ex);
      }
    }


    public void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, string logPath, IUnitOfWork unit)
    {

      var vendorID = vendor.VendorID;

      var CustNo = vendor.VendorSettings.GetValueByKey("BrightPointCustomerNo", string.Empty);
      var Pass = vendor.VendorSettings.GetValueByKey("BrightPointPassword", string.Empty);
      var Instance = vendor.VendorSettings.GetValueByKey("BrightPointInstance", string.Empty);
      var Site = vendor.VendorSettings.GetValueByKey("BrightPointSite", string.Empty);

      var orderResponseDate = unit.Scope.Repository<OrderResponse>().GetAll(x => x.VendorID == vendor.VendorID).ToList().Count > 0 ? unit.Scope.Repository<OrderResponse>().GetAll(x => x.VendorID == vendor.VendorID).Max(x => x.ReceiveDate).ToString("yyyy-MM-dd HH:mm:ss") : null;



      //get order info
      BrightPointOrderInfo.OrderSoapClient infoClient = new BrightPointOrderInfo.OrderSoapClient();
      BrightPointOrderInfo.AuthHeaderUser authHeader = new BrightPointOrderInfo.AuthHeaderUser();
      authHeader.sCustomerNo = CustNo;
      authHeader.sInstance = Instance;
      authHeader.sPassword = Pass;
      authHeader.sSite = Site;

      var date = orderResponseDate != null ? orderResponseDate : DateTime.Now.Date.ToString("yyyy-MM-dd HH:mm:ss");

      BrightPointOrderInfo.Orderhead[] orders = infoClient.getOrderHistory_v1_0(authHeader, date);

      foreach (var info in orders)
      {
        int OrderID = 0;

        Int32.TryParse(info.OrderNo, out OrderID);

        if (OrderID != 0)
        {
          var order = unit.Scope.Repository<Order>().GetSingle(x => x.OrderID == OrderID);
          if (order != null)
          {
            OrderResponseLine orderResponseLine = null;
            OrderResponse orderResponse = null;
            var _orderResponseRepo = unit.Scope.Repository<OrderResponse>();
            string OrderStatus = info.OrderStatus;
            switch (OrderStatus)
            {
              case "Delivered":
                if (orderResponse == null)
                {
                  orderResponse = new OrderResponse
                  {
                    Order = order,
                    VendorDocument = "",
                    VendorID = vendorID,
                    OrderDate = DateTime.Parse(info.EnteredDate),
                    ReceiveDate = DateTime.Parse(info.ModifiedDate),
                    VendorDocumentNumber = order.OrderID.ToString(),
                    TrackAndTrace = string.Join(",", info.TrackTraces.Select(x => x.TrackTraceNum))
                  };
                  _orderResponseRepo.Add(orderResponse);
                }
                orderResponse.ResponseType = OrderResponseTypes.ShipmentNotification.ToString();

                break;
              case "PartiallyDelivered":
                if (orderResponse == null)
                {
                  orderResponse = new OrderResponse
                  {
                    Order = order,
                    VendorDocument = "",
                    VendorID = vendorID,
                    OrderDate = DateTime.Parse(info.EnteredDate),
                    ReceiveDate = DateTime.Parse(info.ModifiedDate),
                    PartialDelivery = true,
                    VendorDocumentNumber = order.OrderID.ToString()
                  };
                  _orderResponseRepo.Add(orderResponse);
                }
                orderResponse.ResponseType = OrderResponseTypes.Acknowledgement.ToString();

                break;
              case "Invoiced":
                if (orderResponse == null)
                {
                  orderResponse = new OrderResponse
                  {
                    Order = order,
                    VendorDocument = "",
                    VendorID = vendorID,
                    OrderDate = DateTime.Parse(info.EnteredDate),
                    ReceiveDate = DateTime.Parse(info.ModifiedDate),
                    VendorDocumentNumber = order.OrderID.ToString(),
                    InvoiceDocumentNumber = info.ReceiverInvoiceNo
                  };
                  _orderResponseRepo.Add(orderResponse);
                }
                orderResponse.ResponseType = OrderResponseTypes.Acknowledgement.ToString();

                break;
              case "Reserved":
                if (orderResponse == null)
                {
                  orderResponse = new OrderResponse
                  {
                    Order = order,
                    VendorDocument = "",
                    VendorID = vendorID,
                    OrderDate = DateTime.Parse(info.EnteredDate),
                    ReceiveDate = DateTime.Parse(info.ModifiedDate),
                    VendorDocumentNumber = order.OrderID.ToString(),
                    InvoiceDocumentNumber = info.ReceiverInvoiceNo
                  };
                  _orderResponseRepo.Add(orderResponse);
                }
                orderResponse.ResponseType = OrderResponseTypes.Acknowledgement.ToString();

                break;
              case "Cancelled":
                if (orderResponse == null)
                {
                  orderResponse = new OrderResponse
                  {
                    Order = order,
                    VendorDocument = "",
                    VendorID = vendorID,
                    OrderDate = DateTime.Parse(info.EnteredDate),
                    ReceiveDate = DateTime.Now,
                    VendorDocumentNumber = order.OrderID.ToString()
                  };
                  _orderResponseRepo.Add(orderResponse);
                }
                orderResponse.ResponseType = OrderResponseTypes.CancelNotification.ToString();

                break;
              default:
                log.WarnFormat("Unknown order status");
                break;
            }
            foreach (var line in info.OrderLines)
            {
              int OrderLineNumber = 0;
              Int32.TryParse(line.LineNumber, out OrderLineNumber);

              if (OrderLineNumber != 0)
              {
                var orderLine = order.OrderLines.FirstOrDefault(x => x.OrderLineID == OrderLineNumber);
                if (orderLine != null)
                {
                  orderResponseLine = new OrderResponseLine
                  {
                    OrderResponse = orderResponse,
                    OrderLineID = orderLine.OrderLineID,
                    Ordered = Int32.Parse(line.AssignedQty),
                    Backordered = 0,
                    Cancelled = Int32.Parse(line.AssignedQty) - Int32.Parse(line.BoughtQty),
                    Shipped = Int32.Parse(line.ShippedQty),
                    SerialNumbers = string.Join(",", line.serials.serial.Select(x => x.Value)),
                    Price = (decimal)orderLine.Price.Value,
                    Processed = false,
                    Delivered = 0,
                    Description = line.Description,
                    Invoiced = OrderStatus != "Delivered" ? 0 : Int32.Parse(line.BoughtQty),
                    Barcode = line.EANCode
                  };

                  unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);
                }
              }
              unit.Save();
            }
          }
        }
      }

    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath)
    {
      throw new System.NotImplementedException();
    }
  }
}
