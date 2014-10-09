using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Configuration;
using System.Data.Linq;
using System.Globalization;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport.Shop
{
  [ConnectorSystem(1)]
  public class ShopImportProduct : ConcentratorPlugin
  {
    private XDocument products;
    private Dictionary<string, int> customerItemNumbers = new Dictionary<string, int>();
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();

    public override string Name
    {
      get { return "MyCom Shop Product Export Plugin"; }
    }

    protected override void Process()
    {

      foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.ShopAssortment)))
      {

        log.DebugFormat("Start Process shop import for {0}", connector.Name);


        try
        {

          DateTime start = DateTime.Now;
          log.InfoFormat("Start process products:{0}", start);
          using (Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient())
          {

            products = new XDocument(soap.GetAssortmentContent(connector.ConnectorID, false, true, null, true));


            log.Info("Start import brands");
            ImportBrands(connector);
            log.Info("Finish import brands");
            log.Info("Start import productgroups");
            ImportProductGroups(connector);
            log.Info("Finish import productgroups");

            log.Info("Start import taxrates");
            ImportTaxRates(connector);
            log.Info("Finish import taxrates");

            log.Info("Start import Products");
            ImportProducts(connector);
            log.Info("Finish import Products");

            CleanUpProducts(customerItemNumbers.Keys, connector);

            log.Info("Start barcode import");
            ImportBarcode(connector);
            log.Info("Finish barcode import");

            log.Info("Start import Stock");
            ShopUtility util = new ShopUtility();
            util.ProcessStock(connector, log, products);
            log.Info("Finish import Stock");

          }
        }
        catch (Exception ex)
        {
          log.Error("Error import shop ProcessProducts", ex);
        }

        log.DebugFormat("Finish Process shop import for {0}", connector.Name);

      }
    }


    private void ImportTaxRates(Connector connector)
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
        {
          var ids = (from r in products.Root.Elements("Product")
                     where r.Element("Price") != null
                     select r.Element("Price").Attribute("TaxRate").Value).Distinct().ToArray();

          List<TaxRate> existingTaxRates = new List<TaxRate>();

          foreach (string id in ids)
          {
            var tx = (from c in context.TaxRates
                      where c.TaxRate1.HasValue && c.TaxRate1.Value == (decimal.Parse(id, CultureInfo.InvariantCulture) / 100)
                      select c).SingleOrDefault();

            if (tx == null)
            {
              TaxRate rate = new TaxRate();
              rate.TaxRateID = (from t in context.TaxRates select t.TaxRateID).Max() + 1;
              rate.TaxRate1 = (decimal.Parse(id, CultureInfo.InvariantCulture) / 100);
              rate.Description = string.Format("{0} % tax", (decimal.Parse(id, CultureInfo.InvariantCulture) / 100));
              context.TaxRates.InsertOnSubmit(rate);
              context.SubmitChanges();
            }
          }
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import taxrates", ex);
      }
    }

    private void ImportOItems(XDocument o_Products, Connector connector)
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
        {
          var ids = (from r in o_Products.Root.Elements("Product")
                     select r.Attribute("CustomProductID").Value).Distinct().ToArray();

          var productList = (from p in context.Products
                             select p).ToList();

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();

          foreach (string id in ids)
          {
            int prodid = 0;
            if (!int.TryParse(id, out prodid))
            {
              log.InfoFormat("Could not process product {0}", id);
              continue;
            }

            var pr = (from c in productList
                      where c.ProductID == prodid
                      select c).SingleOrDefault();

            if (pr != null && !existingProducts.ContainsKey(pr.ProductID))
              existingProducts.Add(pr.ProductID, pr);
          }

          var brands = (from b in context.Brands
                        select b).ToDictionary(x => x.BrandCode.Trim(), x => x);

          var productgroups = (from p in context.ProductGroups
                               select p).ToList();

          List<int> addedcustomOItemNumbers = new List<int>();

          foreach (var r in o_Products.Root.Elements("Product"))
          {
            Product product = null;
            string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
            string longDescription = r.Element("Content").Attribute("LongDescription").Value;

            int customproductID = -1;
            if (int.TryParse(r.Attribute("CustomProductID").Value, out customproductID))
            {

              try
              {
                if (!addedcustomOItemNumbers.Contains(customproductID))
                {
                  int? taxRateID = (from t in context.TaxRates
                                    where t.TaxRate1.HasValue
                                    && (t.TaxRate1.Value * 100) == decimal.Parse(r.Element("Price").Attribute("TaxRate").Value, CultureInfo.InvariantCulture)
                                    select t.TaxRateID).SingleOrDefault();

                  if (!taxRateID.HasValue)
                    taxRateID = 1;


                  int? productgroupid = (from p in productgroups
                                         where p.ProductGroupCode == r.Attribute("ProductGroupID").Value
                                         || p.ProductGroupCode == r.Attribute("ProductSubGroup").Value
                                         select p.ProductGroupID).FirstOrDefault();

                  if (productgroupid.Value == 0)
                  {
                    //int? parentProductGroupID = (from p in productgroups
                    //                             where p.BackendProductGroupCode == r.Attribute("ProductGroupID").Value
                    //                             select p.ParentProductGroupID).FirstOrDefault();

                    //if (!parentProductGroupID.HasValue && parentProductGroupID.Value > 0)
                    //{
                    int? parentProductGroupID = (from p in productgroups
                                                 where p.ProductGroupName == "Missing category"
                                                 select p.ProductGroupID).FirstOrDefault();

                    if (parentProductGroupID.Value == 0)
                    {
                      ProductGroup ppg = new ProductGroup
                      {
                        ProductGroupCode = "n.v.t.",
                        BackendProductGroupCode = "n.v.t.",
                        ProductGroupName = "Missing category",
                        CreationTime = DateTime.Now,
                        LastModificationTime = DateTime.Now
                      };
                      context.ProductGroups.InsertOnSubmit(ppg);
                      context.SubmitChanges();
                      parentProductGroupID = ppg.ProductGroupID;
                      productgroups = (from p in context.ProductGroups
                                       select p).ToList();
                    }
                    //}

                    ProductGroup pg = new ProductGroup
                    {
                      ProductGroupCode = r.Attribute("ProductGroupID").Value,
                      BackendProductGroupCode = r.Attribute("ProductSubGroup").Value,
                      ParentProductGroupID = parentProductGroupID.Value,
                      ProductGroupName = r.Attribute("ProductGroupName").Value,
                      CreationTime = DateTime.Now,
                      LastModificationTime = DateTime.Now
                    };
                    context.ProductGroups.InsertOnSubmit(pg);
                    context.SubmitChanges();
                    productgroupid = pg.ProductGroupID;
                    productgroups = (from p in context.ProductGroups
                                     select p).ToList();
                  }

                  int brandID = 0;

                  if (!brands.ContainsKey(r.Attribute("BrandCode").Value.Trim()))
                  {
                    Brand br = new Brand
                    {
                      BrandCode = r.Attribute("BrandCode").Value.Trim(),
                      BrandName = r.Attribute("BrandCode").Value.Trim(),
                      CreatedBy = 0,
                      CreationTime = DateTime.Now,
                      LastModifiedBy = 0,
                      LastModificationTime = DateTime.Now
                    };
                    context.Brands.InsertOnSubmit(br);
                    context.SubmitChanges();
                    brandID = br.BrandID;
                    brands = (from b in context.Brands
                              select b).ToDictionary(x => x.BrandCode.Trim(), x => x);

                  }
                  else
                  {
                    brandID = brands[r.Attribute("BrandCode").Value.Trim()].BrandID;
                  }

                  bool extended = false;
                  bool.TryParse(r.Element("ShopInformation").Element("ExtendedCatalog").Value, out extended);

                  if (existingProducts.ContainsKey(customproductID))
                  {
                    if (
                      existingProducts[customproductID].ShortDescription != shortDescription
                      || existingProducts[customproductID].LongDescription != longDescription
                      || existingProducts[customproductID].UnitPrice != decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture)
                      || existingProducts[customproductID].ProductStatus != r.Attribute("CommercialStatus").Value
                      || existingProducts[customproductID].ManufacturerID != r.Attribute("ManufacturerID").Value
                      || existingProducts[customproductID].IsVisible != false
                      || existingProducts[customproductID].BrandID != brandID
                      || existingProducts[customproductID].TaxRateID != taxRateID.Value
                      || existingProducts[customproductID].LineType != r.Attribute("LineType").Value.Trim()
                      || existingProducts[customproductID].ProductGroupID != productgroupid.Value
                      || existingProducts[customproductID].LedgerClass != r.Element("ShopInformation").Element("LedgerClass").Value.Trim()
                      || existingProducts[customproductID].ProductDesk != r.Element("ShopInformation").Element("ProductDesk").Value
                      || existingProducts[customproductID].ExtendedCatalog != extended
                      || existingProducts[customproductID].UnitCost != decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture))
                    {
                      product = existingProducts[int.Parse(r.Attribute("CustomProductID").Value)];
                      product.ShortDescription = shortDescription;
                      product.LongDescription = longDescription;
                      product.BrandID = brandID;
                      product.UnitPrice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                      product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                      product.ProductStatus = r.Attribute("CommercialStatus").Value;
                      product.LastModificationTime = DateTime.Now;
                      product.IsVisible = false;
                      product.TaxRateID = taxRateID.Value;
                      product.LineType = r.Attribute("LineType").Value.Trim();
                      product.ProductGroupID = productgroupid.Value;
                      product.LedgerClass = r.Element("ShopInformation").Element("LedgerClass").Value.Trim();
                      product.ProductDesk = r.Element("ShopInformation").Element("ProductDesk").Value;
                      product.ExtendedCatalog = extended;
                      product.UnitCost = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);

                      log.InfoFormat("updateproduct {0}", customproductID);
                    }
                  }
                  else
                  {
                    decimal unitprice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                    decimal costprice = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);

                    product = new Product
                    {
                      ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                      ProductGroupID = productgroupid.Value,
                      ShortDescription = shortDescription,
                      LongDescription = longDescription,
                      ManufacturerID = r.Attribute("ManufacturerID").Value,
                      BrandID = brandID,
                      TaxRateID = taxRateID.Value,
                      UnitPrice = unitprice,
                      UnitCost = costprice,
                      IsCustom = false,
                      LineType = r.Attribute("LineType").Value.Trim(),
                      ProductStatus = r.Attribute("CommercialStatus").Value,
                      IsVisible = false,
                      CreationTime = DateTime.Now,
                      LastModificationTime = DateTime.Now,
                      LedgerClass = r.Element("ShopInformation").Element("LedgerClass").Value.Trim(),
                      ProductDesk = r.Element("ShopInformation").Element("ProductDesk").Value,
                      ExtendedCatalog = extended

                    };

                    context.Products.InsertOnSubmit(product);
                    log.InfoFormat("addedproduct {0}", customproductID);
                    //}
                    //else
                    //{
                    //  if (!string.IsNullOrEmpty(r.Attribute("BrandCode").Value))
                    //  {
                    //    log.DebugFormat("Insert product failed, brand {0} not availible", r.Attribute("BrandCode").Value.Trim());
                    //  }
                    //  else
                    //    log.Debug("Insert product failed");
                    //}
                  }
                  addedcustomOItemNumbers.Add(customproductID);

                  context.SubmitChanges();
                }
              }
              catch (Exception ex)
              {
                log.Error("Error processing product" + customproductID, ex);
              }
            }
            else
            {
              log.InfoFormat("Import failed for product {0}", r.Attribute("CustomProductID").Value);
            }
          }
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import O products", ex);
      }
    }

    private void CleanUpProducts(Dictionary<string, int>.KeyCollection customerItemNumbers, Connector connector)
    {
      log.Info("Start Clean products");
      using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
      {
        var visibleproducts = (from p in context.Products
                               where p.IsVisible == true
                               select p).ToList();

        int counter = 0;

        foreach (var p in visibleproducts)
        {
          if (!customerItemNumbers.Contains(p.ProductID.ToString()))
          {
            counter++;
            p.IsVisible = false;
          }
        }
        log.Info("Products to set false " + counter);
        context.SubmitChanges();
      }
      log.Info("Clean Products Finished");
    }

    private void CleanUpProducts(Connector connector)
    {
      log.Info("Start Clean products");
      using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
      {
        var oldproducts = (from p in context.Products
                           where !customerItemNumbers.ContainsKey(p.ProductID.ToString())
                           && p.IsVisible == true
                           select p).ToList();
        foreach (var p in oldproducts)
        {
          p.IsVisible = false;
        }
        context.SubmitChanges();
      }
      log.Info("Clean Products Finished");
    }

    private void ImportProductGroups(Connector connector)
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
        {
          var productGroups = (from pg in context.ProductGroups
                               select pg).ToList();

          var parids = (from r in products.Root.Elements("Product").Elements("ProductGroupHierarchy").Elements("ProductGroup").Elements("ProductGroup")
                        select new
                        {
                          productgroupid = r.Attribute("ID").Value,
                          name = r.Attribute("Name").Value,
                          score = r.Attribute("Index").Value
                        }
                    ).Distinct().ToArray();

          var subids = (from r in products.Root.Elements("Product").Elements("ProductGroupHierarchy").Elements("ProductGroup")
                        select new
                        {
                          subgroupid = r.Attribute("ID").Value,
                          productgroupid = r.Element("ProductGroup") != null ? r.Element("ProductGroup").Attribute("ID").Value : parids.FirstOrDefault().productgroupid,
                          name = r.Attribute("Name").Value,
                          score = r.Attribute("Index").Value
                        }
                    ).Distinct().ToArray();

          Dictionary<string, List<string>> dic = new Dictionary<string, List<string>>();

          foreach (var id in subids)
          {
            if (dic.ContainsKey(id.productgroupid))
            {
              if (!dic[id.productgroupid].Contains(id.subgroupid))
                dic[id.productgroupid].Add(id.subgroupid);
            }
            else
            {
              List<string> list = new List<string>();
              list.Add(id.subgroupid);
              dic.Add(id.productgroupid, list);
            }
          }

          List<ProductGroup> ExParents = new List<ProductGroup>();
          Dictionary<int, List<string>> NewSub = new Dictionary<int, List<string>>();
          Dictionary<string, List<ProductGroup>> ExSub = new Dictionary<string, List<ProductGroup>>();
          Dictionary<string, List<string>> NewParSub = new Dictionary<string, List<string>>();

          foreach (string prodid in dic.Keys)
          {
            var parent = (from pg in productGroups
                          where !pg.ParentProductGroupID.HasValue
                          && pg.ProductGroupCode == prodid
                          select pg).FirstOrDefault();

            if (parent != null)
            {
              ExParents.Add(parent);

              foreach (string subProdid in dic[prodid])
              {
                var productgroup = (from pg in productGroups
                                    where pg.ParentProductGroupID.HasValue &&
                                    pg.ParentProductGroupID.Value == parent.ProductGroupID
                                    && pg.ProductGroupCode == subProdid
                                    select pg).FirstOrDefault();

                if (productgroup != null)
                {
                  if (ExSub.ContainsKey(prodid))
                    ExSub[prodid].Add(productgroup);
                  else
                  {
                    List<ProductGroup> list = new List<ProductGroup>();
                    list.Add(productgroup);
                    ExSub.Add(prodid, list);
                  }
                }
                else
                {
                  if (NewSub.ContainsKey(parent.ProductGroupID))
                    NewSub[parent.ProductGroupID].Add(subProdid);
                  else
                  {
                    List<string> list = new List<string>();
                    list.Add(subProdid);
                    NewSub.Add(parent.ProductGroupID, list);
                  }
                }
              }
            }
            else
            {
              foreach (string subProdid in dic[prodid])
              {
                if (NewParSub.ContainsKey(prodid))
                  NewParSub[prodid].Add(subProdid);
                else
                {
                  List<string> list = new List<string>();
                  list.Add(subProdid);
                  NewParSub.Add(prodid, list);
                }
              }
            }
          }

          #region NEW PAR + SUB Productgroups
          foreach (string par in NewParSub.Keys)
          {
            var pars = (from pg in parids
                        where pg.productgroupid == par
                        select pg).FirstOrDefault();

            if (pars != null)
            {
              ProductGroup parGroup = new ProductGroup
              {
                ProductGroupCode = pars.productgroupid,
                BackendProductGroupCode = string.Empty,
                ProductGroupName = pars.name,
                LastModificationTime = DateTime.Now,
                CreationTime = DateTime.Now
                //OrderIndex = int.Parse(pars.score)
              };
              context.ProductGroups.InsertOnSubmit(parGroup);
              context.SubmitChanges();

              foreach (string sub in NewParSub[par])
              {
                try
                {
                  var subs = (from pg in subids
                              where pg.subgroupid == sub
                              && pg.productgroupid == par
                              select pg).FirstOrDefault();

                  ProductGroup subGroup = new ProductGroup
                  {
                    ProductGroupCode = sub,
                    BackendProductGroupCode = string.Empty,
                    ProductGroupName = subs.name,
                    LastModificationTime = DateTime.Now,
                    CreationTime = DateTime.Now,
                    //OrderIndex = int.Parse(subs.score),
                    ParentProductGroupID = parGroup.ProductGroupID
                  };
                  context.ProductGroups.InsertOnSubmit(subGroup);
                  context.SubmitChanges();
                }
                catch (Exception ex)
                {
                  log.Error(ex);
                  log.InfoFormat("sub {0}, produc {1}", sub, par);
                }
              }
            }
          }
          #endregion

          #region NEW SUB Productgroups
          var parents = (from p in context.ProductGroups
                         where !p.ParentProductGroupID.HasValue
                         select p).ToList();

          foreach (int parentProductID in NewSub.Keys)
          {
            string backendParentProductID = (from p in parents
                                             where p.ProductGroupID == parentProductID
                                             select p.ProductGroupCode).FirstOrDefault();

            foreach (string sub in NewSub[parentProductID])
            {
              try
              {
                var subs = (from pg in subids
                            where pg.subgroupid == sub
                            && pg.productgroupid == backendParentProductID
                            select pg).FirstOrDefault();

                if (subs != null)
                {
                  string parentproductgroupcode = (from p in parents
                                                   where p.ProductGroupID == parentProductID
                                                   select p.ProductGroupCode).FirstOrDefault();

                  ProductGroup subGroup = new ProductGroup
                  {
                    ProductGroupCode = sub,
                    BackendProductGroupCode = string.Empty,
                    ProductGroupName = subs.name,
                    LastModificationTime = DateTime.Now,
                    CreationTime = DateTime.Now,
                    //OrderIndex = int.Parse(subs.score),
                    ParentProductGroupID = parentProductID
                  };
                  context.ProductGroups.InsertOnSubmit(subGroup);
                  context.SubmitChanges();
                }
              }
              catch (Exception ex)
              {
                log.Error("productgroup", ex);
                log.InfoFormat("sub {0}, produc {1}", sub, backendParentProductID);
              }
            }
          }
          #endregion

          #region EX PAR
          foreach (ProductGroup p in ExParents)
          {
            var parid = (from pg in parids
                         where pg.productgroupid == p.ProductGroupCode
                         select pg).FirstOrDefault();

            if (p.ProductGroupName != parid.name)
            {
              p.BackendProductGroupCode = string.Empty;

              p.ProductGroupName = parid.name;
              p.LastModificationTime = DateTime.Now;
            }
            // if (p.OrderIndex != int.Parse(parid.score))
            //  p.OrderIndex = int.Parse(parid.score);
          }
          context.SubmitChanges();
          #endregion

          #region EX SUB
          foreach (string prodID in ExSub.Keys)
          {
            foreach (ProductGroup p in ExSub[prodID])
            {
              try
              {
                var subid = (from pg in subids
                             where pg.subgroupid == p.ProductGroupCode
                             && pg.productgroupid == prodID
                             select pg).FirstOrDefault();

                string parentproductgroupcode = (from pa in parents
                                                 where pa.ProductGroupID == p.ParentProductGroupID
                                                 select pa.ProductGroupCode).FirstOrDefault();


                if (p.ProductGroupName != subid.name)
                {
                  p.BackendProductGroupCode = string.Empty;

                  p.ProductGroupName = subid.name;
                  p.LastModificationTime = DateTime.Now;
                }
                //if (p.OrderIndex != int.Parse(subid.score))
                //  p.OrderIndex = int.Parse(subid.score);
              }
              catch (Exception ex)
              {
                log.Error(ex);
              }
            }
            context.SubmitChanges();
          }
          #endregion
        }
      }
      catch (Exception ex)
      {
        log.Error("Error productgroups", ex);
      }
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

    private void ImportProducts(Connector connector)
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
        {
          var productList = (from p in context.Products
                             select p).ToList();

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();


          existingProducts = (from c in productList
                              select c).ToDictionary(x => x.ProductID, cr => cr);

          var allProductGroups = (from p in context.ProductGroups
                                  where p.ParentProductGroupID.HasValue
                                  select p).ToList();

          var allParents = (from p in context.ProductGroups
                            where !p.ParentProductGroupID.HasValue
                            select p).ToList();

          var brands = (from b in context.Brands
                        select b).ToDictionary(x => x.BrandCode.Trim(), x => x);

          var records = (from r in products.Root.Elements("Product")
                         where !String.IsNullOrEmpty(r.Attribute("CustomProductID").Value)
                         select r);
          foreach (var r in records)
          {
            Product product = null;
            string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
            string longDescription = r.Element("Content").Attribute("LongDescription").Value;
            string backendDescription = string.Empty;

            int customproductID = int.Parse(r.Attribute("CustomProductID").Value);

            ProductGroup productgroup = null;

            if (r.Element("ProductGroupHierarchy").Element("ProductGroup") == null)
            {
              log.DebugFormat("Empty productgroups for product {0}", customproductID);
            }
            else
            {
              if (r.Element("ProductGroupHierarchy").Element("ProductGroup").Element("ProductGroup") == null)
              {
                productgroup = (from pg in allProductGroups
                                where pg.ProductGroupCode == r.Element("ProductGroupHierarchy").Element("ProductGroup").Attribute("ID").Value
                                select pg).FirstOrDefault();
              }
              else
              {

                var parentproductGroupID = allParents.Where(x => x.ProductGroupCode == r.Element("ProductGroupHierarchy").Element("ProductGroup").Element("ProductGroup").Attribute("ID").Value).FirstOrDefault().ProductGroupID;

                productgroup = (from pg in allProductGroups
                                where pg.ProductGroupCode == r.Element("ProductGroupHierarchy").Element("ProductGroup").Attribute("ID").Value
                                    && pg.ParentProductGroupID == parentproductGroupID
                                select pg).FirstOrDefault();
              }
            }

            try
            {
              if (connector.ImportCommercialText)
              {
                //backendDescription = longDescription;

                //shortDescription = r.Element("Content").Attribute("ShortContentDescription").Value;
                if (r.Element("Content") != null &&
                  r.Element("Content").Attribute("LongContentDescription") != null &&
                  !string.IsNullOrEmpty(r.Element("Content").Attribute("LongContentDescription").Value))
                  longDescription = r.Element("Content").Attribute("LongContentDescription").Value;

              }

              if (connector.ConcatenateBrandName)
              {
                StringBuilder sb = new StringBuilder();
                sb.Append(r.Element("Brands").Element("Brand").Element("Name").Value);
                sb.Append(" ");
                sb.Append(r.Element("Content").Attribute("ShortDescription").Value);
                shortDescription = sb.ToString();
              }

              if (shortDescription.Length > 150)
                shortDescription = shortDescription.Remove(150);

              if (longDescription.Length > 500)
                longDescription = longDescription.Remove(500);

              int? taxRateID = (from t in context.TaxRates
                                where t.TaxRate1.HasValue
                                && (t.TaxRate1.Value * 100) == decimal.Parse(r.Element("Price").Attribute("TaxRate").Value, CultureInfo.InvariantCulture)
                                select t.TaxRateID).FirstOrDefault();

              if (!taxRateID.HasValue)
                taxRateID = 1;

              bool extend = false;
              if (r.Element("ShopInformation") != null)
              {
                if (r.Element("ShopInformation").Element("ExtendedCatalog") != null)
                {
                  bool.TryParse(r.Element("ShopInformation").Element("ExtendedCatalog").Value, out extend);
                }
              }

              if (existingProducts.ContainsKey(customproductID))
              {
                DateTime temppdDate = DateTime.MinValue;
                DateTime? pdDate = null;
                if (DateTime.TryParse(r.Element("Stock").Attribute("PromisedDeliveryDate").Value, out temppdDate))
                {
                  if (temppdDate == DateTime.MinValue && temppdDate == DateTime.MaxValue)
                    pdDate = null;
                  else
                    pdDate = temppdDate;
                }

                if (brands.ContainsKey(r.Element("Brands").Element("Brand").Element("Code").Value.Trim()))
                {
                  product = existingProducts[int.Parse(r.Attribute("CustomProductID").Value)];
                  if (productgroup != null)
                  {
                    try
                    {
                      if (existingProducts[customproductID].ProductGroupID != productgroup.ProductGroupID
                        || existingProducts[customproductID].ShortDescription != shortDescription
                        || existingProducts[customproductID].LongDescription != longDescription
                        || existingProducts[customproductID].UnitPrice != decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture)
                        || existingProducts[customproductID].ProductStatus != r.Element("Price").Attribute("CommercialStatus").Value
                        || existingProducts[customproductID].ManufacturerID != (r.Attribute("ManufacturerID").Value.Length > 100 ? r.Attribute("ManufacturerID").Value.Remove(100) : r.Attribute("ManufacturerID").Value)
                        || existingProducts[customproductID].IsVisible != true
                        || existingProducts[customproductID].TaxRateID != taxRateID.Value
                        || existingProducts[customproductID].BrandID != brands[r.Element("Brands").Element("Brand").Element("Code").Value.Trim()].BrandID
                        || (pdDate.HasValue && (!existingProducts[customproductID].PromisedDeliveryDate.HasValue || existingProducts[customproductID].PromisedDeliveryDate.Value.CompareTo(pdDate.Value) != 0))
                        || existingProducts[customproductID].LineType != r.Attribute("LineType").Value.Trim()
                        || existingProducts[customproductID].LedgerClass != (r.Element("ShopInformation") != null ? r.Element("ShopInformation").Element("LedgerClass").Value.Trim() : string.Empty)
                        || existingProducts[customproductID].ProductDesk != (r.Element("ShopInformation") != null ? r.Element("ShopInformation").Element("ProductDesk").Value : string.Empty)
                        || existingProducts[customproductID].ExtendedCatalog != extend
                        || existingProducts[customproductID].UnitCost != decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture))
                      {
                        product.ProductGroupID = productgroup.ProductGroupID;
                        product.ShortDescription = shortDescription;
                        product.LongDescription = longDescription;
                        product.BrandID = brands[r.Element("Brands").Element("Brand").Element("Code").Value.Trim()].BrandID;
                        product.UnitPrice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                        product.ManufacturerID = r.Attribute("ManufacturerID").Value.Length > 100 ? r.Attribute("ManufacturerID").Value.Remove(100) : r.Attribute("ManufacturerID").Value;
                        product.ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value;
                        product.LastModificationTime = DateTime.Now;
                        product.IsVisible = true;
                        product.TaxRateID = taxRateID.Value;
                        product.PromisedDeliveryDate = pdDate;
                        product.LineType = r.Attribute("LineType").Value.Trim();
                        product.LedgerClass = r.Element("ShopInformation") != null ? r.Element("ShopInformation").Element("LedgerClass").Value.Trim() : string.Empty;
                        product.ProductDesk = r.Element("ShopInformation") != null ? r.Element("ShopInformation").Element("ProductDesk").Value : string.Empty;
                        product.ExtendedCatalog = extend;
                        product.UnitCost = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);
                        //if (!string.IsNullOrEmpty(backendDescription))
                        //  product.BackEndDescription = backendDescription;
                      }
                    }
                    catch (Exception ex)
                    {
                      log.Error("serious error", ex);
                    }
                  }
                  else
                  {
                    if (r.Element("ProductGroupHierarchy").Element("ProductGroup") != null)
                      log.DebugFormat("Productgroup does not exists, backendproductgroup {0}", r.Element("ProductGroupHierarchy").Element("ProductGroup").Attribute("ID").Value);
                    else
                      log.DebugFormat("Productgroup not matched for product {0}", customproductID);

                    product.IsVisible = false;
                    log.DebugFormat("Set Product {0} false, update price to {1}", customproductID, decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture));
                    product.ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value;
                    product.UnitCost = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);
                    product.LineType = r.Attribute("LineType").Value.Trim();
                    product.LedgerClass = r.Element("ShopInformation").Element("LedgerClass").Value.Trim();
                    product.ProductDesk = r.Element("ShopInformation").Element("ProductDesk").Value;
                    product.LastModificationTime = DateTime.Now;
                    product.UnitPrice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                    product.ManufacturerID = r.Attribute("ManufacturerID").Value.Length > 100 ? r.Attribute("ManufacturerID").Value.Remove(100) : r.Attribute("ManufacturerID").Value;
                    product.ShortDescription = shortDescription;
                    product.LongDescription = longDescription;
                  }
                }
                else
                {
                  log.DebugFormat("Update product failed, brand {0} not availible", r.Element("Brands").Element("Brand").Element("Code").Value.Trim());
                }
              }
              else
              {
                if (!customerItemNumbers.ContainsKey(r.Attribute("CustomProductID").Value))
                {
                  decimal unitprice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                  decimal costprice = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);

                  if (brands.ContainsKey(r.Element("Brands").Element("Brand").Element("Code").Value.Trim()))
                  {
                    int productgroupID = 0;

                    if (productgroup != null)
                    {
                      productgroupID = productgroup.ProductGroupID;


                    }
                    else
                    {
                      int? parentProductGroupID = (from p in allParents
                                                   where p.ProductGroupName == "Missing category"
                                                   select p.ProductGroupID).FirstOrDefault();

                      productgroupID = allProductGroups.Where(x => x.ParentProductGroupID == parentProductGroupID).Select(x => x.ProductGroupID).FirstOrDefault();

                    }

                    product = new Product
                                 {
                                   ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                                   ProductGroupID = productgroupID,
                                   ShortDescription = shortDescription,
                                   LongDescription = longDescription,
                                   ManufacturerID = r.Attribute("ManufacturerID").Value,
                                   BrandID = brands[r.Element("Brands").Element("Brand").Element("Code").Value.Trim()].BrandID,
                                   TaxRateID = taxRateID.Value,
                                   UnitPrice = unitprice,
                                   UnitCost = costprice,
                                   IsCustom = false,
                                   LineType = r.Attribute("LineType").Value.Trim(),
                                   ProductStatus = r.Element("Price").Attribute("CommercialStatus").Value,
                                   IsVisible = true,
                                   CreationTime = DateTime.Now,
                                   LastModificationTime = DateTime.Now,
                                   LedgerClass = r.Element("ShopInformation").Element("LedgerClass").Value.Trim(),
                                   ProductDesk = r.Element("ShopInformation").Element("ProductDesk").Value,
                                   ExtendedCatalog = extend
                                 };

                    DateTime pdDate;
                    if (DateTime.TryParse(r.Element("Stock").Attribute("PromisedDeliveryDate").Value, out pdDate))
                    {
                      if (pdDate != DateTime.MinValue && pdDate != DateTime.MaxValue)
                        product.PromisedDeliveryDate = pdDate;
                    }

                    //if (product.BrandID > 0)
                    context.Products.InsertOnSubmit(product);
                  }
                  else
                  {
                    log.DebugFormat("Insert product failed, brand {0} not availible", r.Element("Brands").Element("Brand").Element("Code").Value.Trim());
                  }
                }
              }

              if (!customerItemNumbers.ContainsKey(r.Attribute("CustomProductID").Value))
                customerItemNumbers.Add(r.Attribute("CustomProductID").Value, int.Parse(r.Attribute("ProductID").Value));

            }
            catch (Exception ex)
            {
              log.Error("Error processing product " + customproductID, ex);
              log.Error(ex.StackTrace);
            }
          }
          context.SubmitChanges();
        }
      }
      catch (Exception ex)
      {
        log.Error("Error import products", ex);
        log.Debug(ex.StackTrace);
      }
    }

    private void ImportBrands(Connector connector)
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
        {
          List<string> CreatedBrands = new List<string>();

          var ids = (from r in products.Root.Elements("Product")
                     select r.Element("Brands").Element("Brand").Element("Code").Value).Distinct().ToArray();

          Dictionary<string, Brand> existingBrands = new Dictionary<string, Brand>();
          List<string> brandcodes = new List<string>();

          foreach (string id in ids)
          {
            var stk = (from c in context.Brands
                       where c.BrandCode == id.Trim()
                       select c).SingleOrDefault();

            if (stk != null && !existingBrands.ContainsKey(stk.BrandCode.Trim()))
            {
              existingBrands.Add(stk.BrandCode.Trim(), stk);
              brandcodes.Add(stk.BrandCode.Trim());
            }
          }

          foreach (var r in products.Root.Elements("Product").Elements("Brands").Elements("Brand").Distinct())
          {
            Brand brand = null;
            string brandcode = r.Element("Code").Value.Trim();

            if (existingBrands.ContainsKey(brandcode))
            {
              if (existingBrands[brandcode].BrandName != r.Element("Name").Value)
              {
                brand = existingBrands[brandcode];
                brand.BrandName = r.Element("Name").Value;
                brand.LastModificationTime = DateTime.Now;
              }
            }
            else
            {
              if (!CreatedBrands.Contains(brandcode))
              {
                brand = new Brand();
                brand.CreationTime = DateTime.Now;
                brand.LastModificationTime = DateTime.Now;
                brand.CreatedBy = 0;
                brand.LastModifiedBy = 0;
                brand.BrandCode = brandcode.Trim();
                brand.BrandName = r.Element("Name").Value;
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

    private void ImportBarcode(Connector connector)
    {
      using (ShopDataContext context = new ShopDataContext(connector.ConnectionString))
      {
        var ids = (from r in products.Root.Elements("Product")
                   select int.Parse(r.Attribute("CustomProductID").Value)).Distinct().ToArray();

        var barcodes = (from b in context.ProductBarcodes
                        select b).ToList();

        Dictionary<int, List<ProductBarcode>> existingBarcode = new Dictionary<int, List<ProductBarcode>>();

        //        for (int i = barcodes.Count - 1; i >= 0; i--)
        //        {
        //          ProductBarcode bar = barcodes[i];

        //          var pr = (from p in products.Root.Elements("Product")
        //                    where p.Attribute("CustomProductID").Value == bar.ProductID.ToString()
        //&& (p.Elements("Barcodes").Count() > 0 && p.Elements("Barcodes").Where(x => x.Element("Barcode").Value.Trim() == bar.Barcode).Count() > 0)
        //                    select p).FirstOrDefault();

        //          if (pr == null)
        //          {
        //            log.InfoFormat("Delete productbarcode item {0} ({1})", bar.ProductID, bar.Barcode);

        //            context.ProductBarcodes.DeleteOnSubmit(bar);
        //            context.SubmitChanges();
        //          }
        //        }

        barcodes = (from b in context.ProductBarcodes
                    select b).ToList();

        foreach (int id in ids)
        {
          var stk = (from c in barcodes
                     where c.ProductID == id
                     select c).ToList();

          if (stk != null)
            existingBarcode.Add(id, stk);
        }

        List<ProductBarcode> barcodeList = new List<ProductBarcode>();

        foreach (var prod in products.Root.Elements("Product"))
        {
          try
          {
            if (prod.Elements("Barcodes").Elements("Barcode").Count() > 0)
            {
              foreach (var bar in prod.Elements("Barcodes").Elements("Barcode"))
              {
                if (!string.IsNullOrEmpty(bar.Value))
                {
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

                  ProductBarcode barcode = null;

                  if (existingBarcode.ContainsKey(int.Parse(prod.Attribute("CustomProductID").Value)) && existingBarcode[int.Parse(prod.Attribute("CustomProductID").Value)].Where(x => x.Barcode == barcodecode).Count() > 0)
                  {
                    barcode = existingBarcode[int.Parse(prod.Attribute("CustomProductID").Value)].Where(x => x.Barcode == barcodecode).FirstOrDefault();
                    int custom = int.Parse(prod.Attribute("CustomProductID").Value);
                    if (barcode.Barcode.Trim() != barcodecode
                      || barcode.BarcodeType.Trim() != barcodetype)
                    {
                      context.ProductBarcodes.DeleteOnSubmit(barcode);
                      context.SubmitChanges();

                      if (barcodeList.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1
                        && barcodes.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1)
                      {
                        barcode = ShopUtility.InsertBarcode(int.Parse(prod.Attribute("CustomProductID").Value), barcodecode, barcodetype);
                        context.ProductBarcodes.InsertOnSubmit(barcode);
                        barcodeList.Add(barcode);
                      }
                    }
                  }
                  else
                  {
                    if (barcodeList.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1
                      && barcodes.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1
                      )
                    {
                      barcode = ShopUtility.InsertBarcode(int.Parse(prod.Attribute("CustomProductID").Value), barcodecode, barcodetype);
                      context.ProductBarcodes.InsertOnSubmit(barcode);
                      context.SubmitChanges();
                      barcodeList.Add(barcode);
                    }
                  }
                }
              }
            }
          }
          catch (Exception ex)
          {
            log.Error("Error import barcode", ex);
          }
        }
        context.SubmitChanges(ConflictMode.ContinueOnConflict);

      }
    }


  }
}

