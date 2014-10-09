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
  public class EDIOrderResponseLineController : EDIConcentratorController
  {
  
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList(int ediOrderResponseID)
    {
      return List(context => from e in context.EDIOrderResponseLines
                             where e.EdiOrderResponseID == ediOrderResponseID
                             select new {      
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
                              e.Processed,
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
                              e.Html
                             });      
    }    


  }
}
