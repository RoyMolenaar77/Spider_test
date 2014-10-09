using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Scan;
using Concentrator.Objects.Enumerations;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ScanProviderController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetScanProvider)]
    public ActionResult GetList()
    {
      return List(unit => from c in unit.Service<ScanProvider>().GetAll()
                          select new
                          {
                            c.ScanProviderID,
                            c.Name,
                            c.Url,
                            PriceType = Enum.GetName(typeof(PriceRuleType), c.PriceType),
                            c.IncludeShippingCost
                          });
    }

    [RequiresAuthentication(Functionalities.GetScanProvider)]
    public ActionResult GetStore()
    {
      return Json(new
      {
        results = SimpleList<ScanProvider>(c => new { ScanProviderID = c.ScanProviderID, Name = c.Name })
      });
    }

    [RequiresAuthentication(Functionalities.CreateScanProvider)]
    public ActionResult Create()
    {
      return Create<ScanProvider>();
    }

    [RequiresAuthentication(Functionalities.UpdateScanProvider)]
    public ActionResult Update(int id)
    {
      return Update<ScanProvider>(c => c.ScanProviderID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteScanProvider)]
    public ActionResult Delete(int id)
    {
      return Delete<ScanProvider>(c => c.ScanProviderID == id);
    }
  }
}
