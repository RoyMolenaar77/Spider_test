using System;

namespace Concentrator.Tasks.Euretco.RSO.Navision.Models
{
  public class SimpleProductModel
  {
    public Int32 ConfigurableProductID { get; set; }
    public Int32 SimpleProductID { get; set; }
    public string SimpleVendorItemNumber { get; set; }
    public string Barcode { get; set; }
  }
}
