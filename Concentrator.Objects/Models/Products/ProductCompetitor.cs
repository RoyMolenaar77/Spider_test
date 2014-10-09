using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductCompetitor : BaseModel<ProductCompetitor>
  {
    public Int32 ProductCompetitorID { get; set; }

    public String Name { get; set; }

    public Int32 Reliability { get; set; }

    public DateTime? DeliveryDate { get; set; }

    public Decimal ShippingCostPerOrder { get; set; }

    public Decimal? ShippingCost { get; set; }

    public virtual ICollection<ProductCompetitorMapping> ProductCompetitorMappings { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductCompetitor, bool>> GetFilter()
    {
      return null;
    }
  }
}