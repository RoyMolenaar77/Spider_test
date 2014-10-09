using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;


namespace Concentrator.Objects.Models.Connectors
{
  public class ConnectorSchedule : BaseModel<ConnectorSchedule>
  {
    public int ConnectorID { get; set; }

    public int ConnectorScheduleID { get; set; }

    public int PluginID { get; set; }

    public DateTime? LastRun { get; set; }

    public string Duration { get; set; }

    public DateTime? ScheduledNextRun { get; set; }

    public virtual Int32 ConnectorScheduleStatus { get; set; }

    public bool? ExecuteOnStartup { get; set; }

    public virtual Connector Connector { get; set; }

    public virtual Plugin.Plugin Plugin { get; set; }

    public String CronExpression { get; set; }

    public override System.Linq.Expressions.Expression<Func<ConnectorSchedule, bool>> GetFilter()
    {
      return null;
    }
  }
}
