using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Management;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Web.Models;

namespace Concentrator.ui.Management.Controllers
{
  public class EventController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEvent)]
    public ActionResult GetList(int? PluginID, int? TypeID, ManagementPortalFilter filter)
    {
      return List(unit => from e in unit.Service<Event>().GetAll()
                          let user = e.User
                          select new
                           {
                             e.EventID,
                             e.TypeID,
                             e.ProcessName,
                             Message = e.Message,
                             CreationTime = e.CreationTime,
                             User = user == null ? string.Empty : user.Firstname + " " + user.Lastname,
                             e.ExceptionMessage,
                             e.ExceptionLocation,
                             e.StackTrace
                           });
    }
  }  
}
