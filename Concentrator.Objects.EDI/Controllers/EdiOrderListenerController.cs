using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Users;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Web.Shared.Models;

namespace Concentrator.Objects.EDI.Controllers
{
  public class EdiOrderListenerController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEdiOrderListener)]
    public ActionResult GetList(ContentFilter filter)
    {
      return List(unit => (from e in unit.Service<EdiOrderListener>().GetAll()
                           where (filter.ProcessedOrder.HasValue ? e.Processed == true : true) ||
                           (filter.UnprocessedOrder.HasValue ? e.Processed == false : true)
                           select new
                           {
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
                           }).OrderByDescending(x => x.ReceivedDate));
    }
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult ExportDocument(int id)
    {
      using (var unit = GetUnitOfWork())
      {
        try
        {
          var msg = (unit.Service<EdiOrderListener>().Get(x => x.EdiRequestID == id));
          return new FileResult("EDI" + id + ".txt", msg.RequestDocument);
        }
        catch (Exception ex)
        {
          return Json(new
          {
            success = false,
            message = "Error: " + ex.Message
          });
        }
      }
    }

    public class FileResult : ActionResult
    {
      private readonly string _filename;
      private readonly string _message;

      public FileResult(string filename, string message)
      {
        _filename = filename;
        _message = message;
      }

      public override void ExecuteResult(ControllerContext context)
      {
        context.HttpContext.Response.Clear();
        context.HttpContext.Response.ClearContent();
        context.HttpContext.Response.ClearHeaders();
        context.HttpContext.Response.ContentType = "text/plain";
        context.HttpContext.Response.AddHeader("content-disposition", "attachment; filename=" + _filename);
        context.HttpContext.Response.Buffer = false;
        context.HttpContext.Response.BufferOutput = false;

        using (var writer = new StreamWriter(context.HttpContext.Response.OutputStream))
        {
          writer.WriteLine(_message);
          writer.Flush();
        }

        context.HttpContext.Response.End();
      }
    }

  }
}

