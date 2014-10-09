using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace Concentrator.Objects
{
  public class Filter
  {
    public string DataComparison { get; set; }
    public string DataType { get; set; }
    public string DataValue { get; set; }
    public string Field { get; set; }

    public int Id { get; set; }

    public static bool CheckExistence(int filterIndex, NameValueCollection @params)
    {
      return (@params[string.Format("filter[{0}][field]", filterIndex)] != null);
    }

    public static NameValueCollection ReplaceFilterValue(NameValueCollection @params, List<String> fieldNames, Func<string, string> transformFunc)
    {
      string s;
      NameValueCollection collection = new NameValueCollection(@params);
      for (int i = 0; ((fieldNames.Count() > 0) && (!String.IsNullOrEmpty(s = @params[string.Format("filter[{0}][field]", i)]))); i++)
      {
        if (fieldNames.Contains(s))
        {
          s = string.Format("filter[{0}][data][value]", i);
          collection[s] = transformFunc(@params[s]);
          fieldNames.Remove(s);
        }
      }
      return collection;
    }

    public Filter(int id, NameValueCollection requestParams)
    {
      Id = id;
      Field = requestParams[string.Format("filter[{0}][field]", id)];
      DataType = requestParams[string.Format("filter[{0}][data][type]", id)];
      DataValue = requestParams[string.Format("filter[{0}][data][value]", id)];
      DataComparison = requestParams[string.Format("filter[{0}][data][comparison]", id)];
    }

    public Filter(int id, HttpRequestBase request)
    {
      Id = id;
      Field = request.Params[string.Format("filter[{0}][field]", id)];
      DataType = request.Params[string.Format("filter[{0}][data][type]", id)];
      DataValue = request.Params[string.Format("filter[{0}][data][value]", id)];
      DataComparison = request.Params[string.Format("filter[{0}][data][comparison]", id)];
    }

  }
}
