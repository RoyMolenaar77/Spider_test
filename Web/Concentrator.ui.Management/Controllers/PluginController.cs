using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Web.Shared;
using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Plugin;
using Quartz;
using Concentrator.Objects.Web.Models;
using Concentrator.ui.Management.Models.Anychart;
using Concentrator.Objects.Models.Management;
using System.Collections.Specialized;

namespace Concentrator.ui.Management.Controllers
{
  public class PluginController : BaseController
  {
    [RequiresAuthentication(Functionalities.GetPlugin)]
    public ActionResult GetList()
    {
      return List(unit => from v in unit.Service<Plugin>().GetAll()
                          select new
                          {
                            v.PluginID,
                            v.PluginName,
                            v.PluginType,
                            v.PluginGroup,
                            v.CronExpression,
                            v.ExecuteOnStartup,
                            v.LastRun,
                            v.NextRun,
                            v.Duration,
                            v.IsActive
                          });
    }


    [RequiresAuthentication(Functionalities.GetPlugin)]
    public ActionResult Search(string query)
    {
      using (var unit = GetUnitOfWork())
      {
        var plugins = unit.Scope.Repository<Plugin>().GetAll().Select(d => new { d.PluginID, d.PluginName });

        return Json(new { results = plugins.ToList() });
      }
    }

    [RequiresAuthentication(Functionalities.CreatePlugin)]
    public ActionResult Create()
    {
      return Create<Plugin>();
    }

    [RequiresAuthentication(Functionalities.UpdatePlugin)]
    public ActionResult EditCron(int pluginID, int? Month, int? Day, int? Hour, int? Minute, int? Second)
    {
      return Create<Plugin>((unit, plugin) =>
      {
      });
    }

    private static readonly StringDictionary EventTypeColors = new StringDictionary
    {
      { "Info",     "Grey"    },
      { "Warning",  "Orange"  },
      { "Error",    "Red"     },
      { "Fatal",    "Red"     },
      { "Success",  "Green"   },
      { "Critical", "Yellow"  },
      { "Complete", "Green"   }
    };

    [RequiresAuthentication(Functionalities.GetImage)]
    public ActionResult GetEventsChart(ManagementPortalFilter filter, bool? mb, bool? kb, int? PluginID)
    {
      MergeSession(filter, ManagementPortalFilter.SessionKey);

      using (var unit = GetUnitOfWork())
      {
        var events = unit.Scope
          .Repository<Event>()
          .Include(@event => @event.EventType)
          .Include(@event => @event.Plugin)
          .GetAll(@event 
            => (!PluginID.HasValue || @event.PluginID == PluginID.Value)
            && filter.FromDate != null
              ? @event.CreationTime >= filter.FromDate
              : @event.CreationTime >= DateTime.MinValue
            && filter.UntilDate != null
              ? @event.CreationTime <= filter.UntilDate
              : @event.CreationTime <= DateTime.MaxValue)
          .ToArray();

        var series = events
          .GroupBy(@event => @event.EventType.Type)
          .Select(eventsPerEventType => new Serie(eventsPerEventType
            .GroupBy(@event => @event.Plugin.PluginName)
            .Select(eventsPerPluginName => new PieChartPoint(eventsPerPluginName.Key, eventsPerPluginName.Count(), EventTypeColors[eventsPerEventType.Key] ?? "Black"))
            , eventsPerEventType.Key));

        return View("Anychart/Default2DStackedBarChart", new AnychartComponentModel(series));
      }
    }

    [RequiresAuthentication(Functionalities.UpdatePlugin)]
    public ActionResult Update(int id)
    {
      return Update<Plugin>(c => c.PluginID == id);
    }

    [RequiresAuthentication(Functionalities.DeletePlugin)]
    public ActionResult Delete(int id)
    {
      return Delete<Plugin>(c => c.PluginID == id);
    }    
  }
}