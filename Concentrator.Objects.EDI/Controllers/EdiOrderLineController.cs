using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiOrderLineController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiOrderLine)]
    public ActionResult GetList()
    {
      return List(unit => from e in unit.Service<EdiOrderLine>().GetAll()
                          select new 
                          {
                            e.EdiOrderLineID,
                            e.Remarks,
                            e.EdiOrderID,
                            e.CustomerEdiOrderLineNr,
                            e.CustomerOrderNr,
                            e.ProductID,
                            e.Price,
                            e.Quantity,
                            e.isDispatched,
                            e.VendorOrderNumber,
                            e.Response,
                            e.CentralDelivery,
                            e.CustomerItemNumber,
                            e.WareHouseCode,
                            e.PriceOverride
                          });
    }
  }
}
