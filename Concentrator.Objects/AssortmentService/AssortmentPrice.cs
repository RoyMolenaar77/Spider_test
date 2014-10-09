using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.AssortmentService
{
  public class AssortmentPrice
  {
    public decimal TaxRate { get; set; }

    public string CommercialStatus { get; set; }

    public int MinimumQuantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public decimal CostPrice { get; set; }

    public decimal? SpecialPrice { get; set; }
  }
}
