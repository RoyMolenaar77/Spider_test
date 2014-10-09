using System;
using System.Linq;
using System.Web.Mvc;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.ui.Management.Models;
using Concentrator.Objects.Models.Forms;
using Concentrator.Objects.Models.Users;

namespace Concentrator.ui.Management.Controllers
{
  public class CommentsController : BaseController
  {
    public ActionResult Index(bool? success = null)
    {
      if (success.HasValue)
        ViewBag.Success = true;

      return View();
    }

    public ActionResult Create(CommentsModel model)
    {
      using (var unit = GetUnitOfWork())
      {
        unit.Service<PortalNotification>().Create(new PortalNotification()
        {
          Name = model.EmployeeName,
          Priority = model.Priority,
          ArticleNumber = model.ArticleNumber,
          CreationTime = DateTime.Now,
          Description = model.Description,
          NotificationType = (int)model.Notification,
          ProductName = model.ProductName,
          IsResolved = false
        });

        unit.Save();
      }

      return RedirectToAction("Index", new { success = true });
    }

    [RequiresAuthentication(Functionalities.GetComments)]
    public ActionResult GetList()
    {
      return List(unit =>
          (from c in unit.Service<PortalNotification>().GetAll().ToList()
           select new
           {
             c.ArticleNumber,
             c.Description,
             c.CreationTime,
             CommentID = c.FormID,
             Username = c.Name,
             c.Priority,
             c.ProductName,
             NotificationType = ((FormNotificationType)c.NotificationType).ToString(),
             c.IsResolved
           }).AsQueryable() // temp
        );
    }

    [RequiresAuthentication(Functionalities.GetComments)]
    public ActionResult Get(int id)
    {
      using (var unit = GetUnitOfWork())
      {
        var c = unit.Service<PortalNotification>().Get(l => l.FormID == id);

        return Json(new
        {
          success = true,
          data = new
          {
            c.ArticleNumber,
            c.Description,
            CommentID = c.FormID,
            Username = c.Name,
            c.Priority,
            Product = c.ProductName,
            NotificationType = ((FormNotificationType)c.NotificationType).ToString(),
            c.IsResolved
          }
        });
      }
    }

    [RequiresAuthentication(Functionalities.UpdateComments)]
    public ActionResult Update(int id)
    {
      return Update<PortalNotification>(c => c.FormID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteComments)]
    public ActionResult Delete(int id)
    {
      return Delete<PortalNotification>(c => c.FormID == id);
    }
  }
}
