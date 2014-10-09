using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects.DataClasses;

namespace Concentrator.Objects.Models.Complex
{
  public class VendorPriceResult : ComplexObject
  {
    public int VendorAssortmentID { get; set; }
    public decimal? Price { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? TaxRate { get; set; }
    public int? MinimumQuantity { get; set; }
    public string CommercialStatus { get; set; }
    public int? ConcentratorStatusID { get; set; }
    public decimal? SpecialPrice { get; set; }
  }

}
