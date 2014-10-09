using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Management;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class EventTypeController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetEventType)]
    public ActionResult GetStore()
    {
      return Json(new
      {
        eventTypes = SimpleList<EventType>(c => new
        {
          c.TypeID,
          c.Type
        })
      });
    }

    [RequiresAuthentication(Functionalities.GetEventType)]
    public ActionResult Search(string query)
    {
      using (var unit = GetUnitOfWork())
      {
        var events = unit.Scope.Repository<EventType>().GetAll().Select(d => new { d.TypeID, d.Type });

        return Json(new { results = events.ToList() });
      }
    }
  }
}