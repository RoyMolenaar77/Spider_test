using System.Linq;
using System.Web.Mvc;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Models;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiOrderResponseController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiOrderResponse)]
    public ActionResult GetList(ContentFilter filter)
    {
      return List(unit => (from e in unit.Service<EdiOrderResponse>().GetAll()
                           where filter.ResponseType.HasValue ? filter.ResponseType == e.ResponseType : true
                           select new
                           {
                             e.EdiOrderResponseID,
                             e.ResponseType,
                             e.VendorDocument,
                             e.AdministrationCost,
                             e.DropShipmentCost,
                             e.ShipmentCost,
                             e.OrderDate,
                             e.PartialDelivery,
                             e.VendorDocumentNumber,
                             e.VendorDocumentDate,
                             e.VatPercentage,
                             e.VatAmount,
                             e.TotalGoods,
                             e.TotalExVat,
                             e.TotalAmount,
                             e.PaymentConditionDays,
                             e.PaymentConditionCode,
                             e.PaymentConditionDiscount,
                             e.PaymentConditionDiscountDescription,
                             e.TrackAndTrace,
                             e.InvoiceDocumentNumber,
                             e.ShippingNumber,
                             e.ReqDeliveryDate,
                             e.InvoiceDate,
                             e.Currency,
                             e.DespAdvice,
                             e.ShipToCustomerID,
                             e.SoldToCustomerID,
                             e.ReceiveDate,
                             e.TrackAndTraceLink,
                             e.VendorDocumentReference,
                             e.EdiOrderID
                           }).OrderByDescending(x => x.ReceiveDate));

    }
  }
}
