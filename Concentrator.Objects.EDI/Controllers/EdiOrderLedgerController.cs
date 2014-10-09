using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.Web.Mvc;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiOrderLedgerController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiOrderLedger)]
    public ActionResult GetList(int ediOrderLineID)
    {
      return List(unit => from e in unit.Service<EdiOrderLedger>().GetAll(e => e.EdiOrderLineID == ediOrderLineID)               
                          select new
                          {
                            e.EdiOrderLedgerID,
                            e.EdiOrderLineID,
                            e.Status,
                            e.LedgerDate
                          });
    }
  }
}
