using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Threading;
using Quartz;
using Quartz.Impl;
using Quartz.Job;
using System.Collections;
using System.ServiceModel;
using System.Net;
using AuditLog4Net.Adapter;
using System.Collections.Specialized;
using Ninject;
using Concentrator.Service.Contracts;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Microsoft.Practices.ServiceLocation;
using Concentrator.Objects.Models.Plugin;
using System.Diagnostics;

namespace Concentrator.Objects.ConcentratorService
{
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  public class ServiceLayer : UnitOfWorkPlugin, IConcentratorService
  {
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Service Layer");

    private static List<Plugin> _plugins = new List<Plugin>();

    private static ServiceLayer _instance;

    public static ServiceLayer Instance
    {
      get
      {
        if (_instance == null)
          _instance = new ServiceLayer();
        return _instance;
      }
    }

    private static Thread _monitor;
    private static volatile bool _running = false;

    private static AutoResetEvent _sync = new AutoResetEvent(false);
    private static ManualResetEvent _waitToEnd = new ManualResetEvent(false);

    private static IScheduler _scheduler;

    private void Init() { }

    public static void StartSingleJob(string jobName)
    {
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        Plugin plugin = Plugin.GetPlugin(jobName, unit);

        if (plugin != null)
        {
          JobDetail job = CreateJob(plugin);
          _scheduler.AddJob(job, true);
          _scheduler.TriggerJob(job.Name, job.Group);
        }
      }
    }

    [DebuggerStepThrough]
    public static void Start(int[] pluginIds = null)
    {
      log.Info("Starting scheduler");

      var properties = new NameValueCollection();
      // set remoting expoter
      properties["quartz.scheduler.exporter.type"] = ConfigurationManager.AppSettings["QuartzType"];
      properties["quartz.scheduler.exporter.port"] = ConfigurationManager.AppSettings["QuartzPort"];
      properties["quartz.scheduler.exporter.bindName"] = ConfigurationManager.AppSettings["QuartzBindName"];
      properties["quartz.scheduler.exporter.channelType"] = ConfigurationManager.AppSettings["QuartzChannelType"];
      properties["quartz.threadPool.type"] = "Quartz.Simpl.SimpleThreadPool, Quartz";
      properties["quartz.threadPool.threadCount"] = "50";
      properties["quartz.threadPool.threadPriority"] = "Normal";


      ISchedulerFactory factory = new StdSchedulerFactory(properties);

      // get a scheduler
      _scheduler = factory.GetScheduler();

      string PluginPath;
      if (ConfigurationManager.AppSettings["PluginPath"] != null)
        PluginPath = ConfigurationManager.AppSettings["PluginPath"].ToString();
      else
        throw new Exception("No pluginpath specified in Appsettings: add PluginPath");

      int jobServerID;
      if (ConfigurationManager.AppSettings["JobServerID"] != null)
        jobServerID = int.Parse(ConfigurationManager.AppSettings["JobServerID"].ToString());
      else
        throw new Exception("No JobServerID specified in Appsettings: add jobServerID");

      string[] files = Directory.GetFiles(PluginPath, "*.dll");
      foreach (string file in files)
      {
        try
        {
          System.Reflection.Assembly.LoadFile(Path.Combine(PluginPath, file));
        }
        catch (Exception ex)
        {
          log.Fatal(ex);
        }
      }

      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {
        var plugins = Plugin.GetPlugins(unit, null, jobServerID);

        if (pluginIds != null)
        {
          Console.WriteLine("Starting for " + String.Join(",", pluginIds));
          plugins = Plugin.GetPlugins(unit, pluginIds, null);

          var plugin = plugins[0];
          //ConcentratorPlugin job = (ConcentratorPlugin)Activator.CreateInstance(plugin.Type);
          //job.Execute();
          //return;
        }

        #region System Plugins
        foreach (Plugin plugin in plugins)//_config.Plugins)
        {
          log.InfoFormat("Plugin found : {0}", plugin.PluginName);
          log.DebugFormat("Using typename : {0}", plugin.PluginType);

          plugin.Configure(log, PluginPath);

          plugin.ConnectorSchedules.ForEach((x, idx) =>
          {
            string connectorScheduleName = x.Plugin.PluginName + "_" + x.Connector.Name;

            JobDetail connectorJobDetail = CreateJob(x.Plugin, connectorScheduleName);
            connectorJobDetail.JobDataMap = new JobDataMap();
            connectorJobDetail.JobDataMap.Add("ConnectorSchedule", x);

            _scheduler.AddJob(connectorJobDetail, true);

#if DEBUG
            bool executeOnStartup = true;
#else
            bool executeOnStartup = x.ExecuteOnStartup ?? plugin.ExecuteOnStartup;
#endif

            if (pluginIds != null)
              executeOnStartup = true;

            string cronExpression = x.CronExpression ?? plugin.CronExpression;

            SetTrigger(x.Plugin, connectorJobDetail, connectorScheduleName, executeOnStartup, cronExpression);
          });


          JobDetail jobDetail = CreateJob(plugin);
          jobDetail.JobDataMap = new JobDataMap();
          jobDetail.JobDataMap.Add("Plugin", plugin);

          _scheduler.AddJob(jobDetail, true);
          var j = _scheduler.GetJobDetail(jobDetail.Name, jobDetail.Group);

          log.DebugFormat("The plugin's execute on startup is {0}", plugin.ExecuteOnStartup);

          SetTrigger(plugin, jobDetail, plugin.PluginName, plugin.ExecuteOnStartup, plugin.CronExpression);
          //if (!String.IsNullOrEmpty(plugin.CronExpression))
          //{
          //  CronTrigger trigger = new CronTrigger(plugin.PluginName + " Trigger", plugin.PluginGroup, plugin.PluginName, plugin.PluginGroup, plugin.CronExpression);
          //  trigger.StartTimeUtc = DateTime.Now.ToUniversalTime();
          //  _scheduler.ScheduleJob(trigger);
          //}

          //if (plugin.ExecuteOnStartup)
          //{
          //  //manually trigger this job per config settings
          //  _scheduler.TriggerJob(jobDetail.Name, jobDetail.Group);
          //}
        }
        #endregion

        _scheduler.Start();
        _running = true;

        log.DebugFormat(String.Format("{0}  {1}", "Plugin".PadRight(30), "Next Execution Time".PadRight(25)));
        log.DebugFormat(new String('-', 60));
        log.DebugFormat("Going to schedule {0} plugin(s)", plugins.Count);

        foreach (Plugin plugin in plugins)
        {
          plugin.ConnectorSchedules.ForEach((x, idx) =>
          {
            string connectorScheduleName = x.Plugin.PluginName + "_" + x.Connector.Name;
            Trigger[] triggersc = _scheduler.GetTriggersOfJob(connectorScheduleName, plugin.PluginGroup);
            foreach (Trigger t in triggersc)
            {
              if (t.Name.StartsWith("MT_")) // skip manual triggers in listing
                continue;

              var next = t.GetNextFireTimeUtc().HasValue ? t.GetNextFireTimeUtc().Value.ToLocalTime() : DateTime.MinValue;

              log.DebugFormat("{0}  {1}", plugin.PluginName.PadRight(30), (next.ToString() ?? String.Empty).PadRight(25));
            }
          });

          Trigger[] triggers = _scheduler.GetTriggersOfJob(plugin.PluginName, plugin.PluginGroup);

          foreach (Trigger t in triggers)
          {
            if (t.Name.StartsWith("MT_")) // skip manual triggers in listing
              continue;

            var next = t.GetNextFireTimeUtc().HasValue ? t.GetNextFireTimeUtc().Value.ToLocalTime() : DateTime.MinValue;

            log.DebugFormat("{0}  {1}", plugin.PluginName.PadRight(30), (next.ToString() ?? String.Empty).PadRight(25));

          }
        }
        log.DebugFormat(new String('-', 60));
      }

      _monitor = new Thread(TimerThread);
      _monitor.Start();

    }

    private static void SetTrigger(Plugin plugin, JobDetail jobDetail, string pluginName, bool executeOnStartup, string cronExpression)
    {
      if (!String.IsNullOrEmpty(cronExpression) && cronExpression != "ALWAYS")
      {
        CronTrigger trigger = new CronTrigger(pluginName, plugin.PluginGroup, pluginName, plugin.PluginGroup, cronExpression);
        trigger.StartTimeUtc = DateTime.Now.ToUniversalTime();
        _scheduler.ScheduleJob(trigger);
      }
      log.Info("Execute on startup for " + plugin.PluginName + " is " + plugin.ExecuteOnStartup);
#if DEBUG
      executeOnStartup = true;
#endif

      if (executeOnStartup || cronExpression == "ALWAYS")
      {
        //manually trigger this job per config settings
        _scheduler.TriggerJob(jobDetail.Name, jobDetail.Group);
      }

    }

    private static JobDetail CreateJob(Plugin plugin, string name = null)
    {
      JobDetail jobDetail;
      if (string.IsNullOrEmpty(name))
        jobDetail = new JobDetail(plugin.PluginName, plugin.PluginGroup, plugin.Type);
      else
        jobDetail = new JobDetail(name, plugin.PluginGroup, plugin.Type);

      return jobDetail;
    }

    public static void TimerThread()
    {
      while (_running)
      {
        _sync.WaitOne();
      }
    }

    public static void Stop()
    {
      log.Info("Stopping scheduler");


      IList jobs = _scheduler.GetCurrentlyExecutingJobs();
      foreach (object obj in jobs)
      {
        if (obj != null && obj is JobExecutionContext)

          if (((JobExecutionContext)obj).JobInstance is IInterruptableJob)
            ((IInterruptableJob)((JobExecutionContext)obj).JobInstance).Interrupt();
      }

      _scheduler.Shutdown();

      // signal to stop working 
      _running = false;
      _sync.Set();
      _monitor.Join();
    }

    private static List<Type> GetPluginTypes(string pluginFile, Type type)
    {
      try
      {
        System.Reflection.Assembly assembly = null;
        string typeName = string.Empty;


        if (File.Exists(pluginFile))
        {
          assembly = System.Reflection.Assembly.LoadFile(pluginFile);
        }
        else
        {
          log.FatalFormat("Plugin file {0} not found", pluginFile);
          return null;
        }

        if (assembly != null)
        {
          List<Type> list = assembly.GetTypes().ToList()
              .Where(p => p.IsSubclassOf(type)).ToList();

          return list;

        }

      }
      catch (System.BadImageFormatException)
      {
        log.FatalFormat("The Assembly is invalid--it is not a valid .NET Assembly file");
      }

      return new List<Type>();


    }

    #region IMiddleWareService Members




    List<JobInfo> IConcentratorService.GetScheduledJobs()
    {

      IList jobs = _scheduler.GetCurrentlyExecutingJobs();
      Dictionary<string, List<string>> executingJobs = new Dictionary<string, List<string>>();

      foreach (object obj in jobs)
      {
        JobExecutionContext ctx = obj as JobExecutionContext;
        if (ctx != null)
        {
          if (!executingJobs.ContainsKey(ctx.JobDetail.Group))
            executingJobs[ctx.JobDetail.Group] = new List<string>();

          executingJobs[ctx.JobDetail.Group].Add(ctx.JobDetail.Name);
        }
      }

      List<JobInfo> result = new List<JobInfo>();
      using (var unit = ServiceLocator.Current.GetInstance<IUnitOfWork>())
      {

        foreach (Plugin plugin in Plugin.GetPlugins(unit))
        {
          Trigger[] triggers = _scheduler.GetTriggersOfJob(plugin.PluginName, plugin.PluginGroup);

          JobInfo info = new JobInfo
          {
            JobName = plugin.PluginName,
            Group = plugin.PluginGroup,
            IsExecuting = false
          };

          if (triggers.Length > 0)
          {
            var res = (from t in triggers
                       select new
                       {
                         LastExecutionTime = t.GetPreviousFireTimeUtc(),
                         NextExecutionTime = t.GetNextFireTimeUtc()
                       }).FirstOrDefault();


            if (res != null)
            {
              info.NextExecutionTime = res.NextExecutionTime;
              info.LastExecutionTime = res.LastExecutionTime;
            }

          }

          info.IsExecuting = (executingJobs.ContainsKey(plugin.PluginGroup) && executingJobs[plugin.PluginGroup].Contains(plugin.PluginName));


          result.Add(info);
        }
      }
      return result;
    }

    public void StartJob(string jobName)
    {
      StartSingleJob(jobName);
    }


    public void StopJob(string jobName)
    {
      IList jobs = _scheduler.GetCurrentlyExecutingJobs();
      foreach (object obj in jobs)
      {
        JobExecutionContext context = obj as JobExecutionContext;

        if (context == null || context.JobDetail.Name != jobName)
          continue;

        if ((context).JobInstance is IInterruptableJob)
          ((IInterruptableJob)(context).JobInstance).Interrupt();
      }
    }

    #endregion

    private List<string> GetExecutingJobs()
    {
      IList list = _scheduler.GetCurrentlyExecutingJobs();
      List<string> executingJobs = new List<string>();
      foreach (var obj in list)
      {
        JobExecutionContext context = obj as JobExecutionContext;

        if (context != null)
          executingJobs.Add(context.JobDetail.Name);
      }

      return executingJobs;
    }
  }
}
