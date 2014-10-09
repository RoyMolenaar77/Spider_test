#region Usings

using System;
using System.Text.RegularExpressions;
using Concentrator.Tasks.Euretco.RSO.EDI.Importers;
using Concentrator.Tasks.Euretco.Rso.EDI.Importers;

#endregion

namespace Concentrator.Tasks.Euretco.Rso.EDI
{
  internal class Program
  {
    private static Boolean IsExplicit
    {
      get { return Regex.IsMatch(Environment.CommandLine, @"\s+/E(xplicit)?", RegexOptions.IgnoreCase); }
    }

    private static Boolean IsBrandImporterActive
    {
      get { return GetImporterCommandLineSwitch("Brand"); }
    }

    private static Boolean IsPricatImporterActive
    {
      get { return GetImporterCommandLineSwitch("Pricat"); }
    }

    private static Boolean IsProductContentImporterActive
    {
      get { return GetImporterCommandLineSwitch("ProductContent"); }
    }

    [STAThread]
    public static void Main(String[] arguments)
    {
      CommandLineHelper.Bind<Program>();

      if (IsBrandImporterActive)
      {
        TaskBase.Execute<BrandImporter>();
      }

      if (IsPricatImporterActive)
      {
        TaskBase.Execute<PricatImporter>();
      }

      if (IsProductContentImporterActive)
      {
        TaskBase.Execute<ProductContentImporter>();
      }
    }

    private static Boolean GetImporterCommandLineSwitch(String keyword)
    {
      return !IsExplicit || Regex.IsMatch(Environment.CommandLine, String.Format(@"\s+/{0}", keyword), RegexOptions.IgnoreCase);
    }
  }
}