using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Net;
using System.IO;
using System.Configuration;
using System.Threading;

namespace Concentrator.Test.EDI
{
  public partial class Form1 : Form
  {
    private static string _message;
    private static Color _color = Color.Black;

    public Form1()
    {
      InitializeComponent();
      var timer = new System.Windows.Forms.Timer();
      timer.Tick += new EventHandler(timer_Tick);
      timer.Interval = 10;
      timer.Start();
      textBox2.Text = ConfigurationManager.AppSettings["EDIurl"].ToString();
    }

    void timer_Tick(object sender, EventArgs e)
    {
      label1.Text = _message;
      label1.ForeColor = _color;
    }

    private void button1_Click(object sender, EventArgs e)
    {
      try
      {
        button1.Enabled = false;
        if (string.IsNullOrEmpty(textBox2.Text))
        {
          _message = "No EDI url specified";
          _color = Color.Red;
          return;
        }

        if (string.IsNullOrEmpty(textBox1.Text))
        {
          _message = "No File specified";
          _color = Color.Red;
          return;
        }
        _message = "Start Post file";
        using (StreamReader f = new StreamReader(textBox1.Text))
        {
          
          string ediUrl = textBox2.Text;
          XmlDocument document = new XmlDocument();
          document.LoadXml(f.ReadToEnd());
          
          HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ediUrl);
          request.Method = "POST";

          byte[] byteData = UTF8Encoding.UTF8.GetBytes(document.OuterXml);
          using (Stream s = request.GetRequestStream())
          {
            s.Write(byteData, 0, byteData.Length);
          }

          HttpPostState state = new HttpPostState(request);

          IAsyncResult result = request.BeginGetResponse(HttpCallBack, state);
          result.AsyncWaitHandle.WaitOne();
        }

      }
      catch (Exception ex)
      {
        _message = ex.Message;
        _color = Color.Red;
      }
      finally
      {
        button1.Enabled = true;
        button1.Refresh();
      }
    }

    public static void HttpCallBack(IAsyncResult result)
    {
      try
      {
        HttpPostState state = (HttpPostState)result.AsyncState;

        using (HttpWebResponse httpResponse = (HttpWebResponse)state.Request.EndGetResponse(result))
        {
          if (httpResponse.StatusCode == HttpStatusCode.OK)
          {
            _color = Color.Green;
          }
          else
          {
            _color = Color.Black;
          }
          _message = "Code:" + httpResponse.StatusCode.ToString();
          _message += Environment.NewLine;
          _message += "Description" + httpResponse.StatusDescription;
        }
      }
      catch (Exception ex)
      {
        _message = ex.Message;
        _color = Color.Red;
      }
    }

    private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
    {

    }

    private void button2_Click(object sender, EventArgs e)
    {
      int size = -1;
      DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
      if (result == DialogResult.OK) // Test result.
      {
        textBox1.Text = openFileDialog1.FileName;
      }
    }


  }

  public class HttpPostState
  {
    private HttpWebRequest _request;

    public HttpPostState(HttpWebRequest request)
    {    
      _request = request;
    }
  
    public HttpWebRequest Request
    {
      get { return _request; }
    }
  }
}
