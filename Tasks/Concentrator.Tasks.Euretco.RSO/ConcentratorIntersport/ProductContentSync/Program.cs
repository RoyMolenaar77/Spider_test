using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Concentrator.Tasks.Euretco.Rso.ProductContentSync.Synchronizer;

namespace Concentrator.Tasks.Euretco.Rso.ProductContentSync
{
  internal class Program
  {
    /// <summary>
    /// When the switch /E or /Explicit is specified, each importer needs to be individually specified as well.
    /// </summary>
    private static Boolean IsExplicit
    {
      get
      {
        return Regex.IsMatch(Environment.CommandLine, @"\s+/E(xplicit)?", RegexOptions.IgnoreCase);
      }
    }

    private static Boolean IsProductDescriptionSynchronizerActive
    {
      get
      {
        return GetImporterCommandLineSwitch("Description");
      }
    }

    private static Boolean IsProductMediaSynchronizerActive
    {
      get
      {
        return GetImporterCommandLineSwitch("Media");
      }
    }

    private static Boolean GetImporterCommandLineSwitch(String keyword)
    {
      return !IsExplicit || Regex.IsMatch(Environment.CommandLine, String.Format(@"\s+/{0}", keyword), RegexOptions.IgnoreCase);
    }

    static void Main(string[] args)
    {
      if (IsProductDescriptionSynchronizerActive)
      {
        TaskBase.Execute<ProductDescriptionSync>();
      }

      if (IsProductMediaSynchronizerActive)
      {
        TaskBase.Execute<ProductMediaSynchronizer>();
      }

    }
  }
}
