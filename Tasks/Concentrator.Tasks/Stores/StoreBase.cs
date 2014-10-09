using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Concentrator.Tasks.Stores
{
  public abstract class StoreBase
  {
    private static readonly TraceSource DefaultTraceSource = new TraceSource("Concentrator.Tasks");

    protected TraceSource TraceSource
    {
      get;
      private set;
    }

    protected IEnumerable<PropertyInfo> GetProperties(Func<PropertyInfo, Boolean> propertyFilter = null)
    {
      var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

      if (propertyFilter != null)
      {
        properties = properties.Where(propertyFilter).ToArray();
      }

      return properties;
    }

    public abstract Boolean Load();

    protected StoreBase(TraceSource traceSource = null)
    {
      TraceSource = traceSource ?? DefaultTraceSource;
    }
  }
}
