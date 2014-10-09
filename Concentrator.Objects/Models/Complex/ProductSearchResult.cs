using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{
  public class ProductSearchResult : ComplexObject
  {
    public string imagepath { get; set; }
    public int? ProductID { get; set; }
    public string ProductName { get; set; }
    public string ShortContentDescription { get; set; }
    public string ShortDescription { get; set; }
    public string VendorItemNumber { get; set; }
  }
}
