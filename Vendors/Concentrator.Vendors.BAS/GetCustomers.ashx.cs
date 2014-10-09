using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Data;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using System.Web.Services;

namespace Concentrator.Vendors.BAS.Web.Services
{
  /// <summary>
  /// Summary description for $codebehindclassname$
  /// </summary>
  [WebService(Namespace = "http://tempuri.org/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  public class GetCustomers : IHttpHandler
  {

    public void ProcessRequest(HttpContext context)
    {
      context.Response.ContentType = "text/plain";

      //DataSet ds = new DataSet();

      Database db = DatabaseFactory.CreateDatabase();

      if (string.IsNullOrEmpty(context.Request.Params["BU"]))
      {
        context.Response.Write("Invalid BusinessUnit");
        return;
      }

      if (string.IsNullOrEmpty(context.Request.Params["Type"]))
      {
        context.Response.Write("Invalid Type; customer, phone and email");
        return;
      }

      string separator = ";";

      // Creates the CSV file as a stream, using the given encoding.
      using (StringWriter sw = new StringWriter())
      {
        using (DbCommand command = db.GetStoredProcCommand("PortalGetWebCustomers"))
        {
          db.AddInParameter(command, "BusinessUnit", DbType.String, context.Request.Params["BU"]);

          IDataReader dr = db.ExecuteReader(command);

          //ds = db.ExecuteDataSet(command);
          int result = -1;
          int dataTable = 0;

          switch (context.Request.Params["Type"])
          {
            case "customer":
              result = 0;
              break;
            case "phone":
              result = 1;
              break;
            case "email":
              result = 2;
              break;     
          }

          if (result < 0)
          {
            context.Response.Write("Invalid Type; customer, phone and email");
            return;
          }

          if (result > 0)
          {
            while (dr.NextResult())
            {
              dataTable++;

              if (dataTable == result)
                break;
            }
          }

          DataTable dtSchema = dr.GetSchemaTable();

          string strRow; // represents a full row

          // Writes the column headers if the user previously asked that.
          sw.WriteLine(columnNames(dtSchema, separator));

          // Reads the rows one by one from the SqlDataReader
          // transfers them to a string with the given separator character and
          // writes it to the file.
          while (dr.Read())
          {
            strRow = "";
            for (int i = 0; i < dr.FieldCount; i++)
            {
              strRow += dr.GetValue(i).ToString().Trim().Replace(';', ':');
              if (i < dr.FieldCount - 1)
              {
                strRow += separator;
              }
            }
            sw.WriteLine(strRow);
          }
          context.Response.Write(sw.ToString());
        }
        // Closes the text stream and the database connection.
        sw.Close();

      }
    }

    private string columnNames(DataTable dtSchemaTable, string delimiter)
    {
      string strOut = "";
      if (delimiter.ToLower() == "tab")
      {
        delimiter = "\t";
      }

      for (int i = 0; i < dtSchemaTable.Rows.Count; i++)
      {
        strOut += dtSchemaTable.Rows[i][0].ToString();
        if (i < dtSchemaTable.Rows.Count - 1)
        {
          strOut += delimiter;
        }

      }
      return strOut;
    }

    public bool IsReusable
    {
      get
      {
        return false;
      }
    }
  }
}
