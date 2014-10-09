using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.Web.Mvc;
using System.Reflection;
using Concentrator.Objects.Models.EDI.Validation;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiValidateController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiValidate)]
    public ActionResult GetList()
    {
      return List(unit => from e in unit.Service<EdiValidate>().GetAll()
                          select new
                          {
                            e.EdiValidateID,
                            e.TableName,
                            e.FieldName,
                            e.EdiVendorID,
                            EdiVendorName = e.EdiVendor.Name,
                            e.MaxLength,
                            e.Type,
                            e.Value,
                            e.IsActive,
                            e.EdiType,
                            e.EdiValidationType,
                            e.Connection,
                            e.EdiConnectionType                            
                          });
    }

    [RequiresAuthentication(Functionalities.GetEdiValidate)]
    public ActionResult GetTables()
    {
      var tables = Assembly.Load("Concentrator.Objects.EDI").GetTypes().Where(x => x.Name.StartsWith("EDI")).Select(x => new { x.Name, x.FullName }).ToList();

      var tableList = (from t in tables
                       select new
                       {
                         ID = t.FullName,
                         t.Name
                       });

      return Json(new
      {
        total = tableList.Count(),
        results = tableList.ToList()
      });
    }

    [RequiresAuthentication(Functionalities.UpdateEdiValidate)]
    public ActionResult Update(int ediValidateID, string tableValue, string fieldValue)
    {
      return Update<EdiValidate>(x => x.EdiValidateID == ediValidateID, (unit, ediModel) =>
      {
        ediModel.TableName = tableValue;
        ediModel.FieldName = fieldValue;
      });
    }
  }
}
