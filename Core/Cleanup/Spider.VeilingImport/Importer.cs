using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Xml.Linq;
using System.Globalization;
using Spider.Objects.Concentrator;

namespace Spider.VeilingImport
{
  public class Importer : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "MyCom Auction product import"; }
    }

    protected override void Process()
    {
      try
      {
        log.Debug("Start import auction items");

        Concentrator.SpiderServiceSoapClient soap =
                            new Concentrator.SpiderServiceSoapClient();
        XDocument products = XDocument.Parse(soap.GetAssortment(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString())).OuterXml);


        using (AuctionDataContext ctx = new AuctionDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
        {
          var auctionProductList = (from p in ctx.AuctionProducts
                                    select p).ToList();

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();

          log.DebugFormat("Xml contains {0} products", products.Root.Elements("Product").Count());

          foreach (var r in products.Root.Elements("Product"))
          {
            var auctionProduct = (from a in auctionProductList
                                  where a.AuctionProductID == int.Parse(r.Attribute("CustomProductID").Value)
                                  select a).FirstOrDefault();

            log.DebugFormat("Start import product {0}", r.Attribute("CustomProductID").Value);
            try
            {
              if (auctionProduct == null)
              {
                var product = (from p in ctx.Products
                               where p.ProductID == int.Parse(r.Attribute("CustomProductID").Value)
                               select p).FirstOrDefault();

                if (product == null)
                {
                  #region add product
                  Brand brand = (from b in ctx.Brands
                                 where b.BrandCode == r.Element("Brand").Element("Code").Value.Trim()
                                 select b).FirstOrDefault();

                  if (brand != null)
                  {

                    decimal unitprice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                    decimal costprice = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);
                    string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
                    string longDescription = r.Element("Content").Attribute("LongDescription").Value;

                    int? taxRateID = (from t in ctx.TaxRates
                                      where t.TaxRate1.HasValue
                                      && (t.TaxRate1.Value * 100) == decimal.Parse(r.Element("Price").Attribute("TaxRate").Value, CultureInfo.InvariantCulture)
                                      select t.TaxRateID).SingleOrDefault();

                    if (!taxRateID.HasValue)
                      taxRateID = 1;

                    DateTime temppdDate = DateTime.MinValue;
                    DateTime? pdDate = null;
                    if (DateTime.TryParse(r.Element("Stock").Attribute("PromisedDeliveryDate").Value, out temppdDate))
                    {
                      if (temppdDate == DateTime.MinValue && temppdDate == DateTime.MaxValue)
                        pdDate = null;
                      else
                        pdDate = temppdDate;
                    }

                    product = new Product
                    {
                      ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                      ShortDescription = shortDescription,
                      LongDescription = longDescription,
                      ManufacturerID = r.Attribute("ManufacturerID").Value,
                      BrandID = brand.BrandID,
                      TaxRateID = taxRateID.Value,
                      UnitPrice = unitprice,
                      UnitCost = costprice,
                      IsCustom = false,
                      LineType = string.IsNullOrEmpty(r.Attribute("LineType").Value) ? "S" : r.Attribute("LineType").Value.Trim(),
                      ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value,
                      IsVisible = true,
                      CreationTime = DateTime.Now,
                      LastModificationTime = DateTime.Now,
                      ExtendedCatalog = false,
                      PromisedDeliveryDate = pdDate
                    };

                    ctx.Products.InsertOnSubmit(product);
                    ctx.SubmitChanges();

                  }
                  else
                  {
                    log.DebugFormat("Brand {0} does not exists for {1}", r.Element("Brand").Element("Code").Value.Trim(), r.Attribute("CustomProductID").Value);
                    continue;
                  }
                  #endregion
                }

                auctionProduct = new AuctionProduct()
                {
                  AuctionProductID = int.Parse(r.Attribute("CustomProductID").Value)
                };

                ctx.AuctionProducts.InsertOnSubmit(auctionProduct);
                auctionProductList.Add(auctionProduct);
              }

              var bsc = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                         where b.Attribute("Name").Value == "BSC"
                         select b.Attribute("InStock").Value).FirstOrDefault();

              int bscStock = 0;
              int.TryParse(bsc, out bscStock);

              var bscOEM = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                            where b.Attribute("Name").Value == "BSCOEM"
                            select b.Attribute("InStock").Value).FirstOrDefault();

              int bscOEMStock = 0;
              int.TryParse(bscOEM, out bscOEMStock);

              var bscCostPrices = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                                   where b.Attribute("Name").Value == "BSC"
                                   select b.Attribute("CostPrice").Value).FirstOrDefault();

              decimal BSCCostPrice = 0;
              if (bscCostPrices != null)
                BSCCostPrice = decimal.Parse(bscCostPrices, CultureInfo.InvariantCulture);

              decimal DC10CostPrice = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);

              auctionProduct.AuctionBSCStock = bscStock;
              auctionProduct.AuctionDC10Stock = int.Parse(r.Element("Stock").Attribute("InStock").Value);
              auctionProduct.AuctionOEMStock = bscOEMStock;
              auctionProduct.BSCCostPrice = BSCCostPrice;
              auctionProduct.DC10CostPrice = DC10CostPrice;
              auctionProduct.QuantityToReceive = int.Parse(r.Element("Stock").Attribute("QuantityToReceive").Value); ;
              auctionProduct.StockStatus = r.Element("Stock").Attribute("StockStatus").Value;

              ctx.SubmitChanges();
            }
            catch (Exception ex)
            {
              log.Error("error insert product", ex);
            }
          }
        }

        log.Debug("Start cleanup auctionproducts");
        using (AuctionDataContext context = new AuctionDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
        {
          #region Vendor Assortment


          List<int> itemNumbers = (from r in products.Root.Elements("Product")
                                   select int.Parse(r.Attribute("CustomProductID").Value)).ToList();


          List<AuctionProduct> unused = (from c in context.AuctionProducts
                                         select c).ToList();

          unused.RemoveAll(c => itemNumbers.Contains(c.AuctionProductID));

          #endregion
          if (unused != null && unused.Count > 0)
          {
            context.AuctionProducts.DeleteAllOnSubmit(unused);
          }
          context.SubmitChanges();
        }
        log.Debug("Finish cleanup assormtent");

        log.DebugFormat("Product import Completed On: {0}", DateTime.Now);
      }
      catch (Exception ex)
      {
        log.Error("Error import products", ex);
      }

    }
  }
}
