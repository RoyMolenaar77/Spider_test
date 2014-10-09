using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;

namespace Concentrator.Objects.Configuration
{
  public class Plugin : ConfigurationElement
  {
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Plugin Config");

    public string PluginPath { get; private set; }

    #region Properties

    [ConfigurationProperty("Name", IsRequired = true)]
    [StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;'\"|\\", MinLength = 0, MaxLength = 50)]
    public string Name
    {
      get
      {
        return (string)this["Name"];
      }
      set
      {
        this["Name"] = value;
      }
    }

    [ConfigurationProperty("TypeName", IsRequired = true)]
    public string TypeName
    {
      get
      {
        return (string)this["TypeName"];
      }
      set
      {
        this["TypeName"] = value;
      }
    }

    [ConfigurationProperty("ExecuteOnStartUp", IsRequired = true)]
    public bool ExecuteOnStartUp
    {
      get
      {
        return (bool)this["ExecuteOnStartUp"];
      }
      set
      {
        this["ExecuteOnStartUp"] = value;
      }
    }

    [ConfigurationProperty("Group", IsRequired = true)]
    public string Group
    {
      get
      {
        return (string)this["Group"];
      }
      set
      {
        this["Group"] = value;
      }
    }


    [ConfigurationProperty("CronExpressionString", IsRequired = true)]
    public string CronExpressionString
    {
      get
      {
        return (string)this["CronExpressionString"];
      }
      set
      {
        this["CronExpressionString"] = value;
      }
    }



    private Type _pluginType = null;
    public Type PluginType
    {
      get
      {
        if (_pluginType == null)
          GetPluginType();
        return _pluginType;
      }
      private set
      {
        _pluginType = value;
      }
    }

    #endregion

    private void GetPluginType()
    {
      try
      {
        Type t = AppDomain.CurrentDomain.GetAssemblies().ToList()
          .SelectMany(s => s.GetTypes())
          .Where(p => p.FullName == this.TypeName).FirstOrDefault();

        PluginType = t != null ? t : null;
      }
      catch (System.BadImageFormatException)
      {
        log.FatalFormat("The Assembly is invalid--it is not a valid .NET Assembly file");
      }      
    }


    private DateTime _nextExecution = DateTime.Now;
    public DateTime NextExecution
    {
      get
      {
        return _nextExecution;
      }
      internal set
      {
        _nextExecution = value;
      }
    }

    internal void Configure()
    {
      GetPluginType();
    }
  }
}
