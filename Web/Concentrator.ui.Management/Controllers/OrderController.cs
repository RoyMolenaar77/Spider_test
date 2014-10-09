using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class OrderController : BaseController
  {
    [RequiresAuthentication(Functionalities.ViewOrders)]
    public ActionResult GetList(int? OrderID)
    {
      return List(unit => from o in unit.Service<Order>().GetAll()
                          where (OrderID.HasValue ? o.OrderID == OrderID : true)
                          &&
                          (!Client.User.ConnectorID.HasValue || ((Client.User.ConnectorID.HasValue && (o.ConnectorID == Client.User.ConnectorID || o.Connector.ParentConnectorID == Client.User.ConnectorID))))
                          select new OrderViewModel
                          {
                            OrderID = o.OrderID,
                            BackOrdersAllowed = o.BackOrdersAllowed ?? false,
                            IsDispatched = o.IsDispatched,
                            BSKIdentifier = o.BSKIdentifier,
                            ConnectorID = o.ConnectorID,
                            ShipToCustomerID = o.ShipToCustomerID,
                            CustomerOrderReference = o.CustomerOrderReference,
                            EdiVersion = o.EdiVersion,
                            isDropShipment = o.isDropShipment,
                            PaymentInstrument = o.PaymentInstrument,
                            PaymentTermsCode = o.PaymentTermsCode,
                            ReceivedDate = o.ReceivedDate,
                            RouteCode = o.RouteCode,
                            WebSiteOrderNumber = o.WebSiteOrderNumber
                          });

    }
  }


  public class OrderViewModel
  {
    public int OrderID { get; set; }

    public bool BackOrdersAllowed { get; set; }

    public bool IsDispatched { get; set; }

    public string BSKIdentifier { get; set; }

    public int ConnectorID { get; set; }

    public int? ShipToCustomerID { get; set; }

    public string CustomerOrderReference { get; set; }

    public string EdiVersion { get; set; }

    public bool? isDropShipment { get; set; }

    public string PaymentInstrument { get; set; }

    public string PaymentTermsCode { get; set; }

    public DateTime ReceivedDate { get; set; }

    public string RouteCode { get; set; }

    public string WebSiteOrderNumber { get; set; }
  }

}
