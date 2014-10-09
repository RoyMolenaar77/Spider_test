using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Concentrator.Objects;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Plugins.Magento.Helpers;
using Concentrator.Plugins.Magento.Models;
using Concentrator.Web.Objects.EDI;
using System.Web.Script.Serialization;

namespace Concentrator.Plugins.Magento
{
  public class ResponseListener : ConcentratorPlugin
  {
    HttpListener _httpListener = new HttpListener();
    ManualResetEvent _signalStop = new ManualResetEvent(false);

    public override string Name
    {
      get { return "Magento Listener started"; }
    }

    protected override void Process()
    {
      _httpListener.Prefixes.Clear();
      //Set prefix from Config file to listen to specific URI
      _httpListener.Prefixes.Add(GetConfiguration().AppSettings.Settings["ListenerPrefixes"].Value);
      _httpListener.Start();

      _httpListener.BeginGetContext(ListenerCallback, _httpListener);

      log.Info("HttpListener started succesfully");

      _signalStop.WaitOne();
    }

    void ListenerCallback(IAsyncResult result)
    {
      XmlDocument doc = new XmlDocument();
      HttpListenerContext context = null;
      try
      {
        //Get listener instance from async result
        HttpListener listener = (HttpListener)result.AsyncState;
        if (!listener.IsListening)
          return;

        context = listener.EndGetContext(result);
        log.Info("Response received");

        string responseString;
        using (StreamReader reader = new StreamReader(context.Request.InputStream))
        {
          responseString = reader.ReadToEnd();
        }

        try
        {
          XmlDocument xmlDoc = new XmlDocument();
          doc.LoadXml(responseString);
          ProcessResponse(doc, doc.DocumentElement.Name);
          context.Response.StatusCode = 200;
        }
        catch (WebException ex)
        {
          log.Debug("Web exception", ex);
          var response = ex.Response as HttpWebResponse;

          if (response.StatusCode == HttpStatusCode.GatewayTimeout || response.StatusCode == HttpStatusCode.BadGateway || response.StatusCode == HttpStatusCode.ServiceUnavailable)
          {
            context.Response.StatusCode = (int)response.StatusCode;
          }
          else //any other case, it is ok
          {
            context.Response.StatusCode = 200;
          }

          log.Error("Error processing response", ex);
        }

        catch (Exception ex)
        {

          log.Error("Error processing response", ex);
        }

        using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
        {
          writer.WriteLine("Response received succesfully");
        }
        context.Response.Close();
      }
      catch (Exception ex)
      {
        log.Debug("Writing of response to xml failed", ex);
      }
      finally
      {
        if (_httpListener != null && _httpListener.IsListening)
        {
          _httpListener.BeginGetContext(ListenerCallback, _httpListener);
        }
      }
      log.Info("Response processing ended");

    }
    public T ExtractResponseMessage<T>(XmlDocument doc)
    {
      //// Get XML information
      XmlSerializer xs = new XmlSerializer(typeof(T));
      XmlNodeReader nodeReader = new XmlNodeReader(doc.DocumentElement);
      return (T)xs.Deserialize(nodeReader);
    }

    private void ProcessResponse(XmlDocument responseString, string prefix)
    {
      object ediDocument = null;
      int bskIdentifier = 0;
      switch (prefix)
      {
        case "OrderResponse":
          ediDocument = ExtractResponseMessage<OrderResponse>(responseString);
          bskIdentifier = ((OrderResponse)ediDocument).OrderHeader.BSKIdentifier;
          break;
        case "ShippingNotification":
          ediDocument = ExtractResponseMessage<ShippingNotification>(responseString);
          bskIdentifier = ((ShippingNotification)ediDocument).ShipmentOrderHeader.BSKIdentifier;

          break;
        case "InvoiceNotification":
          ediDocument = ExtractResponseMessage<InvoiceNotification>(responseString);
          bskIdentifier = ((InvoiceNotification)ediDocument).InvoiceOrderHeader.BSKIdentifier;
          break;
        case "Statuses":
          ediDocument = XDocument.Parse(responseString.OuterXml);
          break;
      }

      Connector connector = null;
      using (var unit = GetUnitOfWork())
      {
        if (bskIdentifier != 0)
          connector = unit.Scope.Repository<Connector>().GetSingle(x => x.BSKIdentifier == bskIdentifier);
        else
        {
          if (ediDocument != null)
          {
            var connectorid = int.Parse(((XDocument)ediDocument).Root.Attribute("ConnectorID").Value);
            connector = unit.Scope.Repository<Connector>().GetSingle(x => x.ConnectorID == connectorid);
          }
        }

        if (connector == null)
        {
          string bsk = bskIdentifier.ToString();
          var setting = unit.Scope.Repository<ConnectorSetting>().GetSingle(x => x.SettingKey == "ShopOrderBSK" && x.Value == bsk);

          if (setting != null)
            connector = setting.Connector;

          if (connector == null)
          {
            log.WarnFormat("Process response failed for {0} message", prefix);
            return;
          }
        }
      }

      OrderHelper helper = new OrderHelper(connector.Connection);
      sales_flat_order order = null;
      switch (prefix)
      {
        case "OrderResponse":
          OrderResponse ediResponse = ediDocument as OrderResponse;



          order = helper.GetSalesOrder(ediResponse.OrderHeader.WebSiteOrderNumber);

          if (order != null)
          {
            order.state = "processing";
            order.status = "concentrator";


            log.Info("For order " + order.increment_id + " updating the status to concentrator ");
            helper.UpdateOrderStatus(ediResponse.OrderHeader.WebSiteOrderNumber, MagentoOrderState.Processing, MagentoOrderStatus.concentrator);

            int cancelledlines = 0;
            int backorderLines = 0;
            int lines = 0;

            foreach (var line in ediResponse.OrderDetails)
            {


              //var responseLine = (from r in mctx.sales_flat_order_item
              //                    where r.item_id == line.LineNumber
              //                    select r).FirstOrDefault();



              helper.UpdateOrderLine(order, line);
              lines++;
              var lineEntity = helper.GetOrderLine(order, line.ProductIdentifier.ManufacturerItemID);

              //if (responseLine != null)
              //{
              //  lines++;

              if (line.Quantity.QuantityCancelled > 0)
                cancelledlines++;

              if (line.Quantity.QuantityBackordered > 0)
                backorderLines++;

            }

            #region REPLACED WITH FUNCTION : DiSCUSS WITH FREEK

            //if (cancelledlines == lines)
            //{
            //  // helper.UpdateOrderStatus(order.increment_id, MagentoOrderState.canceled, MagentoOrderStatus.canceled);
            //  log.Info("For order " + order.increment_id + " , trying to call the cancel shipment url ");
            //  if (((ConnectorSystemType)connector.ConnectorSystemType).Has(ConnectorSystemType.CallShipmentLink))
            //  {

            //    int ediOrderLines = ediResponse.OrderDetails.Count();

            //    var isIgnorable = (ediOrderLines == 1 && (ediResponse.OrderDetails.First().ProductIdentifier.ProductNumber.ToLower() == "ShippingCostProductID_IDEAL".ToLower() || ediResponse.OrderDetails.First().ProductIdentifier.ProductNumber.ToLower() == "ShippingCostProductID_CREDITCARD".ToLower()));
            //    if (!isIgnorable)
            //    {
            //      string url = connector.ConnectorSettings.GetValueByKey("CancelShipmentUrl", string.Empty);
            //      if (!string.IsNullOrEmpty(url))
            //      {
            //        var skus = new Dictionary<string, int>();
            //        StringBuilder sb = new StringBuilder();


            //        for (int i = 0; i < ediOrderLines; i++)
            //        {
            //          var line = ediResponse.OrderDetails[i];

            //          sb.Append(HttpUtility.HtmlEncode(line.ProductIdentifier.ManufacturerItemID) + "," + line.Quantity.QuantityShipped);
            //          if (i != ediOrderLines - 1) sb.Append(";");
            //        }

            //        ///basstock/shipment/shippartialorder/order_id/100000342/items/SKU1,5;SKU2,7/trackin_code/123,2345

            //        url = string.Format(url, ediResponse.OrderHeader.WebSiteOrderNumber, HttpUtility.UrlEncode(sb.ToString()));
            //        log.Info("For order " + order.increment_id + " , called cancel shipment url " + url);
            //        WebRequest req = WebRequest.Create(url);
            //        using (WebResponse resp = req.GetResponse())
            //        {
            //          log.AuditInfo("Url for partial shipment called for " + connector.Name + " returned:" + (((HttpWebResponse)resp).StatusDescription));
            //        }
            //      }
            //    }
            //  }
            //}

            #endregion

            if (cancelledlines > 0)
            {
              int totalLines = 0;
              totalLines = helper.GetOrderLinesCount(order, true);
              CancelItems(order, connector, ediResponse, cancelledlines, totalLines, helper);
            }

            //if (backorderLines == lines)
            //{
            //  //helper.UpdateOrderStatus(order.increment_id, MagentoOrderState.Processing, MagentoOrderStatus.backorder);
            //}

          }
          log.DebugFormat("Processed OrderResponse for order {0} (Connector : {1})", ediResponse.OrderHeader.WebSiteOrderNumber, connector.Name);

          break;
        case "ShippingNotification":
          ShippingNotification ediShipment = ediDocument as ShippingNotification;
          log.Info("Shipment order for " + ediShipment.ShipmentOrderHeader.WebSiteOrderNumber);

          bool shopOrder = false;
          order = helper.GetSalesOrder(ediShipment.ShipmentOrderHeader.WebSiteOrderNumber);


          if (order != null)
          {
            if (ediShipment.ShipmentOrderHeader.CustomerOrder.Contains("Winkel#")
              || ediShipment.ShipmentOrderHeader.CustomerOrder.Contains("#winkel"))
            {
              shopOrder = true;
            }

            int cancelledlines = 0;
            int lines = 0;
            foreach (var line in ediShipment.ShipmentOrderDetails)
            {
              long lineNumber = 0;

              if (long.TryParse(line.LineNumber, out lineNumber))
              {
                var responseLine = helper.GetOrderLine(order, line.ProductIdentifier.ManufacturerItemID);

                if (responseLine != null)
                {
                  lines++;

                  if (line.Quantity.QuantityCancelled > 0)
                    cancelledlines++;

                  helper.UpdateOrderLine(order, line);
                }
              }
            }

            log.Info("Number of cancelled lines " + cancelledlines);

            log.Info("For order " + order.increment_id + " trying to call shipment url");
            if (((ConnectorSystemType)connector.ConnectorSystemType).Has(ConnectorSystemType.CallShipmentLink))
            {
              int ediOrderLines = ediShipment.ShipmentOrderDetails.Count();

              JsonOrder orderJson = new JsonOrder();
              orderJson.id = order.increment_id;
              orderJson.tracking_codes = ediShipment.ShipmentOrderDetails.Select(x => x.ShipmentInformation.TrackAndTraceNumber).Distinct().ToArray();
              orderJson.lines = new JsonOrderLine[ediOrderLines];

              var isIgnorable = (ediOrderLines == 1 && (ediShipment.ShipmentOrderDetails.First().ProductIdentifier.ManufacturerItemID.ToLower() == "ShippingCostProductID_IDEAL".ToLower() || ediShipment.ShipmentOrderDetails.First().ProductIdentifier.ManufacturerItemID.ToLower() == "ShippingCostProductID_CREDITCARD".ToLower()));
              if (!isIgnorable)
              {
                string url = connector.ConnectorSettings.GetValueByKey("SCBUrl", string.Empty);
                if (!string.IsNullOrEmpty(url))
                {
                  var skus = new Dictionary<string, int>();

                  for (int i = 0; i < ediOrderLines; i++)
                  {
                    var line = ediShipment.ShipmentOrderDetails[i];

                    orderJson.lines[i] = new JsonOrderLine()
                    {
                      lineid = line.ProductIdentifier.ManufacturerItemID,
                      qty = line.Quantity.QuantityShipped,
                      status = "S"
                    };
                  }

                  SendGenericUpdate(orderJson, connector, helper);
                }
              }
            }
          }

          log.DebugFormat("Processed ShippingNotification for order {0} (Connector : {1})", ediShipment.ShipmentOrderHeader.WebSiteOrderNumber, connector.Name);

          break;
        case "InvoiceNotification":
          InvoiceNotification ediInvoice = ediDocument as InvoiceNotification;

          order = helper.GetSalesOrder(ediInvoice.InvoiceOrderHeader.WebSiteOrderNumber);

          if (order != null)
          {
            foreach (var line in ediInvoice.InvoiceOrderDetails)
            {
              long lineNumber = 0;

              if (long.TryParse(line.LineNumber, out lineNumber))
              {
                var responseLine = helper.GetOrderLine(order, line.ProductIdentifier.ManufacturerItemID);

                if (responseLine != null)
                {
                  helper.UpdateOrderLine(order, line);
                }
              }
            }
          }
          log.DebugFormat("Processed InvoiceNotification for order {0} (Connector : {1})", ediInvoice.InvoiceOrderHeader.WebSiteOrderNumber, connector.Name);

          break;
        case "Statuses":
          string websiteOrderNumber = ((XDocument)ediDocument).Root.Attribute("WebsiteOrderNumber").Value;

          Dictionary<string, string> urls = new Dictionary<string, string>();

          var statuses = ((XDocument)ediDocument).Root.Elements("Status").GroupBy(x => x.Element("StatusDescription").Value).ToArray();

          foreach (var statusGroup in statuses)
          {
            var count = statusGroup.Count();
            var statusId = statusGroup.Key;
            var statusUrl = connector.ConnectorSettings.GetValueByKey(statusGroup.Key, string.Empty);

            JsonOrder ord = new JsonOrder();
            ord.id = websiteOrderNumber;
            ord.tracking_codes = new string[0];
            ord.lines = new JsonOrderLine[count];

            int c = 0;

            foreach (var stat in statusGroup)
            {

              ord.lines[c] = new JsonOrderLine()
              {
                lineid = stat.Element("VendorItemNumber").Value,
                qty = int.Parse(stat.Element("Quantity").Value),
                status = statusId[0].ToString().ToUpper()
              };
              c++;
            }
            SendGenericUpdate(ord, connector, helper, statusUrl);
          }
          break;
      }
    }

    private class JsonOrder
    {
      public string[] tracking_codes { get; set; }

      /// <summary>
      /// The website order number
      /// </summary>
      public string id { get; set; }

      public JsonOrderLine[] lines { get; set; }
    }

    private class JsonOrderLine
    {
      /// <summary>
      /// The sku of the product
      /// </summary>
      public string lineid { get; set; }

      public int qty { get; set; }

      /// <summary>
      /// The status of the orderline
      /// 
      /// </summary>
      public string status { get; set; }
    }


    private void CancelItems(sales_flat_order order, Connector connector, OrderResponse ediResponse, int cancelledLines, int totalLines, OrderHelper helper)
    {
      log.Info("For order " + order.increment_id + " , trying to call the cancel shipment url ");
      if (((ConnectorSystemType)connector.ConnectorSystemType).Has(ConnectorSystemType.CallShipmentLink))
      {

        JsonOrder orderJson = new JsonOrder();
        orderJson.id = order.increment_id;
        orderJson.tracking_codes = new string[0];

        int ediOrderLines = ediResponse.OrderDetails.Count();

        orderJson.lines = new JsonOrderLine[ediOrderLines];

        var isIgnorable = (ediOrderLines == 1 && (ediResponse.OrderDetails.First().ProductIdentifier.ProductNumber.ToLower() == "ShippingCostProductID_IDEAL".ToLower() || ediResponse.OrderDetails.First().ProductIdentifier.ProductNumber.ToLower() == "ShippingCostProductID_CREDITCARD".ToLower()));
        if (!isIgnorable)
        {

          for (int i = 0; i < ediOrderLines; i++)
          {
            var line = ediResponse.OrderDetails[i];

            orderJson.lines[i] = new JsonOrderLine()
            {
              lineid = line.ProductIdentifier.ManufacturerItemID,
              qty = line.Quantity.QuantityCancelled,
              status = "C"
            };

          }
        }
        SendGenericUpdate(orderJson, connector, helper);
      }
    }

    /// <summary>
    /// Call the generic 
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    private bool SendGenericUpdate(JsonOrder order, Connector conn, OrderHelper helper, string url = "")
    {
      bool success = false;

      if (url == string.Empty)
        url = conn.ConnectorSettings.GetValueByKey<string>("SCBUrl", string.Empty);

      url.ThrowIfNullOrEmpty();

      if (url.Contains("{StoreCodeWillBeDetermined}"))
      {

        var orderStoreCode = helper.GetOrderStoreCode(order.id);

        if (conn.ConnectorID == 7) //nastyyyyyy fixx!!!!
        {
          url = url.Replace("{StoreCodeWillBeDetermined}", orderStoreCode.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries)[1]);

        }
        else
        {
          url = url.Replace("{StoreCodeWillBeDetermined}", orderStoreCode);
        }
      }

      string jsonToPass = string.Empty;

      JavaScriptSerializer serializer = new JavaScriptSerializer();
      jsonToPass = serializer.Serialize(order);

      var request = (HttpWebRequest)WebRequest.Create(url);
      request.Method = "POST";
      request.ContentType = "text/json";

      using (var streamWriter = new StreamWriter(request.GetRequestStream()))
      {
        streamWriter.Write(jsonToPass);
      }
      log.Info(order.id + ": Calling url " + url);
      try
      {
        using (WebResponse resp = request.GetResponse())
        {
          var response = ((HttpWebResponse)resp);

          log.Info(order.id + ": Url called returned status: " + (response.StatusCode + " " + response.StatusDescription));

          string returnedMessage = string.Empty;

          using (var reader = new StreamReader(resp.GetResponseStream()))
          {
            returnedMessage = reader.ReadToEnd();

            log.Info(order.id + ": Url returned message  " + returnedMessage);
          }


        }
      }
      catch (WebException e)
      {
        var response = ((HttpWebResponse)e.Response);
        using (var reader = new StreamReader(response.GetResponseStream()))
        {
          var returnedMessage = reader.ReadToEnd();

          if (response.StatusCode == HttpStatusCode.InternalServerError) //log as an error
          {
            log.AuditError(string.Format("Processing {0} failed : code {1} | message {2} | url: {3} | jsonPassed: {4}. Order is marked as processed in the Concentrator", order.id, (int)response.StatusCode + "  " + response.StatusCode.ToString(), returnedMessage, url, jsonToPass));
          }

          throw e;
        }
      }
      catch (Exception e)
      {
        log.AuditError(string.Format("Status update failed for {0}", order.id));
        throw e;
      }
      return success;
    }
  }
}
