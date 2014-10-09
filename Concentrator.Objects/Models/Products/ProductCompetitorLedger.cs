using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.Products
{
  public class ProductCompetitorLedger : AuditObjectBase<ProductCompetitorLedger>
  {
    public Int32 ProductCompetitorLedgerID { get; set; }

    public Int32 ProductCompetitorPriceID { get; set; }

    public String Stock { get; set; }

    public Decimal Price { get; set; }

    public virtual ProductCompetitorPrice ProductCompetitorPrice { get; set; }

    public override System.Linq.Expressions.Expression<Func<ProductCompetitorLedger, bool>> GetFilter()
    {
      return null;
    }
  }
}