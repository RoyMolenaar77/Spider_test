﻿using System;
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
using Concentrator.Web.ServiceClient.AssortmentService;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.BAS.WebExport
{

  [ConnectorSystem(1)]
  public class ImportImages : ConcentratorPlugin
  {
    private XDocument images;
    private string connectionStringName;


    public override string Name
    {
      get { return "Website Image Export Plugin"; }
    }

    protected override void Process()
    {

      try
      {
        foreach (Connector connector in base.Connectors.Where(c => ((ConnectorType)c.ConnectorType).Has(ConnectorType.Images)))
        {  
          log.DebugFormat("Start Process Image import for {0}", connector.Name);
          DateTime start = DateTime.Now;

          AssortmentServiceSoapClient soap = new AssortmentServiceSoapClient();
          images = new XDocument(soap.GetAssortmentImages(connector.ConnectorID));
          connectionStringName = connector.Connection;

          log.Info("Start Brand images");
          ProcessBrandImages(connector);
          log.Info("Finished Brand images");
          log.Info("Start Productgroup images");
          ProcessProductGroupImages(connector);
          log.Info("Finished Productgroup images");
          log.Info("Start Product Images");
          ProcessProductImages(connector);
          log.Info("Finished Product Images");

          //log.Info("Start Product O Images");
          //ProcessOProductImages(connector.ConnectorID, soap);
          //log.Info("Finished Product O Images");
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
        var productImages = (from r in images.Root.Elements("Products").Elements("ProductImage")
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

        List<ImageStore> existingProducts = new List<ImageStore>();

        foreach (var prod in productImages)
        {
          var websiteProduct = (from p in websiteProducts
                                where p.ConcentratorProductID.Value == prod.ConcentratorProductID
                                select p).FirstOrDefault();

          if (websiteProduct != null)
          {
            var product = (from p in ImageDB
                           where p.CustomerProductID == websiteProduct.ProductID.ToString()
                           && p.Sequence == int.Parse(prod.Sequence)
                           select p).FirstOrDefault();

            if (product != null)
              existingProducts.Add(product);
          }
        }

        int counter = productImages.Count();
        int showMessage = 0;
        string imageDirectory = GetConfiguration().AppSettings.Settings["ImageDirectory"].Value;
        foreach (var r in images.Root.Elements("Products").Elements("ProductImage").OrderByDescending(x => x.Attribute("ProductID").Value))
        {
          var websiteProduct = (from p in websiteProducts
                                where p.ConcentratorProductID.Value == int.Parse(r.Attribute("ProductID").Value)
                                select p).FirstOrDefault();

          try
          {
            if (!string.IsNullOrEmpty(r.Value) && websiteProduct != null)
            {
              string url = GetImage(r.Value, ImageType.ProductImage, r.Attribute("ProductID").Value, int.Parse(r.Attribute("Sequence").Value), imageDirectory);

              if (!string.IsNullOrEmpty(url))
              {
                ImageStore productImage = existingProducts.Where(x => x.Sequence == int.Parse(r.Attribute("Sequence").Value)
                  && x.CustomerProductID == websiteProduct.ProductID.ToString()).FirstOrDefault();

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

    private void ProcessProductGroupImages(Connector connector)
    {
      using (WebsiteDataContext context = new WebsiteDataContext(connector.ConnectionString))
      {
        var productGroupImages = (from r in images.Root.Elements("ProductGroups").Elements("ProductGroupImage")
                                  select int.Parse(r.Attribute("ProductGroupID").Value)).ToArray();

        var existingProducts = (from c in context.ImageStores
                                where c.ProductGroupID.HasValue
                                  && productGroupImages.Contains(c.ProductGroupID.Value)
                                   && c.ManufacturerID == null
                                && c.CustomerProductID == null
                                select c).ToDictionary(c => c.ProductGroupID.Value, c => c);

        string imageDirectory = GetConfiguration().AppSettings.Settings["ImageDirectory"].Value;
        foreach (var r in images.Root.Elements("ProductGroups").Elements("ProductGroupImage"))
        {
          //log.DebugFormat("Get ProductGroup for {0} url: {1}", r.Attribute("ProductGroupID").Value, r.Value);
          try
          {
            if (!string.IsNullOrEmpty(r.Value))
            {
              string url = GetImage(r.Value, ImageType.ProductGroupImage, r.Attribute("ProductGroupID").Value, null, imageDirectory);

              if (!string.IsNullOrEmpty(url))
              {
                ImageStore productGroupImage = null;
                if (existingProducts.ContainsKey(int.Parse(r.Attribute("ProductGroupID").Value)))
                {
                  productGroupImage = existingProducts[int.Parse(r.Attribute("ProductGroupID").Value)];
                  productGroupImage.ImageUrl = url;
                }
                else
                {
                  productGroupImage = new ImageStore()
                  {
                    ProductGroupID = int.Parse(r.Attribute("ProductGroupID").Value),
                    ImageUrl = url,
                    ImageType = ImageType.ProductGroupImage.ToString()
                  };
                  context.ImageStores.InsertOnSubmit(productGroupImage);
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
              string url = GetImage(r.Value, ImageType.BrandImage, r.Attribute("BrandVendorCode").Value, null, imageDirectory);

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
