using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Plugins.Axapta.Models
{
  public class SkuModel
  {
    public Int32 ProductID { get; set; }
    public string VendorItemNumber { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
  }
}
