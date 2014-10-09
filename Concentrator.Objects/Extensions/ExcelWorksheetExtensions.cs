using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeOpenXml
{
  public static class ExcelWorksheetExtensions
  {
    public static TValue GetValue<TValue>(this ExcelWorksheet worksheet, Int32 row, Int32 column, TValue defaultValue)
      where TValue : class
    {
      var result = worksheet.GetValue<TValue>(row, column);

      return result != null
        ? result
        : defaultValue;
    }
  }
}
