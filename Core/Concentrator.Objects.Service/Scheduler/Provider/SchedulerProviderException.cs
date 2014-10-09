using System;
using System.Collections.Specialized;

namespace Concentrator.Objects.Service.Scheduler.Provider
{
  public class SchedulerProviderException : Exception
  {
    public SchedulerProviderException(string message, NameValueCollection properties)
      : this(message, null, properties)
    {
    }

    public SchedulerProviderException(string message, Exception innerException, NameValueCollection properties)
      : base(message, innerException)
    {
      SchedulerInitialProperties = properties;
    }

    public NameValueCollection SchedulerInitialProperties { get; private set; }
  }

}
