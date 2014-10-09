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
  public class EDIPostController : EDIConcentratorController
  {
  
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult GetList()
    {
      return List(context => from e in context.EdiOrdePosts
                  select new {      
                               e.EdiOrderID,
                               e.CustomerID,
                               e.EdiBackendOrderID,
                               e.CustomerOrderID,
                               e.Processed,
                               e.Type,
                               e.PostDocument,
                               e.PostDocumentUrl,
                               e.PostUrl,
                               e.TimeStamp,
                               e.ResponseRemark,
                               e.ResponseTime,
                               e.ProcessedCount,
                               e.EdiRequestID,
                               e.ErrorMessage,
                               e.BSKIdentifier,
                               e.DocumentCounter,
                               e.ConnectorID
                             });      

    }    


  }
}
