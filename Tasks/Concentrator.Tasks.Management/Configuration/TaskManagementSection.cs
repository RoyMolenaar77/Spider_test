using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Concentrator.Tasks.Management.Configuration
{
  public class TaskManagementSection : ConfigurationSection
  {
    private const String SectionName = "concentrator.tasks.management";

    /// <summary>
    /// Gets the application default configuration section.
    /// </summary>
    public static TaskManagementSection Default
    {
      get;
      private set;
    }

    static TaskManagementSection()
    {
      Default = (TaskManagementSection)ConfigurationManager.GetSection(SectionName);
    }

    #region Task Scheduler Property

    private const String TaskSchedulerProperty = "taskScheduler";

    [ConfigurationProperty(TaskSchedulerProperty)]
    public TaskSchedulerElement TaskScheduler
    {
      get
      {
        return (TaskSchedulerElement)base[TaskSchedulerProperty];
      }
    }

    #endregion
  }
}
