using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Concentrator.Objects.Web;
using Concentrator.Objects.Security;
using System.Reflection;

namespace Concentrator.Administration.EDI.Controllers
{
  public class EDIFieldMappingController : EDIConcentratorController
  {
  
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {       
      return List(context => from e in context.EDIFieldMappings
                  select new {      
                              e.EdiMappingID,
                              e.TableName,
                              e.FieldName,
                              e.EDIVendorID,
                              e.VendorFieldName,
                              e.VendorTableName,
                              e.VendorFieldLength,
                              e.VendorDefaultValue,
                              e.EdiType
                             });         
    }

    [RequiresAuthentication(Functionalities.Default)]
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
