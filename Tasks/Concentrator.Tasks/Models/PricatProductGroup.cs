using System;

namespace Concentrator.Tasks.Models
{
  public class PricatProductGroup
  {
    public String Code
    {
      get;
      set;
    }

    public String Description
    {
      get;
      set;
    }

    public override String ToString()
    {
      return String.Format("Code: {0}, Description: '{1}'", Code, Description);
    }
  }
}