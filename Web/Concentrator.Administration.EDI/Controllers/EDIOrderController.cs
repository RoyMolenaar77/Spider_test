using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.EDI.Order;

namespace Concentrator.ui.Management.Controllers
{
  public class EdiOrderController : BaseController
  {
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      return List(unit => from e in unit.Service<EdiOrder>().GetAll()
                          select new
                          {
                            e.EdiOrderID,
                            e.Document,
                            e.ConnectorID,
                            e.IsDispatched,
                            e.DispatchToVendorDate,
                            e.ReceivedDate,
                            e.isDropShipment,
                            e.Remarks,
                            e.ShipToCustomerID,
                            e.SoldToCustomerID,
                            e.CustomerOrderReference,
                            e.EdiVersion,
                            e.BSKIdentifier,
                            e.WebSiteOrderNumber,
                            e.PaymentTermsCode,
                            e.PaymentInstrument,
                            e.BackOrdersAllowed,
                            e.RouteCode,
                            e.HoldCode,
                            e.HoldOrder
                          });

    }
  }
}
