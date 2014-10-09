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
  public class EDIOrderListenerController : EDIConcentratorController
  {
  
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      return List(context => from e in context.EDIOrderListeners
                  select new {      
                              e.EdiRequestID,
                              e.CustomerName,
                              e.CustomerIP,
                              e.CustomerHostName,
                              e.RequestDocument,
                              e.ReceivedDate,
                              e.Processed,
                              e.ResponseRemark,                              
                              e.ResponseTime,
                              e.ErrorMessage
                             });           

    }    


  }
}
