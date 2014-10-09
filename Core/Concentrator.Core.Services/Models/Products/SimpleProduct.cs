using System.Collections.Generic;

namespace Concentrator.Core.Services.Models.Products
{
  public class SimpleProduct : ProductBase
  {
    public List<Price> Prices { get; set; }

    public List<Stock> Stock { get; set; }

    public override ProductTypes Type
    {
      get { return ProductTypes.Simple; }
    }
  }
}
