using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.Win32.TaskScheduler;

namespace Concentrator.Tasks.Management
{
  using Configuration;
  using Contracts;
  using Models;
  
  /// <summary>
  /// Represents the task manager, usable for access and running task from the Windows Task Scheduler.
  /// </summary>
  public class TaskManager : IDisposable, ITaskManager
  {
    private static readonly TimeSpan DefaultDuration = TimeSpan.FromDays(1D);

    private IDictionary<String, Regex> FilterCache
    {
      get;
      set;
    }

    private TaskFolder Folder
    {
      get;
      set;
    }

    private TaskService Service
    {
      get;
      set;
    }

    /// <summary>
    /// Initialize the task manager.
    /// </summary>
    /// <param name="folder">
    /// The root folder of all the tasks registered in the Task Scheduler.
    /// </param>
    public TaskManager(String folder = null)
    {
      if (folder == null)
      {
        folder = TaskManagementSection.Default != null && TaskManagementSection.Default.TaskScheduler != null && TaskManagementSection.Default.TaskScheduler.Folder != null
          ? TaskManagementSection.Default.TaskScheduler.Folder
          : String.Empty;
      }

      FilterCache = new Dictionary<String, Regex>();
      Service = new TaskService();
      Folder = Service.GetFolder(folder.Replace('/', '\\'));
    }

    public void Dispose()
    {
      if (Folder != null)
      {
        Folder.Dispose();
        Folder = null;
      }

      if (Service != null)
      {
        Service.Dispose();
        Service = null;
      }
    }

    private static readonly char[] InvalidFilterCharacters = Enumerable
      .Range(1, 31)   // Control characters
      .Select(Convert.ToChar)
      .Append(':', '|', '\"', '<', '>')
      .ToArray();

    private Regex GetFilter(String filter, Boolean matchWholeWord = false)
    {
      if (filter == null)
      {
        filter = String.Empty;
      }

      if (filter.Any(character => InvalidFilterCharacters.Contains(character)))
      {
        throw new ArgumentException("filter contains invalid characters");
      }

      if (filter.Contains('/'))
      {
        filter = filter.Replace('/', '\\');
      }

      var result = default(Regex);

      lock (FilterCache)
      {
        filter = Regex.Escape(filter).Replace("\\*", ".*").Replace("\\?", ".");

        if (matchWholeWord)
        {
          filter = "^" + filter + "$";
        }

        if (!FilterCache.TryGetValue(filter, out result))
        {
          FilterCache[filter] = result = new Regex(filter, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
      }

      return result;
    }

    public TaskInformation GetTask(String path)
    {
      return GetTask(path, DateTime.Now.Date, DateTime.Now.Date + DefaultDuration);
    }

    public TaskInformation GetTask(String path, DateTime start, DateTime finish)
    {
      return GetTasks(GetFilter(path, true), start, finish).SingleOrDefault();
    }

    public IEnumerable<TaskInformation> GetTasks()
    {
      return GetTasks(null as Regex);
    }

    public IEnumerable<TaskInformation> GetTasks(DateTime start, DateTime finish)
    {
      return GetTasks(null as Regex, start, finish);
    }

    public IEnumerable<TaskInformation> GetTasks(String filter)
    {
      return GetTasks(GetFilter(filter));
    }

    public IEnumerable<TaskInformation> GetTasks(String filter, DateTime start, DateTime finish)
    {
      return GetTasks(GetFilter(filter), DateTime.Now.Date, DateTime.Now.Date + DefaultDuration);
    }
    
    private IEnumerable<TaskInformation> GetTasks(Regex filter)
    {
      return GetTasks(filter, DateTime.Now.Date, DateTime.Now.Date + DefaultDuration);
    }

    private IEnumerable<TaskInformation> GetTasks(Regex filter, DateTime start, DateTime finish)
    {
      if (finish < start)
      {
        throw new ArgumentException("'finish' must be equal or later than 'start'");
      }

      foreach (var task in Folder.GetTasks(filter))
      {
        yield return new TaskInformation
        {
          Actions     = CreateTaskActions(task.Definition.Actions.Cast<Microsoft.Win32.TaskScheduler.Action>()).ToArray(),
          Description = task.Definition.RegistrationInfo.Description,
          IsActive    = !task.State.HasFlag(TaskState.Disabled),
          IsRunning   = task.State.HasFlag(TaskState.Running),
          Timing      = task.GetRunTimes(start, finish),
          Location    = Path.GetDirectoryName(task.Path),
          Name        = task.Name,
          Triggers    = CreateTaskTriggers(task.Definition.Triggers.Cast<Trigger>()).ToArray()
        };
      }
    }

    public void RunTask(String path)
    {
      RunTask(path, null);
    }

    public void RunTask(String path, String arguments)
    {
      var task = Folder.GetTasks(GetFilter(path, true)).SingleOrDefault();

      if (task == null)
      {
        throw new ArgumentException(String.Format("'{0}' could not be found"));
      }

      task.Run(arguments);
    }

    private static TaskBaseAction CreateTaskAction(Microsoft.Win32.TaskScheduler.Action action)
    {
      var taskAction = default(TaskBaseAction);

      switch (action.ActionType)
      {
        case TaskActionType.Execute:
          var executeAction = action as ExecAction;

          taskAction = new TaskExecuteAction
          {
            Arguments         = executeAction.Arguments,
            Command           = executeAction.Path,
            WorkingDirectory  = executeAction.WorkingDirectory
          };
          break;
      }

      if (taskAction != null)
      {
        taskAction.ID = action.Id;
      }

      return taskAction;
    }

    private static IEnumerable<TaskBaseAction> CreateTaskActions(IEnumerable<Microsoft.Win32.TaskScheduler.Action> actions)
    {
      return actions
        .Select(CreateTaskAction)
        .Where(action => action != null)
        .ToArray();
    }

    private const int DaysPerWeek = 7;
    private const int LastDayOfTheMonth = 32;
    private const int MonthsPerYear = 12;

    private static TaskBaseTrigger CreateTaskTrigger(Trigger trigger)
    {
      var taskTrigger = default(TaskBaseTrigger);

      switch (trigger.TriggerType)
      {
        case TaskTriggerType.Daily:
          var dailyTrigger = trigger as DailyTrigger;

          taskTrigger = new TaskDailyTrigger
          {
            Interval = TimeSpan.FromDays(dailyTrigger.DaysInterval)
          };
          break;

        case TaskTriggerType.Monthly:
          var monthlyTrigger = trigger as MonthlyTrigger;

          taskTrigger = new TaskMonthlyTrigger
          {
            DaysOfTheMonth = monthlyTrigger.RunOnLastDayOfMonth
              ? monthlyTrigger.DaysOfMonth.Append(LastDayOfTheMonth).ToArray()
              : monthlyTrigger.DaysOfMonth,
            Months = GetMonthNumbers(monthlyTrigger.MonthsOfYear)
          };
          break;

        case TaskTriggerType.MonthlyDOW:
          var monthlyDOWTrigger = trigger as MonthlyDOWTrigger;

          taskTrigger = new TaskMonthlyTrigger
          {
            DaysOfTheWeek = GetDaysOfWeek(monthlyDOWTrigger.DaysOfWeek),
            Months = GetMonthNumbers(monthlyDOWTrigger.MonthsOfYear),
            WeeksOfTheMonth = GetWeekNumbers(monthlyDOWTrigger.WeeksOfMonth)
          };
          break;

        case TaskTriggerType.Weekly:
          var weeklyTrigger = trigger as WeeklyTrigger;

          taskTrigger = new TaskWeeklyTrigger
          {
            DaysOfTheWeek = GetDaysOfWeek(weeklyTrigger.DaysOfWeek),
            Interval = TimeSpan.FromDays(weeklyTrigger.WeeksInterval * DaysPerWeek)
          };
          break;

        case TaskTriggerType.Time:
          taskTrigger = new TaskTimeTrigger();
          break;

        default:
          return null;
      }

      taskTrigger.IsActive = trigger.Enabled;

      if (trigger.EndBoundary != DateTime.MaxValue)
      {
        taskTrigger.End = trigger.EndBoundary;
      }

      if (trigger.StartBoundary != DateTime.MinValue)
      {
        taskTrigger.Start = trigger.StartBoundary;
      }

      if (trigger.Repetition != null && trigger.Repetition.IsSet())
      {
        if (trigger.Repetition.Interval != TimeSpan.Zero)
        {
          taskTrigger.RepeatInterval = trigger.Repetition.Interval;
        }

        if (trigger.Repetition.Duration != TimeSpan.MaxValue)
        {
          taskTrigger.RepeatInterval = trigger.Repetition.Duration;
        }
      }

      return taskTrigger;
    }

    private static IEnumerable<TaskBaseTrigger> CreateTaskTriggers(IEnumerable<Trigger> triggers)
    {
      return triggers
        .Select(CreateTaskTrigger)
        .Where(trigger => trigger != null)
        .ToArray();
    }

    private static readonly IDictionary<DaysOfTheWeek, DayOfWeek> DaysOfTheWeekDictionary = new Dictionary<DaysOfTheWeek, DayOfWeek>
    {
      { DaysOfTheWeek.Friday,     DayOfWeek.Friday    },
      { DaysOfTheWeek.Monday,     DayOfWeek.Monday    },
      { DaysOfTheWeek.Saturday,   DayOfWeek.Saturday  },
      { DaysOfTheWeek.Sunday,     DayOfWeek.Sunday    },
      { DaysOfTheWeek.Thursday,   DayOfWeek.Thursday  },
      { DaysOfTheWeek.Tuesday,    DayOfWeek.Tuesday   },
      { DaysOfTheWeek.Wednesday,  DayOfWeek.Wednesday },
    };

    private static IEnumerable<DayOfWeek> GetDaysOfWeek(DaysOfTheWeek daysOfTheWeek)
    {
      if (daysOfTheWeek.HasFlag(DaysOfTheWeek.AllDays))
      {
        return Enum
          .GetValues(typeof(DayOfWeek))
          .Cast<DayOfWeek>()
          .ToArray();
      }
      else
      {
        // Microsoft.Win32.TaskScheduler.DaysOfTheWeek is a flagged enumeration
        // System.DayOfWeek is not a flagged enumeration
        // This next statement creates a collection of System.DayOfWeek's based on the name
        return Enum
          .GetValues(typeof(DaysOfTheWeek))
          .Cast<DaysOfTheWeek>()
          .Where(flag => daysOfTheWeek.HasFlag(flag))
          .Select(flag => DaysOfTheWeekDictionary[flag])
          .ToArray();
      }
    }

    private static IEnumerable<Int32> GetMonthNumbers(MonthsOfTheYear monthsOfTheYear)
    {
      foreach (var monthOfTheYear in Enum.GetValues(typeof(MonthsOfTheYear)).Cast<MonthsOfTheYear>().Where(flag => monthsOfTheYear.HasFlag(flag)))
      {
        switch (monthOfTheYear)
        {
          case MonthsOfTheYear.January:   yield return 1;   break;
          case MonthsOfTheYear.February:  yield return 2;   break;
          case MonthsOfTheYear.March:     yield return 3;   break;
          case MonthsOfTheYear.April:     yield return 4;   break;
          case MonthsOfTheYear.May:       yield return 5;   break;
          case MonthsOfTheYear.June:      yield return 6;   break;
          case MonthsOfTheYear.July:      yield return 7;   break;
          case MonthsOfTheYear.August:    yield return 8;   break;
          case MonthsOfTheYear.September: yield return 9;   break;
          case MonthsOfTheYear.October:   yield return 10;  break;
          case MonthsOfTheYear.November:  yield return 11;  break;
          case MonthsOfTheYear.December:  yield return 12;  break;
        }
      }
    }

    private static IEnumerable<Int32> GetWeekNumbers(WhichWeek weekOfTheMonth)
    {
      foreach (var week in Enum.GetValues(typeof(WhichWeek)).Cast<WhichWeek>().Where(flag => weekOfTheMonth.HasFlag(flag)))
      {
        switch (week)
        {
          case WhichWeek.FirstWeek:   yield return 1; break;
          case WhichWeek.SecondWeek:  yield return 2; break;
          case WhichWeek.ThirdWeek:   yield return 3; break;
          case WhichWeek.FourthWeek:  yield return 4; break;
          case WhichWeek.LastWeek:    yield return 5; break;
        }
      }
    }
  }
}
