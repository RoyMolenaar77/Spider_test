using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using log4net;
using Concentrator.Objects;
using Concentrator.Objects.Enumerations;
using Concentrator.Web.Objects.EDI;
using AuditLog4Net.Adapter;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Ordering.XmlFormats;

namespace Concentrator.Objects.Ordering.Dispatch
{
  public class BasDispatcher : IDispatchable
  {
    #region IDispatchable Members
    public int DispatchOrders(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, Vendor vendor, IAuditLogAdapter log, IUnitOfWork unit)
    {
      try
      {
        var orders = new BasOrderExporter().GetOrder(orderLines, vendor);
        int msgID = 0;
        string ediUrl = vendor.VendorSettings.GetValueByKey("EDIUrl", string.Empty);

        if (string.IsNullOrEmpty(ediUrl))
          throw new Exception("Unable process orders empty EDI url for BasDispatcher");

        foreach (var order in orders)
        {
          StringBuilder requestString = new StringBuilder();
          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Encoding = Encoding.UTF8;
          using (XmlWriter xw = XmlWriter.Create(requestString, settings))
          {
            xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
            XmlSerializer serializer = new XmlSerializer(typeof(WebOrderRequest));
            XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
            nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            serializer.Serialize(xw, order, nm);

            XmlDocument document = new XmlDocument();
            document.LoadXml(requestString.ToString());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ediUrl);
            request.Method = "POST";

            HttpPostState state = new HttpPostState(orderLines, order.WebOrderDetails.Select(x => x.CustomerReference), request);

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(document.OuterXml);
            using (Stream s = request.GetRequestStream())
            {
              s.Write(byteData, 0, byteData.Length);
            }

            IAsyncResult result = request.BeginGetResponse(HttpCallBack, state);
            result.AsyncWaitHandle.WaitOne();

            if (order.WebOrderHeader != null && !string.IsNullOrEmpty(order.WebOrderHeader.CustomerOrderReference))
              LogOrder(document, vendor.VendorID, string.Format("{0}.xml", order.WebOrderHeader.WebSiteOrderNumber), log);
          }
        }
        return msgID;
      }
      catch (Exception e)
      {
        throw new Exception("Bas dispatching failed", e);
      }
    }
    #endregion
    public void LogOrder(object orderInformation, int vendorID, string fileName, AuditLog4Net.Adapter.IAuditLogAdapter log)
    {
      try
      {
        var logPath = ConfigurationManager.AppSettings["ConcentratorOrderLog"];

        logPath = Path.Combine(logPath, DateTime.Now.ToString("yyyyMMdd"), vendorID.ToString());

        if (!Directory.Exists(logPath))
          Directory.CreateDirectory(logPath);

        ((XmlDocument)orderInformation).Save(Path.Combine(logPath, fileName));
      }
      catch (Exception ex)
      {
        log.AuditError("Failed to log order information for " + vendorID, ex);
      }
    }


    public static void HttpCallBack(IAsyncResult result)
    {
      try
      {
        HttpPostState state = (HttpPostState)result.AsyncState;

        using (HttpWebResponse httpResponse = (HttpWebResponse)state.Request.EndGetResponse(result))
        {
          var order = (from o in state.OrderLines where o.Value[0].OrderLineID.ToString() == state.CustomerReference.Select(x => x.CustomerOrderLine).FirstOrDefault() select o.Key).FirstOrDefault();

          var orderLines = state.OrderLines[order];

          //(from ol in state.OrderLines
          //                where ol.Key.OrderID.ToString() == state.CustomerReference.Select(x => x.CustomerOrder).FirstOrDefault()
          //                select ol.Value.);

          switch (httpResponse.StatusCode)
          {
            case HttpStatusCode.OK:
              //POST OK
              foreach (var ol in state.CustomerReference)
              {
                var line = orderLines.Where(x => x.OrderLineID == int.Parse(ol.CustomerOrderLine)).FirstOrDefault();

                if (line != null)
                  line.Response = httpResponse.StatusDescription;
              }
              break;

            default:
              foreach (var ol in state.CustomerReference)
              {
                var line = orderLines.Where(x => x.OrderLineID == int.Parse(ol.CustomerOrderLine)).FirstOrDefault();

                if (line != null)
                  line.Response = httpResponse.StatusDescription;
              }
              throw new Exception("Error in order response");
          }
        }
      }
      catch (Exception)
      {
        throw new Exception("Callback failed");
      }
    }

    #region IDispatchable Members


    public void GetAvailableDispatchAdvices(Vendor vendor, IAuditLogAdapter log, string logPath, IUnitOfWork unit)
    {

    }

    public void CancelOrder(Vendor vendor, IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }

    #endregion
  }

  public class HttpPostState
  {
    private HttpWebRequest _request;
    private IEnumerable<CustomerReference> _customerReference;
    private Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> _orderLines;

    public HttpPostState(Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> orderLines, IEnumerable<CustomerReference> customerReference, HttpWebRequest request)
    {
      _customerReference = customerReference;
      _orderLines = orderLines;
      _request = request;
    }

    public Dictionary<Concentrator.Objects.Models.Orders.Order, List<OrderLine>> OrderLines { get { return _orderLines; } }

    public IEnumerable<CustomerReference> CustomerReference
    {
      get { return _customerReference; }
    }

    public HttpWebRequest Request
    {
      get { return _request; }
    }
  }

  public class HttpOutboundPostState
  {
    private HttpWebRequest _request;
    private int _outBoundID;
    private IUnitOfWork _unit;
    private DateTime _startPost;

    public HttpOutboundPostState(HttpWebRequest request, int outboundID, IUnitOfWork unit, DateTime startPost)
    {
      _unit = unit;
      _outBoundID = outboundID;
      _request = request;
      _startPost = startPost;
    }

    public IUnitOfWork Unit
    {
      get { return _unit; }
    }

    public int OutBoundID
    {
      get { return _outBoundID; }
    }

    public HttpWebRequest Request
    {
      get { return _request; }
    }

    public DateTime StartPost
    {
      get { return _startPost; }
    }
  }
}
