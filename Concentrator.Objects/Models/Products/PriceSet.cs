using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;
using Concentrator.Objects.Web;
using Concentrator.Objects.Models.Connectors;

namespace Concentrator.Objects.Models.Products
{
  public class PriceSet : BaseModel<PriceSet>
  {
    public Int32 PriceSetID { get; set; }

    public String Name { get; set; }

    public Decimal? Price { get; set; }

    public Decimal? DiscountPercentage { get; set; }

    public string Description { get; set; }

    public bool IsCatalog { get; set; }

    public virtual ICollection<ProductPriceSet> ProductPriceSets { get; set; }

    public int ConnectorID { get; set; }

    public virtual Connector Connector { get; set; }

    public override System.Linq.Expressions.Expression<Func<PriceSet, bool>> GetFilter()
    {
      return null;
    }
  }
}
