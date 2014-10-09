using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Concentrator.Objects.ZipUtil;
using System.IO;
using Concentrator.Objects;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;


namespace Concentrator.Plugins.VSN
{
  public abstract class ImageImportBase : VSNBase
  {
    protected void ProcessImageFile(ZipProcessor.ZippedFile zipFile, IUnitOfWork unit, string filePath)
    {
      var productID = Path.GetFileNameWithoutExtension(zipFile.FileName).Trim();

      var product = unit.Scope.Repository<VendorAssortment>().GetSingle(va => va.CustomItemNumber == productID && va.VendorID == VendorID && va.IsActive == true);
      if (product == null)
      {
        //log.WarnFormat("Cannot process image for product with VSN number: {0} because it doesn't exist in Concentrator database", productID);
        return;
      }

      string destFileName = String.Format("{0}_{1}_{2}{3}", product.ProductID, product.CustomItemNumber, product.VendorID, Path.GetExtension(zipFile.FileName));
      //string destinationPath = Path.Combine(GetConfiguration().AppSettings.Settings["FTPImageDirectory"].Value, "Products");
      string destinationPath = Path.Combine(filePath, "Products");

      string finalDestination = Path.Combine(destinationPath, destFileName);
      var mediaRepo = unit.Scope.Repository<ProductMedia>();
      var productImage = mediaRepo.GetSingle(c => c.ProductID == product.ProductID && c.VendorID == VendorID);

      if (productImage == null)
      {
        productImage = new ProductMedia
        {
          ProductID = product.ProductID,
          VendorID = VendorID,
          MediaUrl = String.Empty,
          TypeID = 1
        };
        mediaRepo.Add(productImage);
      }

      if (File.Exists(finalDestination))
      {
        //log.WarnFormat("Skipping image for product with VSN number: {0} because it already exists", productID);
      }
      else
      {
        if (!Directory.Exists(destinationPath))
          Directory.CreateDirectory(destinationPath);

        using (var file = File.Create(finalDestination))
        {
          zipFile.Data.WriteTo(file);
        }
      }
      productImage.MediaPath = Path.Combine("Products", destFileName);
    }
  }
}
