using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Slurp
{
  public enum SlurpItemState : byte
  {
    NotStarted = 0,
    Waiting,
    Starting,
    Working,
    Pausing,
    Paused,
    Ended,
    EndedWithError

  }
}
