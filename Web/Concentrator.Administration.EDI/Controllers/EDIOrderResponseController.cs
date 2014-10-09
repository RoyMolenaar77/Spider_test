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
  public class EDIOrderResponseController : EDIConcentratorController
  {
  
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      return List(context => from e in context.EDIOrderResponses
                  select new {      
                              e.EdiOrderResponseID,
                              e.ResponseType,
                              e.VendorDocument,
                              e.VendorID,
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
                             });
      


      

    }    


  }
}
