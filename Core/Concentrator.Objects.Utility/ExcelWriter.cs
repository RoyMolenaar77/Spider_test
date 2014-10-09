using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using System.Data;
using System.IO;

namespace Concentrator.Objects.Utility
{
  public class ExcelWriter
  {
    private void GenerateHeaders(ExcelWorksheet sheet, params string[] headers)
    {
      int counter = 1;
      foreach (var header in headers)
      {
        var cell = sheet.Cells[1, counter];
        cell.Value = header;
        cell.Style.Font.Bold = true;
        counter++;
      }
    }

    public string ToExcel(System.Data.DataTable dataTable, string type, string ordernumber, string customerordernumber, int documentcounter, string path)
    {
      try
      {
        ExcelPackage package = new ExcelPackage();

        var worksheet = package.Workbook.Worksheets.Add(type);


        string[] columnNames = (from dc in dataTable.Columns.Cast<DataColumn>() select dc.ColumnName).ToArray();

        GenerateHeaders(worksheet, columnNames);

        int rowCounter = 2;

        foreach (DataRow r in dataTable.Rows)
        {
          //row = sheet.Table.Rows.Add();
          int columnCounter = 1;
          foreach (var v in r.ItemArray)
          {
            worksheet.Cells[rowCounter, columnCounter].Value = v;
            columnCounter++;
          }

          rowCounter++;
        }

        package.SaveAs(new FileInfo(path));

        return path;
      }
      catch (Exception ex)
      {
        return string.Empty;
        //Logging.WriteLogMessage("Fout bij opslaan Excel");
        //Logging.WriteLogMessage(ex);
      }
    }

  }
}
