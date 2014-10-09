using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Complex
{
  public class AdvancedPricing
  {
    public decimal Price { get; set; }
    public int MinimumQuantity { get; set; }
    public decimal TaxRate { get; set; }
    public int ProductID { get; set; }
  }
}
