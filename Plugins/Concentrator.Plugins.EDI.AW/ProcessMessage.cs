using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Net;
using System.Threading;
using System.Xml;
using System.IO;
using System.Xml.Serialization;
using System.Data;
using System.Data.Odbc;
using System.Configuration;
using Concentrator.Objects.ConcentratorService;

namespace Concentrator.Plugins.EDI.AW
{
  public class ResponseListener : ConcentratorPlugin
  {
    HttpListener _httpListener = new HttpListener();
    ManualResetEvent _signalStop = new ManualResetEvent(false);
    private const int _vendorID = 1;
    
    public override string Name
    {
      get { return "Concentrator AW Datalistener started"; }
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
      HttpListenerContext context = null;
      try
      {
        //Get listener instance from async result
        HttpListener listener = (HttpListener)result.AsyncState;
        if (!listener.IsListening)
          return;

        context = listener.EndGetContext(result);
        log.Info("data received");

        string query;
        using (StreamReader reader = new StreamReader(context.Request.InputStream))
        {
          query = reader.ReadToEnd();
        }

        try
        {
          DataSet response = ProcessData(query);
        }
        catch (Exception ex)
        {
          log.Error("Error processing response", ex);
        }

        context.Response.StatusCode = 200;
        using (StreamWriter writer = new StreamWriter(context.Response.OutputStream))
        {
          writer.WriteLine("Response received succesfully");
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

    private string AWConnectionString
    {
      get
      {
        return ConfigurationManager.ConnectionStrings["AW"].ConnectionString;
      }
    }

    private DataSet ProcessData(string query)
    {
      var config = GetConfiguration();

      DataSet ds = new DataSet();
      using (OdbcConnection cn = new OdbcConnection(AWConnectionString))
      {
        cn.Open();
        using (OdbcCommand cmd = new OdbcCommand(query, cn))
        {
          OdbcDataAdapter adapter = new OdbcDataAdapter(cmd);
          adapter.Fill(ds);
        }
        cn.Close();
      }

      return ds;
    }
  }
}
