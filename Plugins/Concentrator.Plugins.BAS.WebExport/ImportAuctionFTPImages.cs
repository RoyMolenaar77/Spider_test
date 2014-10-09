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

  [ConnectorSystem(3)]
  public class ImportAuctionFTPImages : ConcentratorPlugin
  {
    private XDocument images;
    private string connectionStringName;


    public override string Name
    {
      get { return "Website Auction FTP Image Export Plugin"; }
    }

    protected override void Process()
    {

      try
      {
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.Images)))
        {
            
          log.DebugFormat("Start Process Auction FTP Image import for {0}", connector.Name);

          DateTime start = DateTime.Now;
          Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient soap = new Concentrator.Web.ServiceClient.AssortmentService.AssortmentServiceSoapClient();
          images = new XDocument(soap.GetFTPAssortmentImages(connector.ConnectorID));
          connectionStringName = connector.Connection;

          log.Info("Start Auction Product Images");
          ProcessProductImages(connector);
          log.Info("Finished Auction Product Images");

          
          log.DebugFormat("Finished Process Auction Image import for {0}", connector.Name);

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
        foreach (var r in images.Root.Elements("Products").Elements("ProductMedia").OrderByDescending(x => x.Attribute("ProductID").Value))
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
                                select c).Distinct().ToDictionary(c => c.BrandCode, c => c);

        string imageDirectory = GetConfiguration().AppSettings.Settings["ImageDirectory"].Value;
        foreach (var r in images.Root.Elements("Brands").Elements("BrandMedia"))
        {
          //log.DebugFormat("Get BrandImage for {0} url: {1}", r.Attribute("BrandVendorCode").Value, r.Value);
          try
          {
            if (!string.IsNullOrEmpty(r.Value))
            {
              string url = GetImage(r.Value, ImageType.BrandImage, r.Attribute("BrandID").Value, null, imageDirectory);

              if (!string.IsNullOrEmpty(url))
              {
                ImageStore brandImage = null;
                if (existingProducts.ContainsKey(r.Attribute("BrandVendorCode").Value))
                {
                  brandImage = existingProducts[r.Attribute("BrandVendorCode").Value];
                  brandImage.ImageUrl = url;
                  brandImage.BrandCode = r.Attribute("BrandVendorCode").Value;
                  brandImage.BrandID = brands.Where(x => x.BrandCode == r.Attribute("BrandVendorCode").Value).Select(x => x.BrandID).FirstOrDefault();
                }
                else
                {
                  brandImage = new ImageStore()
                   {
                     BrandID = brands.Where(x => x.BrandCode == r.Attribute("BrandVendorCode").Value).Select(x => x.BrandID).FirstOrDefault(),
                     BrandCode = r.Attribute("BrandVendorCode").Value,
                     ImageUrl = url,
                     ImageType = ImageType.BrandImage.ToString()
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

#if DEBUG
      return name;
#endif

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
