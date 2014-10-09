using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.ZipUtil;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Web.Objects.EDI.DirectShipment;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Products;



namespace Concentrator.Plugins.Lenmar
{
  public class ImageImportFull : ConcentratorPlugin
  {
    private const string _name = "Lenmar Image FULL";
    private const int vendorID = 40;
    public override string Name
    {
      get { return _name; }
    }

    protected override void Process()
    {
      var config = GetConfiguration().AppSettings.Settings;
      try
      {
        var ftp = new FtpManager(config["LenmarImageFtpUrl"].Value, "mycom/Images/" + config["LenmarImageFtpFolder"].Value + "/",
                config["LenmarImageUser"].Value, config["LenmarImagePassword"].Value, false, true, log);

        using (var unit = GetUnitOfWork())
        {


          foreach (var file in ftp)
          {
            try
            {
              ProcessImageFile(file, unit);
              file.Data.Dispose();

              unit.Save();
            }
            catch (Exception ex)
            {
              log.AuditError("Error processing image", ex);
            }
          }
        }
        log.AuditComplete("Finished full Lenmar image import", "Lenmar Image Import");
      }
      catch (Exception ex)
      {
        log.AuditError("Error import Lenmar Images", ex);
      }
    }

    protected void ProcessImageFile(FtpManager.RemoteFile imageFile, IUnitOfWork unit)
    {
      var productID = Path.GetFileNameWithoutExtension(imageFile.FileName).Trim().Replace("_large", "").Trim();

      var product = unit.Scope.Repository<VendorAssortment>().GetSingle(va => va.CustomItemNumber == productID);
      if (product == null)
      {
        log.WarnFormat("Cannot process image for product with Lenmar number: {0} because it doesn't exist in Concentrator database", productID);
        return;
      }

      string destFileName = String.Format("{0}_{1}_{2}{3}", product.ProductID, product.CustomItemNumber, product.VendorID, Path.GetExtension(imageFile.FileName));
      string destinationPath = Path.Combine(GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value, "Products");

      string finalDestination = Path.Combine(destinationPath, destFileName);
      var mediaRepo = unit.Scope.Repository<ProductMedia>();
      var productImage = mediaRepo.GetSingle(pi => pi.ProductID == product.ProductID && pi.VendorID == vendorID);
      if (productImage == null)
      {
        productImage = new ProductMedia
        {
          ProductID = product.ProductID,
          VendorID = vendorID,
          MediaUrl = String.Empty,
          TypeID = 1
        };
        mediaRepo.Add(productImage);
      }

      if (File.Exists(finalDestination))
      {
        log.AuditWarning(string.Format("Skipping image for product with Lenmar number: {0} because it already exists", productID));
      }
      else
      {
        if (!Directory.Exists(destinationPath))
          Directory.CreateDirectory(destinationPath);

        using (var file = new BinaryWriter(System.IO.File.Open(finalDestination, FileMode.Create)))
        {
          using (Stream imageData = imageFile.Data)
          {
            using (var ms = new MemoryStream())
            {
              int count = 0;
              do
              {
                byte[] buf = new byte[1024];
                count = imageData.Read(buf, 0, 1024);
                ms.Write(buf, 0, count);
              } while (imageData.CanRead && count > 0);

              byte[] data = ms.ToArray();
              file.Write(data, 0, data.Length);
            }
          }
        }
      }
      productImage.MediaPath = Path.Combine("Products", destFileName);
    }

  }
}
