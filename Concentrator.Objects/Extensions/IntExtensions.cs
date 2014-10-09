using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
  public static class IntExtensions
  {
    /// <summary>
    /// Checks whether a number is in a range.
    /// Inclusive
    /// </summary>
    /// <param name="instance"></param>
    /// <param name="low">Low range</param>
    /// <param name="high">High range</param>
    /// <returns></returns>
    public static bool InRange(this int instance, int low, int high)
    {
      return (instance >= low && instance <= high);
    }
  }
}
