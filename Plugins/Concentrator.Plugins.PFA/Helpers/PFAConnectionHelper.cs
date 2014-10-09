using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.PFA.Helpers
{
  internal static class PFAConnectionHelper
  {
    internal static string GetCoolcatPFAConnection(System.Configuration.Configuration pluginConfig)
    {
      var dataSource = pluginConfig.AppSettings.Settings["CCDataSourcePFA"].Value;

      dataSource.ThrowIfNullOrEmpty(new InvalidOperationException("CC PFA data source is not specified"));

      return "DSN=" + dataSource + ";PWD=progress";

    }

    internal static string GetCoolcatPFAVrdsConnection(System.Configuration.Configuration pluginConfig)
    {
      var dataSource = pluginConfig.AppSettings.Settings["CCDataSourceStockPFA"].Value;

      dataSource.ThrowIfNullOrEmpty(new InvalidOperationException("CC PFA stock data source is not specified"));

      return "DSN=" + dataSource + ";PWD=progress";
    }

    
  }
}
