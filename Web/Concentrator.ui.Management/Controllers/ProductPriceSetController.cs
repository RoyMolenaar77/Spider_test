using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Concentrator.Web.Shared;
using System.Web.Mvc;
using Concentrator.Objects.Models.Products;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Web;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductPriceSetController : BaseController
  {
    [RequiresAuthentication(Functionalities.ViewPriceSet)]
    public ActionResult GetList(int PriceSetID)
    {
      var list = List(unit => from s in unit.Service<ProductPriceSet>().GetAll(x => x.PriceSetID == PriceSetID)
                          select new
                          {
                            PriceSetID = s.PriceSetID,
                            ProductID = s.ProductID,
                            Product = s.Product.VendorItemNumber,
                            Quantity = s.Quantity
                          });
      return list;
    }

    [RequiresAuthentication(Functionalities.UpdatePriceSet)]
    public ActionResult Update(int PriceSetID, int _ProductID)
    {
      return Update<ProductPriceSet>(c => c.PriceSetID == PriceSetID && c.ProductID == _ProductID);
    }

    [RequiresAuthentication(Functionalities.DeletePriceSet)]
    public ActionResult Delete(int PriceSetID, int _ProductID)
    {
      return Delete<ProductPriceSet>(c => c.PriceSetID == PriceSetID && c.ProductID == _ProductID);
    }

    [RequiresAuthentication(Functionalities.CreatePriceSet)]
    public ActionResult Create(int PriceSetID, int ProductID)
    {
      return Create<ProductPriceSet>();
    }
  }
}