using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace System.Web.Mvc
{
  public static class HtmlExtensions
  {
    public static string Serialize(this HtmlHelper helper, object ob)
    {
      JavaScriptSerializer serialize = new JavaScriptSerializer();
      return serialize.Serialize(ob);
    }
    
    public static string RenderPrice(this HtmlHelper helper, decimal amount)
    {
      int prefix = (int)amount;

      decimal suffix = (int)((amount - prefix) * 100);

      return string.Format(@"<span>{0}.</span><span style=""font-size: 75%;""><sup>{1}</sup></span>", prefix, suffix > 0 ? suffix.ToString() : "-");
    }


  }
}
