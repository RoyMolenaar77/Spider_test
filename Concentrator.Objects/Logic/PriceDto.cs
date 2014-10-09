using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Enumerations;

namespace Concentrator.Objects.Logic
{
  public class PriceDto
  {
    public decimal Price { get; set; }

    public decimal CostPrice { get; set; }

    public string MarginSign { get; set; }

    public decimal BasePrice { get; set; }

    public decimal BaseCostPrice { get; set; }

    public decimal UnitPriceIncrease { get; set; }

    public decimal CostPriceIncrease { get; set; }

    public int Index { get; set; }

    public PriceRuleType PriceRuleType { get; set; }
  }
}
