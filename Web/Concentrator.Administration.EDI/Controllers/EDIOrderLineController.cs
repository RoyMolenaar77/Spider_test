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
  public class EDIOrderLineController : EDIConcentratorController
  {
  
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList(int ediOrderID)
    {
      return List(context => from e in context.EDIOrderLines
                             where e.EdiOrderID == ediOrderID
                             select new {      
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
