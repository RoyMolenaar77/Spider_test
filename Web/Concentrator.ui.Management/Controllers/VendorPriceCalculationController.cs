using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Web.Shared;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class VendorPriceCalculationController : BaseController
  {
    [RequiresAuthentication(Functionalities.CreateVendorPriceCalculation)]
    public ActionResult Create()
    {
      return Create<VendorPriceCalculation>();
    }

    [RequiresAuthentication(Functionalities.GetVendorPriceCalculation)]
    public ActionResult Search(string query)
    {
      var q = query.IfNullOrEmpty("").ToLower();
      return Search(unit => from s in unit.Service<VendorPriceCalculation>().GetAll(c => c.Name.ToLower().Contains(q))
                            select new
                            {
                              VendorPriceCalculationName = s.Name,
                              s.VendorPriceCalculationID
                            });
    }
  }
}
