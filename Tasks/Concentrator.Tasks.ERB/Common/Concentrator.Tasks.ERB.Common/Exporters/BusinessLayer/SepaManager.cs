using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Diagnostics;
using System.Configuration;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Tasks.ERB.Common.Exporters.BusinessLayer
{
  using Objects.Monitoring;
  using Common.Models;
  using Common.Exporters.BusinessLayer;
  using Common.Exceptions;
  using Extensions;
  using Concentrator.Objects.Models.Connectors;
  using System.Net;
  using Newtonsoft.Json;

  /// <summary>
  /// 
  /// </summary>
  public class SepaManager
  {
    //=========================================================================
    // Class variables
    //=========================================================================

    #region Class variables

    /// <summary>
    /// Delegate to method which is processing each order.
    /// </summary>
    private Func<RefundQueueElement, Func<RefundQueueElement, CustomerInfo>, Action<SepaRowElement>, RefundQueueElement> process;

    /// <summary>
    /// Temp list of orders.
    /// </summary>
    private static List<SepaRowElement> m_SepaRowQueue = new List<SepaRowElement>();

    /// <summary>
    /// SEPA Document.
    /// </summary>
    private XDocument xdoc;

    /// <summary>
    /// Sepa document description.
    /// </summary>
    private string XmlDocumentDescription = string.Format("This document is generated at: {0}", DateTime.Now.ToLongDateString());

    /// <summary>
    /// If true then turn off logging.
    /// </summary>
    public bool test;

    /// <summary>
    /// This Exception is raised when a order has failed to write to the Sepa document.
    /// </summary>
    public event EventHandler<WriteSepaOrderEventArgs> FailedToWriteOrder;

    #endregion

    //=========================================================================
    // Class constructors, Load/ Shown events
    //=========================================================================

    #region Class constructors

    /// <summary>
    /// 
    /// </summary>
    public SepaManager(TraceSource trace)
    {
      process = ProcessRefundOrder;
      TraceListenerObject = trace;

      // Retrieve App.config settings
      SepaNameSpace = ReadSetting("SepaNameSpace");
      PmtMtd = ReadSetting("PmtMtd");
      InstrPrty = ReadSetting("InstrPrty");
      Cd = ReadSetting("Cd");
      ChrgBr = ReadSetting("ChrgBr");
      Ccy = ReadSetting("Ccy");
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
    /// <param name="args"></param>
    static private void ValidationEventhandler(object sender, ValidationEventArgs args)
    {
      if (args.Severity == XmlSeverityType.Warning)
      {
        //if (!test)
        //  TraceListenerObject.TraceWarning(string.Format("\tWarning: Matching schema not found.  No validation occurred." + args.Message));
      }
      else
      {
        //if (!test)
        //  TraceListenerObject.TraceWarning(string.Format("\tValidation error: " + args.Message));
      }
    }

    #endregion

    //=========================================================================
    // Class members (properties & methods)
    //=========================================================================

    #region Class members

    /// <summary>
    /// Get from the Database connector settings.
    /// </summary>
    [ConnectorSetting(Constants.Connector.Setting.NameDebtor, true)]
    private static string NameDebtor { get; set; }

    /// <summary>
    /// Get from the Database connector settings.
    /// </summary>
    [ConnectorSetting(Constants.Connector.Setting.BodyElement, true)]
    private static string BodyElement { get; set; }

    /// <summary>
    /// Get from the Database connector settings.
    /// </summary>
    [ConnectorSetting(Constants.Connector.Setting.IBANDebtor, true)]
    private static string IBANDebtor { get; set; }

    /// <summary>
    /// Get from the Database connector settings.
    /// </summary>
    [ConnectorSetting(Constants.Connector.Setting.BIC, true)]
    private static string BIC { get; set; }

    /// <summary>
    /// Get from the Database connector settings.
    /// </summary>
    [ConnectorSetting(Constants.Connector.Setting.MagentoGetCustomerInfoOnOrderIdUrl, true)]
    private static string MagentoGetCustomerInfoOnOrderIdUrl { get; set; }

    /// <summary>
    /// Get from the App.config settings.
    /// </summary>
    private static string SepaNameSpace { get; set; }

    /// <summary>
    /// Get from the App.config settings.
    /// </summary>
    private static string PmtMtd { get; set; }

    /// <summary>
    /// Get from the App.config settings.
    /// </summary>
    private static string InstrPrty { get; set; }

    /// <summary>
    /// Get from the App.config settings.
    /// </summary>
    private static string Cd { get; set; }

    /// <summary>
    /// Get from the App.config settings.
    /// </summary>
    private static string ChrgBr { get; set; }

    /// <summary>
    /// Get from the App.config settings.
    /// </summary>
    private static string Ccy { get; set; }

    /// <summary>
    /// This property contains the Trace listener instance from The SepaExporterTask.
    /// </summary>
    private static TraceSource TraceListenerObject { get; set; }

    /// <summary>
    /// This property contains the Customer which is currently processed.
    /// 
    /// TODO this property is only public for testing!!!
    /// 
    /// </summary>
    public CustomerInfo currentCustomer { get; set; }

    /// <summary>
    /// This method is called foreach order.
    /// </summary>
    public void ProcessOrders(IEnumerable<RefundQueueElement> validatedOrders)
    {
      validatedOrders.ForEach(order => process(order, HttpRequest, AddToSepaCollection));
    }

    /// <summary>
    /// This method generates the complete Sepa document.
    /// </summary>
    public void GenerateSepa()
    {
      if (ValidatedSepaData.Count == 0)
      {
        if (!test)
          TraceListenerObject.TraceWarning(string.Format("The SepaExporterTask initiated generating a Sepa document, but {0} are found", ValidatedSepaData.Count));
        return;
      }

      xdoc = GenerateSepaDocument(XmlDocumentDescription);
      XElement body = new XElement(BodyElement);
      XElement headerGroup = GenerateHeaderGroup();
      XNamespace ns = SepaNameSpace;

      var DebitGroup = GenerateDebtorGroup();

      // Add Crediters (Customers) to Debit Group (Coolcat)
      var enumerator = ValidatedSepaData.GetEnumerator();
      while (enumerator.MoveNext())
      {
        try
        {
          XElement CreditGroup = GenerateCreditGroup(enumerator.Current);
          DebitGroup.Add(CreditGroup);
        }
        catch (Exception)
        {
          TraceListenerObject.TraceWarning(string.Format("Failed to add Separow with oderID: {0} to Sepa document.", enumerator.Current.OrderID));
          Action a = () => { FailedToWriteOrder(this, new WriteSepaOrderEventArgs(enumerator.Current)); };
          a.Invoke();
        }
      }

      ValidatedSepaData.Clear();
      headerGroup.Add(DebitGroup);
      body.Add(headerGroup);
      xdoc.Add(body);
    }

    /// <summary>
    /// This property contains the validated and merged order and customer data.
    /// Ready to be stored into a Sepafile.
    /// </summary>
    private static List<SepaRowElement> ValidatedSepaData
    {
      get { return m_SepaRowQueue; }
      set { m_SepaRowQueue = value; }
    }

    #endregion

    //=========================================================================
    // Private routines (private methods)
    //=========================================================================

    #region Private routines

    /// <summary>
    /// This method validates a specific Refund order.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    private CustomerInfo Validate(CustomerInfo customer)
    {
      customer.Validate();
      if (!test)
      {
        if (!customer.IsValid)
        {
          foreach (var errors in customer.ValidationFailures)
          {
            foreach (var member in errors.MemberNames)
            {
              TraceListenerObject.TraceInformation(string.Format("Member: {0} from customer info: {1} did not pass validation, reason: {2}", member, customer.OrderID, errors.ErrorMessage));
            }
          }
        }
      }
      return customer;
    }

    /// <summary>
    /// This method does all the validation, retrieving data and writing to sepa and marking as handle for the given <paramref name="order"/>
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    private RefundQueueElement ProcessRefundOrder(RefundQueueElement order, Func<RefundQueueElement, CustomerInfo> httpReuest,
            Action<SepaRowElement> AddToSepaCollection)
    {
      // Retrieve customer info + refund data for this order
      Task<CustomerInfo> httpRequestToMagento = Task<CustomerInfo>.Factory.StartNew(() => httpReuest(order));
      httpRequestToMagento.Wait();
      CustomerInfo customerData = httpRequestToMagento.Result;

      currentCustomer.ValidationFailures.Clear();

      // Validate customer info + refund data.   
      customerData.Validate();

      // Found the correct customer!!!
      if (order.OrderID == currentCustomer.OrderID)
      {
        // if both "processingOrder" and "customerData" objects are validated,
        // Merge "processingOrder" and "customerData" in a SepaRowElement object and write to Sepa document.
        if (order.IsValid && customerData.IsValid)
        {
          SepaRowElement sepaRow = new SepaRowElement
          {
            OrderDescription = order.OrderDescription,
            OrderID = currentCustomer.OrderID,  // This customer order ID came back from Magento.
            AccountName = customerData.AccountName,
            ConnectorID = order.ConnectorID,
            Email = customerData.Email,
            IBAN = customerData.IBAN,
            RefundAmount = customerData.RefundAmount,
            OrderResponseID = order.OrderResponseID,
            BIC = currentCustomer.BIC,
            Address = currentCustomer.Address,
            Country = currentCustomer.CountryCode
          };

          // Log valid order data
          if (!test)
            TraceListenerObject.TraceInformation("OrderID: {0} description: {1}, from connector: {2}, state: {3} is being added to the Sepa collection to be processed.", order.OrderID, order.OrderDescription, order.ConnectorID, order.IsValid);

          // Send to Sepa colllection
          AddToSepaCollection(sepaRow);
        }
        else // Log failures
        {

        }
      }
      else
      {
        // Logging the wrong customer data is retrieved from Magento.
        if (!test)
          TraceListenerObject.TraceWarning(string.Format("Request of OrderID {0} has resulted in a Customer info mismatch. Customer info OrderID {1}.", order.OrderID, currentCustomer.OrderID));
      }
      return order;
    }

    /// <summary>
    /// This method looks up all the required customer info for the given <paramref name="order"/>.
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    private CustomerInfo HttpRequest(RefundQueueElement order)
    {
      //CustomerInfo result;

      //var request = (HttpWebRequest)WebRequest.Create(MagentoGetCustomerInfoOnOrderIdUrl);
      //using (var response = request.GetResponse())
      //{
      //  using (var reader = new StreamReader(response.GetResponseStream()))
      //  {
      //    var dataResult = reader.ReadToEnd();
      //    result = JsonConvert.DeserializeObject<CustomerInfo>(dataResult);
      //  }
      //}
      //currentCustomer = result;
      //return result;
      return currentCustomer;
    }

    /// <summary>
    /// Store validated Order and custmer info into a collection.
    /// </summary>
    /// <param name="validatedData"></param>
    private void AddToSepaCollection(SepaRowElement validatedData)
    {
      ValidatedSepaData.Add(validatedData);
    }

    /// <summary>
    /// Save sepa document to file system.
    /// </summary>
    /// <param name="sepaPath"></param>
    public void SaveSepaDoc(string sepaPath)
    {
      if (xdoc == null)
      {
        TraceListenerObject.TraceWarning(string.Format("There were no orders found in the queue, Sepa document could is not being generated."));
        return;
      }

      try
      {
        xdoc.Save(sepaPath);
        TraceListenerObject.TraceInformation(string.Format("Saving {0} succeeded.", sepaPath));
      }
      catch (Exception e)
      {
        TraceListenerObject.TraceWarning(string.Format("During saving the Sepa document an exception occured, saving process is aborted, {0}, {1}", e.Message, e.InnerException));
      }
    }

    /// <summary>
    /// Validate generated sepa document according Pain.xsd.
    /// </summary>
    public void ValidateSepaDoc(string sepaPath, string xsdPath)
    {
      XmlSchemaSet schemas = new XmlSchemaSet();
      schemas.Add(sepaPath, xsdPath);

      try
      {
        XDocument validateDoc = XDocument.Load(sepaPath);
        validateDoc.Validate(schemas, ValidationEventhandler, true);
        TraceListenerObject.TraceInformation(string.Format("Validating {0} based on {1} succeeded.", sepaPath, xsdPath));
      }
      catch (Exception e)
      {
        TraceListenerObject.TraceWarning(string.Format("There was no Sepa document found, validation process is aborted, {0}, {1}", e.Message, e.InnerException));
      }
    }

    /// <summary>
    /// Generates Sepa document.
    /// </summary>
    /// <param name="comment"></param>
    /// <returns></returns>
    static private XDocument GenerateSepaDocument(string comment)
    {
      return new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XComment(comment));
    }

    /// <summary>
    /// Generates Sepa Header group.
    /// </summary>
    /// <returns></returns>
    static private XElement GenerateHeaderGroup()
    {
      var header = new XElement("CstmrCdtTrfInitn",
            new XElement("GrpHdr",
              new XElement("MsgId", "MSGID006"),
              new XElement("CreDtTm", DateTime.Now.ToLongDateString()),
              new XElement("NbOfTxs", ValidatedSepaData.Count),
              new XElement("InitgPty",
                new XElement("Nm", "Name by which a party is known and which is usually used to identify that party")
              )
            )
            );
      return header;
    }

    /// <summary>
    /// Generates Debtor group.
    /// </summary>
    /// <returns></returns>
    static private XElement GenerateDebtorGroup()
    {
      XElement el = new XElement("PmtInf",
                new XElement("PmtInfId", "PAYID001"),
                new XElement("PmtMtd", PmtMtd),
                new XElement("NbOfTxs", ValidatedSepaData.Count),
                new XElement("CtrlSum", ValidatedSepaData.Sum(i => i.RefundAmount)),
                new XElement("PmtTpInf",
                  new XElement("InstrPrty", InstrPrty),
                  new XElement("SvcLvl",
                    new XElement("Cd", Cd)
                  )
                ),
                new XElement("ReqdExctnDt", GenerateExecutionDate(DateTime.Now)),
                new XElement("Dbtr",
                  new XElement("Nm", NameDebtor)
                ),
                new XElement("DbtrAcct",
                  new XElement("Id",
                    new XElement("IBAN", IBANDebtor)
                  )
                ),
                new XElement("DbtrAgt",
                  new XElement("FinInstnId",
                    new XElement("BIC", BIC)
                  )
                ),
                new XElement("ChrgBr", ChrgBr));
      return el;
    }

    /// <summary>
    /// Generates Credit group.
    /// </summary>
    /// <returns></returns>
    static private XElement GenerateCreditGroup(SepaRowElement data)
    {
      XElement el = new XElement("CdtTrfTxInf",
                   new XElement("PmtId",
                     new XElement("EndToEndId", string.Empty)
                   ),
                   new XElement("Amt",
                     new XElement("InstdAmt", new XAttribute("Ccy", Ccy), data.RefundAmount)
                   ),
                   new XElement("CdtrAgt",
                     new XElement("FinInstnId",
                       new XElement("BIC", data.BIC)
                     )
                   ),
                   new XElement("Cdtr",
                     new XElement("Nm", data.AccountName),
                     new XElement("PstlAdr",
                       new XElement("Ctry", data.Country),
                       new XElement("AdrLine", data.Address),
                       new XElement("AdrLine", string.Empty)
                     )
                   ),
                   new XElement("CdtrAcct",
                     new XElement("Id",
                       new XElement("IBAN", data.IBAN)
                     )
                   ),
                   new XElement("RmtInf",
                     new XElement("Ustrd", data.OrderDescription)
                   ));
      return el;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="now"></param>
    /// <returns></returns>
    private static string GenerateExecutionDate(DateTime now)
    {
      DateTime executionDate = now;

      if (now.DayOfWeek <= DayOfWeek.Thursday || now.DayOfWeek == DayOfWeek.Sunday)
      {
        // Execution date is the next day from now.
        executionDate.AddDays(1).ToLongDateString();
      }

      if (now.DayOfWeek == DayOfWeek.Friday)
      {
        // Execution date is after the weekend.
        executionDate.AddDays(3).ToLongDateString();
      }

      if (now.DayOfWeek == DayOfWeek.Saturday)
      {
        // Execution date is after the weekend.
        executionDate.AddDays(2).ToLongDateString();
      }
      return executionDate.ToLongDateString();
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
