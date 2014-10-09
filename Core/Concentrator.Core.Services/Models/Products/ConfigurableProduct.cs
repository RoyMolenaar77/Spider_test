using System.Collections.Generic;

namespace Concentrator.Core.Services.Models.Products
{
  public class ConfigurableProduct : ProductBase
  {
    public List<SimpleProduct> ChildProducts { get; set; }

    public override ProductTypes Type
    {
      get { return ProductTypes.Configurable; }
    }
  }
}
