using System;
using System.Collections.Generic;

namespace Concentrator.Tasks.Management
{
  using Models;

  public static class TaskTriggerUtility
  {
    public static IEnumerable<DateTime> GetNextRunTimes(TaskTimeTrigger trigger)
    {
      return GetNextRunTime(trigger, DateTime.Now);
    }

    public static IEnumerable<DateTime> GetNextRunTimes(TaskTimeTrigger trigger, DateTime baseRunTime)
    {
      var startRunTime = trigger.Start.GetValueOrDefault(DateTime.MinValue);
      var endRunTime = trigger.End.GetValueOrDefault(DateTime.MaxValue);

      if (startRunTime > endRunTime)
      {
        throw new InvalidOperationException("Trigger start date-time must be earlier than the end date-time when both are specified");
      }

      var nextRunTime = baseRunTime;

      while (nextRunTime < endRunTime)
      {
        if (baseRunTime > startRunTime)
        {
          nextRunTime = startRunTime;
        }
        else
        {
          var repeatInterval = trigger.RepeatInterval.GetValueOrDefault(TimeSpan.MaxValue);


        }

        yield return nextRunTime;
      }
    }
  }
}
