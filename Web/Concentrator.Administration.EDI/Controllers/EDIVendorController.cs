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
using Concentrator.Objects.EDI.Mapping;
using Concentrator.Objects.EDI.Vendor;


namespace Concentrator.Administration.EDI.Controllers
{
  public class EDIVendorController : EDIConcentratorController
  {

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
     
      return List(context => from e in context.EDIVendors
                             select new
                             {
                               e.EdiVendorID,
                               e.Name,     
                               //e.EdiConnectionType,
                               //ConnectionType = Enum.GetName(typeof(EDIConnectorTypeEnum), e.EdiConnectionType),
                               e.EdiVendorType
                             });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetEdiConnectorTypes()
    {
      List<EDIConnectorTypeEnum> enums = EnumHelper.EnumToList<EDIConnectorTypeEnum>();

      return Json(new
      {
        results = (from e in enums
                   select new
                   {
                     EDIConnectionType = (int)e,
                     Name = Enum.GetName(typeof(EDIConnectorTypeEnum), e)
                   }).ToArray()
      });
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Create()
    {
      return Create<EDIVendor>();
    }

    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Search(string query)
    {
      return SimpleList((context) => from c in context.EDIVendors
                                     select new
                                     {
                                       c.EdiVendorID,
                                       c.Name
                                     });
    }

  }
}
