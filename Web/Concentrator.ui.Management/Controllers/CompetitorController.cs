using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class CompetitorController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetCompetitor)]
    public ActionResult GetList()
    {
      return List(unit =>
                   from c in unit.Service<ProductCompetitor>().GetAll()
                   select new
                   {
                     c.ProductCompetitorID,
                     c.Name,
                     c.Reliability,
                     c.DeliveryDate,
                     c.ShippingCostPerOrder,
                     c.ShippingCost
                   });
    }

    [RequiresAuthentication(Functionalities.DeleteCompetitor)]
    public ActionResult Delete(int id)
    {
      return Delete<ProductCompetitor>(c => c.ProductCompetitorID == id);
    }

    [RequiresAuthentication(Functionalities.UpdateCompetitor)]
    public ActionResult Update(int id)
    {
      return Update<ProductCompetitor>(c => c.ProductCompetitorID == id);
    }
  }
}
