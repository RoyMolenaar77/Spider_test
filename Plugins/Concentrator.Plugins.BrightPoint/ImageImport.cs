using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Products;
using System.IO;
using Concentrator.Objects.Models.Vendors;


namespace Concentrator.Plugins.BrightPoint
{
  class ImageImport : ConcentratorPlugin
  {

    public override string Name
    {
      get { return "Brightpoint Image Import"; }
    }

    protected override void Process()
    {


      var config = GetConfiguration();

      var CustNo = config.AppSettings.Settings["BrightPointCustomerNo"].Value;
      var Pass = config.AppSettings.Settings["BrightPointPassword"].Value;
      var Instance = config.AppSettings.Settings["BrightPointInstance"].Value;
      var Site = config.AppSettings.Settings["BrightPointSite"].Value;
      var WorkingDirectory = config.AppSettings.Settings["WorkingDirectory"].Value;
      int VendorID = Int32.Parse(config.AppSettings.Settings["vendorID"].Value);

      BrightPointService.AuthHeaderUser authHeader = new BrightPointService.AuthHeaderUser();

      authHeader.sCustomerNo = CustNo;
      authHeader.sInstance = Instance;
      authHeader.sPassword = Pass;
      authHeader.sSite = Site;

      if (VendorID != null)
      {
        using (BrightPointService.PartcatalogSoapClient client = new BrightPointService.PartcatalogSoapClient("Part catalogSoap"))
        {
          using (var unit = GetUnitOfWork())
          {

            var _productRepo = unit.Scope.Repository<VendorAssortment>();

            var products = _productRepo.GetAll(x => x.VendorID == VendorID);

            foreach (var product in products)
            {
              if (product == null)
              {
                //log.WarnFormat("Cannot process image for product with VSN number: {0} because it doesn't exist in Concentrator database", productID);
                continue;
              }

              string destinationPath = Path.Combine(GetConfiguration().AppSettings.Settings["ImageDirectory"].Value, "Products");
              var mediaRepo = unit.Scope.Repository<ProductMedia>();
              var productImage = mediaRepo.GetSingle(c => c.ProductID == product.ProductID && c.VendorID == VendorID);

              string destFileName = String.Format("{0}_{1}_{2}{3}", product.ProductID, product.CustomItemNumber, product.VendorID, Path.GetExtension(productImage.FileName));
              string finalDestination = Path.Combine(destinationPath, destFileName);

              if (productImage != null)
              {

                var imagebin = client.getImage(authHeader, productImage.FileName, 500, 0);

                if (File.Exists(finalDestination))
                {
                  //file already exists
                }
                else
                {
                  if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);

                  using (var file = File.Create(finalDestination))
                  {
                    // Convert Base64 String to byte[]
                    byte[] imageBytes = Convert.FromBase64String(imagebin);
                    MemoryStream ms = new MemoryStream(imageBytes);

                    //write file  
                    ms.WriteTo(file);
                  }
                }
                productImage.MediaPath = Path.Combine("Products", destFileName);
              }
              else
              {
                //should never come here
              }
            }
          }
        }
      }
    }
  }
}
