using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.Web.Mvc;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiOrderResponseLineController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiOrderResponseLine)]
    public ActionResult GetList(int ediOrderResponseID)
    {
      return List(unit => from e in unit.Service<EdiOrderResponseLine>().GetAll(x => x.EdiOrderResponseID == ediOrderResponseID)
                          select new 
                          {
                            e.EdiOrderResponseLineID,
                            e.EdiOrderResponseID,
                            e.EdiOrderLineID,
                            e.Ordered,
                            e.Backordered,
                            e.Cancelled,
                            e.Shipped,
                            e.Invoiced,
                            e.Unit,
                            e.Price,
                            e.DeliveryDate,
                            e.VendorLineNumber,
                            e.VendorItemNumber,
                            e.OEMNumber,
                            e.Barcode,
                            e.Remark,
                            e.Description,
                            e.processed,
                            e.RequestDate,
                            e.VatAmount,
                            e.CarrierCode,
                            e.NumberOfPallets,
                            e.NumberOfUnits,
                            e.TrackAndTrace,
                            e.SerialNumbers,
                            e.Delivered,
                            e.TrackAndTraceLink,
                            e.ProductName,
                            e.html
                          });
    }
  }
}
