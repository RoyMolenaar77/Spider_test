using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport.Shop
{
  public class ShopUtility
  {
    public static ProductBarcode InsertBarcode(int productID, string barcode, string barcodeType)
    {
      ProductBarcode productBarcode = new ProductBarcode();
      productBarcode.LastModificationTime = DateTime.Now;
      productBarcode.ProductID = productID;
      productBarcode.BarcodeType = barcodeType;
      productBarcode.Barcode = barcode;
      return productBarcode;
    }



    public void ProcessStock(Connector connector, log4net.ILog log, XDocument products)
    {
      DateTime start = DateTime.Now;
      log.InfoFormat("Start process stock:{0}", start);
      AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();
      if (products == null)
        products =  new XDocument(soap.GetAssortment(connector.ConnectorID, null, false));

      log.Info(products.Root.Elements("Product").Count());

      ImportStock(connector, products, log); // DC10 Stock naar winkels
      ImportRetailStock(connector, products, log); // Overige winkels naar winkel
    }

    public static int BASrelationID = 962542;
    public static string BASLocation = "BAS Group";
    private void ImportStock(Connector connector, XDocument products, log4net.ILog log)
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
        {
          //var ids = (from r in products.Root.Elements("Product")
          //select int.Parse(r.Attribute("CustomProductID").Value)).Distinct().ToArray();

          //Dictionary<int, List<VendorStock>> existingStock = new Dictionary<int, List<VendorStock>>();

          //foreach (int id in ids)
          //{
          //  var stk = (from c in context.VendorStocks
          //             where c.ProductID == id
          //             select c).ToList();

          //  if (stk.Count() > 0)
          //  {
          //    if (!existingStock.ContainsKey(stk.First().ProductID))
          //      existingStock.Add(stk.First().ProductID, stk);
          //    else
          //    {
          //      foreach (VendorStock vs in stk)
          //      {
          //        if (!existingStock[stk.First().ProductID].Contains(vs))
          //          existingStock[stk.First().ProductID].Add(vs);
          //      }
          //    }
          //  }
          //}

          //List<int> addedstock = new List<int>();

          context.VendorStocks.DeleteAllOnSubmit(context.VendorStocks.Where(x => x.RelationID == BASrelationID));


          var records = (from r in products.Root.Elements("Product")
                         where !String.IsNullOrEmpty(r.Attribute("CustomProductID").Value)
                         select r);

          var groupedByProduct = (from r in records
                                  group r by r.Attribute("CustomProductID").Value
                                    into grouped
                                    select grouped
                        );

          foreach (var r in groupedByProduct)
          {
            var product = r;
            int custom = int.Parse(product.Key);

            var stock = new VendorStock();
            stock.CreationTime = DateTime.Now;
            stock.LastModificationTime = DateTime.Now;
            stock.CreatedBy = 0;
            stock.LastModifiedBy = 0;
            stock.ProductID = custom;
            stock.RelationID = BASrelationID;
            stock.VendorProductStatus = "S";
            stock.StockLocationID = BASLocation;

            int qty = product.Sum(x => int.Parse(x.Element("Stock").Attribute("InStock").Value));
            stock.QuantityOnHand = qty;
            context.VendorStocks.InsertOnSubmit(stock);
          }
          context.SubmitChanges();
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import stock", ex);
        log.Debug(ex.InnerException);
      }
    }

    private void ImportRetailStock(Connector connector, XDocument products, log4net.ILog log)
    {
      using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
      {
        try
        {
          Dictionary<int, List<VendorStock>> existingStock = new Dictionary<int, List<VendorStock>>();


          var records = (from r in products.Root.Elements("Product")
                         where !String.IsNullOrEmpty(r.Attribute("CustomProductID").Value)
                         select r);

          foreach (var r in records)
          {

            int custom = int.Parse(r.Attribute("CustomProductID").Value);

            context.VendorStocks.DeleteAllOnSubmit(
              context.VendorStocks.Where(
                x => x.ProductID == custom && x.RelationID == BASrelationID && x.StockLocationID != BASLocation));

            foreach (var vendor in r.Element("Stock").Element("Retail").Elements("RetailStock"))
            {
              VendorStock stock = null;

              stock = new VendorStock();
              stock.CreationTime = DateTime.Now;
              stock.LastModificationTime = DateTime.Now;
              stock.CreatedBy = 0;
              stock.LastModifiedBy = 0;
              stock.ProductID = custom;
              stock.RelationID = 962542;
              stock.VendorProductStatus = "S";
              stock.StockLocationID = vendor.Attribute("Name").Value;
              stock.QuantityOnHand = int.Parse(vendor.Attribute("InStock").Value);
              context.VendorStocks.InsertOnSubmit(stock);

              //addedstock.Add(custom);
            }


            context.SubmitChanges();
          }

        }
        catch (Exception ex)
        {
          log.Error("Error import vendor stock", ex);
          log.Debug(ex.StackTrace);
        }
      }
    }
  }
}
