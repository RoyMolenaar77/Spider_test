using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Concentrator.Tasks.ERB.Common.Exporters;
using Concentrator.Tasks.ERB.Common.Exporters.BusinessLayer;
using Concentrator.Tasks.ERB.Common.Models;
using System.Collections.Generic;
using System.Configuration;

namespace Concentrator.Tasks.ERB.Common.Tests
{
  [TestClass]
  public class UnitTest1
  {
    [TestMethod]
    public void TestMethod1()
    {

      const string SepaPathSetting = @"SepaPath";
      const string XsdPathSetting = @"XsdPath";



      var x = new SepaManager(null);
      x.test = true;
      var y = new List<Models.RefundQueueElement>();

      y.Add(new Models.RefundQueueElement { ConnectorID = 412, OrderID = 563456, OrderResponseID = "ku", OrderDescription = "ewfwefwefwefwefwefg" });
      y.Add(new Models.RefundQueueElement { ConnectorID = 53, OrderID = 43254, OrderResponseID = "sdf", OrderDescription = "rthrthrthhhhhhhhhhhhhhhhhhhhhhhhhh" });
      y.Add(new Models.RefundQueueElement { ConnectorID = 6786, OrderID = 3453, OrderResponseID = "kyuk", OrderDescription = "rthhhhhhhhhhhhhhhhh" });
      y.Add(new Models.RefundQueueElement { ConnectorID = 264, OrderID = 345, OrderResponseID = "gfb", OrderDescription = "ewfwefwefwefwefwefg" });
      y.Add(new Models.RefundQueueElement { ConnectorID = 96759, OrderID = 43534345, OrderResponseID = "fdn", OrderDescription = "thrs4436q34gqv" });

      // order validation is done during retrieving from http or sql query..
      y.ForEach(o => o.Validate());

      x.currentCustomer = new CustomerInfo
      {
        AccountName = string.Empty,
        BIC = "INGBNL2A",
        Address = "Dorpsweg 53453",
        CountryCode = "NL",
        Email = "roy.molenaar@casema.nl",
        IBAN = "NL91ABNA0417164300",
        OrderID = 563456,
        RefundAmount = 23423.24
      };

      // Customer validation is inside here..
      x.ProcessOrders(y);

      Assert.AreEqual(true, y[0].IsValid);
      Assert.AreEqual(true, y[1].IsValid);
      Assert.AreEqual(true, y[2].IsValid);
      Assert.AreEqual(true, y[3].IsValid);
      Assert.AreEqual(true, y[4].IsValid);
      Assert.AreEqual(true, x.currentCustomer.IsValid);

      x.GenerateSepa();

      //x.DocumentIsValid


      x.SaveSepaDoc(ReadSetting(SepaPathSetting));

      x.ValidateSepaDoc(ReadSetting(SepaPathSetting), ReadSetting(XsdPathSetting));

    }



    /// <summary>
    /// 
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
        //TraceListenerObject.TraceError(string.Format("SepaExportedTask setting: {0} value: {1}", key, setting));
      }
      return setting;
    }
  }
}
