using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Sql
{
  using Select;

  public class QueryBuilder
  {
    public String ColumnSeparator
    {
      get;
      set;
    }

    public Int32 IndentSize
    {
      get;
      set;
    }

    public QueryBuilder()
    {
      ColumnSeparator = ", ";
      IndentSize = 2;
    }

    public FromBuilder From(String objectName)
    {
      return new FromBuilder(this, objectName);
    }
  }
}
