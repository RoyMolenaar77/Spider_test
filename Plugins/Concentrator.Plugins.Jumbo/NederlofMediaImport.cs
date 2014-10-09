using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Data;
using Concentrator.Objects.Enumerations;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using System.Linq;
using System.Drawing;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Media;

namespace Concentrator.Plugins.Jumbo
{
  public class NederlofMediaImport : JumboBase
  {
    public override string Name
    {
      get { return "Jumbo Nederlof media import"; }
    }

    private const int UnMappedID = -1;

    protected override void Process()
    {
      int vendorID = int.Parse(GetConfiguration().AppSettings.Settings["NederlofVendorID"].Value);
      var dir = GetConfiguration().AppSettings.Settings["NederlofMediaFiles"].Value;

      List<NederLofData> productData = new List<NederLofData>();
      //string destinationPath = Path.Combine(GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value, "Products");

      bool networkDrive = false;
      string drive = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;
      //string drive = @"\\SOL\Company_Shares\Database Backup";
      bool.TryParse(GetConfiguration().AppSettings.Settings["IsNetworkDrive"].Value, out networkDrive);

      if (networkDrive)
      {
        NetworkDrive oNetDrive = new NetworkDrive();
        try
        {
          oNetDrive.LocalDrive = "H:";
          oNetDrive.ShareName = drive;
          //oNetDrive.MapDrive("diract", "D1r@ct379");
          oNetDrive.MapDrive();
          drive = "H:";
        }
        catch (Exception err)
        {
          log.Error("Invalid network drive", err);
        }
        oNetDrive = null;
      }
      string destinationPath = Path.Combine(drive, "Products");


      foreach (var f in Directory.GetFiles(dir))
      {
        XDocument xdoc = null;
        using (StreamReader reader = new StreamReader(f))
        {
          xdoc = XDocument.Parse(reader.ReadToEnd());
        };

        XNamespace xName = "http://www.graphit.nl/xmlns/netpublishpro/entity-info/1.4";

        //-		xdoc.Elements().Elements(xName + "object").First().Elements(xName + "field").FirstOrDefault().Element(xName + "value")	<value xmlns="http://www.graphit.nl/xmlns/netpublishpro/entity-info/1.4">15 mei 2008 9:21:21 CEST</value>	System.Xml.Linq.XElement

        var fields = xdoc.Elements().Elements(xName + "object").Elements(xName + "field").Select(x => x.Attribute("name").Value).Distinct().ToList();
        var itemFields = xdoc.Elements().Elements(xName + "object").Elements(xName + "items").Elements(xName + "item").Elements(xName + "field").Select(x => x.Attribute("name").Value).Distinct().ToList();
        var productInfoFields = xdoc.Elements().Elements(xName + "object").Elements(xName + "objects").Where(x => x.Attribute("classname").Value == "Jumbo_Product_lanquage").Elements(xName + "object").Elements(xName + "field").Select(x => x.Attribute("name").Value).Distinct().ToList();

        var products = (from pr in xdoc.Elements().Elements(xName + "object")
                        select new NederLofData
                        {
                          barcode = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Barcode") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Barcode").Element(xName + "value").Value : string.Empty,
                          VendorItemNumber = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Product code") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Product code").Element(xName + "value").Value : string.Empty,
                          Pieces = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Pieces") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Pieces").Element(xName + "value").Value : string.Empty,
                          productName = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Product name") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Product name").Element(xName + "value").Value : string.Empty,
                          Age = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Age") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Age").Element(xName + "value").Value : string.Empty,
                          Group = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Group") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Group").Element(xName + "value").Value : string.Empty,
                          NrOfPlayers = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Nr. of Players") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Nr. of Players").Element(xName + "value").Value : string.Empty,
                          Package = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Package") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Package").Element(xName + "value").Value : string.Empty,
                          Size3D = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Size 3D") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Size 3D").Element(xName + "value").Value : string.Empty,
                          Sound = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Sound") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Sound").Element(xName + "value").Value : string.Empty,
                          Type = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Type") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Type").Element(xName + "value").Value : string.Empty,
                          Weight = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Weight") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Weight").Element(xName + "value").Value : string.Empty,
                          PlayingTime = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Playing time") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Playing time").Element(xName + "value").Value : string.Empty,
                          Batteries = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Batteries") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Batteries").Element(xName + "value").Value : string.Empty,
                          Size2D = pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Size 2D") != null ? pr.Elements(xName + "field").FirstOrDefault(x => x.Attribute("name").Value == "Size 2D").Element(xName + "value").Value : string.Empty,
                          ProductInfo = pr.Elements(xName + "objects").Where(x => x.Attribute("classname").Value == "Jumbo_Product_lanquage").Elements(xName + "object").Select(x => new NederlofProductLanguageInfo
                          {
                            ContentStatus = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Content status") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Content status").Element(xName + "value").Value : string.Empty,
                            Created = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Created") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Created").Element(xName + "value").Value : string.Empty,
                            Description = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Catalog Description") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Catalog Description").Element(xName + "value").Value : string.Empty,
                            BusinessDescription = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "B to B Description") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "B to B Description").Element(xName + "value").Value : string.Empty,
                            InternetDescription = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Internet Description") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Internet Description").Element(xName + "value").Value : string.Empty,
                            Language = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Language") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Language").Element(xName + "value").Value : string.Empty,
                            ProductName = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Product name") != null ? x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Product name").Element(xName + "value").Value : string.Empty
                          }).ToList(),
                          ProductMedia = pr.Elements(xName + "items").Elements(xName + "item").Select(x => new NederlofProductMedia
                          {
                            Description = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "File Description").Element(xName + "value").Value,
                            Extension = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Extension Win").Element(xName + "value").Value,
                            FileName = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Filename").Element(xName + "value").Value,
                            FileSize = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "File Size").Element(xName + "value").Value,
                            Height = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Height").Element(xName + "value").Value,
                            HorizontalResolution = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Horizontal Resolution").Element(xName + "value").Value,
                            Path = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Path").Element(xName + "value").Value.Substring(48),
                            ThumbnailSize = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Thumbnail Size").Value,
                            VerticalResolution = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Vertical Resolution").Element(xName + "value").Value,
                            Width = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "Width").Element(xName + "value").Value,
                            MediaType = x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "KInd of product") == null ? "PDF" : x.Elements(xName + "field").FirstOrDefault(y => y.Attribute("name").Value == "KInd of product").Element(xName + "value").Value
                          }).ToList()
                        }).ToList();

        productData.AddRange(products);
      }


      using (var unit = GetUnitOfWork())
      {
        //DataLoadOptions options = new DataLoadOptions();
        //options.LoadWith<ProductGroupVendor>(x => x.VendorProductGroupAssortments);
        //options.LoadWith<VendorAssortment>(x => x.VendorPrice);
        //options.LoadWith<Product>(x => x.ProductBarcodes);
        //options.LoadWith<ProductGroupLanguage>(x => x.ProductGroup);
        //ctx.LoadOptions = options;

        var productRepo = unit.Scope.Repository<Product>().Include(c => c.ProductMedias);

        int counter = 0;
        int total = productData.Count;
        int totalNumberOfProductsToProcess = total;
        log.InfoFormat("Start import {0} media", total);

        foreach (var product in productData)
        {
          try
          {
            var currLanguage = 2;

            if (counter == 100)
            {
              counter = 0;
              log.InfoFormat("Still need to process {0} of {1}; {2} done;", totalNumberOfProductsToProcess, total, total - totalNumberOfProductsToProcess);
            }
            totalNumberOfProductsToProcess--;
            counter++;

            #region Product
            string barcode = product.barcode;

            var item = productRepo.GetSingle(p => p.VendorItemNumber == barcode && p.VendorAssortments.Any(y => y.CustomItemNumber == product.VendorItemNumber));

            if (item == null)
            {
              log.DebugFormat("No product found for vendoritemnumber {0} and customitemnumber {1}", barcode, product.VendorItemNumber);
              continue;
            }

            #endregion Product

            #region Images

            var maxSeq = (item.ProductMedias.Max(p => (int?)p.Sequence) ?? 0);
            var mediaTypeList = unit.Scope.Repository<MediaType>().GetAll().ToList();

            product.ProductMedia.ForEach((imgUrl, idx) =>
            {
              var fullImagePath = Path.Combine(destinationPath, imgUrl.Path);

              if (File.Exists(fullImagePath))
              {
                MediaType mType = unit.Scope.Repository<MediaType>().GetSingle(x => x.Type == imgUrl.MediaType);
                if (mType == null)
                {
                  mType = new MediaType()
                  {
                    Type = imgUrl.MediaType
                  };
                  unit.Scope.Repository<MediaType>().Add(mType);
                  mediaTypeList.Add(mType);
                }

                if (item.ProductMedias == null) item.ProductMedias = new List<ProductMedia>();
                if (!item.ProductMedias.Any(pi => pi.VendorID == vendorID && pi.MediaPath == imgUrl.Path))
                {
                  unit.Scope.Repository<ProductMedia>().Add(new ProductMedia
                  {
                    VendorID = vendorID,
                    MediaPath = imgUrl.Path,
                    MediaType = mType,
                    Product = item,
                    Sequence = maxSeq,
                    //Resolution = getRes(fullImagePath),//imgUrl.HorizontalResolution + "x" + imgUrl.VerticalResolution,
                    Size = (int)Math.Round(new FileInfo(fullImagePath).Length / 1024d, 0),
                    Description = imgUrl.Description
                  });
                  maxSeq++;
                }
              }
              else
                log.DebugFormat("Could not find media: {0}", fullImagePath);
            });
            #endregion

            unit.Save();
          }
          catch (Exception ex)
          {
            log.Error("Error JDE product Import", ex);
          }
        }

        //}
        log.AuditInfo("Products processing finished. Processed " + (total - totalNumberOfProductsToProcess) + " products");
      }

      if (networkDrive)
      {
        NetworkDrive oNetDrive = new NetworkDrive();
        try
        {
          oNetDrive.LocalDrive = drive;
          oNetDrive.UnMapDrive();
        }
        catch (Exception err)
        {
          log.Error("Error unmap drive" + err.InnerException);
        }
        oNetDrive = null;
      }

    }

    private string getRes(string imagePath)
    {
      if (File.Exists(imagePath))
      {
        FileStream fs = null;
        try
        {
          Image img = null;
          using (fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
          {
            using (var image = Image.FromStream(fs))
            {
              string res = string.Format("{0}x{1}", image.Width, image.Height);
              return res;
            }
          }
        }
        catch (Exception ex)
        {
          log.DebugFormat("Failed get image format for {0}", imagePath);
        }
      }
      return null;

    }
  }
}


