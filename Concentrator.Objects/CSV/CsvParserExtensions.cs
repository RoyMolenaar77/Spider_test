using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.CSV
{
  public static class CsvParserExtensions
  {
    public static string Get(this Dictionary<string, string> instance, string column)
    {
      string s = null;
      instance.TryGetValue(column, out s);
      return s;
    }
  }
}
