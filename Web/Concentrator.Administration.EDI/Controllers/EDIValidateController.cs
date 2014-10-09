using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Concentrator;
using Concentrator.Objects.Product;
using Concentrator.Objects.Security;
using Concentrator.Objects.Web;
using Concentrator.Objects.Enumaration;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects;
using System.Reflection;
using Concentrator.Objects.EDI.Validation;
using Concentrator.Objects.EDI.EDI.Communication;

namespace Concentrator.Administration.EDI.Controllers
{
  public class EDIValidateController : EDIConcentratorController
  {

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      return List(context => from e in context.EDIValidations
                             select new
                             {
                               e.EdiValidateID,
                               e.TableName,
                               e.FieldName,
                               e.EdiVendorID,
                               e.MaxLength,
                               e.Type,
                               e.Value,
                               e.IsActive,
                               e.EdiType,
                               e.EdiConnectionType,
                               ConnectionType = Enum.GetName(typeof(EDIConnectionType), e.EdiConnectionType)
                             });

    }

    [RequiresAuthentication(Functionalities.Default)]
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
                    }
                 );
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetFields(string tableFullName)
    {
      var fields = Assembly.Load("Concentrator.Objects.EDI").GetType(tableFullName).GetProperties().Where(x => x.Name != null).Select(x => new { x.Name } ).ToList();

      foreach (var item in fields)
      {
          if (item == null)
          fields.Remove(item);
      }

      return Json(new
                    {
                      total = fields.Count(),
                      results = fields.ToList()
                    }
                 );
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Update(int ediValidateID, string tableValue, string fieldValue)
    {
      return Update<EDIValidate>(x => x.EdiValidateID == ediValidateID, (ediModel, context) =>
      { 
        ediModel.TableName = tableValue;
        ediModel.FieldName = fieldValue;
      });
    }

  }
}
