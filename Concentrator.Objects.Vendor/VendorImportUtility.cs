using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Concentrator.Objects.Vendors
{
  public class VendorImportUtility
  {
    public static string SetDataSetValue(string name, DataRow row, string defaultValue = null)
    {      
      try
      {        
        return row.Table.Columns.Contains(name) && !row.IsNull(name) && !string.IsNullOrEmpty(row.Field<object>(name).ToString()) ? row.Field<object>(name).ToString().Trim() : defaultValue;        
      }
      catch
      {
        return defaultValue;
      }
    }
  }
}
