using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using Concentrator.Objects.Models.Management;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Web;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class ApplicationController : BaseController
  {
    [RequiresAuthentication(Functionalities.Default)]
    public ActionResult Index()
    {
      if (Client.User != null)
      {
        using (var unit = GetUnitOfWork())
        {
          var pages = (
            from page in unit.Service<ManagementPage>().GetAll(p => p.isVisible.HasValue && p.isVisible.Value).ToArray()
            join user in unit.Service<User>().GetAll(u => u.UserID == Client.User.UserID).SelectMany(u => u.UserRoles.SelectMany(ur => ur.Role.FunctionalityRoles)).ToArray() on page.FunctionalityName equals user.FunctionalityName
            group page by page.ManagementGroup into pageGrouping
            let pageGroup = pageGrouping.Key
            select new
            {
              id = pageGroup.Group + "-management-group",
              title = pageGroup.Group,
              portalID = pageGroup.PortalID,
              portalName = pageGroup.Portal == null ? string.Empty : pageGroup.Portal.Name,
              children = pageGrouping.Select(c => new
              {
                c.ID,
                c.PageID,
                c.Name,
                c.Description,
                c.Icon,
                action = c.JSAction
              })
            }).OrderBy(c => c.title).ToArray();

          ViewData["ManagementPages"] = new JavaScriptSerializer().Serialize(pages);
        }
      }
      else
      {
        ViewData["ManagementPages"] = "[]";
      }

      var applicationName = string.Empty;
      var applicationNameSetting = ConfigurationManager.AppSettings["ApplicationName"];

      if (!String.IsNullOrEmpty(applicationNameSetting))
      {
        applicationName = applicationNameSetting;
      }

      ViewData["ApplicationName"] = applicationName;

      var moduleSetting = ConfigurationManager.AppSettings["ClientModules"];

      if (moduleSetting != null)
      {
        ViewData["AdditionalIncludes"] = moduleSetting
          .Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)
          .SelectMany(module => Directory.GetFiles(Server.MapPath("~/Scripts/Custom/" + module)).Select(filePath => String.Format("{0}/{1}", module, Path.GetFileName(filePath))))
          .Distinct()
          .ToArray();
      }

      return View();
    }
  }
}
