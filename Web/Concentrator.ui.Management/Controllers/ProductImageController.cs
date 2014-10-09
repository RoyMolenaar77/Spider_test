using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ProductImageController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductImage)]
    public ActionResult GetImages(int productID)
    {
      return List(unit => from img in unit.Service<ProductMedia>().GetAll(x => x.ProductID == productID)
                          select new
                          {
                            ImageURL = img.MediaUrl,
                            img.Vendor.Name,
                            img.Sequence
                          });
    }
  }
}
