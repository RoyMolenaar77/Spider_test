using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.Web.Mvc;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects.Models.EDI.Vendor;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiVendorController : BaseController
  {
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      return List(unit => (from e in unit.Service<EdiVendor>().GetAll().ToList()
                           select new
                           {
                             e.EdiVendorID,
                             e.Name,
                             e.EdiVendorType,
                             e.CompanyCode,
                             e.DefaultDocumentType,
                             e.OrderBy
                           }).AsQueryable());
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetEdiConnectorTypes()
    {
      List<EdiConnectorTypeEnum> enums = EnumHelper.EnumToList<EdiConnectorTypeEnum>();

      return Json(new
      {
        results = (from e in enums
                   select new
                   {
                     EDIConnectionType = (int)e,
                     Name = Enum.GetName(typeof(EdiConnectorTypeEnum), e)
                   }).ToArray()
      });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Create()
    {
      return Create<EdiVendor>();
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Search(string query)
    {
      return SimpleList(unit => from c in unit.Service<EdiVendor>().GetAll()
                                select new
                                {
                                  c.EdiVendorID,
                                  c.Name
                                });
    }
  }
}
