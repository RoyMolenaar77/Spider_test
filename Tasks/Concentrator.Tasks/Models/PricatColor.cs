using System;
using System.Collections.Generic;

namespace Concentrator.Tasks.Models
{
  public class PricatColor
  {
    public String ColorCode
    {
      get;
      set;
    }

    public String Description
    {
      get;
      set;
    }

    public String Filter
    {
      get;
      set;
    }

    public override String ToString()
    {
      return String.Join(" | ", ColorCode, Description, Filter);
    }
  }
}