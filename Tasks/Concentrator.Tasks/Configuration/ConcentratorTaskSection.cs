using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace Concentrator.Tasks.Configuration
{
  public class ConcentratorTaskSection : ConfigurationSection
  {
    private const String SectionName = "concentrator.task";

    public static ConcentratorTaskSection Default
    {
      get;
      private set;
    }

    static ConcentratorTaskSection()
    {
      var section = ConfigurationManager.GetSection(SectionName);

      Default = section as ConcentratorTaskSection ?? new ConcentratorTaskSection();
    }
  }
}
