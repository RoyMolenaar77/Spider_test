using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Contents;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;


namespace Concentrator.ui.Management.Controllers
{
  public class ContentPriceCalculationController : BaseController
  {

    [RequiresAuthentication(Functionalities.CreateContentPriceCalculation)]
    public ActionResult Create()
    {
      return Create<ContentPriceCalculation>();
    }

    [RequiresAuthentication(Functionalities.GetContentPriceCalculation)]
    public ActionResult Search(string query)
    {
      var q = query.IfNullOrEmpty("").ToLower();
      return Search(unit => from s in unit.Service<ContentPriceCalculation>().GetAll(c => c.Name.ToLower().Contains(q))
                            select new
                            {
                              ContentPriceCalculationName = s.Name,
                              s.ContentPriceCalculationID
                            });
    }

  }
}
