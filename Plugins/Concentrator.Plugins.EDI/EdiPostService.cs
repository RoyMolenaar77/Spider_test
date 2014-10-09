using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.EDI;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Xml;
using System.IO;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.EDI.Post;
using Concentrator.Objects.DataAccess.UnitOfWork;
using Concentrator.Objects.Models.Orders;
using Concentrator.Objects.Ftp;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Connectors;
using Concentrator.Objects.Enumerations;
using System.Configuration;
using Concentrator.Objects.Utility;
using System.Data;


namespace Concentrator.Plugins.EDI
{
  public class EdiPostService : ConcentratorPlugin
  {
    private const string vendorSettingType = "DispatcherType";

    public override string Name
    {
      get { return "EDI Outbound Processor"; }
    }

    protected override void Process()
    {
      log.Info("Starting EDI outbound process");
      var config = GetConfiguration();

      using (var unit = GetUnitOfWork())
      {
        var outboundMessages = (from o in unit.Scope.Repository<EdiOrderPost>().GetAllAsQueryable()
                                where !o.Processed
                                && (!o.ProcessedCount.HasValue || o.ProcessedCount.Value < 5)
                                select o).ToList();

        foreach (var message in outboundMessages)
        {
          message.Processed = true;
          var connectorRelation = unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.ConnectorRelationID == message.ConnectorRelationID);

          if (message.Type == "Excel")
          {
            XmlDocument doc = new XmlDocument();
                doc.LoadXml(message.PostDocument);

                DataTable table = new DataTable("Acknowledgement");
                table.Columns.Add("EDIIdentifier");
                table.Columns.Add("CustomerOrderNumber");
                table.Columns.Add("ItemNumber");
                table.Columns.Add("QuantityOrdered");
                table.Columns.Add("QuantityBackordered");
                table.Columns.Add("QuantityCancelled");
                table.Columns.Add("Price");
                table.Columns.Add("Message");
            using (XmlReader reader = new XmlNodeReader(doc))
            {
              table.ReadXml(reader);
            }            

            ExcelWriter writer = new ExcelWriter();
            var path = Path.Combine(config.AppSettings.Settings["ExcelPath"].Value,message.Type + message.EdiBackendOrderID + "_" + message.DocumentCounter + ".xlsx");
            writer.ToExcel(table, message.Type,message.EdiBackendOrderID, message.CustomerOrderID, message.DocumentCounter, path);

            EmailDaemon daemon = new EmailDaemon(log);
            daemon.AcknowledgementNotification(connectorRelation.Name, connectorRelation.OutboundOrderConfirmation, false, path);
            
          }
          else if (connectorRelation.ConnectorType.HasValue && connectorRelation.ConnectorType.Value == (int)Objects.Enumerations.EdiConnectorTypeEnum.FtpConnector)
          {
            try
            {
              string fileName = string.Format("{0}-{1}-{2}.xml", message.Type, message.EdiBackendOrderID, message.DocumentCounter);
              if (connectorRelation.UseFtp.HasValue && connectorRelation.UseFtp.Value)
              {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(message.PostDocument);

                if (connectorRelation.FtpType.HasValue && connectorRelation.FtpType.Value == (int)FtpTypeEnum.Customer)
                {
                  FtpManager manager = new FtpManager(connectorRelation.FtpAddress,
                              string.Empty, connectorRelation.OutboundUsername, connectorRelation.FtpPass, false, true, log);

                  using (Stream s = new MemoryStream())
                  {
                    doc.Save(s);
                    manager.Upload(s, fileName);
                  }
                }
                else
                {
                  string basePath = ConfigurationManager.AppSettings["ConcentratorFtpUserDir"];

                  string ftpPath = Path.Combine(basePath, connectorRelation.ConnectorRelationID.ToString());

                  if (!Directory.Exists(ftpPath))
                    Directory.CreateDirectory(ftpPath);

                  doc.Save(Path.Combine(ftpPath, fileName));
                }
              }
            }
            catch (Exception ex)
            {
              message.ErrorMessage = ex.Message;
            }
          }
          else if (connectorRelation.ConnectorType.HasValue && connectorRelation.ConnectorType.Value == (int)Objects.Enumerations.EdiConnectorTypeEnum.HttpConnector)
          {

            DateTime startTimePost = DateTime.Now;
            try
            {
              System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Ssl3;
              HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(message.PostDocumentUrl);
              request.Method = "POST";

              ServicePointManager.ServerCertificateValidationCallback +=
                    delegate(object sender, X509Certificate certificate, X509Chain chain,
                    SslPolicyErrors sslPolicyErrors)
                    {
                      return true;
                    };

              XmlDocument doc = new XmlDocument();
              string contentType = "text/xml";
              string document = message.PostDocument;

              HttpOutboundPostState state = new HttpOutboundPostState(request, message.EdiOrderPostID, unit, startTimePost);

              byte[] byteData = UTF8Encoding.UTF8.GetBytes(document);

              state.Request.ContentType = contentType;
              state.Request.ContentLength = byteData.Length;
              using (Stream postStream = request.GetRequestStream())
              {
                postStream.Write(byteData, 0, byteData.Length);
              }

              request.BeginGetResponse(HttpOutboundCallBack, state);
            }
            catch (Exception ex)
            {
              message.Processed = false;

              if (message.ProcessedCount.HasValue)
                message.ProcessedCount++;
              else
                message.ProcessedCount = 1;

              message.ErrorMessage = ex.Message;
              DateTime resDateTime = new DateTime(DateTime.Now.Ticks - startTimePost.Ticks);
              message.ResponseTime = resDateTime.Second;

            }
            finally
            {
              unit.Save();
            }
          }
          else
          {
            message.ErrorMessage = "No ConnectionRelationSettings, skip outbound message";
          }
          unit.Save();
        }
      }
      log.Info("Finish EDI outbound process");
    }

    public static void HttpOutboundCallBack(IAsyncResult result)
    {
      try
      {
        HttpOutboundPostState state = (HttpOutboundPostState)result.AsyncState;

        using (HttpWebResponse httpResponse = (HttpWebResponse)state.Request.EndGetResponse(result))
        {
          var outbound = (from o in state.Unit.Scope.Repository<EdiOrderPost>().GetAllAsQueryable()
                          where o.EdiOrderPostID == state.OutBoundID
                          select o).FirstOrDefault();

          DateTime dateTime = new DateTime(DateTime.Now.Ticks - state.StartPost.Ticks);
          outbound.ResponseRemark = string.Format("HTTP POST Status {0} in {1} seconds", httpResponse.StatusCode, dateTime.Second);
          outbound.ResponseTime = dateTime.Second;

          switch (httpResponse.StatusCode)
          {
            case HttpStatusCode.OK:
              //POST OK
              outbound.Processed = true;
              break;

            default:

              break;
          }
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Callback failed");
      }
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
