using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using System.Xml.Linq;
using System.Xml;
using System.Globalization;
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Web.ServiceClient.SelectorService;
using System.Transactions;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{
  [ConnectorSystem(1)]
  public class ImportSelectors : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Import Selectors"; }
    }

    protected override void Process()
    {
      log.Info("Selector import cycle started...");
      try
      {
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.WebAssortment) && c.Selectors))
        {
          
          DateTime start = DateTime.Now;
          AssortmentServiceSoapClient contentClient = new AssortmentServiceSoapClient();
          SelectorServiceSoapClient serviceClient = new SelectorServiceSoapClient();

          using (
            WebsiteDataContext context =
              new WebsiteDataContext(
                ConfigurationManager.ConnectionStrings[connector.Connection].ConnectionString))
          {
            log.InfoFormat("Start selector import for connector {0}", connector.Name);

            var selectorIDs = (from s in context.Selectors select s.SelectorID).ToList();

            foreach (int selectorID in selectorIDs)
            {
              var products = XDocument.Parse(contentClient.GetSelectorProducts(connector.ConnectorID, selectorID));

              Processor processor = new Processor(products, log, connector);

              log.Debug("Start import brands");
              processor.ImportBrands();
              log.Debug("Finished import brands");
              log.Debug("Start import productgroups");
              processor.ImportProductGroups(false);
              log.Debug("Finished import productgroups");
              log.Debug("Start import Products");
              processor.ImportSelectorProducts();
              log.Debug("Finished import Products");

              log.Debug("Start import Attributes");
              processor.processSelectorAttributes(connector);
              log.Debug("Finished import Attributes");

              log.Debug("Start import images");
              ProcessProductImages(connector, products);
              log.Debug("Finish import images");

              //processor.CleanUpProducts();
              //
            }

            try
            {
              log.InfoFormat("Starting import of related products");
              ImportRelatedProducts(context, serviceClient, contentClient, connector);
              log.InfoFormat("Finished import of related products");


            }
            catch (Exception e)
            {
              log.Error("Importing of related products for connector " + connector.Name + "failed ", e);
            }

            try
            {
              using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Suppress, TimeSpan.FromMinutes(5)))
              {
                context.SubmitChanges();
                scope.Complete();
              }
              
              log.AuditSuccess("Finished selector export for connector " + connector.Name, "Selector export");
            }
            catch (Exception e)
            {
              log.AuditFatal("There was an error inserting the selectors", e);
            }
          }
        }

      }
      catch (Exception e)
      {
        log.Fatal("Selector import cycle failed", e);
      }
    }

    private void ProcessSelectors(WebsiteDataContext context, XmlElement selectors)
    {
      var selectorCollection = XDocument.Parse(selectors.OuterXml).Element("selectors").Elements("selector");

      foreach (var selector in selectorCollection)
      {
        #region selector

        int? selectorID = selector.Attribute("id").Try<XAttribute, int?>(c => int.Parse(c.Value), null);

        if (!selectorID.HasValue)
        {
          continue;
        }
        Selector sel = (from s in context.Selectors
                        where s.SelectorID == selectorID
                        select s).FirstOrDefault();
        if (sel == null)
        {
          sel = new Selector()
                  {
                    SelectorID = selectorID.Value
                  };
          context.Selectors.InsertOnSubmit(sel);
        }
        sel.Name = selector.Value;

        #endregion
      }
    }

    private void ProcessProductImages(Connector connector, XDocument images)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(connector.ConnectionString))
      {
        var productImages = (from r in images.Root.Elements("Product").Elements("ProductImages").Elements("ProductImage")
                             select new
                             {
                               ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value),
                               Sequence = r.Attribute("Sequence").Value
                             }).ToList().OrderByDescending(x => x.ConcentratorProductID);

        var websiteProducts = (from p in context.Products
                               where p.ConcentratorProductID.HasValue
                               select p).ToList();

        var ImageDB = (from i in context.ImageStores
                       where i.ImageType == ImageType.ProductImage.ToString()
                       select i).ToList();

        var brands = (from b in context.Brands
                      select b).ToList();

        List<ImageStore> existingProducts = new List<ImageStore>();

        foreach (var prod in productImages)
        {
          var websiteProduct = (from p in websiteProducts
                                where p.ConcentratorProductID.Value == prod.ConcentratorProductID
                                select p).FirstOrDefault();

          if (websiteProduct != null)
          {
            var product = (from p in ImageDB
                           where p.ConcentratorProductID == prod.ConcentratorProductID
                           && p.Sequence == int.Parse(prod.Sequence)
                           select p).FirstOrDefault();

            if (product != null)
              existingProducts.Add(product);
          }
        }

        int counter = productImages.Count();
        int showMessage = 0;
        //string imageDirectory = GetConfiguration().AppSettings.Settings["ImageDirectory"].Value;
        foreach (var r in images.Root.Elements("Product").Elements("ProductImages").Elements("ProductImage").OrderByDescending(x => x.Attribute("ProductID").Value))
        {
          var websiteProduct = (from p in websiteProducts
                                where p.ConcentratorProductID.Value == int.Parse(r.Attribute("ProductID").Value)
                                select p).FirstOrDefault();

          try
          {
            if (!string.IsNullOrEmpty(r.Value) && websiteProduct != null)
            {
              //string url = GetImage(r.Value, ImageType.ProductImage, r.Attribute("ProductID").Value, int.Parse(r.Attribute("Sequence").Value), imageDirectory);
              string url = r.Value;

              if (!string.IsNullOrEmpty(url))
              {
                ImageStore productImage = existingProducts.Where(x => x.ImageUrl == url && x.ConcentratorProductID.Value == int.Parse(r.Attribute("ProductID").Value) && x.Sequence == int.Parse(r.Attribute("Sequence").Value)).FirstOrDefault();

                if (productImage != null)
                {
                  productImage.ImageUrl = url;
                  productImage.BrandID = int.Parse(r.Attribute("BrandID").Value);
                  productImage.ManufacturerID = r.Attribute("ManufacturerID").Value;
                  productImage.Sequence = int.Parse(r.Attribute("Sequence").Value);
                  productImage.ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);
                }
                else
                {
                  productImage = new ImageStore()
                  {
                    ImageUrl = url,
                    BrandID = int.Parse(r.Attribute("BrandID").Value),
                    CustomerProductID = websiteProduct.ProductID.ToString(),
                    ManufacturerID = r.Attribute("ManufacturerID").Value,
                    ImageType = ImageType.ProductImage.ToString(),
                    Sequence = int.Parse(r.Attribute("Sequence").Value),
                    ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value)
                  };
                  context.ImageStores.InsertOnSubmit(productImage);
                }
              }
              counter--;
              showMessage++;
              if (showMessage == 500)
              {
                log.InfoFormat("{0} images to process", counter);
                showMessage = 0;
              }
            }
          }
          catch (Exception ex)
          {
            log.Error(ex);
          }
          context.SubmitChanges();
        }
        // context.SubmitChanges();

      }
    }

    public void ImportRelatedProducts(WebsiteDataContext context, SelectorServiceSoapClient client, Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient contentClient, Connector connector)
    {
      var selectorIDs = (from s in context.Selectors select s.SelectorID).ToList();
      Processor processor = new Processor(new XDocument(), log, connector);

      foreach (int selectorID in selectorIDs)
      {
        var selectorProducts = client.GetAllSelectorProducts(selectorID, connector.ConnectorID);
        XDocument comparableProducts = XDocument.Parse(selectorProducts);
        int counter = 0;
        int total = comparableProducts.Root.Elements("RelatedProduct").Count();

        List<Product> products = (from p in context.Products
                                  select p).ToList();

        int totalRelatedproducts = total;

        foreach (var rProduct in comparableProducts.Root.Elements("RelatedProduct"))
        {
          if (counter == 50)
          {
            base.log.InfoFormat("Still need to process {0} of {1}; {2} done;", total, totalRelatedproducts, totalRelatedproducts - total);
            counter = 0;
          }
          total--;
          counter++;
          int concentratorProductID = int.Parse(rProduct.Attribute("ProductID").Value);

          Product product = null;
          //try
          //{
          product = (from p in products
                     where p.ConcentratorProductID.HasValue && (p.ConcentratorProductID.Value == concentratorProductID)
                     select p).FirstOrDefault();

          if (product == null)
            continue;

          foreach (var comparableProduct in rProduct.Elements("ComparableProducts").Elements("ComparableProduct"))
          {
            int relatedProductID = int.Parse(comparableProduct.Attribute("ProductID").Value);
            try
            {
              Product relatedProduct = (from p in products
                                        where p.ConcentratorProductID.HasValue && (p.ConcentratorProductID.Value == relatedProductID)
                                        select p).FirstOrDefault();

              if (relatedProduct == null) continue;

              //insert related product 
              RelatedProduct relatedProd = (from rp in context.RelatedProducts
                                            where
                                            rp.ProductID == product.ProductID
                                            &&
                                            rp.RelatedProductID == relatedProduct.ProductID
                                            &&
                                            rp.SelectorID == selectorID
                                            select rp).FirstOrDefault();

              if (relatedProd == null)
                context.RelatedProducts.InsertOnSubmit(new RelatedProduct
                                                         {
                                                           ProductID = product.ProductID,
                                                           RelatedProductID = relatedProduct.ProductID,
                                                           SelectorID = selectorID
                                                         });
              context.SubmitChanges();
            }
            catch (Exception ex)
            {
              log.ErrorFormat("Error process product {0} error {1}", relatedProductID, ex.StackTrace);
            }
          }
        }

        #region Cleanup

        var itemNumbers = comparableProducts.Root.Elements("RelatedProduct").ToDictionary(x => x.Attribute("ProductID").Value, x => x);


        List<RelatedProduct> unused = (from c in context.RelatedProducts
                                       select c).ToList();

        foreach (var p in itemNumbers)
        {
          foreach (var v in p.Value.Elements("ComparableProducts").Elements("ComparableProduct"))
          {
            unused.RemoveAll(c => (c.Product.ConcentratorProductID.HasValue && c.Product.ConcentratorProductID.Value.ToString() == p.Key) && (c.Product1.ConcentratorProductID.HasValue && c.Product1.ConcentratorProductID.Value.ToString() == v.Attribute("ProductID").Value));
          }
        }

        #endregion
        if (unused != null && unused.Count > 0)
        {
          context.RelatedProducts.DeleteAllOnSubmit(unused);

          context.SubmitChanges();
        }


      }


    }
  }

  public class WebsiteProduct
  {
    public string BrandCode { get; set; }
    public Product Product { get; set; }
  }
}
