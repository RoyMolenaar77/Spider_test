using System;
using System.Diagnostics;
using System.Text;

namespace Concentrator.Tasks.Diagnostics
{
  /// <summary>
  /// Represents assembly Console Trace Listener that formats the events as [yyyy-MM-dd HH':'mm':'dd'.'ssssss] <i>Message</i>.
  /// </summary>
  public class ConsoleTraceListener : System.Diagnostics.ConsoleTraceListener
  {
    public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, Int32 id)
    {
      TraceEvent(eventCache, source, eventType, id, String.Empty);
    }

    public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, Int32 id, String format, params Object[] args)
    {
      TraceEvent(eventCache, source, eventType, id, String.Format(format, args));
    }

    public override void TraceEvent(TraceEventCache eventCache, String source, TraceEventType eventType, Int32 id, String message)
    {
      var builder = new StringBuilder();

      if (TraceOutputOptions.HasFlag(TraceOptions.DateTime))
      {
        builder.AppendFormat("[{0:HH':'mm':'ss.fff}] {1,-16}", DateTime.Now, eventType);
      }
      else
      {
        builder.AppendFormat("{0,-16}", eventType);
      }

      if (!message.IsNullOrWhiteSpace())
      {
        builder.Append(message);
      }

      WriteLine(builder.ToString());
    }

    public override void WriteLine(String message)
    {
      base.WriteLine(message);
    }
  }
}
