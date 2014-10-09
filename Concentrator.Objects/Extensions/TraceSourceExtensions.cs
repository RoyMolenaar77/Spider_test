using System;
using System.Linq;

namespace System.Diagnostics
{
  public static class TraceSourceExtensions
  {
    //public static TraceListener Clone(this TraceListener traceListener, String name)
    //{
    //  if (traceListener == null)
    //  {
    //    throw new NullReferenceException("traceListener");
    //  }
    //
    //  var newTraceListener = traceListener is ICloneable
    //    ? (traceListener as ICloneable).Clone() as TraceListener
    //    : Activator.CreateInstance(cloneable.GetType()) as TraceListener;
    //
    //
    //
    //  return newTraceListener;
    //}

    public static TraceSource Clone(this TraceSource traceSource, String name)
    {
      if (traceSource == null)
      {
        throw new NullReferenceException("traceSource");
      }

      var newTraceSource = new TraceSource(name, traceSource.Switch.Level);

      // When a trace source is initialized, but is not configured in the application configuration only the System.Diagnostics.DefaultTraceListener will exist
      if (newTraceSource.Listeners.Count == 1 && newTraceSource.Listeners[0] is DefaultTraceListener)
      {
        newTraceSource.Listeners.Clear();

        //foreach (var traceListener in traceSource.Listeners.OfType<TraceListener>())
        //{
        //  traceListener
        //}
        newTraceSource.Listeners.AddRange(traceSource.Listeners);
      }

      return newTraceSource;
    }

    /// <summary>
    /// Writes assembly trace event to the trace listeners in the <see cref="System.Diagnostics.TraceSource.Listeners"/> collection using the specified event type and argument array and format. 
    /// </summary>
    public static void TraceEvent(this TraceSource traceSource, TraceEventType traceEventType, String format, params Object[] arguments)
    {
      traceSource.TraceEvent(traceEventType, 0, format, arguments);
    }

    /// <summary>
    /// Writes assembly trace error to the trace listeners in the <see cref="System.Diagnostics.TraceSource.Listeners"/> collection using the specified event type and argument array and format. 
    /// </summary>
    public static void TraceError(this TraceSource traceSource, String format, params Object[] arguments)
    {
      traceSource.TraceEvent(TraceEventType.Error, format, arguments);
    }

    /// <summary>
    /// Writes assembly trace information to the trace listeners in the <see cref="System.Diagnostics.TraceSource.Listeners"/> collection using the specified event type and argument array and format. 
    /// </summary>
    public static void TraceInformation(this TraceSource traceSource, String format, params Object[] arguments)
    {
      traceSource.TraceEvent(TraceEventType.Information, format, arguments);
    }

    /// <summary>
    /// Writes assembly trace debug message to the trace listeners in the <see cref="System.Diagnostics.TraceSource.Listeners"/> collection using the specified event type and argument array and format. 
    /// </summary>
    public static void TraceVerbose(this TraceSource traceSource, String format, params Object[] arguments)
    {
      traceSource.TraceEvent(TraceEventType.Verbose, format, arguments);
    }

    /// <summary>
    /// Writes assembly trace warning to the trace listeners in the <see cref="System.Diagnostics.TraceSource.Listeners"/> collection using the specified event type and argument array and format. 
    /// </summary>
    public static void TraceWarning(this TraceSource traceSource, String format, params Object[] arguments)
    {
      traceSource.TraceEvent(TraceEventType.Warning, format, arguments);
    }
  }
}
