using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Scan;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ScanDataController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetScanData)]
    public ActionResult GetList()
    {
      return List(unit => (from c in unit.Service<ScanData>().GetAll()
                           select new
                           {
                             c.ProductGroupMappingID,
                             c.ScanTime,
                             c.ConnectorID,
                             c.ScanDisplay
                           }));
    }


    [RequiresAuthentication(Functionalities.UpdateScanData)]
    public ActionResult Update(int id, string ScanTime)
    {
      return Update<ScanData>(c => c.ProductGroupMappingID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteScanData)]
    public ActionResult Delete(int id)
    {
      return Delete<ScanData>(c => c.ProductGroupMappingID == id);
    }
  }
}
