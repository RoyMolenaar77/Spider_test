using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Connectors
{
  [Flags]
  public enum ConnectorScheduleStatus
  {
    WaitForNextRun = 1,
    Disabled = 2,
    Running = 3
  }
}
