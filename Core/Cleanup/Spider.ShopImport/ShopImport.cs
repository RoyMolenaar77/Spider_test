using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Configuration;
using System.Data.Linq;
using System.Globalization;

namespace Spider.ShopImport
{
  public class ShopImport
  {
    private XDocument products;
    private Dictionary<string, int> customerItemNumbers = new Dictionary<string, int>();
    private Dictionary<int, List<int>> pgmList = new Dictionary<int, List<int>>();

    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("WebsiteImportService");

    public ShopImport()
    {

    }

    public void ProcessProducts()
    {
      try
      {
        DateTime start = DateTime.Now;
        log.InfoFormat("Start process products:{0}", start);
        ConcentratorContentService.SpiderServiceSoapClient soap =
          new ConcentratorContentService.SpiderServiceSoapClient();
        if (ConfigurationManager.AppSettings["ImportCommercialText"] != null)
        {
          if (bool.Parse(ConfigurationManager.AppSettings["ImportCommercialText"].ToString()))
            products = XDocument.Parse(soap.GetAssortmentContent(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), true, true, true).OuterXml);
          else
            products = XDocument.Parse(soap.GetAssortmentContent(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), false, true, true).OuterXml);
        }
        else
          products = XDocument.Parse(soap.GetAssortmentContent(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), false, true, true).OuterXml);

        log.Info("Start import brands");
        ImportBrands();
        log.Info("Finish import brands");
        log.Info("Start import productgroups");
        ImportProductGroups();
        log.Info("Finish import productgroups");

        log.Info("Start import taxrates");
        ImportTaxRates();
        log.Info("Finish import taxrates");

        log.Info("Start import Products");
        ImportProducts();
        log.Info("Finish import Products");

        log.Info("Start import productbarcodes");
        ImportBarcode();
        log.Info("Finish import productbarcodes");

        CleanUpProducts(customerItemNumbers.Keys);

        //ImportBarcode();
        log.InfoFormat("Finish process products: {0} duration {1} minutes", DateTime.Now, TimeSpan.FromMilliseconds(DateTime.Compare(DateTime.Now, start)).TotalMinutes);
      }
      catch (Exception ex)
      {
        log.Error("Error import ProcessProducts", ex);
      }
    }

    private void ImportTaxRates()
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
        {
          var ids = (from r in products.Root.Elements("Product")
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

    private void ImportOItems(XDocument o_Products)
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
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

    public void ProcessOItems()
    {
      try
      {
        ConcentratorContentService.SpiderServiceSoapClient soap = new ConcentratorContentService.SpiderServiceSoapClient();
        log.Info("Start import O Products");
        XDocument o_Products = XDocument.Parse(soap.GetOAssortment(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), true).OuterXml);
        if (o_Products != null)
        {
          ImportOItems(o_Products);
          //ImportOBarcodes(o_Products);
        }
        else
          log.Info("Get O Assortment failed");
      }
      catch (Exception ex)
      {
        log.Fatal(ex);
      }
    }

    public void ProcessProductsAndAttributes()
    {
      DateTime start = DateTime.Now;
      log.InfoFormat("Start process products and attributes:{0}", start);
      ConcentratorContentService.SpiderServiceSoapClient soap = new ConcentratorContentService.SpiderServiceSoapClient();
      if (ConfigurationManager.AppSettings["ImportCommercialText"] != null)
      {
        if (bool.Parse(ConfigurationManager.AppSettings["ImportCommercialText"].ToString()))
          products = XDocument.Parse(soap.GetAssortmentContent(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), true, true, true).OuterXml);
        else
          products = XDocument.Parse(soap.GetAssortmentContent(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), false, true, true).OuterXml);
      }
      else
        products = XDocument.Parse(soap.GetAssortmentContent(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), false, true, true).OuterXml);

      log.Info("Start import brands");
      ImportBrands();
      log.Info("Finish import brands");
      log.Info("Start import productgroups");
      ImportProductGroups();
      log.Info("Finish import productgroups");
      log.Info("Start import Products");
      ImportProducts();
      log.Info("Finish import Products");

      //log.Info("Start import productbarcodes");
      //ImportBarcode();
      //log.Info("Finish import productbarcodes");

      CleanUpProducts(customerItemNumbers.Keys);

      try
      {
        if (ConfigurationManager.AppSettings["ImportOItems"] != null)
        {
          if (bool.Parse(ConfigurationManager.AppSettings["ImportOItems"].ToString()))
          {
            log.Info("Start import O Products");
            XDocument o_Products = XDocument.Parse(soap.GetOAssortment(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), true).OuterXml);
            if (o_Products != null)
            {
              ImportOItems(o_Products);
              //ImportOBarcodes(o_Products);
            }
            else
              log.Info("Get O Assortment failed");

            log.Info("Finish import O Products");
          }
        }
      }
      catch (Exception ex)
      {
        log.Error("ProductAttributes error", ex);
      }

      try
      {
        int counter = customerItemNumbers.Count();
        ProcessAttributes(customerItemNumbers, soap);
        log.InfoFormat("Finish process products and attributes: {0} duration {1} minutes", DateTime.Now, TimeSpan.FromMilliseconds(DateTime.Compare(DateTime.Now, start)).TotalMinutes);
      }
      catch (Exception ex)
      {
        log.Error("ProductAttributes error", ex);
      }

    }

    private void ImportOBarcodes(XDocument o_Products)
    {
      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
      {
        var ids = (from r in o_Products.Root.Elements("Product")
                   select r.Attribute("CustomProductID").Value).Distinct().ToArray();

        var barcodes = (from b in context.ProductBarcodes
                        select b).ToList();

        Dictionary<int, ProductBarcode> existingBarcode = new Dictionary<int, ProductBarcode>();

        for (int i = barcodes.Count - 1; i >= 0; i--)
        {
          ProductBarcode bar = barcodes[i];

          var pr = (from p in o_Products.Root.Elements("Product")
                    where p.Attribute("CustomProductID").Value == bar.ProductID.ToString()
&& p.Elements("Barcodes").Where(x => x.Element("Barcode").Value.Trim() == bar.Barcode).Count() > 0
                    select p).FirstOrDefault();

          if (pr == null)
          {
            log.InfoFormat("Delete productbarcode item {0} ({1})", bar.ProductID, bar.Barcode);

            context.ProductBarcodes.DeleteOnSubmit(bar);
            context.SubmitChanges();
          }
        }

        barcodes = (from b in context.ProductBarcodes
                    select b).ToList();

        foreach (string id in ids)
        {
          int prodid = 0;
          if (!int.TryParse(id, out prodid))
          {
            log.InfoFormat("Could not process product {0}", id);
            continue;
          }

          var stk = (from c in barcodes
                     where c.ProductID == prodid
                     select c).FirstOrDefault();

          if (stk != null && !existingBarcode.ContainsKey(stk.ProductID))
            existingBarcode.Add(stk.ProductID, stk);
        }

        List<ProductBarcode> barcodeList = new List<ProductBarcode>();

        foreach (var prod in o_Products.Root.Elements("Product"))
        {
          try
          {
            int prodid = 0;
            if (!int.TryParse(prod.Attribute("CustomProductID").Value, out prodid))
            {
              log.InfoFormat("Could not process product {0}", prod.Attribute("CustomProductID").Value);
              continue;
            }

            foreach (var bar in prod.Elements("Barcodes"))
            {
              if (bar.Element("Barcode") != null && !string.IsNullOrEmpty(bar.Element("Barcode").Value))
              {
                string barcodecode = bar.Element("Barcode").Value.Trim();
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
                if (existingBarcode.ContainsKey(int.Parse(prod.Attribute("CustomProductID").Value)))
                {
                  int custom = int.Parse(prod.Attribute("CustomProductID").Value);
                  if (existingBarcode[custom].Barcode.Trim() != barcodecode
                    || existingBarcode[custom].BarcodeType.Trim() != barcodetype)
                  {
                    barcode = existingBarcode[custom];
                    context.ProductBarcodes.DeleteOnSubmit(barcode);
                    context.SubmitChanges();

                    if (barcodeList.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1
                      && barcodes.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1)
                    {
                      barcode = InsertBarcode(int.Parse(prod.Attribute("CustomProductID").Value), barcodecode, barcodetype);
                      context.ProductBarcodes.InsertOnSubmit(barcode);
                      barcodeList.Add(barcode);
                    }
                  }
                }
                else
                {
                  if (barcodeList.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1
                    && barcodes.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1)
                  {
                    barcode = InsertBarcode(int.Parse(prod.Attribute("CustomProductID").Value), barcodecode, barcodetype);
                    context.ProductBarcodes.InsertOnSubmit(barcode);
                    context.SubmitChanges();
                    barcodeList.Add(barcode);
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
        context.SubmitChanges();

      }
    }

    private void CleanUpProducts(Dictionary<string, int>.KeyCollection customerItemNumbers)
    {
      log.Info("Start Clean products");
      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
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

    public void ProcessStock()
    {
      DateTime start = DateTime.Now;
      log.InfoFormat("Start process stock:{0}", start);
      ConcentratorContentService.SpiderServiceSoapClient soap =
       new ConcentratorContentService.SpiderServiceSoapClient();
      products = XDocument.Parse(soap.GetAssortment(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString())).OuterXml);

      log.Info(products.Root.Elements("Product").Count());

      ImportStock();
      ImportRetailStock();

      log.InfoFormat("Finish process stock: {0} duration {1} minutes", DateTime.Now, TimeSpan.FromMilliseconds(DateTime.Compare(DateTime.Now, start)).TotalMinutes);
    }

    private void ImportRetailStock()
    {
      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
      {
        var ids = (from r in products.Root.Elements("Product")
                   select int.Parse(r.Attribute("CustomProductID").Value)).Distinct().ToArray();

        try
        {
          Dictionary<int, List<VendorStock>> existingStock = new Dictionary<int, List<VendorStock>>();

          foreach (int id in ids)
          {
            var stk = (from c in context.VendorStocks
                       where c.ProductID == id
                       select c).ToList();

            if (stk.Count() > 0)
            {
              if (!existingStock.ContainsKey(stk.First().ProductID))
                existingStock.Add(stk.First().ProductID, stk);
              else
              {
                foreach (VendorStock vs in stk)
                {
                  if (!existingStock[stk.First().ProductID].Contains(vs))
                    existingStock[stk.First().ProductID].Add(vs);
                }
              }
            }
          }

          List<int> addedstock = new List<int>();

          foreach (var r in products.Root.Elements("Product"))
          {
            int custom = int.Parse(r.Attribute("CustomProductID").Value);
            if (!addedstock.Contains(custom))
            {
              foreach (var vendor in r.Element("Stock").Element("Retail").Elements("RetailStock"))
              {
                VendorStock stock = null;
                if (existingStock.ContainsKey(int.Parse(r.Attribute("CustomProductID").Value)))
                {
                  stock = existingStock[int.Parse(r.Attribute("CustomProductID").Value)].Where(x => x.StockLocationID == vendor.Attribute("Name").Value).SingleOrDefault();
                  if (stock != null)
                  {
                    if (stock.QuantityOnHand != int.Parse(vendor.Attribute("InStock").Value))
                    {
                      stock.QuantityOnHand = int.Parse(vendor.Attribute("InStock").Value);
                      stock.LastModificationTime = DateTime.Now;
                    }
                  }
                  else
                  {
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
                  }
                }
                else
                {
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
                }
                addedstock.Add(custom);
              }
            }
          }
          context.SubmitChanges();
        }
        catch (Exception ex)
        {
          log.Error("Error import vendor stock", ex);
          log.Debug(ex.StackTrace);
        }

        try
        {
          log.Info("Start Clean vendorstock");
          var oldvendorstock = (from p in context.VendorStocks
                                where !ids.Contains(p.ProductID)
         && p.QuantityOnHand > 0
                                select p).ToList();
          foreach (var p in oldvendorstock)
          {
            p.QuantityOnHand = 0;
          }
          context.SubmitChanges();

          log.Info("Clean vendorstock finished");
        }
        catch (Exception ex)
        {
          log.Error("Error import vendor stock", ex);
          log.Debug(ex.StackTrace);
        }
      }
    }

    private void CleanUpProducts()
    {
      log.Info("Start Clean products");
      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
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

    #region MultiThread product attribute processing

    // private static Mutex mut = new Mutex();
    private static Mutex mut2 = new Mutex();
    private static Mutex mut3 = new Mutex();
    private static Mutex mut4 = new Mutex();

    private void ProcessAttributes(Dictionary<string, int> productList, ConcentratorContentService.SpiderServiceSoapClient soap)
    {
      List<ManualResetEvent> manualEvents = new List<ManualResetEvent>();
      ThreadPool.SetMaxThreads(40, 40);

      List<BatchResult> _batchResults = new List<BatchResult>();

      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
      {
        List<ProductGroup> productgroups = (from pg in context.ProductGroups
                                            select pg).ToList();

        List<Product> products = (from p in context.Products
                                  where p.IsVisible
                                  select p).ToList();

        for (int i = productList.Keys.Count - 1; i >= 0; i--)
        {
          ManualResetEvent e = new ManualResetEvent(false);
          StateInfo info = new StateInfo { CustomItemNumber = productList.Keys.ToList()[i], ProductID = productList.Values.ToList()[i], ResetEvent = e, soap = soap, ProductGroups = productgroups, Products = products };

          manualEvents.Add(e);
          productList.Remove(info.CustomItemNumber);
          ThreadPool.QueueUserWorkItem(new WaitCallback(Callback), info);

          int totalNumberOfProductsToProcess = productList.Count();

          if (productList.Count <= 0 || manualEvents.Count > 39)
          {
            DateTime startTime = DateTime.Now;

            int numberOfProducts = manualEvents.Count;
            log.Info("=================================================");
            log.InfoFormat("Start processing product specs, batch of {0} products.", numberOfProducts);
            WaitHandle.WaitAll(manualEvents.ToArray());
            double executionTime = (DateTime.Now - startTime).TotalSeconds;
            log.InfoFormat("Processed {0} products in : {1} seconds", numberOfProducts, executionTime);
            _batchResults.Add(new BatchResult(numberOfProducts, executionTime));

            double averageSecondsPerProduct = _batchResults.Average(b => b.ExecutionTimeInSeconds / b.NumberOfProducts);
            double secondsNeeded = productList.Count * averageSecondsPerProduct;

            log.InfoFormat("Still need to process {0} of {1}; {2} done; Estimated completiontime: {3}", productList.Count, totalNumberOfProductsToProcess, totalNumberOfProductsToProcess - productList.Count, DateTime.Now.AddSeconds(secondsNeeded));
            log.Info("=================================================");
            manualEvents.Clear();
          }
        }
      }

    }

    private static void Callback(object state)
    {
      StateInfo info = state as StateInfo;
      //log.InfoFormat("Process attributes for {0}", info.ProductID);
      try
      {
        XDocument attributes = XDocument.Parse(info.soap.GetAttributesAssortment(int.Parse(ConfigurationManager.AppSettings["ConnectorID"].ToString()), info.ProductID).OuterXml);
        // mut.WaitOne();
        // log.InfoFormat("ImportAttributeParents for productid {0}", info.ProductID);
        // ImportAttributeParents(attributes, info.ProductGroups);
        // mut.ReleaseMutex();

        mut2.WaitOne();
        //log.InfoFormat("ImportAttributes for productid {0}", info.ProductID);
        ImportAttributes(attributes);
        mut2.ReleaseMutex();

        mut3.WaitOne();
        //log.InfoFormat("ImportProductGroupsAttributes for productid {0}", info.ProductID);
        ImportProductGroupsAttributes(attributes, info.ProductGroups);
        mut3.ReleaseMutex();

        mut4.WaitOne();
        // log.InfoFormat("ImportAttributeValues for productid {0}", info.ProductID);
        ImportAttributeValues(info.CustomItemNumber, attributes, info.Products, info.ProductGroups);
        mut4.ReleaseMutex();
      }
      catch (Exception ex)
      {
        log.Error("Error importing product attribute for" + info.ProductID, ex);
      }
      finally
      {
        info.ResetEvent.Set();
      }
    }

    public class StateInfo
    {
      public ConcentratorContentService.SpiderServiceSoapClient soap { get; set; }
      public int ProductID { get; set; }
      public string CustomItemNumber { get; set; }
      public ManualResetEvent ResetEvent { get; set; }
      public List<ProductGroup> ProductGroups { get; set; }
      public List<Product> Products { get; set; }
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

    private static void ImportAttributeValues(string customItemNumber, XDocument attributes, List<Product> products, List<ProductGroup> productGroups)
    {
      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
      {
        var existingProducts = (
            from p in products
            where p.ProductID == int.Parse(customItemNumber)
            select p.ProductID).ToList();

        List<int> createdAttributeValues = new List<int>();

        var attributList = (from r in attributes.Root.Elements("ProductAttribute").Elements("Attributes").Elements("Attribute")
                            let groupID = r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                            let productgroup = (from p in productGroups
                                                where p.ParentProductGroupID.HasValue
                                                && p.ProductGroupCode == r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                                                select p.ProductGroupID).SingleOrDefault()
                            let customproductid = r.Ancestors("ProductAttribute").First().Attribute("CustomProductID")
                            where productgroup > 0
                            select new
                            {
                              AttributeID = int.Parse(r.Attribute("AttributeID").Value),
                              Name = r.Element("Name").Value,
                              GroupID = groupID,
                              ProductID = int.Parse(customproductid.Value),
                              Value = r.Element("Value").Value,
                              ProductGroupID = productgroup,
                              AttributeGroupID = int.Parse(r.Attribute("AttributeGroupID").Value)
                            }).Distinct().ToList();

        Dictionary<int, int> pgList = new Dictionary<int, int>();
        List<ProductAttribute> existingProductAttributes = new List<ProductAttribute>();

        foreach (var att in attributList)
        {
          var attmd = (from a in context.ProductAttributes
                       where a.ProductGroupID == att.ProductGroupID
                       && a.ProductID == att.ProductID
                       && a.AttributeMetaDataID == att.AttributeID
                       select a).SingleOrDefault();

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
                         && a.ProductGroupID == att.ProductGroupID
                         select a).SingleOrDefault();

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
                && x.ProductID == att.ProductID
                && x.ProductGroupID == att.ProductGroupID).FirstOrDefault();
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
                val.LastModificationTime = DateTime.Now;
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
                  )
                {
                  existatval.AttributeValue = att.Value.Length > 255 ? att.Value.Substring(0, 255) : att.Value;//truncating text to 50 chars
                  existatval.LastModificationTime = DateTime.Now;
                }
              }
            }
            // context.SubmitChanges();
          }
          context.SubmitChanges();
        }
      }
    }

    private static void ImportProductGroupsAttributes(XDocument attributes, List<ProductGroup> productGroups)
    {
      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
      {
        Dictionary<string, List<int>> createdAttributes = new Dictionary<string, List<int>>();

        var attributList = (from r in attributes.Root.Elements("ProductAttribute").Elements("Attributes").Elements("Attribute")
                            let productgroup = (from p in productGroups
                                                where p.ParentProductGroupID.HasValue
                                                && p.ProductGroupCode == r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                                                select p.ProductGroupID).SingleOrDefault()
                            select new
                            {
                              AttributeMetaDataID = int.Parse(r.Attribute("AttributeID").Value),
                              KeyFeature = r.Attribute("KeyFeature").Value,
                              Index = r.Attribute("Index").Value,
                              IsSearchable = r.Attribute("IsSearchable").Value,
                              ProductGroupID = productgroup,
                              BackendProductID = r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                            }).Distinct().ToList();

        Dictionary<int, int> pgList = new Dictionary<int, int>();
        List<ProductGroupsAttributeMetaData> existingAttributes = new List<ProductGroupsAttributeMetaData>();

        foreach (var att in attributList)
        {
          var attmd = (from a in context.ProductGroupsAttributeMetaDatas
                       where a.ProductGroupID == att.ProductGroupID
                       && a.AttributeMetaDataID == att.AttributeMetaDataID
                       select a).SingleOrDefault();

          if (attmd != null)
            existingAttributes.Add(attmd);
        }

        List<ProductGroupsAttributeMetaData> addedList = new List<ProductGroupsAttributeMetaData>();

        if (attributList.Count() > 0)
        {
          foreach (var at in attributList)
          {
            try
            {
              if (at.ProductGroupID != null && at.ProductGroupID > 0)
              {
                ProductGroupsAttributeMetaData pgamd = existingAttributes.Where(x => x.AttributeMetaDataID == at.AttributeMetaDataID
                  && x.ProductGroupID == at.ProductGroupID).FirstOrDefault();

                var added = (from a in addedList
                             where a.ProductGroupID == at.ProductGroupID
                             && a.AttributeMetaDataID == at.AttributeMetaDataID
                             select a).ToList();

                if (added.Count < 1)
                {
                  if (pgamd == null)
                  {
                    pgamd = new ProductGroupsAttributeMetaData();
                    pgamd.AttributeMetaDataID = at.AttributeMetaDataID;
                    if (at.IsSearchable == "0")
                      pgamd.IsQuickSearch = false;
                    else
                      pgamd.IsQuickSearch = true;

                    if (at.KeyFeature == "0")
                      pgamd.IsRequired = true;
                    else
                      pgamd.IsRequired = false;

                    pgamd.OrderIndex = int.Parse(at.Index);
                    pgamd.ProductGroupID = at.ProductGroupID;
                    pgamd.LastModificationTime = DateTime.Now;

                    context.ProductGroupsAttributeMetaDatas.InsertOnSubmit(pgamd);
                    addedList.Add(pgamd);
                    //createdAttributes.Add(at.AttributeMetaDataID);
                  }
                  else
                  //if (existingAttributes.ContainsKey(at.AttributeMetaDataID)
                  //&& existingAttributes[at.AttributeMetaDataID].ProductGroup.BackendProductGroupCode == at.BackendProductGroupCode)
                  {
                    bool isQuickSearch = false;
                    bool isRequired = false;

                    if (at.IsSearchable == "0")
                      isQuickSearch = false;
                    else
                      isQuickSearch = true;

                    if (at.KeyFeature == "0")
                      isRequired = true;
                    else
                      isRequired = false;

                    if (pgamd.IsQuickSearch != isQuickSearch
                      || pgamd.IsRequired != isRequired
                      || pgamd.OrderIndex != int.Parse(at.Index))
                    {
                      pgamd.IsQuickSearch = isQuickSearch;
                      pgamd.IsRequired = isRequired;
                      pgamd.OrderIndex = int.Parse(at.Index);
                      pgamd.LastModificationTime = DateTime.Now;
                    }
                  }
                }
                //context.SubmitChanges();
              }
              else
              {
                log.InfoFormat("ProductGroup with backendproductgroupcode {0} does not exists", at.BackendProductID);
              }

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

    private static void ImportAttributes(XDocument attributes)
    {
      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
      {
        List<int> createdAttributes = new List<int>();

        var attributList = (from r in attributes.Root.Elements("ProductAttribute").Elements("Attributes").Elements("Attribute")
                            let productgroup = r.Ancestors("ProductAttribute").First().Attribute("ProductSubGroupID").Value
                            select new AttributeMetaData
                            {
                              AttributeMetaDataID = int.Parse(r.Attribute("AttributeID").Value),
                              AttributeName = r.Element("Name").Value,
                              FormatString = r.Element("Sign").Value
                            }).Distinct().ToList();

        if (attributList.Count > 0)
        {
          List<AttributeMetaData> existingAttributes = (
     from a in context.AttributeMetaDatas
     select a).ToList();

          foreach (AttributeMetaData at in attributList)
          {
            if (existingAttributes.Where(x => x.AttributeMetaDataID == at.AttributeMetaDataID).Count() < 1
              && !createdAttributes.Contains(at.AttributeMetaDataID))
            {
              at.LastModificationTime = DateTime.Now;
              at.DataTypeID = 1;
              context.AttributeMetaDatas.InsertOnSubmit(at);
              createdAttributes.Add(at.AttributeMetaDataID);
            }
            else
            {
              AttributeMetaData existat = (from a in existingAttributes
                                           where a.AttributeMetaDataID == at.AttributeMetaDataID
                                           select a).SingleOrDefault();
              if (existat != null)
              {
                if (existat.AttributeName != at.AttributeName
                || existat.FormatString != at.FormatString
                || existat.AttributeMetaDataID != at.AttributeMetaDataID)
                {
                  //map existing attribute...
                  existat.AttributeName = at.AttributeName;
                  existat.DataTypeID = 1;
                  existat.FormatString = at.FormatString;
                  existat.AttributeMetaDataID = at.AttributeMetaDataID;
                  existat.LastModificationTime = DateTime.Now;
                }
              }
            }
            //context.SubmitChanges();
          }
          context.SubmitChanges();
        }
      }
    }

    private void ImportProductGroups()
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
        {
          var productGroups = (from pg in context.ProductGroups
                               select pg).ToList();

          var parids = (from r in products.Root.Elements("Product").Elements("ProductGroups").Elements("ProductGroup")
                        select new
                        {
                          productgroupid = r.Attribute("ProductGroupID").Value,
                          code = r.Element("Code").Value,
                          name = r.Element("Name").Value,
                          score = r.Attribute("Index").Value
                        }
                    ).Distinct().ToArray();

          var subids = (from r in products.Root.Elements("Product").Elements("ProductGroups").Elements("SubProductGroup")
                        select new
                        {
                          subgroupid = r.Attribute("SubProductGroupID").Value,
                          productgroupid = r.Attribute("ProductGroupID").Value,
                          code = r.Element("Code").Value,
                          name = r.Element("Name").Value,
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
                BackendProductGroupCode = pars.code,
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
                    BackendProductGroupCode = subs.code,
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
                                             select p.BackendProductGroupCode).FirstOrDefault();

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
                    BackendProductGroupCode = subs.code,
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

            if (p.ProductGroupName != parid.name
              || p.BackendProductGroupCode != parid.code)
            {
              p.BackendProductGroupCode = parid.code;
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


                if (p.ProductGroupName != subid.name
                  || p.BackendProductGroupCode != subid.code)
                {
                  p.BackendProductGroupCode = subid.code;
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

    private void ImportProducts()
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
        {
          var ids = (from r in products.Root.Elements("Product")
                     select int.Parse(r.Attribute("CustomProductID").Value)).Distinct().ToArray();

          var productList = (from p in context.Products
                             select p).ToList();

          Dictionary<int, Product> existingProducts = new Dictionary<int, Product>();

          foreach (int id in ids)
          {
            var pr = (from c in productList
                      where c.ProductID == id
                      select c).SingleOrDefault();

            if (pr != null && !existingProducts.ContainsKey(pr.ProductID))
              existingProducts.Add(pr.ProductID, pr);
          }

          var allProductGroups = (from p in context.ProductGroups
                                  where p.ParentProductGroupID.HasValue
                                  select p).ToList();

          var allParents = (from p in context.ProductGroups
                            where !p.ParentProductGroupID.HasValue
                            select p).ToList();

          var brands = (from b in context.Brands
                        select b).ToDictionary(x => x.BrandCode.Trim(), x => x);

          foreach (var r in products.Root.Elements("Product"))
          {
            Product product = null;
            string shortDescription = r.Element("Content").Attribute("ShortDescription").Value;
            string longDescription = r.Element("Content").Attribute("LongDescription").Value;
            string backendDescription = string.Empty;

            int customproductID = int.Parse(r.Attribute("CustomProductID").Value);

            ProductGroup productgroup = null;

            if (r.Element("ProductGroups").Element("SubProductGroup") == null)
            {
              log.DebugFormat("Empty productgroups for product {0}", customproductID);
            }
            else
            {
              productgroup = (from pg in allProductGroups
                              where pg.ProductGroupCode == r.Element("ProductGroups").Element("SubProductGroup").Attribute("SubProductGroupID").Value
                                  && pg.ParentProductGroupID == allParents.Where(x => x.ProductGroupCode == r.Element("ProductGroups").Element("SubProductGroup").Attribute("ProductGroupID").Value).FirstOrDefault().ProductGroupID
                              select pg).FirstOrDefault();
            }

            try
            {
              if (ConfigurationManager.AppSettings["ImportCommercialText"] != null)
              {
                if (bool.Parse(ConfigurationManager.AppSettings["ImportCommercialText"].ToString()))
                {
                  backendDescription = longDescription;

                  //shortDescription = r.Element("Content").Attribute("ShortContentDescription").Value;
                  if (!string.IsNullOrEmpty(r.Element("Content").Attribute("LongContentDescription").Value))
                    longDescription = r.Element("Content").Attribute("LongContentDescription").Value;

                }
              }

              if (ConfigurationManager.AppSettings["ConcatBrandName"] != null)
              {
                if (bool.Parse(ConfigurationManager.AppSettings["ConcatBrandName"].ToString()))
                {
                  StringBuilder sb = new StringBuilder();
                  sb.Append(r.Element("Brand").Element("Name").Value);
                  sb.Append(" ");
                  sb.Append(r.Element("Content").Attribute("ShortDescription").Value);
                  shortDescription = sb.ToString();
                }
              }

              int? taxRateID = (from t in context.TaxRates
                                where t.TaxRate1.HasValue
                                && (t.TaxRate1.Value * 100) == decimal.Parse(r.Element("Price").Attribute("TaxRate").Value, CultureInfo.InvariantCulture)
                                select t.TaxRateID).SingleOrDefault();

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

                if (brands.ContainsKey(r.Element("Brand").Element("Code").Value.Trim()))
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
                        || existingProducts[customproductID].ProductStatus != r.Attribute("CommercialStatus").Value
                        || existingProducts[customproductID].ManufacturerID != r.Attribute("ManufacturerID").Value
                        || existingProducts[customproductID].IsVisible != true
                        || existingProducts[customproductID].TaxRateID != taxRateID.Value
                        || existingProducts[customproductID].BrandID != brands[r.Element("Brand").Element("Code").Value.Trim()].BrandID
                        || (pdDate.HasValue && (!existingProducts[customproductID].PromisedDeliveryDate.HasValue || existingProducts[customproductID].PromisedDeliveryDate.Value.CompareTo(pdDate.Value) != 0))
                        || existingProducts[customproductID].LineType != r.Attribute("LineType").Value.Trim()
                        || existingProducts[customproductID].LedgerClass != r.Element("ShopInformation").Element("LedgerClass").Value.Trim()
                        || existingProducts[customproductID].ProductDesk != r.Element("ShopInformation").Element("ProductDesk").Value
                        || existingProducts[customproductID].ExtendedCatalog != extend
                        || existingProducts[customproductID].UnitCost != decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture))
                      {
                        product.ProductGroupID = productgroup.ProductGroupID;
                        product.ShortDescription = shortDescription;
                        product.LongDescription = longDescription;
                        product.BrandID = brands[r.Element("Brand").Element("Code").Value.Trim()].BrandID;
                        product.UnitPrice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                        product.ManufacturerID = r.Attribute("ManufacturerID").Value;
                        product.ProductStatus = r.Attribute("CommercialStatus").Value;
                        product.LastModificationTime = DateTime.Now;
                        product.IsVisible = true;
                        product.TaxRateID = taxRateID.Value;
                        product.PromisedDeliveryDate = pdDate;
                        product.LineType = r.Attribute("LineType").Value.Trim();
                        product.LedgerClass = r.Element("ShopInformation").Element("LedgerClass").Value.Trim();
                        product.ProductDesk = r.Element("ShopInformation").Element("ProductDesk").Value;
                        product.ExtendedCatalog = extend;
                        product.UnitCost = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);
                        //if (!string.IsNullOrEmpty(backendDescription))
                        //product.BackEndDescription = backendDescription;
                      }
                    }
                    catch (Exception ex)
                    {
                      log.Error("serious error", ex);
                    }
                  }
                  else
                  {
                    if (r.Element("ProductGroups").Element("SubProductGroup") != null)
                      log.DebugFormat("Productgroup does not exists, backendproductgroup {0}", r.Element("ProductGroups").Element("SubProductGroup").Attribute("SubProductGroupID").Value);
                    else
                      log.DebugFormat("Productgroup not matched for product {0}", customproductID);

                    product.IsVisible = false;
                    log.DebugFormat("Set Product {0} false, update price to {1}", customproductID, decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture));
                    product.ProductStatus = r.Attribute("CommercialStatus").Value;
                    product.UnitCost = product.UnitCost = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);
                    product.LineType = r.Attribute("LineType").Value.Trim();
                    product.LedgerClass = r.Element("ShopInformation").Element("LedgerClass").Value.Trim();
                    product.ProductDesk = r.Element("ShopInformation").Element("ProductDesk").Value;
                    product.LastModificationTime = DateTime.Now;
                    product.UnitPrice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                  }
                }
                else
                {
                  log.DebugFormat("Update product failed, brand {0} not availible", r.Element("Brand").Element("Code").Value.Trim());
                }
              }
              else
              {
                if (!customerItemNumbers.ContainsKey(r.Attribute("CustomProductID").Value))
                {
                  decimal unitprice = decimal.Parse(r.Element("Price").Element("UnitPrice").Value, CultureInfo.InvariantCulture);
                  decimal costprice = decimal.Parse(r.Element("Price").Element("CostPrice").Value, CultureInfo.InvariantCulture);

                  if (brands.ContainsKey(r.Element("Brand").Element("Code").Value.Trim()))
                  {

                    product = new Product
                                 {
                                   ProductID = int.Parse(r.Attribute("CustomProductID").Value),
                                   ProductGroupID = productgroup.ProductGroupID,
                                   ShortDescription = shortDescription,
                                   LongDescription = longDescription,
                                   ManufacturerID = r.Attribute("ManufacturerID").Value,
                                   BrandID = brands[r.Element("Brand").Element("Code").Value.Trim()].BrandID,
                                   TaxRateID = taxRateID.Value,
                                   UnitPrice = unitprice,
                                   UnitCost = costprice,
                                   IsCustom = false,
                                   LineType = r.Attribute("LineType").Value.Trim(),
                                   ProductStatus = r.Attribute("CommercialStatus").Value,
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
                    log.DebugFormat("Insert product failed, brand {0} not availible", r.Element("Brand").Element("Code").Value.Trim());
                  }
                }
              }

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

    private void ImportStock()
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
        {
          var ids = (from r in products.Root.Elements("Product")
                     select int.Parse(r.Attribute("CustomProductID").Value)).Distinct().ToArray();

          Dictionary<int, List<VendorStock>> existingStock = new Dictionary<int, List<VendorStock>>();

          foreach (int id in ids)
          {
            var stk = (from c in context.VendorStocks
                       where c.ProductID == id
                       select c).ToList();

            if (stk.Count() > 0)
            {
              if (!existingStock.ContainsKey(stk.First().ProductID))
                existingStock.Add(stk.First().ProductID, stk);
              else
              {
                foreach (VendorStock vs in stk)
                {
                  if (!existingStock[stk.First().ProductID].Contains(vs))
                    existingStock[stk.First().ProductID].Add(vs);
                }
              }
            }
          }

          List<int> addedstock = new List<int>();

          foreach (var r in products.Root.Elements("Product"))
          {
            int custom = int.Parse(r.Attribute("CustomProductID").Value);
            if (!addedstock.Contains(custom))
            {

              VendorStock stock = null;
              if (existingStock.ContainsKey(int.Parse(r.Attribute("CustomProductID").Value)))
              {
                stock = existingStock[int.Parse(r.Attribute("CustomProductID").Value)].Where(x => x.StockLocationID == "BAS Distributie").SingleOrDefault();
                if (stock != null)
                {
                  if (stock.QuantityOnHand != int.Parse(r.Element("Stock").Attribute("InStock").Value))
                  {
                    stock.QuantityOnHand = int.Parse(r.Element("Stock").Attribute("InStock").Value);
                    stock.LastModificationTime = DateTime.Now;
                  }
                }
              }
              else
              {
                stock = new VendorStock();
                stock.CreationTime = DateTime.Now;
                stock.LastModificationTime = DateTime.Now;
                stock.CreatedBy = 0;
                stock.LastModifiedBy = 0;
                stock.ProductID = custom;
                stock.RelationID = 962542;
                stock.VendorProductStatus = "S";
                stock.StockLocationID = "BAS Distributie";
                stock.QuantityOnHand = int.Parse(r.Element("Stock").Attribute("InStock").Value);
                context.VendorStocks.InsertOnSubmit(stock);
              }
              addedstock.Add(custom);
            }
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

    private void ImportBrands()
    {
      try
      {
        using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
        {
          List<string> CreatedBrands = new List<string>();

          var ids = (from r in products.Root.Elements("Product")
                     select r.Element("Brand").Element("Code").Value).Distinct().ToArray();

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

          foreach (var r in products.Root.Elements("Product").Elements("Brand").Distinct())
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

    private ProductBarcode InsertBarcode(int productID, string barcode, string barcodeType)
    {
      ProductBarcode productBarcode = new ProductBarcode();
      productBarcode.LastModificationTime = DateTime.Now;
      productBarcode.ProductID = productID;
      productBarcode.BarcodeType = barcodeType;
      productBarcode.Barcode = barcode;
      return productBarcode;
    }

    private void ImportBarcode()
    {
      using (ShopDataContext context = new ShopDataContext(ConfigurationManager.ConnectionStrings["Staging"].ConnectionString))
      {
        var ids = (from r in products.Root.Elements("Product")
                   select int.Parse(r.Attribute("CustomProductID").Value)).Distinct().ToArray();

        var barcodes = (from b in context.ProductBarcodes
                        select b).ToList();

        Dictionary<int, ProductBarcode> existingBarcode = new Dictionary<int, ProductBarcode>();

        for (int i = barcodes.Count - 1; i >= 0; i--)
        {
          ProductBarcode bar = barcodes[i];

          var pr = (from p in products.Root.Elements("Product")
                    where p.Attribute("CustomProductID").Value == bar.ProductID.ToString()
&& p.Elements("Barcodes").Where(x => x.Element("Barcode").Value.Trim() == bar.Barcode).Count() > 0
                    select p).FirstOrDefault();

          if (pr == null)
          {
            log.InfoFormat("Delete productbarcode item {0} ({1})", bar.ProductID, bar.Barcode);

            context.ProductBarcodes.DeleteOnSubmit(bar);
            context.SubmitChanges();
          }
        }

        barcodes = (from b in context.ProductBarcodes
                    select b).ToList();

        foreach (int id in ids)
        {
          var stk = (from c in barcodes
                     where c.ProductID == id
                     select c).FirstOrDefault();

          if (stk != null)
            existingBarcode.Add(stk.ProductID, stk);
        }

        List<ProductBarcode> barcodeList = new List<ProductBarcode>();

        foreach (var prod in products.Root.Elements("Product"))
        {
          try
          {
            foreach (var bar in prod.Elements("Barcodes"))
            {
              if (bar.Element("Barcode") != null && !string.IsNullOrEmpty(bar.Element("Barcode").Value))
              {
                string barcodecode = bar.Element("Barcode").Value.Trim();
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
                if (existingBarcode.ContainsKey(int.Parse(prod.Attribute("CustomProductID").Value)))
                {
                  int custom = int.Parse(prod.Attribute("CustomProductID").Value);
                  if (existingBarcode[custom].Barcode.Trim() != barcodecode
                    || existingBarcode[custom].BarcodeType.Trim() != barcodetype)
                  {
                    barcode = existingBarcode[custom];
                    context.ProductBarcodes.DeleteOnSubmit(barcode);
                    context.SubmitChanges();

                    if (barcodeList.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1
                      && barcodes.Where(x => x.Barcode.Trim() == barcodecode).Count() < 1)
                    {
                      barcode = InsertBarcode(int.Parse(prod.Attribute("CustomProductID").Value), barcodecode, barcodetype);
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
                    barcode = InsertBarcode(int.Parse(prod.Attribute("CustomProductID").Value), barcodecode, barcodetype);
                    context.ProductBarcodes.InsertOnSubmit(barcode);
                    context.SubmitChanges();
                    barcodeList.Add(barcode);
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

