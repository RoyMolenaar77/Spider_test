using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Concentrator.Objects.Models.Base;

namespace Concentrator.Objects.Models.EDI.Order
{
  public class EdiOrderLedger : BaseModel<EdiOrderLedger>
  {
    public Int32 EdiOrderLedgerID { get; set; }

    public Int32 EdiOrderLineID { get; set; }

    public Int32 Status { get; set; }

    public DateTime LedgerDate { get; set; }

    public virtual EdiOrderLine EdiOrderLine { get; set; }

    public override System.Linq.Expressions.Expression<Func<EdiOrderLedger, bool>> GetFilter()
    {
      return null;
    }
  }
}
