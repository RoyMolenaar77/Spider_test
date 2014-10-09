using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.EDI.JDE.Utility
{
  public class JdeUtility
  {
    public static string FilterValue(string value,int maxLength)
    {
      if (value.Length > maxLength)
        return value.Remove(25).Trim();
      else
        return value.Trim();
    }
  }
}
