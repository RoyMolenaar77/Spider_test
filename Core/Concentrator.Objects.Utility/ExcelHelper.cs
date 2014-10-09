using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CarlosAg.ExcelXmlWriter;
using System.Xml.Linq;
using Concentrator.Web.ServiceClient.AssortmentService;
using System.Data;

namespace Concentrator.Objects.Utility
{
  public class ExcelException : Exception
  {
    private int _errorId;
    public ExcelException(int errorId) { _errorId = errorId; }
    public ExcelException(int errorId, string message) : base(message) { _errorId = errorId; }
    public ExcelException(int errorId, string message, Exception inner) : base(message, inner) { _errorId = errorId; }
    protected ExcelException(
    System.Runtime.Serialization.SerializationInfo info,
    System.Runtime.Serialization.StreamingContext context)
      : base(info, context) { }

    public int ErrorId
    {
      get { return _errorId; }
    }
  }

  public static class ExcelHelper
  {
    //public static Workbook GenerateExcel(List<ExcelColumn> columns, DataSet data, bool generateHeader)
    //{
    //  Workbook book = new Workbook();
    //  Worksheet sheet = book.Worksheets.Add("Assortment");

    //  WorksheetRow row;

    //  GenerateStyles(book.Styles);

    //  foreach (var name in columns)
    //  {
    //    WorksheetColumn column = new WorksheetColumn();
    //    column.AutoFitWidth = true;

    //    sheet.Table.Columns.Add(column);
    //  }

    //  int rowIndex = GenerateHeaders(sheet, generateHeader, columns);

    //  Dictionary<int, Dictionary<string, string>> excelRows = new Dictionary<int, Dictionary<string, string>>();

    //  foreach (var productRow in data.Tables[0].AsEnumerable())
    //  {
    //    rowIndex++;
    //    var product = productRow;
    //    Dictionary<string, string> excelvalues = new Dictionary<string, string>();

    //    foreach (var att in data.Tables[0].AsEnumerable())
    //    {
    //      if (excelvalues.ContainsKey(att.Field<string>))
    //        excelvalues[att.Name.LocalName] = att.Value;
    //      else
    //        excelvalues.Add(att.Name.LocalName, att.Value);
    //    }

    //    foreach (XElement el in product.Elements())
    //    {
    //      if (columns.Any(x => x.ValueFieldName == el.Name.LocalName))
    //      {
    //        if (excelvalues.ContainsKey(el.Name.LocalName))
    //          excelvalues[el.Name.LocalName] = el.Value;
    //        else
    //          excelvalues.Add(el.Name.LocalName, el.Value);
    //      }

    //      foreach (XAttribute att in el.Attributes())
    //      {
    //        if (columns.Any(x => x.ValueFieldName == att.Name.LocalName))
    //        {
    //          if (excelvalues.ContainsKey(att.Name.LocalName))
    //            excelvalues[att.Name.LocalName] = att.Value;
    //          else
    //            excelvalues.Add(att.Name.LocalName, att.Value);

    //        }
    //      }
    //    }
    //    excelRows.Add(rowIndex, excelvalues);
    //  }

    //  foreach (var val in excelRows.Keys)
    //  {
    //    row = sheet.Table.Rows.Add();

    //    foreach (var key in columns)
    //    {
    //      row.Cells.Add(excelRows[val][key.ColumnName]);
    //    }
    //  }

    //  return book;
    //}

    public static Workbook GenerateExcel(int connectorID, List<ExcelColumn> columns, bool generateHeader)
    {
      AssortmentServiceSoapClient client = new AssortmentServiceSoapClient();



      var assortmentSource = new XDocument(client.GetAssortment(1, null, true));
      Workbook book = new Workbook();
      Worksheet sheet = book.Worksheets.Add("Assortment");

      WorksheetRow row;

      GenerateStyles(book.Styles);

      foreach (var name in columns)
      {
        WorksheetColumn column = new WorksheetColumn();
        column.AutoFitWidth = true;

        sheet.Table.Columns.Add(column);
      }

      int rowIndex = GenerateHeaders(sheet, generateHeader, columns);

      Dictionary<int, Dictionary<string, string>> excelRows = new Dictionary<int, Dictionary<string, string>>();

      foreach (var productRow in assortmentSource.Root.Elements("Product"))
      {
        rowIndex++;
        var product = productRow;
        Dictionary<string, string> excelvalues = new Dictionary<string, string>();

        foreach (XAttribute att in product.Attributes())
        {
          if (columns.Any(x => x.ValueFieldName == att.Name.LocalName))
          {
            if (excelvalues.ContainsKey(att.Name.LocalName))
              excelvalues[att.Name.LocalName] = att.Value;
            else
              excelvalues.Add(att.Name.LocalName, att.Value);

          }
        }

        foreach (XElement el in product.Elements())
        {
          if (columns.Any(x => x.ValueFieldName == el.Name.LocalName))
          {
            if (excelvalues.ContainsKey(el.Name.LocalName))
              excelvalues[el.Name.LocalName] = el.Value;
            else
              excelvalues.Add(el.Name.LocalName, el.Value);
          }

          foreach (XAttribute att in el.Attributes())
          {
            if (columns.Any(x => x.ValueFieldName == att.Name.LocalName))
            {
              if (excelvalues.ContainsKey(att.Name.LocalName))
                excelvalues[att.Name.LocalName] = att.Value;
              else
                excelvalues.Add(att.Name.LocalName, att.Value);

            }
          }
        }
        excelRows.Add(rowIndex, excelvalues);
      }

      foreach (var val in excelRows.Keys)
      {
        row = sheet.Table.Rows.Add();

        foreach (var key in columns)
        {
          row.Cells.Add(excelRows[val][key.ColumnName]);
        }
      }

      return book;
    }

    private static void GenerateStyles(WorksheetStyleCollection styles)
    {
      WorksheetStyle Default = styles.Add("Default");
      Default.Name = "Normal";
      Default.Alignment.Vertical = StyleVerticalAlignment.Bottom;

      WorksheetStyle quantity = styles.Add("Number");
      quantity.NumberFormat = "0";

      WorksheetStyle euro = styles.Add("Euro");
      euro.NumberFormat = "\"€\"\\ #,##0.00_-";

      WorksheetStyle header = styles.Add("Header");
      header.Font.Bold = true;
    }

    private static int GenerateHeaders(Worksheet sheet, bool generatedRow, List<ExcelColumn> headers)
    {
      int index = 1;

      WorksheetRow row = sheet.Table.Rows.Add();
      if (generatedRow)
      {
        row.Index = index;
        index++;
        row.Cells.Add("Generated at " + DateTime.Now.ToString(), DataType.String, "Default");
      }

      row = sheet.Table.Rows.Add();
      row.Index = index;

      foreach (var header in headers)
      {
        row.Cells.Add(header.ColumnName, DataType.String, "Header");
      }

      return index;
    }
     
  }

  public class ExcelColumn
  {
    public string ColumnName { get; private set; }
    public string ValueFieldName { get; private set; }
    public SourceValueType ValueType { get; private set; }
    public int Depth { get; private set; }
    public string Path { get; private set; }

    public ExcelColumn(string columnName, string valueFieldName, SourceValueType valueType)
    {
      columnName = columnName;
      ValueFieldName = valueFieldName;
      ValueType = valueType;
    }

    public ExcelColumn(string columnName, string valueFieldName, SourceValueType valueType, string path, int depth)
    {
      columnName = columnName;
      ValueFieldName = valueFieldName;
      ValueType = valueType;
      Path = path;
      Depth = depth;
    }
  }

  public enum SourceValueType
  {
    XmlAttribute,
    XmlElement,
    AttributeName
  }
}

