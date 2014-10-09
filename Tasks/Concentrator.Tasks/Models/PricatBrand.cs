using System;

namespace Concentrator.Tasks.Models
{
  public class PricatBrand
  {
    public String Alias
    {
      get;
      set;
    }

    public String Code
    {
      get;
      set;
    }

    public String Name
    {
      get;
      set;
    }

    public override String ToString()
    {
      return String.Format("Code: {0}, Alias: '{1}', Name: '{2}'", Code, Alias, Name);
    }
  }
}