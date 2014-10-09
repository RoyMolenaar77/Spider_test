using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Xml.Linq;
using System.Threading;
using System.Globalization;
using System.Xml;
using System.IO;
using System.Reflection;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Data;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{
  public class Processor
  {
    private XDocument products;
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();
    private List<int> _concentratorItemNumbers = new List<int>();
    private log4net.ILog log;

    public List<int> ConcentratorItemNumbers
    {
      get { return _concentratorItemNumbers; }
      set { _concentratorItemNumbers = value; }
    }

    public Processor(XDocument productfeed, log4net.ILog logging, Connector connector)
    {
      products = productfeed;
      log = logging;
      this.Connector = connector;
    }
    private Connector Connector
    {
      get;
      set;
    }

    private void AddProductGroupMapping(int item, int productGroupID)
    {
      if (pgmList.ContainsKey(item))
        pgmList[item].Add(productGroupID);
      else
      {
        List<int> list = new List<int>();
        list.Add(productGroupID);
        pgmList.Add(item, list);
      }
    }

    public void ImportProductGroupMapping(WebsiteDataContext ctx, List<ProductGroup> productgroups, Dictionary<int, List<ProductGroupMapping>> productGroupMappings, int productID, int concentratorProductID)
    {
      foreach (ProductGroup productGroup in productgroups)
      {
        IEnumerable<ProductGroupMapping> pgm = null;

        if (productGroupMappings.ContainsKey(productGroup.ProductGroupID))
          pgm = productGroupMappings[productGroup.ProductGroupID].Where(x => x.ProductID == productID);

        //var pgm = (from p in productGroupMappings
        //           where p.ProductID == product.ProductID
        //           && p.ProductGroupID == productGroup.ProductGroupID
        //           select p).FirstOrDefault();

        if (!productGroupMappings.ContainsKey(productGroup.ProductGroupID) || pgm == null)
        {

          if (!pgmList.ContainsKey(concentratorProductID))
          {
            ProductGroupMapping mapping = new ProductGroupMapping
            {
              ProductID = productID,
              ProductGroup = productGroup
            };
            ctx.ProductGroupMappings.InsertOnSubmit(mapping);
          }
          else
          {
            if (!pgmList[concentratorProductID].Contains(productGroup.ProductGroupID))
            {
              ProductGroupMapping mapping = new ProductGroupMapping
              {
                ProductID = productID,
                ProductGroup = productGroup
              };
              ctx.ProductGroupMappings.InsertOnSubmit(mapping);
            }
          }
        }
        AddProductGroupMapping(concentratorProductID, productGroup.ProductGroupID);
      }
    }

    public void ImportProductGroupMapping(WebsiteDataContext context, Connector connector)
    {
      var allProductGroups = (from p in context.ProductGroups
                              where p.ParentProductGroupID.HasValue
                              select p).ToList();

      var allParents = (from p in context.ProductGroups
                        where !p.ParentProductGroupID.HasValue
                        select p).ToList();

      var productGroupMappings = (from p in context.ProductGroupMappings
                                  select p).ToList();

      var websiteProducts = (from p in context.Products
                             select p).ToList();

      foreach (var r in products.Root.Elements("Product"))
      {


        int concentratorProductID = 0;

        if (connector.UseConcentratorProductID)
          concentratorProductID = int.Parse(r.Attribute("ProductID").Value);
        else
        {
          Int32.TryParse(r.Attribute("CustomProductID").Value, out concentratorProductID);
          if (concentratorProductID == 0)
          {
            log.WarnFormat("Skipping product '{0}', because it has a non-numeric Custom Product ID ",
                           r.Attribute("CustomProductID").Value);
            continue;
          }
        }




        List<ProductGroup> productgroups = new List<ProductGroup>();

        if (r.Element("ProductGroupHierarchy").Element("ProductGroup") == null)
        {
          log.DebugFormat("Empty productgroups for product {0}", int.Parse(r.Attribute("ProductID").Value));
        }
        else
        {
          foreach (var pgs in r.Element("ProductGroupHierarchy").Elements("ProductGroup"))
          {
            var productgroup = (from pg in allProductGroups
                                where pg.BackendProductGroupCode == pgs.Attribute("ID").Value
                                && pg.ParentProductGroupID == (allParents.Where(x => x.BackendProductGroupCode == pgs.Element("ProductGroup").Attribute("ID").Value).Select(x => x.ProductGroupID)).Single()
                                select pg).FirstOrDefault();

            if (productgroup != null)
              productgroups.Add(productgroup);
          }
        }

        //int concentratorProductID = int.Parse(r.Attribute("ProductID").Value);
        Product websiteProduct = null;

        if (connector.UseConcentratorProductID)
          websiteProduct = (from a in websiteProducts
                            where a.ConcentratorProductID.HasValue && a.ConcentratorProductID.Value == concentratorProductID
                            select a).FirstOrDefault();
        else
        {

          websiteProduct = (from a in websiteProducts
                            where a.ProductID == concentratorProductID
                            select a).FirstOrDefault();
        }


        //var websiteProduct = (from p in websiteProducts
        //                      where (p.ConcentratorProductID.HasValue && p.ConcentratorProductID.Value == concentratorProductID)
        //                      || (p.Brand.BrandCode == r.Element("Brand").Attribute("BrandID").Value.Trim()
        //     && p.ManufacturerID == r.Attribute("ManufacturerID").Value)
        //                      select p).FirstOrDefault();

        foreach (ProductGroup productgroup in productgroups)
        {
          var pgm = (from p in productGroupMappings
                     where p.ProductID == websiteProduct.ProductID
                     && p.ProductGroupID == productgroup.ProductGroupID
                     select p).FirstOrDefault();

          if (pgm == null)
          {
            if (pgmList.ContainsKey(concentratorProductID))
              if (pgmList[concentratorProductID].Contains(productgroup.ProductGroupID))
                continue;

            ProductGroupMapping mapping = new ProductGroupMapping
            {
              ProductID = websiteProduct.ProductID,
              ProductGroupID = productgroup.ProductGroupID
            };
            context.ProductGroupMappings.InsertOnSubmit(mapping);
          }
          AddProductGroupMapping(concentratorProductID, productgroup.ProductGroupID);
        }
      }
      context.SubmitChanges();
    }

    public bool ImportPrice()
    {
      bool errorImportProduct = false;

      try
      {
        using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
        {
          var nodes = (from r in products.Root.Elements("Product")
                       select r);

          if (!this.Connector.UseConcentratorProductID)
            nodes = nodes.Where(x => x.Attribute("CustomProductID").Value != "");

          var productList = (from p in context.Products
                             where p.ManufacturerID != null && p.IsVisible
                             select new WebsiteProduct
                             {
                               BrandCode = p.Brand.BrandCode,
                               Product = p
                             }).ToList();

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();

          if (Connector.UseConcentratorProductID)
          {
            Dictionary<int?, List<Product>> test = (from p in productList.Select(x => x.Product)
                                                    join r in nodes on p.ConcentratorProductID equals int.Parse(r.Attribute("ProductID").Value)
                                                    where !string.IsNullOrEmpty(r.Attribute("ProductID").Value)
                                                    group p by p.ConcentratorProductID
                                                      into grouped
                                                      select grouped).ToDictionary(x => x.Key, y => y.ToList());


            var ids = (from r in nodes
                       where !string.IsNullOrEmpty(r.Attribute("ProductID").Value)
                       select new
                       {
                         brandCode = r.Element("Brand").Attribute("BrandID").Value.Trim(),
                         manufactuerID = r.Attribute("ManufacturerID").Value.Trim(),
                         concentratorProductID = this.Connector.UseConcentratorProductID ? r.Attribute("ProductID").Value : r.Attribute("CustomProductID").Value
                       }).Distinct().ToList();

            foreach (var id in ids)
            {
              var pr = (from c in productList
                        where c.Product.ManufacturerID.Trim() == id.manufactuerID.Trim()
                              && c.BrandCode.Trim() == id.brandCode.Trim()
                        select c.Product).FirstOrDefault();


              var cpid = Int32.Parse(id.concentratorProductID);
              if (pr != null && !existingProducts.ContainsKey(cpid))
                existingProducts.Add(cpid, pr);
            }
          }
          else
          {
            existingProducts = (from c in productList
                                select c).ToDictionary(p => p.Product.ProductID, p => p.Product);
            // BAS Sites

          }

          var brands = (from b in context.Brands
                        select b).ToDictionary(x => x.BrandCode.Trim(), x => x);

          int productCount = products.Root.Elements("Product").Count();
          int counter = 0;
          int logCount = 0;

          foreach (var r in products.Root.Elements("Product"))
          {
            counter++;
            logCount++;
            if (logCount == 250)
            {
              log.DebugFormat("Products Processed : {0}/{1} for Connector {2}", counter, productCount, Connector.Name);
              logCount = 0;
            }

            int realConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);

            int concentratorProductID = 0;

            if (this.Connector.UseConcentratorProductID)
              concentratorProductID = int.Parse(r.Attribute("ProductID").Value);
            else
            {
              Int32.TryParse(r.Attribute("CustomProductID").Value, out concentratorProductID);
              if (concentratorProductID == 0)
              {
                log.WarnFormat("Skipping product '{0}', because it has a non-numeric Custom Product ID ",
                               r.Attribute("CustomProductID").Value);
                continue;


              }
            }

            if (r.Element("Price") != null)
            {

              Product product = null;
              string backendDescription = string.Empty;

              try
              {
                int? taxRateID = (from t in context.TaxRates
                                  where t.TaxRate1.HasValue
                                  && (t.TaxRate1.Value * 100) == decimal.Parse(r.Element("Price").Attribute("TaxRate").Value, CultureInfo.InvariantCulture)
                                  select t.TaxRateID).FirstOrDefault();

                if (!taxRateID.HasValue)
                  taxRateID = 1;

                if (existingProducts.ContainsKey(concentratorProductID))
                {
                  product = existingProducts[concentratorProductID];

                  if (brands.ContainsKey(r.Element("Brand").Attribute("BrandID").Value.Trim()))
                  {
                    var priceElement = r.Element("Price").Element("UnitPrice");
                    product.ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value;
                    product.TaxRateID = taxRateID.Value;
                    //product.LastModificationTime = DateTime.Now;
                    if (priceElement != null)
                      product.UnitPrice = (string.IsNullOrEmpty(priceElement.Value) ? 0 : decimal.Parse(priceElement.Value, CultureInfo.InvariantCulture));
                  }
                  else
                  {
                    log.DebugFormat("Update product failed, brand {0} not availible", r.Element("Brand").Attribute("BrandID").Value.Trim());
                  }
                  context.SubmitChanges();
                }
              }
              catch (Exception ex)
              {
                log.Error("Error processing product" + concentratorProductID, ex);

              }
            }
            else
              log.ErrorFormat("No price found for product {0}", concentratorProductID);

            if (!ConcentratorItemNumbers.Contains(realConcentratorProductID))
              ConcentratorItemNumbers.Add(realConcentratorProductID);

          }
        }
      }
      catch (Exception ex)
      {
        errorImportProduct = true;
        log.Error("Error import products", ex);
      }

      return errorImportProduct;
    }

    public static bool TableExists(WebsiteDataContext context, string tableName)
    {
      bool exists = false;

      try
      {
        using (SqlConnection connection = new SqlConnection(context.Connection.ConnectionString))
        {
          string checkTable =
   String.Format(
      "IF OBJECT_ID('{0}', 'U') IS NOT NULL SELECT 'true' ELSE SELECT 'false'",
      tableName);

          SqlCommand command = new SqlCommand(checkTable, connection);
          command.CommandType = CommandType.Text;
          connection.Open();

          return Convert.ToBoolean(command.ExecuteScalar());
        }
      }
      catch
      {
        exists = false;
      }
      return exists;
    }

    public bool ImportProducts()
    {
      bool errorImportProduct = false;

      try
      {
        using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
        {
          var nodes = (from r in products.Root.Elements("Product")
                       select r);

          if (!this.Connector.UseConcentratorProductID)
            nodes = nodes.Where(x => x.Attribute("CustomProductID").Value != "");

          var productList = (from p in context.Products
                             where p.ManufacturerID != null
                             select new WebsiteProduct
                             {
                               BrandCode = p.Brand.BrandCode,
                               Product = p
                             }).ToList();


          List<int> processedProductIDs = new List<int>();

          //int x1 = 150359;
          //log.DebugFormat("Product {0} is in existingProducts : {1}", x1, productList.Any(x=>x.Product.ProductID == x1));

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();


          if (Connector.UseConcentratorProductID)
          {
            Dictionary<int?, List<Product>> test = (from p in productList.Select(x => x.Product)
                                                    join r in nodes on p.ConcentratorProductID equals int.Parse(r.Attribute("ProductID").Value)
                                                    where !string.IsNullOrEmpty(r.Attribute("ProductID").Value)
                                                    group p by p.ConcentratorProductID
                                                      into grouped
                                                      select grouped).ToDictionary(x => x.Key, y => y.ToList());


            var ids = (from r in nodes
                       where !string.IsNullOrEmpty(r.Attribute("ProductID").Value)
                       select new
                       {
                         brandCode = r.Element("Brands").Element("Brand").Attribute("BrandID").Value.Trim(),
                         manufactuerID = r.Attribute("ManufacturerID").Value.Trim(),
                         concentratorProductID = this.Connector.UseConcentratorProductID ? r.Attribute("ProductID").Value : r.Attribute("CustomProductID").Value
                       }).Distinct().ToList();

            foreach (var id in ids)
            {
              var pr = (from c in productList
                        where c.Product.ManufacturerID.Trim() == id.manufactuerID.Trim()
                              && c.BrandCode.Trim() == id.brandCode.Trim()
                        select c.Product).FirstOrDefault();


              var cpid = Int32.Parse(id.concentratorProductID);
              if (pr != null && !existingProducts.ContainsKey(cpid))
                existingProducts.Add(cpid, pr);
            }
          }
          else
          {
            existingProducts = (from c in productList
                                select c).ToDictionary(p => p.Product.ProductID, p => p.Product);
            // BAS Sites

          }


          var allProductGroups = (from p in context.ProductGroups
                                  select p).ToList();


          var brands = (from b in context.Brands
                        select b).ToDictionary(x => x.BrandCode.Trim(), x => x);


          var productGroupMappings = (from p in context.ProductGroupMappings
                                      group p by p.ProductGroupID
                                        into grouped
                                        select grouped).ToDictionary(x => x.Key, y => y.ToList());

          int total = products.Root.Elements("Product").Count();
          int productsImported = 0;
          int counter = 0;

          foreach (var r in products.Root.Elements("Product"))
          {
            productsImported++;
            counter++;

            if (counter == 100)
            {
              log.DebugFormat("Processing product {0}/{1} for {2}", productsImported, total, "Import Web Products");
              counter = 0;
            }

            int realConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);

            var concentratorProducts = productList.Where(x => x.Product.ConcentratorProductID.HasValue && x.Product.ConcentratorProductID.Value == realConcentratorProductID);

            if (concentratorProducts.Count() > 1)
            {
              if (TableExists(context, "AuctionProducts"))
              {
                foreach (var notVisible in concentratorProducts.Where(x => x.Product.IsVisible == false && x.Product.AuctionProduct == null && x.Product.RelatedProducts.Count < 1))
                {
                  var notVisibleProduct = context.Products.Where(x => x.ProductID == notVisible.Product.ProductID).FirstOrDefault();
                  notVisibleProduct.ConcentratorProductID = null;
                  context.SubmitChanges();
                }
              }
              else
              {
                foreach (var notVisible in concentratorProducts.Where(x => x.Product.IsVisible == false))
                {
                  var notVisibleProduct = context.Products.Where(x => x.ProductID == notVisible.Product.ProductID).FirstOrDefault();
                  notVisibleProduct.ConcentratorProductID = null;
                  context.SubmitChanges();
                }
              }
            }


            //#if DEBUG
            //            if (realConcentratorProductID != 2823168)
            //            {
            //              if (!ConcentratorItemNumbers.Contains(realConcentratorProductID))
            //                ConcentratorItemNumbers.Add(realConcentratorProductID);

            //              continue;
            //            }
            //#endif
            int concentratorProductID = 0;

            if (this.Connector.UseConcentratorProductID)
              concentratorProductID = int.Parse(r.Attribute("ProductID").Value);
            else
            {
              Int32.TryParse(r.Attribute("CustomProductID").Value, out concentratorProductID);
              if (concentratorProductID == 0)
              {
                log.WarnFormat("Skipping product '{0}', because it has a non-numeric Custom Product ID ",
                               r.Attribute("CustomProductID").Value);
                continue;


              }
            }




            if (r.Element("Price") != null)
            {

              Product product = null;
              string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
              string longDescription = r.Element("Content").Attribute("LongDescription").Value;
              string backendDescription = string.Empty;




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




              try
              {
                if (this.Connector.ImportCommercialText)
                  backendDescription = longDescription.Cap(50);

                //  //shortDescription = r.Element("Content").Attribute("ShortContentDescription").Value;
                //  if (!string.IsNullOrEmpty(r.Element("Content").Attribute("LongContentDescription").Value))
                //    longDescription = r.Element("Content").Attribute("LongContentDescription").Value;

                //}

                if (this.Connector.ConcatenateBrandName)
                {
                  StringBuilder sb = new StringBuilder();
                  sb.Append(r.Element("Brands").Element("Brand").Element("Name").Value);
                  sb.Append(" ");
                  sb.Append(r.Element("Content").Attribute("ShortDescription").Value);
                  shortDescription = sb.ToString();
                }

                int? taxRateID = (from t in context.TaxRates
                                  where t.TaxRate1.HasValue
                                  && (t.TaxRate1.Value * 100) == decimal.Parse(r.Element("Price").Attribute("TaxRate").Value, CultureInfo.InvariantCulture)
                                  select t.TaxRateID).FirstOrDefault();

                if (!taxRateID.HasValue)
                  taxRateID = 1;

                //log.DebugFormat("Product {0} is in existingProducts : {1}", concentratorProductID, existingProducts.ContainsKey(concentratorProductID));
                if (existingProducts.ContainsKey(concentratorProductID))
                {
                  product = existingProducts[concentratorProductID];
                  DateTime temppdDate = DateTime.MinValue;
                  DateTime? pdDate = null;
                  if (DateTime.TryParse(r.Element("Stock").Attribute("PromisedDeliveryDate").Value, out temppdDate))
                  {
                    if (temppdDate == DateTime.MinValue && temppdDate == DateTime.MaxValue)
                      pdDate = null;
                    else
                      pdDate = temppdDate;
                  }

                  DateTime tempcutoffDate = DateTime.MinValue;
                  DateTime? cutoffDate = null;
                  if (DateTime.TryParse(r.Attribute("CutOffTime").Value, out tempcutoffDate))
                  {
                    if (tempcutoffDate == DateTime.MinValue && tempcutoffDate == DateTime.MaxValue)
                      cutoffDate = null;
                    else
                      cutoffDate = tempcutoffDate;
                  }

                  foreach (ProductGroup productGroup in productgroups)
                  {
                    IEnumerable<ProductGroupMapping> pgm = null;

                    if (productGroupMappings.ContainsKey(productGroup.ProductGroupID))
                      pgm = productGroupMappings[productGroup.ProductGroupID].Where(x => x.ProductID == product.ProductID);

                    //var pgm = (from p in productGroupMappings
                    //           where p.ProductID == product.ProductID
                    //           && p.ProductGroupID == productGroup.ProductGroupID
                    //           select p).FirstOrDefault();

                    if (pgm == null || pgm.Count() < 1)
                    {

                      if (!pgmList.ContainsKey(concentratorProductID))
                      {
                        ProductGroupMapping mapping = new ProductGroupMapping
                        {
                          Product = product,
                          ProductGroup = productGroup
                        };
                        context.ProductGroupMappings.InsertOnSubmit(mapping);
                      }
                      else
                      {
                        if (!pgmList[concentratorProductID].Contains(productGroup.ProductGroupID))
                        {
                          ProductGroupMapping mapping = new ProductGroupMapping
                          {
                            Product = product,
                            ProductGroup = productGroup
                          };
                          context.ProductGroupMappings.InsertOnSubmit(mapping);
                        }
                      }
                    }
                    AddProductGroupMapping(concentratorProductID, productGroup.ProductGroupID);
                    context.SubmitChanges();
                  }



                  if (brands.ContainsKey(r.Element("Brands").Element("Brand").Attribute("BrandID").Value.Trim()))
                  {

                    if (productgroups.Count() > 0)
                    {
                      if (//existingProducts[customproductID].ProductGroupID != productGroup.ProductGroupID
                        product.ShortDescription != shortDescription
                        //|| product.LongDescription != longDescription
                        || product.UnitPrice != (string.IsNullOrEmpty(r.Element("Price").Element("UnitPrice").Value) ? 0 : decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture))
                        || product.ProductStatus != (string.IsNullOrEmpty(r.Element("Price").Attribute("CommercialStatus").Value) ? "O" : r.Element("Price").Attribute("CommercialStatus").Value)
                        || product.ManufacturerID != r.Attribute("ManufacturerID").Value
                        || product.IsVisible != true
                        || product.TaxRateID != taxRateID
                        || product.BrandID != brands[r.Element("Brands").Element("Brand").Attribute("BrandID").Value.Trim()].BrandID
                        || ((pdDate.HasValue && !product.PromisedDeliveryDate.HasValue) || (!pdDate.HasValue && product.PromisedDeliveryDate.HasValue) || (pdDate.HasValue && product.PromisedDeliveryDate.HasValue && DateTime.Compare(product.PromisedDeliveryDate.Value, pdDate.Value) != 0))
                        || (!string.IsNullOrEmpty(backendDescription) && product.BackEndDescription != backendDescription)
                        || product.CustomProductID != r.Attribute("CustomProductID").Value
                        || product.ConcentratorProductID != realConcentratorProductID
                        || ((cutoffDate.HasValue && !product.CutoffTime.HasValue) || (!cutoffDate.HasValue && product.CutoffTime.HasValue) || (cutoffDate.HasValue && product.CutoffTime.HasValue && (product.CutoffTime.Value.Hour != cutoffDate.Value.Hour || product.CutoffTime.Value.Minute != cutoffDate.Value.Minute))))
                      {
                        product.ShortDescription = shortDescription;
                        //product.LongDescription = longDescription;
                        product.BrandID = brands[r.Element("Brands").Element("Brand").Attribute("BrandID").Value.Trim()].BrandID;
                        product.UnitPrice = (string.IsNullOrEmpty(r.Element("Price").Element("UnitPrice").Value) ? 0 : decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture));
                        product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                        product.ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value;
                        product.LastModificationTime = DateTime.Now;
                        product.IsVisible = true;
                        product.PromisedDeliveryDate = pdDate;
                        product.TaxRateID = taxRateID.Value;
                        if (!string.IsNullOrEmpty(backendDescription))
                          product.BackEndDescription = backendDescription;
                        product.LineType = string.IsNullOrEmpty(r.Attribute("LineType").Value) ? "S" : r.Attribute("LineType").Value;
                        product.CutoffTime = cutoffDate;
                      }
                    }
                    else
                    {
                      product.IsVisible = false;

                      var priceElement = r.Element("Price").Element("UnitPrice");


                      log.DebugFormat("Set Product {0} false, update price to {1}", concentratorProductID, priceElement, CultureInfo.InvariantCulture);

                      product.ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value;
                      product.LastModificationTime = DateTime.Now;
                      if (priceElement != null)
                        product.UnitPrice = (string.IsNullOrEmpty(priceElement.Value) ? 0 : decimal.Parse(priceElement.Value, CultureInfo.InvariantCulture));


                    }
                    product.CustomProductID = r.Attribute("CustomProductID").Value;
                    product.ConcentratorProductID = realConcentratorProductID;
                  }
                  else
                  {
                    log.DebugFormat("Update product failed, brand {0} not availible", r.Element("Brand").Attribute("BrandID").Value.Trim());
                  }


                }
                else
                {
                  if (!ConcentratorItemNumbers.Contains(realConcentratorProductID))
                  {
                    decimal unitprice = (string.IsNullOrEmpty(r.Element("Price").Element("UnitPrice").Value) ? 0 : decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture));

                    if (brands.ContainsKey(r.Element("Brands").Element("Brand").Attribute("BrandID").Value.Trim()))
                    {

                      var brandID = brands[r.Element("Brands").Element("Brand").Attribute("BrandID").Value.Trim()].BrandID;

                      if (!Connector.UseConcentratorProductID)
                      {
                        if (!processedProductIDs.Contains(concentratorProductID) && !context.Products.Any(x => x.ProductID == concentratorProductID))
                        {
                          log.DebugFormat("DO NOT Use concentrator for insert product {0}", concentratorProductID);
                          //create stub product with identity_insert on
                          if (!string.IsNullOrEmpty(r.Attribute("ManufacturerID").Value))
                          {
                            string cmd =
                              String.Format(
                                @"
INSERT INTO Products (ProductID, ShortDescription,LongDescription,brandid,taxrateid,iscustom,isvisible,extendedcatalog
	, canmodifyprice,creationtime,lastmodificationtime
	)  VALUES( {0}, '{1}','{2}', {3},{4},0,0,1,0,getdate(), getdate())
",
                                concentratorProductID, shortDescription.Replace("'", "''"), longDescription.Replace("'", "''"), brandID, taxRateID.Value);

                            context.ExecuteCommand(cmd);

                            product = context.Products.Single(x => x.ProductID == concentratorProductID);
                            product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                            product.UnitPrice = unitprice;
                            product.UnitCost = 0;
                            product.IsCustom = false;
                            product.LineType = "S";
                            product.ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value;
                            product.IsVisible = true;
                            product.CustomProductID = r.Attribute("CustomProductID").Value;
                            product.ConcentratorProductID = realConcentratorProductID;

                            context.SubmitChanges();
                            processedProductIDs.Add(concentratorProductID);
                          }
                          else
                            log.DebugFormat("Cannot insert product {0}, manufacturerid missing", concentratorProductID);
                        }

                      }
                      else
                      {
                        log.DebugFormat("Use concentrator productID for insert product {0}", concentratorProductID);

                        product = new Product
                                    {
                                      //ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                                      //ProductGroupID = subprodid,
                                      ShortDescription = shortDescription,
                                      LongDescription = longDescription,
                                      ManufacturerID = r.Attribute("ManufacturerID").Value,
                                      BrandID = brandID,
                                      TaxRateID = taxRateID.Value,
                                      UnitPrice = unitprice,
                                      UnitCost = 0,
                                      IsCustom = false,
                                      LineType = "S",
                                      ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value,
                                      IsVisible = true,
                                      CreationTime = DateTime.Now,
                                      LastModificationTime = DateTime.Now
                                    };

                        product.CustomProductID = r.Attribute("CustomProductID").Value;
                        product.ConcentratorProductID = realConcentratorProductID;
                        context.Products.InsertOnSubmit(product);

                      }



                      //Type t = typeof(Product);
                      //PropertyInfo pi = t.GetProperty("ProductID");
                      //ColumnAttribute ca = Attribute.GetCustomAttribute(pi, typeof(ColumnAttribute)) as ColumnAttribute;

                      //if (!this.Connector.UseConcentratorProductID)
                      //{
                      //  ca.AutoSync = AutoSync.Never;
                      //  ca.IsDbGenerated = false;
                      //  product.ProductID = concentratorProductID;
                      //  log.DebugFormat("Creating new product with CID : {0} - {1}", concentratorProductID, context.GetChangeSet().Inserts.Count);
                      //}
                      //else
                      //{
                      //  ca.AutoSync = AutoSync.OnInsert;
                      //  ca.IsDbGenerated = true;
                      //}


                      if (!string.IsNullOrEmpty(backendDescription))
                        product.BackEndDescription = backendDescription;

                      DateTime pdDate;
                      if (DateTime.TryParse(r.Element("Stock").Attribute("PromisedDeliveryDate").Value, out pdDate))
                      {
                        if (pdDate != DateTime.MinValue && pdDate != DateTime.MaxValue)
                          product.PromisedDeliveryDate = pdDate;
                      }

                      DateTime cutoffDate;
                      if (DateTime.TryParse(r.Attribute("CutOffTime").Value, out cutoffDate))
                      {
                        if (cutoffDate == DateTime.MinValue && cutoffDate == DateTime.MaxValue)
                          product.CutoffTime = cutoffDate;
                      }

                      //if (product.BrandID > 0)
                      context.SubmitChanges();

                      foreach (ProductGroup productgroup in productgroups)
                      {
                        IEnumerable<ProductGroupMapping> pgm = null;

                        if (productGroupMappings.ContainsKey(productgroup.ProductGroupID))
                          pgm = productGroupMappings[productgroup.ProductGroupID].Where(x => x.ProductID == product.ProductID);

                        //var pgm = (from p in productGroupMappings
                        //           where p.ProductID == product.ProductID
                        //           && p.ProductGroupID == productgroup.ProductGroupID
                        //           select p).FirstOrDefault();

                        if (!productGroupMappings.ContainsKey(productgroup.ProductGroupID) || pgm == null || pgm.Count() < 1)
                        {
                          if (pgmList.ContainsKey(concentratorProductID))
                            if (pgmList[concentratorProductID].Contains(productgroup.ProductGroupID))
                              continue;

                          ProductGroupMapping mapping = new ProductGroupMapping
                          {
                            Product = product,
                            ProductGroup = productgroup
                          };
                          context.ProductGroupMappings.InsertOnSubmit(mapping);
                        }
                        AddProductGroupMapping(concentratorProductID, productgroup.ProductGroupID);
                      }
                    }
                    else
                    {
                      log.DebugFormat("Insert product failed, brand {0} not available", r.Element("Brand").Attribute("BrandID").Value.Trim());
                    }
                  }
                }

                if (!ConcentratorItemNumbers.Contains(realConcentratorProductID))
                  ConcentratorItemNumbers.Add(realConcentratorProductID);

                context.SubmitChanges();
              }
              catch (Exception ex)
              {
                log.Error("Error processing product" + concentratorProductID, ex);
              }
            }
            else
              log.ErrorFormat("No price found for product {0}", concentratorProductID);
          }
          context.SubmitChanges();
        }
      }
      catch (Exception ex)
      {
        errorImportProduct = true;
        log.Error("Error import products", ex);
      }

      return errorImportProduct;
    }

    public void ImportSelectorProducts()
    {
      try
      {
        using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
        {
          var nodes = (from r in products.Root.Elements("Product")
                       select r);

          if (!this.Connector.UseConcentratorProductID)
            nodes = nodes.Where(x => x.Attribute("CustomProductID").Value != "");

          var productList = (from p in context.Products
                             where p.ManufacturerID != null
                             select new WebsiteProduct
                             {
                               BrandCode = p.Brand.BrandCode,
                               Product = p
                             }).ToList();

          //int x1 = 150359;
          //log.DebugFormat("Product {0} is in existingProducts : {1}", x1, productList.Any(x=>x.Product.ProductID == x1));

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();


          if (Connector.UseConcentratorProductID)
          {
            Dictionary<int?, List<Product>> test = (from p in productList.Select(x => x.Product)
                                                    join r in nodes on p.ConcentratorProductID equals int.Parse(r.Attribute("ProductID").Value)
                                                    where !string.IsNullOrEmpty(r.Attribute("ProductID").Value)
                                                    group p by p.ConcentratorProductID
                                                      into grouped
                                                      select grouped).ToDictionary(x => x.Key, y => y.ToList());


            var ids = (from r in nodes
                       where !string.IsNullOrEmpty(r.Attribute("ProductID").Value)
                       select new
                       {
                         brandCode = r.Element("Brand").Attribute("BrandID").Value.Trim(),
                         manufactuerID = r.Attribute("ManufacturerID").Value.Trim(),
                         concentratorProductID = this.Connector.UseConcentratorProductID ? r.Attribute("ProductID").Value : r.Attribute("CustomProductID").Value
                       }).Distinct().ToList();

            foreach (var id in ids)
            {
              var pr = (from c in productList
                        where c.Product.ManufacturerID.Trim() == id.manufactuerID.Trim()
                              && c.BrandCode.Trim() == id.brandCode.Trim()
                        select c.Product).FirstOrDefault();


              var cpid = Int32.Parse(id.concentratorProductID);
              if (pr != null && !existingProducts.ContainsKey(cpid))
                existingProducts.Add(cpid, pr);
            }
          }
          else
          {
            existingProducts = (from c in productList
                                select c).ToDictionary(p => p.Product.ProductID, p => p.Product);
            // BAS Sites

          }


          var allProductGroups = (from p in context.ProductGroups
                                  select p).ToList();


          var brands = (from b in context.Brands
                        select b).ToDictionary(x => x.BrandCode.Trim(), x => x);


          var productGroupMappings = (from p in context.ProductGroupMappings
                                      group p by p.ProductGroupID
                                        into grouped
                                        select grouped).ToDictionary(x => x.Key, y => y.ToList());

          foreach (var r in products.Root.Elements("Product"))
          {
            int realConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);

            //#if DEBUG
            //            if (realConcentratorProductID != 2823168)
            //            {
            //              if (!ConcentratorItemNumbers.Contains(realConcentratorProductID))
            //                ConcentratorItemNumbers.Add(realConcentratorProductID);

            //              continue;
            //            }
            //#endif
            int concentratorProductID = 0;

            if (this.Connector.UseConcentratorProductID)
              concentratorProductID = int.Parse(r.Attribute("ProductID").Value);
            else
            {
              Int32.TryParse(r.Attribute("CustomProductID").Value, out concentratorProductID);
              if (concentratorProductID == 0)
              {
                log.WarnFormat("Skipping product '{0}', because it has a non-numeric Custom Product ID ",
                               r.Attribute("CustomProductID").Value);
                continue;


              }
            }




            if (r.Element("Price") != null)
            {
              Product product = null;
              string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
              string longDescription = r.Element("Content").Attribute("LongDescription").Value;

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




              try
              {
                //  //shortDescription = r.Element("Content").Attribute("ShortContentDescription").Value;
                //  if (!string.IsNullOrEmpty(r.Element("Content").Attribute("LongContentDescription").Value))
                //    longDescription = r.Element("Content").Attribute("LongContentDescription").Value;

                //}

                if (this.Connector.ConcatenateBrandName)
                {
                  StringBuilder sb = new StringBuilder();
                  sb.Append(r.Element("Brand").Element("Name").Value);
                  sb.Append(" ");
                  sb.Append(r.Element("Content").Attribute("ShortDescription").Value);
                  shortDescription = sb.ToString();
                }

                int? taxRateID = (from t in context.TaxRates
                                  where t.TaxRate1.HasValue
                                  && (t.TaxRate1.Value * 100) == decimal.Parse(r.Element("Price").Attribute("TaxRate").Value, CultureInfo.InvariantCulture)
                                  select t.TaxRateID).FirstOrDefault();

                if (!taxRateID.HasValue)
                  taxRateID = 1;

                //log.DebugFormat("Product {0} is in existingProducts : {1}", concentratorProductID, existingProducts.ContainsKey(concentratorProductID));
                if (existingProducts.ContainsKey(concentratorProductID))
                {
                  product = existingProducts[concentratorProductID];

                  foreach (ProductGroup productGroup in productgroups)
                  {
                    IEnumerable<ProductGroupMapping> pgm = null;

                    if (productGroupMappings.ContainsKey(productGroup.ProductGroupID))
                      pgm = productGroupMappings[productGroup.ProductGroupID].Where(x => x.ProductID == product.ProductID);

                    //var pgm = (from p in productGroupMappings
                    //           where p.ProductID == product.ProductID
                    //           && p.ProductGroupID == productGroup.ProductGroupID
                    //           select p).FirstOrDefault();

                    if (pgm == null || pgm.Count() < 1)
                    {

                      if (!pgmList.ContainsKey(concentratorProductID))
                      {
                        ProductGroupMapping mapping = new ProductGroupMapping
                        {
                          Product = product,
                          ProductGroup = productGroup
                        };
                        context.ProductGroupMappings.InsertOnSubmit(mapping);
                      }
                      else
                      {
                        if (!pgmList[concentratorProductID].Contains(productGroup.ProductGroupID))
                        {
                          ProductGroupMapping mapping = new ProductGroupMapping
                          {
                            Product = product,
                            ProductGroup = productGroup
                          };
                          context.ProductGroupMappings.InsertOnSubmit(mapping);
                        }
                      }
                    }
                    AddProductGroupMapping(concentratorProductID, productGroup.ProductGroupID);
                    context.SubmitChanges();
                  }


                  product.ConcentratorProductID = realConcentratorProductID;

                  if (brands.ContainsKey(r.Element("Brand").Attribute("BrandID").Value.Trim()))
                  {

                    if (productgroups.Count() > 0)
                    {
                      if (product.ShortDescription != shortDescription)
                      {
                        product.ShortDescription = shortDescription;
                      }
                    }
                  }
                  else
                  {
                    log.DebugFormat("Update product failed, brand {0} not availible", r.Element("Brand").Attribute("BrandID").Value.Trim());
                  }


                }
                else
                {
                  if (!ConcentratorItemNumbers.Contains(realConcentratorProductID))
                  {
                    decimal unitprice = (string.IsNullOrEmpty(r.Element("Price").Element("UnitPrice").Value) ? 0 : decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture));

                    if (brands.ContainsKey(r.Element("Brand").Attribute("BrandID").Value.Trim()))
                    {

                      var brandID = brands[r.Element("Brand").Attribute("BrandID").Value.Trim()].BrandID;

                      if (!this.Connector.UseConcentratorProductID)
                      {
                        //create stub product with identity_insert on
                        if (!string.IsNullOrEmpty(r.Attribute("ManufacturerID").Value))
                        {
                          string cmd =
                            String.Format(
                              @"
INSERT INTO Products (ProductID, ShortDescription,LongDescription,brandid,taxrateid,iscustom,isvisible,extendedcatalog
	, canmodifyprice,creationtime,lastmodificationtime
	)  VALUES( {0}, '{1}','{2}', {3},{4},0,0,1,0,getdate(), getdate())
",
                              concentratorProductID, shortDescription.Replace("'", "''"), longDescription.Replace("'", "''"), brandID, taxRateID.Value);

                          context.ExecuteCommand(cmd);

                          product = context.Products.Single(x => x.ProductID == concentratorProductID);
                          product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                          product.UnitPrice = unitprice;
                          product.UnitCost = 0;
                          product.IsCustom = false;
                          product.LineType = "S";
                          product.ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value;
                          product.IsVisible = false;
                          product.CustomProductID = r.Attribute("CustomProductID").Value;
                          product.ConcentratorProductID = realConcentratorProductID;

                          context.SubmitChanges();
                        }
                        else
                          log.DebugFormat("Cannot insert product {0}, manufacturerid missing", concentratorProductID);

                      }
                      else
                      {


                        product = new Product
                        {
                          //ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                          //ProductGroupID = subprodid,
                          ShortDescription = shortDescription,
                          LongDescription = longDescription,
                          ManufacturerID = r.Attribute("ManufacturerID").Value,
                          BrandID = brandID,
                          TaxRateID = taxRateID.Value,
                          UnitPrice = unitprice,
                          UnitCost = 0,
                          IsCustom = false,
                          LineType = "S",
                          ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value,
                          IsVisible = false,
                          CreationTime = DateTime.Now,
                          LastModificationTime = DateTime.Now
                        };

                        product.CustomProductID = r.Attribute("CustomProductID").Value;
                        product.ConcentratorProductID = realConcentratorProductID;
                        context.Products.InsertOnSubmit(product);

                      }



                      //Type t = typeof(Product);
                      //PropertyInfo pi = t.GetProperty("ProductID");
                      //ColumnAttribute ca = Attribute.GetCustomAttribute(pi, typeof(ColumnAttribute)) as ColumnAttribute;

                      //if (!this.Connector.UseConcentratorProductID)
                      //{
                      //  ca.AutoSync = AutoSync.Never;
                      //  ca.IsDbGenerated = false;
                      //  product.ProductID = concentratorProductID;
                      //  log.DebugFormat("Creating new product with CID : {0} - {1}", concentratorProductID, context.GetChangeSet().Inserts.Count);
                      //}
                      //else
                      //{
                      //  ca.AutoSync = AutoSync.OnInsert;
                      //  ca.IsDbGenerated = true;
                      //}


                      //if (!string.IsNullOrEmpty(backendDescription))
                      //  product.BackEndDescription = backendDescription;

                      //DateTime pdDate;
                      //if (DateTime.TryParse(r.Element("Stock").Attribute("PromisedDeliveryDate").Value, out pdDate))
                      //{
                      //  if (pdDate != DateTime.MinValue && pdDate != DateTime.MaxValue)
                      //    product.PromisedDeliveryDate = pdDate;
                      //}

                      //if (product.BrandID > 0)
                      context.SubmitChanges();

                      foreach (ProductGroup productgroup in productgroups)
                      {
                        IEnumerable<ProductGroupMapping> pgm = null;

                        if (productGroupMappings.ContainsKey(productgroup.ProductGroupID))
                          pgm = productGroupMappings[productgroup.ProductGroupID].Where(x => x.ProductID == product.ProductID);

                        //var pgm = (from p in productGroupMappings
                        //           where p.ProductID == product.ProductID
                        //           && p.ProductGroupID == productgroup.ProductGroupID
                        //           select p).FirstOrDefault();

                        if (!productGroupMappings.ContainsKey(productgroup.ProductGroupID) || pgm == null)
                        {
                          if (pgmList.ContainsKey(concentratorProductID))
                            if (pgmList[concentratorProductID].Contains(productgroup.ProductGroupID))
                              continue;

                          ProductGroupMapping mapping = new ProductGroupMapping
                          {
                            Product = product,
                            ProductGroup = productgroup
                          };
                          context.ProductGroupMappings.InsertOnSubmit(mapping);
                        }
                        AddProductGroupMapping(concentratorProductID, productgroup.ProductGroupID);
                      }
                    }
                    else
                    {
                      log.DebugFormat("Insert product failed, brand {0} not available", r.Element("Brand").Attribute("BrandID").Value.Trim());
                    }
                  }
                }

                if (!ConcentratorItemNumbers.Contains(realConcentratorProductID))
                  ConcentratorItemNumbers.Add(realConcentratorProductID);

                context.SubmitChanges();
              }
              catch (Exception ex)
              {
                log.Error("Error processing product" + concentratorProductID, ex);
              }
            }
            else
              log.ErrorFormat("No price found for product {0}", concentratorProductID);
          }
          context.SubmitChanges();
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import products", ex);
      }
    }

    public void ImportRetailStock()
    {
      try
      {
        using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
        {
          Dictionary<int, List<VendorStock>> existingStock = new Dictionary<int, List<VendorStock>>();


          var retailProducts = (from r in products.Root.Elements("Product")
                                select new
                                {
                                  ProductID = int.Parse(r.Attribute("ProductID").Value),
                                  RetailStock = r.Element("Stock").Element("Retail").Elements("RetailStock")
                                }).Distinct().ToList();

          var stockVendors = (from r in products.Root.Elements("Product").Elements("Stock").Elements("Retail").Elements("RetailStock")
                              select int.Parse(r.Attribute("VendorCode").Value)).Distinct().ToList();

          var stockVendorLocations = (from v in context.Relations
                                      where stockVendors.Contains(v.RelationID)
                                      select new
                                      {
                                        RelationID = v.RelationID,
                                        StockLocation = v.Name
                                      }).ToList();

          var ExistingVendorStock = (from vs in context.VendorStocks
                                     where vs.Product.ConcentratorProductID.HasValue
                                     select new
                                     {
                                       ConcentratorProductID = vs.Product.ConcentratorProductID.Value,
                                       VendorStock = vs
                                     }).ToList();

          var UnusedStockRecords = ExistingVendorStock.ToList();

          var websiteProducts = (from p in context.Products
                                 where p.IsVisible && p.ConcentratorProductID.HasValue
                                 select new
                                 {
                                   productID = p.ProductID,
                                   concentratorProductid = p.ConcentratorProductID.Value
                                 }).ToList();

          Dictionary<int, IEnumerable<XElement>> records = new Dictionary<int, IEnumerable<XElement>>();
          foreach (var retail in retailProducts)
          {
            if (!records.ContainsKey(retail.ProductID))
              records.Add(retail.ProductID, retail.RetailStock);
          }

          int totalProducts = records.Keys.Count;
          int couterProduct = 0;
          int logCount = 0;

          foreach (var r in records.Keys)
          {
            couterProduct++;
            logCount++;
            if (logCount == 100)
            {
              log.DebugFormat("retailstock Processed : {0}/{1} for connector {2}", couterProduct, totalProducts, Connector.Name);
              logCount = 0;
            }

            try
            {
              List<VendorStock> vStockList = ExistingVendorStock.Where(x => x.ConcentratorProductID == r).Select(x => x.VendorStock).ToList();

              foreach (var p in records[r])
              {
                var location = stockVendorLocations.Where(x => x.RelationID == int.Parse(p.Attribute("VendorCode").Value)).Select(x => x.StockLocation).FirstOrDefault();

                if (!string.IsNullOrEmpty(location))
                {
                  VendorStock stock = vStockList.Where(x => x.RelationID == int.Parse(p.Attribute("VendorCode").Value) && x.StockLocationID == location).FirstOrDefault();

                  if (stock == null)
                  {
                    int? productID = websiteProducts.Where(x => x.concentratorProductid == r).Select(x => x.productID).FirstOrDefault();

                    if (productID.HasValue && productID.Value > 0)
                    {
                      stock = new VendorStock();
                      stock.CreationTime = DateTime.Now;
                      stock.LastModificationTime = DateTime.Now;
                      stock.CreatedBy = 0;
                      stock.LastModifiedBy = 0;
                      stock.ProductID = productID.Value;
                      stock.RelationID = int.Parse(p.Attribute("VendorCode").Value);
                      stock.VendorProductStatus = "S";
                      stock.StockLocationID = location;
                      stock.QuantityOnHand = int.Parse(p.Attribute("InStock").Value);
                      context.VendorStocks.InsertOnSubmit(stock);
                      ExistingVendorStock.Add(new { ConcentratorProductID = r, VendorStock = stock });
                    }
                    else
                      continue;
                  }
                  else
                  {
                    vStockList.Remove(stock);
                    ExistingVendorStock.Remove(new { ConcentratorProductID = r, VendorStock = stock });
                    UnusedStockRecords.Remove(new { ConcentratorProductID = r, VendorStock = stock });
                  }

                  if (stock.QuantityOnHand != int.Parse(p.Attribute("InStock").Value))
                  {
                    stock.QuantityOnHand = int.Parse(p.Attribute("InStock").Value);
                    stock.LastModificationTime = DateTime.Now;
                  }
                  context.SubmitChanges();
                }
              }

              //context.VendorStocks.DeleteAllOnSubmit(vStockList);
              //context.SubmitChanges();
            }
            catch (Exception ex)
            {
              log.ErrorFormat("Error import retailstock for {0}", r);
            }
          }

          context.VendorStocks.DeleteAllOnSubmit(UnusedStockRecords.Select(x => x.VendorStock));
          context.SubmitChanges();
        }

      }
      catch (Exception ex)
      {
        log.Error("Error import vendor stock", ex);
        log.Debug(ex.StackTrace);
      }
    }

    public void ImportStock()
    {
      try
      {
        using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
        {
          var nodes = (from r in products.Root.Elements("Product")
                       select r);

          if (!this.Connector.UseConcentratorProductID)
            nodes = nodes.Where(x => x.Attribute("CustomProductID").Value != "");


          var ids = (from r in nodes
                     select new
                     {
                       brandCode = r.Element("Brand").Attribute("BrandID").Value.Trim(),
                       manufactuerID = r.Attribute("ManufacturerID").Value.Trim(),
                       concentratorProductID = this.Connector.UseConcentratorProductID ? r.Attribute("ProductID").Value : r.Attribute("CustomProductID").Value
                     }).Distinct().ToList();

          Dictionary<int, Stock> existingStock = new Dictionary<int, Stock>();


          var stockProducts = (from s in context.Stocks
                               join p in context.Products on s.ProductID equals p.ProductID
                               where s.StockLocationID == 1
                               select new
                                {
                                  brandcode = p.Brand.BrandCode,
                                  product = p,
                                  stock = s
                                }).ToList();

          List<int> _existingProductIDs = (from s in stockProducts
                                           select s.product.ProductID).Distinct().ToList();



          if (this.Connector.UseConcentratorProductID)
          {
            foreach (var id in ids)
            {

              var stk = (from p in stockProducts
                         where (p.product.ConcentratorProductID.HasValue
                                && p.product.ConcentratorProductID.Value == Int32.Parse(id.concentratorProductID))
                         select p.stock).FirstOrDefault();

              var cpid = Int32.Parse(id.concentratorProductID);

              if (stk != null && !existingStock.ContainsKey(cpid))
                existingStock.Add(cpid, stk);
            }
          }
          else
          {
            existingStock = (from p in stockProducts
                             select p).ToDictionary(p => p.product.ProductID, p => p.stock);
            // BAS Sites
          }

          List<int> addedstock = new List<int>();

          int productCount = products.Root.Elements("Product").Count();
          int counter = 0;
          int logCount = 0;

          foreach (var r in products.Root.Elements("Product"))
          {
            counter++;
            logCount++;
            if (logCount == 250)
            {
              log.DebugFormat("Products Processed : {0}/{1} for Connector {2}", counter, productCount, Connector.Name);
              logCount = 0;
            }

            Product websiteProduct = null;


            int realConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);

            int concentratorProductID = 0;

            if (Connector.UseConcentratorProductID)
              concentratorProductID = int.Parse(r.Attribute("ProductID").Value);
            else
            {
              Int32.TryParse(r.Attribute("CustomProductID").Value, out concentratorProductID);
              if (concentratorProductID == 0)
              {
                log.WarnFormat("Skipping product '{0}', because it has a non-numeric Custom Product ID ",
                               r.Attribute("CustomProductID").Value);
                continue;


              }
            }

            Stock stock = null;


            if (existingStock.ContainsKey(concentratorProductID))
            {
              _existingProductIDs.Remove(concentratorProductID);
              if (existingStock[concentratorProductID].QuantityOnHand != int.Parse(r.Element("Stock").Attribute("InStock").Value))
              {
                stock = existingStock[concentratorProductID];
                stock.QuantityOnHand = int.Parse(r.Element("Stock").Attribute("InStock").Value);
                stock.LastModificationTime = DateTime.Now;
              }
            }
            else
            {
              var productID = (from p in context.Products
                               where p.ConcentratorProductID == realConcentratorProductID
                               select p.ProductID).FirstOrDefault();

              if (productID > 0)
              {
                stock = new Stock();
                stock.CreationTime = DateTime.Now;
                stock.LastModificationTime = DateTime.Now;
                stock.CreatedBy = 0;
                stock.LastModifiedBy = 0;
                stock.ProductID = productID;
                stock.StockLocationID = 1;
                stock.QuantityOnHand = int.Parse(r.Element("Stock").Attribute("InStock").Value);
                context.Stocks.InsertOnSubmit(stock);
                existingStock.Add(concentratorProductID, stock);
              }

            }

            try
            {
              context.SubmitChanges();
            }
            catch (Exception ex)
            {
              log.Error("Error stock update/insert for product" + concentratorProductID.ToString());
            }


            //if (this.Connector.UseConcentratorProductID)
            //{
            //  websiteProduct = (from p in stockProducts
            //                    where p.product.ProductID == realConcentratorProductID
            //                    select p.product).FirstOrDefault();
            //}
            //else
            //{
            //  websiteProduct = (from p in stockProducts
            //                    where
            //                    p.product.ProductID == concentratorProductID
            //                    select p.product).FirstOrDefault();
            //}

            //if (websiteProduct != null)
            //{
            //  _existingProductIDs.Remove(websiteProduct.ProductID);


            //}
          }

          //cleanup old records
          //foreach (int pid in _existingProductIDs)
          //{
          //  int productID = pid;
          //  context.Stocks.DeleteAllOnSubmit(context.Stocks.Where(x => x.ProductID == productID && x.StockLocationID == 1));
          //}

          context.SubmitChanges();
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import stock", ex);
      }
    }

    public void ImportBrands()
    {
      try
      {
        using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
        {
          List<string> CreatedBrands = new List<string>();

          var ids = (from r in products.Root.Elements("Product").Elements("Brands").Elements("Brand")
                     select r.Attribute("BrandID").Value).Distinct().ToArray();

          Dictionary<string, Brand> existingBrands = new Dictionary<string, Brand>();
          List<string> brandcodes = new List<string>();

          foreach (string id in ids)
          {
            var stk = (from c in context.Brands
                       where c.BrandCode == id.Trim()
                       select c).FirstOrDefault();

            if (stk != null && !existingBrands.ContainsKey(stk.BrandCode.Trim()))
            {
              existingBrands.Add(stk.BrandCode.Trim(), stk);
              brandcodes.Add(stk.BrandCode.Trim());
            }
          }

          //var nonUsedBrands = (from b in context.Brands
          //                     where !brandcodes.Contains(b.BrandCode.Trim())
          //                     && b.IsActive == true
          //                     select b).ToList();

          //if (nonUsedBrands.Count > 0)
          //{
          //  foreach (Brand brand in nonUsedBrands)
          //  {
          //    brand.IsActive = false;
          //  }
          //  context.SubmitChanges();
          //}



          foreach (var r in products.Root.Elements("Product").Elements("Brands").Elements("Brand").Distinct())
          {
            Brand brand = null;
            string brandID = r.Attribute("BrandID").Value;

            if (existingBrands.ContainsKey(brandID))
            {
              if (existingBrands[brandID].BrandName != r.Element("Name").Value
                || existingBrands[brandID].IsActive == false)
              {
                brand = existingBrands[brandID];
                brand.BrandName = r.Element("Name").Value;
                brand.IsActive = true;
                brand.LastModificationTime = DateTime.Now;
              }
            }
            else
            {
              if (!CreatedBrands.Contains(brandID))
              {
                brand = new Brand();
                brand.CreationTime = DateTime.Now;
                brand.LastModificationTime = DateTime.Now;
                brand.CreatedBy = 0;
                brand.LastModifiedBy = 0;
                brand.BrandCode = brandID;
                brand.BrandName = r.Element("Name").Value;
                brand.IsActive = true;
                context.Brands.InsertOnSubmit(brand);
                CreatedBrands.Add(brand.BrandCode);
              }
            }
          }
          context.SubmitChanges();
        }
      }
      catch (Exception ex)
      {
        log.Error("Import Brand failed", ex);
      }
    }

    public void ImportOItems(XDocument o_Products, string customProductID)
    {
      try
      {
        using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
        {
          var ids = (from r in products.Root.Elements("Product")
                     select new
                     {
                       brandCode = r.Element("Brand").Attribute("BrandID").Value.Trim(),
                       manufactuerID = r.Attribute("ManufacturerID").Value.Trim(),
                       concentratorProductID = int.Parse(r.Attribute("CustomProductID").Value)
                     }).Distinct().ToList();

          var productList = (from p in context.Products
                             where (!string.IsNullOrEmpty(customProductID) && (this.Connector.UseConcentratorProductID && p.CustomProductID == customProductID)
                             || (!this.Connector.UseConcentratorProductID && p.ProductID == int.Parse(customProductID)))
                             select new
                             {
                               brandCode = p.Brand.BrandCode,
                               product = p
                             }).ToList();

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();

          foreach (var id in ids)
          {
            var pr = (from p in productList
                      where (p.brandCode == id.brandCode
     && p.product.ManufacturerID == id.manufactuerID)
                      select p.product).FirstOrDefault();


            if (pr != null && !existingProducts.ContainsKey(id.concentratorProductID))
              existingProducts.Add(id.concentratorProductID, pr);
          }

          var brands = (from b in context.Brands
                        select b).ToDictionary(x => x.BrandCode.Trim(), x => x);

          List<int> addedcustomOItemNumbers = new List<int>();

          foreach (var r in o_Products.Root.Elements("Product"))
          {
            Product product = null;
            string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
            string longDescription = r.Element("Content").Attribute("LongDescription").Value;
            int concentratorProductID = int.Parse(r.Attribute("CustomProductID").Value);

            try
            {
              if (!addedcustomOItemNumbers.Contains(concentratorProductID))
              {
                int? taxRateID = (from t in context.TaxRates
                                  where t.TaxRate1.HasValue
                                  && (t.TaxRate1.Value * 100) == int.Parse(r.Element("Price").Attribute("TaxRate").Value)
                                  select t.TaxRateID).FirstOrDefault();

                if (!taxRateID.HasValue)
                  taxRateID = 1;

                if (existingProducts.ContainsKey(concentratorProductID))
                {
                  if (brands.ContainsKey(r.Element("Brand").Attribute("BrandID").Value.Trim()))
                  {
                    if (
                      existingProducts[concentratorProductID].ShortDescription != shortDescription
                      || existingProducts[concentratorProductID].LongDescription != longDescription
                      || existingProducts[concentratorProductID].UnitPrice != decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture)
                      || existingProducts[concentratorProductID].ProductStatus != r.Attribute("CommercialStatus").Value
                      || existingProducts[concentratorProductID].ManufacturerID != r.Attribute("ManufacturerID").Value
                      || existingProducts[concentratorProductID].IsVisible != false
                      || existingProducts[concentratorProductID].BrandID != brands[r.Element("Brand").Attribute("BrandID").Value.Trim()].BrandID
                      || existingProducts[concentratorProductID].TaxRateID != taxRateID.Value
                      || existingProducts[concentratorProductID].LineType != r.Attribute("LineType").Value.Trim()
                      || existingProducts[concentratorProductID].CustomProductID != r.Attribute("CustomProductID").Value
                      || existingProducts[concentratorProductID].ConcentratorProductID != concentratorProductID
                      )
                    {
                      product = existingProducts[concentratorProductID];
                      product.ShortDescription = shortDescription;
                      product.LongDescription = longDescription;
                      product.BrandID = brands[r.Element("Brand").Attribute("BrandID").Value.Trim()].BrandID;
                      product.UnitPrice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                      product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                      product.ProductStatus = r.Attribute("CommercialStatus").Value;
                      product.LastModificationTime = DateTime.Now;
                      product.IsVisible = false;
                      product.TaxRateID = taxRateID.Value;
                      product.LineType = r.Attribute("LineType").Value.Trim();
                      product.CustomProductID = r.Attribute("CustomProductID").Value;
                      //product.ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);
                      log.InfoFormat("updateproduct {0}", r.Attribute("CustomProductID").Value);
                    }
                  }
                  else
                  {
                    log.DebugFormat("Update product failed, brand {0} not availible", r.Attribute("BrandCode").Value.Trim());
                  }
                }
                else
                {
                  decimal unitprice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);

                  if (!brands.ContainsKey(r.Element("Brand").Attribute("BrandID").Value.Trim()))
                  {
                    Brand brand = new Brand();
                    brand.CreationTime = DateTime.Now;
                    brand.LastModificationTime = DateTime.Now;
                    brand.CreatedBy = 0;
                    brand.LastModifiedBy = 0;
                    brand.BrandCode = r.Element("Brand").Attribute("BrandID").Value;
                    brand.BrandName = r.Element("Brand").Element("Name").Value;
                    brand.IsActive = true;
                    context.Brands.InsertOnSubmit(brand);
                    context.SubmitChanges();
                    brands.Add(brand.BrandCode, brand);
                  }

                  if (!this.Connector.UseConcentratorProductID)
                  {
                    //create stub product with identity_insert on

                    string cmd =
                      String.Format(
                        @"
INSERT INTO Products (ProductID, ShortDescription,LongDescription,brandid,taxrateid,iscustom,isvisible,extendedcatalog
	, canmodifyprice,creationtime,lastmodificationtime
	)  VALUES( {0}, '{1}','{2}', {3},{4},0,0,1,0,getdate(), getdate())
",
                        concentratorProductID, shortDescription.Replace("'", "''"), longDescription.Replace("'", "''"), brands[r.Element("Brand").Attribute("BrandID").Value.Trim()].BrandID, taxRateID.Value);

                    context.ExecuteCommand(cmd);

                    product = context.Products.Single(x => x.ProductID == concentratorProductID);
                    product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                    product.UnitPrice = unitprice;
                    product.UnitCost = 0;
                    product.IsCustom = false;
                    product.LineType = "S";
                    product.ProductStatus = r.Attribute("CommercialStatus").Value;
                    product.IsVisible = false;
                    product.CustomProductID = r.Attribute("CustomProductID").Value;
                    //product.ConcentratorProductID = realConcentratorProductID;

                    context.SubmitChanges();

                  }
                  else
                  {

                    product = new Product
                    {
                      ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                      ShortDescription = shortDescription,
                      LongDescription = longDescription,
                      ManufacturerID = r.Attribute("ManufacturerID").Value,
                      BrandID = brands[r.Element("Brand").Attribute("BrandID").Value.Trim()].BrandID,
                      TaxRateID = taxRateID.Value,
                      UnitPrice = unitprice,
                      UnitCost = 0,
                      IsCustom = false,
                      LineType = r.Attribute("LineType").Value.Trim(),
                      ProductStatus = r.Attribute("CommercialStatus").Value,
                      IsVisible = false,
                      CreationTime = DateTime.Now,
                      LastModificationTime = DateTime.Now,
                      //ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value),
                      CustomProductID = r.Attribute("CustomProductID").Value
                    };


                    context.Products.InsertOnSubmit(product);
                  }
                  log.InfoFormat("addedproduct {0}", r.Attribute("CustomProductID").Value);
                  //}
                  //else
                  //{
                  //  if (!string.IsNullOrEmpty(r.Attribute("BrandCode").Value))
                  //    log.DebugFormat("Insert product failed, brand {0} not availible", r.Attribute("BrandCode").Value.Trim());
                  //  else
                  //    log.Debug("Insert product failed");
                  //}
                }
                addedcustomOItemNumbers.Add(concentratorProductID);

                context.SubmitChanges();
              }
            }
            catch (Exception ex)
            {
              log.Error("Error processing product" + concentratorProductID, ex);
            }
          }
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import products", ex);
      }
    }

    public void ImportProductGroups(bool update)
    {
      using (var ctx = new WebsiteDataContext(this.Connector.ConnectionString))
      {
        var productGroups = ctx.ProductGroups.ToList();

        //.Where(x=>x.Attribute("ProductID").Value == "262")
        foreach (var hierarchy in products.Root.Elements("Product").Select(c => c.Element("ProductGroupHierarchy")))
        {
          foreach (var visibleGroup in hierarchy.Elements("ProductGroup"))
          {
            ProcessProductGroupNode(ctx, visibleGroup, productGroups, update);
          }
        }
        ctx.SubmitChanges();
      }
    }

    private ProductGroup ProcessProductGroupNode(WebsiteDataContext ctx, XElement currentNode, List<ProductGroup> productGroups, bool update)
    {
      var nodes = currentNode.Elements("ProductGroup");

      var backendCode = currentNode.Try(c => c.Attribute("ID").Value, string.Empty);
      ProductGroup productGroup = null;
      //depth first
      foreach (var node in nodes)
      {
        var parentProductGroup = ProcessProductGroupNode(ctx, node, productGroups, update);

        var parentBackendCode = node.Try(c => c.Attribute("ID").Value, string.Empty);

        if (!String.IsNullOrEmpty(parentBackendCode))
        {
          productGroup = productGroups.FirstOrDefault(c => c.BackendProductGroupCode == backendCode
                                                           &&
                                                           c.ParentProductGroupID == parentProductGroup.ProductGroupID
                                                           );
        }
        else
        {
          productGroup = productGroups.FirstOrDefault(c => c.BackendProductGroupCode == backendCode
                                                           &&
                                                           !c.ParentProductGroupID.HasValue
            );
        }

        //var parentProductGroup = productGroups.FirstOrDefault(c => c.BackendProductGroupCode == parentBackendCode);
        if (productGroup == null)
        {
          productGroup = new ProductGroup
                           {
                             BackendProductGroupCode = backendCode,
                             ProductGroupCode = string.Empty,
                             ProductGroupName = currentNode.Attribute("Name").Value,
                             OrderIndex = currentNode.Attribute("Index").Try<XAttribute, int?>(c => int.Parse(c.Value), null),
                             ParentProductGroup = parentProductGroup
                           };
          ctx.ProductGroups.InsertOnSubmit(productGroup);
          productGroups.Add(productGroup);
        }
        else
        {
          if (update)
          {
            productGroup.ProductGroupName = currentNode.Attribute("Name").Value;
            productGroup.OrderIndex = currentNode.Attribute("Index").Try<XAttribute, int?>(c => int.Parse(c.Value), null);
          }
        }
        ctx.SubmitChanges();
        return productGroup;
      }

      //self


      if (nodes.Count() == 0)
      {
        //root level
        productGroup = productGroups.FirstOrDefault(c => c.BackendProductGroupCode == backendCode && !c.ParentProductGroupID.HasValue);
        if (productGroup == null)
        {
          productGroup = new ProductGroup
                           {
                             BackendProductGroupCode = backendCode,
                             ProductGroupCode = string.Empty,
                             ProductGroupName = currentNode.Attribute("Name").Value,
                             OrderIndex =
                               currentNode.Attribute("Index").Try<XAttribute, int?>(c => int.Parse(c.Value), null)
                           };

          ctx.ProductGroups.InsertOnSubmit(productGroup);
          productGroups.Add(productGroup);
        }
        else
        {
          if (update)
          {
            productGroup.ProductGroupName = currentNode.Attribute("Name").Value;
            productGroup.OrderIndex = currentNode.Attribute("Index").Try<XAttribute, int?>(c => int.Parse(c.Value), null);
          }
        }
        ctx.SubmitChanges();
        return productGroup;

      }
      return null;



    }


    public void ImportBarcode()
    {
      WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString);

      var ids = (from r in products.Root.Elements("Product")
                 select int.Parse(r.Attribute("ProductID").Value)).Distinct().ToArray();

      var barcodes = (from b in context.ProductBarcodes
                      join p in context.Products on b.ProductID equals p.ProductID
                      where p.ConcentratorProductID.HasValue
                      select new
                      {
                        ConcentratorProductID = p.ConcentratorProductID.Value,
                        Barcode = b
                      }).ToList();

      var webProducts = context.Products.Select(x => new { x.ProductID, x.ConcentratorProductID }).Distinct().ToList();
      List<ProductBarcode> barcodeList = new List<ProductBarcode>();
      List<ProductBarcode> unusedBarcodes = barcodes.Select(x => x.Barcode).ToList();

      foreach (var prod in products.Root.Elements("Product"))
      {
        try
        {
          int concentratorProductid = int.Parse(prod.Attribute("ProductID").Value);

          foreach (var bar in prod.Element("Barcodes").Elements("Barcode"))
          {
            //if (bar.Element("Barcode") != null && !string.IsNullOrEmpty(bar.Element("Barcode").Value))
            //{
            string barcodecode = bar.Value.Trim();
            string barcodetype;
            switch (barcodecode.Length)
            {
              case 8:
                barcodetype = "Ean8";
                break;
              case 12:
                barcodetype = "UpcA";
                break;
              case 13:
                barcodetype = "Ean13";
                break;
              default:
                barcodetype = "Ean13";
                break;
            }

            foreach (var productid in webProducts.Where(x => x.ConcentratorProductID == concentratorProductid).Select(x => x.ProductID))
            {
              var existing = barcodes.Any(x => x.Barcode.BarcodeType.Trim() == barcodetype && x.Barcode.ProductID == productid && x.Barcode.Barcode.Trim() == barcodecode);

              if (!existing)
              {
                if (!barcodeList.Any(x => x.Barcode == barcodecode && x.BarcodeType == barcodetype && x.ProductID == productid) && productid > 0)
                {
                  ProductBarcode barcode = InsertBarcode(productid, barcodecode, barcodetype);
                  context.ProductBarcodes.InsertOnSubmit(barcode);
                  barcodeList.Add(barcode);
                }
              }
              else
              {
                unusedBarcodes.RemoveAll(x => x.BarcodeType.Trim() == barcodetype.Trim() && x.ProductID == productid && x.Barcode.Trim() == barcodecode.Trim());
              }
            }
            //}
            context.SubmitChanges();
          }
        }
        catch (Exception ex)
        {
          log.Error(string.Format("Error import barcode: {0} product: {1}", string.Join(",", prod.Elements("Barcodes").Elements("Barcode").Select(x => x.Value).ToArray()), prod.Attribute("ProductID").Value), ex);
          context = new WebsiteDataContext(Connector.ConnectionString);
        }
      }

      log.DebugFormat("Found {0} barcodes to remove", unusedBarcodes.Count());

      context.ProductBarcodes.DeleteAllOnSubmit(unusedBarcodes);

      context.SubmitChanges();
      context.Dispose();

    }

    private ProductBarcode InsertBarcode(int productID, string barcode, string barcodeType)
    {
      ProductBarcode productBarcode = new ProductBarcode();
      productBarcode.LastModificationTime = DateTime.Now;
      productBarcode.ProductID = productID;
      productBarcode.BarcodeType = barcodeType;
      productBarcode.Barcode = barcode;
      return productBarcode;
    }

    private ProductGroup ParseProductGroups(XElement element, ProductGroup group, Table<ProductGroup> productGroupTable, List<ProductGroup> groupCollection)
    {
      var backendID = element.Attribute("ID").Value;
      var name = element.Attribute("Name").Value;
      int? order = element.Attribute("Index").Try<XAttribute, int?>(c => int.Parse(c.Value), null);


      var productGroup = groupCollection.Where(c => c.BackendProductGroupCode == backendID).FirstOrDefault();
      if (productGroup == null)
      {
        productGroup = new ProductGroup
                         {
                           BackendProductGroupCode = backendID,
                           ProductGroupName = name,
                           ProductGroupCode = string.Empty,
                           OrderIndex = order
                         };
        groupCollection.Add(productGroup);
        productGroupTable.InsertOnSubmit(productGroup);
      }

      group.ParentProductGroup = productGroup;

      if (element.Elements("ProductGroup") != null)
      {
        foreach (var el in element.Elements("ProductGroup"))
        {
          TraverseProductGroupHierarchy(element.Element("ProductGroup"), group.ParentProductGroup);
        }
      }
      return group;
    }

    private ProductGroup TraverseProductGroupHierarchy(XElement element, ProductGroup group)
    {
      group.ParentProductGroup = new ProductGroup
                                   {
                                     BackendProductGroupCode = element.Attribute("ID").Value,
                                     ProductGroupName = element.Attribute("Name").Value,
                                     OrderIndex = element.Attribute("Index").Try<XAttribute, int?>(c => int.Parse(c.Value), null),
                                     ProductGroupCode = string.Empty
                                   };
      if (element.Elements("ProductGroup") != null)
      {
        foreach (var el in element.Elements("ProductGroup"))
        {
          TraverseProductGroupHierarchy(el, group.ParentProductGroup);
        }
      }
      return group;
    }

    #region cleanup
    public void CleanUpProducts()
    {
      log.Info("Start Clean products");

      if (ConcentratorItemNumbers.Count > 0)
      {
        using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
        {
          var visibleproducts = (from p in context.Products
                                 where p.IsVisible && p.ConcentratorProductID.HasValue
                                 select p).ToList();

          int counter = 0;

          foreach (var p in visibleproducts)
          {
            if (!ConcentratorItemNumbers.Contains(p.ConcentratorProductID.Value))
            {
              counter++;
              p.IsVisible = false;
              context.SubmitChanges();
            }
          }
          log.Info("Products to set false " + counter);

        }
      }
      log.Info("Clean Products Finished");
    }

    public void CleanUpProductGroupMapping()
    {
      using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
      {
        log.Info("Start Cleanup For Product Group Mapping");

        context.CommandTimeout = 2222000;

        var productgroupmappings = (from pgm in context.ProductGroupMappings
                                    select pgm).ToList();

        foreach (var pgm in productgroupmappings)
        {
          int productID = pgm.ProductID;
          if (this.Connector.UseConcentratorProductID)
          {
            if (pgm.Product != null)
            {
              if (pgm.Product.ConcentratorProductID.HasValue)
                productID = pgm.Product.ConcentratorProductID.Value;
              else
              {
                log.DebugFormat("Product Missing concentratorproductid, delete productgroupmapping for productID {0}", pgm.ProductID);
                context.ProductGroupMappings.DeleteOnSubmit(pgm);
                context.SubmitChanges();
                continue;
              }
            }
            else
            {
              log.DebugFormat("ProductID {0} does not exists, remove mapping", pgm.ProductID);
              context.ProductGroupMappings.DeleteOnSubmit(pgm);
            }
          }

          if (pgmList.ContainsKey(productID))
          {
            var product = pgmList[productID];
            if (product != null)
            {
              if (!pgmList[productID].Contains(pgm.ProductGroupID))
              {
                context.ProductGroupMappings.DeleteOnSubmit(pgm);
              }
            }
            else
              context.ProductGroupMappings.DeleteOnSubmit(pgm);

            context.SubmitChanges();
          }
        }
        log.Info("Finished Cleanup For Product Group Mapping");
      }
    }

    #endregion

    #region Attributes

    public void processSelectorAttributes(Connector connector)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
      {
        log.DebugFormat("Fetched attributes for {0}", connector.Name);

        int total = products.Root.Elements("Product").Count();
        int totalNumberOfProductsToProcess = total;
        int productcounter = 0;
        int counter = 0;

        List<ProductGroupMapping> ProductGroupMappings = (from pg in context.ProductGroupMappings
                                                          select pg).ToList();

        List<ProductGroup> productgroups = (from pg in context.ProductGroups
                                            select pg).ToList();

        var allproducts = (from p in context.Products
                           where p.ConcentratorProductID.HasValue
                           select p).ToList();

        foreach (var r in products.Root.Elements("Product"))
        {
          int concentratorProductid = int.Parse(r.Attribute("ProductID").Value);

          counter++;
          productcounter++;
          totalNumberOfProductsToProcess--;
          //ManualResetEvent e = new ManualResetEvent(false);

          XmlDocument doc = new XmlDocument();

          var element = r.Element("ProductAttribute");

          if (element == null)
            continue;

          XElement productAttribute = new XElement("ProductAttributes", new XElement(element));

          doc.LoadXml(productAttribute.ToString());

          Product prod = (from p in context.Products
                          where p.ConcentratorProductID.HasValue && p.ConcentratorProductID.Value == concentratorProductid
                          select p).FirstOrDefault();

          if (prod != null)
          {
            StateInfo info = new StateInfo
            {
              ProductID = prod.ProductID,
              //ResetEvent = e,
              ProductGroupMappings = ProductGroupMappings,
              ProductGroups = productgroups,
              Products = allproducts,
              Attributes = XDocument.Parse(doc.OuterXml)
            };

            //manualEvents.Add(e);            

            if (counter >= 250)
            {
              log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, productcounter);
              counter = 0;
            }

            Callback(info);
          }
          else
          {
            log.DebugFormat("Product does not exists {0} concentratorProductID", concentratorProductid);
          }
        }
      }
    }


    #region MultiThread product attribute processing

    private static Mutex mut = new Mutex();
    private static Mutex mut2 = new Mutex();
    private static Mutex mut3 = new Mutex();
    private static Mutex mut4 = new Mutex();

    public void ProcessAttributes(Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap, Connector connector, bool auction, DateTime? lastupdate, bool fullImport)
    {
      log.DebugFormat("Start process attributes for {0}", connector.Name);

      List<ManualResetEvent> manualEvents = new List<ManualResetEvent>();


      ThreadPool.SetMaxThreads(40, 40);

      List<BatchResult> _batchResults = new List<BatchResult>();

      using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
      {
        List<ProductGroupMapping> ProductGroupMappings = (from pg in context.ProductGroupMappings
                                                          select pg).ToList();

        List<ProductGroup> productgroups = (from pg in context.ProductGroups
                                            select pg).ToList();

        var allproducts = (from p in context.Products
                           where p.ConcentratorProductID.HasValue
                           select p).ToList();

        List<int> productList = new List<int>();
        productList = new List<int>(ConcentratorItemNumbers);

        if (productList.Count == 0)
        {
          productList = allproducts.Select(x => x.ConcentratorProductID.Value).ToList();
        }

        if (!fullImport && !lastupdate.HasValue)
        {
          lastupdate = (from lu in context.ProductAttributes
                        select lu.LastModificationTime.HasValue ? lu.LastModificationTime.Value : lu.CreationTime).Max();
          lastupdate.Value.AddDays(-1);
        }

        if (lastupdate.HasValue)
          log.DebugFormat("Update productattributes after {0}", lastupdate.Value.ToString("dd-MM-yyyy HH:mm"));

        XDocument attributes = null;

        if (ConfigurationManager.AppSettings["XMLfile"] != null)
        {
          if (Directory.Exists(ConfigurationManager.AppSettings["XMLfile"].ToString()))
          {
            foreach (string file in Directory.GetFiles(ConfigurationManager.AppSettings["XMLfile"].ToString()))
            {
              FileInfo fileInf = new FileInfo(file);
              if (fileInf.CreationTime > DateTime.Now.AddHours(-1) && file.EndsWith(string.Format("_{0}.xml", connector.ConnectorID)))
              {
                using (StreamReader reader = new StreamReader(file))
                {
                  attributes = XDocument.Load(reader);
                }
              }
            }
          }
        }

#if DEBUG
        if (System.IO.File.Exists(@"c:\attributes.xml"))
          attributes = XDocument.Parse(System.IO.File.ReadAllText(@"c:\attributes.xml"));
        else
        {
#endif

        Concentrator.Web.ServiceClient.AssortmentService.ArrayOfInt array = new Concentrator.Web.ServiceClient.AssortmentService.ArrayOfInt();
        array.AddRange(ConcentratorItemNumbers.Distinct().ToArray());

        if (attributes == null)
            attributes = new XDocument(soap.GetAttributesAssortment(connector.ConnectorID, array, lastupdate));

#if DEBUG
          attributes.Save(@"C:\attributes.xml", SaveOptions.DisableFormatting);
        }

#endif

        if (ConfigurationManager.AppSettings["XMLfile"] != null)
        {
          if (!Directory.Exists(ConfigurationManager.AppSettings["XMLfile"].ToString()))
            Directory.CreateDirectory(ConfigurationManager.AppSettings["XMLfile"].ToString());

          string file = Path.Combine(ConfigurationManager.AppSettings["XMLfile"].ToString(), string.Format("XML_Attributes_{0}_{1}.xml", DateTime.Now.ToString("ddMMyyyyHHmmss"), connector.ConnectorID));

          attributes.Save(file);

        }

        log.DebugFormat("Fetched attributes for {0}", connector.Name);

        int totalNumberOfProductsToProcess = productList.Count();
        int productcounter = 0;
        int counter = 0;

        for (int i = productList.Count - 1; i >= 0; i--)
        {
          counter++;
          productcounter++;
          totalNumberOfProductsToProcess--;
          //ManualResetEvent e = new ManualResetEvent(false);

          XmlDocument doc = new XmlDocument();

          var selector = (from x in attributes.Root.Elements("ProductAttribute")
                          where x.Attribute("ProductID").Value == productList[i].ToString()
                          select x);

          var element = selector.FirstOrDefault();

          if (element == null)
            continue;

          XElement productAttribute = new XElement("ProductAttributes", new XElement(element));

          int concentratorProductid = int.Parse(productAttribute.Element("ProductAttribute").Attribute("ProductID").Value);
          //productList.Remove(concentratorProductid);

          doc.LoadXml(productAttribute.ToString());

          Product prod = (from p in allproducts
                          where p.ConcentratorProductID.HasValue && p.ConcentratorProductID.Value == concentratorProductid
                          select p).FirstOrDefault();

          if (prod != null)
          {
            StateInfo info = new StateInfo
            {
              ProductID = prod.ProductID,
              //ResetEvent = e,
              soap = soap,
              ProductGroupMappings = ProductGroupMappings,
              ProductGroups = productgroups,
              Products = allproducts,
              Attributes = XDocument.Parse(doc.OuterXml)
            };

            //manualEvents.Add(e);            

            if (counter >= 250)
            {
              log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, productList.Count(), productcounter);
              counter = 0;
            }

            Callback(info);
          }
          else
          {
            log.DebugFormat("Product does not exists {0} concentratorProductID", concentratorProductid);
          }

          //ThreadPool.QueueUserWorkItem(new WaitCallback(Callback), info);

          //if (productList.Count <= 0 || manualEvents.Count > 39)
          //{
          //  DateTime startTime = DateTime.Now;

          //  int numberOfProducts = manualEvents.Count;
          //  log.Info("=================================================");
          //  log.InfoFormat("Start processing product specs, batch of {0} products.", numberOfProducts);
          //  WaitHandle.WaitAll(manualEvents.ToArray());
          //  double executionTime = (DateTime.Now - startTime).TotalSeconds;
          //  log.InfoFormat("Processed {0} products in : {1} seconds", numberOfProducts, executionTime);
          //  _batchResults.Add(new BatchResult(numberOfProducts, executionTime));

          //  double averageSecondsPerProduct = _batchResults.Average(b => b.ExecutionTimeInSeconds / b.NumberOfProducts);
          //  double secondsNeeded = productList.Count * averageSecondsPerProduct;

          //  log.InfoFormat("Still need to process {0} of {1}; {2} done; Estimated completiontime: {3}", productList.Count, totalNumberOfProductsToProcess, totalNumberOfProductsToProcess - productList.Count, DateTime.Now.AddSeconds(secondsNeeded));
          //  log.Info("=================================================");
          //  manualEvents.Clear();
          //}
        }
      }

    }

    private void Callback(object state)
    {
      StateInfo info = state as StateInfo;
      try
      {
        //XDocument attributes = XDocument.Parse(info.soap.GetAttributesAssortment(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), info.ProductID).OuterXml);
        XDocument attributes = info.Attributes;

        //mut.WaitOne();
        ImportAttributeParents(attributes, info.ProductGroups, info.ProductGroupMappings, info.ProductID);
        //mut.ReleaseMutex();

        // mut2.WaitOne();
        ImportProductAttributes(attributes);
        //mut2.ReleaseMutex();

        // mut3.WaitOne();
        ImportProductGroupsAttributes(attributes, info.ProductGroups, info.ProductGroupMappings, info.ProductID);
        // mut3.ReleaseMutex();

        //mut4.WaitOne();
        ImportAttributeValues(info.ProductID, attributes, info.Products, info.ProductGroups, info.ProductGroupMappings);
        // mut4.ReleaseMutex();
      }
      catch (Exception ex)
      {
        log.Error("Error importing product attribute for" + info.ProductID, ex);
      }
      finally
      {
        //info.ResetEvent.Set();
      }
    }

    public void ProcessContentDescriptions(Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap, Connector connector)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(connector.ConnectionString))
      {
        var contentDescriptions = new XDocument(soap.GetAssortmentContentDescriptions(connector.ConnectorID, null));

        var products = (from p in context.Products
                        where p.ConcentratorProductID.HasValue
                        select p).ToList();

        foreach (var r in contentDescriptions.Root.Elements("Product"))
        {
          int realConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);

          var product = products.Where(x => x.ConcentratorProductID == realConcentratorProductID).FirstOrDefault();

          if (product != null)
          {
            if (!string.IsNullOrEmpty(r.Element("Content").Attribute("LongContentDescription").Value))
              product.LongDescription = r.Element("Content").Attribute("LongContentDescription").Value;
            else
              product.LongDescription = r.Element("Content").Attribute("LongDescription").Value;

            context.SubmitChanges();
          }
        }
      }
    }

    public class StateInfo
    {
      public Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap { get; set; }
      public int ProductID { get; set; }
      public string CustomItemNumber { get; set; }
      public ManualResetEvent ResetEvent { get; set; }
      public List<ProductGroupMapping> ProductGroupMappings { get; set; }
      public List<ProductGroup> ProductGroups { get; set; }
      public List<Product> Products { get; set; }
      public XDocument Attributes { get; set; }
    }

    public struct BatchResult
    {

      public BatchResult(int numberOfProducts, double executionTimeInSeconds)
      {
        NumberOfProducts = numberOfProducts;
        ExecutionTimeInSeconds = executionTimeInSeconds;
      }

      public int NumberOfProducts;
      public double ExecutionTimeInSeconds;

    }

    #endregion MultiThread product attribute processing

    private void ImportAttributeParents(XDocument attributes, List<ProductGroup> productGroups, List<ProductGroupMapping> productGroupMappings, int productID)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
      {
        List<AttributeGroupMetaData> createdAttributes = new List<AttributeGroupMetaData>();

        var attributeGroupList = (from r in attributes.Root.Elements("ProductAttribute").Elements("AttributeGroups").Elements("AttributeGroup")
                                  //let parentproductgroupid = (from p in productGroups
                                  //                            where !p.ParentProductGroupID.HasValue
                                  //                            && p.BackendProductGroupCode == r.Ancestors("ProductAttribute").First().Attribute("ProductGroupID").Value
                                  //                            select p.ProductGroupID).FirstOrDefault()
                                  let productgroup = (from p in productGroupMappings
                                                      where p.ProductID == productID
                                                      select p.ProductGroupID).FirstOrDefault()
                                  select new AttributeGroupMetaData
                                  {
                                    AttributeGroupMetaDataID = int.Parse(r.Attribute("AttributeGroupID").Value),
                                    AttributeGroupName = r.Attribute("Name").Value,
                                    AttributeGroupIndex = int.Parse(r.Attribute("AttributeGroupIndex").Value),
                                    ProductGroupID = productgroup
                                  }).Distinct().ToList();

        foreach (AttributeGroupMetaData at in attributeGroupList)
        {
          var existingAttributes = (from a in context.AttributeGroupMetaDatas
                                    where a.ProductGroupID == at.ProductGroupID
                                      && a.AttributeGroupMetaDataID == at.AttributeGroupMetaDataID
                                    select a).ToList();

          if (!createdAttributes.Contains(at)
            && existingAttributes.Count() < 1)
          {
            at.LastModificationTime = DateTime.Now;
            at.AttributeGroupIndex = at.AttributeGroupIndex;
            context.AttributeGroupMetaDatas.InsertOnSubmit(at);
            createdAttributes.Add(at);
          }
          else
          {
            AttributeGroupMetaData existat = existingAttributes.FirstOrDefault();
            if (existat != null)
            {
              if (existat.AttributeGroupName != at.AttributeGroupName
                || existat.AttributeGroupIndex != at.AttributeGroupIndex
                || existat.ProductGroupID != at.ProductGroupID)
              {
                //map existing attribute...
                existat.AttributeGroupName = at.AttributeGroupName;
                existat.AttributeGroupIndex = at.AttributeGroupIndex;
                existat.LastModificationTime = DateTime.Now;
                existat.ProductGroupID = at.ProductGroupID;
              }
            }
          }
          //context.SubmitChanges();
        }
        context.SubmitChanges();
      }

    }

    private void ImportAttributeValues(int productID, XDocument attributes, List<Product> products, List<ProductGroup> productGroups, List<ProductGroupMapping> productGroupMappings)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
      {
        var existingProducts = (
            from p in products
            where p.ProductID == productID
            select p.ProductID).ToList();

        List<int> createdAttributeValues = new List<int>();

        var attributList = (from r in attributes.Root.Elements("ProductAttribute").Elements("Attributes").Elements("Attribute")
                            //let groupID = r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                            //let parentproductgroupid = (from p in productGroups
                            //                            where !p.ParentProductGroupID.HasValue
                            //                            && p.BackendProductGroupCode == r.Ancestors("ProductAttribute").First().Attribute("ProductGroupID").Value
                            //                            select p.ProductGroupID).FirstOrDefault()
                            let productgroup = (from p in productGroupMappings
                                                where p.ProductID == productID
                                                select p.ProductGroupID).FirstOrDefault()
                            select new
                            {
                              AttributeID = int.Parse(r.Attribute("AttributeID").Value),
                              Name = r.Element("Name").Value,
                              //GroupID = groupID,
                              ProductID = productID,
                              Value = r.Element("Value").Value,
                              ProductGroupID = productgroup,
                              AttributeGroupID = int.Parse(r.Attribute("AttributeGroupID").Value)
                            }).Distinct().ToList();

        Dictionary<int, int> pgList = new Dictionary<int, int>();
        List<ProductAttribute> existingProductAttributes = new List<ProductAttribute>();

        foreach (var att in attributList)
        {
          var attmd = (from a in context.ProductAttributes
                       where //a.ProductGroupID == att.ProductGroupID &&
                       a.ProductID == att.ProductID
                       && a.AttributeMetaDataID == att.AttributeID
                       select a).FirstOrDefault();

          if (attmd != null)
            existingProductAttributes.Add(attmd);
        }

        List<ProductAttribute> addedList = new List<ProductAttribute>();

        if (attributList.Count > 0)
        {
          foreach (var att in attributList)
          {

            var added = (from a in addedList
                         where a.AttributeMetaDataID == att.AttributeID
                         && a.ProductID == att.ProductID
                         //&& a.ProductGroupID == att.ProductGroupID
                         select a).FirstOrDefault();

            if (added == null)
            {

              if (!existingProducts.Contains(att.ProductID))
              {
                log.Error(string.Format("Product {0} not found in database.", att.ProductID));
                continue;
              }

              if (context.AttributeMetaDatas.Where(a => a.AttributeMetaDataID == att.AttributeID).Count() < 1)
              {
                log.Error(string.Format("Attribute{0} not found in database", att.AttributeID));
                continue;
              }

              ProductAttribute val = existingProductAttributes.Where(x => x.AttributeMetaDataID == att.AttributeID
                && x.ProductID == att.ProductID).FirstOrDefault();
              //&& x.ProductGroupID == att.ProductGroupID).FirstOrDefault();
              //if the product exists (in the db) and the value does not, and we didn't create it before, then go ahead, make me a new Attribute
              if (val == null)
              //&& existingAttributeValues.Where(attr => attr.Equals(new int[] { att.AttributeID, att.ProductID })).FirstOrDefault() == null
              //&& createdAttributeValues.Where(attr => attr.Equals(new int[] { att.AttributeID, att.ProductID })).FirstOrDefault() == null)
              {
                val = new ProductAttribute();
                val.AttributeMetaDataID = att.AttributeID;
                val.ProductID = att.ProductID;
                val.ProductGroupID = att.ProductGroupID;
                val.AttributeValue = att.Value.Length > 255 ? att.Value.Substring(0, 255) : att.Value;//truncating text to 50 chars
                val.attributegroupmetadataid = att.AttributeGroupID;
                val.CreationTime = DateTime.Now;
                context.ProductAttributes.InsertOnSubmit(val);

                //mark as created
                createdAttributeValues.Add(att.AttributeID);
                addedList.Add(val);
              }//so if we didn't create it, but the product and  value are present in the db, lets update it.
              //else if (createdAttributeValues.Where(attr => attr.Equals(new int[] { att.AttributeID, att.ProductID })).FirstOrDefault() != null
              //  && existingAttributeValues.Where(attr => attr.Equals(new int[] { att.AttributeID, att.ProductID })).FirstOrDefault() != null
              //  && existingProducts.Contains(att.ProductID) && existingAttributes.ContainsKey(att.AttributeID))
              else if (!createdAttributeValues.Contains(att.AttributeID))
              {
                ProductAttribute existatval = val;
                if (existatval.AttributeValue != (att.Value.Length > 255 ? att.Value.Substring(0, 255) : att.Value)
                  || existatval.attributegroupmetadataid != att.AttributeGroupID
                  )
                {
                  existatval.AttributeValue = att.Value.Length > 255 ? att.Value.Substring(0, 255) : att.Value;//truncating text to 50 chars
                  existatval.LastModificationTime = DateTime.Now;
                  existatval.attributegroupmetadataid = att.AttributeGroupID;
                }
              }
            }
            // context.SubmitChanges();
          }
          context.SubmitChanges();
        }
      }
    }

    private void ImportProductGroupsAttributes(XDocument attributes, List<ProductGroup> productGroups, List<ProductGroupMapping> productGroupMappings, int productID)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
      {
        Dictionary<string, List<int>> createdAttributes = new Dictionary<string, List<int>>();

        var attributList = (from r in attributes.Root.Elements("ProductAttribute").Elements("Attributes").Elements("Attribute")
                            //let parentproductgroupid = (from p in productGroups
                            //                            where !p.ParentProductGroupID.HasValue
                            //                            && p.BackendProductGroupCode == r.Ancestors("ProductAttribute").First().Attribute("ProductGroupID").Value
                            //                            select p.ProductGroupID).FirstOrDefault()
                            let productgroup = (from p in productGroupMappings
                                                where //p.ProductGroup.BackendProductGroupCode == r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                                                  //&& p.ProductGroup.ParentProductGroupID == parentproductgroupid && 
                                                p.ProductID == productID
                                                select p.ProductGroupID).ToList()
                            select new
                            {
                              AttributeMetaDataID = int.Parse(r.Attribute("AttributeID").Value),
                              KeyFeature = (bool)r.Attribute("KeyFeature"),
                              Index = r.Attribute("Index").Value,
                              IsSearchable = (bool)r.Attribute("IsSearchable"),
                              ProductGroups = productgroup//,
                              //BackendProductID = r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                            }).Distinct().ToList();

        Dictionary<int, int> pgList = new Dictionary<int, int>();
        List<ProductGroupsAttributeMetaData> existingAttributes = new List<ProductGroupsAttributeMetaData>();

        foreach (var att in attributList)
        {
          var attmd = (from a in context.ProductGroupsAttributeMetaDatas
                       where att.ProductGroups.Contains(a.ProductGroupID)
                       && a.AttributeMetaDataID == att.AttributeMetaDataID
                       select a).ToList();

          if (attmd.Count > 0)
            existingAttributes.AddRange(attmd);
        }

        List<ProductGroupsAttributeMetaData> addedList = new List<ProductGroupsAttributeMetaData>();

        if (attributList.Count() > 0)
        {
          foreach (var at in attributList)
          {
            try
            {
              foreach (var productGroupID in at.ProductGroups)
              {
                ProductGroupsAttributeMetaData pgamd = existingAttributes.Where(x => x.AttributeMetaDataID == at.AttributeMetaDataID
                  && x.ProductGroupID == productGroupID).FirstOrDefault();

                var added = (from a in addedList
                             where a.ProductGroupID == productGroupID
                             && a.AttributeMetaDataID == at.AttributeMetaDataID
                             select a).ToList();

                if (added.Count < 1)
                {
                  if (pgamd == null)
                  {
                    pgamd = new ProductGroupsAttributeMetaData();
                    pgamd.AttributeMetaDataID = at.AttributeMetaDataID;
                    pgamd.IsQuickSearch = at.IsSearchable;
                    pgamd.IsRequired = at.KeyFeature;
                    pgamd.OrderIndex = int.Parse(at.Index);
                    pgamd.ProductGroupID = productGroupID;
                    pgamd.LastModificationTime = DateTime.Now;

                    context.ProductGroupsAttributeMetaDatas.InsertOnSubmit(pgamd);
                    addedList.Add(pgamd);
                    //createdAttributes.Add(at.AttributeMetaDataID);
                  }
                  else
                  //if (existingAttributes.ContainsKey(at.AttributeMetaDataID)
                  //&& existingAttributes[at.AttributeMetaDataID].ProductGroup.BackendProductGroupCode == at.BackendProductGroupCode)
                  {
                    if (pgamd.IsQuickSearch != at.IsSearchable
                      || pgamd.IsRequired != at.KeyFeature
                      || pgamd.OrderIndex != int.Parse(at.Index))
                    {
                      pgamd.IsQuickSearch = at.IsSearchable;
                      pgamd.IsRequired = at.KeyFeature;
                      pgamd.OrderIndex = int.Parse(at.Index);
                      pgamd.LastModificationTime = DateTime.Now;
                    }
                  }
                }
                //context.SubmitChanges();
              }
              //else
              //{
              //  log.InfoFormat("ProductGroup with backendproductgroupcode {0} does not exists", at.BackendProductID);
              //}

              context.SubmitChanges();
            }
            catch (Exception ex)
            {
              log.Error(ex);
            }
          }
        }
      }
    }

    private void ImportProductAttributes(XDocument attributes)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(this.Connector.ConnectionString))
      {
        List<AttributeMetaData> createdAttributes = new List<AttributeMetaData>();

        var attributList = (from r in attributes.Root.Elements("ProductAttribute").Elements("Attributes").Elements("Attribute")
                            //let productgroup = r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                            select new AttributeMetaData
                            {
                              AttributeMetaDataID = int.Parse(r.Attribute("AttributeID").Value),
                              AttributeName = r.Element("Name").Value,
                              FormatString = r.Element("Sign").Value,
                              AttributeGroupMetaDataID = int.Parse(r.Attribute("AttributeGroupID").Value)
                            }).Distinct().ToList();

        if (attributList.Count > 0)
        {
          foreach (AttributeMetaData at in attributList)
          {
            List<AttributeMetaData> existingAttributes = (from a in context.AttributeMetaDatas
                                                          where a.AttributeGroupMetaDataID == at.AttributeGroupMetaDataID
                                                            && a.AttributeMetaDataID == at.AttributeMetaDataID
                                                          select a).ToList();

            if (createdAttributes.Where(x => x.AttributeGroupMetaDataID == at.AttributeGroupMetaDataID
                && x.AttributeMetaDataID == at.AttributeMetaDataID).Count() < 1
              && existingAttributes.Count() < 1)
            {
              at.LastModificationTime = DateTime.Now;
              at.DataTypeID = 1;
              context.AttributeMetaDatas.InsertOnSubmit(at);
              createdAttributes.Add(at);
            }
            else
            {
              AttributeMetaData existat = existingAttributes.FirstOrDefault();
              if (existat != null)
              {
                if (existat.AttributeName != at.AttributeName
                || existat.FormatString != at.FormatString
                || existat.AttributeMetaDataID != at.AttributeMetaDataID
                || existat.AttributeGroupMetaDataID != at.AttributeGroupMetaDataID)
                {
                  //map existing attribute...
                  existat.AttributeName = at.AttributeName;
                  existat.DataTypeID = 1;
                  existat.FormatString = at.FormatString;
                  existat.AttributeMetaDataID = at.AttributeMetaDataID;
                  existat.LastModificationTime = DateTime.Now;
                  existat.AttributeGroupMetaDataID = at.AttributeGroupMetaDataID;
                }
              }
            }
            //context.SubmitChanges();
          }
          context.SubmitChanges();
        }
      }
    }
    #endregion
  }
}
