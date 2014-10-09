using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using Concentrator.Objects.EDI;
using Concentrator.Objects.ConcentratorService;
using Concentrator.Objects.Models.EDI.Order;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Plugins.EDI
{
  public class EdiListener : ConcentratorPlugin
  {
    HttpListener _httpListener = new HttpListener();
    ManualResetEvent _signalStop = new ManualResetEvent(false);
    private const int _vendorID = 1;
    private bool _authentication = false;

    public override string Name
    {
      get { return "Concentrator EDI Listener started"; }
    }

    protected override void Process()
    {
      _httpListener.Prefixes.Clear();
      //Set prefix from Config file to listen to specific URI
      _httpListener.Prefixes.Add(GetConfiguration().AppSettings.Settings["EDIListenerPrefixes"].Value);

      _authentication = bool.Parse(GetConfiguration().AppSettings.Settings["RequireAuthentication"].Value);

      if (_authentication)
      {
        _httpListener.AuthenticationSchemes = AuthenticationSchemes.Basic;

      }

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
          if (!string.IsNullOrEmpty(responseString))
          {
            using (var unit = GetUnitOfWork())
            {
              ConnectorRelation relation = null;
              #region Authentication
              if (_authentication)
              {
                HttpListenerBasicIdentity identity = (HttpListenerBasicIdentity)context.User.Identity;
                relation = unit.Scope.Repository<ConnectorRelation>().GetSingle(x => x.Username == identity.Name && x.Password == identity.Password && x.IsActive);
                if (relation == null)
                {
                  #region Forward
                  if (GetConfiguration().AppSettings.Settings["ForwardPostUrl"] != null && !string.IsNullOrEmpty(GetConfiguration().AppSettings.Settings["ForwardPostUrl"].Value))
                  {
                    try
                    {
                      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(GetConfiguration().AppSettings.Settings["ForwardPostUrl"].Value);
                      request.Method = "POST";
                      request.Credentials = new NetworkCredential(identity.Name, identity.Password);

                      byte[] byteData = UTF8Encoding.UTF8.GetBytes(responseString);

                      using (Stream postStream = request.GetRequestStream())
                      {
                        postStream.Write(byteData, 0, byteData.Length);
                      }

                      HttpWebResponse objResponse = (HttpWebResponse)request.GetResponse();
                      using (StreamReader sr = new StreamReader(objResponse.GetResponseStream()))
                      {
                        context.Response.StatusCode = (int)objResponse.StatusCode;
                        context.Response.StatusDescription = objResponse.StatusDescription;
                        using (StreamWriter writer = new StreamWriter(objResponse.GetResponseStream()))
                        {
                          writer.WriteLine(sr.ReadToEnd());
                        }
                      }
                    }
                    catch (Exception ex)
                    {
                      log.AuditError("Forward EDI message failed", ex);
                    }
                  #endregion
                  }
                  else
                  {
                    context.Response.StatusCode = 401;
                    log.AuditInfo(string.Format("Login failed for username: {0} and password: {1}", identity.Name, identity.Password), "EDI listener");
                  }
                }
              }

              if (context.Response.StatusCode == 401)
              {
                context.Response.AddHeader("WWW-Authenticate",
                    "Basic Realm=\"EDI Service\""); // show login dialog
                byte[] message = new UTF8Encoding().GetBytes("Access denied");
                context.Response.ContentLength64 = message.Length;
                context.Response.OutputStream.Write(message, 0, message.Length);
              }
              else
              {

              #endregion

                string logPath = Path.Combine(GetConfiguration().AppSettings.Settings["XMLlogReceive"].Value, DateTime.Now.ToString("dd-MM-yyyy"));

                if (!Directory.Exists(logPath))
                  Directory.CreateDirectory(logPath);

                string fileName = LogFile(logPath, responseString);



                EdiOrderListener ediListener = new EdiOrderListener()
                  {
                    CustomerName = relation == null ? "new" : relation.Name,
                    CustomerIP = context.Request.UserHostAddress,
                    CustomerHostName = context.Request.UserHostName,
                    RequestDocument = responseString,
                    ReceivedDate = DateTime.Now,
                    Processed = false,
                    ConnectorID = int.Parse(GetConfiguration().AppSettings.Settings["DefaultConnectorID"].Value)
                  };

                if (relation != null && relation.ConnectorID.HasValue)
                  ediListener.ConnectorID = relation.ConnectorID.Value;

                unit.Scope.Repository<EdiOrderListener>().Add(ediListener);
                unit.Save();

                context.Response.StatusCode = 200;
                using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
                {
                  writer.WriteLine("Response received succesfully");
                }
              }
            }
          }
          else
          {
            context.Response.StatusCode = (int)HttpStatusCode.NoContent;
            using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
            {
              writer.WriteLine("No content found in request");
            }
          }

        }
        catch (Exception ex)
        {
          log.Error("Error processing response", ex);
        }
              
        context.Response.Close();
      }
      catch (Exception ex)
      {
        log.Error("Writing of response to xml failed", ex);
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




    public static string LogFile(string path, string fileContents)
    {
      if (!path.EndsWith(@"\"))
        path += @"\";

      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);

      if (!path.EndsWith(@"\"))
        path += @"\";

      string fileName = Guid.NewGuid() + ".xml";
      string filePath = Path.Combine(path, fileName);

      File.WriteAllText(filePath, fileContents);

      return fileName;
    }
  }
}

