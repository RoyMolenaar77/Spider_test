using Concentrator.Objects.ConcentratorService;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Concentrator.Plugins.Monitoring
{
  public class Monitoring : ConcentratorPlugin
  {
    public override string Name
    {
      get { return "Monitoring plugin"; }
    }

    protected override void Process()
    {
      //Do nothing
    }

    /// <summary>
    /// Send a notification message to a monitoring tool.
    ///
    /// AppSettings msHost, msApplicationID and msCustomerID must be set!
    /// </summary>
    /// <param name="processID">The process to notify about</param>
    /// <param name="counter">Optional counter data</param>
    public void Notify(string processID, int counter)
    {
      var shouldNotify = GetConfiguration().AppSettings.Settings["ShouldNotify"];
      if (shouldNotify != null)
      {
        if (!bool.Parse(shouldNotify.Value)) return;
      }
      try
      {
        StartNotify(processID, counter);
      }
      catch
      {
        try
        {
          StartNotify(processID, counter);
        }
        catch (Exception ex)
        {
          log.AuditWarning("Failed to notify Feebl", ex);
        }
      }
    }

    private void StartNotify(string processID, int counter)
    {
      var caller = new AsyncDoNotify(DoNotify);
      var asyncResult = caller.BeginInvoke(processID, counter, null, null);

      var result = caller.EndInvoke(asyncResult);

      if (!string.IsNullOrEmpty(result))
      {
        log.AuditInfo(string.Format("Feebl result: {0}", result));
      }
    }

    private delegate string AsyncDoNotify(string processID, int counter);
    private string DoNotify(string processID, int counter)
    {
      try
      {
        var config = GetConfiguration().AppSettings.Settings;
        var host = config["FeeblHost"].Value; // 82.148.218.242:9797
        var applicationID = config["FeeblApplicationID"].Value; // Concentrator
        var customerID = config["FeeblCustomerID"].Value; // e.g. Fashionwheels

        string msg;
        Feebl.Client.Service.Update(out msg, host, applicationID, customerID, processID, counter);

        return msg;
      }
      catch
      {
        try
        {
          var config = GetConfiguration().AppSettings.Settings;
          var host = config["FeeblHost"].Value; // 82.148.218.242:9797
          var applicationID = config["FeeblApplicationID"].Value; // Concentrator
          var customerID = config["FeeblCustomerID"].Value; // e.g. Fashionwheels

          string msg;
          Feebl.Client.Service.Update(out msg, host, applicationID, customerID, processID, counter);

          return msg;
        }
        catch (Exception ex)
        {
          log.AuditWarning("Failed to notify Feebl", ex);
        }
      }

      return string.Empty;
    }
  }
}
