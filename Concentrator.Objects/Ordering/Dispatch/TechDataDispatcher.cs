using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.Xml;
using System.Xml.Schema;
using System.Configuration;
using Concentrator.Objects.Xml;
using log4net;
using Concentrator.Objects.Enumerations;
using Concentrator.Objects;
using AuditLog4Net.Adapter;
using System;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Ordering.XmlFormats;

namespace Concentrator.Objects.Ordering.Dispatch
{
  public class TechDataDispatcher : IDispatchable
  {

    public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor, IAuditLogAdapter log, IUnitOfWork unit)
    {
      try
      {
        var order = new TechDataOrderExporter().GetOrder(orderLines, vendor);
        var msgID = int.Parse(order.Root.Attribute("MsgID").Value);

        string url = vendor.VendorSettings.GetValueByKey("TechDataURL", string.Empty);

        if (string.IsNullOrEmpty(url))
          throw new Exception("Unable process orders empty TechDataURL for TechDataDispatcher");

        WebRequest request = WebRequest.Create(url);
        request.Method = "POST";
        request.ContentType = "text/xml";


        using (Stream s = request.GetRequestStream())
        {
          using (XmlWriter wr = XmlWriter.Create(s))
          {
            order.Save(wr);
          }
        }
        WebResponse response = request.GetResponse();
        using (Stream s = response.GetResponseStream())
        {
          using (StreamReader reader = new StreamReader(s))
          {
            XDocument resp = XDocument.Parse(reader.ReadToEnd());
            if (resp.Root.Element("Success") == null)
            {
              throw new Exception("Dispatching to TechData failed: " + resp.Root.Element("Failure").Value);
            }
          }
        }

        LogOrder(order, vendor.VendorID, string.Format("{0}.xml", msgID), log);

        return msgID;
      }
      catch (Exception e)
      {
        throw e;
      }
    }

    public void LogOrder(object orderInformation, int vendorID, string fileName, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      try
      {
        var logPath = ConfigurationManager.AppSettings["ConcentratorOrderLog"];

        logPath = Path.Combine(logPath, DateTime.Now.ToString("yyyyMMdd"), vendorID.ToString());

        if (!Directory.Exists(logPath))
          Directory.CreateDirectory(logPath);

        ((XDocument)orderInformation).Save(Path.Combine(logPath, fileName));
      }
      catch (Exception ex)
      {
        log.AuditError("Failed to log order information for " + vendorID, ex);
      }
    }

    public void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, string logPath, IUnitOfWork unit)
    {

    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }
  }
}