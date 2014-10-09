using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Concentrator.Objects.Excel
{
  public class DefaultColumnDefinition
  {


    public DefaultColumnDefinition() { }

    public DefaultColumnDefinition(string header, string dataIndex, PropertyInfo info, double? width = null, bool isHidden = false)
    {
      Hidden = isHidden;
      DataIndex = dataIndex;
      Header = header;
      ColumnInfo = info;
      Width = width;
    }

    public string Header
    {
      get;
      set;
    }

    public string DataIndex
    {
      get;
      set;
    }

    public bool Hidden
    {
      get;
      set;
    }

    public PropertyInfo ColumnInfo
    {
      get;
      set;
    }

    public double? Width
    {
      get;
      set;
    }
  }
}
