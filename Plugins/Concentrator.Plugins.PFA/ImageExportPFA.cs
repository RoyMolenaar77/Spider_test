using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using System.IO;
using System.Configuration;
using System.Drawing;
using Concentrator.Objects.Images;
using Concentrator.Plugins.PFA.Helpers;
using Concentrator.Objects;
using System.Drawing.Imaging;

namespace Concentrator.Plugins.PFA
{
  public class ImageExportPFA : ConcentratorPlugin
  {
    private int _imgHeight = 400;
    private int _vendorID = 1;
    public override string Name
    {
      get { return "Image export PFA"; }
    }

    protected override void Process()
    {
      using (var unit = GetUnitOfWork())
      {
        NetworkExportUtility util = new NetworkExportUtility();

        var config = GetConfiguration();

        var pathToExportTo = config.AppSettings.Settings["PFAImagePath"].Value;
#if DEBUG
        pathToExportTo = @"D:\ImagesPFA";
#endif
        var baseConcentratorImagePath = ConfigurationManager.AppSettings["FTPMediaDirectory"];


        var userName = config.AppSettings.Settings["DatColLocationUserName"].Value;
        if (string.IsNullOrEmpty(userName))
          throw new Exception("No DatColLocation UserName");

        var password = config.AppSettings.Settings["DatColLocationPassword"].Value;
        if (string.IsNullOrEmpty(password))
          throw new Exception("No DatColLocation Password");
#if !DEBUG
        pathToExportTo = util.ConnectorNetworkPath(pathToExportTo, "K:", userName, password);
#endif
        var products = unit.Scope.Repository<Product>().GetAll(c => c.SourceVendorID == _vendorID && c.IsConfigurable && c.ParentProductID == null && c.ProductMedias.Any()).ToList();
#if DEBUG
        products = products.Where(c => c.VendorItemNumber.Contains("84B4860001")).ToList();
#endif

        log.Debug(string.Format("Found {0} products to process", products.Count));

        int count = 0;
        foreach (var product in products) //config products
        {

          if (count % 100 == 0)
          {
            log.Debug(string.Format("Processed {0} products", count));
          }
          count++;

          var image = product.ProductMedias.Where(c => c.Sequence == 0).ToList().FirstOrDefault();

          if (image == null) continue;

          var concentratorPath = Path.Combine(baseConcentratorImagePath, image.MediaPath);

          if (!File.Exists(concentratorPath)) continue; //short circuit in case of problems

          var destinationFileName = string.Format("{0}.png", product.VendorItemNumber);

          try
          {
            File.Copy(concentratorPath, Path.Combine(pathToExportTo, destinationFileName), true);
          }
          catch (Exception e)
          {
            log.Debug(e);
          }
        }

#if !DEBUG
        util.DisconnectNetworkPath(pathToExportTo);
#endif
      }
    }
  }
}
