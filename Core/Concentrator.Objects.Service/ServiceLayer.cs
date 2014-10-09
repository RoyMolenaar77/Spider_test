using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;
using System.IO;
using System.Threading;
using Concentrator.Objects.Configuration;
using Concentrator.Service.Contracts;
using Quartz;
using Quartz.Impl;
using Quartz.Job;
using System.Collections;
using System.ServiceModel;
using System.Net;
using AuditLog4Net.Adapter;
using System.Collections.Specialized;
using Ninject;

namespace Concentrator.Objects.Service
{
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  public class ServiceLayer : IConcentratorService
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
    private static ConcentratorConfiguration _config;
    private static ServiceHost _host;

    private void Init() { }

    public static void StartSingleJob(string jobName)
    {

      _config.ScanPlugins();

      Plugin plugin = _config.GetPlugin(jobName);



      if (plugin != null)
      {
        JobDetail job = CreateJob(plugin);
        _scheduler.AddJob(job, true);
        _scheduler.TriggerJob(job.Name, job.Group);
      }
    }

    public static void Start()
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

      _config = (ConcentratorConfiguration)ConfigurationManager.GetSection("ConcentratorConfiguration");

      #region System Plugins

      // Read the plugin path for available dll's
      _config.ScanPlugins();

      foreach (Plugin plugin in _config.Plugins)
      {
        log.InfoFormat("Plugin found : {0}", plugin.Name);
        log.DebugFormat("Using typename : {0}", plugin.TypeName);

        plugin.Configure();

        JobDetail jobDetail = CreateJob(plugin);

        _scheduler.AddJob(jobDetail, true);

        if (!String.IsNullOrEmpty(plugin.CronExpressionString))
        {
          CronTrigger trigger = new CronTrigger(plugin.Name + " Trigger", plugin.Group, plugin.Name, plugin.Group, plugin.CronExpressionString);
          trigger.StartTimeUtc = DateTime.Now.ToUniversalTime();
          _scheduler.ScheduleJob(trigger);
        }

        if (plugin.ExecuteOnStartUp)
        {
          //manually trigger this job per config settings
          _scheduler.TriggerJob(jobDetail.Name, jobDetail.Group);
        }
      }

      #endregion

      _scheduler.Start();
      _running = true;

      log.DebugFormat(String.Format("{0}  {1}", "Plugin".PadRight(30), "Next Execution Time".PadRight(25)));
      log.DebugFormat(new String('-', 60));
      foreach (Plugin plugin in _config.Plugins)
      {

        Trigger[] triggers = _scheduler.GetTriggersOfJob(plugin.Name, plugin.Group);
        foreach (Trigger t in triggers)
        {
          if (t.Name.StartsWith("MT_")) // skip manual triggers in listing
            continue;

          log.DebugFormat("{0}  {1}", plugin.Name.PadRight(30), (t.GetNextFireTimeUtc().ToString() ?? String.Empty).PadRight(25));
        }
      }
      log.DebugFormat(new String('-', 60));

      _monitor = new Thread(TimerThread);
      _monitor.Start();

      //#if !DEBUG
      //      if (ConfigurationManager.AppSettings["PluginListener"] != null)
      //      {
      //        string uri = string.Format("net.tcp://{0}:{1}/MiddleWareService", Dns.GetHostName(), int.Parse(ConfigurationManager.AppSettings["PluginListener"]));
      //        _host = new ServiceHost(Instance, new Uri(uri));
      //        _host.AddServiceEndpoint(typeof(IConcentratorService),
      //                                new NetTcpBinding(),
      //                                 new Uri(uri));
      //        _host.Open();
      //      }
      //#endif
    }

    private static JobDetail CreateJob(Plugin plugin)
    {
      JobDetail jobDetail = new JobDetail(plugin.Name, plugin.Group, plugin.PluginType);
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


      foreach (Plugin plugin in _config.Plugins)
      {
        Trigger[] triggers = _scheduler.GetTriggersOfJob(plugin.Name, plugin.Group);

        JobInfo info = new JobInfo
        {
          JobName = plugin.Name,
          Group = plugin.Group,
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

        info.IsExecuting = (executingJobs.ContainsKey(plugin.Group) && executingJobs[plugin.Group].Contains(plugin.Name));


        result.Add(info);
      }
      return result;
    }

    void IConcentratorService.RefreshConfiguration()
    {
      _config.ScanPlugins();
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
