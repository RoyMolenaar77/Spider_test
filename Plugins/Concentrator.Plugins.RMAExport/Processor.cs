using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.Attributes;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.RMAExport
{
  public class Processor : ConcentratorPlugin
  {
    private int vendorID_NL;
    private int vendorID_BE;
    public override string Name
    {
      get { return "Sennheiser RMAExport Plugin"; }
    }


    protected override void Process()
    {
      var config = GetConfiguration();

      vendorID_NL = int.Parse(config.AppSettings.Settings["VendorID_NL"].Value);
      vendorID_BE = int.Parse(config.AppSettings.Settings["VendorID_BE"].Value);

      using (var unit = GetUnitOfWork())
      {
        RmaDataContext contextRma = new RmaDataContext(config.ConnectionStrings.ConnectionStrings["Concentrator.Plugins.RMAExport.Properties.Settings.Sennheiser_DevelopmentConnectionString"].ConnectionString);

        var productsToExport = unit.Scope.Repository<Concentrator.Objects.Models.Products.Product>().GetAll(prod => prod.SourceVendorID == vendorID_NL || prod.SourceVendorID == vendorID_BE).Where(x => x.VendorAssortments.Any(y => y.IsActive)).ToList();
        var productsExported = contextRma.Products.ToList();
        var InActiveProducts = productsExported.ToList();

        foreach (var productToExport in productsToExport)
        {
          var product = productsExported.FirstOrDefault(prod => prod.ConcentratorIdentifier == productToExport.ProductID);

          string name = productToExport.VendorItemNumber;

          var productDesc = productToExport.ProductDescriptions.FirstOrDefault();
          var productVendorDesc = productToExport.VendorAssortments.FirstOrDefault();

          if (productDesc != null && !string.IsNullOrEmpty(productDesc.ProductName)) name = productDesc.ProductName;
          else if (productVendorDesc != null && !string.IsNullOrEmpty(productVendorDesc.ShortDescription)) name = productVendorDesc.ShortDescription;

          if (product == null)
          {
            product = new Product
                                       {
                                         ConcentratorIdentifier = productToExport.ProductID
                                       };
            contextRma.Products.InsertOnSubmit(product);
            productsExported.Add(product);
          }
          else
          {
            InActiveProducts.Remove(product);
          }

          var prodImage = productToExport.ProductMedias.FirstOrDefault();
          var prodBarcode = productToExport.ProductBarcodes.FirstOrDefault();
          product.Name = name.Cap(100);
          product.isConsumer =unit.Scope.Repository<ProductAttributeValue>().GetAllAsQueryable(p => p.Product == productToExport).Any(pa => pa.Value == "CSN") ? true : false;
          product.Barcode = prodBarcode != null ? prodBarcode.Barcode : null;
          product.ImageURL = prodImage != null ? prodImage.MediaUrl.Cap(150) : null;
          product.Description = name.Cap(500);
          product.ItemNumber = productToExport.VendorItemNumber.PadLeft(6, '0').Cap(50); //TO VERIFY
          product.IsActive = true;

        }
        contextRma.SubmitChanges();

        InActiveProducts.ForEach(x => x.IsActive = false);
        contextRma.SubmitChanges();
      }
    }
  }
}
