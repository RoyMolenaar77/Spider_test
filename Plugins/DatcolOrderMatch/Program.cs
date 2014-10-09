using PetaPoco;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatcolOrderMatch
{
  class Program
  {
    static void Main(string[] args)
    {
      var datcolLines = File.ReadAllLines(@"D:\tmp\CC\webdatcol_20140716_web.txt");
      List<OrderLineConcentratorModel> orderIDs = new List<OrderLineConcentratorModel>();
      List<int> matchedOrderIDs = new List<int>();

      var linesGrouped = datcolLines.GroupBy(c => c.Substring(12, 4)).ToList();
      using (var db = new Database("Data Source=127.0.0.1,7013;Initial Catalog=Concentrator_Test_Migration;Persist Security Info=True; User ID=root;Password=Tieneif4johCh9neOoyoh8xi;MultipleActiveResultSets=True", "System.Data.SqlClient"))
      {
        db.CommandTimeout = 60 * 10;
        var orders = db.Fetch<OrderLineConcentratorModel>(@"select distinct o.OrderID, ol.OrderLineID, o.ReceivedDate, ol.productid, ol.quantity, cs.value as ShopNumber ,  o.WebsiteOrderNumber , p.VendorItemNumber from [order] o
inner join orderline ol on ol.orderid = o.orderid
inner join product p on p.productid = ol.productid
inner join orderledger old on old.orderlineid= ol.orderlineid and old.Status = 10
left join orderledger old2 on old2.orderlineid = ol.orderlineid and old2.status = 97 and old2.OrderLedgerID != old.orderledgerid
inner join connectorsetting cs on cs.connectorid = o.connectorid and cs.settingkey = 'DatcolShopNumber'
where old2.OrderLedgerID is null and o.connectorid in (5,7,8,13) and ol.isDispatched = 1 and o.paymenttermscode != 'Shop' and o.ordertype = 1 and o.receiveddate < '2014-07-17' order by productid ");

        var lookup = orders.GroupBy(c => c.OrderID);

        foreach (var group in linesGrouped)
        {
          var convertedVins = new List<ProductDatcolModel>();
          string shopNumber = string.Empty;


          foreach (var line in group)
          {
            //get the size 
            var vin = line.Substring(128, 20).TrimStart(new char[] { '0' });

            shopNumber = line.Substring(0, 3);

            var size = vin.Substring(vin.Length - 4);

            var color = vin.Substring(vin.Length - 7, 3);

            var sku = vin.Substring(0, vin.Length - 7);

            var amount = (decimal)int.Parse(line.Substring(114, 9)) / 100;

            ProductDatcolModel product = null;
            if (sku == "5000999040")
            {
              product = new ProductDatcolModel()
              {
                ProductID = 220750,
                VendorItemNumber = sku
              };
            }
            else
            {
              product = db.FirstOrDefault<ProductDatcolModel>(string.Format(@"select p.ProductID, p.VendorItemNumber from productbarcode pb 
                            inner join product p on p.productid = pb.productid 
                            inner join productattributevalue pav on pav.attributeid = 41 and pav.productid = p.productid
                            inner join productattributevalue pavC on pavC.attributeid = 40 and pavC.productid = p.productid 
                            where barcode = '{0}' and p.vendoritemnumber like '{1}%' and (pavC.value = '{2}' or pavC.value = {3}) ", size, sku, color, color.TrimStart(new char[] { '0' })));
            }

            if (product == null)
            {
              Console.WriteLine("Could not match product with {0} {1} {2} for bon {3}", sku, color, size, group.Key);
              Console.ReadLine();
            }

            var qty = int.Parse(line.Substring(39, 6));
            product.Quantity = qty;
            product.ShopNumber = shopNumber;
            product.Amount = amount;

            convertedVins.Add(product);
          }

          //first get the same number
          var matchedGroups = lookup.Where(c => c.Count() == convertedVins.Count && c.First().ShopNumber == convertedVins.First().ShopNumber).ToList();

          var b = string.Join("\n", convertedVins.OrderBy(c => c.ProductID).Select(c => c.ToMatch()));

          var matches = matchedGroups.Where(c => string.Join("\n", c.Select(l => l.ToMatch())) == b).ToList();

          if (matches.Count == 0)
          {
            Console.WriteLine("No matches could be found for bon {0}", group.Key);

          }
          else if (matches.Count() > 1)
          {
            Console.WriteLine("Multiple matches could be found for bon {0}", group.Key);
            var firstMatch = matches.Where(c => !matchedOrderIDs.Contains(c.Key)).FirstOrDefault();

            if (firstMatch != null)
            {
              foreach (var fm in firstMatch)
              {
                fm.BonNumber = group.Key;
                fm.Amount = convertedVins.Sum(c => c.Amount);
              }
              orderIDs.AddRange(firstMatch);
              matchedOrderIDs.Add(firstMatch.Key);
            }
          }
          else
          {
            foreach (var fm in matches.First().ToList())
            {
              fm.BonNumber = group.Key;
              fm.Amount = convertedVins.Sum(c => c.Amount);
            }
            orderIDs.AddRange(matches.First().ToList());
          }
        }
        try
        {
          StringBuilder s = new StringBuilder();
          s.Append(string.Join(";", orderIDs.First().GetType().GetProperties().Select(c => c.Name)));
          s.Append("\n");

          foreach (var orderLine in orderIDs)
          {
            s.AppendLine(string.Join(";", orderLine.GetType().GetProperties().Select(c => c.GetValue(orderLine))));
          }

          File.WriteAllText(@"D:\tmp\CC\webdatcol_deserialize_20140716.txt", s.ToString());
        }
        catch (Exception e)
        {

        }

        try
        {
          db.BeginTransaction();
          foreach (var orderline in orderIDs)
          {
            db.Execute("insert into OrderLedger (OrderLineID, Status, LedgerDate) values (@0, @1, @2)", orderline.OrderLineID, 97, DateTime.Now.ToUniversalTime());
          }

          foreach (var order in orderIDs.GroupBy(c => c.OrderID))
          {
            db.Execute("insert into DatcolLink (OrderID, ShopNumber, DateCreated, Amount, DatcolNumber, SourceMessage) values (@0, @1, @2, @3, @4, @5)", order.Key, order.First().ShopNumber, order.First().ReceivedDate.ToUniversalTime(), order.First().Amount, order.First().BonNumber, "Sales order");
          }

          db.CompleteTransaction();
        }
        catch
        {
          db.AbortTransaction();
        }
      }
    }
  }

  public class OrderLineConcentratorModel
  {
    public int OrderID { get; set; }
    public int ProductID { get; set; }
    public string VendorItemNumber { get; set; }
    public int Quantity { get; set; }
    public int OrderLineID { get; set; }
    public string BonNumber { get; set; }
    public DateTime ReceivedDate { get; set; }
    public string ShopNumber { get; set; }
    public decimal Amount { get; set; }
    public string WebsiteOrderNumber { get; set; }

    public string ToMatch()
    {
      return string.Format("{0} {1}", ProductID, Quantity);
    }
  }

  public class OrderDatcolModel
  {
    public List<OrderLineDatcolModel> Lines { get; set; }

    public string Bon { get; set; }

    public string WebsiteOrderNumber { get; set; }

    public int OrderID { get; set; }
  }

  public class ProductDatcolModel
  {
    public string VendorItemNumber { get; set; }

    public int ProductID { get; set; }

    public int Quantity { get; set; }
    public string ShopNumber { get; set; }
    public decimal Amount { get; set; }

    public string ToMatch()
    {
      return string.Format("{0} {1}", ProductID, Quantity);
    }
  }

  public class OrderLineDatcolModel
  {
    public ProductDatcolModel Product { get; set; }

    public int OrderLineID { get; set; }


  }
}
