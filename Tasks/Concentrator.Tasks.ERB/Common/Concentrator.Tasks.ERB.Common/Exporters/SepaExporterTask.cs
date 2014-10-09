using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;

namespace Concentrator.Tasks.ERB.Common.Exporters
{
  using Objects.Monitoring;
  using Common.Models;
  using Common.Exporters.BusinessLayer;
  using Extensions;
  using Concentrator.Objects.Models.Connectors;
  using Common.Exceptions;

  /// <summary>
  /// This class acts as a Task that acts upon:
  /// using <see cref="ConnectorTaskBase"/> 
  /// </summary>
  [Task(Constants.Vendor.Coolcat + " " + "Sepa Exporter Task")]
  public class SepaExporterTask : ConnectorTaskBase
  {
    //=========================================================================
    // Class variables
    //=========================================================================

    #region Class variables

    /// <summary>
    /// 
    /// </summary>
    private readonly FeeblMonitoring _monitoring = new FeeblMonitoring();

    /// <summary>
    /// Instantiate business Layer.
    /// </summary>
    private SepaManager manager = new SepaManager(TraceListenerObject);

    /// <summary>
    /// 
    /// </summary>
    private const string SepaPathSetting = @"SepaPath";

    /// <summary>
    /// 
    /// </summary>
    private const string XsdPathSetting = @"XsdPath";

    #endregion

    //=========================================================================
    // Class constructors, Load/ Shown events
    //=========================================================================

    #region Class constructors

    /// <summary>
    /// Constructs and initialises connections.
    /// </summary>
    public SepaExporterTask()
    {
      initialise(); TraceListenerObject = TraceSource;
      // Do not remove order from RefundQueue if writing to Sepa failed.
      manager.FailedToWriteOrder += Handler;
    }

    #endregion

    //=========================================================================
    // Class or Control events
    //=========================================================================

    #region Class events

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void Handler(object sender, WriteSepaOrderEventArgs e)
    {
      WriteSepaOrderFailures.Add(e.GetOrder());
    }

    #endregion

    //=========================================================================
    // Class members (properties & methods)
    //=========================================================================

    #region Class members

    /// <summary>
    /// This property contains all retrieved and Validated refund orders from the RefundQueue in the Concentrator database for a specific ConnectorID.
    /// </summary>
    IEnumerable<RefundQueueElement> ValidatedRefundOrders
    {
      get
      {
        IEnumerable<RefundQueueElement> orders = this.GetRefundOrders(Context.ConnectorID);
        foreach (var order in orders)
        {
          yield return order;
        }
      }
    }

    /// <summary>
    /// This property contains the Trace listener instance from The SepaExporterTask.
    /// </summary>
    private static TraceSource TraceListenerObject { get; set; }

    /// <summary>
    /// This property is mapped on a *.sql file which is an embedded resource. 
    /// </summary>
    [Resource]
    private static readonly String GetRefundQueue = null;

    /// <summary>
    /// This property is mapped on a *.sql file which is an embedded resource. 
    /// </summary>
    [Resource]
    private static readonly String RemoveFromRefundQueue = null;

    /// <summary>
    /// This property is mapped on a *.sql file which is an embedded resource. 
    /// </summary>
    [Resource]
    private static readonly String InsertIntoRefundQueueHistory = null;

    /// <summary>
    /// Contains all the order which has failed to wrtie to the Sepa.
    /// </summary>
    private static List<SepaRowElement> WriteSepaOrderFailures { get; set; }

    #endregion

    //=========================================================================
    // Private routines (private methods)
    //=========================================================================

    #region Private routines

    /// <summary>
    /// 
    /// </summary>
    protected override void ExecuteConnectorTask()
    {
      Notify(0);

      // 1. Get [RefundQueue], If [RefundQueue].count > 0 generate Sepa file
      if (!OrderAvailable())
        return;

      // 2. Do stuff for each order, like getting customer data by Http request for each order, validate customer data
      //    Merge order- and customerdata and add to the SepaCollection.
      manager.ProcessOrders(ValidatedRefundOrders);

      // 3. Generate and Write to Sepa document.
      manager.GenerateSepa();

      // 4. Send Sepa to FTP server or file location
      manager.SaveSepaDoc(ReadSetting(SepaPathSetting));

      // 5. Validate the stored Sepa Document.
      manager.ValidateSepaDoc(ReadSetting(SepaPathSetting), ReadSetting(XsdPathSetting));



      int failures = WriteSepaOrderFailures.Count();


      // Continue process for all validated orders.
      var enumerator = ValidatedRefundOrders.Where(i => i.IsValid).GetEnumerator();
      while (enumerator.MoveNext())
      {
        // 6. Add all valid orders to [RefundQueueHistory]  
        InsertOrderHistory(enumerator.Current);
        // 7. Remove all valid orders from [RefundQueue]
        RemoveRefundOrder(enumerator.Current);
      }

      Notify(1);
    }

    /// <summary>
    /// This method retrieves all refund orders from the refundqueue in the Concentrator database.
    /// And validates each order directly. If an order validation fails the order is marked by setting the Valid field to false.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private IEnumerable<RefundQueueElement> GetRefundOrders(int ConnectorID)
    {
      // Todo testen !!!
      return Database.Query<RefundQueueElement>(string.Format(GetRefundQueue, ConnectorID)).Select(Validate).ToArray();
    }

    /// <summary>
    /// This method deletes an order by a given orderID from the RefundQueue in the Concentrator Database.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private void RemoveRefundOrder(RefundQueueElement order)
    {
      try
      {
        Database.Execute(RemoveFromRefundQueue, order.OrderID, order.ConnectorID);
        TraceListenerObject.TraceInformation(string.Format("OrderID: {0} description: {1} with State: {2} is removed from the RefundQueue table", order.OrderID, order.OrderDescription, order.IsValid));
      }
      catch (Exception e)
      {
        TraceListenerObject.TraceWarning(string.Format("OrderID: {0} description: {1} with State: {2} failed to remove from the RefundQueue table", order.OrderID, order.OrderDescription, order.IsValid));
      }
    }

    /// <summary>
    /// This method insert an order into the RefundQueueHistory in the Concentrator Database.
    /// </summary>
    /// <param name=""></param>
    /// <returns></returns>
    private void InsertOrderHistory(RefundQueueElement order)
    {
      try
      {
        Database.Execute(InsertIntoRefundQueueHistory, order);
        TraceListenerObject.TraceInformation(string.Format("OrderID: {0} description: {1} with State: {2} is logged into the RefeundQueueHistory table", order.OrderID, order.OrderDescription, order.IsValid));
      }
      catch (Exception e)
      {
        TraceListenerObject.TraceWarning(string.Format("OrderID: {0} description: {1} with State: {2} failed to insert in the RefeundQueueHistory table", order.OrderID, order.OrderDescription, order.IsValid));
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private bool OrderAvailable()
    {
      if (ValidatedRefundOrders.Count() <= 0)
        return false;
      return true;
    }

    /// <summary>
    /// This method validates if the connector is ofType(Sepa).
    /// </summary>
    /// <returns></returns>
    protected override bool ValidateContext()
    {
      return ((ConnectorType)Context.ConnectorType).Has(ConnectorType.Sepa);
    }

    /// <summary>
    ///
    /// </summary>
    private void initialise()
    {

    }

    /// <summary>
    /// Send message to Feebl.
    /// </summary>
    private void Notify(int counter)
    {
      _monitoring.Notify(Name, counter);
    }

    /// <summary>
    /// This method validates a specific Refund order.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    private RefundQueueElement Validate(RefundQueueElement order)
    {
      order.Validate();

      if (!order.IsValid)
      {
        foreach (var errors in order.ValidationFailures)
        {
          foreach (var member in errors.MemberNames)
          {
            TraceListenerObject.TraceInformation(string.Format("Member: {0} from sepa Order: {1} did not pass validation, reason: {2}", member, order.OrderID, errors.ErrorMessage));
          }
        }
      }
      return order;
    }

    /// <summary>
    /// Read settings from App.config
    /// </summary>
    /// <param name="key"></param>
    static string ReadSetting(string key)
    {
      string setting = string.Empty;
      try
      {
        System.Collections.Specialized.NameValueCollection appSettings = ConfigurationManager.AppSettings;
        setting = appSettings[key] ?? "Not Found";
      }
      catch (ConfigurationErrorsException)
      {
        TraceListenerObject.TraceError(string.Format("SepaExportedTask setting: {0} value: {1}", key, setting));
      }

      return setting;
    }

    #endregion
  }
}
