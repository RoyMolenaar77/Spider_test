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
using Concentrator.Web.Objects.EDI.DirectShipment;
using Concentrator.Web.Objects.EDI.Purchase;
using Concentrator.Web.Objects.EDI.ChangeOrder;
using Concentrator.Objects.Models.Vendors;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Ordering.XmlFormats;
using Concentrator.Objects.DataAccess.UnitOfWork;
using log4net.Core;

namespace Concentrator.Objects.Ordering.Purchase
{
  public class BasPurchase : IPurchase
  {
    public bool PurchaseOrders(Concentrator.Objects.Models.Orders.Order order, List<Concentrator.Objects.Models.Orders.OrderLine> orderLines, Vendor administrativeVendor, Vendor vendor, bool directShipment, IUnitOfWork unit, ILog logger)
    #region IPurchase Members
    {
      try
      {
        if (directShipment)
        {
          var directShipmentOrder = new BasOrderExporter().GetDirectShipmentOrder(order, orderLines, administrativeVendor, vendor);

          StringBuilder requestString = new StringBuilder();
          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Encoding = Encoding.UTF8;
          using (XmlWriter xw = XmlWriter.Create(requestString, settings))
          {
            xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
            XmlSerializer serializer = new XmlSerializer(typeof(DirectShipmentRequest));
            XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
            nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            serializer.Serialize(xw, directShipmentOrder, nm);

            XmlDocument document = new XmlDocument();
            document.LoadXml(requestString.ToString());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(administrativeVendor.VendorSettings.GetValueByKey("EDIUrl", string.Empty));
            request.Method = "POST";

            HttpPurchasePostState state = new HttpPurchasePostState(orderLines, administrativeVendor, request, "DirectShipmentPost");

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(document.OuterXml);
            using (Stream s = request.GetRequestStream())
            {
              s.Write(byteData, 0, byteData.Length);
            }

            IAsyncResult result = request.BeginGetResponse(HttpCallBack, state);
            result.AsyncWaitHandle.WaitOne();
          }
          return true;
        }
        else
        {
          var purchaseOrder = new BasOrderExporter().GetDirectShipmentOrder(order, orderLines, administrativeVendor, vendor);

          StringBuilder requestString = new StringBuilder();
          XmlWriterSettings settings = new XmlWriterSettings();
          settings.Encoding = Encoding.UTF8;
          using (XmlWriter xw = XmlWriter.Create(requestString, settings))
          {
            xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
            XmlSerializer serializer = new XmlSerializer(typeof(PurchaseRequest));
            XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
            nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            serializer.Serialize(xw, purchaseOrder, nm);

            XmlDocument document = new XmlDocument();
            document.LoadXml(requestString.ToString());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(administrativeVendor.VendorSettings.GetValueByKey("EDIUrl", string.Empty));
            request.Method = "POST";

            HttpPurchasePostState state = new HttpPurchasePostState(orderLines, administrativeVendor, request, "PurchasePost");

            byte[] byteData = UTF8Encoding.UTF8.GetBytes(document.OuterXml);
            using (Stream s = request.GetRequestStream())
            {
              s.Write(byteData, 0, byteData.Length);
            }

            IAsyncResult result = request.BeginGetResponse(HttpCallBack, state);
            result.AsyncWaitHandle.WaitOne();
          }
          return true;
        }
      }
      catch (Exception e)
      {
        throw new Exception("Bas dispatching failed", e);
      }
    }

    public void PurchaseConfirmation(PurchaseConfirmation purchaseConfirmation, Vendor administrativeVendor, List<OrderLine> orderLines)
    {
      StringBuilder requestString = new StringBuilder();
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Encoding = Encoding.UTF8;
      using (XmlWriter xw = XmlWriter.Create(requestString, settings))
      {
        xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
        XmlSerializer serializer = new XmlSerializer(typeof(PurchaseConfirmation));
        XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
        nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        serializer.Serialize(xw, purchaseConfirmation, nm);

        XmlDocument document = new XmlDocument();
        document.LoadXml(requestString.ToString());
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(administrativeVendor.VendorSettings.GetValueByKey("EDIUrl", string.Empty));
        request.Method = "POST";

        HttpPurchasePostState state = new HttpPurchasePostState(orderLines, administrativeVendor, request, "PurchaseConfirmation");

        byte[] byteData = UTF8Encoding.UTF8.GetBytes(document.OuterXml);
        using (Stream s = request.GetRequestStream())
        {
          s.Write(byteData, 0, byteData.Length);
        }

        IAsyncResult result = request.BeginGetResponse(HttpCallBack, state);
        result.AsyncWaitHandle.WaitOne();
      }
    }

    public static void HttpCallBack(IAsyncResult result)
    {
      try
      {
        HttpPurchasePostState state = (HttpPurchasePostState)result.AsyncState;

        using (HttpWebResponse httpResponse = (HttpWebResponse)state.Request.EndGetResponse(result))
        {
          switch (httpResponse.StatusCode)
          {
            case HttpStatusCode.OK:
              //POST OK
              foreach (var line in state.OrderLines)
              {
                line.Response += string.Format(",{0} {1} status OK: {2}", state.Type, state.Vendor.Name, httpResponse.StatusDescription);
              }
              break;

            default:
              foreach (var line in state.OrderLines)
              {
                line.Response += string.Format(",{0} {1} failed: {2}", state.Type, state.Vendor.Name, httpResponse.StatusDescription);
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

    public void InvoiceMessage(InvoiceMessage invoiceMessage, Vendor administrativeVendor, List<OrderLine> orderLines)
    {
      StringBuilder requestString = new StringBuilder();
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Encoding = Encoding.UTF8;
      using (XmlWriter xw = XmlWriter.Create(requestString, settings))
      {
        xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
        XmlSerializer serializer = new XmlSerializer(typeof(InvoiceMessage));
        XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
        nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        serializer.Serialize(xw, invoiceMessage, nm);

        XmlDocument document = new XmlDocument();
        document.LoadXml(requestString.ToString());
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(administrativeVendor.VendorSettings.GetValueByKey("EDIUrl", string.Empty));
        request.Method = "POST";

        HttpPurchasePostState state = new HttpPurchasePostState(orderLines, administrativeVendor, request, "Invoice");

        byte[] byteData = UTF8Encoding.UTF8.GetBytes(document.OuterXml);
        using (Stream s = request.GetRequestStream())
        {
          s.Write(byteData, 0, byteData.Length);
        }

        IAsyncResult result = request.BeginGetResponse(HttpCallBack, state);
        result.AsyncWaitHandle.WaitOne();
      }
    }

    public void OrderChange(ChangeOrderRequest changeOrderRequest, Vendor administrativeVendor, List<OrderLine> orderLines)
    {
      StringBuilder requestString = new StringBuilder();
      XmlWriterSettings settings = new XmlWriterSettings();
      settings.Encoding = Encoding.UTF8;
      using (XmlWriter xw = XmlWriter.Create(requestString, settings))
      {
        xw.WriteProcessingInstruction("xml", "version=\"1.0\" encoding=\"utf-8\"");
        XmlSerializer serializer = new XmlSerializer(typeof(ChangeOrderRequest));
        XmlSerializerNamespaces nm = new XmlSerializerNamespaces();
        nm.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
        serializer.Serialize(xw, changeOrderRequest, nm);

        XmlDocument document = new XmlDocument();
        document.LoadXml(requestString.ToString());
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(administrativeVendor.VendorSettings.GetValueByKey("EDIUrl", string.Empty));
        request.Method = "POST";

        HttpPurchasePostState state = new HttpPurchasePostState(orderLines, administrativeVendor, request, "OrderChange");

        byte[] byteData = UTF8Encoding.UTF8.GetBytes(document.OuterXml);
        using (Stream s = request.GetRequestStream())
        {
          s.Write(byteData, 0, byteData.Length);
        }

        IAsyncResult result = request.BeginGetResponse(HttpCallBack, state);
        result.AsyncWaitHandle.WaitOne();
      }
    }

    #endregion
  }

  public class HttpPurchasePostState
  {
    private HttpWebRequest _request;
    private List<OrderLine> _orderLines;
    private Vendor _vendor;
    private string _type;

    public HttpPurchasePostState(List<OrderLine> orderLines, Vendor vendor, HttpWebRequest request, string type)
    {
      _orderLines = orderLines;
      _vendor = vendor;
      _request = request;
      _type = type;
    }

    public List<OrderLine> OrderLines
    {
      get { return _orderLines; }
    }

    public Vendor Vendor
    {
      get { return _vendor; }
    }

    public HttpWebRequest Request
    {
      get { return _request; }
    }

    public string Type
    {
      get { return _type; }
    }

  }
}
