using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace Spider.MiddleWare.Tweakers
{
  class Program
  {
    static void Main(string[] args)
    {
      string url = "http://pwm.tweakers.net/pricelist";
      WebRequest myReq = WebRequest.Create(url);

      string username = "17";
      string password = "r3G4zxqg";
      string usernamePassword = username + ":" + password;
      CredentialCache mycache = new CredentialCache();
      mycache.Add(new Uri(url), "Basic", new NetworkCredential(username, password));
      myReq.Credentials = mycache;
      myReq.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(new ASCIIEncoding().GetBytes(usernamePassword)));

      WebResponse wr = myReq.GetResponse();
      Stream receiveStream = wr.GetResponseStream();
      using (StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8))
      {
        List<string> rows = new List<string>();

        string record = reader.ReadLine();
        while (record != null)
        {
          rows.Add(record);
          record = reader.ReadLine();
        }

        List<string[]> rowObjects = new List<string[]>();

        int maxColsCount = 0;
        foreach (string s in rows)
        {
          string[] convertedRow = s.Split(new char[] { ';' });
          if (convertedRow.Length > maxColsCount)
            maxColsCount = convertedRow.Length;
          rowObjects.Add(convertedRow);
        }

        DataTable table = new DataTable("Prices");
        for (int i = 0; i < maxColsCount; i++)
        {
          table.Columns.Add(new DataColumn());
        }

        foreach (string[] rowArray in rowObjects)
        {
          table.Rows.Add(rowArray);
        }
        table.AcceptChanges();



        

      }
    }
  }
}