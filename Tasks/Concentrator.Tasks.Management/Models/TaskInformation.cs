using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;

namespace Concentrator.Tasks.Management.Models
{
  /// <summary>
  /// Represents the task registration information of Windows Task Scheduler v2.0.
  /// </summary>
  [DataContract(Name = "Task")]
  public class TaskInformation
  {
    [DataMember]
    public String Description
    {
      get;
      set;
    }

    [DataMember]
    public Boolean IsActive
    {
      get;
      set;
    }

    [DataMember]
    public Boolean IsRunning
    {
      get;
      set;
    }

    [DataMember]
    public String Location
    {
      get;
      set;
    }

    [DataMember]
    public String Name
    {
      get;
      set;
    }

    [DataMember]
    public TaskBaseAction[] Actions
    {
      get;
      set;
    }

    [DataMember]
    public DateTime[] Timing
    {
      get;
      set;
    }

    [DataMember]
    public TaskBaseTrigger[] Triggers
    {
      get;
      set;
    }

    public override String ToString()
    {
      return Path.Combine(Location, Name);
    }
  }
}
