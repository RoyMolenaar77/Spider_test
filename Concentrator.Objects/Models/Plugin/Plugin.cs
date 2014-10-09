using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.EDI.Response;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using System.IO;
using Concentrator.Objects.Models.Management;
using System.Diagnostics;
using System.Reflection;

namespace Concentrator.Objects.Models.Plugin
{
  public class Plugin : BaseModel<Plugin>
  {
    public Int32 PluginID { get; set; }

    public String PluginName { get; set; }

    public String PluginType { get; set; }

    public String PluginGroup { get; set; }

    public String CronExpression { get; set; }

    public Boolean ExecuteOnStartup { get; set; }

    public virtual ICollection<Event> Events { get; set; }

    public DateTime? LastRun { get; set; }

    public DateTime? NextRun { get; set; }

    public String Duration { get; set; }

    public Boolean IsActive { get; set; }

    public Int32 JobServer { get; set; }

    public virtual ICollection<ConnectorSchedule> ConnectorSchedules { get; set; }

    private Type _type = null;
    public Type Type
    {
      get
      {
        if (_type == null)
          GetPluginType();
        return _type;
      }
      private set
      {
        _type = value;
      }
    }

    private void GetPluginType()
    {
      try
      {
        Type t = AppDomain.CurrentDomain.GetAssemblies().ToList()
          .SelectMany(s => s.GetTypes())
          .Where(p => p.FullName == this.PluginType).FirstOrDefault();

        Type = t != null ? t : null;
      }
      catch (Exception)
      {
        //log.FatalFormat("Something went wrong when loading one of the assemblies");
      }
    }

    private void GetPluginType(log4net.ILog log, string pluginPath)
    {
      try
      {
        log.Debug(this.PluginType);

        Type t = null;

        string[] files = Directory.GetFiles(pluginPath, "*.dll");
        foreach (string file in files)
        {
          try
          {
            System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFile(Path.Combine(pluginPath, file));

            foreach (var a in asm.GetTypes())
            {
              if (a.FullName == this.PluginType)
                t = a;
            }
          }
          catch (Exception ex)
          {

            if (ex is ReflectionTypeLoadException)
            {
              var e = ex as ReflectionTypeLoadException;
              foreach (var re in e.LoaderExceptions)
              {
                log.Fatal(re);
              }
            }

            log.Fatal(ex);
          }
        }
        Type = t != null ? t : null;

      }
      catch (System.BadImageFormatException e)
      {
        log.Fatal("Error format", e.InnerException ?? e);
      }
      catch (Exception e)
      {
        log.Fatal("Error", e);
      }
    }

    internal void Configure(log4net.ILog log, string pluginPath)
    {
      GetPluginType(log, pluginPath);
    }

    [DebuggerStepThrough]
    public static List<Plugin> GetPlugins(IUnitOfWork unit, int[] pluginIds = null, int? jobServerID = null)
    {
      var plugins = unit.Scope.Repository<Plugin>().GetAll();


      if (pluginIds != null)
        plugins = plugins.Where(c => pluginIds.Contains(c.PluginID));
      else
      {
        plugins = plugins.Where(x => x.IsActive);

        if (jobServerID.HasValue)
          plugins = plugins.Where(x => x.JobServer == jobServerID);
      }


      return plugins.ToList();
    }

    public static Plugin GetPlugin(string pluginName, IUnitOfWork unit)
    {
      return unit.Scope.Repository<Plugin>().GetSingle(x => x.PluginName == pluginName);
    }


    public override System.Linq.Expressions.Expression<Func<Plugin, bool>> GetFilter()
    {
      return null;
    }


  }
}