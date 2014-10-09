using Concentrator.Objects.Environments;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Plugins.PFA.Models;
using Excel;
using PetaPoco;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Repos
{
  public static class ReturnOrderRepository
  {
    public static List<ReturnOrderModel> GetReturnOrders(string path)
    {
      if (!File.Exists(path))
        throw new FileNotFoundException("Excel file with order not found: " + path);

      using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
      {
        using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
        {

          DataSet result = excelReader.AsDataSet();

          System.Data.DataTable dt = result.Tables[0];

          return (from c in dt.AsEnumerable()
                  group c by new
                  {
                    ItemCode = c.Field<string>(0),
                    ColorCode = c.Field<string>(1)
                  } into colorGroups
                  select new ReturnOrderModel()
                  {
                    ItemNumber = colorGroups.Key.ItemCode,
                    ColorCode = colorGroups.Key.ColorCode,
                    StoreNumber = colorGroups.FirstOrDefault().Field<string>(4)
                  }).ToList();
        }
      }
    }

    public static List<Vendor> GetVendorsForReturnOrders()
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        return db.Fetch<Vendor>("select vendorid, Name from vendor where isactive = 1 and vendortype & @0 = @0", VendorType.SupportsPFATransferOrders);
      }
    }

    public static int GetWehkampConnectorForVendor(int vendorID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        return db.SingleOrDefault<int>("select ConnectorID from ContentProduct where VendorID = @0 and IsAssortment = 1", vendorID);
      }
    }

    internal static List<Product> GetAllSkusOfArtikelColor(string itemNumber, string colorCode)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        return db.Fetch<Product>("select ProductID, VendorItemNumber from Product where VendorItemNumber like @0 and IsConfigurable = 0", string.Format("{0} {1}%", itemNumber, colorCode));
      }
    }

    internal static bool OpenOrderLineExistsForProduct(int productID)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        return db.SingleOrDefault<OrderLine>(@"select OrderLineID from OrderLine ol inner join [Order] o on o.OrderID = ol.OrderID where ol.ProductID = @0 and  o.OrderType = @1 and ol.IsDispatched = 0", productID, OrderTypes.ReturnOrder) != null;
      }
    }

    internal static void CreateReturnOrder(Order order, List<OrderLine> orderLines)
    {
      using (var db = new Database(Environments.Current.Connection, "System.Data.SqlClient"))
      {
        using (var tran = new Transaction(db))
        {
          db.CommandTimeout = 1 * 60;

          var orderID = db.ExecuteScalar<int>(@"insert into [order] ([Document],[ConnectorID],[IsDispatched],[ReceivedDate],[isDropShipment],[WebSiteOrderNumber],[HoldOrder],[OrderType],[PaymentTermsCode]) OUTPUT Inserted.OrderID VALUES (@0,  @1, @2, @3, @4, @5, @6, @7, @8)",
            order.Document, order.ConnectorID, 0, DateTime.Now.ToUniversalTime(), 0, order.WebSiteOrderNumber, 0, order.OrderType, order.PaymentTermsCode);

          foreach (var orderLine in orderLines)
          {
            db.Execute(@"insert into orderline (OrderID, ProductID,Quantity, WarehouseCode) values (@0, @1, @2, @3)", orderID, orderLine.ProductID, 0, orderLine.WareHouseCode);
          }

          tran.Complete();
        }
      }
    }
  }
}
