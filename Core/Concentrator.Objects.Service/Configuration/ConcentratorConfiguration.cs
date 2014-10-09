using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Concentrator.Objects.Configuration
{
  internal class ConcentratorConfiguration : ConfigurationSection
  {
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger("Plugin Config");

    #region Plugin
    [ConfigurationProperty("Plugins", IsDefaultCollection = true)]
    [ConfigurationCollection(typeof(PluginsCollection),
        AddItemName = "Plugin",
        ClearItemsName = "Clear",
        RemoveItemName = "Remove")]
    public PluginsCollection Plugins
    {
      get { return (PluginsCollection)this["Plugins"]; }
    }

    public Plugin GetPlugin(string pluginName)
    {
      Plugin plugin = null;
      foreach (Plugin pl in Plugins)
      {
        if (pl.Name == pluginName)
        {
          plugin = pl;
          break;
        }
      }
      return plugin;
    }

    [ConfigurationProperty("PluginPath", IsRequired = true)]
    public string PluginPath
    {
      get
      {
        return (string)this["PluginPath"];
      }
      set
      {
        this["PluginPath"] = value;
      }
    }

    public void ScanPlugins()
    {
      string effectivePath = PluginPath;
      if (!PluginPath.Contains(":"))
      {
        effectivePath = Path.Combine(Path.GetDirectoryName(
                                       System.Reflection.Assembly.GetExecutingAssembly().CodeBase), effectivePath);
        if (effectivePath.StartsWith("file:\\"))
          effectivePath = effectivePath.Substring(6);
      }

      string[] files = Directory.GetFiles(effectivePath, "*.dll");
      foreach (string file in files)
      {
        try
        {
          System.Reflection.Assembly asm = System.Reflection.Assembly.LoadFile(Path.Combine(PluginPath, file));
        }
        catch (Exception ex)
        {
          log.Fatal(ex);
        }
      }
    }
    #endregion


  }
}
