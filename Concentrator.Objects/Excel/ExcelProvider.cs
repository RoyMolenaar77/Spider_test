using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Reflection;
using System.Globalization;

namespace Concentrator.Objects.Excel
{
  public class ExcelProvider
  {
    private ExcelPackage _package;
    private string _name;
    private ExcelWorksheet _worksheet;
    private object _dataObject;
    private DefaultColumnDefinition[] _columnDefinitionArray;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="definitions"></param>
    /// <param name="name"></param>
    /// <param name="dataObject"></param>
    public ExcelProvider(List<DefaultColumnDefinition> definitions, object dataObject, string name = "export.xlsx")
    {
      _name = name;
      _dataObject = dataObject;
      InitColumnDefinitions(definitions);
      Init();
    }

    public bool isDate(string value)
    {
      DateTime date;
      return DateTime.TryParse(value, out date);
    }

    /// <summary>
    /// Resolves the object properties agains the column definitions
    /// If a property is not in the object definitions, the property is shown with its original data (name, visible, value)
    /// </summary>
    private void InitColumnDefinitions(List<DefaultColumnDefinition> _columnDefinitions)
    {
      PropertyInfo[] props = null;

      if ((_dataObject is IEnumerable<object>) && ((_dataObject as IEnumerable<object>).Count() != 0)) //if enumerable get first obj and resolve props
      {
        props = ((IEnumerable<object>)_dataObject).FirstOrDefault().GetType().GetProperties();
      }

      else
      {
        props = _dataObject.GetType().GetProperties();
      }

      _columnDefinitionArray = new DefaultColumnDefinition[props.Count()];

      //try resolve and map the properties
      for (int i = 0; i < props.Length; i++)
      {
        var propertyInfo = props[i];

        var propDefinition = _columnDefinitions.FirstOrDefault(c => c.DataIndex == propertyInfo.Name);

        if (propDefinition != null)
        {
          propDefinition.ColumnInfo = propertyInfo;
          _columnDefinitionArray[i] = propDefinition.Hidden ? null : propDefinition;
        }
      }
      //ignore the hidden columns 
    }

    private void Init()
    {
      _package = new ExcelPackage();
      _worksheet = _package.Workbook.Worksheets.Add(_name);
    }

    /// <summary>
    /// Returns the created Excel document
    /// </summary>
    /// <returns></returns>
    public byte[] GetExcelDocument()
    {
      int headerColIndex = 1;

      foreach (var columnDef in _columnDefinitionArray)
      {
        if (columnDef != null)
        {
          _worksheet.Cells[1, headerColIndex].Value = columnDef.Header;
          _worksheet.Cells[1, headerColIndex].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
          _worksheet.Cells[1, headerColIndex].Style.Font.Bold = true;
          headerColIndex++;
        }
      }

      if (_dataObject is IEnumerable<object>)
      {
        int rowIndex = 3; //skip one line after headers
        foreach (var dataRow in ((IEnumerable<object>)_dataObject))
        {
          var colIndex = 1;
          var dataRowroPerties = dataRow.GetType().GetProperties();
          for (int j = 0; j < dataRowroPerties.Length; j++)
          {
            var cellData = dataRow.GetType().GetProperties()[j];
            //ignore the ones that are not contained in the col definitions. 
            if (_columnDefinitionArray[j] != null)
            {
              var cell = _worksheet.Cells[rowIndex, colIndex];
              cell.Value = cellData.GetValue(dataRow, null);

              if (cellData.PropertyType.Name.Contains("Date"))
              {
                cell.Style.Numberformat.Format = "dd-MM-YYYY";

              }
              else if ((cellData.PropertyType.Name.Contains("Decimal")) || (cellData.PropertyType.Name.Contains("Float")))
              {
                cell.Style.Numberformat.Format = "#.##";
              }
              else if (cellData.PropertyType.Name.Contains("Boolean"))
              {
                cell.Style.Numberformat.Format = "@";
                cell.Value = (bool)cell.Value ? "Yes" : "No";
              }
              else
              {
                cell.Style.Numberformat.Format = "@";
              }

              colIndex++;
            }
          }
          rowIndex++;
        }
      }
      for (var i = 1; i <= headerColIndex; i++)
        _worksheet.Column(i).AutoFit();

      return _package.GetAsByteArray();
    }
  }
}
