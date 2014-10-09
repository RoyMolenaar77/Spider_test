using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using System.Configuration;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared.Models;
using Concentrator.Objects.Models.Vendors;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductMatchController : BaseController
  {
    [RequiresAuthentication(Functionalities.ViewProductMatch)]
    public ActionResult GetList(bool? isMatched, ContentFilter filter)
    {
      return List(unit => (from c in unit.Service<ProductMatch>().GetAll()
                           join matchProduct in unit.Service<ProductMatch>().GetAll() on c.ProductMatchID equals matchProduct.ProductMatchID
                           let pMedia = c.Product.ProductMedias.Where(m => m.MediaType.Type == "Image").FirstOrDefault()
                           let cMedia = matchProduct.Product.ProductMedias.Where(x => x.MediaType.Type == "Image").FirstOrDefault()
                           let productName = c.Product != null ?
                           c.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                           c.Product.ProductDescriptions.FirstOrDefault() : null
                           let productMatchName = matchProduct.Product != null ?
                           matchProduct.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                           matchProduct.Product.ProductDescriptions.FirstOrDefault() : null
                           where (isMatched.HasValue ? c.isMatched == isMatched : true)
                           && (c.ProductID != matchProduct.ProductID) &&
                           ((filter.MatchedProduct.HasValue ? c.isMatched == true : true) ||
                           (filter.UnmatchedProduct.HasValue ? c.isMatched == false : false))
                           select new
                           {
                             c.ProductMatchID,
                             c.ProductID,
                             ProductBarcode = c.Product.ProductBarcodes.Count > 0 ? c.Product.ProductBarcodes.FirstOrDefault().Barcode : string.Empty,
                             Description = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
                             c.Product != null ? c.Product.VendorItemNumber : string.Empty,
                             VendorItemNumber = c.Product.VendorItemNumber,
                             Brand = c.Product.BrandID,
                             BrandName = c.Product.Brand.Name,
                             CorrespondingProductID = matchProduct.ProductID,
                             CorrespondingProductBarcode = matchProduct.Product.ProductBarcodes.Count > 0 ? matchProduct.Product.ProductBarcodes.FirstOrDefault().Barcode : string.Empty,
                             CorrespondingDescription = productMatchName != null ? (productMatchName.ProductName ?? productMatchName.ShortContentDescription) :
                             matchProduct.Product != null ? matchProduct.Product.VendorItemNumber : string.Empty,
                             CorrespondingBrand = matchProduct.Product.BrandID,
                             CorrespondingBrandName = matchProduct.Product.Brand.Name,
                             CorrespondingVendorItemNumber = matchProduct.Product.VendorItemNumber,
                             c.isMatched,
                             MatchPercentage = c.MatchPercentage.HasValue ? c.MatchPercentage.Value : 0,
                             //ConnectorID = c.Product.Contents.Select(e => e.ConnectorID).FirstOrDefault() != null ? c.Product.Contents.Select(e => e.ConnectorID).FirstOrDefault() : int.Parse(ConfigurationManager.AppSettings["ConcentratorVendorID"].ToString()),
                             ConnectorID = c.Product.Contents.Select(e => e.ConnectorID).FirstOrDefault(),
                             VendorID = c.Product.SourceVendorID,
                             MediaUrl = pMedia != null ? pMedia.MediaUrl : String.Empty,
                             MediaPath = pMedia != null ? pMedia.MediaUrl : String.Empty,
                             CorrespondingMediaUrl = cMedia != null ? cMedia.MediaUrl : String.Empty,
                             CorrespondingMediaPath = cMedia != null ? cMedia.MediaPath : String.Empty,
                             MatchStatus = c.MatchStatus
                           }));
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetActiveProductsPerProduct(int productID, int vendorID)
    {
      return List(unit => (from v in unit.Service<VendorAssortment>().GetAll(v => v.IsActive == true && v.ProductID == productID && v.VendorID == vendorID)
                           select new
                           {
                             v.ProductID,
                             v.CustomItemNumber,
                             v.ShortDescription
                           }).OrderBy(x => x.ProductID));


    }

    [RequiresAuthentication(Functionalities.ViewProductMatch)]
    public ActionResult GetStatusTypes()
    {
      List<MatchStatuses> enums = EnumHelper.EnumToList<MatchStatuses>();

      return Json(new
      {
        results = (from e in enums
                   select new
                   {
                     ID = (int)e,
                     Name = Enum.GetName(typeof(MatchStatuses), e)
                   }).ToArray()
      });
    }

    [RequiresAuthentication(Functionalities.UpdateProductMatch)]
    public ActionResult Update(int _ProductMatchID, int _ProductID, string _VendorItemNumber)
    {
      return Update<ProductMatch>(c => c.ProductMatchID == _ProductMatchID && c.ProductID == _ProductID);
    }

    [RequiresAuthentication(Functionalities.DeleteProductMatch)]
    public ActionResult Delete(int _ProductMatchID, int _ProductID, string _VendorItemNumber)
    {
      return Delete<ProductMatch>(c => c.ProductMatchID == _ProductMatchID && c.ProductID == _ProductID);
    }

    [RequiresAuthentication(Functionalities.CreateProductMatch)]
    public ActionResult Create(int CorrespondingProductID, int ProductID, string isMatched)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {

          var Matched = isMatched != null ? isMatched.Contains("on") ? true : false : false;

          var matchService = unit.Service<ProductMatch>();

          ProductMatch productMatch = new ProductMatch();
          ProductMatch correspondingMatch = new ProductMatch();
          var matchID = matchService.GetAll().Count() > 0 ? matchService.GetAll().Max(x => x.ProductMatchID + 1) : 1;

          var pID = matchService.Get(i => i.ProductID == ProductID);
          var cpID = matchService.Get(i => i.ProductID == CorrespondingProductID);


          if (pID != null && cpID == null)
          {
            correspondingMatch.ProductMatchID = pID.ProductMatchID;
            correspondingMatch.ProductID = CorrespondingProductID;
            correspondingMatch.isMatched = Matched;
            correspondingMatch.MatchStatus = 1;

            matchService.Create(correspondingMatch);
            unit.Save();
          }

          if (cpID != null && pID == null)
          {
            productMatch.ProductMatchID = cpID.ProductMatchID;
            productMatch.ProductID = ProductID;
            productMatch.isMatched = Matched;
            productMatch.MatchStatus = 1;

            matchService.Create(productMatch);
            unit.Save();
          }

          if (cpID == null && pID == null)
          {
            productMatch.ProductMatchID = matchID;
            productMatch.ProductID = ProductID;
            productMatch.isMatched = Matched;
            productMatch.MatchStatus = 1;

            matchService.Create(productMatch);
            unit.Save();

            correspondingMatch.ProductMatchID = matchID;
            correspondingMatch.ProductID = CorrespondingProductID;
            correspondingMatch.isMatched = Matched;
            correspondingMatch.MatchStatus = 1;

            matchService.Create(correspondingMatch);
            unit.Save();
          }

          if (cpID == pID)
          {
            return Failure("These products share the same identification");
          }

          if (cpID != null && pID != null)
          {
            return Failure("This match already exists");
          }

          return Success("Product match successfully added");
        }
        catch (Exception ex)
        {
          return Failure(String.Format("Something went wrong while trying to add a product match, {0}", ex));
        }
      }
    }

    [RequiresAuthentication(Functionalities.ViewProductMatch)]
    public ActionResult GetById(int productID, int? correspondingProductID, string vendorItemNumber)
    {
      using (var unit = GetUnitOfWork())
      {
        var product = (from p in unit.Service<ProductMatch>().GetAll()
                       join matchProduct in unit.Service<ProductMatch>().GetAll() on p.ProductMatchID equals matchProduct.ProductMatchID
                       let va = p.Product.VendorAssortments.FirstOrDefault()
                       let matchProductVa = matchProduct.Product.VendorAssortments.FirstOrDefault()
                       where p.ProductID == productID && matchProduct.ProductID == correspondingProductID.Value
                       select new
                       {
                         p.ProductMatchID,
                         p.ProductID,
                         Description = va != null ? va.ShortDescription : p.Product.ProductDescriptions.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID && p.ProductID == d.ProductID).ShortContentDescription,
                         VendorItemNumber = p.Product.VendorItemNumber,
                         Vendor = va != null ? va.Vendor.Name : string.Empty,
                         VendorCount =p.Product.VendorAssortments.Count(),
                         //ProductBarcode = string.Join(",", (p.Product.ProductBarcodes.Select(c => c.Barcode).ToArray())),
                         ProductBarcode = p.Product.ProductBarcodes.Count > 0 ? p.Product.ProductBarcodes.FirstOrDefault().Barcode : string.Empty,
                         Brand = p.Product.BrandID,
                         BrandName = (p.Product.Brand.Name),
                         CorrespondingProductID = matchProduct.ProductID,
                         CorrespondingDescription = matchProductVa != null ? matchProductVa.ShortDescription : matchProduct.Product.ProductDescriptions.FirstOrDefault(d => d.LanguageID == Client.User.LanguageID).ShortContentDescription,
                         CorrespondingVendor = matchProductVa != null ? matchProductVa.Vendor.Name : string.Empty,
                         CorrespondingVendorCount = matchProduct.Product.VendorAssortments.Count(),
                         CorrespondingVendorItemNumber = matchProduct.Product.VendorItemNumber,
                         //CorrespondingProductBarcode = string.Join(",", matchProduct.Product.ProductBarcodes.Select(c => c.Barcode).ToArray()),
                         CorrespondingProductBarcode = matchProduct.Product.ProductBarcodes.Count > 0 ? matchProduct.Product.ProductBarcodes.FirstOrDefault().Barcode : string.Empty,
                         CorrespondingBrand = matchProduct.Product.Brand.BrandID,
                         CorrespondingBrandName = matchProduct.Product.Brand.Name
                       }).FirstOrDefault();

        return Json(new
        {
          success = true,
          data = product
        });
      }
    }

    [RequiresAuthentication(Functionalities.ViewProductMatch)]
    public ActionResult CreateMatch(int productID, int correspondingProductID, int productMatchID)
    {
      try
      {
        using (var unit = GetUnitOfWork())
        {
          ((IProductService)unit.Service<Product>()).CreateMatchForProduct(productID, correspondingProductID, productMatchID);

          unit.Save();

          return Json(new
          {
            succes = true,
            message = "Successfully matched the products"
          });
        }
      }
      catch (Exception ex)
      {
        return Failure(String.Format("Product weren't matched {0}", ex.Message));
      }
    }

    [RequiresAuthentication(Functionalities.ViewProductMatch)]
    public ActionResult RemoveMatch(int correspondingProductID, int productMatchID)
    {
      using (var unit = GetUnitOfWork())
      {
        ((IProductService)unit.Service<Product>()).RemoveMatchForProduct(correspondingProductID, productMatchID);

        unit.Save();

        return Json(new
        {
          succes = true,
          message = "These products have been stored as a non-match"
        });
      }

    }
  }
}
