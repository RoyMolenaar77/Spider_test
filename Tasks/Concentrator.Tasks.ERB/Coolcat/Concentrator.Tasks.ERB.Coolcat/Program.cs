using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Concentrator.Tasks.ERB.Common.Exporters;

namespace Concentrator.Tasks.ERB.Coolcat
{
  /// <summary>
  /// 
  /// </summary>
  internal class Program
  {

    /// <summary>
    /// TODO
    /// </summary>
    [CommandLineParameter("/StartSepa", "/S")]
    public static Boolean Sepa
    {
      get;
      set;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    [STAThread]
    static void Main(string[] args)
    {
      CommandLineHelper.Bind<Program>();

      if (Sepa)
      {
        TaskBase.Execute<SepaExporterTask>();
      }

      if (TaskBase.ShowConsole)
      {
        Console.WriteLine("Press enter to exit...");
        Console.ReadLine();
      }
    }
  }
}
