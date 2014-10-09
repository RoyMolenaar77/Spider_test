using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.Reflection;
using Concentrator.Objects.Models.EDI.Mapping;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiFieldMappingController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiFieldMapping)]
    public ActionResult GetList()
    {
      return List(unit => from e in unit.Service<EdiFieldMapping>().GetAll()
                          select new
                          {
                            e.EdiMappingID,
                            e.TableName,
                            e.FieldName,
                            e.EdiVendorID,
                            e.VendorFieldName,
                            e.VendorTableName,
                            EdiVendorName = e.EdiVendor.Name,
                            e.VendorFieldLength,
                            e.VendorDefaultValue,
                            e.EdiType
                          });
    }

    [RequiresAuthentication(Functionalities.CreateEdiFieldMapping)]
    public ActionResult CreateFieldMapping()
    {
      Assembly a = Assembly.Load("Concentrator.Objects.EDI");
      var tables = a.GetTypes();
      var field = tables[0].Assembly.GetType().GetProperties();

      return Json(new
      {
        success = true
      });
    }
  }
}
