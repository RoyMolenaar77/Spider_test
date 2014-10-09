using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using AuditLog4Net.Adapter;

namespace Concentrator.Plugins.SennHeiser
{
  class xlsParser
  {
    private string xlsPath = string.Empty;
    private IAuditLogAdapter log;

    public xlsParser(string xlsPath, IAuditLogAdapter log4net)
    {
      this.log = log4net;
      this.xlsPath = xlsPath;
    }

    public DataTable[] Process()
    {

      try
      {
        string path = xlsPath;

        OleDbConnection con = null;
        System.Data.DataTable exceldt = null;

        try
        {
          if (path.Contains(".xlsx"))
          {
            con =
              new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path +
                                  ";Extended Properties=\"Excel 12.0;HDR=YES;\"");
          }

          else if (path.Contains(".xls"))
          {
            con =
              new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path +
                                  ";Extended Properties=Excel 8.0");
          }

          else
          {
            log.Error("Foutief bestand");
            return null;
          }

          con.Open();

          using (exceldt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null))
          {
            if (exceldt == null)
            {
              log.AuditError("Fout verwerken file: Ongeldig tablad");
            }

            String[] excelSheets = new String[exceldt.Rows.Count];
            log.AuditInfo(string.Format("Found {0} worksheets in excel", exceldt.Rows.Count));
            int i = 0;

            DataTable[] tables = new DataTable[exceldt.Rows.Count];
            // Add the sheet name to the string array.
            foreach (DataRow row in exceldt.Rows)
            {
              excelSheets[i] = row["TABLE_NAME"].ToString();
              
              string sheet = row["TABLE_NAME"].ToString();
              OleDbDataAdapter da = null;
              DataTable dt = null;

              da = new OleDbDataAdapter(string.Format("select * from [{0}]", sheet), con);
              dt = new DataTable();
              da.Fill(dt);

              // exceldt.
              tables[i] = dt;
              i++;
            }
            return tables;
          }
          if (con != null)
          {
            con.Close();
            con.Dispose();
          }

          //File.(path, Path.Combine(ConfigurationManager.AppSettings["ExcelProcessedDirectory"], e.Name + "-" + Guid.NewGuid()));
        }
        catch (IOException ex)
        {
          log.AuditFatal("Fout verwerken file: " + ex.Message);
          //File.Move(e.FullPath, Path.Combine(ConfigurationManager.AppSettings["ExcelErrorDirectory"], e.Name + "-" + Guid.NewGuid()));
        }
        catch (Exception ex)
        {
          log.AuditFatal("Fout verwerken file: " + ex.Message);

          //if (File.Exists(e.FullPath))
          //  File.Move(e.FullPath, Path.Combine(ConfigurationManager.AppSettings["ExcelErrorDirectory"], e.Name + "-" + Guid.NewGuid()));
          //Logging.UserMail("Fout bij het verwerken van de excel order, de naam van het tabblad dient 'EDI_Order_V2' te zijn. Of u maakt gebruik van een verouderde versie, vraag uw accountmanager naar de nieuwe EDI Excel template", mailLog.Mailaddress, DocumentType.Excel);
        }
        finally
        {
          if (con != null)
          {
            con.Close();
            con.Dispose();

          }
        }
      }
      catch (Exception ex)
      {
        log.AuditFatal(ex.InnerException);
      }
      return null;
    }

    public DataTable ProcessFirst()
    {
      try
      {
        string path = xlsPath;

        OleDbConnection con = null;
        System.Data.DataTable exceldt = null;

        try
        {
          if (path.Contains(".xlsx"))
          {
            con =
              new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path +
                                  ";Extended Properties=\"Excel 12.0;HDR=YES;\"");
          }
          else if (path.Contains(".xls"))
          {
            con =
              new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path +
                                  ";Extended Properties=Excel 8.0");
          }
          else
          {
            log.Error("Foutief bestand");
            return null;
          }

          con.Open();

          using (exceldt = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null))
          {
            if (exceldt == null)
            {
              log.AuditError("Fout verwerken file: Ongeldig tablad");
            }

            String[] excelSheets = new String[exceldt.Rows.Count];
            log.AuditInfo(string.Format("Found {0} worksheets in excel", exceldt.Rows.Count));
            int i = 0;


            // Add the sheet name to the string array.
            foreach (DataRow row in exceldt.Rows)
            {

              //excelSheets[exceldt.Rows.Count - 1] = row["TABLE_NAME"].ToString();
              i++;
              string sheet = row["TABLE_NAME"].ToString();
              OleDbDataAdapter da = null;
              DataTable dt = null;

            
              
                da = new OleDbDataAdapter(string.Format("select * from [{0}]", sheet), con);
                dt = new DataTable();
                da.Fill(dt);

                // exceldt.
                return dt;
              

            }
          }
          if (con != null)
          {
            con.Close();
            con.Dispose();
          }

          //File.(path, Path.Combine(ConfigurationManager.AppSettings["ExcelProcessedDirectory"], e.Name + "-" + Guid.NewGuid()));
        }
        catch (IOException ex)
        {
          log.AuditFatal("Fout verwerken file: " + ex.Message);
          //File.Move(e.FullPath, Path.Combine(ConfigurationManager.AppSettings["ExcelErrorDirectory"], e.Name + "-" + Guid.NewGuid()));
        }
        catch (Exception ex)
        {
          log.AuditFatal("Fout verwerken file: " + ex.Message);

          //if (File.Exists(e.FullPath))
          //  File.Move(e.FullPath, Path.Combine(ConfigurationManager.AppSettings["ExcelErrorDirectory"], e.Name + "-" + Guid.NewGuid()));
          //Logging.UserMail("Fout bij het verwerken van de excel order, de naam van het tabblad dient 'EDI_Order_V2' te zijn. Of u maakt gebruik van een verouderde versie, vraag uw accountmanager naar de nieuwe EDI Excel template", mailLog.Mailaddress, DocumentType.Excel);
        }
        finally
        {
          if (con != null)
          {
            con.Close();
            con.Dispose();

          }
        }
      }
      catch (Exception ex)
      {
        log.AuditFatal(ex.InnerException);
      }
      return null;
    }

  }

}
