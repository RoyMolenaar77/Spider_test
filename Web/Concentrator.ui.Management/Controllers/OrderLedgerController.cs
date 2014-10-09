using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Orders;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class OrderLedgerController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetOrderLedger)]
    public ActionResult GetList(int orderLineID)
    {
      return List(unit =>
                  from o in unit.Service<OrderLedger>().GetAll(c => c.OrderLedgerID == orderLineID)
                  select new
                  {
                    o.OrderLedgerID,
                    o.OrderLineID,
                    o.Status,
                    LedgerDate = o.LedgerDate.ToLocalTime()
                  });
    }
  }
}
