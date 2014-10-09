using System.Linq;
using System.Web.Mvc;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared.Models;
using System;
using Concentrator.Objects.Models.EDI.Enumerations;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiOrderController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiOrder)]
    public ActionResult GetList(ContentFilter filter)
    {
      return List(unit => (from e in unit.Service<EdiOrder>().GetAll().ToList()
                           where filter.Status.HasValue ? e.Status == filter.Status : true
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
                           }).AsQueryable().OrderByDescending(x => x.EdiOrderID));
    }
  }
}
