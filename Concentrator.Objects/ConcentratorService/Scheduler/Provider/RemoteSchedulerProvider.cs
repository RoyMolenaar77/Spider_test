using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.ConcentratorService.Scheduler.Provider
{
  public class RemoteSchedulerProvider : StdSchedulerProvider
  {
    public string SchedulerHost { get; set; }

    protected override bool IsLazy
    {
      get { return true; }
    }

    protected override NameValueCollection GetSchedulerProperties()
    {
      var properties = base.GetSchedulerProperties();
      properties["quartz.scheduler.proxy"] = "true";
      properties["quartz.scheduler.proxy.address"] = SchedulerHost;
      return properties;
    }
  }

}
