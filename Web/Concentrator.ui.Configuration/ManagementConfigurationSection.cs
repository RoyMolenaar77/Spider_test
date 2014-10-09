using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Concentrator.ui.Configuration
{
  public class ManagementConfigurationSection : ConfigurationSection
  {
    public static ManagementConfigurationSection Default
    {
      get;
      private set;
    }

    static ManagementConfigurationSection()
    {
      Default = ConfigurationManager.GetSection("management") as ManagementConfigurationSection ?? new ManagementConfigurationSection();
    }

    private const String CommercialPropertyName = "commercial";

    public CommercialConfigurationElement Commercial
    {
      get
      {
        return (CommercialConfigurationElement)this[CommercialPropertyName];
      }
    }
  }
}
