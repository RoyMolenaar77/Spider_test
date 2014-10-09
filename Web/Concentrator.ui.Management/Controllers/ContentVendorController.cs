using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Services.ServiceInterfaces;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ContentVendorController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetContentVendor)]
    public ActionResult Search(string query)
    {
      if (query != null && query != String.Empty && query != "")
      {
        return Search(unit => from v in ((IVendorService)unit.Service<Vendor>()).GetContentVendors()
                              where v.Name.ToLower().Contains(query.ToLower())
                              select new
                              {
                                v.VendorID,
                                VendorName = v.Name
                              });
      }
      else
      {
        return Search(unit => from v in ((IVendorService)unit.Service<Vendor>()).GetContentVendors()
                              select new
                              {
                                v.VendorID,
                                VendorName = v.Name
                              });
      }
    }
  }
}
