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


namespace Concentrator.Administration.EDI.Controllers
{
  public class EDIOrderLedgerController : EDIConcentratorController
  {
  
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList(int ediOrderLineID)
    {
      return List(context => from e in context.EDIOrderLedgers
                             where e.EdiOrderLineID == ediOrderLineID
                             select new {      
                                          e.EdiOrderLedgerID,
                                          e.EdiOrderLineID,
                                          e.Status,
                                          e.LedgerDate
                                        });     

    }    


  }
}
