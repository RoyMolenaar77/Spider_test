using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Concentrator.Tasks.Management.Models
{
  [DataContract]
  public abstract class TaskBaseAction
  {
    [DataMember]
    public String ID
    {
      get;
      set;
    }
  }

  [DataContract]
  public class TaskExecuteAction : TaskBaseAction
  {
    [DataMember]
    public String Arguments
    {
      get;
      set;
    }

    [DataMember]
    public String Command
    {
      get;
      set;
    }

    [DataMember]
    public String WorkingDirectory
    {
      get;
      set;
    }
  }
}
