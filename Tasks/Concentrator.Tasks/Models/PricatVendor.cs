using System;

namespace Concentrator.Tasks.Models
{
  public class PricatVendor
  {
    public String Alias
    {
      get;
      set;
    }

    public String BackendID
    {
      get;
      set;
    }

    public String Barcode
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
      return String.Join(" | ", Barcode, BackendID, Name);
    }
  }
}