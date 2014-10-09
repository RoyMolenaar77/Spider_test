using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{
  public class ProductMatchResult : ComplexObject
  {
    public Int32 ProductID { get; set; }
    public Int32? CProductID { get; set; }
    public string VendorItemNumber { get; set; }
    public string CVendorItemNumber { get; set; }
    public Int32? BrandID { get; set; }
    public Int32? CBrandID { get; set; }
    public int MatchPercentage { get; set; }
  }
}
