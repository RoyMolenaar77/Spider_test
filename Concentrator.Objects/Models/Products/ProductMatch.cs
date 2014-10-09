using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductMatch : BaseModel<ProductMatch>
  {
    public Int32 ProductMatchID { get; set; }

    public Int32 ProductID { get; set; }

    public Boolean isMatched { get; set; }

    public Int32? MatchPercentage { get; set; }

    public Boolean? CalculatedMatch { get; set; }

    public Int32 MatchStatus { get; set; }

    public bool Primary { get; set; }

    public virtual Product Product { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductMatch, bool>> GetFilter()
    {
      return null;
    }
  }
}