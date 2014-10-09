using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{
  public class VendorAssortmentResult : ComplexObject
  {
    public int VendorAssortmentID { get; set; }
    public int ProductID { get; set; }
    public int VendorID { get; set; }
  }

}
