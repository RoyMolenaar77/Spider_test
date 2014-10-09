using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.EDI.Order
{
  public class EdiOrderType : AuditObjectBase<EdiOrderType>
  {
    public Int32 EdiOrderTypeID { get; set; }

    public string Name { get; set; }

    public virtual ICollection<EdiOrder> EdiOrders { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiOrderType, bool>> GetFilter()
    {
      return null;
    }
  }
}
