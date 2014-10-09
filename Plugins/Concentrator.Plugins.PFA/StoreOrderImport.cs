using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.Objects.EDI;
using Excel;
using Concentrator.Web.ServiceClient.OrderInbound;
using Concentrator.Objects.Models.Orders;
using System.Globalization;
using Concentrator.Objects.Models.Products;
using Concentrator.Plugins.PFA.Repos;
using System.Collections.Generic;
using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Objects.Ordering.Dispatch;

namespace Concentrator.Plugins.PFA
{
  public class StoreOrderImport : ConcentratorPlugin
  {
    public StoreOrderImport()
    { }

    public override string Name
    {
      get { return "Coolcat Store Order import"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        Int32 connectorID = Int32.Parse(GetConfiguration().AppSettings.Settings["connectorID"].Value);
        Int32 vendorID = Int32.Parse(GetConfiguration().AppSettings.Settings["ccVendorID"].Value);
        String path = GetConfiguration().AppSettings.Settings["OrderPath"].Value;
        var productRepo = unit.Scope.Repository<Product>();
        var stockRepo = unit.Scope.Repository<VendorStock>();
#if DEBUG
				path = @"D:\Concentrator\OrderImport";
#endif

        var vendor = unit.Scope.Repository<Vendor>().GetSingle(c => c.VendorID == vendorID);

        var setting = vendor.VendorSettings.Where(c => c.SettingKey == "ShipOrderNumber").FirstOrDefault();

        var maxOrderNumber = int.Parse(setting.Value);
        if (maxOrderNumber == 9999)
          maxOrderNumber = 5000;

        StoreOrderHelper helper = new StoreOrderHelper();

        //check sold to customer - always the same so add and retrieve here
        var customerRepo = unit.Scope.Repository<Concentrator.Objects.Models.Orders.Customer>();
        var soldToCustomer = customerRepo.GetSingle(c => c.CustomerName == "Coolcat Fashion");

        if (soldToCustomer == null)
        {
          soldToCustomer = new Concentrator.Objects.Models.Orders.Customer()
          {
            CustomerAddressLine1 = "Hoofdveste 10",
            CustomerAddressLine2 = string.Empty,
            City = "Houten",
            Country = "NL",
            CustomerName = "Coolcat Fashion",
            HouseNumber = "10",
            PostCode = "3992 DG"
          };
          unit.Scope.Repository<Concentrator.Objects.Models.Orders.Customer>().Add(soldToCustomer);
          unit.Save();
        }

        WMSDispatcher dispatcher = new WMSDispatcher();

        foreach (String file in Directory.GetFiles(path, "*.xlsx"))
        {
          String fullPath = Path.Combine(path, file);
          try
          {
            var repo = new StoreOrderRepository(fullPath);

            var orders = repo.GetOrders();

            //validate orders
            foreach (var order in orders)
            {
              Order or = new Order()
              {
                OrderLines = new List<OrderLine>(),
                ConnectorID = 5,
                CustomerOrderReference = string.Empty,
                HoldOrder = false,
                WebSiteOrderNumber = "W_" + maxOrderNumber,
                SoldToCustomer = soldToCustomer,
                PaymentTermsCode = "SHOP",
                PaymentInstrument = string.Empty
              };

              foreach (var orderLineModel in order.OrderLines)
              {
                string vendorItemNumber = string.Format("{0} {1} {2}", orderLineModel.Art_Number, orderLineModel.Color_Code, orderLineModel.Size_Code);

                var product = productRepo.GetSingle(c => c.VendorItemNumber == vendorItemNumber);
                if (product == null)
                  continue;

                var vendorStock = stockRepo.GetSingle(c => c.VendorID == vendorID && c.ProductID == product.ProductID && c.VendorStockTypeID == 3);
                var vendorStockWeb = stockRepo.GetSingle(c => c.VendorID == vendorID && c.ProductID == product.ProductID && c.VendorStockTypeID == 1);
                var vendorPrice = product.VendorAssortments.FirstOrDefault(c => c.VendorID == vendorID).VendorPrices.FirstOrDefault();

                //validate and get order line
                var orderLine = helper.GetOrderLine(orderLineModel, vendorStock.QuantityOnHand, product.ProductID, vendorPrice.Price.Value, vendorPrice.SpecialPrice, product);

                if (orderLine != null)
                {
                  vendorStock.QuantityOnHand = vendorStock.QuantityOnHand - orderLine.Quantity; //update Concentrator qty
                  vendorStockWeb.QuantityOnHand = vendorStockWeb.QuantityOnHand - orderLine.Quantity; //update web qty

                  or.OrderLines.Add(orderLine);
                }
              }

              //ship to customer details
              var shipTo = customerRepo.GetSingle(c => c.CustomerName == order.Store_Number);
              if (shipTo == null)
              {
                shipTo = new Concentrator.Objects.Models.Orders.Customer()
                {
                  CustomerAddressLine1 = order.Address1,
                  CustomerAddressLine2 = order.Address2,
                  CompanyName = order.Store_Number,
                  CustomerName = order.Store_Number,
                  Country = order.Country,
                  City = order.City,
                  Street = order.Address1,
                  PostCode = order.Postcode,
                };
                customerRepo.Add(shipTo);
              }
              shipTo.Street = order.Address1;

              or.ShippedToCustomer = shipTo;

              dispatcher.DispatchOrders(new Dictionary<Order, List<OrderLine>>() { { or, or.OrderLines.ToList() } },
              vendor,
              log,
              unit); 

              if (maxOrderNumber < 9999)
                maxOrderNumber++;
              else
                maxOrderNumber = 5000;

              unit.Save();
            }

            String pPath = Path.Combine(path, "Processed");

            if (!Directory.Exists(pPath))
              Directory.CreateDirectory(pPath);

            FileInfo inf = new FileInfo(file);

            String nPath = Path.Combine(pPath, String.Format("{0} {1}.xlsx", Path.GetFileNameWithoutExtension(inf.Name), DateTime.Now.ToString("dd-MM-yy hh-mm")));

            inf.MoveTo(nPath);
          }
          catch (Exception e)
          {
            File.Move(fullPath, Path.ChangeExtension(fullPath, ".xlsx.err"));

            File.Create(Path.Combine(path, Path.GetFileNameWithoutExtension(fullPath) + ".txt.err"));
            using (FileStream logFile = File.Create(Path.Combine(Path.ChangeExtension(path, ".log"))))
            {
              using (StreamWriter writer = new StreamWriter(logFile, Encoding.UTF8))
              {
                writer.WriteLine("Error processing orderlines from file : " + fullPath);
                writer.WriteLine(String.Empty);
                writer.WriteLine(e.ToString());
              }
            }
            log.Error(e);
          }
        }
        setting.Value = maxOrderNumber.ToString();
        unit.Save();
      }
    }
  }
}



