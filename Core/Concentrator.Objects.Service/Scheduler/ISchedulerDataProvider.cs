using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Service.Scheduler
{
  /// <summary>
  /// Translates Quartz.NET entyties to CrystalQuartz objects graph.
  /// </summary>
  public interface ISchedulerDataProvider
  {
    SchedulerData Data { get; }

    JobDetailsData GetJobDetailsData(string name, string group);
  }

}
