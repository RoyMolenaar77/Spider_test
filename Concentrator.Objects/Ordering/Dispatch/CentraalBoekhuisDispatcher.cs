using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Models.Vendors;

// Author: Dylan Ariese

namespace Concentrator.Objects.Ordering.Dispatch
{
  public class CentraalBoekhuisDispatcher : IDispatchable
  {
    // Response and credentials
    private String _response;
    private String _endPointNm = ConfigurationManager.AppSettings.Get("EndpointNm");
    private String _certificaat = ConfigurationManager.AppSettings.Get("CbCertificate");

    /// <summary>
    ///   Dispatches passed in order lines
    /// </summary>
    public int DispatchOrders(Dictionary<Models.Orders.Order, List<Models.Orders.OrderLine>> orderLines, Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, DataAccess.UnitOfWork.IUnitOfWork uni)
    {
      foreach (var order in orderLines)
      {
        //Retrieves the values
        String EAN = uni.Scope.Repository<VendorAssortment>().GetSingle(c => c.VendorID == vendor.VendorID).CustomItemNumber;
        String orderReference = String.Concat("C", order.Key.OrderID.ToString());
        String clientID = ConfigurationManager.AppSettings.Get("CbClientID"); //We will get this from centraal boekhuis

        //Checks if the book is available //DON'T REMOVE UNLESS YOURE SURE! this checks if the book is available
        //SendSoapRequest(EAN, orderReference, clientID, false, true); 

        // Order Product request
        SendSoapRequest(EAN, orderReference, clientID, true, false);

        //TEST
        //string orderReference = DateTime.Now.TimeOfDay.ToString();

        //Test
        //GetAvailableDispatchAdvices(vendor, log, String.Empty, uni);
      }

      // Whatever floats your boat!      
      return orderLines.Count();
    }

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

    /// <summary>
    ///   Processes an order response
    /// </summary>
    public void GetAvailableDispatchAdvices(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath, DataAccess.UnitOfWork.IUnitOfWork unit)
    {
      //Creates an instance of xdocument
      XDocument doc = new XDocument();

      // creates an instance of Xmlreader
      using (XmlReader reader = XmlReader.Create(new StringReader(_response)))
      {
        while (reader.Read())
        {
          // Loads the soap response into the locally created Xdocument
          doc = XDocument.Load(reader);
        }
        // Calls method ProcessXml
        ProcessXml(doc, vendor, unit);
      }
    }

    private bool ProcessXml(XDocument doc, Models.Vendors.Vendor vendor, DataAccess.UnitOfWork.IUnitOfWork unit)
    {
      // Name space needed
      XNamespace xName = "http://www.cbonline.nl/xsd";
      // The soap hierarchy
      IEnumerable<XElement> hierarchy = doc.Root.Descendants().Elements("return").Elements(xName + "CbOrderProduct").Descendants();
      //use this to check if its a bundle
      IEnumerable<XElement> eBookBundle = hierarchy.Elements(xName + "Item");

      // Checks for errors
      var errorList = (from e in hierarchy.Elements("Error")
                       select new
                       {
                         ErrorCode = e.Element("ErrorCode").Value,
                         Msg = e.Element("ErrorMsg").Value
                       }).FirstOrDefault();

      // If theres an error, return!
      if (errorList != null)
        return false;

      // Retrieves the order references
      var orderList = (from i in hierarchy
                       select new
                       {
                         CbOrderReference = i.Element(xName + "CbOrderReference").Value,
                         OrderReference = i.Element(xName + "OrderReference").Value,
                       }).FirstOrDefault();
      
      // Creates a new object of type OrderResponse
      var orderResponse = new OrderResponse()
      {
        OrderID = Int32.Parse(orderList.OrderReference.Remove(0, 1)),
        ResponseType = Enum.GetName(typeof(OrderResponseTypes), 100),
        VendorDocument = doc.ToString(),
        VendorID = vendor.VendorID,
        VendorDocumentNumber = orderList.CbOrderReference,
        ReceiveDate = DateTime.Today
      };

      // Add the new object to the OrderResponse repository
      unit.Scope.Repository<OrderResponse>().Add(orderResponse);

      // Retrieves the orderline from the order
      var _orderLineRepo = unit.Scope.Repository<OrderLine>().GetSingle(x => x.OrderID == orderResponse.OrderID);
      // Instanciates a new OrderResponseLine
      var orderResponseLine = new OrderResponseLine();

      // if its a bundle book
      if (eBookBundle.Count() > 0)
      {
        // The response contains multiple items, so for each item..
        eBookBundle.ForEach((item, idx) =>
        {
          // creates a new OrderResponseLine
          orderResponseLine = new OrderResponseLine()
          {
            OrderResponseID = orderResponse.OrderResponseID,
            OrderLineID = _orderLineRepo.OrderLineID,
            Ordered = 1,
            Backordered = 0,
            Cancelled = 0,
            Shipped = 0,
            Invoiced = 0,
            Price = Decimal.Parse(_orderLineRepo.Price.ToString()),
            Processed = false,
            Delivered = 0,
            VendorItemNumber = item.Element(xName + "EAN").Value
          };
          // Adds it to the repository
          unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);

          // creates a new orderitemfullfillment object
          var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation()
          {
            OrderResponseLine = orderResponseLine,
            Value = item.Element(xName + "DownloadURL").Value
          };
          // adds it to the repository
          unit.Scope.Repository<OrderItemFullfilmentInformation>().Add(orderItemFullfilmentInformation);
        });
      }

      // A single book
      else
      {
        // retries the download links
        var downloadlinks = (from i in hierarchy
                             select new
                             {
                               DownloadPage = i.Element(xName + "DownloadPage").Value,
                               DownloadURL = i.Element(xName + "DownloadURL").Value
                             }).FirstOrDefault();

        //Creates a new OrderResponseLine object
        orderResponseLine = new OrderResponseLine()
        {
          OrderResponseID = orderResponse.OrderResponseID,
          OrderLineID = _orderLineRepo.OrderLineID,
          Ordered = 1,
          Backordered = 0,
          Cancelled = 0,
          Shipped = 0,
          Invoiced = 0,
          Price = Decimal.Parse(_orderLineRepo.Price.ToString()),
          Processed = false,
          Delivered = 0
        };
        // Adds it to the repository
        unit.Scope.Repository<OrderResponseLine>().Add(orderResponseLine);

        // Creates a new OrderItemFullfilmentInformation object
        var orderItemFullfilmentInformation = new OrderItemFullfilmentInformation()
        {
          OrderResponseLine = orderResponseLine,
          Value = downloadlinks.DownloadURL
        };
        // Adds it to the repository
        unit.Scope.Repository<OrderItemFullfilmentInformation>().Add(orderItemFullfilmentInformation);
      }

      // Saves the objects
      unit.Save();

      // returns true
      return true;
    }

    /// <summary>
    /// Cancel an order
    /// </summary>
    public void CancelOrder(Models.Vendors.Vendor vendor, AuditLog4Net.Adapter.IAuditLogAdapter log, string logPath)
    {
      throw new NotImplementedException();
    }

    private static HttpWebRequest HttpSoapRequest(Uri uri)
    {
      // Creates a new HttpWebRequest obviously and uses the uri paremeter to do so
      HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

      // Set the properties
      request.ContentType = "text/xml;charset=\"utf-8\"";
      request.Accept = "text/xml";
      request.Method = "POST";

      // returns the webrequest with its properties set
      return request;
    }

    private XmlDocument CheckAvailabilitySoapEnvelope(string EAN)
    {
      // Creates an Xml Document form availability
      XmlDocument soapEnvelope = new XmlDocument();

      // Fills the Xml Document with SOAP lol
      soapEnvelope.LoadXml(@"<soapenv:Envelope 
                                  xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                                  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                                  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""> 
                                <soapenv:Body> 
                                  <exec xmlns=""CBWSCallEngine"" 
                                      soapenv:encodingStyle=""http://xml.apache.org/xml-soap/literalxml"">
                                    <arguments> 
                                      <CbCheckAvailability xmlns=""http://www.cbonline.nl/xsd"">
                                        <Header> 
                                          <EndpointNm> " + _endPointNm + @"</EndpointNm> 
                                          <Certificaat> " + _certificaat + @"</Certificaat>
                                        </Header> 
                                        <Detail> 
                                          <EAN>" + EAN + @"</EAN> 
                                        </Detail> 
                                      </CbCheckAvailability>
                                    </arguments> 
                                  </exec> 
                                </soapenv:Body> 
                             </soapenv:Envelope>");

      // return the envelope
      return soapEnvelope;
    }

    private XmlDocument OrderProductSoapEnvelope(string EAN, string orderReference, string clientID)//(string EAN, string orderReference, int clientID)
    {
      // Creates an Xml Document for the order product request
      XmlDocument soapEnvelope = new XmlDocument();
      // Loads the XML
      soapEnvelope.LoadXml(@"<soapenv:Envelope 
                                  xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" 
                                  xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" 
                                  xmlns:xsd=""http://www.w3.org/2001/XMLSchema""> 
                                <soapenv:Body> 
                                  <exec xmlns=""CBWSCallEngine"" 
                                      soapenv:encodingStyle=""http://xml.apache.org/xml-soap/literalxml"">
                                    <arguments> 
                                      <CbOrderProduct xmlns=""http://www.cbonline.nl/xsd""> 
                                        <Header> 
                                          <EndpointNm> " + _endPointNm + @"</EndpointNm> 
                                          <Certificaat> " + _certificaat + @"</Certificaat>                               
                                        </Header> 
                                        <Detail> 
                                          <EAN>" + EAN + @"</EAN> 
                                          <OrderReference>" + orderReference + @"</OrderReference>
                                          <ClientId>" + clientID + @"</ClientId> 
                                        </Detail> 
                                      </CbOrderProduct>
                                    </arguments> 
                                  </exec> 
                                </soapenv:Body> 
                             </soapenv:Envelope>");

      // Returns the Order Product SOAP envelope
      return soapEnvelope;
    }

    private static void CombineSoapAndWebRequest(XmlDocument soapEnvelope, HttpWebRequest request)
    {
      // Requests the stream of data
      using (Stream stream = request.GetRequestStream())
      {
        // Stores the data in the SOAPEnvelope
        soapEnvelope.Save(stream);
      }
    }

    public void SendSoapRequest(string ean, string orderReference, string clientID, bool orderProduct, bool checkAvailability)
    {
      // Creates a new XMLDocument
      XmlDocument soapEnvelopeXml = new XmlDocument();

      // If the bool checkAvailability is set to true
      if (checkAvailability)
        // Soap envelope will be filled with the check availability xml
        soapEnvelopeXml = CheckAvailabilitySoapEnvelope(ean);

      // If the bool orderProduct equals true
      if (orderProduct)
        // Soap envelope will be filled with the order product xml
        soapEnvelopeXml = OrderProductSoapEnvelope(ean, orderReference, clientID);

      // Creates a new webrequest with the uri retrieved from appsetting.config and stores 
      // the request in the variable webRequest
      HttpWebRequest webRequest = HttpSoapRequest(new Uri(ConfigurationManager.AppSettings.Get("CbUri")));

      // Adds the stream to the soapEnvelopeXml
      CombineSoapAndWebRequest(soapEnvelopeXml, webRequest);

      // GetResponse will get the response send back from the webservice
      using (HttpWebResponse response = (HttpWebResponse)webRequest.GetResponse())
      {
        // Places the HttpWebResponse in a StreamReader with the Encoding for windows operating systems
        Encoding encoding = Encoding.GetEncoding(1252);
        StreamReader responseStream = new StreamReader(response.GetResponseStream(), encoding);

        // Reads the entire response and places it in the ResponseString
        _response = responseStream.ReadToEnd();

        // Testing
        Console.WriteLine(_response);
      }
    }
  }
}