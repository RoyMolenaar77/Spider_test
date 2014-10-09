using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Results
{
  public class ProductPriceResult
  {
    public string CustomItemNumber { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal CalculatedPrice { get; set; }
    public decimal Marge { get; set; }
    public string ProductStatus { get; set; }
    public string ShortDescription { get; set; }
    public string LongDescription { get; set; }
    public int PriceRuleID { get; set; }
    public string ConnectorDescription { get; set; }
    public int ConnectorID { get; set; }
  }
}
