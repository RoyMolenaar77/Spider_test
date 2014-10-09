using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.Web.Mvc;
using Concentrator.Objects.Models.EDI.Post;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiOrderPostController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiOrderPost)]
    public ActionResult GetList()
    {
      return List(unit => (from e in unit.Service<EdiOrderPost>().GetAll()
                           select new
                           {
                             e.EdiOrderPostID,
                             e.EdiOrderID,                             
                             CustomerID = e.ConnectorRelationID,
                             e.EdiBackendOrderID,
                             e.CustomerOrderID,                             
                             e.Processed,
                             e.Type,
                             e.PostDocument,
                             e.PostDocumentUrl,
                             e.PostUrl,
                             e.Timestamp,
                             e.ResponseRemark,
                             e.ResponseTime,
                             e.ProcessedCount,
                             e.EdiRequestID,
                             e.ErrorMessage,
                             e.BSKIdentifier,
                             e.DocumentCounter,
                             e.ConnectorID,
                             Connector = e.ConnectorID.HasValue ? e.Connector.Name : String.Empty,
                             CustomerName = e.ConnectorRelation.Name                             
                           }).OrderByDescending(x => x.Timestamp).AsQueryable());
    }
  }
}
