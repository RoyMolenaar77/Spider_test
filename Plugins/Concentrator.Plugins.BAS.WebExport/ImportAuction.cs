using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Configuration;
using System.Xml.Linq;
using System.Globalization;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Web.ServiceClient.AssortmentService;
using System.Reflection;
using System.Data.Linq.Mapping;
using log4net;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(3)]
  public class ImportAuction : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "MyCom Auction Product Export"; }
    }

    protected override void Process()
    {
      foreach (Connector connector in base.Connectors) //already filtered
      {
        
        log.DebugFormat("Start Process auction export for {0}", connector.Name);
        DateTime start = DateTime.Now;
        try
        {
          log.Debug("Start import auction items");

          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();
          XDocument products = new XDocument(soap.GetAssortment(connector.ConnectorID, null, true));

          Processor processor = new Processor(products, log, connector);
          DateTime? lastUpdate = null;

          using (WebsiteDataContext ctx = new WebsiteDataContext(connector.ConnectionString))
          {
            var auctionProductList = (from p in ctx.AuctionProducts
                                      select new
                                      {
                                        AuctionProduct = p,
                                        BrandCode = p.Product.Brand.BrandCode,
                                        Product = p.Product
                                      }).ToList();

            if (auctionProductList.Count > 0)
            {
              lastUpdate = (from lu in auctionProductList
                            let update = (lu.AuctionProduct.LastModificationTime.HasValue ? lu.AuctionProduct.LastModificationTime.Value : lu.AuctionProduct.CreationTime)
                            select update).Max();
            }

            Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();

            log.DebugFormat("Xml contains {0} products", products.Root.Elements("Product").Count());

            log.Info("Start import brands");
            processor.ImportBrands();
            log.Info("Finish import brands");
            log.Info("Start import productgroups");
            processor.ImportProductGroups(false);
            log.Info("Finish import productgroups");

            var allProductGroups = (from p in ctx.ProductGroups
                                    select p).ToList();


            var productGroupMappings = (from p in ctx.ProductGroupMappings
                                        group p by p.ProductGroupID
                                          into grouped
                                          select grouped).ToDictionary(x => x.Key, y => y.ToList());

            foreach (var r in products.Root.Elements("Product"))
            {
              #region Get Concentrator Product ID

              int concentratorProductID = Utility.GetConcentratorProductID(connector, r, log);
              if (concentratorProductID == 0)
                continue;

              AuctionProduct auctionProduct = null;
              Product product = null;

              if (connector.UseConcentratorProductID)
              {
                auctionProduct = (from a in auctionProductList
                                  where
                                    a.Product.ConcentratorProductID.HasValue &&
                                    a.Product.ConcentratorProductID.Value == concentratorProductID
                                  select a.AuctionProduct).FirstOrDefault();

                product = (from a in ctx.Products
                           where
                             a.ConcentratorProductID.HasValue &&
                             a.ConcentratorProductID.Value == concentratorProductID
                           select a).FirstOrDefault();
              }
              else
              {
                auctionProduct = (from a in auctionProductList
                                  where a.Product.ProductID == concentratorProductID
                                  select a.AuctionProduct).FirstOrDefault();

                product = (from a in ctx.Products
                           where a.ProductID == concentratorProductID
                           select a).FirstOrDefault();
              }


              #endregion

              try
              {

                List<ProductGroup> productgroups = new List<ProductGroup>();

                foreach (var productGroupNode in r.Element("ProductGroupHierarchy").Elements("ProductGroup"))
                {
                  var productGroups = (from pg in allProductGroups
                                       where pg.BackendProductGroupCode == productGroupNode.Attribute("ID").Value
                                       select pg).ToList();


                  List<string> parentNodes = new List<string>() { productGroupNode.Attribute("ID").Value };

                  var parent = productGroupNode.Element("ProductGroup");
                  while (parent != null)
                  {
                    parentNodes.Add(parent.Attribute("ID").Value);
                    parent = parent.Element("ProductGroup");
                  }

                  string path = String.Join("/", parentNodes.ToArray());

                  ProductGroup group = null;

                  foreach (var g in productGroups)
                  {
                    string groupPath = g.GetProductGroupCodeTree();
                    if (groupPath == path)
                    {
                      group = g;
                      break;
                    }

                  }

                  if (group != null)
                    productgroups.Add(group);
                }


                if (r.Element("Price") != null)
                {
                  decimal unitprice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value,
                                                    CultureInfo.InvariantCulture);
                  decimal costprice = decimal.Parse(r.Element("Price").Element("CostPrice").Value,
                                                    CultureInfo.InvariantCulture);
                  string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
                  string longDescription = r.Element("Content").Attribute("LongDescription").Value;
                  string backendDescription = string.Empty;

                  if (connector.ImportCommercialText)
                  {
                    backendDescription = longDescription;

                    //shortDescription = r.Element("Content").Attribute("ShortContentDescription").Value;
                    //if (!string.IsNullOrEmpty(r.Element("Content").Attribute("LongContentDescription").Value))
                    //longDescription = r.Element("Content").Attribute("LongContentDescription").Value;

                  }

                  int? taxRateID = (from t in ctx.TaxRates
                                    where t.TaxRate1.HasValue
                                          &&
                                          (t.TaxRate1.Value * 100) ==
                                          decimal.Parse(r.Element("Price").Attribute("TaxRate").Value,
                                                        CultureInfo.InvariantCulture)
                                    select t.TaxRateID).FirstOrDefault();

                  if (!taxRateID.HasValue)
                    taxRateID = 1;

                  if (auctionProduct == null)
                  {
                    #region Create New (Auction) Product


                    Brand brand = (from b in ctx.Brands
                                   where b.BrandCode == r.Element("Brands").Element("Brand").Attribute("BrandID").Value.Trim()
                                   select b).FirstOrDefault();

                    if (brand != null)
                    {


                      //DateTime temppdDate = DateTime.MinValue;
                      //DateTime? pdDate = null;
                      //if (DateTime.TryParse(r.Element("Stock").Attribute("PromisedDeliveryDate").Value, out temppdDate))
                      //{
                      //  if (temppdDate == DateTime.MinValue && temppdDate == DateTime.MaxValue)
                      //    pdDate = null;
                      //  else
                      //    pdDate = temppdDate;
                      //}


                      if (product == null)
                      {
                        if (!connector.UseConcentratorProductID)
                        {
                          //create stub product with identity_insert on

                          string cmd =
                            String.Format(
                              @"
INSERT INTO Products (ProductID, ShortDescription,LongDescription,brandid,taxrateid,iscustom,isvisible,extendedcatalog
	, canmodifyprice,creationtime,lastmodificationtime
	)  VALUES( {0}, '{1}','{2}', {3},{4},0,0,1,0,getdate(), getdate())
",
                              concentratorProductID, shortDescription, longDescription, brand.BrandID, taxRateID.Value);

                          ctx.ExecuteCommand(cmd);

                          product = ctx.Products.Single(x => x.ProductID == concentratorProductID);
                          product.IsVisible = false;
                          ctx.SubmitChanges();

                        }
                        else
                        {


                          product = new Product
                          {
                            //ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                            //ProductGroupID = subprodid,

                            IsVisible = false,
                            CreationTime = DateTime.Now,
                            LastModificationTime = DateTime.Now
                          };
                          ctx.Products.InsertOnSubmit(product);

                        }

                        if (!string.IsNullOrEmpty(backendDescription))
                          product.BackEndDescription = backendDescription;

                        DateTime pdDate;
                        if (DateTime.TryParse(r.Element("Stock").Attribute("PromisedDeliveryDate").Value, out pdDate))
                        {
                          if (pdDate != DateTime.MinValue && pdDate != DateTime.MaxValue)
                            product.PromisedDeliveryDate = pdDate;
                        }
                      }

                      if (!product.IsVisible)
                      {
                        product.UnitPrice = unitprice;
                        product.ShortDescription = shortDescription;
                        product.LongDescription = longDescription;
                        product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                        product.BrandID = brand.BrandID;
                        product.TaxRateID = taxRateID.Value;
                        product.UnitCost = 0;
                        product.IsCustom = false;
                        product.LineType = string.IsNullOrEmpty(r.Attribute("LineType").Value)
                                     ? "S"
                                     : r.Attribute("LineType").Value.Trim();
                        product.ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value;
                      }
                      product.CustomProductID = r.Attribute("CustomProductID").Value;
                      product.ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);

                      ctx.SubmitChanges();

                    }
                    else
                    {
                      log.DebugFormat("Product {0} does not have a price in xml", r.Attribute("ProductID").Value);
                    }

                    if (product != null)
                    {
                      auctionProduct = new AuctionProduct()
                                         {
                                           AuctionProductID = product.ProductID,
                                           CreationTime = DateTime.Now
                                         };

                      ctx.AuctionProducts.InsertOnSubmit(auctionProduct);
                      auctionProductList.Add(new
                                               {
                                                 AuctionProduct = auctionProduct,
                                                 BrandCode = product.Brand.BrandCode,
                                                 Product = product
                                               });
                    }
                    else
                    {
                      log.WarnFormat("Cannot create new product info for product ID : {0}", concentratorProductID);
                      continue;
                    }
                    #endregion
                  }

                }
                else
                {
                  log.DebugFormat("Brand {0} does not exists for {1}",
                                  r.Elements("Brands").Elements("Brand").FirstOrDefault().Attribute("BrandID").Value.Trim(), r.Attribute("ProductID").Value);
                  continue;
                }

                if (auctionProduct == null)
                {
                  log.WarnFormat("Cannot create new auction product info for product ID : {0}", concentratorProductID);
                  continue;
                }


                #region Parse Stock Data

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

                //DEMO
                var bscDemo = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                               where b.Attribute("Name").Value == "BSCDEMO"
                               select b.Attribute("InStock").Value).FirstOrDefault();

                int bscDemoStock = 0;
                int.TryParse(bscDemo, out bscDemoStock);
                //DMGBOX
                var bscDMGBOX = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                                 where b.Attribute("Name").Value == "BSCDMGBOX"
                                 select b.Attribute("InStock").Value).FirstOrDefault();

                int bscDMGBOXStock = 0;
                int.TryParse(bscDMGBOX, out bscDMGBOXStock);
                //DMGITEM
                var bscDMGITEM = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                                  where b.Attribute("Name").Value == "BSCDMGITEM"
                                  select b.Attribute("InStock").Value).FirstOrDefault();

                int bscDMGITEMStock = 0;
                int.TryParse(bscDMGITEM, out bscDMGITEMStock);
                //INCOMPL
                var bscINCOMPL = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                                  where b.Attribute("Name").Value == "BSCINCOMPL"
                                  select b.Attribute("InStock").Value).FirstOrDefault();

                int bscINCOMPLStock = 0;
                int.TryParse(bscINCOMPL, out bscINCOMPLStock);
                //RETURN
                var bscRETURN = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                                 where b.Attribute("Name").Value == "BSCRETURN"
                                 select b.Attribute("InStock").Value).FirstOrDefault();

                int bscRETURNStock = 0;
                int.TryParse(bscRETURN, out bscRETURNStock);
                //USED
                var bscUSED = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                               where b.Attribute("Name").Value == "BSCUSED"
                               select b.Attribute("InStock").Value).FirstOrDefault();

                int bscUSEDStock = 0;
                int.TryParse(bscUSED, out bscUSEDStock);


                //COSTPRice
                var totalBscStock = bscDemoStock + bscDMGBOXStock + bscDMGITEMStock + bscINCOMPLStock + bscRETURNStock + bscUSEDStock + bscStock;


                var bscCostPrices = (from b in r.Element("Stock").Element("Retail").Elements("RetailStock")
                                     where b.Attribute("Name").Value == "BSC"
                                     select b.Attribute("CostPrice").Value).FirstOrDefault();

                decimal BSCCostPrice = 0;
                if (bscCostPrices != null)
                  BSCCostPrice = decimal.Parse(bscCostPrices, CultureInfo.InvariantCulture);

                decimal DC10CostPrice = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);


                #endregion

                if (auctionProduct.AuctionBSCStock != totalBscStock ||
                     auctionProduct.AuctionDC10Stock != int.Parse(r.Element("Stock").Attribute("InStock").Value) ||
                                  auctionProduct.AuctionOEMStock != bscOEMStock ||
                                  auctionProduct.BSCCostPrice != BSCCostPrice ||
                                  auctionProduct.DC10CostPrice != DC10CostPrice ||
                                  auctionProduct.AuctionDEMOStock != bscDemoStock ||
                                  auctionProduct.AuctionDMGBOXStock != bscDMGBOXStock ||
                                  auctionProduct.AuctionDMGITEMStock != bscDMGITEMStock ||
                                  auctionProduct.AuctionINCOMPLStock != bscINCOMPLStock ||
                                  auctionProduct.AuctionRETURNStock != bscRETURNStock ||
                                  auctionProduct.AuctionUSEDStock != bscUSEDStock ||
                                  auctionProduct.AuctionMYVEILStock != bscStock ||
                                  auctionProduct.QuantityToReceive != int.Parse(r.Element("Stock").Attribute("QuantityToReceive").Value) ||
                                  auctionProduct.StockStatus != r.Element("Stock").Attribute("StockStatus").Value)
                {
                  auctionProduct.LastModificationTime = DateTime.Now;
                }


                auctionProduct.AuctionBSCStock = totalBscStock;
                auctionProduct.AuctionDC10Stock = int.Parse(r.Element("Stock").Attribute("InStock").Value);
                auctionProduct.AuctionOEMStock = bscOEMStock;
                auctionProduct.BSCCostPrice = BSCCostPrice;

                auctionProduct.AuctionDEMOStock = bscDemoStock;
                auctionProduct.AuctionDMGBOXStock = bscDMGBOXStock;
                auctionProduct.AuctionDMGITEMStock = bscDMGITEMStock;
                auctionProduct.AuctionINCOMPLStock = bscINCOMPLStock;
                auctionProduct.AuctionRETURNStock = bscRETURNStock;
                auctionProduct.AuctionUSEDStock = bscUSEDStock;
                auctionProduct.AuctionMYVEILStock = bscStock;

                auctionProduct.DC10CostPrice = DC10CostPrice;
                auctionProduct.QuantityToReceive = int.Parse(r.Element("Stock").Attribute("QuantityToReceive").Value); ;
                auctionProduct.StockStatus = r.Element("Stock").Attribute("StockStatus").Value;

                ctx.SubmitChanges();

                processor.ImportProductGroupMapping(ctx, productgroups, productGroupMappings, auctionProduct.AuctionProductID, concentratorProductID);
                ctx.SubmitChanges();

                if (!product.ConcentratorProductID.HasValue)
                {
                  product.ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);
                  ctx.SubmitChanges();
                }
              }
              catch (Exception ex)
              {
                log.Error("error insert product", ex);
              }
            }

            //log.Info("Start import productgroupmapppings");
            //processor.ImportProductGroupMapping(ctx, connector);
            //log.Info("Finish import productgroupmapppings");


            log.Debug("Start cleanup auctionproducts");

            var cItemNumbers = (from r in products.Root.Elements("Product")
                                select new
                                         {
                                           concentratorProductID = r.Attribute("ProductID").Value,
                                           customProductID = r.Attribute("CustomProductID").Value
                                         }).ToList();

            List<string> xmlProducts = new List<string>();

            if (connector.UseConcentratorProductID)
            {
              xmlProducts = (from c in cItemNumbers
                             where c.concentratorProductID != String.Empty
                             select c.concentratorProductID).ToList();
            }
            else
            {
              foreach (var c in cItemNumbers.Select(x => x.customProductID))
              {
                int tmp = 0;
                if (Int32.TryParse(c, out tmp))
                  xmlProducts.Add(tmp.ToString());
              }
            }

            List<AuctionProduct> siteProducts = new List<AuctionProduct>();

            if (connector.UseConcentratorProductID)
            {
              var tempSiteProducts = (from c in ctx.AuctionProducts
                                      where c.Product.ConcentratorProductID.HasValue
                                      select new
                                      {
                                        concentratorProductID = c.Product.ConcentratorProductID.Value.ToString(),
                                        auctionItem = c
                                      }).ToList();

              foreach (var p in cItemNumbers)
              {
                tempSiteProducts.RemoveAll(c => c.concentratorProductID == p.concentratorProductID.ToString());
              }

              siteProducts = tempSiteProducts.Select(x => x.auctionItem).ToList();

              if (siteProducts != null && siteProducts.Count > 0)
              {
                ctx.AuctionProducts.DeleteAllOnSubmit(siteProducts);
              }
              ctx.SubmitChanges();

              var delProds = (from c in ctx.AuctionProducts
                              where !c.Product.ConcentratorProductID.HasValue
                              select c).ToList();

              log.DebugFormat("Delete {0} items without concentrator productid", delProds.Count);
              ctx.AuctionProducts.DeleteAllOnSubmit(delProds);
              ctx.SubmitChanges();
            }
            else
            {
              siteProducts = (from c in ctx.AuctionProducts
                              select c).ToList();

              foreach (var p in cItemNumbers)
              {
                siteProducts.RemoveAll(c => c.AuctionProductID.ToString() == p.customProductID);
              }

              if (siteProducts != null && siteProducts.Count > 0)
              {
                ctx.AuctionProducts.DeleteAllOnSubmit(siteProducts);
              }
              ctx.SubmitChanges();
            }
          }
          log.Debug("Finish cleanup assormtent");

          // processor.CleanUpProductGroupMapping();

          log.Debug("Start import productattributes");
          processor.ProcessAttributes(soap, connector, true, lastUpdate, false);
          log.Debug("Finish import productattributes");

          log.DebugFormat("Auction Product import Completed On: {0}", DateTime.Now);
        }
        catch (Exception ex)
        {
          log.Error("Error import products", ex);
        }
        
        log.DebugFormat("Finish Process auction import for {0}", connector.Name);

      }
    }


  }
}
