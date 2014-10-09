using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Web;

namespace Concentrator.ui.Management.Controllers
{
  public class VendorPriceRuleController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetVendorPriceRule)]
    public ActionResult GetList()
    {
      return List(unit =>
          from vp in unit.Service<VendorPriceRule>().GetAll()
          let productName = vp.Product != null ?
                                           vp.Product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                                           vp.Product.ProductDescriptions.FirstOrDefault() : null
          select new
          {
            vp.BrandID,
            BrandName = vp.Brand != null ? vp.Brand.Name : string.Empty,
            vp.VendorPriceRuleID,
            vp.VendorID,

            vp.ProductID,
            Product = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
                      vp.Product != null ? vp.Product.VendorItemNumber : null,
            vp.ProductGroupID,
            ProductGroupName = vp.ProductGroup != null ? vp.ProductGroup.ProductGroupLanguages.FirstOrDefault(c => c.LanguageID == Client.User.LanguageID).Name : string.Empty,
            vp.Margin,
            vp.MinimumQuantity,
            vp.PriceRuleType,
            vp.VendorPriceCalculationID,
            VendorPriceCalculationName = vp.VendorPriceCalculation != null ? vp.VendorPriceCalculation.Name : string.Empty,
            vp.UnitPriceIncrease,
            vp.CostPriceIncrease
          }
        );
    }

    [RequiresAuthentication(Functionalities.CreateVendorPriceRule)]
    public ActionResult Create()
    {
      return Create<VendorPriceRule>();
    }

    [RequiresAuthentication(Functionalities.DeleteVendorPriceRule)]
    public ActionResult Delete(int ID)
    {
      return Delete<VendorPriceRule>(c => c.VendorPriceRuleID == ID);
    }

    [RequiresAuthentication(Functionalities.UpdateVendorPriceRule)]
    public ActionResult Update(int ID)
    {
      return Update<VendorPriceRule>(c => c.VendorPriceRuleID == ID);
    }
  }
}
