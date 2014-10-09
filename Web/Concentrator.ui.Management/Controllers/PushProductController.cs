using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class PushProductController : BaseController
  {
    [RequiresAuthentication(Functionalities.PushProducts)]
    public ActionResult GetList(bool? Processed)
    {
      return List(unit => (from c in unit.Service<PushProduct>().GetAll().ToList()
                           where Processed.HasValue ? c.Processed == Processed : true
                           let product = c.ProductID.HasValue ? unit.Service<Product>().Get(q => q.ProductID == c.ProductID) : null
                           let productName = product != null ?
                   product.ProductDescriptions.FirstOrDefault(x => x.LanguageID == Client.User.LanguageID) ??
                   product.ProductDescriptions.FirstOrDefault() : null
                           select new
                           {
                             c.ProductID,
                             ProductName = productName != null ? (productName.ProductName ?? productName.ShortContentDescription) :
  product != null ? product.VendorItemNumber : string.Empty,
                             c.ConnectorID,
                             c.CustomItemNumber,
                             c.PushProductID,
                             c.VendorID,
                             c.Processed,
                             LastPushDate = c.LastPushDate.ToNullOrLocal()
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.PushProducts)]
    public ActionResult Create()
    {
      return Create<PushProduct>();
    }

    [RequiresAuthentication(Functionalities.PushProducts)]
    public ActionResult Delete(int id)
    {
      return Delete<PushProduct>(c => c.PushProductID == id);
    }

    [RequiresAuthentication(Functionalities.PushProducts)]
    public ActionResult PushProducts()
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          ((IProductService)unit.Service<Product>()).PushProducts();

          return Success("Job started");
        }
        catch (Exception ex)
        {
          return Failure("Something went wrong" + ex.Message);
        }
      }
    }
  }
}
