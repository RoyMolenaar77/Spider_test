using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
  using System.Threading.Tasks;

namespace Concentrator.Objects.Monitoring
{
  using FeeblService = Feebl.Client.Service;

  public delegate String AsyncNotifyHandler(String processID, Int32 counter);

  public class FeeblMonitoring
  {
    public string FeeblApplicationID
    {
      get;
      set;
    }

    public string FeeblCustomerID
    {
      get;
      set;
    }

    public string FeeblHost
    {
      get;
      set;
    }

    public bool ShouldNotify
    {
      get;
      set;
    }

    public FeeblMonitoring()
    {
      FeeblHost = ConfigurationManager.AppSettings["Feebl.Host"];
      FeeblApplicationID = ConfigurationManager.AppSettings["Feebl.ApplicationID"];
      FeeblCustomerID = ConfigurationManager.AppSettings["Feebl.CustomerID"];
      ShouldNotify = (ConfigurationManager.AppSettings["Feebl.ShouldNotify"] ?? String.Empty).ParseToBool().GetValueOrDefault(true);
    }

    public IAsyncResult BeginNotify(string processID, int counter)
    {
      return new AsyncNotifyHandler(Notify).BeginInvoke(processID, counter, null, null);
    }

    public void EndNotify(IAsyncResult asyncResult)
    {
      new AsyncNotifyHandler(Notify).EndInvoke(asyncResult);
    }

    /// <summary>
    /// Send a notification message to a monitoring tool.
    ///
    /// AppSettings msHost, msApplicationID and msCustomerID must be set!
    /// </summary>
    /// <param name="processID">The process to notify about</param>
    /// <param name="counter">Optional counter data</param>
    public String Notify(string processID, int counter)
    {
      var message = String.Empty;

      if (ShouldNotify)
      {
        try
        {
          FeeblService.Update(out message, FeeblHost, FeeblApplicationID, FeeblCustomerID, processID, counter);
        }
        catch
        {
        }
      }

      return message;
    }

    public Task<String> NotifyAsync(string processID, int counter)
    {
      return Task.Factory.StartNew(delegate
      {
        return Notify(processID, counter);
      });
    }

    private void StartNotify(string processID, int counter)
    {
      new AsyncNotifyHandler(Notify).Invoke(processID, counter);
    }
  }
}
