using System;
using System.IO;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.ZipUtil;
using Concentrator.Objects.Vendors;
using System.Globalization;
using Concentrator.Objects.Models.Media;

namespace Concentrator.Plugins.CentraalBoekhuis
{
  public abstract class MediaImportBase : VendorBase
  {
    protected void ProcessMediaFile(ZipProcessor.ZippedFile zipFile, IUnitOfWork unit, string filePath)
    {
      var productID = Path.GetFileNameWithoutExtension(zipFile.FileName).Trim();

      if (productID.EndsWith("_h1", StringComparison.CurrentCultureIgnoreCase))
        productID = productID.Replace("_h1", String.Empty);

      var product = unit.Scope.Repository<VendorAssortment>().GetSingle(va => va.CustomItemNumber == productID && va.VendorID == VendorID && va.IsActive == true);

      if (product == null)
      {
        log.WarnFormat("Cannot process image for product with Centraal boekhuis number: {0} because it doesn't exist in Concentrator database", productID);
        return;
      }

      String destFileName = String.Format("{0}_{1}_{2}{3}", product.ProductID, product.CustomItemNumber, product.VendorID, Path.GetExtension(zipFile.FileName));
      String destinationPath = Path.Combine(filePath, "Products");
      String finalDestination = Path.Combine(destinationPath, destFileName);

      //Handle mediatype
      String extension = Path.GetExtension(finalDestination).ToLower();
      Int32 typeID = 0;


      var _mediaTypeRepo = unit.Scope.Repository<MediaType>();

      switch (extension)
      {
        case ".pdf":
          typeID = _mediaTypeRepo.GetSingle(x => (x.Type).ToLower() == "pdf").TypeID;
          break;
        case ".jpg":
          typeID = _mediaTypeRepo.GetSingle(x => (x.Type).ToLower() == "image").TypeID;
          break;
        case ".jpeg":
          typeID = _mediaTypeRepo.GetSingle(x => (x.Type).ToLower() == "image").TypeID;
          break;
        case ".epub":
          typeID = _mediaTypeRepo.GetSingle(x => (x.Type).ToLower() == "ebook").TypeID;
          break;
        case ".mp3":
          typeID = _mediaTypeRepo.GetSingle(x => (x.Type).ToLower() == "audio").TypeID;
          break;
        default:
          log.Error("Unknown file extension");
          break;
      }

      var mediaRepo = unit.Scope.Repository<ProductMedia>();
      var productMedia = mediaRepo.GetSingle(c => c.ProductID == product.ProductID && c.VendorID == VendorID);

      if (productMedia == null)
      {
        productMedia = new ProductMedia
        {
          ProductID = product.ProductID,
          VendorID = VendorID,
          MediaUrl = String.Empty,
          TypeID = typeID
        };

        mediaRepo.Add(productMedia);
      }

      if (File.Exists(finalDestination))
      {
        log.WarnFormat("Skipping image for product with centraal boekhuis number: {0} because it already exists", productID);
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

      productMedia.MediaPath = Path.Combine("Products", destFileName);
    }
  }
}