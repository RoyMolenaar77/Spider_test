using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using Concentrator.Web.Objects.EDI;

namespace Concentrator.Objects.Ordering.EDI
{
  public static class EDIUtility
  {
    public static bool SendMessage(XmlDocument document)
    {
      using (Stream r = new MemoryStream())
      {
        using (XmlWriter wr = XmlWriter.Create(r))
        {
          document.WriteTo(wr);
        }
        return SendMessage(r);
      }
    }

    public static bool SendMessage(XDocument document)
    {
      using (var r = new MemoryStream())
      {
        using (XmlWriter wr = XmlWriter.Create(r))
        {
          document.WriteTo(wr);
        }
        return SendMessage(r);
      }
    }

    public static bool SendMessage(Stream s)
    {
      HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(ConfigurationManager.AppSettings["EDIUrl"]);
      request.Method = "POST";
      using (Stream r = request.GetRequestStream())
      {
        s.Position = 0;
        byte[] buffer = new byte[s.Length];
        s.Read(buffer, 0, (int)s.Length);
        r.Write(buffer, 0, (int)s.Length);
      }

      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      using (Stream str = response.GetResponseStream())
      {
        using (var reader = new StreamReader(str))
        {
          Console.WriteLine(reader.ReadToEnd());
        }
      }

      if (response.StatusCode == HttpStatusCode.OK)
      {
        return true;
      }
      else
      {
        throw new Exception("EDI Request failed. Status Code: " + response.StatusCode);
      }
    }
  }
}
