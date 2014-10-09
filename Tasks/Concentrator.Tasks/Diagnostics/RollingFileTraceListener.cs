using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Concentrator.Tasks.Diagnostics
{
  public sealed class RollingFileTraceListener : TraceListener
  {
    private const String DateTimeFormat = "yyyy'-'MM'-'dd";
    private const String DefaultDirectory = "Log";
    private const String DirectoryAttribute = "directory";

    public String DirectoryName
    {
      get
      {
        return Attributes[DirectoryAttribute];
      }
      set
      {
        Attributes[DirectoryAttribute] = value;
      }
    }

    private ConcurrentDictionary<String, FileStream> FileStreams
    {
      get;
      set;
    }

    public RollingFileTraceListener()
      : this(null)
    {
    }

    public RollingFileTraceListener(String name)
      : base(name)
    {
      FileStreams = new ConcurrentDictionary<String, FileStream>(StringComparer.InvariantCultureIgnoreCase);
    }

    protected override void Dispose(Boolean disposing)
    {
      if (disposing)
      {
        foreach (var filePath in FileStreams.Keys)
        {
          var fileStream = default(FileStream);

          if (FileStreams.TryRemove(filePath, out fileStream))
          {
            fileStream.Dispose();
          }
        }
      }

      base.Dispose(disposing);
    }

    private FileStream GetFileStream(String eventSource)
    {
      if (DirectoryName.IsNullOrWhiteSpace())
      {
        DirectoryName = DefaultDirectory;
      }

      var fileInfo = new FileInfo(Path.Combine(DirectoryName, DateTime.Now.ToString(DateTimeFormat), eventSource + ".log"));

      Directory.CreateDirectory(fileInfo.DirectoryName);

      return FileStreams.GetOrAdd(fileInfo.FullName, filePath => new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
    }

    protected override String[] GetSupportedAttributes()
    {
      return new[] { DirectoryAttribute };
    }

    public override void TraceData(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID, Object data)
    {
      TraceEvent(eventCache, eventSource, eventType, eventID, data.ToString());
    }

    public override void TraceData(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID, params Object[] data)
    {
      foreach (var dataItem in data.Where(item => item != null))
      {
        TraceData(eventCache, eventSource, eventType, eventID, dataItem);
      }
    }
    
    public override void TraceEvent(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID)
    {
      TraceEvent(eventCache, eventSource, eventType, eventID, String.Empty);
    }

    public override void TraceEvent(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID, String message)
    {
      if (Filter == null 
        || Filter.ShouldTrace(eventCache, eventSource, eventType, eventID, message, null, null, null)
        && !eventSource.IsNullOrWhiteSpace()
        && !message.IsNullOrWhiteSpace())
      {
        var fileStream = GetFileStream(eventSource);

        if (fileStream != null)
        {
          var stringBuilder = new StringBuilder();

          stringBuilder.AppendFormat("[{0:HH':'mm':'ss.fff}] {1, -16}{2}" + Environment.NewLine, DateTime.Now, eventType, message);

          var buffer = Encoding.Default.GetBytes(stringBuilder.ToString());

          fileStream.Write(buffer, 0, buffer.Length);

          if (Trace.AutoFlush)
          {
            fileStream.Flush();
          }
        }
      }
    }

    public override void TraceEvent(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID, String format, params Object[] arguments)
    {
      TraceEvent(eventCache, eventSource, eventType, eventID, String.Format(format, arguments));
    }

    public override void Write(String message)
    {
      WriteLine(message);
    }

    public override void WriteLine(String message)
    {
      TraceEvent(new TraceEventCache(), Name, TraceEventType.Verbose, 0, message);
    }
  }
}
