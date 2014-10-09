using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using System.Xml.Linq;
using System.Configuration;
using System.Xml;
using System.Drawing;
using System.Net;
using System.IO;
using System.Drawing.Imaging;
using Concentrator.Objects;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{

  [ConnectorSystem(1)]
  public class ImportFTPImages : ConcentratorPlugin
  {
    private XDocument images;
    private string connectionStringName;


    public override string Name
    {
      get { return "Website FTP Image Export Plugin"; }
    }

    protected override void Process()
    {

      try
      {
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.Images)))
        {
          log.DebugFormat("Start Process FTP Image import for {0}", connector.Name);
          DateTime start = DateTime.Now;

          Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient();
          images = new XDocument(soap.GetFTPAssortmentImages(connector.ConnectorID));
          connectionStringName = connector.Connection;

          log.Info("Start Brand images");
          ProcessBrandImages(connector);
          log.Info("Finished Brand images");

          log.Info("Start Product Images");
          ProcessProductImages(connector);
          log.Info("Finished Product Images");

          log.DebugFormat("Finished Process Image import for {0}", connector.Name);

        }
      }
      catch (Exception ex)
      {
        log.Fatal(ex);
      }
    }

    private void ProcessProductImages(Connector connector)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(connector.ConnectionString))
      {
        var productImages = (from r in images.Root.Elements("Products").Elements("ProductMedia")
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

        var unusedImages = ImageDB;

        var brands = (from b in context.Brands
                      select b).ToList();

        List<ImageStore> existingProducts = new List<ImageStore>();

        foreach (var prod in productImages)
        {
          var websiteProduct = (from p in websiteProducts
                                where p.ConcentratorProductID.Value == prod.ConcentratorProductID
                                select p).OrderByDescending(x => x.IsVisible).FirstOrDefault();

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
        bool succes = true;
        //string imageDirectory = GetConfiguration().AppSettings.Settings["ImageDirectory"].Value;
        foreach (var r in images.Root.Elements("Products").Elements("ProductMedia").OrderByDescending(x => x.Attribute("ProductID").Value))
        {
          var websiteProduct = (from p in websiteProducts
                                where p.ConcentratorProductID.Value == int.Parse(r.Attribute("ProductID").Value)
                                select p).OrderByDescending(x => x.IsVisible).FirstOrDefault();

          try
          {
            if (!string.IsNullOrEmpty(r.Value) && websiteProduct != null)
            {
              //string url = GetImage(r.Value, ImageType.ProductImage, r.Attribute("ProductID").Value, int.Parse(r.Attribute("Sequence").Value), imageDirectory);
              string url = r.Value;

              if (!string.IsNullOrEmpty(url))
              {
                ImageStore productImage = existingProducts.Where(x => x.ConcentratorProductID.Value == int.Parse(r.Attribute("ProductID").Value) && x.Sequence == int.Parse(r.Attribute("Sequence").Value)).FirstOrDefault();

                if (productImage != null)
                {
                  if (productImage.ImageUrl != url
                   || productImage.BrandID != int.Parse(r.Attribute("BrandID").Value)
                  || productImage.ManufacturerID != r.Attribute("ManufacturerID").Value
                  || productImage.Sequence != int.Parse(r.Attribute("Sequence").Value)
                  || productImage.ConcentratorProductID != int.Parse(r.Attribute("ProductID").Value)
                  )
                  {
                    productImage.ImageUrl = url;
                    productImage.BrandID = int.Parse(r.Attribute("BrandID").Value);
                    productImage.ManufacturerID = r.Attribute("ManufacturerID").Value;
                    productImage.Sequence = int.Parse(r.Attribute("Sequence").Value);
                    productImage.ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value);
                    productImage.LastModificationTime = DateTime.Now;
                  }
                  unusedImages.Remove(productImage);
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
                    ConcentratorProductID = int.Parse(r.Attribute("ProductID").Value),
                    LastModificationTime = DateTime.Now
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
            context.SubmitChanges();
          }
          catch (Exception ex)
          {
            succes = false;
            log.Error(ex);
          }
        }

        try
        {
          if (succes)
          {
            if (Processor.TableExists(context, "AuctionProducts"))
            {
              var auctionImages = (from a in context.AuctionProducts
                                   join i in context.ImageStores on a.Product.ConcentratorProductID equals i.ConcentratorProductID
                                   select i).ToList();
              unusedImages = unusedImages.Except(auctionImages).ToList();
            }

            if (Processor.TableExists(context, "RelatedProducts"))
            {
              var relatedproducts = (from r in context.RelatedProducts
                                     join i in context.ImageStores on r.Product.ConcentratorProductID equals i.ConcentratorProductID
                                     select i).ToList();

              unusedImages = unusedImages.Except(relatedproducts).ToList();
            }

            log.DebugFormat("Try To delete {0} product images", unusedImages.Count);

            context.ImageStores.DeleteAllOnSubmit(unusedImages);
            context.SubmitChanges();
          }
        }
        catch (Exception ex)
        {
          log.Error("Error delete unused images", ex);
        }
      }
    }

    private void ProcessBrandImages(Connector connector)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(connector.ConnectionString))
      {
        var brandImages = (from r in images.Root.Elements("Brands").Elements("BrandMedia")
                           select int.Parse(r.Attribute("BrandID").Value)).Distinct().ToArray();

        var brands = (from b in context.Brands
                      select b).ToList();


        var existingProducts = (from c in context.ImageStores
                                where c.ImageType == ImageType.BrandImage.ToString()
                                select c).ToList();

        var unusedImages = existingProducts;

        string imageDirectory = GetConfiguration().AppSettings.Settings["ImageDirectory"].Value;
        foreach (var r in images.Root.Elements("Brands").Elements("BrandMedia"))
        {
          //log.DebugFormat("Get BrandImage for {0} url: {1}", r.Attribute("BrandVendorCode").Value, r.Value);
          try
          {
            if (!string.IsNullOrEmpty(r.Value))
            {
              //string url = GetImage(r.Value, ImageType.BrandImage, r.Attribute("BrandID").Value, null, imageDirectory);
              string url = r.Value;

              if (!string.IsNullOrEmpty(url))
              {
                ImageStore brandImage = existingProducts.FirstOrDefault(x => x.BrandCode == r.Attribute("BrandID").Value);
                if (brandImage != null)
                {
                  if (
                    brandImage.ImageUrl != url
                  || brandImage.BrandCode != r.Attribute("BrandID").Value
                  || brandImage.BrandID != brands.Where(x => x.BrandCode == r.Attribute("BrandID").Value).Select(x => x.BrandID).FirstOrDefault()
                    )
                  {
                    brandImage.ImageUrl = url;
                    brandImage.BrandCode = r.Attribute("BrandID").Value;
                    brandImage.BrandID = brands.Where(x => x.BrandCode == r.Attribute("BrandID").Value).Select(x => x.BrandID).FirstOrDefault();
                    brandImage.LastModificationTime = DateTime.Now;
                  }
                  unusedImages.Remove(brandImage);
                }
                else
                {
                  brandImage = new ImageStore()
                   {
                     BrandID = brands.Where(x => x.BrandCode == r.Attribute("BrandID").Value).Select(x => x.BrandID).FirstOrDefault(),
                     BrandCode = r.Attribute("BrandID").Value,
                     ImageUrl = url,
                     ImageType = ImageType.BrandImage.ToString(),
                     LastModificationTime = DateTime.Now
                   };
                  context.ImageStores.InsertOnSubmit(brandImage);
                }
              }
            }
          }
          catch (Exception ex)
          {
            log.Error(ex);
          }
        }
        context.SubmitChanges();

        //try
        //{
        //  log.DebugFormat("Try To delete {0} brand images", unusedImages.Count);
        //  context.ImageStores.DeleteAllOnSubmit(unusedImages);
        //  context.SubmitChanges();
        //}
        //catch (Exception ex)
        //{
        //  log.Error("Error delete unused images", ex);
        //}

      }
    }

    public static string GetImage(string url, ImageType type, string fileName, int? sequence, string imageDirectory)
    {
      // http://support.microsoft.com/?id=814675
      Image _productImage;
      string path = string.Empty;
      string name = string.Empty;
      string file = string.Empty;
      switch (type)
      {
        case ImageType.ProductGroupImage:
          name = Path.Combine("ProductGroup", fileName);
          path = Path.Combine(imageDirectory, "ProductGroup");
          break;
        case ImageType.ProductImage:
          name = Path.Combine("Product", fileName);
          path = Path.Combine(imageDirectory, "Product");
          //name = imgClass.CustomProductID;
          break;
        case ImageType.BrandImage:
          name = Path.Combine("Brand", fileName);
          path = Path.Combine(imageDirectory, "Brand");
          break;
      }

      if (sequence != null && sequence > 0)
        name += "_" + sequence;

      name += ".jpg";

      file = Path.Combine(imageDirectory, name);

      if (!File.Exists(file))
      {
        try
        {
          byte[] b;
          HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
          WebResponse myResp = myReq.GetResponse();

          using (Stream stream = myResp.GetResponseStream())
          {

            using (BinaryReader br = new BinaryReader(stream))
            {
              b = br.ReadBytes((int)myResp.ContentLength);
              br.Close();
            }
            myResp.Close();

            using (MemoryStream ms = new MemoryStream(b))
            {
              try
              {
                using (Image originalImage = Image.FromStream(ms, true, true))
                {
                  Image i = Image.FromStream(ms);
                  Bitmap bit = new Bitmap(new Bitmap(i));

                  //_productImage = new Bitmap(originalImage.Width, originalImage.Height);

                  // bit.Save(url);
                  if (!Directory.Exists(path))
                  {
                    Directory.CreateDirectory(path);
                  }

                  i.Save(file, ImageFormat.Jpeg);

                }
              }
              catch (ArgumentException)
              {
                _productImage = new Bitmap(1, 1);
              }
            }
          }
        }
        catch (Exception ex)
        {
          return string.Empty;
        }
      }
      return name;
    }
  }
}
