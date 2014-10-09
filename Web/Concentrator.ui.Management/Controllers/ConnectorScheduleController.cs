using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using Concentrator.Objects.Models.Brands;
using Concentrator.Objects.Models.Products;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.ui.Management.Controllers
{
  public class ConnectorScheduleController : BaseController
  {

    [RequiresAuthentication(Functionalities.GetConnectorSchedule)]
    public ActionResult GetList(int pluginID)
    {

      return List(unit => from b in unit.Service<ConnectorSchedule>().GetAll()
                          where b.PluginID == pluginID
                          select new
                          {
                            b.ConnectorScheduleID,
                            b.ConnectorID,
                            ConnectorName = b.Connector.Name,
                            b.Plugin.PluginName,
                            b.LastRun,
                            b.Duration,
                            b.ScheduledNextRun,
                            b.ConnectorScheduleStatus,
                            b.ExecuteOnStartup,
                            b.CronExpression
                          });
    }

    [RequiresAuthentication(Functionalities.CreateConnectorSchedule)]
    public ActionResult Create()
    {
      return Create<ConnectorSchedule>();
    }

    [RequiresAuthentication(Functionalities.UpdateConnectorSchedule)]
    public ActionResult Update(int id)
    {
      return Update<ConnectorSchedule>(c => c.ConnectorScheduleID == id);
    }

    [RequiresAuthentication(Functionalities.DeleteConnectorSchedule)]
    public ActionResult Delete(int id)
    {
      return Delete<ConnectorSchedule>(c => c.ConnectorScheduleID == id);
    }

  }
}