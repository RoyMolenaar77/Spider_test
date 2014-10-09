using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using Quartz;
using System.Configuration;
using AuditLog4Net.Adapter;
using AuditLog4Net.AuditLog;
using System.Reflection;
using System.IO;
using Ninject;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.DataAccess.Repository;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Plugin;
using System.Diagnostics;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Attributes;


namespace Concentrator.Objects.ConcentratorService
{

  public abstract class ConcentratorPlugin : UnitOfWorkPlugin, IStatefulJob, IInterruptableJob
  {
    protected IAuditLogAdapter log;
    protected int VendorID;
    protected Environments.Environments environment;
    protected int concentratorVendorID;
    public const string GeneralProductGroupName = "General";

    protected IKernel Kernel { get; set; }



    public ConcentratorPlugin()
    {
      log = new AuditLogAdapter(log4net.LogManager.GetLogger(Name), new AuditLog(new ConcentratorAuditLogProvider()));
      //log = log4net.LogManager.GetLogger(Name);
      environment = Environments.Environments.Current;

      if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["ConcentratorVendorID"]))
        concentratorVendorID = int.Parse(ConfigurationManager.AppSettings["ConcentratorVendorID"].ToString());
      else
        log.Warn("No ConcentratorVendorID set in configuration file");
    }

    public abstract string Name { get; }

    protected string ProcessType { get { return GetType().FullName; } }

    /// <summary>
    /// Custom plugins need to implement this method to execute their work.
    /// </summary>
    protected abstract void Process();

    #region IJob Members
    public void Execute()
    {
      using (var unit = GetUnitOfWork())
      {
        Concentrator.Objects.Web.Client.Login(Concentrator.Objects.Web.ConcentratorPrincipal.SystemPrincipal);

        var _repoConnectors = unit.Scope.Repository<Connector>();

        // filter connectors based on the optional attribute on Plugin level
        var att = GetType().GetCustomAttributes(typeof(ConnectorSystemAttribute), true);

        if (att.Length > 0)
        {
          var attributevalue = ((ConnectorSystemAttribute)att[0]).ConnectorSystem;
          _connectors = _repoConnectors.Include(x => x.ConnectorSettings).Include("ConnectorLanguages.Language").GetAll(c => c.ConnectorSystemID.HasValue && c.ConnectorSystem.Name == attributevalue).ToList();
        }
        else
        {
          try
          {
            _connectors = _repoConnectors.Include(x => x.ConnectorSettings).Include("ConnectorLanguages.Language").GetAll().ToList();
          }
          catch (Exception e)
          {
            log.Debug(e.InnerException);
          }
        }

        _vendors = unit.Scope.Repository<Vendor>().GetAll().ToList();

      }
      try
      {
        Process();
      }
      catch (Exception e)
      {
        log.AuditError("Plugin failed " + Name + " .", e);
      }
    }

    public void Execute(JobExecutionContext context)
    {

      string pluginName = context.JobDetail.FullName;
      try
      {
        using (var unit = GetUnitOfWork())
        {
          DataMap = context.JobDetail.JobDataMap;
          DateTime start = DateTime.Now;

          Plugin plugin = null;
          if (DataMap.Contains("Plugin"))
          {
            int pluginID = ((Plugin)DataMap.Get("Plugin")).PluginID;
            plugin = unit.Scope.Repository<Plugin>().GetSingle(x => x.PluginID == pluginID);

            plugin.NextRun = null;
            plugin.Duration = null;
          }

          ConnectorSchedule schedule = null;
          if (DataMap.Contains("ConnectorSchedule"))
          {
            int connectorScheduleID = ((ConnectorSchedule)DataMap.Get("ConnectorSchedule")).ConnectorScheduleID;
            schedule = unit.Scope.Repository<ConnectorSchedule>().GetSingle(x => x.ConnectorScheduleID == connectorScheduleID);
            schedule.LastRun = start;
            schedule.Duration = null;
            schedule.ScheduledNextRun = null;
            schedule.ConnectorScheduleStatus = (int)ConnectorScheduleStatus.Running;
          }

          unit.Save();

          Running = true;

          log.InfoFormat("Starting plugin: {0}", pluginName);

          Concentrator.Objects.Web.Client.Login(Concentrator.Objects.Web.ConcentratorPrincipal.SystemPrincipal);

          var _repoConnectors = unit.Scope.Repository<Connector>();

          // filter connectors based on the optional attribute on Plugin level
          var att = GetType().GetCustomAttributes(typeof(ConnectorSystemAttribute), true);

          if (att.Length > 0)
          {
            var attributevalue = ((ConnectorSystemAttribute)att[0]).ConnectorSystem;
            _connectors = _repoConnectors.GetAll(c => c.ConnectorSystemID.HasValue && c.ConnectorSystem.Name == attributevalue).ToList();

#if !DEBUG
						_connectors = _connectors.Where(c => c.IsActive).ToList();
#endif
          }
          else
          {
            try
            {
              _connectors = _repoConnectors.GetAll().ToList();
#if !DEBUG
							_connectors = _connectors.Where(c => c.IsActive).ToList();
#endif
            }
            catch (Exception e)
            {
              log.Debug(e.InnerException);
            }
          }

          if (schedule == null)
          {
            var allConnectorSchedules = unit.Scope.Repository<ConnectorSchedule>().GetAll(x => x.PluginID == plugin.PluginID).ToList();
            _connectors = _connectors.Except(allConnectorSchedules.Select(x => x.Connector));
          }
          else
          {
            _connectors = _connectors.Where(x => x.ConnectorID == schedule.ConnectorID);
          }

          _vendors = unit.Scope.Repository<Vendor>().GetAll().ToList();

          Stopwatch watch = Stopwatch.StartNew();
          log.AuditInfo(string.Format("Starting {0} at {1}", Name, DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss")), Name);
          Process();
          watch.Stop();
          log.AuditComplete(string.Format("Finished {0} at {1}. The plugin took {2} to finish", Name, DateTime.Now.ToString("yyyy-MMM-dd HH:mm:ss"), watch.Elapsed.TotalMinutes.ToString()), Name);

          //_connectorSchedules.Where(x => x.ConnectorScheduleStatus != (int)ConnectorScheduleStatus.WaitForNextRun).ToList().ForEach(x => x.ConnectorScheduleStatus = (int)ConnectorScheduleStatus.Disabled);
          TimeSpan ts = DateTime.Now.Subtract(start);
          string time = ts.ToString().Substring(0, 8);

          if (plugin != null && schedule == null)
          {
            plugin.LastRun = start;
            plugin.Duration = time;
            if (context.NextFireTimeUtc.HasValue)
              plugin.NextRun = context.NextFireTimeUtc.Value.ToLocalTime();
          }

          if (schedule != null)
          {
            if (context.NextFireTimeUtc.HasValue)
              schedule.ScheduledNextRun = context.NextFireTimeUtc.Value.ToLocalTime();

            schedule.ConnectorScheduleStatus = (int)ConnectorScheduleStatus.WaitForNextRun;
            schedule.Duration = time;
          }

          log.InfoFormat("Finished plugin: {0}, duration : {1}, next run : {2}", pluginName, time, context.NextFireTimeUtc.HasValue ? context.NextFireTimeUtc.Value.ToLocalTime() : DateTime.MinValue);
          unit.Save();
        }
      }
      catch (Exception ex)
      {
        //log.FatalFormat("Error executing {0} plugin : {1}\n{2}", pluginName, ex.InnerException != null ? ex.InnerException.Message : ex.Message, ex.StackTrace);
        log.AuditFatal(string.Format("Error executing {0} plugin : {1}\n{2}", pluginName, ex.InnerException != null ? ex.InnerException.Message : ex.Message, ex.StackTrace), ex, pluginName);
      }
    }

    public JobExecutionContext context
    {
      get;
      private set;
    }

    protected JobDataMap DataMap
    {
      get;
      private set;
    }

    private IEnumerable<Vendor> _vendors = new List<Vendor>();
    protected IEnumerable<Vendor> Vendors
    {
      get
      {
        return _vendors;
      }
      private set
      {
        _vendors = value;
      }
    }

    private IEnumerable<Connector> _connectors = new List<Connector>();
    protected IEnumerable<Connector> Connectors
    {
      get
      {
        return _connectors;
      }
      private set
      {
        _connectors = value;
      }
    }

    private List<ConnectorSchedule> _connectorSchedules = new List<ConnectorSchedule>();
    protected List<ConnectorSchedule> ConnectorSchedules
    {
      get
      {
        return _connectorSchedules;
      }
      private set
      {
        _connectorSchedules = value;
      }
    }

    #endregion

    protected bool Running { get; private set; }
    public void Interrupt()
    {
      log.DebugFormat("Interrupt signal received");
      Running = false;
    }

    private System.Configuration.Configuration _config = null;
    protected System.Configuration.Configuration GetConfiguration()
    {
      if (_config == null)
      {
        var configFile = Assembly.GetAssembly(GetType()).Location + ".config";

        if (!File.Exists(configFile))
          return null;

        var map = new ExeConfigurationFileMap()
        {
          ExeConfigFilename = configFile
        };
        _config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
      }

      return _config;
    }

  }
}
