using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.ConnectorProductSync.Helpers
{
  public interface ILogging
  {
    void DebugFormat(int value);
    void DebugFormat(string message);
    void DebugFormat(string format, object arg0);
    void DebugFormat(string format, object arg0, object arg1);
    void DebugFormat(string format, object arg0, object arg1, object arg2);
  }
}
