using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Magento;

namespace Concentrator.ui.Management.Controllers
{
  public class MagentoPageLayoutController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetProductGroupMapping)]
    public ActionResult GetList()
    {
      using (var unit = GetUnitOfWork())
      {
        var results = (from m in unit.Service<MagentoPageLayout>().GetAll()
                          select new
                          {
                            m.LayoutID,
                            m.LayoutName
                          }).ToList();
        results.Add(new { LayoutID = -1, LayoutName = "No layout" });
        return Json(new { results = results });
      }
    }
  }
}