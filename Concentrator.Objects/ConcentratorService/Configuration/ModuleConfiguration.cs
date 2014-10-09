using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Concentrator.Objects.Configuration
{
  public class ModuleConfiguration : ConfigurationSection
  {
    #region Module

    [ConfigurationProperty("Modules", IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(ModulesCollection),
        AddItemName = "Module",
        ClearItemsName = "Clear",
        RemoveItemName = "Remove")]

    public ModulesCollection Modules
    {
      get { return (ModulesCollection)this["Modules"]; }
    }

    public Module GetModule(string moduleName)
    {
      Module module = null;
      foreach (Module m in Modules)
      {
        if (m.Name == moduleName)
        {
          module = m;
          break;
        }
      }
      return module;
    }

    [ConfigurationProperty("ModulePath", IsRequired = true)]
    public string ModulePath
    {
      get
      {
        return (string)this["ModulePath"];
      }
      set
      {
        this["ModulePath"] = value;
      }
    }

    #endregion


  }
}
