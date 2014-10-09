using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Concentrator.Objects.Web;
using PetaPoco;

namespace Concentrator.Tasks.Diagnostics
{
  using Objects.DataAccess.Repository;
  using Objects.DataAccess.UnitOfWork;
  using Objects.Environments;
  using Objects.Models.Management;

  public sealed class ConcentratorTraceListener : TraceListener
  {
    private static IDictionary<EventTypes, Int32> ConcentratorEventTypes
    {
      get;
      set;
    }

    static ConcentratorTraceListener()
    {
      ConcentratorEventTypes = new Dictionary<EventTypes, Int32>();

      using (var database = new Database(Environments.Current.Connection, Database.MsSqlClientProvider))
      {
        foreach (var eventType in database.Query<EventType>("SELECT [TypeID], [Type] FROM [dbo].[EventType]"))
        {
          var concentratorEventType = default(EventTypes);

          if (Enum.TryParse(eventType.Type, out concentratorEventType))
          {
            ConcentratorEventTypes[concentratorEventType] = eventType.TypeID;
          }
        }
      }
    }

    private Database Database
    {
      get;
      set;
    }

    public ConcentratorTraceListener()
    {
      Database = new Database(Environments.Current.Connection, Database.MsSqlClientProvider);
    }

    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        Database.Dispose();
        Database = null;
      }

      base.Dispose(disposing);
    }

    private void CreateEvent(TraceEventType eventType, String eventSource, String message, Exception exception = null)
    {
      Database.Execute("INSERT [dbo].[Event] ([TypeID], [ProcessName], [Message], [ExceptionMessage], [StackTrace], [CreatedBy], [CreationTime]) VALUES (@0, @1, @2, @3, @4, @5, @6)"
        , TranslateEventTypeID(eventType)
        , eventSource
        , message
        , exception != null ? exception.Message : String.Empty
        , exception != null ? exception.StackTrace : String.Empty
        , Client.User.UserID
        , DateTime.Now);
    }

    private Int32 TranslateEventTypeID(TraceEventType eventType)
    {
      var concentratorEventType = default(EventTypes);

      switch (eventType)
      {
        case TraceEventType.Critical:
          concentratorEventType = EventTypes.Critical;
          break;

        case TraceEventType.Error:
          concentratorEventType = EventTypes.Error;
          break;

        case TraceEventType.Warning:
          concentratorEventType = EventTypes.Warn;
          break;

        default:
          concentratorEventType = EventTypes.Info;
          break;
      }

      return ConcentratorEventTypes.ContainsKey(concentratorEventType)
        ? ConcentratorEventTypes[concentratorEventType]
        : default(Int32);
    }

    public override void TraceData(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID, Object data)
    {
      if (data is Exception)
      {
        var exception = data as Exception;

        if (Filter == null || Filter.ShouldTrace(eventCache, eventSource, eventType, eventID, String.Empty, null, null, null))
        {
          CreateEvent(eventType, eventSource, exception.Message, exception);
        }
      }
      else
      {
        TraceEvent(eventCache, eventSource, eventType, eventID, data.ToString());
      }
    }

    public override void TraceData(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID, params Object[] data)
    {
      foreach (var dataEntry in data)
      {
        TraceData(eventCache, eventSource, eventType, eventID, dataEntry);
      }
    }

    public override void TraceEvent(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID)
    {
      TraceEvent(eventCache, eventSource, eventType, eventID, String.Empty);
    }

    public override void TraceEvent(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID, String format, params Object[] arguments)
    {
      TraceEvent(eventCache, eventSource, eventType, eventID, String.Format(format, arguments));
    }

    public override void TraceEvent(TraceEventCache eventCache, String eventSource, TraceEventType eventType, Int32 eventID, String message)
    {
      if (Filter == null || Filter.ShouldTrace(eventCache, eventSource, eventType, eventID, message, null, null, null))
      {
        CreateEvent(eventType, eventSource, message);
      }
    }

    public override void Write(String message)
    {
      TraceEvent(new TraceEventCache(), String.Empty, TraceEventType.Verbose, 0, message);
    }

    public override void WriteLine(String message)
    {
      Write(message + Environment.NewLine);
    }
  }
}
