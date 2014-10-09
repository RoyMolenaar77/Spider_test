using System;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using NinjectAdapter;
using PetaPoco;

namespace Concentrator.Tasks
{
  using Contracts;
  using Diagnostics;
  using Objects.DataAccess.UnitOfWork;
  using Objects.DependencyInjection.NinjectModules;
  using Objects.Environments;
  using Objects.Web;

  /// <summary>
  /// Represents the base of assembly Concentrator task.
  /// </summary>
  public abstract class TaskBase : ITask
  {
    /// <summary>
    /// Gets the name of the Concentrator task.
    /// </summary>
    public String Name
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the service locator.
    /// </summary>
    protected Microsoft.Practices.ServiceLocation.IServiceLocator ServiceLocator
    {
      get;
      private set;
    }

    /// <summary>
    /// Gets the Micro-ORM database instance.
    /// </summary>
    protected Database Database
    {
      get;
      private set;
    }

    private Lazy<IUnitOfWork> UnitProvider
    {
      get;
      set;
    }

    /// <summary>
    /// Gets the unit of work instance.
    /// </summary>
    protected IUnitOfWork Unit
    {
      get
      {
        return UnitProvider.Value;
      }
    }

    #region Diagnostics

    /// <summary>
    /// Gets or sets the trace source used for tracing all events for this task.
    /// </summary>
    protected TraceSource TraceSource
    {
      get;
      set;
    }

    [Browsable(false)]
    public void TraceCritical(Exception exception)
    {
      TraceEvent(TraceEventType.Critical, exception.ToString());
    }

    [Browsable(false)]
    public void TraceEvent(TraceEventType traceEventType, String format, params Object[] arguments)
    {
      if (TraceSource != null)
      {
        TraceSource.TraceEvent(traceEventType, 0, format, arguments);
      }
      else
      {
        foreach (var listener in Trace.Listeners.Cast<TraceListener>())
        {
          listener.TraceEvent(new TraceEventCache(), Name, traceEventType, 0, format, arguments);
        }
      }
    }

    [Browsable(false)]
    public void TraceError(String format, params Object[] arguments)
    {
      TraceEvent(TraceEventType.Error, format, arguments);
    }

    [Browsable(false)]
    public void TraceInformation(String format, params Object[] arguments)
    {
      TraceEvent(TraceEventType.Information, format, arguments);
    }

    [Browsable(false)]
    public void TraceVerbose(String format, params Object[] arguments)
    {
      TraceEvent(TraceEventType.Verbose, format, arguments);
    }

    [Browsable(false)]
    public void TraceWarning(String format, params Object[] arguments)
    {
      TraceEvent(TraceEventType.Warning, format, arguments);
    }

    #endregion

    protected TaskBase()
    {
      Name = GetType()
        .GetCustomAttributes<TaskAttribute>(true)
        .Select(attribute => attribute.Name)
        .FirstOrDefault() ?? GetType().FullName;

      ServiceLocator = Microsoft.Practices.ServiceLocation.ServiceLocator.Current;
    }

    /// <summary>
    /// Execute the task logic.
    /// </summary>
    public void Execute()
    {
      ConcentratorService.Initialize();

      if (TraceSource == null)
      {
        TraceSource = new TraceSource(Name, SourceLevels.All);

        // When a trace source is initialized, but is not configured in the application configuration only the System.Diagnostics.DefaultTraceListener will exist
        if (TraceSource.Listeners.Count == 1 && TraceSource.Listeners[0] is DefaultTraceListener)
        {
          TraceSource.Listeners.Clear();
          TraceSource.Listeners.AddRange(Trace.Listeners);
        }
      }
      
      foreach (var traceListener in TraceSource.Listeners.Cast<TraceListener>())
      {
        traceListener.Name = Name;
      }

      var dashes = new String(Enumerable.Repeat('-', Name.Length + 6).ToArray());

      TraceEvent(TraceEventType.Start, dashes);
      TraceEvent(TraceEventType.Start, "-- {0} --", Name);
      TraceEvent(TraceEventType.Start, dashes);

      UnitProvider = new Lazy<IUnitOfWork>(ServiceLocator.GetInstance<IUnitOfWork>);

      try
      {
        using (Database = new Database(Environments.Current.Connection, Database.MsSqlClientProvider))
        {
          ExecuteTask();
        }
      }
      catch (Exception exception)
      {
        TraceSource.TraceData(TraceEventType.Critical, 0, exception);
      }
      finally
      {
        if (UnitProvider.IsValueCreated)
        {
          UnitProvider.Value.Dispose();
        }

        TraceEvent(TraceEventType.Stop, dashes);
      }
    }

    [CommandLineParameter("/Console")]
    public static Boolean ShowConsole
    {
      get;
      private set;
    }
    
    static TaskBase()
    {
      try
      {
        CommandLineHelper.Bind<TaskBase>();

        if (ShowConsole)
        {
          AllocConsole();
        }

        foreach (var taskBaseType in Assembly.GetEntryAssembly().GetTypes().Where(type => typeof(TaskBase).IsAssignableFrom(type)))
        {
          CommandLineHelper.Bind(taskBaseType);
          EmbeddedResourceHelper.Bind(taskBaseType);
        }

        ConcentratorService.Initialize();
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception);
      }
    }

    [System.Runtime.InteropServices.DllImport("Kernel32", SetLastError = true)]
    private static extern Boolean AllocConsole();   
 
    /// <summary>
    /// Creates <typeparamref name="T"/> and the calls the <see cref="TaskBase.Execute"/>-method.
    /// </summary>
    /// <param name="arguments">
    /// The constructor arguments.
    /// </param>
    public static void Execute<T>(params Object[] arguments)
      where T : TaskBase
    {
      try
      {
        var taskBase = (T)Activator.CreateInstance(typeof(T), arguments);

        try
        {
          taskBase.Execute();
        }
        catch (Exception exception)
        {
          taskBase.TraceCritical(exception);
        }
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception);
      }
    }

    /// <summary>
    /// Executes the task.
    /// </summary>
    protected abstract void ExecuteTask();
  }
}
