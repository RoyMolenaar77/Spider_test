using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Prices
{
  public class PriceResult
  {
    /// <summary>
    /// The price rule ID
    /// </summary>
    public decimal CalculatePrice { get; set; }

    /// <summary>
    /// Cost price
    /// </summary>
    public decimal CostPrice { get; set; }
  }
}
