using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Reflection;

namespace Concentrator.Objects.Configuration
{
  public class Module : ConfigurationElement
  {
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Plugin Config");

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

    [ConfigurationProperty("AssemblyName", IsRequired = true)]
    public string AssemblyName
    {
      get
      {
        return (string)this["AssemblyName"];
      }
      set
      {
        this["AssemblyName"] = value;
      }
    }

    private void GetModuleType()
    {
      try
      {
        Type t = AppDomain.CurrentDomain.GetAssemblies().ToList()
          .SelectMany(s => s.GetTypes())
          .Where(p => p.FullName == this.AssemblyName).FirstOrDefault();

        ModuleType = t != null ? t : null;
      }
      catch (System.BadImageFormatException)
      {
        log.FatalFormat("The Assembly is invalid--it is not a valid .NET Assembly file");
      }
    }

    private Type _moduleType = null;
    public Type ModuleType
    {
      get
      {
        if (_moduleType == null)
          GetModuleType();
        return _moduleType;
      }
      private set
      {
        _moduleType = value;
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
      GetModuleType();
    }

  }
}
