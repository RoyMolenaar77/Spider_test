using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Users;
using System.Collections.Specialized;
using System.Configuration;
using Quartz;
using Concentrator.Objects.ConcentratorService;
using Quartz.Impl;
using Concentrator.Objects.ConcentratorService.Scheduler;
using Concentrator.Objects.ConcentratorService.Scheduler.Provider;
using Concentrator.Web.Shared; using Concentrator.Web.Shared.Controllers;

namespace Concentrator.ui.Management.Controllers
{
  public class MiddleWareController : BaseController
  {
    [RequiresAuthentication(Functionalities.StartJob)]
    [AcceptVerbs(HttpVerbs.Post)]
    public ActionResult StartJob(string jobName, string groupName)
    {
      ISchedulerProvider provider = new RemoteSchedulerProvider { SchedulerHost = "tcp://localhost:555/QuartzScheduler" };
      provider.Init();

      var properties = new NameValueCollection();
      properties["quartz.scheduler.exporter.type"] = ConfigurationManager.AppSettings["QuartzType"];
      properties["quartz.scheduler.exporter.port"] = ConfigurationManager.AppSettings["QuartzPort"];
      properties["quartz.scheduler.exporter.bindName"] = ConfigurationManager.AppSettings["QuartzBindName"];
      properties["quartz.scheduler.exporter.channelType"] = ConfigurationManager.AppSettings["QuartzChannelType"];


      ISchedulerFactory fact = new StdSchedulerFactory(properties);
      var scheduler = fact.GetScheduler();
      try
      {
        scheduler.TriggerJob(jobName, groupName);

        return Success("Job started");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong " + e.Message);
      }

    }

    [AcceptVerbs(HttpVerbs.Get)]
    [RequiresAuthentication(Functionalities.ViewJobs)]
    public ActionResult GetList(int limit, int start, string dir)
    {
      ISchedulerProvider provider = new RemoteSchedulerProvider { SchedulerHost = "tcp://localhost:555/QuartzScheduler" };
      provider.Init();
      ISchedulerDataProvider dataProvider = new DefaultSchedulerDataProvider(provider);

      List<JobDetailsData> details = new List<JobDetailsData>();
      foreach (var group in provider.Scheduler.JobGroupNames)
      {
        foreach (var job in provider.Scheduler.GetJobNames(group))
        {
          details.Add(dataProvider.GetJobDetailsData(job, group));
        }
      }

      return Json(new
      {
        total = details.Count,
        results = (from m in details.Select(x => x.PrimaryData)
                   select new
                   {
                     m.Name,
                     m.GroupName,
                     PreviousFireDate = m.Triggers.FirstOrDefault().Try(c => c.PreviousFireDate, null),
                     NextFireDate = m.Triggers.FirstOrDefault().Try(c => c.NextFireDate, null),
                     Status = m.CanStart ? "Scheduled" : "Executing",
                   }
                     )
      });

    }

    [AcceptVerbs(HttpVerbs.Post)]
    [RequiresAuthentication(Functionalities.StopJob)]
    public ActionResult StopJob(string jobName, string groupName)
    {
      ISchedulerProvider provider = new RemoteSchedulerProvider { SchedulerHost = "tcp://localhost:555/QuartzScheduler" };
      provider.Init();

      var properties = new NameValueCollection();
      properties["quartz.scheduler.exporter.type"] = ConfigurationManager.AppSettings["QuartzType"];
      properties["quartz.scheduler.exporter.port"] = ConfigurationManager.AppSettings["QuartzPort"];
      properties["quartz.scheduler.exporter.bindName"] = ConfigurationManager.AppSettings["QuartzBindName"];
      properties["quartz.scheduler.exporter.channelType"] = ConfigurationManager.AppSettings["QuartzChannelType"];


      ISchedulerFactory fact = new StdSchedulerFactory(properties);
      var scheduler = fact.GetScheduler();
      try
      {
        scheduler.Interrupt(jobName, groupName);

        return Success("Job stoped");
      }
      catch (Exception e)
      {
        return Failure("Something went wrong " + e.Message);
      }
    }
  }
}
