using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Slurp
{
  public enum DeliveryStatus
  {
    NextDay,
    WithinTwoDays,
    WithinThreeDays,
    WithinTwoToFiveDays,
    WithinOneWeek,
    WithinTwoWeeks,
    LongerAsTwoWeeks,
    Unknown

  }
}
