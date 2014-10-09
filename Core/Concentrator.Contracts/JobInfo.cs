using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Service.Contracts
{
  public class JobInfo
  {
    public string JobName { get; set; }
    public DateTime? NextExecutionTime { get; set; }
    public DateTime? LastExecutionTime { get; set; }
    public string Group { get; set; }
    public bool IsExecuting { get; set; }

  }

}
