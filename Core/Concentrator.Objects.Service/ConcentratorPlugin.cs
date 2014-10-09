using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Text;
using Concentrator.Objects.Configuration;
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

namespace Concentrator.Objects.Service
{

  public abstract class ConcentratorPlugin : UnitOfWorkPlugin, IStatefulJob, IInterruptableJob
  {
    //protected log4net.ILog log;
    protected IAuditLogAdapter log;
    protected Environments.Environments environment;
    protected int concentratorVendorID;

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

    /// <summary>
    /// Custom plugins need to implement this method to execute their work.
    /// </summary>
    protected abstract void Process();

    protected ConnectorSchedule RegisterConnectorSchedule(int connectorID)
    {
      var cs = ConnectorSchedules.FirstOrDefault(x => x.ConnectorID == connectorID);

      cs.ConnectorScheduleStatus = ConnectorScheduleStatus.Running;
      cs.LastRun = DateTime.Now;
      ctx.SubmitChanges();

      return cs;
    }

    protected void FinishConnectorSchedule(DateTime start, ConnectorSchedule connectorSchedule)
    {
      TimeSpan ts = DateTime.Now.Subtract(start);
      string time = ts.ToString().Substring(0, 8);

      connectorSchedule.Duration = time;
      connectorSchedule.ScheduledNextRun = context.NextFireTimeUtc;
      connectorSchedule.ConnectorScheduleStatus = ConnectorScheduleStatus.WaitForNextRun;
      ctx.SubmitChanges();
    }

    #region IJob Members
    private ConcentratorDataContext ctx;

    public void Execute(JobExecutionContext context)
    {
      try
      {
        DataMap = context.JobDetail.JobDataMap;
        Running = true;

        DateTime start = DateTime.Now;
        log.InfoFormat("Starting plugin: {0}", Name);

        Concentrator.Objects.Web.Client.Login(Concentrator.Objects.Web.ConcentratorPrincipal.SystemPrincipal);

        using (var unit = GetUnitOfWork())
        {
          var _repoConnectors = unit.Scope.Repository<Connector>();

          //TODO : Preload options

          //  var options = new DataLoadOptions();
          //  options.LoadWith<Connector>(x => x.ConnectorSystem);
          //  options.LoadWith<Connector>(x => x.Settings);
          //  options.LoadWith<Connector>(x => x.ContentProducts);
          //  options.LoadWith<Connector>(x => x.PreferredConnectorVendors);
          //  options.LoadWith<Connector>(x => x.ConnectorLanguages);
          //  options.LoadWith<ConnectorLanguage>(x => x.Language);
          //  options.LoadWith<Vendor>(v => v.VendorSettings);
          //  ctx.LoadOptions = options;

          // filter connectors based on the optional attribute on Plugin level
          var att = GetType().GetCustomAttributes(typeof(ConnectorSystemAttribute), true);

          if (att.Length > 0)
          {
            var attributevalue = ((ConnectorSystemAttribute)att[0]).ConnectorSystemID;
            _connectors = _repoConnectors.GetAll(c => c.ConnectorSystemID.HasValue && c.ConnectorSystemID.Value == attributevalue && c.IsActive).ToList();
          }
          else
          {
            _connectors = _repoConnectors.GetAll(x => x.IsActive).ToList();
          }

          _connectorSchedules = ctx.ConnectorSchedules.Where(x => x.Plugin == Name).ToList();

          foreach (var c in _connectors)
          {
            var s = _connectorSchedules.FirstOrDefault(x => x.ConnectorID == c.ConnectorID);
            if (s == null)
            {
              s = new ConnectorSchedule()
              {
                ConnectorID = c.ConnectorID,
                Plugin = Name,
                ConnectorScheduleStatus = ConnectorScheduleStatus.Disabled
              };
              ctx.ConnectorSchedules.InsertOnSubmit(s);
              _connectorSchedules.Add(s);
            }
          }

          _vendors = ctx.Vendors.ToList();


          Process();

          _connectorSchedules.Where(x => x.ConnectorScheduleStatus != ConnectorScheduleStatus.WaitForNextRun).ToList().ForEach(x => x.ConnectorScheduleStatus = ConnectorScheduleStatus.Disabled);
          ctx.SubmitChanges();
          TimeSpan ts = DateTime.Now.Subtract(start);
          string time = ts.ToString().Substring(0, 8);

          log.InfoFormat("Finished plugin: {0}, duration : {1}, next run : {2}", Name, time, context.NextFireTimeUtc);
        }
      }
      catch (Exception ex)
      {
        log.FatalFormat("Error executing {0} plugin : {1}\n{2}", this.Name, ex.Message, ex.StackTrace);
      }
      catch
      {
        log.Fatal("Unknown error plugin");
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
