using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Concentrator.Objects.Environments;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Vendors.IceCat;
using System.Threading.Tasks;
using System.Net;
using Concentrator.Objects;
using System.Data.SqlClient;
using System.Data;
using System.Security.AccessControl;
using System.Xml;
using System.Transactions;
using System.Data.Linq;
using System.Xml.Linq;
using Concentrator.Plugins.IceCat.Bulk;
using Concentrator.Objects.Models.Products;


namespace Concentrator.Plugins.IceCat
{
  public class DataImport : ConcentratorPlugin
  {
    private const string _name = "IceCat Staging Import";

    public const int VendorID = 38;
    public bool OpenIceCat { get; set; }
    protected NetworkCredential Credentials;
    public string CacheDir { get; set; }
    public string BaseURI { get; set; }
    protected string ProdCache { get; set; }

    public override string Name
    {
      get { return _name; }
    }

    public static Dictionary<int, int> Languages = new Dictionary<int, int>() { 
      { 1, 1 },
      { 2, 2 },
      { 3, 3 }
    };

    public DataImport()
    {
      var config = GetConfiguration();

      Credentials = new NetworkCredential(config.AppSettings.Settings["User"].Value, config.AppSettings.Settings["Password"].Value);
      BaseURI = config.AppSettings.Settings["BaseDownloadUrl"].Value;

      OpenIceCat = bool.Parse(config.AppSettings.Settings["FullIceCat"].Value);

      CacheDir = config.AppSettings.Settings["CachingDirectory"].Value;
      //ProdCache = Path.Combine(CacheDir, "Products");
      Directory.CreateDirectory(CacheDir); // Ensure exists
    }

    #region Scripts


    /*
     // To create schema:
     CREATE SCHEMA [DataStaging] AUTHORIZATION [dbo]
     GO
     */

    #endregion

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        var context = unit.Context;
        context.CommandTimeout = 3600;

        try
        {
          using (var productMatchBulk = new ProductMappingBulk(BaseURI, CacheDir, Credentials, OpenIceCat))
          {
            productMatchBulk.Init(context);
            productMatchBulk.Sync(context);
          }

          using (var brandBulk = new BrandBulk(BaseURI, CacheDir, Credentials, OpenIceCat))
          {
            brandBulk.Init(context);
            brandBulk.Sync(context);

            using (var productIndex = new ProductIndexBulk(BaseURI, CacheDir, Credentials, OpenIceCat, log))
            {
              productIndex.Init(context);
              productIndex.Sync(context);

              using (var productBulk = new ProductBulk(BaseURI, Credentials))
              {
                productBulk.Init(context);

                using (var attributeBulk = new AttributeMetaDataBulk(BaseURI, CacheDir, Credentials, OpenIceCat))
                {
                  try
                  {
                    attributeBulk.Init(context);
                    attributeBulk.Sync(context);
                  }
                  catch (Exception ex)
                  {
                    log.AuditFatal("No icecat attributes because", ex.StackTrace);
                  }
                  productBulk.Sync(context);
                }
             }
            }
          }
        }
        catch (Exception e)
        {
          log.AuditFatal("Icecat product bulk failed", e, "Icecat import");
        }
        //try
        //{
        //  var images = unit.Scope.Repository<ProductMedia>().GetAll(x => x.VendorID == VendorID && x.MediaPath == null).ToList();
        //  ///context.ProductMedia.Where(x => x.VendorID == VendorID && x.MediaPath == null).ToList();

        //  string imageDirectory = GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value;
        //  string path = Path.Combine(imageDirectory, "Products");

        //  if (!string.IsNullOrEmpty(imageDirectory))
        //  {
        //    foreach (var image in images)
        //    {
        //      string fileName = string.Format("{0}_{1}_{2}_{3}", image.MediaID, image.ProductID, image.Sequence, image.VendorID);

        //      var file = Path.Combine(path, fileName);
        //      if (File.Exists(file))
        //      {
        //        File.Delete(file);
        //      }
        //    }
        //  }
        //}
        //catch (Exception ex)
        //{
        //  log.Error("Failed to delete images", ex);
        //}
      }

    }
  }
}
