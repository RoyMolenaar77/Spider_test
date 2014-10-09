using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using System.Data.OleDb;

namespace Concentrator.Objects.Utility
{
  public class ExcelToCSV
  {
  private string sourceFile = string.Empty;
    private Dictionary<string, DataTable> streams;
    private bool _includeHeader = false;

    public void Save()
    {

      foreach (var stream in streams)
      {
        using (var streamWriter = new StreamWriter(sourceFile + "_" + stream.Key + ".csv"))
        {

          for (int x = 0; x < stream.Value.Rows.Count; x++)
          {
            string rowString = "";

            for (int y = 0; y < stream.Value.Columns.Count; y++)
            {
              rowString += "\"" + stream.Value.Rows[x][y].ToString() + "\",";
            }
            streamWriter.WriteLine(rowString);
          }
        }
      }
    }

    public ExcelToCSV(string sourceFile, bool includeHeader)
    {
      streams = new Dictionary<string, DataTable>();
      _includeHeader = includeHeader;

      this.sourceFile = sourceFile;

      string excel2010ConnectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + sourceFile +
                                                       ";Extended Properties=\"Excel 12.0;{0}\"",
                                                       includeHeader ? "" : "HDR=Yes");
      string excel2003ConnectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sourceFile +
                                                       ";Extended Properties=\"Excel 8.0;{0}\"",
                                                       includeHeader ? "" : "HDR=Yes");

      string strConn = sourceFile.EndsWith("xls") ? excel2003ConnectionString : excel2010ConnectionString;

      OleDbConnection conn = null;

      OleDbCommand cmd = null;

      OleDbDataAdapter da = null;

      FileInfo fileinfo = new FileInfo(sourceFile);

      try
      {
        conn = new OleDbConnection(strConn);

        conn.Open();

        var exceldt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

        string[] sheets = new string[exceldt.Rows.Count];


        for (int s = 0; s < sheets.Length; s++)
        {
          sheets[s] = exceldt.Rows[s].ItemArray[2].ToString();
        }

        int sheetNumber = 0;

        foreach (var sheet in sheets)
        {
          sheetNumber++;

          string targetFile = sourceFile + "_" + sheetNumber.ToString() + ".csv";

          cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);

          cmd.CommandType = CommandType.Text;

          da = new OleDbDataAdapter(cmd);

          DataTable dt = new DataTable();

          da.Fill(dt);


          //for (int x = 0; x < dt.Rows.Count; x++)
          //{
          //  string rowString = "";

          //  for (int y = 0; y < dt.Columns.Count; y++)
          //  {
          //    rowString += "\"" + dt.Rows[x][y].ToString() + "\",";
          //  }
          //  sb.AppendLine(rowString);
          //}

          streams.Add(sheet, dt);

          Console.WriteLine();

          //Console.WriteLine(string.Format("Sheet {0} converted to csv", sheet.ToString()));
        }


        // Console.WriteLine("Done! Your " + sourceFile + " has been converted to csv");

        // Console.WriteLine();
      }

      catch (Exception exc)
      {

        Console.WriteLine(exc.ToString());

        Console.ReadLine();

      }

      finally
      {

        if (conn.State == ConnectionState.Open)

          conn.Close();

        conn.Dispose();

        cmd.Dispose();

        da.Dispose();

      }

    }

    public DataSet asDataSet()
    {
      FileInfo fileinfo = new FileInfo(sourceFile);

      string fileName = fileinfo.Name;

      DataSet ds = new DataSet(fileName);

      foreach (var stream in streams.Keys)
      {
        streams[stream].TableName = stream;

        ds.Tables.Add(streams[stream]);
      }

      return ds;
    }
  }

}

