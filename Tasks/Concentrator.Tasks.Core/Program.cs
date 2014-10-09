using System;

namespace Concentrator.Tasks.Core
{
  using Concentrator.Tasks.Core.Monitoring;
  using Processors;

  internal class Program
  {
    [CommandLineParameter("/AssortmentGenerator", "/AG")]
    private static Boolean AssortmentGenerator
    {
      get;
      set;
    }

    [CommandLineParameter("/SearchingGenerator", "/SG")]
    private static Boolean SearchingGenerator
    {
      get;
      set;
    }

    [CommandLineParameter("/EventNotifier", "/EN")]
    private static Boolean EventNotifier
    {
      get;
      set;
    }

    public static void Main(String[] arguments)
    {
      CommandLineHelper.Bind<Program>();

      if (AssortmentGenerator)
      {
        TaskBase.Execute<AssortmentGeneratorTask>();
      }

      if (SearchingGenerator)
      {
        TaskBase.Execute<SearchingGeneratorTask>();
      }

      if (EventNotifier)
      {
        TaskBase.Execute<Notifier>();
      }

      if (TaskBase.ShowConsole)
      {
        Console.WriteLine("Press enter to exit...");
        Console.ReadLine();
      }
    }
  }
}
