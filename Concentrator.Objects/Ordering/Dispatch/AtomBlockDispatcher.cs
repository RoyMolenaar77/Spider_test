using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.AtomblockWebService;
using System.Security.Cryptography;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;
using System.Configuration;
using System.IO;
using System.Xml;

namespace Concentrator.Objects.Ordering.Dispatch
{
  class AtomBlockDispatcher : IDispatchable
  {
    public int DispatchOrders(Dictionary<Models.Orders.Order, List<Models.Orders.OrderLine>> orderLines, Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, DataAccess.UnitOfWork.IUnitOfWork uni)
    {

      var Username = vendor.VendorSettings.GetValueByKey("Username", string.Empty);
      var Secret = vendor.VendorSettings.GetValueByKey("Secret", string.Empty);

      var _orderResponseRepo = uni.Scope.Repository<OrderResponse>();
      var _orderResponseLineRepo = uni.Scope.Repository<OrderResponseLine>();
      var _orderItemFulfillmentRepo = uni.Scope.Repository<OrderItemFullfilmentInformation>();


      RetailServices10SoapClient client = new RetailServices10SoapClient();

      RetailAccount account = new RetailAccount();

      var inputString = Username + Secret;

      MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
      byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(inputString);
      byte[] byteHash = MD5.ComputeHash(byteValue);
      MD5.Clear();

      account.Client = Username;
      account.SecureHash = Convert.ToBase64String(byteHash);

      var response = client.Ping(account);

      if (response != null)
      {
        log.AuditInfo("AtomBlock services are online");

        ShopOrder[] orders = new ShopOrder[orderLines.Count()];

        int orderCount = 0;
        foreach (var order in orderLines.Keys)
        {
          int count = 0;
          double totalAmount = 0;
          ShopOrder shopOrder = new ShopOrder();
          shopOrder.Identifier = order.OrderID.ToString();
          shopOrder.IsTestOrder = true;
          shopOrder.Items = new ShopOrderItemRequest[order.OrderLines.Count()];
          //shopOrder.PaymentMethod = "????"; //SET PAYMENT

          foreach (var item in order.OrderLines)
          {
            shopOrder.Items[count] = new ShopOrderItemRequest();
            shopOrder.Items[count].Discount = 0; // ?????
            shopOrder.Items[count].ItemPrice = (int)item.Price * 100;
            shopOrder.Items[count].ProductIdentifier = item.Product.VendorAssortments.FirstOrDefault(x => x.VendorID == vendor.VendorID).CustomItemNumber;
            shopOrder.Items[count].Quantity = item.GetDispatchQuantity();
            count++;
            totalAmount += item.Price.Value;
          }

          shopOrder.TotalAmount = (int)totalAmount;
          shopOrder.TotalDiscount = 0; // ?????
          shopOrder.TotalTransactionCost = 0; // ?????
          orders[orderCount] = shopOrder;
          orderCount++;
        }

        ShopProfile profile = new ShopProfile();
        //add info?
        // profile.Email = 

        var result = client.AddOrderBatched(account, profile, orders);
        var successes = new List<ShopOrderReceipt>();
        var failures = new List<ShopOrderReceipt>();

        if (orders.Count() > 0)
          LogOrder(orders, vendor.VendorID, string.Format("{0}.xml", orders.FirstOrDefault().Identifier), log);

        foreach (var callback in result)
        {
          switch (callback.CommunicationStatus.Type)
          {
            case MessageType.ERROR:
              failures.Add(callback);
              break;
            case MessageType.NO_ACCESS:
              failures.Add(callback);
              break;
            case MessageType.OK:
              successes.Add(callback);
              break;
            case MessageType.WRONG_REQUEST:
              failures.Add(callback);
              break;
            default:
              failures.Add(callback);
              break;
          }

        }
        //all orders correctly processed
        log.AuditInfo("Processing order details");

        foreach (var success in successes)
        {
          var id = Int32.Parse(success.Reference);
          var Order = uni.Scope.Repository<Order>().GetSingle(x => x.OrderID == id);

          if (Order != null)
          {
            var orderResponse = new OrderResponse
                            {
                              Order = Order,
                              OrderDate = DateTime.Now,
                              ReceiveDate = DateTime.Now,
                              Vendor = vendor,
                              VendorDocument = "",
                              VendorDocumentNumber = Order.OrderID.ToString()
                            };
            orderResponse.ResponseType = OrderResponseTypes.ShipmentNotification.ToString();
            _orderResponseRepo.Add(orderResponse);

            var TotalAmount = 0;

            foreach (var orderline in success.OrderLines)
            {
              var name = orderline.Title;

              var OrderLine = Order.OrderLines.FirstOrDefault(x => x.Product.ProductDescriptions.FirstOrDefault(p => p.ProductName == name).ProductName == name);

              if (OrderLine != null)
              {
                var orderResponseLine = new OrderResponseLine
                {
                  OrderResponse = orderResponse,
                  OrderLine = OrderLine,
                  Ordered = orderline.Quantity,
                  Backordered = orderline.IsPreOrder ? orderline.Quantity : 0,
                  Cancelled = 0,
                  Shipped = orderline.IsPreOrder ? 0 : orderline.Quantity,
                  Invoiced = orderline.IsPreOrder ? 0 : orderline.Quantity,
                  Price = orderline.ItemPrice / 100,
                  Processed = false,
                  Delivered = 0,
                  ProductName = name,
                  Remark = orderline.IsPreOrder ? string.Format("PreOrder:{0}", orderline.PreOrderReleaseDate.ToString()) : null
                };

                _orderResponseLineRepo.Add(orderResponseLine);

                TotalAmount += orderline.ItemPrice / 100;

                if (!orderline.IsPreOrder)
                {
                  //DownloadFiles
                  foreach (var file in orderline.DownloadFiles)
                  {
                    var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation
                      {
                        OrderResponseLine = orderResponseLine
                      };
                    orderItemFullfilmentInformation.Type = "Name";
                    orderItemFullfilmentInformation.Label = "FileName";
                    orderItemFullfilmentInformation.Value = file.File;
                    _orderItemFulfillmentRepo.Add(orderItemFullfilmentInformation);
                  }

                  //Serials
                  foreach (var serial in orderline.Serials)
                  {
                    var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation
                    {
                      OrderResponseLine = orderResponseLine
                    };
                    orderItemFullfilmentInformation.Type = "Key";
                    orderItemFullfilmentInformation.Label = "Serial";
                    orderItemFullfilmentInformation.Value = serial.Code;
                    _orderItemFulfillmentRepo.Add(orderItemFullfilmentInformation);
                  }

                  //additional stuff
                  foreach (var prop in orderline.GetType().GetProperties())
                  {
                    var propName = prop.Name;
                    if (propName == "DownloadManagerUrl" || propName == "DownloadManagerReference" || propName == "SetupFileName")
                    {
                      var value = prop.GetValue(orderline, null);

                      var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation
                      {
                        OrderResponseLine = orderResponseLine
                      };
                      orderItemFullfilmentInformation.Type = propName == "DownloadManagerUrl" ? "Binary" : "Name";
                      orderItemFullfilmentInformation.Label = propName;
                      orderItemFullfilmentInformation.Value = value.ToString();
                      _orderItemFulfillmentRepo.Add(orderItemFullfilmentInformation);
                    }
                  }
                }
              }
              else
              {
              }
            }
            orderResponse.TotalAmount = TotalAmount;
          }
        }

        if (failures.Count != 0)
        {
          log.AuditError("Some orders failed to process: ");
          foreach (var failure in failures)
          {
            log.AuditError(string.Format("Failed ID: {0}", failure.OrderIdentifier));
          }
        }

      }
      else
      {
        log.AuditError("AtomBlock services are offline, or incorrect credentials");
        return -1;
      }
      uni.Save();
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

        var order = (ShopOrder[])orderInformation;

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

    public void GetAvailableDispatchAdvices(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath, DataAccess.UnitOfWork.IUnitOfWork unit)
    {
      var Username = vendor.VendorSettings.GetValueByKey("Username", string.Empty);
      var Secret = vendor.VendorSettings.GetValueByKey("Secret", string.Empty);

      var _orderResponseRepo = unit.Scope.Repository<OrderResponse>();
      var _orderResponseLineRepo = unit.Scope.Repository<OrderResponseLine>();
      var _orderItemFulfillmentRepo = unit.Scope.Repository<OrderItemFullfilmentInformation>();

      RetailServices10SoapClient client = new RetailServices10SoapClient();

      RetailAccount account = new RetailAccount();

      var inputString = Username + Secret;

      MD5CryptoServiceProvider MD5 = new MD5CryptoServiceProvider();
      byte[] byteValue = System.Text.Encoding.UTF8.GetBytes(inputString);
      byte[] byteHash = MD5.ComputeHash(byteValue);
      MD5.Clear();

      account.Client = Username;
      account.SecureHash = Convert.ToBase64String(byteHash);

      var response = client.Ping(account);

      if (response != null)
      {
        log.AuditInfo("AtomBlock services are online");

        //get orderresponses for items pre ordered
        var items = unit.Scope.Repository<OrderResponse>().GetAll(x => x.ResponseType == OrderResponseTypes.ShipmentNotification.ToString() && x.Vendor == vendor);

        foreach (var item in items)
        {
          var preOrders = item.OrderResponseLines.Where(x => x.Backordered > 0);

          var OrdersToProcess = new List<string>();

          foreach (var order in preOrders)
          {
            var TotalAmount = 0;
            var releaseDate = DateTime.Parse(order.Remark.Substring(9).Trim());
            if (releaseDate < DateTime.Now)
            {
              ////item is available so get order details
              //var productIdentifier = unit.Scope.Repository<ProductDescription>().GetSingle(x => x.ProductName == order.ProductName).Product.VendorItemNumber;//nasty

              if (order.OrderLineID.HasValue)
              {
                var success = client.GetOrderDetails(account, order.OrderLine.OrderID.ToString());
                if (success.CommunicationStatus.Type == MessageType.OK)
                {
                  var Order = order.OrderLine.Order;
                  //var Order = uni.Scope.Repository<Order>().GetSingle(x => x.OrderID == Int32.Parse(success.OrderIdentifier));
                  var orderResponse = new OrderResponse
                  {
                    Order = Order,
                    // OrderDate = Order.,
                    ReceiveDate = DateTime.Now,
                    VendorDocument = "",
                    Vendor = vendor,
                    VendorDocumentNumber = Order.OrderID.ToString()
                  };
                  orderResponse.ResponseType = OrderResponseTypes.ShipmentNotification.ToString();
                  _orderResponseRepo.Add(orderResponse);


                  foreach (var orderline in success.OrderLines)
                  {
                    var name = orderline.Title;

                    var OrderLine = Order.OrderLines.FirstOrDefault(x => x.Product.ProductDescriptions.FirstOrDefault().ProductName == name);

                    if (OrderLine != null)
                    {
                      var orderResponseLine = new OrderResponseLine
                      {
                        OrderResponse = orderResponse,
                        OrderLine = OrderLine,
                        Ordered = orderline.Quantity,
                        Backordered = orderline.IsPreOrder ? orderline.Quantity : 0, //should always be false
                        Cancelled = 0,
                        Shipped = orderline.IsPreOrder ? 0 : orderline.Quantity,
                        Invoiced = orderline.IsPreOrder ? 0 : orderline.Quantity,
                        Price = orderline.ItemPrice,
                        Processed = false,
                        Delivered = 0,
                        ProductName = name,
                        Remark = orderline.IsPreOrder ? string.Format("PreOrder:{0}", orderline.PreOrderReleaseDate.ToString()) : null
                      };

                      _orderResponseLineRepo.Add(orderResponseLine);

                      TotalAmount += orderline.ItemPrice / 100;

                      if (!orderline.IsPreOrder)
                      {
                        //DownloadFiles
                        foreach (var file in orderline.DownloadFiles)
                        {
                          var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation
                          {
                            OrderResponseLine = orderResponseLine
                          };
                          orderItemFullfilmentInformation.Type = "Name";
                          orderItemFullfilmentInformation.Label = "FileName";
                          orderItemFullfilmentInformation.Value = file.File;
                          _orderItemFulfillmentRepo.Add(orderItemFullfilmentInformation);
                        }

                        //Serials
                        foreach (var serial in orderline.Serials)
                        {
                          var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation
                          {
                            OrderResponseLine = orderResponseLine
                          };
                          orderItemFullfilmentInformation.Type = "Key";
                          orderItemFullfilmentInformation.Label = "Serial";
                          orderItemFullfilmentInformation.Value = serial.Code;
                          _orderItemFulfillmentRepo.Add(orderItemFullfilmentInformation);
                        }

                        //additional stuff
                        foreach (var prop in orderline.GetType().GetProperties())
                        {
                          var propName = prop.Name;
                          if (propName == "DownloadManagerUrl" || propName == "DownloadManagerReference" || propName == "SetupFileName")
                          {
                            var value = prop.GetValue(orderline, null);

                            var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation
                            {
                              OrderResponseLine = orderResponseLine
                            };
                            orderItemFullfilmentInformation.Type = propName == "DownloadManagerUrl" ? "Binary" : "Name";
                            orderItemFullfilmentInformation.Label = propName;
                            orderItemFullfilmentInformation.Value = value.ToString();
                            _orderItemFulfillmentRepo.Add(orderItemFullfilmentInformation);
                          }
                        }
                      }
                    }
                    else
                    {
                      //hmmzz very strange indeed

                    }
                  }
                  orderResponse.TotalAmount = TotalAmount;
                }
              }

            }

          }
        }
      }
      else
      {
        log.AuditError("AtomBlock services are offline, or incorrect credentials");
      }
    }

    public void CancelOrder(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }
  }
}
