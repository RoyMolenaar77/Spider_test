using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Concentrator.Tasks.Management.Models
{
  /// <summary>
  /// Represents a trigger base class.
  /// </summary>
  [DataContract]
  [KnownType(typeof(TaskCalendarTrigger))]
  [KnownType(typeof(TaskTimeTrigger))]
  public abstract class TaskBaseTrigger
  {
    /// <summary>
    /// Gets or sets the date and time when this trigger ends. 
    /// When this value is null, fallback on <see cref="System.DateTime.MaxValue"/>.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public DateTime? End
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the date and time when this trigger starts. 
    /// When this value is null, fallback on <see cref="System.DateTime.MinValue"/>.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public DateTime? Start
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the repeat duration of this trigger.
    /// When this value is null, fallback on <see cref="System.TimeSpan.MaxValue"/>.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public TimeSpan? RepeatDuration
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the repeat interval of this trigger.
    /// When this value is null, fallback on <see cref="System.TimeSpan.MaxValue"/>.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public TimeSpan? RepeatInterval
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets whether this trigger is enabled or not.
    /// </summary>
    [DataMember]
    public Boolean IsActive
    {
      get;
      set;
    }
    
    /// <summary>
    /// Initializes a task trigger.
    /// </summary>
    public TaskBaseTrigger()
    {
      IsActive = true;
    }
  }

  /// <summary>
  /// Represents a calendar trigger.
  /// </summary>
  [DataContract]
  [KnownType(typeof(TaskDailyTrigger))]
  [KnownType(typeof(TaskMonthlyTrigger))]
  [KnownType(typeof(TaskWeeklyTrigger))]
  public abstract class TaskCalendarTrigger : TaskBaseTrigger
  {
  }

  /// <summary>
  /// Represents a daily calendar trigger.
  /// </summary>
  [DataContract(Name = "DailyTrigger")]
  public sealed class TaskDailyTrigger : TaskCalendarTrigger
  {
    /// <summary>
    /// Gets or sets the interval.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public TimeSpan Interval
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Represents a monthly calendar trigger.
  /// </summary>
  [DataContract(Name = "MonthlyTrigger")]
  public sealed class TaskMonthlyTrigger : TaskCalendarTrigger
  {
    /// <summary>
    /// Gets or sets the number of the days of the month when this trigger can run.
    /// The first day is represented by the number 1.
    /// The number 32 represents the last day of the month, which can be the same as 28-31 depending on the month and year.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<Int32> DaysOfTheMonth
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the days of the week when this trigger runs.
    /// Januari = 1 ... December = 12
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<DayOfWeek> DaysOfTheWeek
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the numbers of months when this trigger can run.
    /// </summary>
    [DataMember]
    public IEnumerable<Int32> Months
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the weeks of the months in numbers. 
    /// The numbers 1 -> 4 represent the first to the fourth week of the month. 
    /// Number 5 represents the last week of the month, which can be the same week as number 4.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public IEnumerable<Int32> WeeksOfTheMonth
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Represents a weekly calendar trigger.
  /// </summary>
  [DataContract(Name = "WeeklTrigger")]
  public sealed class TaskWeeklyTrigger : TaskCalendarTrigger
  {
    /// <summary>
    /// Gets or sets the days of the week when this trigger runs.
    /// </summary>
    [DataMember]
    public IEnumerable<DayOfWeek> DaysOfTheWeek
    {
      get;
      set;
    }

    /// <summary>
    /// Gets or sets the interval.
    /// </summary>
    [DataMember(EmitDefaultValue = false)]
    public TimeSpan Interval
    {
      get;
      set;
    }
  }

  /// <summary>
  /// Represents a time trigger.
  /// </summary>
  [DataContract(Name = "TimeTrigger")]
  public sealed class TaskTimeTrigger : TaskBaseTrigger
  {
  }
}
