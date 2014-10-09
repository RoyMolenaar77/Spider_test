using System;
using System.Text;

namespace Concentrator.Tasks.Models
{
  public class PricatSize
  {
    public String BrandName
    {
      get;
      set;
    }

    public String ModelName
    {
      get;
      set;
    }

    public String GroupCode
    {
      get;
      set;
    }

    public String From
    {
      get;
      set;
    }

    public String To
    {
      get;
      set;
    }

    public override String ToString()
    {
      var builder = new StringBuilder();

      builder.AppendFormat("Brand: '{0}'", BrandName);

      if (!ModelName.IsNullOrWhiteSpace())
      {
        builder.AppendFormat(" | Model: '{0}'", ModelName);
      }

      if (!GroupCode.IsNullOrWhiteSpace())
      {
        builder.AppendFormat(" | Group: {0}", GroupCode);
      }

      builder.AppendFormat(" | From: {0} => {1}", From, To);

      return builder.ToString();
    }
  }
}