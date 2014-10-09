using Concentrator.Objects.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Concentrator.Objects.Models.Orders
{
  public class DatcolLink : BaseModel<DatcolLink>
  {
    public int Id { get; set; }

    public int OrderID { get; set; }

    public string ShopNumber { get; set; }

    public DateTime DateCreated { get; set; }

    public decimal Amount { get; set; }

    public string DatcolNumber { get; set; }

    public string SourceMessage { get; set; }

    public virtual Order Order { get; set; }

    public override System.Linq.Expressions.Expression<Func<DatcolLink, bool>> GetFilter()
    {
      return null;
    }
  }
}
