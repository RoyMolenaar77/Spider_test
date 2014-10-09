using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Products
{
  public class ProductResult
  {
    public string ProductID { get; set; }
    public string Artnr { get; set; }
    public string Chapter { get; set; }
    public string Category { get; set; }
    public string Subcategory { get; set; }
    public string PriceGroup { get; set; }
    public string Description1 { get; set; }
    public string Description2 { get; set; }
    public decimal? PriceNL { get; set; }
    public decimal? PriceBE { get; set; }
    public decimal? VatExcl { get; set; }
    public string ProductImage { get; set; }
  }

}
