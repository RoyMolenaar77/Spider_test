using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.ui.Management.Models;
using Concentrator.Objects.Models.Contents;
using Concentrator.Objects.Models.Media;
using System.Configuration;
using Concentrator.Objects.Web;
using Concentrator.Objects.Logic;
using Concentrator.Objects.DataAccess.EntityFramework;

namespace Concentrator.ui.Management.Controllers
{
  public class FactSheetController : BaseController
  {
    [AcceptVerbs(HttpVerbs.Get)]
    public ActionResult PrintFactSheet(int? languageID, int productID)
    {
      string logoPath = string.Empty;

      Uri baseUri = new Uri(ConfigurationManager.AppSettings["ConcentratorWebservice"].ToString());
      if (Concentrator.Objects.Web.Client.User != null && !string.IsNullOrEmpty(Concentrator.Objects.Web.Client.User.Logo))
      {
        if (Concentrator.Objects.Web.Client.User.ConnectorID.HasValue)
          logoPath = new Uri(baseUri, "imagetranslate.ashx?mediaPath=ConnectorLogoPath/" + Concentrator.Objects.Web.Client.User.Logo).ToString();
        else
          logoPath = "~/Content/images/Pdf/" + Concentrator.Objects.Web.Client.User.Logo;
      }

      return new PdfResult(logoPath, languageID.HasValue ? languageID.Value : 2, productID);
    }

    public ActionResult FactSheet(int productID, string logo, int languageID, bool? success = null)
    {
      Uri baseUri = new Uri(ConfigurationManager.AppSettings["ConcentratorWebservice"].ToString());

      if(string.IsNullOrEmpty(logo))
        logo = "~/Content/images/Pdf/clientLogo.png";

      using (var unit = GetUnitOfWork())
      {
        var product = (from x in unit.Service<Product>().GetAll(p => p.ProductID == productID).ToList()
                       let desc = x.ProductDescriptions.FirstOrDefault(c => c.LanguageID == 2)//Client.User.LanguageID)
                       select new FactSheetModel
                       {
                         ProductID = x.ProductID,
                         BrandName = x.Brand.Name,
                         ProductName = desc != null ? desc.ProductName : string.Empty

                       }).FirstOrDefault();

        if (product == null)
        {
          //geen product
        }
        
        
        var logic = ((IProductService)unit.Service<Product>()).FillPriceInformation(Client.User.ConnectorID.Value);
        var price = logic.CalculatePrice(product.ProductID).OrderBy(x => x.MinimumQuantity).FirstOrDefault();

        if (price != null)
        {
          decimal unitPrice = price.Price.HasValue ? price.Price.Value : 0;
          decimal taxRate = price.TaxRate.HasValue ? price.TaxRate.Value / 100 : 0;

          product.Price = unitPrice * (taxRate + 1);
        }

        var attributes = unit.Service<ContentAttribute>().GetAll(x => x.ConnectorID == Client.User.ConnectorID.Value
          && x.ProductID == productID).Select(x => new AttributeModel
          {
            AttributeName = x.AttributeName,
            AttributeValue = x.AttributeValue,
            GroupIndex = x.GroupIndex,
            GroupName = x.GroupName,
            AttributeIndex = x.OrderIndex
          }).ToList();

        product.AttributeModels = attributes;
        
        var images = unit.Service<ImageView>().GetAll(x => x.ConnectorID == Client.User.ConnectorID.Value
          && x.ProductID == productID).ToList().Select(x => new ImageModel
          {
            Sequence = x.Sequence,
            ImagePath = new Uri(baseUri, "imagetranslate.ashx?mediaPath=" + x.ImagePath).ToString()
          }).ToList();

        product.ImageModels = images;

        var barcodes = unit.Service<ProductBarcodeView>().GetAll(x => x.ConnectorID == Client.User.ConnectorID.Value
          && x.ProductID == productID).Select(x => new BarCodeModel
          {
            Barcode = x.Barcode

          }).ToList();

        product.BarCode = barcodes.FirstOrDefault() != null ? barcodes.FirstOrDefault().Barcode : string.Empty;

        var descriptions = unit.Service<AssortmentContentView>().GetAll(x => x.ConnectorID == 2//Client.User.ConnectorID.Value
          && x.ProductID == productID).Select(x => new DescriptionModel
          {
            ShortDescription = x.ShortDescription,
            LongDescription = x.LongDescription

          }).ToList();

        if (string.IsNullOrEmpty(product.ProductName))
          product.ProductName = descriptions.FirstOrDefault() != null ? descriptions.FirstOrDefault().ShortDescription : product.ProductID.ToString();

        product.DescriptionModels = descriptions;
        product.logoPath = logo;
        
        if (success.HasValue)
          ViewBag.Success = true;

        return View(product);
      }
    }

  }
}
