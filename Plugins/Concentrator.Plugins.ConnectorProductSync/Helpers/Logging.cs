using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Helpers
{
  public class Logging : ILogging
  {
    public void DebugFormat(int value)
    {
      DebugFormat(value.ToString());
    }

    public void DebugFormat(string message)
    {
      Console.WriteLine("{0} - {1}", DateTime.Now, message);
    }

    public void DebugFormat(string format, object arg0)
    {
      string dateTime = string.Format("{0} - ", DateTime.Now);
      Console.WriteLine(dateTime + format, arg0);
    }

    public void DebugFormat(string format, object arg0, object arg1)
    {
      string dateTime = string.Format("{0} - ", DateTime.Now);
      Console.WriteLine(dateTime + format, arg0, arg1);
    }

    public void DebugFormat(string format, object arg0, object arg1, object arg2)
    {
      string dateTime = string.Format("{0} - ", DateTime.Now);
      Console.WriteLine(dateTime + format, arg0, arg1, arg2);
    }
  }
}
